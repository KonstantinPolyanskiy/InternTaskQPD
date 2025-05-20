using QPDCar.Models.BusinessModels.EmployerModels;

namespace QPDCar.Api.Models.Requests;

public record ClientLoginRequest
{
    public required string Login { get; init; }
    public required string Password { get; init; }
}

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

    public IReadOnlyCollection<ApplicationRoles> NewRoles { get; init; } = [];
}
