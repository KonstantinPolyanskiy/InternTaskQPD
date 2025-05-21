using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ErrorTypes;

namespace QPDCar.Services.ErrorHelpers;

internal static class UserErrorHelper
{
    internal static ApplicationError ErrorIncorrectLoginOrPasswordWarning()
        => new(
            UserErrors.InvalidLoginOrPassword, "Не верный логин или пароль",
            "Переданный логин или пароль не верны",
            ErrorSeverity.NotImportant);
    
    internal static ApplicationError ErrorUserNotFoundWarning(string id)
    => new (
        UserErrors.UserNotFound, "Пользователь не найден",
        $"Не получилось найти пользователя по переданному признаку - {id}",
        ErrorSeverity.NotImportant);
    
}