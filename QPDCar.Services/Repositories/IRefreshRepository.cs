using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.StorageModels;

namespace QPDCar.Services.Repositories;

public interface IRefreshRepository
{
    Task<ApplicationExecuteResult<RefreshTokenEntity>> SaveTokenAsync(RefreshTokenEntity token);
    Task<ApplicationExecuteResult<RefreshTokenEntity>> TokenByJtiAsync(string jti);
    Task<ApplicationExecuteResult<RefreshTokenEntity>> TokenByBodyAsync(string refresh);
    Task<ApplicationExecuteResult<RefreshTokenEntity>> TokenByUserId(string userId);
    Task<ApplicationExecuteResult<Unit>> DeleteByIdAsync(int id);
    Task<ApplicationExecuteResult<Unit>> DeleteAllUsersTokens(string userId);
}