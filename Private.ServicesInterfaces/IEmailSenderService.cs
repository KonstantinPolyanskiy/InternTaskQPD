using Public.Models.CommonModels;

namespace Private.ServicesInterfaces;

/// <summary> Сервис для управления ролями пользователей </summary>
public interface IMailSenderService
{
    /// <summary> Отправляет recipient письмо с ссылкой на подтверждение аккаунта </summary>
    public Task<ApplicationExecuteLogicResult<Unit>> SendConfirmationEmailAsync(string recipient, string url);
    
    /// <summary> Отправляет recipient письмо с благодарностью о подтверждении почты </summary>
    public Task<ApplicationExecuteLogicResult<Unit>> SendThanksForConfirmationEmailAsync(string recipient, string thanksMessage);
    
    /// <summary> Отправляет recipient письмо о входе в аккаунт </summary>
    public Task<ApplicationExecuteLogicResult<Unit>> SendAccountLoginEmailAsync(string recipient, string loginMessage);
    
    /// <summary> Отправляет recipient письмо о том, что менеджер добавил машину без фото </summary>
    public Task<ApplicationExecuteLogicResult<Unit>> SendNoPhotoNotifyEmailAsync(string recipient, string managerLogin, int carId);
}