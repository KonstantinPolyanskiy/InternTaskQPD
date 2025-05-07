namespace Public.Models.ErrorEnums;

public enum UserErrors
{
    LoginIsBusy,
    EmailIsBusy,
    FailSaveUser,
    PasswordIsNotValid,
    UserNotFound,
    ForbiddenRole
}

public enum EmailTokenErrors
{
    TokenConfirmationNotFound,
    AlreadyConfirmed,
    IncorrectUserOrExpired
}

public enum RefreshTokenErrors
{
    TokenNotFound,
    TokenExpired,
    TokenNotDeleted
}

public enum AccessTokenErrors
{
    TokenNotSaved,
    BanExpired,
    UnknownError
}

public enum JwtTokenErrors
{
    UserIdNotFoundInClaims,
    JtiNotFoundInClaims,
    ExpNotFoundInClaims,
}

//TODO удалить 
public enum DatabaseErrors
{
    SeeMessage,
    NotFound,
}