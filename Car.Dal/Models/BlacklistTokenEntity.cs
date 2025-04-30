using Car.App.Models.Dto;

namespace Car.Dal.Models;

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
    
    public string Jti { get; set; }
    
    public DateTime ExpiresAt { get; set; }
}