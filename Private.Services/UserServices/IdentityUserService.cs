using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Private.ServicesInterfaces;
using Private.StorageModels;
using Public.Models.ApplicationErrors;
using Public.Models.CommonModels;
using Public.Models.DtoModels.UserDtoModels;

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
    
    public async Task<ApplicationExecuteLogicResult<ApplicationUserEntity>> CreateUserAsync(DtoForCreateUser dto)
    {
        _logger.LogInformation("Попытка создать пользователя c логином {login}", dto.Data.Login);
        _logger.LogDebug("Входные данные - {@data}", dto);
        
        // Логин не занят
        var userByName = await _userManager.FindByNameAsync(dto.Data.Login);
        if (userByName is not null)
        {
            _logger.LogWarning("Аккаунт с логином {login} уже существует", dto.Data.Login);
            return ApplicationExecuteLogicResult<ApplicationUserEntity>.Failure(new ApplicationError(
                UserErrors.LoginIsBusy, "Логин занят",
                $"Пользователь с логином {dto.Data.Login} уже существует", ErrorSeverity.Critical, HttpStatusCode.BadRequest));
        }
        
        // Почта не занята
        var userByEmail = await _userManager.FindByEmailAsync(dto.Data.Email);
        if (userByEmail is not null)
        {
            _logger.LogWarning("Аккаунт с почтой {email} уже существует", dto.Data.Email);
            return ApplicationExecuteLogicResult<ApplicationUserEntity>.Failure(new ApplicationError(
                UserErrors.EmailIsBusy, "Почта занята",
                $"Пользователь с почтой {dto.Data.Email} уже существует", ErrorSeverity.Critical, HttpStatusCode.BadRequest));
        }
        
        // Сохраняем в БД 
        var user = new ApplicationUserEntity
        {
            UserName = dto.Data.Login,
            Email = dto.Data.Email,
            FirstName = dto.Data.FirstName,
            LastName = dto.Data.LastName,
        };
        
        var saved = await _userManager.CreateAsync(user, dto.Data.Password);
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

    public async Task<ApplicationExecuteLogicResult<ApplicationUserEntity>> UserByLoginAsync(string login)
    {
        _logger.LogInformation("Попытка найти пользователя по логину {login}", login);
        
        // Ищем пользователя
        var user = await _userManager.FindByNameAsync(login);
        if (user is null)
        {
            _logger.LogWarning("Аккаунт с логином {login} не найден", login);
            return ApplicationExecuteLogicResult<ApplicationUserEntity>.Failure(new ApplicationError(
                UserErrors.UserNotFound, "Пользователь не найден",
                $"Пользователь с логином {login} не найден или не существует", ErrorSeverity.Critical, HttpStatusCode.NotFound));
        }
        
        _logger.LogInformation("Пользователь по логину {login} успешно найден", login);
        _logger.LogDebug("Найденный пользователь - {user}", user);

        return ApplicationExecuteLogicResult<ApplicationUserEntity>.Success(user);
    }

    public async Task<ApplicationExecuteLogicResult<ApplicationUserEntity>> UserByIdAsync(Guid userId)
    {
        _logger.LogInformation("Попытка найти пользователя по id {@id}", userId);
        
        // Ищем пользователя
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            _logger.LogWarning("Аккаунт с логином {@id} не найден", userId);
            return ApplicationExecuteLogicResult<ApplicationUserEntity>.Failure(new ApplicationError(
                UserErrors.UserNotFound, "Пользователь не найден",
                $"Пользователь с id {userId.ToString()} не найден или не существует", ErrorSeverity.Critical, HttpStatusCode.NotFound));
        }
        
        _logger.LogInformation("Пользователь по логину {@id} успешно найден", userId);
        _logger.LogDebug("Найденный пользователь - {user}", user);

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

    public async Task<ApplicationExecuteLogicResult<List<string>>> GetRolesByUser(ApplicationUserEntity user)
    {
        _logger.LogInformation("Попытка получить роли пользователя с id {id}", user.Id);
        
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Any())
            return ApplicationExecuteLogicResult<List<string>>.Failure(new ApplicationError(
                UserErrors.NotFoundAnyRoleForUser, "Роли не найдены",
                $"Не найдено ни 1 роли для пользователя {user.UserName}", ErrorSeverity.Critical, HttpStatusCode.NotFound));
        
        _logger.LogInformation("Для пользователя с id {id} найдено {count} ролей", user.Id, roles.Count);

        return ApplicationExecuteLogicResult<List<string>>.Success(roles.ToList());
    }
}