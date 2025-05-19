using QPDCar.Models.BusinessModels.EmployerModels;

namespace QPDCar.Models.DtoModels.UserDtos;

public record DtoForCreateUser
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    
    public required string Login { get; init; } 

    public required string Email { get; init; } 

    public required string Password { get; init; }

    public IReadOnlyCollection<ApplicationRoles> InitialRoles { get; set; } = [];
}

public record DtoForCreateConsumer
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    
    public required string Login { get; init; }
    public required string Password { get; init; }
    
    public required string Email { get; init; }
}