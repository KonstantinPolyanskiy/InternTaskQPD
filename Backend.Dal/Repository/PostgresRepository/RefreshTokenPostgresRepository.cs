using Backend.App.Models.Business;
using Backend.App.Models.Dto;
using Backend.App.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Dal.Repository.PostgresRepository;

public class PostgresRefreshTokenRepository(AuthDbContext dbContext) : IRefreshTokenRepository
{
    public async Task SaveRefreshTokenAsync(RefreshTokenDto dto)
    {
        var entity = new RefreshToken
        {
            UserId = dto.UserId!,
            Token = dto.RefreshToken!,
            Jti = dto.Jti!,
            ExpiresAtUtc = dto.ExpiresUtc!.Value,
        };

        dbContext.Add(entity);
        await dbContext.SaveChangesAsync();
    }
   
    
    public async Task<RefreshTokenDto?> GetByRefreshTokenAsync(string refreshToken)
    {
        var entity = await dbContext.RefreshTokens.SingleOrDefaultAsync(x => x.Token == refreshToken);
        if (entity is null)
            return null;

        return new RefreshTokenDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RefreshToken = entity.Token,
            Jti = entity.Jti,
            ExpiresUtc = entity.ExpiresAtUtc
        };
    }
    public async Task<RefreshTokenDto?> GetLastRefreshTokenByUserIdAsync(string userId)
    {
        var entity = await dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .OrderByDescending(rt => rt.Id)            
            .FirstOrDefaultAsync();

        if (entity is null)
            return null;
        
        return new RefreshTokenDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RefreshToken = entity.Token,
            Jti = entity.Jti,
            ExpiresUtc = entity.ExpiresAtUtc
        };
    }
    public async Task<RefreshTokenDto?> GetRefreshTokenByJtiAsync(string jti)
    {
        var entity = await dbContext.RefreshTokens.SingleOrDefaultAsync(x => x.Jti == jti);
        if (entity is null)
            return null;

        return new RefreshTokenDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RefreshToken = entity.Token,
            Jti = entity.Jti,
            ExpiresUtc = entity.ExpiresAtUtc
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