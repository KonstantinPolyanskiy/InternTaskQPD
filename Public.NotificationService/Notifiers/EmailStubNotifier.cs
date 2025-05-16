using Private.ServicesInterfaces;
using Public.Models.CommonModels;
using Public.Models.NotificationModels;

namespace Public.NotificationService.Notifiers;

public class EmailStubNotifier(ILogger<EmailStubNotifier> logger) : INotifier
{
    public bool CanHandle(string channel)
    {
        throw new NotImplementedException();
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> NotifyAsync(EmailNotification notification)
    {
        logger.LogInformation("На почту {to} с темой {subject} отправлено сообщение {body}", 
            notification.To, notification.Subject, notification.PlainBody);
        
        return await Task.FromResult(ApplicationExecuteLogicResult<Unit>.Success(Unit.Value));
    }
}