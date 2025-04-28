using Microsoft.AspNetCore.Identity;
using Models.Bridge.Auth;

namespace Car.App.Services.TokenService;

public interface ITokenService 
{
    /// <summary>
    /// Генерирует новый набор токенов (access + refresh) для пользователя
    /// </summary>
    Task<TokenResponse> GenerateTokensAsync(IdentityUser user, string? password = null);

    /// <summary>
    /// Валидирует и обновляет access по refresh-токену
    /// </summary>
    Task<TokenResponse> RefreshAsync(string refreshToken);

    /// <summary>
    /// Отозвать (удалить) refresh-токен
    /// </summary>
    Task RevokeRefreshTokenAsync(string refreshToken);
}