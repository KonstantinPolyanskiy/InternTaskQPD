using Private.StorageModels;
using Public.Models.CommonModels;

namespace Private.ServicesInterfaces;

public interface IEmailConfirmationService
{
    /// <summary> Создать токен подтверждения почты</summary>
    public Task<ApplicationExecuteLogicResult<string>> CreateConfirmationTokenAsync(ApplicationUserEntity user);
    
    /// <summary> Проверить и установить почту пользователя как подтвержденная </summary>
    public Task<ApplicationExecuteLogicResult<Unit>> ConfirmEmailAsync(ApplicationUserEntity user, string token);
}