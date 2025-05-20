using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QPDCar.Infrastructure.DbContexts;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.StorageModels;
using QPDCar.Repositories.ErrorHelpers;
using QPDCar.Services.Repositories;

namespace QPDCar.Repositories.Repositories;

public class RefreshTokenRepository(AppDbContext db, ILogger<RefreshTokenRepository> logger) : IRefreshRepository
{
    private const string EntityName = "RefreshToken";
    public async Task<ApplicationExecuteResult<RefreshTokenEntity>> SaveTokenAsync(RefreshTokenEntity token)
    {
        try
        {
            await db.RefreshToken.AddAsync(token);
            await db.SaveChangesAsync();

            return ApplicationExecuteResult<RefreshTokenEntity>.Success(token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteResult<RefreshTokenEntity>.Failure(
                ErrorHelper.PrepareNotSavedError(EntityName));
        }
    }

    public async Task<ApplicationExecuteResult<RefreshTokenEntity>> TokenByJtiAsync(string jti)
    {
        try
        {
            var entity = await db.RefreshToken.FirstOrDefaultAsync(x => x.AccessJti == jti);
            if (entity == null)
                return ApplicationExecuteResult<RefreshTokenEntity>.Failure(
                    ErrorHelper.PrepareNotFoundErrorSingle(EntityName));

            return ApplicationExecuteResult<RefreshTokenEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteResult<RefreshTokenEntity>.Failure(
                ErrorHelper.PrepareNotFoundErrorSingle(EntityName));
        }
    }

    public async Task<ApplicationExecuteResult<RefreshTokenEntity>> TokenByBodyAsync(string refresh)
    {
        try
        {
            var entity = await db.RefreshToken.FirstOrDefaultAsync(x => x.RefreshBody == refresh);
            if (entity == null)
                return ApplicationExecuteResult<RefreshTokenEntity>.Failure(
                    ErrorHelper.PrepareNotFoundErrorSingle(EntityName));

            return ApplicationExecuteResult<RefreshTokenEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteResult<RefreshTokenEntity>.Failure(
                ErrorHelper.PrepareNotFoundErrorSingle(EntityName));
        }
    }

    public async Task<ApplicationExecuteResult<RefreshTokenEntity>> TokenByUserId(string userId)
    {
        try
        {
            var entity = await db.RefreshToken.FirstOrDefaultAsync(x => x.UserId == userId);
            if (entity == null)
                return ApplicationExecuteResult<RefreshTokenEntity>.Failure(
                    ErrorHelper.PrepareNotFoundErrorSingle(EntityName));

            return ApplicationExecuteResult<RefreshTokenEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteResult<RefreshTokenEntity>.Failure(
                ErrorHelper.PrepareNotFoundErrorSingle(EntityName));
        }
    }

    public async Task<ApplicationExecuteResult<Unit>> DeleteByIdAsync(int id)
    {
        try
        {
            var entity = await db.RefreshToken.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return ApplicationExecuteResult<Unit>.Failure(
                    ErrorHelper.PrepareNotDeletedError(EntityName));

            db.RefreshToken.Remove(entity);
            await db.SaveChangesAsync();

            return ApplicationExecuteResult<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteResult<Unit>.Failure(
                ErrorHelper.PrepareNotDeletedError(EntityName));
        }
    }

    public async Task<ApplicationExecuteResult<Unit>> DeleteAllUsersTokens(string userId)
    {
        try
        {
            var entities = await db.RefreshToken.Where(x => x.UserId == userId).ToListAsync();
            if (entities.Count == 0)
                return ApplicationExecuteResult<Unit>.Failure(
                    ErrorHelper.PrepareNotDeletedError(EntityName));

            db.RefreshToken.RemoveRange(entities);
            await db.SaveChangesAsync();

            return ApplicationExecuteResult<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteResult<Unit>.Failure(
                ErrorHelper.PrepareNotDeletedError(EntityName));
        }
    }
}