namespace Private.StorageModels;

public class EmailConfirmationTokenEntity
{
    public int Id { get; set; }
    
    public string TokenBody { get; set; } = null!;
    
    public DateTime ExpiresAt { get; set; }
    
    public bool Confirmed { get; set; }
    
    public DateTime? ConfirmedAt { get; set; }
    
    public Guid UserId { get; set; }
    
    public ApplicationUserEntity User { get; set; } = null!;
}