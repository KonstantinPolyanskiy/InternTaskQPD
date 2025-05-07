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

public record DataForUserRegistration
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    
    public required string Login { get; init; }
    public required string Password { get; init; }

    public ApplicationUserRole RequestedUserRole { get; set; } = ApplicationUserRole.Client;
}

public record DataForCreateUser
{
    // Пока поля совпадают - можно просто встроить, не создавая ненужный маппинг
    public required DataForUserRegistration Data { get; init; }
}