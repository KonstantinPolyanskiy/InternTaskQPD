namespace Private.ServicesInterfaces;

public interface IEventSubscriber { }

public interface IEventSubscriber<in TEvent> : IEventSubscriber
{
    Task OnEventAsync(TEvent evt, CancellationToken ct = default);
}
