using Car.App.Models.TokenModels;
using Car.App.Repositories;

namespace Car.Dal.Repository.EntityFrameworkRepository;

public class PostgresRefreshTokenRepository : IRefreshTokenRepository
{
    public Task SaveRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAtUtc)
    {
        throw new NotImplementedException();
    }

    public Task<RefreshTokenResult?> GetByRefreshTokenAsync(string refreshToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteRefreshTokenAsync(string refreshToken)
    {
        throw new NotImplementedException();
    }
}