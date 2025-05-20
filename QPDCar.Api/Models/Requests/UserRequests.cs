using QPDCar.Models.BusinessModels.EmployerModels;

namespace QPDCar.Api.Models.Requests;

/// <summary> Запрос на вход в аккаунт </summary>
public record ClientLoginRequest
{
    public required string Login { get; init; }
    public required string Password { get; init; }
}

/// <summary> Запрос на регистрацию клиента </summary>
public record ClientRegistrationRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    
    public required string Email { get; init; }
    
    public required string Login { get; init; }
    public required string Password { get; init; }
}

/// <summary> Запрос на обновление пользователя приложения </summary>
public record UpdateUserRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }

    public IReadOnlyCollection<ApplicationRoles> NewRoles { get; init; } = [];
}
