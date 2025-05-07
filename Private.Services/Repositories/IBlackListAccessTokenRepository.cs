using Private.StorageModels;
using Public.Models.CommonModels;

namespace Private.Services.Repositories;

public interface IBlackListAccessTokenRepository
{
    public Task<ApplicationExecuteLogicResult<Unit>> AddAccessTokenToBlackListAsync(BlackListTokenAccessEntity entity);
    public Task<ApplicationExecuteLogicResult<BlackListTokenAccessEntity>> GetAccessTokenByJtiAsync(string jti);
}