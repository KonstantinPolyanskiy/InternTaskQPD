using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Backend.App.Models.Business;

public class ApplicationUser : IdentityUser
{
    [MaxLength(200)]
    public string? FirstName { get; init; }
    
    [MaxLength(200)]
    public string? LastName { get; init; }

    public ICollection<RefreshToken> RefreshTokens { get; }
        = new List<RefreshToken>();
}