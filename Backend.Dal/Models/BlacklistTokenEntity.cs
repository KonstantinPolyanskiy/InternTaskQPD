using Backend.App.Models.Dto;

namespace Backend.Dal.Models;

/// <summary>
/// Таблица black_list
/// </summary>
public class BlacklistTokenEntity()
{
    public BlacklistTokenEntity(BlacklistTokenDto data) : this()
    {
        Jti = data.Jti;
        ExpiresAt = data.ExpiresAt;
    }
    public int Id { get; set; }

    public string Jti { get; set; } = null!;
    
    public DateTime ExpiresAt { get; set; }
}