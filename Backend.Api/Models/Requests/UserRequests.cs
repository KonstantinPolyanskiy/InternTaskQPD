namespace Backend.Api.Models.Requests;


public record UserRegistrationRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    
    public required string Email { get; init; }
    
    public required string Login { get; init; }
    public required string Password { get; init; }
}

public record LoginRequest
{
    public required string Login { get; init; }
    public required string Password { get; init; }
}

public class RefreshTokenPairRequest
{
    public required string RefreshToken { get; set; }
}