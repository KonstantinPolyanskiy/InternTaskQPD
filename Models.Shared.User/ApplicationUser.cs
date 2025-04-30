using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Models.Shared.User;

/// <summary>
/// Таблица ApplicationUser
/// </summary>
public class ApplicationUser : IdentityUser
{
    [MaxLength(200)]
    public string? FirstName { get; init; }
    
    [MaxLength(200)]
    public string? LastName { get; init; }

    public ICollection<RefreshToken> RefreshTokens { get; }
        = new List<RefreshToken>();
}

/// <summary>
/// Таблица RefreshToken 
/// </summary>
public class RefreshToken
{
    public int      Id           { get; set; }
    public string   UserId       { get; set; }      = null!;
    public string   Token        { get; set; }      = null!;
    
    public string Jti            { get; set; }      = null!;
    
    public DateTime ExpiresAtUtc { get; set; }

    public ApplicationUser? User    { get; set; }
}