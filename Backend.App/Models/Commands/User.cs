using Enum.Common;

namespace Backend.App.Models.Commands;

public record LoginUserCommand
{
    public required string Login { get; init; }
    public required string Password { get; init; }
}

public record CreateUserCommand
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    
    public required string Email { get; init; }
    
    public required string Login { get; init; }
    public required string Password { get; init; }
    
    public ApplicationUserRole Role { get; set; }
}