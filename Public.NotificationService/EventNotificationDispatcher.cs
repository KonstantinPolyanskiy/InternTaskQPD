using Private.ServicesInterfaces;

namespace Public.NotificationService;

public class NotificationDispatcher(IEnumerable<INotifier> notifiers)
{
    public async Task DispatchAsync()
}