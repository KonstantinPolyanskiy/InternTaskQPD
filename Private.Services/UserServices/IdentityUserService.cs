using System.ComponentModel;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Private.ServicesInterfaces;
using Private.StorageModels;
using Public.Models.ApplicationErrors;
using Public.Models.CommonModels;
using Public.Models.DtoModels.UserDtoModels;
using Public.Models.Extensions;

namespace Private.Services.UserServices;

public class IdentityUserService : IUserService
{
    private readonly ILogger<IdentityUserService> _logger;

    private readonly UserManager<ApplicationUserEntity> _userManager;
    
    // ReSharper disable once ConvertToPrimaryConstructor
    public IdentityUserService(UserManager<ApplicationUserEntity> userManager, ILogger<IdentityUserService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }
    
    public async Task<ApplicationExecuteLogicResult<ApplicationUserEntity>> CreateUserAsync(DataForCreateUser dto)
    {
        _logger.LogInformation("Попытка создать пользователя c логином {login}", dto.Login);
        _logger.LogDebug("Входные данные - {@data}", dto);
        
        // Логин не занят
        var userByName = await _userManager.FindByNameAsync(dto.Login);
        if (userByName is not null)
        {
            _logger.LogWarning("Аккаунт с логином {login} уже существует", dto.Login);
            return ApplicationExecuteLogicResult<ApplicationUserEntity>.Failure(new ApplicationError(
                UserErrors.LoginIsBusy, "Логин занят",
                $"Пользователь с логином {dto.Login} уже существует", ErrorSeverity.Critical, HttpStatusCode.BadRequest));
        }
        
        // Почта не занята
        var userByEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (userByEmail is not null)
        {
            _logger.LogWarning("Аккаунт с почтой {email} уже существует", dto.Email);
            return ApplicationExecuteLogicResult<ApplicationUserEntity>.Failure(new ApplicationError(
                UserErrors.EmailIsBusy, "Почта занята",
                $"Пользователь с почтой {dto.Email} уже существует", ErrorSeverity.Critical, HttpStatusCode.BadRequest));
        }
        
        // Сохраняем в БД 
        var user = new ApplicationUserEntity
        {
            UserName = dto.Login,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
        };
        
        var saved = await _userManager.CreateAsync(user, dto.Password);
        if (saved.Succeeded is not true)
        {
            _logger.LogError("Не получилось сохранить пользователя в хранилище, ошибки - {@errors}", saved.Errors);
            return ApplicationExecuteLogicResult<ApplicationUserEntity>.Failure(new ApplicationError(
                UserErrors.FailSaveUser, "Пользователь не сохранен", 
                "В процессе сохранения пользователя возникла неизвестная ошибка", ErrorSeverity.Critical, HttpStatusCode.InternalServerError));
        }
        
        _logger.LogInformation("Пользователь с логином {login} успешно создан и сохранен", user.UserName);

        return ApplicationExecuteLogicResult<ApplicationUserEntity>.Success(user);
    }

    public async Task<ApplicationExecuteLogicResult<ApplicationUserEntity>> UserByLoginOrIdAsync(string userIdentifier)
    {
        _logger.LogInformation("Попытка найти пользователя по id/login {@id}", userIdentifier);

        // Ищем пользователя
        var userById = await _userManager.FindByIdAsync(userIdentifier);
        var userByLogin = await _userManager.FindByNameAsync(userIdentifier);

        if (userById is null && userByLogin is null)
        {
            return ApplicationExecuteLogicResult<ApplicationUserEntity>.Failure(new ApplicationError(
                UserErrors.UserNotFound, "Пользователь не найден",
                $"Пользователь с идентификатором {userIdentifier} не найден или не существует", ErrorSeverity.Critical, HttpStatusCode.NotFound));
        }
        
        _logger.LogInformation("Пользователь по логину {@id} успешно найден", userIdentifier);
        _logger.LogDebug("Найденный пользователь - {user}", userById);  
        
        var user = userById ?? userByLogin;

        return ApplicationExecuteLogicResult<ApplicationUserEntity>.Success(user!);
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> DeleteUserByLoginOrIdAsync(string userIdentifier)
    {
        var userResult = await UserByLoginOrIdAsync(userIdentifier);
        if (userResult.IsSuccess is not true || userResult.Value is null)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(userResult);
        var user = userResult.Value;
        
        var deleted = await _userManager.DeleteAsync(user);
        if (deleted.Succeeded is not true)
        {
            _logger.LogError("Ошибка при удалении пользователя - {@err}", deleted.Errors);
            return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(UserErrors.FailDeleteUser, "Пользователь не удален", 
                "При удалении пользователя возникла неизвестная ошибка", ErrorSeverity.Critical, HttpStatusCode.InternalServerError));
        }
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }
    
    public async Task<ApplicationExecuteLogicResult<List<ApplicationUserEntity>>> UsersAll()
    {
        var users = await _userManager.Users.OrderBy(u => u.Id).ToListAsync();

        return ApplicationExecuteLogicResult<List<ApplicationUserEntity>>.Success(users);
    }

    public async Task<ApplicationExecuteLogicResult<ApplicationUserEntity>> SaveUserAsync(ApplicationUserEntity user)
    {
        await _userManager.UpdateAsync(user);
        
        return ApplicationExecuteLogicResult<ApplicationUserEntity>.Success(user);
    }

    public async Task<ApplicationExecuteLogicResult<bool>> CheckPasswordForUserAsync(ApplicationUserEntity user, string password)
    {
        _logger.LogInformation("Проверка пароля пользователя {login}", user.UserName);

        var isValid = await _userManager.CheckPasswordAsync(user, password);
        if (isValid is not true)
        {
            _logger.LogWarning("Пароль {password} для пользователя {login} не подходит", password, user.UserName);
            return ApplicationExecuteLogicResult<bool>.Failure(new ApplicationError(
                UserErrors.PasswordIsNotValid, "Пароль неверен",
                $"Переданный пароль не подходит для указанного пользователя", ErrorSeverity.Critical, HttpStatusCode.Forbidden));
        }
        
        _logger.LogInformation("Проверка пользователя {login} успешна", user.UserName);
        return ApplicationExecuteLogicResult<bool>.Success(true);
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> SetEmailAddressAsConfirmedAsync(ApplicationUserEntity user)
    {
        _logger.LogInformation("Смена состояния почты пользователя с id {@id} на подтвержденную", user.Id);
        
        user.EmailConfirmed = true;
        
        var updated= await _userManager.UpdateAsync(user);
        if (updated.Succeeded is not true)
        {
            _logger.LogError("Не получилось обновить пользователя в хранилище, ошибки - {@errors}", updated.Errors);
            return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(
                UserErrors.FailSaveUser, "Пользователь не обновлен", 
                "В процессе обновления пользователя возникла неизвестная ошибка", ErrorSeverity.Critical, HttpStatusCode.InternalServerError));
        }
        
        _logger.LogInformation("Почта пользователя с id {@id} теперь подтвержденная", user.Id);
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> UpdateUserSecurityStampAsync(ApplicationUserEntity user)
    {
        _logger.LogInformation("Обновление метки безопасности пользователя с id {@id} на новую", user.Id);
        
        var updated= await _userManager.UpdateSecurityStampAsync(user);
        if (updated.Succeeded is not true)
        {
            _logger.LogError("Не получилось обновить метку безопасности пользователя, ошибки - {@errors}", updated.Errors);
            return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(
                UserErrors.FailSaveUser, "Метка безопасности не обновлена", 
                "В процессе обновления метки безопасности пользователя возникла неизвестная ошибка, access токены не инвалидированы", 
                ErrorSeverity.Critical, HttpStatusCode.InternalServerError));
        }
        
        _logger.LogInformation("Метки безопасности пользователя с id {@id} теперь новые", user.Id);
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }
}