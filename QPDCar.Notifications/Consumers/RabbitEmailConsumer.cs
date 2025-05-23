using System.Text.Json;
using QPDCar.Models.ApplicationModels.Events;
using QPDCar.Models.ApplicationModels.Settings;
using QPDCar.ServiceInterfaces.MailServices;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace QPDCar.Notifications.Consumers;

public class RabbitEmailConsumer(IConnection conn, IMailSender mailSender) : BackgroundService
{
    private IChannel? _channel; 
    
    /// <summary>
    /// Топик оповещений
    /// </summary>
    private const string ExchangeName = "notifications";
    
    /// <summary>
    /// Routing-key email сообщений
    /// </summary>
    private const string RoutingKeyName = "email";
    
    /// <summary>
    /// Очередь email сообщений
    /// </summary>
    private const string QueueName = "email-notify";
    
    /// <summary>
    /// Очередь неотправленных email сообщений
    /// </summary>
    private const string DeadQueueName = QueueName + "-death";
    
    private const string DeadExchangeName = ExchangeName + "-death";
    
    private const string RetryExchangeName = DeadExchangeName + "-retry";
    
    private const string DeadLetterHeader = "x-dead-letter-exchange";

    private RabbitRetryPolicy? RetryPolicy { get; set; }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await conn.CreateChannelAsync(cancellationToken: stoppingToken);

        var args = new Dictionary<string, object?>();
        
        if (RetryPolicy is not null)
            args.Add(DeadLetterHeader, DeadExchangeName);
        
        await _channel.ExchangeDeclareAsync(
            exchange: ExchangeName, 
            type: ExchangeType.Topic,
            durable: true, 
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: QueueName + ".dlq",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);
        
        await _channel.QueueBindAsync(
            queue: QueueName, 
            exchange: ExchangeName,
            routingKey: RoutingKeyName,
            arguments: args,
            cancellationToken: stoppingToken);

        if (RetryPolicy is not null)
        {
            await _channel.ExchangeDeclareAsync(DeadExchangeName + ".dlx", 
                type: ExchangeType.Direct, 
                durable: true,
                cancellationToken: stoppingToken);
            
            await _channel.ExchangeDeclareAsync(RetryExchangeName + ".dlx", 
                type: ExchangeType.Direct, 
                durable: true,
                cancellationToken: stoppingToken);
            
            await _channel.QueueDeclareAsync(DeadQueueName,
                durable: true,
                autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    {"x-message-ttl", RetryPolicy.Timeout},
                    {DeadLetterHeader, RetryPolicy.Timeout},
                },
                cancellationToken: stoppingToken);
            
            await _channel.QueueBindAsync(DeadQueueName,
                exchange: DeadExchangeName,
                routingKey: RoutingKeyName,
                cancellationToken: stoppingToken);
            
            await _channel.QueueBindAsync(QueueName,
                RetryExchangeName,
                RoutingKeyName,
                cancellationToken: stoppingToken);
        }
        
        
        var consumer = new AsyncEventingBasicConsumer(_channel);
        
        consumer.ReceivedAsync += OnMessageAsync;

        await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer, cancellationToken: stoppingToken);
        
        await _channel.CloseAsync(stoppingToken);
        
        await Task.CompletedTask;
    }
    
    async Task OnMessageAsync(object sender, BasicDeliverEventArgs ea)
    {
        EmailNotificationEvent? evt;
        try
        {
            evt = JsonSerializer.Deserialize<EmailNotificationEvent>(ea.Body.Span);
            if (evt is null) throw new InvalidDataException("Empty event");

            await mailSender.SendAsync(evt.To, evt.Subject, evt.BodyHtml);

            if (_channel != null) await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            if (_channel != null) await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }
}
