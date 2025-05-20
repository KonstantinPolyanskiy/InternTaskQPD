using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        var result = await userManager.UpdateAsync(user);
        if (result.Succeeded is false)
        {
            logger.LogInformation("При обновлении пользователя {id} возникли ошибки {@data}", user.Id, result.Errors.ToList());
            return ApplicationExecuteResult<ApplicationUserEntity>.Failure(new ApplicationError(
                UserErrors.UserNotUpdated, "Пользователь не обновлен",
                $"Пользователь {user.Id} не обновлен",
                ErrorSeverity.Critical, HttpStatusCode.InternalServerError));
        }
        
        return ApplicationExecuteResult<ApplicationUserEntity>.Success(user);
    }

    public async Task<ApplicationExecuteResult<ApplicationUserEntity>> ByLoginOrIdAsync(string loginOrId)
    {
        var byLogin = await userManager.FindByNameAsync(loginOrId);
        if (byLogin != null)
            return ApplicationExecuteResult<ApplicationUserEntity>.Success(byLogin);
        
        var byId = await userManager.FindByIdAsync(loginOrId);
        if (byId != null)
            return ApplicationExecuteResult<ApplicationUserEntity>.Success(byId);
        
        return ApplicationExecuteResult<ApplicationUserEntity>.Failure(new ApplicationError(
            UserErrors.UserNotFound, "Пользователь не найден",
            $"Пользователь с идентификатором {loginOrId} не найден или не существует", ErrorSeverity.Critical, HttpStatusCode.NotFound));
    }

    public async Task<ApplicationExecuteResult<List<ApplicationUserEntity>>> AllUsers()
    {
        var users = await userManager.Users.OrderBy(x => x.Id).ToListAsync();
        
        return ApplicationExecuteResult<List<ApplicationUserEntity>>.Success(users);
    }

    public async Task<ApplicationExecuteResult<Unit>> BlockOrUnblockAsync(ApplicationUserEntity user)
    {
        if (user.LockoutEnabled)
        {
            await userManager.ResetAccessFailedCountAsync(user);
            await userManager.SetLockoutEndDateAsync(user, null);
        }
        else
        {
            await userManager.SetLockoutEnabledAsync(user, true);
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(1));
        }
        
        return ApplicationExecuteResult<Unit>.Success(Unit.Value);
    }
}