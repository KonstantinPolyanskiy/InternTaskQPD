using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;

namespace Models.Bridge.Auth;

/// <summary>
/// Запрос на регистрацию пользователя
/// </summary>
public class UserRegistrationRequest
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    
    public required string Email { get; set; }
    
    public required string Login { get; set; }
    public required string Password { get; set; }
}

/// <summary>
/// Ответ сервиса на регистрацию пользователя
/// </summary>
public class UserRegistrationServiceResponse
{
    public required string Id { get; set; }
    public required string Login { get; set; }
    public required string Email { get; set; }
}

/// <summary>
/// Запрос на авторизацию пользователя
/// </summary>
public class UserLoginRequest
{
    public required string Login { get; set; }
    public required string Password { get; set; }
}