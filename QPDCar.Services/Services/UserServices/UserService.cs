using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ErrorTypes;
using QPDCar.Models.DtoModels.UserDtos;
using QPDCar.Models.StorageModels;
using QPDCar.ServiceInterfaces;
using QPDCar.ServiceInterfaces.UserServices;

namespace QPDCar.Services.Services.UserServices;

public class UserService(UserManager<ApplicationUserEntity> userManager, ILogger<UserService> logger) : IUserService
{
    public async Task<ApplicationExecuteResult<ApplicationUserEntity>> CreateAsync(DtoForCreateUser dtoUser)
    {
        logger.LogInformation("Создание пользователя {data}", dtoUser);
        
        // Занят ли логин
        var byName = await userManager.FindByNameAsync(dtoUser.Login);
        if (byName != null)
            return ApplicationExecuteResult<ApplicationUserEntity>.Failure(new ApplicationError(
                UserErrors.LoginBusy, "Логин занят",
                $"Логин {dtoUser.Login} уже занят",
                ErrorSeverity.Critical, HttpStatusCode.BadRequest));
        
        // Занята ли почта
        var byEmail = await userManager.FindByEmailAsync(dtoUser.Email);
        if (byEmail != null)
            return ApplicationExecuteResult<ApplicationUserEntity>.Failure(new ApplicationError(
                UserErrors.EmailBusy, "Почта занята",
                $"Почта {dtoUser.Email} уже занята",
                ErrorSeverity.Critical, HttpStatusCode.BadRequest));

        var user = new ApplicationUserEntity
        {
            UserName = dtoUser.Login,
            Email = dtoUser.Email,
            FirstName = dtoUser.FirstName,
            LastName = dtoUser.LastName,
        };
        
        // Сохраняем 
        var saved = await userManager.CreateAsync(user, dtoUser.Password);
        if (saved.Succeeded is false)
        {
            logger.LogWarning("Не получилось сохранить пользователя, ошибка - {@errs}", saved.Errors.ToList());
            return ApplicationExecuteResult<ApplicationUserEntity>.Failure(new ApplicationError(
                UserErrors.UserNotSaved, "Пользователь не создан",
                "При сохрании пользователя возникла неизвестная ошибка",
                ErrorSeverity.Critical, HttpStatusCode.InternalServerError));
        }
        
        logger.LogInformation("Пользователь с логином {login} успешно создан", dtoUser.Login);

        return ApplicationExecuteResult<ApplicationUserEntity>.Success(user);
    }

    public async Task<ApplicationExecuteResult<ApplicationUserEntity>> UpdateAsync(ApplicationUserEntity user)
    {
        throw new NotImplementedException();
    }

    public async Task<ApplicationExecuteResult<ApplicationUserEntity>> ByLoginOrIdAsync(string loginOrId)
    {
        throw new NotImplementedException();
    }

    public async Task<ApplicationExecuteResult<List<ApplicationUserEntity>>> AllUsers()
    {
        throw new NotImplementedException();
    }

    public async Task<ApplicationExecuteResult<Unit>> BlockOrUnblockAsync(ApplicationUserEntity user)
    {
        throw new NotImplementedException();
    }
}