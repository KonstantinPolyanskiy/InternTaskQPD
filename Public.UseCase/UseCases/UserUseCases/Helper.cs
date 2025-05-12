using System.Globalization;
using System.Net;
using Private.StorageModels;
using Public.Models.ApplicationErrors;
using Public.Models.CommonModels;

namespace Public.UseCase.UseCases.UserUseCases;

internal static class Helper
{
    internal static ApplicationError ForbiddenRoleApplicationError() => 
        new(UserErrors.ForbiddenRole, "Недопустимая роль",
                  $"Попытка регистрации с ролью отличной от {ApplicationUserRole.Client.ToString()}", 
                  ErrorSeverity.Critical, HttpStatusCode.Forbidden);

    internal static ApplicationExecuteLogicResult<DateTime> StringExpirationToDateTime(string rawExp)
    {
        if (!long.TryParse(rawExp, NumberStyles.Integer, CultureInfo.InvariantCulture, out var raw))
            return ApplicationExecuteLogicResult<DateTime>.Failure(new ApplicationError(
                ParsingErrors.FailStringToLongError,
                "Не удалось спарсить string в long", "Попытка сконвертировать string в long неудачна", ErrorSeverity.NotImportant));
        
        try
        {
            return ApplicationExecuteLogicResult<DateTime>.Success(DateTimeOffset.FromUnixTimeSeconds(raw).DateTime);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return ApplicationExecuteLogicResult<DateTime>.Failure(new ApplicationError(
                ParsingErrors.FailLongToDateTimeError,
                "Не удалось спарсить long в date time", ex.Message, ErrorSeverity.NotImportant));
        }
    }

    internal static string ThanksForConfirmingEmailMessage(ApplicationUserEntity user) => 
        $"Уважаемый {string.Join(" ", user.FirstName, user.LastName)} спасибо за подтверждение почты!";
    
    internal static string AccountLoginEmailMessage(ApplicationUserEntity user) => 
        $"Уважаемый {string.Join(" ", user.FirstName, user.LastName)}, в ваш аккаунт {user.Email} был совершен вход сегодня в {DateTime.Now}";
}