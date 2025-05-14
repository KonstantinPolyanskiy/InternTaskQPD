using Private.ServicesInterfaces.Commands.AuthTokenServiceCommands;
using Private.StorageModels;
using Public.Models.BusinessModels.TokenModels;
using Public.Models.CommonModels;

namespace Private.ServicesInterfaces;

public interface IAuthTokenService
{
    /// <summary> Создает пару токенов для авторизации </summary>
    public Task<ApplicationExecuteLogicResult<AuthTokensPair>> GenerateAuthTokensPairAsync(ApplicationUserEntity user, List<ApplicationUserRole> roles);
    
    /// <summary> Обновляет пару токенов для авторизации </summary>
    public Task<ApplicationExecuteLogicResult<AuthTokensPair>> RefreshAuthTokensPairAsync(string refreshToken, ApplicationUserEntity user, List<ApplicationUserRole> roles);
    
    /// <summary> Блокирует (отзывает) access токен/ы авторизации </summary>
    public Task<ApplicationExecuteLogicResult<Unit>> RevokeAccessTokenAsync(BlockAccessTokenCommand blockCommand);
    
    /// <summary> Блокирует refresh токен/ы авторизации </summary>
    public Task<ApplicationExecuteLogicResult<Unit>> BlockRefreshTokenAsync(BlockRefreshTokenCommand blockCommand);
   
    /// <summary> Находится ли access токен по Jti в блокировке </summary>
    public Task<ApplicationExecuteLogicResult<bool>> AccessTokenIsBlockedAsync(string jti);
    
}