namespace Models.Bridge.Auth;

/// <summary>
/// Настройки jwt токена
/// </summary>
public class JwtSettings
{
    public string Issuer { get; set; }                = null!;
    public string Audience { get; set; }              = null!;
    public string SecretKey { get; set; }             = null!;
    public int AccessTokenLifetimeMinutes { get; set; }
    public int RefreshTokenLifetimeDays    { get; set; }
}