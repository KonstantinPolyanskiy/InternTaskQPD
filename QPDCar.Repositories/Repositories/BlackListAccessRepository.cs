using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QPDCar.Infrastructure.DbContexts;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.StorageModels;
using QPDCar.Repositories.ErrorHelpers;
using QPDCar.Services.Repositories;

namespace QPDCar.Repositories.Repositories;

public class BlackListAccessTokenRepository(AppDbContext db, ILogger<BlackListAccessTokenRepository> logger) : IBlackListAccessRepository
{
    private const string EntityName = "AccessTokenInBlackList";

    public async Task<ApplicationExecuteResult<BlackListAccessTokenEntity>> SaveTokenInBlackListAsync(BlackListAccessTokenEntity entity)
    {
        try
        {
            await db.BlackListToken.AddAsync(entity);
            await db.SaveChangesAsync();
            
            return ApplicationExecuteResult<BlackListAccessTokenEntity>.Success(entity); 
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteResult<BlackListAccessTokenEntity>.Failure(ErrorHelper.PrepareNotSavedError(EntityName));
        }
    }

    public async Task<ApplicationExecuteResult<BlackListAccessTokenEntity>> TokenByJtiAsync(string jti)
    {
        try
        {
            var entity = await db.BlackListToken.FirstOrDefaultAsync(x => x.Jti == jti);
            if (entity == null)
                return ApplicationExecuteResult<BlackListAccessTokenEntity>.Failure(ErrorHelper.PrepareNotFoundErrorMany(EntityName));
            
            return ApplicationExecuteResult<BlackListAccessTokenEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteResult<BlackListAccessTokenEntity>.Failure(ErrorHelper.PrepareNotFoundErrorSingle(EntityName));
        }
    }
}