namespace Public.Models.NotificationModels;

public record EmailNotification
{
    public required string To { get; init; }
    public required string Subject { get; init; }
    public required string PlainBody { get; init; }
}