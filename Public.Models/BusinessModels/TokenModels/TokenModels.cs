namespace Public.Models.BusinessModels.TokenModels;

public record ConfirmEmailToken
{
    public int Id { get; init; }
    public Guid UserId { get; init; } 
    public string Token { get; set; } = null!;
}

public record AuthTokensPair
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}
