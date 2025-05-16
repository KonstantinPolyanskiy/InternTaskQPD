namespace Private.StorageModels;

public class EmailConfirmationTokenEntity
{
    public int Id { get; set; }
    
    public required string TokenBody { get; set; }
    
    public DateTime ExpiresAt { get; set; }
    
    public bool Confirmed { get; set; }
    
    public DateTime? ConfirmedAt { get; set; }
    
    public required string UserId { get; set; }
    
    public ApplicationUserEntity User { get; set; } = null!;
}