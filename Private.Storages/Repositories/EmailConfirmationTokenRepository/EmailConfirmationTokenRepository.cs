using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Private.Services.Repositories;
using Private.StorageModels;
using Private.Storages.DbContexts;
using Private.Storages.ErrorHelpers;
using Public.Models.CommonModels;

namespace Private.Storages.Repositories.EmailConfirmationTokenRepository;

public class EmailConfirmationTokenRepository(AppDbContext db, ILogger<EmailConfirmationTokenRepository> logger) : IEmailConfirmationTokenRepository
{
    private const string EntityName = "EmailConfirmationToken";
    
    public async Task<ApplicationExecuteLogicResult<EmailConfirmationTokenEntity>> SaveEmailConfirmationTokenAsync(EmailConfirmationTokenEntity entity)
    {
        try
        {
            await db.EmailConfirmationTokens.AddAsync(entity);
            await db.SaveChangesAsync();
            
            return ApplicationExecuteLogicResult<EmailConfirmationTokenEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<EmailConfirmationTokenEntity>.Failure(ErrorHelper.PrepareStorageException(EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<EmailConfirmationTokenEntity>> GetEmailConfirmationTokenByBodyAsync(string tokenBody)
    {
        try
        {
            var entity = await db.EmailConfirmationTokens.FirstOrDefaultAsync(e => e.TokenBody == tokenBody);
            if (entity == null)
                return ApplicationExecuteLogicResult<EmailConfirmationTokenEntity>.Failure(ErrorHelper.PrepareNotFoundError(EntityName));
            
            return ApplicationExecuteLogicResult<EmailConfirmationTokenEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<EmailConfirmationTokenEntity>.Failure(ErrorHelper.PrepareStorageException(EntityName)); 
        }
    }

    public async Task<ApplicationExecuteLogicResult<EmailConfirmationTokenEntity>> RewriteEmailConfirmationTokenAsync(EmailConfirmationTokenEntity entity)
    {
        try
        {   
            var exist = await db.EmailConfirmationTokens.FirstOrDefaultAsync(e => e.TokenBody == entity.TokenBody);
            if (exist == null)
                return ApplicationExecuteLogicResult<EmailConfirmationTokenEntity>.Failure(ErrorHelper.PrepareNotFoundError(EntityName));
            
            db.EmailConfirmationTokens.Update(entity);
            await db.SaveChangesAsync();
            
            return ApplicationExecuteLogicResult<EmailConfirmationTokenEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<EmailConfirmationTokenEntity>.Failure(ErrorHelper.PrepareStorageException(EntityName));
        }
    }
}