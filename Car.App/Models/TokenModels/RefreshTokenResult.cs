namespace Car.App.Models.TokenModels;

public class RefreshTokenResult
{
    public required string UserId { get; set; }
    public required string RefreshToken { get; set; }
    public required DateTime ExpiresAtUtc { get; set; } 
}