using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;

namespace QPDCar.ServiceInterfaces.MailServices;

/// <summary> Сервис для отправки почты </summary>
public interface IMailSender
{
    /// <summary> Отправляет email по указанной почте </summary>
    Task<ApplicationExecuteResult<Unit>> SendAsync(string to, string subject, string body);
}