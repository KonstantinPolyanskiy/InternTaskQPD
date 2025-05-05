using Backend.App.Models.Business;

namespace Backend.App.Models.Commands;

public record LogoutCommand
{
    public required string UserId { get; set; }
    
    public string? Jti { get; set; }
    public string? RawExpiration { get; set; }
    
    public bool LogoutAll { get; set; } = false;
}

public record RefreshTokenPairCommand
{
    public required string RefreshToken { get; init; }
}

public record GenerateTokenPairCommand
{
    public required ApplicationUser User { get; init; }
    public string? Password { get; init; }
}