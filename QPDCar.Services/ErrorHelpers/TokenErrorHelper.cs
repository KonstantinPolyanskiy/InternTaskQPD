using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ErrorTypes;

namespace QPDCar.Services.ErrorHelpers;

internal static class TokenErrorHelper
{
    internal static ApplicationError ErrorRefreshTokenExpiredWarning(string token)
        => new(
            RefreshTokenErrors.TokenExpired, "Refresh истек",
            $"Refresh токен {token} истек",
            ErrorSeverity.NotImportant);
    
    internal static ApplicationError ErrorRefreshTokenNotDeletedWarning(string token)
     => new (
         RefreshTokenErrors.TokenNotDeleted, "Refresh не удален",
         $"Refresh токен {token} не удален",
         ErrorSeverity.NotImportant);
}