using System.Text.Json;
using QPDCar.Models.ApplicationModels.Events;
using QPDCar.ServiceInterfaces.MailServices;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace QPDCar.Notifications.Consumers;

public class RabbitEmailConsumer(IConnection conn, IMailSender mailSender) : BackgroundService
{
    private IChannel? channel; 
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        channel = await conn.CreateChannelAsync(cancellationToken: stoppingToken);
        
        const string exchange = "notifications";
        const string queue    = "email-notify";
        await channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
        
        await channel.ExchangeDeclareAsync("notifications.dlx", ExchangeType.Fanout, durable: true, cancellationToken: stoppingToken);
        await channel.QueueDeclareAsync("email-notify.dlq", durable: true,
            exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(queue, durable: true,
            exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = "notifications.dlx"
            }, cancellationToken: stoppingToken);
        await channel.QueueBindAsync(queue, exchange, routingKey: "email", cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += OnMessageAsync;

        await channel.BasicConsumeAsync(queue, autoAck: false, consumer, cancellationToken: stoppingToken);
        await Task.CompletedTask;
    }
    
    async Task OnMessageAsync(object sender, BasicDeliverEventArgs ea)
    {
        EmailNotificationEvent? evt = null;
        try
        {
            evt = JsonSerializer.Deserialize<EmailNotificationEvent>(ea.Body.Span);
            if (evt is null) throw new InvalidDataException("Empty event");

            await mailSender.SendAsync(evt.To, evt.Subject, evt.BodyHtml);

            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }
}
