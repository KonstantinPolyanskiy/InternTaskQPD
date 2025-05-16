using Public.Models.CommonModels;

namespace Private.ServicesInterfaces;

/// <summary> Сервис для управления ролями пользователей </summary>
public interface IMailSenderService
{
    public Task<ApplicationExecuteLogicResult<Unit>> SendAsync(string to, string subject, string body);
}