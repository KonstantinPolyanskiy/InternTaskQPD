using Microsoft.AspNetCore.Identity;

namespace Public.Models.UserModels;

public class ApplicationUser : IdentityUser
{
    public required string FirstName { get; set; }
    public string? LastName { get; set; }

    public ICollection<int> RefreshTokenIds { get; } = new List<int>();
}