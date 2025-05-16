using Private.StorageModels;
using Public.Models.CommonModels;

namespace Private.Services.Repositories;

public interface IRefreshTokenRepository
{
    public Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> SaveTokenAsync(RefreshTokenEntity token);
    public Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> TokenByJtiAsync(string jti);
    public Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> TokenByBodyAsync(string refresh);
    public Task<ApplicationExecuteLogicResult<RefreshTokenEntity>> TokenByUserId(string userId);
    public Task<ApplicationExecuteLogicResult<Unit>> DeleteByIdAsync(int id);
    public Task<ApplicationExecuteLogicResult<Unit>> DeleteAllUsersTokens(string userId);
    
}