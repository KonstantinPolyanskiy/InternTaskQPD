namespace Car.App.Models.TokenModels;

public record RefreshTokenResult
{
    public int Id { get; set; }
    public string UserId { get; set; }
    
    public string Token { get; set; }
    
    public string Jti { get; set; }
    
    public DateTime ExpiresAtUtc { get; set; } 
}

public record RefreshTokenResponse
{
    public required string UserId { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}