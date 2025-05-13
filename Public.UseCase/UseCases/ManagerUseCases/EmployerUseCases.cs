using System.Net;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Private.ServicesInterfaces;
using Public.Models.ApplicationErrors;
using Public.Models.BusinessModels.StorageModels;
using Public.Models.CommonModels;
using Public.Models.DtoModels.CarDtoModels;
using Public.Models.DtoModels.PhotoDtoModels;
using Public.Models.Extensions;
using Public.UseCase.Helpers;
using Public.UseCase.Models.CarModels;
using Public.UseCase.Models.PhotoModels;
using Public.UseCase.Models.UserModels;

namespace Public.UseCase.UseCases.ManagerUseCases;

public class EmployerUseCases
{
    private readonly ICarService _carService;
    private readonly IPhotoService _photoService;
    private readonly IEmployerService _employerService;
    private readonly IUserService _userService;
    private readonly IMailSenderService _mailService;
    private readonly IRoleService _roleService;

    private readonly ILogger<EmployerUseCases> _logger;
    
    // ReSharper disable once ConvertToPrimaryConstructor
    public EmployerUseCases(ICarService carService, IPhotoService photoService, IEmployerService employerService, 
        ILogger<EmployerUseCases> logger, IMailSenderService mailService, IUserService userService, IRoleService roleService)
    {
        _carService = carService;
        _photoService = photoService;
        _employerService = employerService;
        _mailService = mailService;
        _userService = userService;
        _roleService = roleService;
        
        _logger = logger;
    }
    

    
    
    /// <summary> Кейс получения сотрудником машин по параметрам </summary>
    public async Task<ApplicationExecuteLogicResult<CarsUseCaseResponse>> GetCarsByParams(DtoForSearchCars paramsDto, ClaimsPrincipal employerClaims)
    {
        _logger.LogInformation("Попытка получить данные о машинах по параметрам сотрудником");
        _logger.LogDebug("Данные запроса {@data}", paramsDto);
        
        var warnings = new List<ApplicationError>();
        
        // Получаем пользователя
        var requestedEmployerId = Guid.Parse(employerClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var userResult = await _userService.UserByLoginOrIdAsync(requestedEmployerId.ToString());
        if (userResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarsUseCaseResponse>.Failure().Merge(userResult);
        var requestedEmployer = userResult.Value!;
        
        var rolesResult = await _roleService.GetRolesByUser(requestedEmployer);
        if (rolesResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarsUseCaseResponse>.Failure().Merge(rolesResult);
        var roles = rolesResult.Value!;
        
        var isAdmin   = roles.Contains(ApplicationUserRole.Admin);
        var isManager = roles.Contains(ApplicationUserRole.Manager);
        
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
            
            // Определяем доступ
            var isOwnManager = isManager && Guid.Parse(requestedEmployer.Id) == manager.Id;
            var fullAccess = isAdmin || isOwnManager;
            
            if (!fullAccess)
                warnings.Add(new ApplicationError(RoleErrors.DontHaveEnoughPermissions, "Не полный ответ",
                    $"Частичный ответ: пользователь не является администратором или ответственным менеджером машины {domainCar.Id}",
                    ErrorSeverity.NotImportant));
            
            preparedCars.Add(fullAccess
                ? CarHelper.BuildFullResponse(domainCar, manager, photo)
                : CarHelper.BuildRestrictedResponse(domainCar, manager, photo));
        }
        
        response.Cars = preparedCars.ToArray();
        
        return ApplicationExecuteLogicResult<CarsUseCaseResponse>.Success(response).WithWarnings(warnings);
    }
    
    /// <summary> Кейс получения сотрудником машин по параметрам. Админ - может удалить любую, менеджер - только свою </summary>
    public async Task<ApplicationExecuteLogicResult<Unit>> DeleteCarByCarId(int carId, ClaimsPrincipal employerClaims)
    {
        _logger.LogInformation("Попытка удалить данные о машине {id} сотрудником ", carId);
        
        // Получаем сотрудника
        var requestedEmployerId = Guid.Parse(employerClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var userResult = await _userService.UserByLoginOrIdAsync(requestedEmployerId.ToString());
        if (userResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(userResult);
        var requestedEmployer = userResult.Value!;
        
        var rolesResult = await _roleService.GetRolesByUser(requestedEmployer);
        if (rolesResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(rolesResult);
        var roles = rolesResult.Value!;
        
        // Получаем машину
        var carResult = await _carService.GetCarByIdAsync(carId);
        if (carResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(carResult);
        var car = carResult.Value!;
        
        // Определяем можно ли удалить и удаляем
        var errors = new List<ApplicationError>();

        if (roles.Contains(ApplicationUserRole.Admin))
        {
            var result = await _carService.DeleteCarByIdAsync(carId);
            errors.AddRange(result.GetCriticalErrors);
        } 
        else if (roles.Contains(ApplicationUserRole.Manager) && requestedEmployerId == car.Manager!.Id)
        {
            var result = await _carService.DeleteCarByIdAsync(carId);
            errors.AddRange(result.GetCriticalErrors);
        }
        else
        {
            return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(
                RoleErrors.DontHaveEnoughPermissions, "Машина не удалена",
                "Недостаточно прав для удаления машины",
                ErrorSeverity.Critical, HttpStatusCode.Forbidden));
        }
        
        return errors.Any() ? ApplicationExecuteLogicResult<Unit>.Failure().WithCriticals(errors) : ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }

    /// <summary> Кейс обновления сотрудником машины. Админ - может обновить любую или переназначить менеджера, менеджер - только свою </summary>
    public async Task<ApplicationExecuteLogicResult<CarUseCaseResponse>> UpdateCarById(DtoForUpdateCar carDto, ClaimsPrincipal employerClaims)
    {
        _logger.LogInformation("Попытка удалить данные о машине {id} сотрудником ", carDto.CarId);
        
        var warnings = new List<ApplicationError>();
        
        // Получаем сотрудника
        var requestedEmployerId = Guid.Parse(employerClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var userResult = await _userService.UserByLoginOrIdAsync(requestedEmployerId.ToString());
        if (userResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(userResult);
        var requestedEmployer = userResult.Value!;
        
        var rolesResult = await _roleService.GetRolesByUser(requestedEmployer);
        if (rolesResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(rolesResult);
        var roles = rolesResult.Value!;
        
        var isAdmin   = roles.Contains(ApplicationUserRole.Admin);
        var isManager = roles.Contains(ApplicationUserRole.Manager);
        
        // Получаем машину
        var carResult = await _carService.GetCarByIdAsync(carDto.CarId);
        if (carResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(carResult);
        var car = carResult.Value!;
        
        // Получаем ее менеджера
        var managerResult = await _employerService.ManagerByUserIdAsync(car.Manager!.Id);
        if (managerResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(managerResult);
        var manager = managerResult.Value!;
        
        // Получаем фото машины
        var photoResult = await _photoService.GetPhotoByCarIdAsync(car.Id);
        if (photoResult.IsSuccess is not true)
            warnings.Add(new ApplicationError(
                CarErrors.CarNotFoundPhoto, "Фото не найдено",
                $"Для машины {car.Id} не найдено фото", ErrorSeverity.NotImportant));
        var photo = photoResult.Value!;
        
        // Определяем доступ
        var isOwnManager = isManager && Guid.Parse(requestedEmployer.Id) == manager.Id;
        var fullAccess = isAdmin || isOwnManager;
    
        // Если не админ - не может изменить ответственного за машину менеджера
        if (!isAdmin && carDto.NewManager is not null)
            carDto.NewManager = null;
        
        // Обновляем машину
        if (!fullAccess)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure(new ApplicationError(RoleErrors.DontHaveEnoughPermissions, "Машина не обновлена",
                "Недостаточно прав для обновления машины", ErrorSeverity.Critical, HttpStatusCode.Forbidden));
        
        var updatedCarResult = await _carService.UpdateCarAsync(carDto);
        if (updatedCarResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(updatedCarResult);
        var updatedCar = updatedCarResult.Value!;
        
        return ApplicationExecuteLogicResult<CarUseCaseResponse>.Success(CarHelper.BuildFullResponse(updatedCar, manager, photo)).WithWarnings(warnings);
    }
}