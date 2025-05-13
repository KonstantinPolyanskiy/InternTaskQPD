using Microsoft.Extensions.Logging;
using Private.ServicesInterfaces;
using Public.Models.ApplicationErrors;
using Public.Models.CommonModels;
using Public.Models.DtoModels.CarDtoModels;
using Public.Models.DtoModels.UserDtoModels;
using Public.Models.Extensions;
using Public.UseCase.Helpers;
using Public.UseCase.Models;
using Public.UseCase.Models.CarModels;
using Public.UseCase.UseCases.UserUseCases;

namespace Public.UseCase.UseCases.ConsumerUseCases;

public class ConsumerUseCases
{
    private readonly ILogger<UserUseCase> _logger;
    
    private readonly ICarService _carService;
    private readonly IPhotoService _photoService;
    private readonly IEmployerService _employerService;
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly IRoleService _roleService;
    private readonly IMailSenderService _mailSenderService;
    
    
    // ReSharper disable once ConvertToPrimaryConstructor
    public ConsumerUseCases(IUserService userService, ITokenService tokenService,
        IMailSenderService mailSenderService, ILogger<UserUseCase> logger, ICarService carService,
        IEmployerService employerService, IPhotoService photoService, IRoleService roleService)
    {
        
        _userService = userService;
        _tokenService = tokenService;
        _carService = carService;
        _mailSenderService = mailSenderService;
        _employerService = employerService;
        _photoService = photoService;
        _roleService = roleService;

        _logger = logger;
    }
    
    /// <summary> Процесс регистрации клиента </summary>
    public async Task<ApplicationExecuteLogicResult<UserRegistrationResponse>> RegistrationClientAsync(DataForConsumerRegistration data)
    {
        _logger.LogInformation("Попытка регистрации пользователя с логином {login}", data.Login);
        
        var warnings = new List<ApplicationError>();
        
        // Проверяем что запрашиваемая роль - Client
        if (data.RequestedUserRole is not ApplicationUserRole.Client)
            return ApplicationExecuteLogicResult<UserRegistrationResponse>.Failure(
                Helper.ForbiddenRoleApplicationError());
        
        // Создаем аккаунт пользователя
        var dataForUser = new DataForCreateUser
        {
            FirstName = data.FirstName,
            LastName = data.LastName,
            Login = data.Login,
            Email = data.Email,
            Password = data.Password,
            InitialRoles = [ApplicationUserRole.Client]
        };
        
        var user = await _userService.CreateUserAsync(dataForUser);
        if (user.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserRegistrationResponse>.Failure().Merge(user);
        warnings.AddRange(user.GetWarnings);

        var setRoles = await _roleService.AddRolesToUser(user.Value!, [ApplicationUserRole.Client]);
        if (setRoles.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserRegistrationResponse>.Failure().Merge(setRoles);
        
        var userId = user.Value!.Id;
        
        // Создаем токен и отправляем на почту ссылку-подтверждение
        var token = await _tokenService.GenerateConfirmEmailTokenAsync(Guid.Parse(userId), DateTime.UtcNow.AddHours(24));
        if (token.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserRegistrationResponse>.Failure().Merge(token);
        warnings.AddRange(user.GetWarnings);
        
        var url = new Uri($"/confirm-email?uid={ Uri.EscapeDataString(userId) }&code={ Uri.EscapeDataString(token.Value!) }", UriKind.Relative);

        var confirmEmail = await _mailSenderService.SendConfirmationEmailAsync(user.Value.Email!, url.ToString());
        if (confirmEmail.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserRegistrationResponse>.Failure().Merge(confirmEmail);
        warnings.AddRange(confirmEmail.GetWarnings);
        
        _logger.LogInformation("Успешная регистрация пользователя с логином {login}", user.Value.UserName);

        var registrationResponse = new UserRegistrationResponse
        {
            Login = user.Value.UserName!,
            Email = user.Value.Email!,
        };

        return ApplicationExecuteLogicResult<UserRegistrationResponse>.Success(registrationResponse).WithWarnings(warnings);
    }

    /// <summary> Получение машины по id клиентом </summary>
    public async Task<ApplicationExecuteLogicResult<CarUseCaseResponse>> GetCarById(int carId)
    {
        _logger.LogInformation("Попытка клиентом получить машину {id}", carId);
        
        var warnings = new List<ApplicationError>();
        
        // Получаем машину
        var carResult = await _carService.GetCarByIdAsync(carId);
        if (carResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(carResult);
        var car = carResult.Value!;
        
        // Получаем менеджера
        var managerResult = await _employerService.ManagerByUserIdAsync(car.Manager!.Id);
        if (managerResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(managerResult);
        var manager = car.Manager!;
        
        // Получаем фото 
        var photoResult = await _photoService.GetPhotoByCarIdAsync(car.Id);
        if (photoResult.IsSuccess is not true)
            warnings.Add(new ApplicationError(CarErrors.CarNotFoundPhoto, "Нет фото",
                $"Для машины {car.Id} не найдено фото", ErrorSeverity.NotImportant));
        var photo = photoResult.Value!;
            
        // Подготавливаем ответ
        var response = CarHelper.BuildRestrictedResponse(car, manager, photo);

        return ApplicationExecuteLogicResult<CarUseCaseResponse>.Success(response).WithWarnings(warnings);
    }

    /// <summary> Получение машины по параметрам клиентом </summary>
    public async Task<ApplicationExecuteLogicResult<CarsUseCaseResponse>> GetCarsByParams(DtoForSearchCars paramsDto)
    {
        _logger.LogInformation("Попытка получить данные о машинах по параметрам клиентом");
        _logger.LogDebug("Данные запроса {@data}", paramsDto);
        
        var warnings = new List<ApplicationError>();
        
        // Получаем машины
        var carsPageResult = await _carService.GetCarsAsync(paramsDto);
        if (carsPageResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarsUseCaseResponse>.Failure().Merge(carsPageResult);
        var carsPage = carsPageResult.Value!;
        
        var preparedCars = new List<CarUseCaseResponse>();
        var response = new CarsUseCaseResponse
        {
            PageSize = carsPage.PageSize,
            PageNumber = carsPage.PageNumber,
        };
        
        // Проходим по каждой полученной машине и собираем ее данные
        foreach (var domainCar in carsPage.DomainCars)
        {
            // Получаем менеджера машины
            var managerResult = await _employerService.ManagerByUserIdAsync(domainCar.Manager!.Id);
            if (managerResult.IsSuccess is not true)
                return ApplicationExecuteLogicResult<CarsUseCaseResponse>.Failure().Merge(managerResult);
            var manager = managerResult.Value!;
            
            // Получаем фото машины
            var photoResult = await _photoService.GetPhotoByCarIdAsync(domainCar.Id);
            if (photoResult.IsSuccess is not true)
                warnings.Add(new ApplicationError(
                    CarErrors.CarNotFoundPhoto, "Фото не найдено",
                    $"Для машины {domainCar.Id} не найдено фото", ErrorSeverity.NotImportant));
            var photo = photoResult.Value!;
            
            preparedCars.Add(CarHelper.BuildRestrictedResponse(domainCar, manager, photo));
        }
        
        response.Cars = preparedCars.ToArray();
        
        return ApplicationExecuteLogicResult<CarsUseCaseResponse>.Success(response).WithWarnings(warnings);
    }
}