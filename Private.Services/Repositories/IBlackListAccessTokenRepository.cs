using Private.StorageModels;
using Public.Models.CommonModels;

namespace Private.Services.Repositories;

public interface IBlackListAccessTokenRepository
{
    public Task<ApplicationExecuteLogicResult<BlackListTokenAccessEntity>> SaveTokenInBlackListAsync(BlackListTokenAccessEntity entity);
    public Task<ApplicationExecuteLogicResult<BlackListTokenAccessEntity>> TokenByJtiAsync(string jti);
}