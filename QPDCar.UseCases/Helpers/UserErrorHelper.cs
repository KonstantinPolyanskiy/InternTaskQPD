using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ErrorTypes;

namespace QPDCar.UseCases.Helpers;

internal static class UserErrorHelper
{
    internal static ApplicationError ErrorUserNotFound(string id)
        => new(
            UserErrors.UserNotFound, "Пользователь не найден",
            $"Пользователь по признаку {id} не найден",
            ErrorSeverity.NotImportant);
}