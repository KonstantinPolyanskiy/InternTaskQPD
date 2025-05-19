using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.AuthModels;
using QPDCar.Models.StorageModels;

namespace QPDCar.ServiceInterfaces.UserServices;

public interface IAuthService
{
    /// <summary> Выдает пару токенов для авторизации </summary>
    Task<ApplicationExecuteResult<AuthTokensPair>> SignInAndGetAuthTokensAsync(string login, string password);
    
    /// <summary> Обновляет пару токенов авторизации </summary>
    Task<ApplicationExecuteResult<AuthTokensPair>> GetFreshTokensAsync(string refreshToken);
    
    /// <summary> Инвалидирует токены пользователя </summary>
    Task<ApplicationExecuteResult<Unit>> LogoutUserAsync(ApplicationUserEntity? user, bool globally, string? reason = null);
}