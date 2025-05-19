namespace QPDCar.Models.ApplicationModels.ErrorTypes;

public enum UserErrors
{
    LoginBusy,
    EmailBusy,
    
    UserNotSaved,
    UserNotFound,
    
    NotFoundAnyRole,
    UserRolesNotUpdated,
    
    LoginClaimNotFound,
    DontEnoughPermissions,
}