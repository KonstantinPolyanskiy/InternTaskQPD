namespace QPDCar.Models.ApplicationModels.ErrorTypes;

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