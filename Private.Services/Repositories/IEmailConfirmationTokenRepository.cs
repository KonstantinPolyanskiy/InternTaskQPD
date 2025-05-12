using Private.StorageModels;
using Public.Models.CommonModels;

namespace Private.Services.Repositories;

public interface IEmailConfirmationTokenRepository
{
    public Task<ApplicationExecuteLogicResult<EmailConfirmationTokenEntity>> SaveEmailConfirmationTokenAsync(EmailConfirmationTokenEntity entity);
    public Task<ApplicationExecuteLogicResult<EmailConfirmationTokenEntity>> GetEmailConfirmationTokenByBodyAsync(string tokenBody);
    public Task<ApplicationExecuteLogicResult<EmailConfirmationTokenEntity>> RewriteEmailConfirmationTokenAsync(EmailConfirmationTokenEntity entity);
}