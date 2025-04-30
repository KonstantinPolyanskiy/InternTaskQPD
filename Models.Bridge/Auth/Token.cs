namespace Models.Bridge.Auth;

/// <summary>
/// Ответ на авторизацию - пара токенов
/// </summary>
public class TokenPairResponse
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}