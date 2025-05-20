using System.Text.Json;
using Microsoft.Extensions.Logging;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.Events;
using QPDCar.ServiceInterfaces.Publishers;
using RabbitMQ.Client;

namespace QPDCar.Infrastructure.Publishers;

public class RabbitPublisher(IConnection connection, ILogger<RabbitPublisher> logger) : INotificationPublisher
{
    private const string ExchangeName = "notifications";
    public async Task<ApplicationExecuteResult<Unit>> NotifyAsync(EmailNotificationEvent evt)
    {
        await using var channel = await connection.CreateChannelAsync();
        
        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true);

        var props = new BasicProperties
        {
            ContentType = "application/json",
            ContentEncoding = "UTF-8",
            MessageId = evt.MessageId.ToString(),
            DeliveryMode = DeliveryModes.Persistent,
        };

        var body = JsonSerializer.SerializeToUtf8Bytes(evt);

        await channel.BasicPublishAsync(ExchangeName, "email", false, props, body);
        
        logger.LogInformation($"Сообщение {evt.MessageId} отправлено в rabbit");
        
        return ApplicationExecuteResult<Unit>.Success(Unit.Value);
    }
}