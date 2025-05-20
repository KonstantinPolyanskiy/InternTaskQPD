namespace QPDCar.Models.ApplicationModels.ErrorTypes;

/// <summary> Типы ошибок связанные с пользователями приложения и их аккаунтами </summary>
public enum UserErrors
{
    LoginBusy,
    EmailBusy,

    InvalidLoginOrPassword,
    
    UserNotSaved,
    UserNotFound,
    UserNotUpdated,
    
    NotFoundAnyRole,
    UserRolesNotUpdated,
    
    LoginClaimNotFound,
    DontEnoughPermissions,
}