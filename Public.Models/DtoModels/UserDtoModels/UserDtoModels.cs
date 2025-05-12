using Public.Models.CommonModels;

namespace Public.Models.DtoModels.UserDtoModels;

public record DataForConsumerRegistration
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    
    public required string Login { get; init; }
    public required string Password { get; init; }
    
    public required string Email { get; init; }

    public ApplicationUserRole RequestedUserRole { get; set; } = ApplicationUserRole.Client;
}

public record DataForCreateUser
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    
    public required string Login { get; init; } 

    public required string Email { get; init; } 

    public required string Password { get; init; }

    public IReadOnlyCollection<ApplicationUserRole> InitialRoles { get; set; } = [];
}

public record DataForUpdateUser
{
    public required Guid UserId { get; init; }
    
    public required string FirstName { get; init; }
    public required string LastName { get; init; }

    public IReadOnlyCollection<ApplicationUserRole> NewRoles { get; init; } = [];
}