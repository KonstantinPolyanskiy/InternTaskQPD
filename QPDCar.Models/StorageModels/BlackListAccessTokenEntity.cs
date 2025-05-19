namespace QPDCar.Models.StorageModels;

/// <summary> Описание таблицы с заблокированными Access токенами </summary>
public class BlackListAccessTokenEntity()
{
    public int Id { get; set; }
    
    public string Jti { get; set; } = null!;
    
    public string Reason {get; set; } = null!;
    
    public DateTime ExpiresAtUtc { get; set; }
}