using Public.Models.CommonModels;
using Public.Models.NotificationModels;

namespace Private.ServicesInterfaces;

public interface INotifier
{
    public bool CanHandle(string channel);
    public Task<ApplicationExecuteLogicResult<Unit>> NotifyAsync(EmailNotification notification);
}