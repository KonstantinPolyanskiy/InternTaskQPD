using Backend.App.Models.Dto;

namespace Backend.App.Repositories;

public interface IRefreshTokenRepository
{
    Task SaveRefreshTokenAsync(RefreshTokenDto refreshDto);
    
    Task<RefreshTokenDto?> GetByRefreshTokenAsync(string refreshToken);
    Task<RefreshTokenDto?> GetLastRefreshTokenByUserIdAsync(string userId);
    Task<RefreshTokenDto?> GetRefreshTokenByJtiAsync(string jti);
    
    Task DeleteRefreshTokenByTokenAsync(string refreshToken);
    Task DeleteRefreshTokenByUserIdAsync(string userId);
    
    Task DeleteAllRefreshTokensAsync(string userId);
}