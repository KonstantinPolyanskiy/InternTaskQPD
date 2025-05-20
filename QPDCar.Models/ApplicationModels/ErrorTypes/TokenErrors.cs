namespace QPDCar.Models.ApplicationModels.ErrorTypes;

/// <summary> Типы ошибок связанные с Refresh токенами </summary>
public enum RefreshTokenErrors
{
    TokenNotCreated,
    TokenNotFoundOrBlocked,
    TokenExpired,
    TokenNotDeleted,
}

/// <summary> Типы ошибок связанные с Access токенами </summary>
public enum AccessTokenErrors
{
    UnknownError,
    BanExpired,
}