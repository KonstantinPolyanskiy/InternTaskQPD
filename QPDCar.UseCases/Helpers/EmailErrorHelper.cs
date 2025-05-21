using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ErrorTypes;

namespace QPDCar.UseCases.Helpers;

internal static class EmailErrorHelper
{
    internal static ApplicationError ErrorMailNotSendWarn(string email, string action)
        => new(
            EmailErrors.MailNotSend, "Email не отправлено",
            $"Письмо на почту {email} о {action} не отправлено",
            ErrorSeverity.NotImportant);
}