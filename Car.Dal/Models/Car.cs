namespace Car.Dal.Models;

/// <summary>
/// Таблица Car
/// </summary>
public class Car()
{
    public Car(string brand, string color, decimal price, byte carType, string photoIdTerm) : this()
    {
    }
    
    public Photo(int id) : this() => Id = id;
    
    public int Id { get; set; }
    
    public string Brand { get; set; }
    
    public string Color { get; set; }
    
    public decimal Price { get; set; }
    
    /// <summary>
    /// Числовое представление типа машины (см <see cref="CarTypes"/>)
    /// </summary>
    public byte CarType { get; set; }
    
    /// <summary>
    /// Id фото автомобиля, может быть fk для таблицы хранящей фото, или идентификатор на запись в другом хранилище
    /// </summary>
    public int? PhotoId { get; set; }

    /// <summary>
    /// Фото
    /// </summary>
    public Photo? Photo { get; set; }
}