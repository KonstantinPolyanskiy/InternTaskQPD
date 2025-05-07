using System.Net;
using Private.Services.Repositories;
using Private.StorageModels;
using Private.Storages.DbContexts;
using Public.Models.CommonModels;
using Public.Models.ErrorEnums;

namespace Private.Storages.Repositories.RefreshTokenRepository;

public class PostgresRefreshTokenRepository(AuthDbContext db) : IRefreshTokenRepository
{
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
            return ApplicationExecuteLogicResult<RefreshTokenEntity>.Failure(new ApplicationError(DatabaseErrors.SeeMessage, "Ошибка сохранения refresh",
                ex.Message, ErrorSeverity.Critical, HttpStatusCode.InternalServerError)); 
        }
    }

    public async Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> GetRefreshTokenByBodyAsync(string refreshTokenBody)
    {
        try
        {
            var entity = db.RefreshTokens.SingleOrDefault(x => x.RefreshTokenBody == refreshTokenBody);
            if (entity == null)
                return ApplicationExecuteLogicResult<RefreshTokenEntity>.Failure(NotFoundError());

            return ApplicationExecuteLogicResult<RefreshTokenEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            return ApplicationExecuteLogicResult<RefreshTokenEntity>.Failure(new ApplicationError(DatabaseErrors.SeeMessage, "Ошибка получения refresh",
                ex.Message, ErrorSeverity.Critical, HttpStatusCode.InternalServerError)); 
        }
    }

    public Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> GetRefreshTokenByJtiAsync(string jti)
    {
        try
        {
            
        }
    }

    public Task<ApplicationExecuteLogicResult<Unit>> DeleteRefreshTokenByIdAsync(int refreshTokenId)
    {
        throw new NotImplementedException();
    }

    public Task<ApplicationExecuteLogicResult<Unit>> DeleteRefreshTokenByBodyAsync(string refreshTokenBody)
    {
        throw new NotImplementedException();
    }

    public Task<ApplicationExecuteLogicResult<Unit>> DeleteAllUserRefreshTokensAsync(Guid userId)
    {
        throw new NotImplementedException();
    }
    
    private ApplicationError NotFoundError() => new ApplicationError(
        DatabaseErrors.NotFound, "Refresh не найден", "DbContext для сущности вернул null", ErrorSeverity.Critical, HttpStatusCode.NotFound);
    
}