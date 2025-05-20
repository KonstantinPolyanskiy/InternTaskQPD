namespace QPDCar.Models.ApplicationModels.AuthModels;

/// <summary> Пара токенов авторизации (Refresh, Access) </summary>
public record AuthTokensPair
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}
