using Public.Models.CommonModels;

namespace Public.Api.Models.Requests;

public record ClientRegistrationRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    
    public required string Email { get; init; }
    
    public required string Login { get; init; }
    public required string Password { get; init; }
}

public record UpdateUserRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }

    public IReadOnlyCollection<ApplicationUserRole> NewRoles { get; init; } = [];
}

public record ClientLoginRequest
{
    public required string Login { get; init; }
    public required string Password { get; init; }
}