using Car.App.Models.TokenModels;
using Car.App.Repositories;
using Microsoft.EntityFrameworkCore;
using Models.Shared.User;

namespace Car.Dal.Repository.PostgresRepository;

public class PostgresRefreshTokenRepository(AuthDbContext dbContext) : IRefreshTokenRepository
{
    public async Task SaveRefreshTokenAsync(string userId, string refreshToken, string jti, DateTime expiresAtUtc)
    {
        var entity = new RefreshToken
        {
            UserId = userId,
            Token = refreshToken,
            Jti = jti,
            ExpiresAtUtc = expiresAtUtc
        };

        dbContext.Add(entity);
        await dbContext.SaveChangesAsync();
    }
   
    
    public async Task<RefreshTokenResult?> GetByRefreshTokenAsync(string refreshToken)
    {
        var entity = await dbContext.RefreshTokens.SingleOrDefaultAsync(x => x.Token == refreshToken);
        if (entity is null)
            return null;

        return new RefreshTokenResult
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Token = entity.Token,
            Jti = entity.Jti,
            ExpiresAtUtc = entity.ExpiresAtUtc
        };
    }
    public async Task<RefreshTokenResult?> GetLastRefreshTokenByUserIdAsync(string userId)
    {
        var entity = await dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .OrderByDescending(rt => rt.Id)            
            .FirstOrDefaultAsync();
        if (entity is null)
            return null;
        
        return new RefreshTokenResult
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Token = entity.Token,
            Jti = entity.Jti,
            ExpiresAtUtc = entity.ExpiresAtUtc
        };
    }
    public async Task<RefreshTokenResult?> GetRefreshTokenByJtiAsync(string jti)
    {
        var entity = await dbContext.RefreshTokens.SingleOrDefaultAsync(x => x.Jti == jti);
        if (entity is null)
            return null;

        return new RefreshTokenResult
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Token = entity.Token,
            Jti = entity.Jti,
            ExpiresAtUtc = entity.ExpiresAtUtc
        };
    }


    public async Task DeleteRefreshTokenByTokenAsync(string refreshToken)
    {
        var entity = await dbContext.RefreshTokens.SingleOrDefaultAsync(x => x.Token == refreshToken);
        if (entity is not null)
        {
            dbContext.Remove(entity);
            await dbContext.SaveChangesAsync();
        }
    }
    public async Task DeleteRefreshTokenByUserIdAsync(string userId)
    {
        var entity = await dbContext.RefreshTokens.SingleOrDefaultAsync(x => x.UserId == userId);
        if (entity is not null)
        {
            dbContext.Remove(entity);
            await dbContext.SaveChangesAsync();
        }
    }
    public async Task DeleteAllRefreshTokensAsync(string userId)
    {
        var entities = await dbContext.RefreshTokens.Where(x => x.UserId == userId).ToListAsync();
        if (entities.Count != 0)
        {
            dbContext.RemoveRange(entities);
            await dbContext.SaveChangesAsync();
        }
    }
}