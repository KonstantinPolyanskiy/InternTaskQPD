using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.Events;

namespace QPDCar.ServiceInterfaces.Publishers;

public interface INotificationPublisher
{
    Task<ApplicationExecuteResult<Unit>> NotifyAsync(EmailNotificationEvent evt);
}