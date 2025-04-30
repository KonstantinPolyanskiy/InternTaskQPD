using Models.Bridge.Auth;
using Models.Shared.User;

namespace Car.App.Services.TokenService;

public interface ITokenService
{
    /// <summary> Инвалидировать все access и refresh токены пользователя </summary>
    Task<bool> LogoutAllAsync(string userId);

    /// <summary> Инвалидировать последний refresh и текущий access </summary>
    Task<bool> LogoutCurrentAsync(string userId, string? jti, string? exp);
    
    /// <summary> Обновить пару access и refresh токенов </summary> 
    Task<TokenPairResponse> RefreshTokenAsync(string refreshToken);
    
    /// <summary> Генерирует новый набор токенов (access + refresh) для пользователя </summary>
    Task<TokenPairResponse> GenerateTokensAsync(ApplicationUser user, string? password = null);

    
    /// <summary> Есть ли access токен в чс </summary>
    public Task<bool> AccessTokenInBlackList(string jti);
}