using Public.Models.BusinessModels.TokenModels;
using Public.Models.CommonModels;
using Public.UseCase.Models;

namespace Public.UseCase.Services;

/// <summary> Сервис для работы с различными токенами </summary>
public interface ITokenService
{
    /// <summary> Создать токен для подтверждения Email </summary>
    public Task<ApplicationExecuteLogicResult<string>> GenerateConfirmEmailTokenAsync(Guid userId, int tokenLiveHours);
    public Task<ApplicationExecuteLogicResult<ConfirmEmailToken>> CheckEmailConfirmationToken(Guid userId, string token);
}