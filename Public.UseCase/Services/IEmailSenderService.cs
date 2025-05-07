using Public.Models.BusinessModels.EmailModels;
using Public.Models.CommonModels;

namespace Public.UseCase.Services;

/// <summary> Сервис для управления ролями пользователей </summary>
public interface IMailSenderService
{
    public Task<ApplicationExecuteLogicResult<string>> SendConfirmationEmailAsync(string recipient, string url);
}