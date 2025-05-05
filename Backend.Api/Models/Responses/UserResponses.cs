namespace Backend.Api.Models.Responses;

public record CreateUserResponse
{
    public required string Login { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
}

public record TokenPairResponse
{
    public required string RefreshToken { get; init; }
    public required string AccessToken { get; init; }
}

public record LogoutResponse
{
    public bool Success { get; init; }
    public bool LogoutAll { get; init; }
}