using System.Text.Json;
using Private.Jobs.Models;
using Private.ServicesInterfaces;
using RabbitMQ.Client;


namespace Private.Publishers.Publishers;

public class CarWithoutPhotoRabbitPublisher(IChannel channel, string exchange) : IEventSubscriber<MissingPhotoEvent>
{
    public async Task OnEventAsync(MissingPhotoEvent evt, CancellationToken ct = default)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(evt);
        var properties = new BasicProperties();
        properties.ContentType = "application/json";
        properties.DeliveryMode = DeliveryModes.Persistent;
        
        await channel.BasicPublishAsync(exchange, routingKey: string.Empty, true, properties, body);
    }
}