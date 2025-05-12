namespace Private.StorageModels;

/// <summary> Описание таблицы refresh_tokens </summary>
public class RefreshTokenEntity()
{
    public int Id { get; set; }

    public string UserId { get; set; }
    
    public string RefreshTokenBody { get; set; } = null!;
    
    public string Jti { get; set; }  = null!;
    
    public DateTime ExpiresAtUtc { get; set; }
    
    public ApplicationUserEntity ApplicationUser { get; set; } = null!;
}