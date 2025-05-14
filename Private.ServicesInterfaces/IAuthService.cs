using Private.StorageModels;
using Public.Models.BusinessModels.TokenModels;
using Public.Models.CommonModels;

namespace Private.ServicesInterfaces;

public interface IAuthService
{
    public Task<ApplicationExecuteLogicResult<AuthTokensPair>> SignInAndGetAuthTokensAsync(string login, string password);
    public Task<ApplicationExecuteLogicResult<AuthTokensPair>> GetFreshTokensAsync(string refreshToken);
    public Task<ApplicationExecuteLogicResult<Unit>> LogoutUserAsync(ApplicationUserEntity? user, bool globally, string? reason = null);
}