using Car.App.Models.Dto;
using Car.App.Repositories;
using Car.Dal.Models;
using Microsoft.EntityFrameworkCore;

namespace Car.Dal.Repository.PostgresRepository;

public class PostgresBlacklistTokenRepository(AuthDbContext dbContext) : IBlacklistTokenRepository
{
    public async Task<BlacklistTokenResultDto> AddBlackList(BlacklistTokenDto data)
    {
        var entity = new BlacklistTokenEntity(data);
        
        await dbContext.BlacklistTokens.AddAsync(entity);
        await dbContext.SaveChangesAsync();

        return new BlacklistTokenResultDto
        {
            Id = entity.Id,
            Jti = entity.Jti,
            ExpiresAt = entity.ExpiresAt
        };
    }

    public async Task<BlacklistTokenResultDto?> GetBlacklistToken(string token)
    {
        var entity = await dbContext.BlacklistTokens.FirstOrDefaultAsync(x => x.Jti == token);
        if (entity is null)
            return null;

        return new BlacklistTokenResultDto
        {
            Id = entity.Id,
            Jti = entity.Jti,
            ExpiresAt = entity.ExpiresAt
        };
    }

    public async Task<BlacklistTokenResultDto?> GetBlacklistToken(int id)
    {
        var entity = await dbContext.BlacklistTokens.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
            return null;

        return new BlacklistTokenResultDto
        {
            Id = entity.Id,
            Jti = entity.Jti,
            ExpiresAt = entity.ExpiresAt
        };
    }

    public async Task<bool> InBlackList(string jti)
    {
        var entity = await GetBlacklistToken(jti);
        return entity is not null;
    }
}