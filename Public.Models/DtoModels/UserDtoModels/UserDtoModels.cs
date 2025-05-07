using Public.Models.CommonModels;

namespace Public.Models.DtoModels.UserDtoModels;

public record DataForUserRegistration
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    
    public required string Login { get; init; }
    public required string Password { get; init; }
    
    public required string Email { get; init; }

    public ApplicationUserRole RequestedUserRole { get; set; } = ApplicationUserRole.Client;
}

public record DtoForCreateUser
{
    // Пока поля совпадают - можно просто встроить, не создавая ненужный маппинг
    public required DataForUserRegistration Data { get; init; }
}