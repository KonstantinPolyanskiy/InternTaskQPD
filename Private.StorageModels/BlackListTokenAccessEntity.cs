namespace Private.StorageModels;

/// <summary> Описание таблицы token_black_list </summary>
public class BlackListTokenAccessEntity()
{
    public int Id { get; set; }
    
    public string Jti { get; set; } = null!;
    
    public string Reason {get; set; } = null!;
    
    public DateTime ExpiresAtUtc { get; set; }
}