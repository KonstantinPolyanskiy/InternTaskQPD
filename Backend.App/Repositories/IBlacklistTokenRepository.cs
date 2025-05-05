using Backend.App.Models.Dto;

namespace Backend.App.Repositories;

public interface IBlacklistTokenRepository
{
    public Task<BlacklistTokenResultDto> AddBlackList(BlacklistTokenDto data);
    
    public Task<BlacklistTokenResultDto?> GetBlacklistToken(string token);
    public Task<BlacklistTokenResultDto?> GetBlacklistToken(int id);
    
    public Task<bool> InBlackList(string jti);
}