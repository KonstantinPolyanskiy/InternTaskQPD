using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

    public async Task<ApplicationExecuteLogicResult<ApplicationUserEntity>> ByLoginOrIdAsync(string userIdentifier)
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

    public async Task<ApplicationExecuteLogicResult<Unit>> BlockUser(ApplicationUserEntity user)
    {
        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(1));
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> UnblockUser(ApplicationUserEntity user)
    {
        await _userManager.ResetAccessFailedCountAsync(user);
        await _userManager.SetLockoutEndDateAsync(user, null);
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }
    
    public async Task<ApplicationExecuteLogicResult<List<ApplicationUserEntity>>> UsersAll()
    {
        var users = await _userManager.Users.OrderBy(u => u.Id).ToListAsync();

        return ApplicationExecuteLogicResult<List<ApplicationUserEntity>>.Success(users);
    }

    public async Task<ApplicationExecuteLogicResult<ApplicationUserEntity>> UpdateAsync(ApplicationUserEntity user)
    {
        await _userManager.UpdateAsync(user);
        
        return ApplicationExecuteLogicResult<ApplicationUserEntity>.Success(user);
    }
}