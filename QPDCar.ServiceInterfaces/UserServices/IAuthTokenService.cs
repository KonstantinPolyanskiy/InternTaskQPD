using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.AuthModels;
using QPDCar.Models.BusinessModels.EmployerModels;
using QPDCar.Models.StorageModels;

namespace QPDCar.ServiceInterfaces.UserServices;

public interface IAuthTokenService
{
    /// <summary> Создает пару токенов для авторизации </summary>
    public Task<ApplicationExecuteResult<AuthTokensPair>> GenerateAuthTokensPairAsync(ApplicationUserEntity user, List<ApplicationRoles> roles);
    
    /// <summary> Обновляет пару токенов для авторизации </summary>
    public Task<ApplicationExecuteResult<AuthTokensPair>> RefreshAuthTokensPairAsync(string refreshToken, ApplicationUserEntity user, List<ApplicationRoles> roles);
    
    /// <summary> Блокирует (отзывает) access токен/ы авторизации </summary>
    public Task<ApplicationExecuteResult<Unit>> RevokeAccessTokenAsync(ApplicationUserEntity user, string jti, string reason, bool global);
    
    /// <summary> Блокирует refresh токен/ы авторизации </summary>
    public Task<ApplicationExecuteResult<Unit>> BlockRefreshTokenAsync(ApplicationUserEntity user, string reason, bool global);
   
    /// <summary> Находится ли access токен по Jti в блокировке </summary>
    public Task<ApplicationExecuteResult<bool>> AccessTokenIsBlockedAsync(string jti);
}