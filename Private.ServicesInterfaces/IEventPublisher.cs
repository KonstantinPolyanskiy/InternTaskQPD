namespace Private.ServicesInterfaces;


public interface IEventPublisher
{
    /// <summary> Подписывает слушателя на событие </summary>
    void Subscribe<TEvent>(IEventSubscriber<TEvent> subscriber);
    
    /// <summary> Отписывает слушателя от события </summary> 
    public void Unsubscribe<TEvent>(IEventSubscriber<TEvent> listener);

    /// <summary> Оповещает слушателей </summary>
    public Task PublishAsync<TEvent>(TEvent evt, CancellationToken ct = default);
}