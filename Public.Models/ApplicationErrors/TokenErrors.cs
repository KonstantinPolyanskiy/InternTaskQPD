namespace Public.Models.ApplicationErrors;

/// <summary> Ошибки, связанные с токенами подтверждения почты </summary>
public enum EmailTokenErrors
{
    TokenNotCreated,
    TokenNotFound,
    AlreadyConfirmed,
    TokenNotUpdated,
    TokenExpired,
    IncorrectUserOrExpired
}

/// <summary> Ошибки, связанные с токенами refresh </summary>
public enum RefreshTokenErrors
{
    TokenNotFound,
    TokenNotFoundOrBlocked,
    TokenNotCreated,
    TokenExpired,
    TokenNotDeleted
}

/// <summary> Ошибки, связанные с токенами access </summary>
public enum AccessTokenErrors
{
    TokensNotRevokedBecauseUserIsNull,
    TokensNotRevokedBecauseUnknownError,
    TokenNotRevokedBecauseJtiIsNull,
    TokenNotRevokedUnknownError,
    TokenNotFound,
    BanExpired,
    UnknownError
}


/// <summary> Ошибки, связанные с токенами jwt </summary>
public enum JwtTokenErrors
{
    UserIdNotFoundInClaims,
    JtiNotFoundInClaims,
    ExpNotFoundInClaims,
}
