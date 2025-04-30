using System.Text.Json.Serialization;

namespace CarService.Models;

/// <summary>
/// Ответ на регистрацию пользователя
/// </summary>
public class RegistrationResponse
{
    public string? Login { get; set; }
    public string? Email { get; set; }
}

public class LoginRequest
{
    public required string Login { get; set; }
    public required string Password { get; set; }
}

public class RefreshRequest
{
    public required string RefreshToken { get; set; }
}