namespace QPDCar.Models.ApplicationModels.AuthModels;

public record AuthTokensPair
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}
