using Public.Models.CommonModels;

namespace Public.UseCase.Models;

public record UserRegistrationResponse
{
    public required string Login { get; init; }
    public string? Email { get; init; }
}

public record UserEmailConfirmationResponse
{
    public required string Login { get; init; }
    public required string Email { get; init; }

    public string Message { get; set; } = string.Empty;
}



