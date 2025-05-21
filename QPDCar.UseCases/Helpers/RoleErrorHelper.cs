using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ErrorTypes;
using QPDCar.Models.BusinessModels.CarModels;

namespace QPDCar.UseCases.Helpers;

internal static class RoleErrorHelper
{
    internal static ApplicationError ErrorUnknownRoleWarning(string login)
        => new(
            UserErrors.NotFoundAnyRole, "Ошибка с получением ролей",
            $"Для пользователя {login} возникла проблема с получением ролей",
            ErrorSeverity.NotImportant);
    
    internal static ApplicationError ErrorDontEnoughPermissionWarning(string action, string objectId)
    => new (
        UserErrors.DontEnoughPermissions, "Не достаточно прав",
        $"Недостаточно прав для совершения действия {action} у пользователя над объектом {objectId}",
        ErrorSeverity.NotImportant);
}