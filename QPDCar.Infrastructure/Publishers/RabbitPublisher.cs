using System.Text.Json;
using Microsoft.Extensions.Logging;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.Events;
using QPDCar.ServiceInterfaces.Publishers;
using RabbitMQ.Client;

namespace QPDCar.Infrastructure.Publishers;

/// <summary> Публикует Email нотификации в очередь Rabbit </summary>
public class RabbitPublisher(IConnection connection, ILogger<RabbitPublisher> logger) : INotificationPublisher
{
    private const string RoutingKeyName = "email";
    private const string ExchangeName = "notifications";
    
    public async Task<ApplicationExecuteResult<Unit>> NotifyAsync(EmailNotificationEvent evt)
    {
        await using var channel = await connection.CreateChannelAsync();

        await channel.BasicPublishAsync(ExchangeName, RoutingKeyName, false,
            CreateBasicPropertiesEmailEvent(), 
            JsonSerializer.SerializeToUtf8Bytes(evt));
        
        await channel.CloseAsync();
        
        logger.LogInformation($"Сообщение {evt.MessageId} отправлено в rabbit");
        
        return ApplicationExecuteResult<Unit>.Success(Unit.Value);
    }

    private BasicProperties CreateBasicPropertiesEmailEvent()
        => new()
        {
            ContentType = "application/json",
            ContentEncoding = "UTF-8",
            MessageId = Guid.NewGuid().ToString(),
            DeliveryMode = DeliveryModes.Persistent,
        };
}