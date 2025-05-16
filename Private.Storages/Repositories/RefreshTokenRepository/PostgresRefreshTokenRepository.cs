using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Private.Services.Repositories;
using Private.StorageModels;
using Private.Storages.DbContexts;
using Private.Storages.ErrorHelpers;
using Public.Models.ApplicationErrors;
using Public.Models.CommonModels;

namespace Private.Storages.Repositories.RefreshTokenRepository;

public class PostgresRefreshTokenRepository(AppDbContext db, ILogger<PostgresRefreshTokenRepository> logger) : IRefreshTokenRepository
{
    private const string EntityName = "RefreshToken";
    public async Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> SaveTokenAsync(RefreshTokenEntity token)
    {
        try
        {
            await db.RefreshTokens.AddAsync(token);
            await db.SaveChangesAsync();

            return ApplicationExecuteLogicResult<RefreshTokenEntity>.Success(token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<RefreshTokenEntity>.Failure(
                ErrorHelper.PrepareStorageException(EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> TokenByJtiAsync(string jti)
    {
        try
        {
            var entity = await db.RefreshTokens.FirstOrDefaultAsync(x => x.AccessJti == jti);
            if (entity == null)
                return ApplicationExecuteLogicResult<RefreshTokenEntity>.Failure(
                    ErrorHelper.PrepareNotFoundError(EntityName));

            return ApplicationExecuteLogicResult<RefreshTokenEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<RefreshTokenEntity>.Failure(
                ErrorHelper.PrepareStorageException(EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> TokenByBodyAsync(string refresh)
    {
        try
        {
            var entity = await db.RefreshTokens.FirstOrDefaultAsync(x => x.RefreshBody == refresh);
            if (entity == null)
                return ApplicationExecuteLogicResult<RefreshTokenEntity>.Failure(
                    ErrorHelper.PrepareNotFoundError(EntityName));

            return ApplicationExecuteLogicResult<RefreshTokenEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<RefreshTokenEntity>.Failure(
                ErrorHelper.PrepareStorageException(EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> TokenByUserId(string userId)
    {
        try
        {
            var entity = await db.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId);
            if (entity == null)
                return ApplicationExecuteLogicResult<RefreshTokenEntity>.Failure(
                    ErrorHelper.PrepareNotFoundError(EntityName));

            return ApplicationExecuteLogicResult<RefreshTokenEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<RefreshTokenEntity>.Failure(
                ErrorHelper.PrepareStorageException(EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> DeleteByIdAsync(int id)
    {
        try
        {
            var entity = await db.RefreshTokens.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return ApplicationExecuteLogicResult<Unit>.Failure(
                    ErrorHelper.PrepareNotFoundError(EntityName));

            db.RefreshTokens.Remove(entity);
            await db.SaveChangesAsync();

            return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<Unit>.Failure(
                ErrorHelper.PrepareStorageException(EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> DeleteAllUsersTokens(string userId)
    {
        try
        {
            var entities = await db.RefreshTokens.Where(x => x.UserId == userId).ToListAsync();
            if (entities.Count == 0)
                return ApplicationExecuteLogicResult<Unit>.Failure(
                    ErrorHelper.PrepareNotFoundError(EntityName));

            db.RefreshTokens.RemoveRange(entities);
            await db.SaveChangesAsync();

            return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<Unit>.Failure(
                ErrorHelper.PrepareStorageException(EntityName));
        }
    }
}
