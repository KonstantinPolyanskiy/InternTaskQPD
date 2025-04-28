using Car.App.Models;
using Car.App.Models.CarModels;

namespace Car.Dal.Models;

/// <summary>
/// Таблица Car
/// </summary>
public class Car()
{
    #region Конструктор

    public Car(CarData data) : this()
    {
        Brand = data.Brand;
        Color = data.Color;
        Price = (decimal)data.Price;

        PrioritySale = (CarPrioritySale)data.PrioritySale; 
        CarCondition = (CarCondition)data.Condition;
    }
    
    public Car(int id) : this() => Id = id;

    #endregion
    
    #region Поля
    
    public int Id { get; set; }
    
    public string Brand { get; set; }
    
    public string Color { get; set; }
    
    public decimal Price { get; set; }
    
    /// <summary>
    /// Приоритет продажи машины
    /// </summary>
    public CarPrioritySale PrioritySale { get; set; }
    
    /// <summary>
    /// Состояние машины
    /// </summary>
    public CarCondition CarCondition { get; set; }
    
    /// <summary>
    /// Откуда получено фото / куда необходимо загрузить
    /// </summary>
    public PhotoStorageType StorageType { get; set; }
    
    /// <summary>
    /// Id фото автомобиля, может быть fk для таблицы хранящей фото, или идентификатор на запись в другом хранилище
    /// </summary>
    public int? PhotoId { get; set; }
    
    #endregion

    #region Другие таблицы

    /// <summary>
    /// Фото
    /// </summary>
    public Photo? Photo { get; set; }
    
    /// <summary>
    /// Любые дополнительные детали (UsedCarDetail, ManufacturingDetail, ManagerDetail и т.д.)
    /// будет храниться в виде JSON
    /// </summary>
    public Dictionary<string, object>? Details { get; set; }

    #endregion
}