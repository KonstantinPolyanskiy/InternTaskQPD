using Microsoft.AspNetCore.Identity;

namespace Private.StorageModels;

public class ApplicationUserEntity : IdentityUser
{
    public required string FirstName { get; set; }
    public string? LastName { get; set; }

    public  ICollection<RefreshTokenEntity> RefreshTokens { get; }
    = new List<RefreshTokenEntity>();
    
    public EmailConfirmationTokenEntity? EmailConfirmationToken { get; set; }
}