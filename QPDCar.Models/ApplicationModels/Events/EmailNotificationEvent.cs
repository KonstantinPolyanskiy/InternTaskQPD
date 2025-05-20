namespace QPDCar.Models.ApplicationModels.Events;

/// <summary> Данные для эвента отправки email сообщения </summary>
public class EmailNotificationEvent
{
    public required Guid MessageId { get; init; }
    
    public required string To { get; init; }
    public required string Subject { get; init; }
    
    public required string BodyHtml { get; init; }
}