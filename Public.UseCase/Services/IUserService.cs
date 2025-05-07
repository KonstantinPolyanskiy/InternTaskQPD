using Microsoft.AspNetCore.Identity;
using Public.Models.CommonModels;
using Public.Models.UserModels;
using Public.UseCase.Models;

namespace Public.UseCase.Services;

/// <summary> Сервис для управления пользователями и их данными </summary>
public interface IUserService
{
    /// <summary> Создать и сохранить пользователя </summary>
    internal Task<ApplicationExecuteLogicResult<ApplicationUser>> CreateUserAsync(DataForCreateUser data);
    
    /// <summary> Получить пользователя по Id </summary>
    internal Task<ApplicationExecuteLogicResult<ApplicationUser>> UserByLoginAsync(string login);
    
    /// <summary> Валиден ли пароль для пользователя </summary>
    internal Task<ApplicationExecuteLogicResult<bool>> CheckPasswordForUserAsync(IdentityUser user, string password);
}