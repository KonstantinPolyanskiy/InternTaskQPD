using System.Text.Json;
using QPDCar.Models.ApplicationModels.Events;
using QPDCar.Models.ApplicationModels.Settings;
using QPDCar.ServiceInterfaces.MailServices;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace QPDCar.Notifications.Consumers;

public class RabbitEmailConsumer(IConnection conn, IMailSender mailSender, ILogger<RabbitEmailConsumer> logger) : BackgroundService
{
    private IChannel? _channel;
    
    // Константы для именования RabbitMQ сущностей
    private const string ExchangeName = "notifications";
    private const string RoutingKeyName = "email";
    private const string QueueName = "email-notify";
    private const string DeadQueueName = QueueName + "-dead";
    private const string DeadExchangeName = ExchangeName + "-dead";
    private const string RetryExchangeName = ExchangeName + "-retry";
    
    // Политика повторов с настройками по умолчанию
    private RabbitRetryPolicy RetryPolicy { get; } = new()
    {
        Count = 5,
        Timeout = 5000
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await conn.CreateChannelAsync(cancellationToken: stoppingToken);

        // Основной топик
        await _channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: stoppingToken);

        // Топик неотправленных
        await _channel.ExchangeDeclareAsync(
            exchange: DeadExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            cancellationToken: stoppingToken);

        // Топик на повтор
        await _channel.ExchangeDeclareAsync(
            exchange: RetryExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            cancellationToken: stoppingToken);

        // Основная очередь
        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", DeadExchangeName },
                { "x-dead-letter-routing-key", DeadQueueName }  
            },
            cancellationToken: stoppingToken);

        // Очередь для мертвых
        await _channel.QueueDeclareAsync(
            queue: DeadQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                { "x-message-ttl", RetryPolicy.Timeout },       
                { "x-dead-letter-exchange", RetryExchangeName } 
            },
            cancellationToken: stoppingToken);

        // Привязки
        await _channel.QueueBindAsync(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: RoutingKeyName,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: DeadQueueName,
            exchange: DeadExchangeName,
            routingKey: DeadQueueName,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: QueueName,
            exchange: RetryExchangeName,
            routingKey: RoutingKeyName,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageAsync;

        await _channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false, 
            consumer: consumer,
            cancellationToken: stoppingToken);
    }

    private async Task OnMessageAsync(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            var evt = JsonSerializer.Deserialize<EmailNotificationEvent>(ea.Body.Span);
            if (evt == null)
            {
                logger.LogError("Получено пустое email-сообщение");
                await NegativeAck(ea);
                return;
            }

            logger.LogInformation("Отправка email на адрес {Email}", evt.To);
            await mailSender.SendAsync(evt.To, evt.Subject, evt.BodyHtml);
            
            await PositiveAck(ea);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке email-уведомления");
            await NegativeAck(ea);
        }
    }

    private async Task PositiveAck(BasicDeliverEventArgs ea)
    {
        if (_channel != null && _channel.IsOpen)
            await _channel.BasicAckAsync(ea.DeliveryTag, false);
    }

    private async Task NegativeAck(BasicDeliverEventArgs ea)
    {
        if (_channel != null && _channel.IsOpen)
            await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null)
            await _channel.CloseAsync(cancellationToken: cancellationToken);
        
        await base.StopAsync(cancellationToken);
    }
}