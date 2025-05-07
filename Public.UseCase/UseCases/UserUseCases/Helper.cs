using Public.Models.CommonModels;
using Public.Models.ErrorEnums;

namespace Public.UseCase.UseCases.UserUseCases;

internal static class Helper
{
    internal static ApplicationError ForbiddenRoleApplicationError()
    {
        return new ApplicationError(UserErrors.ForbiddenRole, "Недопустимая роль",
            $"Попытка регистрации с ролью отличной от {ApplicationUserRole.Client.ToString()}", ErrorSeverity.Critical,
            503);
    }
}