using Private.StorageModels;
using Public.Models.CommonModels;

namespace Private.Services.Repositories;

public interface IRefreshTokenRepository
{
    public Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> SaveTokenAsync(RefreshTokenEntity token);
    public Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> TokenByJtiAsync(string jti);
    public Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> TokenByBodyAsync(string refresh);
    public Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> TokenByUserId(string userId);
    public Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> DeleteByIdAsync(int id);
    public Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> DeleteAllUsersTokens(string userId);
    
    
    
    public Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> SaveRefreshTokenAsync(RefreshTokenEntity refreshToken);
    public Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> GetRefreshTokenByBodyAsync(string refreshTokenBody);
    public Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> GetRefreshTokenByJtiAsync(string jti);
    public Task<ApplicationExecuteLogicResult<Unit>> DeleteRefreshTokenByIdAsync(int refreshTokenId);
    public Task<ApplicationExecuteLogicResult<Unit>> DeleteRefreshTokenByBodyAsync(string refreshTokenBody);
    public Task<ApplicationExecuteLogicResult<Unit>> DeleteAllUserRefreshTokensAsync(Guid userId);
}