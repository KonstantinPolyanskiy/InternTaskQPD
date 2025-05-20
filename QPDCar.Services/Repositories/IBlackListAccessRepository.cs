using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.StorageModels;

namespace QPDCar.Services.Repositories;

public interface IBlackListAccessRepository
{
    public Task<ApplicationExecuteResult<BlackListAccessTokenEntity>> SaveTokenInBlackListAsync(BlackListAccessTokenEntity entity);
    public Task<ApplicationExecuteResult<BlackListAccessTokenEntity>> TokenByJtiAsync(string jti);
}