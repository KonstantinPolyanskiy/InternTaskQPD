namespace QPDCar.Models.ApplicationModels.ErrorTypes;

public enum RefreshTokenErrors
{
    TokenNotCreated,
    TokenNotFoundOrBlocked,
    TokenExpired,
    TokenNotDeleted,
}

public enum AccessTokenErrors
{
    UnknownError,
    BanExpired,
}