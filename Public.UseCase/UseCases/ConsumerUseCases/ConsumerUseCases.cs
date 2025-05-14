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

public class ConsumerUseCases(IEmailConfirmationService emailConfirmationService, IMailSenderService mailSenderService,  
    IUserService userService, IRoleService roleService, ICarService carService, ILogger<ConsumerUseCases> logger)
{
    /// <summary> Процесс регистрации клиента </summary>
    public async Task<ApplicationExecuteLogicResult<UserRegistrationResponse>> RegistrationClientAsync(DataForConsumerRegistration data)
    {
        logger.LogInformation("Попытка регистрации пользователя с логином {login}", data.Login);
        
        var warnings = new List<ApplicationError>();
        
        // Проверяем что запрашиваемая роль - Client
        if (data.RequestedUserRole is not ApplicationUserRole.Client)
            return ApplicationExecuteLogicResult<UserRegistrationResponse>.Failure(
                Helper.ForbiddenRoleApplicationError());
        
        // Создаем аккаунт пользователя
        var userResult = await userService.CreateUserAsync(new DataForCreateUser
        {
            FirstName = data.FirstName,
            LastName = data.LastName,
            Login = data.Login,
            Email = data.Email,
            Password = data.Password,
            InitialRoles = [ApplicationUserRole.Client]
        });
        if (userResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserRegistrationResponse>.Failure().Merge(userResult);
        var user = userResult.Value!;
        warnings.AddRange(userResult.GetWarnings);

        var rolesResult = await roleService.AddRolesToUser(user, [ApplicationUserRole.Client]);
        if (rolesResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserRegistrationResponse>.Failure().Merge(rolesResult);
        
        // Создаем токен и отправляем на почту ссылку-подтверждение
        var tokenResult = await emailConfirmationService.CreateConfirmationTokenAsync(user);
        if (tokenResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserRegistrationResponse>.Failure().Merge(tokenResult);
        var token = tokenResult.Value!;
        warnings.AddRange(userResult.GetWarnings);
        
        var url = new Uri($"/confirm-email?uid={ Uri.EscapeDataString(user.Id) }&code={ Uri.EscapeDataString(token) }", UriKind.Relative);

        var confirmEmail = await mailSenderService.SendConfirmationEmailAsync(user.Email!, url.ToString());
        if (confirmEmail.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserRegistrationResponse>.Failure().Merge(confirmEmail);
        warnings.AddRange(confirmEmail.GetWarnings);
        
        logger.LogInformation("Успешная регистрация пользователя с логином {login}", user.UserName);

        var registrationResponse = new UserRegistrationResponse
        {
            Login = user.UserName!,
            Email = user.Email!,
        };

        return ApplicationExecuteLogicResult<UserRegistrationResponse>.Success(registrationResponse).WithWarnings(warnings);
    }

    /// <summary> Получение машины по id клиентом </summary>
    public async Task<ApplicationExecuteLogicResult<CarUseCaseResponse>> GetCarById(int carId)
    {
        logger.LogInformation("Попытка клиентом получить машину {id}", carId);
        
        // Получаем машину
        var carResult = await carService.CarById(carId);
        if (carResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(carResult);
        var car = carResult.Value!;
            
        // Подготавливаем ответ
        var response = CarHelper.BuildRestrictedResponse(car);

        return ApplicationExecuteLogicResult<CarUseCaseResponse>.Success(response).WithWarnings(carResult.GetWarnings);
    }

    /// <summary> Получение машины по параметрам клиентом </summary>
    public async Task<ApplicationExecuteLogicResult<CarsUseCaseResponse>> GetCarsByParams(DtoForSearchCars paramsDto)
    {
        logger.LogInformation("Попытка получить данные о машинах по параметрам клиентом");
        logger.LogDebug("Данные запроса {@data}", paramsDto);
        
        var warnings = new List<ApplicationError>();
        
        // Получаем машины
        var carsPageResult = await carService.CarsByParams(paramsDto);
        if (carsPageResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarsUseCaseResponse>.Failure().Merge(carsPageResult);
        var carsPage = carsPageResult.Value!;
        warnings.AddRange(carsPageResult.GetWarnings);
        
        var response = new CarsUseCaseResponse
        {
            PageSize = carsPage.PageSize,
            PageNumber = carsPage.PageNumber,
        };
        
        response.Cars = carsPage.DomainCars.Select(CarHelper.BuildRestrictedResponse).ToArray();
        
        return ApplicationExecuteLogicResult<CarsUseCaseResponse>.Success(response).WithWarnings(warnings);
    }
}