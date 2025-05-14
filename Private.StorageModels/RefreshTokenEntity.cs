namespace Private.StorageModels;

/// <summary> Описание таблицы refresh_tokens </summary>
public class RefreshTokenEntity()
{
    public int Id { get; set; }

    public string RefreshBody { get; set; } = null!;
    
    public string AccessJti { get; set; }  = null!;
    
    public DateTime ExpiresAtUtc { get; set; }
    public string UserId { get; set; }
    public ApplicationUserEntity ApplicationUser { get; set; } = null!;
}