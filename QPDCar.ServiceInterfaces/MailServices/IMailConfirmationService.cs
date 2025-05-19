using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.StorageModels;

namespace QPDCar.ServiceInterfaces.MailServices;

/// <summary> Сервис для подтверждения почты пользователя </summary>
public interface IMailConfirmationService
{
    /// <summary> Создать токен подтверждения почты</summary>
    Task<ApplicationExecuteResult<string>> CreateConfirmationTokenAsync(ApplicationUserEntity user);
    
    /// <summary> Проверить и установить почту пользователя как подтвержденная </summary>
    Task<ApplicationExecuteResult<Unit>> ConfirmEmailAsync(ApplicationUserEntity user, string token);
}