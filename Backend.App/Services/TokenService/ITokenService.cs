using Backend.App.Models.Business;
using Backend.App.Models.Commands;

namespace Backend.App.Services.TokenService;

public interface ITokenService
{
    Task<bool> LogoutAsync(LogoutCommand cmd);
    
    /// <summary> Обновить пару access и refresh токенов </summary> 
    Task<TokenPair> RefreshTokenAsync(RefreshTokenPairCommand cmd);
    
    /// <summary> Генерирует новый набор токенов (access + refresh) для пользователя </summary>
    Task<TokenPair> GenerateTokensAsync(GenerateTokenPairCommand cmd);

    
    /// <summary> Есть ли access токен в чс </summary>
    public Task<bool> AccessTokenInBlackList(string jti);
}