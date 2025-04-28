using Car.App.Models.TokenModels;

namespace Car.App.Repositories;

public interface IRefreshTokenRepository
{
    Task SaveRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAtUtc);
    Task<RefreshTokenResult?> GetByRefreshTokenAsync(string refreshToken);
    Task DeleteRefreshTokenAsync(string refreshToken);
}