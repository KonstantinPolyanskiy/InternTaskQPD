namespace Private.StorageModels;

/// <summary> Описание таблицы refresh_tokens </summary>
public class RefreshTokenEntity()
{
    public int Id { get; set; }

    public required string RefreshBody { get; set; } = null!;
    
    public required string AccessJti { get; set; }  = null!;
    
    public DateTime ExpiresAtUtc { get; set; }
    public required string UserId { get; set; }
    public ApplicationUserEntity ApplicationUser { get; set; } = null!;
}