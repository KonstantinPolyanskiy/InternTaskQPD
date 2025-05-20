namespace QPDCar.Models.StorageModels;

/// <summary> Описание таблицы c Refresh токенами </summary>
public class RefreshTokenEntity()
{
    public int Id { get; set; }

    public required string RefreshBody { get; set; }
    
    public required string AccessJti { get; set; }
    
    public DateTime ExpiresAtUtc { get; set; }
    public required string UserId { get; set; }
    public ApplicationUserEntity ApplicationUser { get; set; } = null!;
}