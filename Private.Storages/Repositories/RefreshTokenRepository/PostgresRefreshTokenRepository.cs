using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Private.Services.Repositories;
using Private.StorageModels;
using Private.Storages.DbContexts;
using Private.Storages.ErrorHelpers;
using Public.Models.CommonModels;

namespace Private.Storages.Repositories.RefreshTokenRepository;

public class PostgresRefreshTokenRepository(AuthDbContext db, ILogger<PostgresRefreshTokenRepository> logger) : IRefreshTokenRepository
{
    private const string EntityName = "RefreshToken";
    
    public async Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> SaveRefreshTokenAsync(RefreshTokenEntity refreshToken)
    {
        try
        {
            await db.RefreshTokens.AddAsync(refreshToken);
            await db.SaveChangesAsync();
            
            return ApplicationExecuteLogicResult<RefreshTokenEntity>.Success(refreshToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<RefreshTokenEntity>.Failure(ErrorHelper.PrepareStorageException(EntityName)); 
        }
    }

    public async Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> GetRefreshTokenByBodyAsync(string refreshTokenBody)
    {
        try
        {
            var entity = await db.RefreshTokens.FirstOrDefaultAsync(x => x.RefreshTokenBody == refreshTokenBody);
            if (entity == null)
                return ApplicationExecuteLogicResult<RefreshTokenEntity>.Failure(ErrorHelper.PrepareNotFoundError(EntityName));

            return ApplicationExecuteLogicResult<RefreshTokenEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<RefreshTokenEntity>.Failure(ErrorHelper.PrepareStorageException(EntityName)); 
        }
    }

    public async Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> GetRefreshTokenByJtiAsync(string jti)
    {
        try
        {
            var entity = await db.RefreshTokens.FirstOrDefaultAsync(x => x.Jti == jti);
            if (entity == null)
                return ApplicationExecuteLogicResult<RefreshTokenEntity>.Failure(ErrorHelper.PrepareNotFoundError(EntityName));
            
            return ApplicationExecuteLogicResult<RefreshTokenEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<RefreshTokenEntity>.Failure(ErrorHelper.PrepareStorageException(EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> DeleteRefreshTokenByIdAsync(int refreshTokenId)
    {
        try
        {
            var entity = await db.RefreshTokens.FirstOrDefaultAsync(x => x.Id == refreshTokenId);
            if (entity == null)
                return ApplicationExecuteLogicResult<Unit>.Failure(ErrorHelper.PrepareNotFoundError(EntityName));
            
            db.RefreshTokens.Remove(entity);
            await db.SaveChangesAsync();
            
            return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<Unit>.Failure(ErrorHelper.PrepareStorageException(EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> DeleteRefreshTokenByBodyAsync(string refreshTokenBody)
    {
        try
        {
            var entity = await db.RefreshTokens.FirstOrDefaultAsync(x => x.RefreshTokenBody == refreshTokenBody);
            if (entity == null)
                return ApplicationExecuteLogicResult<Unit>.Failure(ErrorHelper.PrepareNotFoundError(EntityName));
            
            db.RefreshTokens.Remove(entity);
            await db.SaveChangesAsync();
            
            return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<Unit>.Failure(ErrorHelper.PrepareStorageException(EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> DeleteAllUserRefreshTokensAsync(Guid userId)
    {
        try
        {
            var entities = db.RefreshTokens.Where(x => x.UserId == userId);
            if (!entities.Any())
                return ApplicationExecuteLogicResult<Unit>.Failure(ErrorHelper.PrepareNotFoundError(EntityName));
            
            db.RefreshTokens.RemoveRange(entities);
            await db.SaveChangesAsync();

            return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<Unit>.Failure(ErrorHelper.PrepareStorageException(EntityName));
        }
    }
}