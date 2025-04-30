using Car.App.Models.TokenModels;

namespace Car.App.Repositories;

public interface IRefreshTokenRepository
{
    Task SaveRefreshTokenAsync(string userId, string refreshToken, string jti, DateTime expiresAtUtc);
    
    Task<RefreshTokenResult?> GetByRefreshTokenAsync(string refreshToken);
    Task<RefreshTokenResult?> GetLastRefreshTokenByUserIdAsync(string userId);
    Task<RefreshTokenResult?> GetRefreshTokenByJtiAsync(string jti);
    
    Task DeleteRefreshTokenByTokenAsync(string refreshToken);
    Task DeleteRefreshTokenByUserIdAsync(string userId);
    
    Task DeleteAllRefreshTokensAsync(string userId);
}