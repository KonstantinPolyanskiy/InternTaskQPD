using Private.StorageModels;
using Public.Models.CommonModels;

namespace Private.Services.Repositories;

public interface IEmailConfirmationTokenRepository
{
    public Task<ApplicationExecuteLogicResult<EmailConfirmationTokenEntity>> SaveEmailConfirmationTokenAsync(Guid userId, string token, DateTime expiresAt);
    public Task<ApplicationExecuteLogicResult<EmailConfirmationTokenEntity>> GetEmailConfirmationTokenByBodyAsync(string tokenBody);
    public Task<ApplicationExecuteLogicResult<EmailConfirmationTokenEntity>> RewriteEmailConfirmationTokenAsync(EmailConfirmationTokenEntity entity);
}