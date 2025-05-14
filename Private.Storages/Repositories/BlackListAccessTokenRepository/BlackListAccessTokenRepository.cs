using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Private.Services.Repositories;
using Private.StorageModels;
using Private.Storages.DbContexts;
using Private.Storages.ErrorHelpers;
using Public.Models.CommonModels;

namespace Private.Storages.Repositories.BlackListAccessTokenRepository;

public class BlackListAccessTokenRepository(AppDbContext db, ILogger<BlackListAccessTokenRepository> logger) : IBlackListAccessTokenRepository
{
    private const string EntityName = "AccessTokenInBlackList";
    
    public async Task<ApplicationExecuteLogicResult<BlackListTokenAccessEntity>> SaveTokenInBlackListAsync(BlackListTokenAccessEntity entity)
    {
        try
        {
            await db.BlackListTokens.AddAsync(entity);
            await db.SaveChangesAsync();
            
            return ApplicationExecuteLogicResult<BlackListTokenAccessEntity>.Success(entity); 
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<BlackListTokenAccessEntity>.Failure(ErrorHelper.PrepareStorageException(EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<BlackListTokenAccessEntity>> TokenByJtiAsync(string jti)
    {
        try
        {
            var entity = await db.BlackListTokens.FirstOrDefaultAsync(x => x.Jti == jti);
            if (entity == null)
                return ApplicationExecuteLogicResult<BlackListTokenAccessEntity>.Failure(ErrorHelper.PrepareNotFoundError(EntityName));
            
            return ApplicationExecuteLogicResult<BlackListTokenAccessEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<BlackListTokenAccessEntity>.Failure(ErrorHelper.PrepareStorageException(EntityName));
        }
    }
}