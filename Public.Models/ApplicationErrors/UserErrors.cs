namespace Public.Models.ApplicationErrors;

/// <summary> Ошибки, связанные с пользователями приложения и аккаунтами </summary>
public enum UserErrors
{
    LoginIsBusy,
    EmailIsBusy,
    FailSaveUser,
    FailDeleteUser,
    PasswordIsNotValid,
    UserNotFound,
    NotFoundAnyRoleForUser,
    ForbiddenRole
}