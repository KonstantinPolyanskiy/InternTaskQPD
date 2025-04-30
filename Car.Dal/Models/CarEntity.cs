using System.ComponentModel.DataAnnotations;
using Car.App.Models.CarModels;
using Car.App.Models.Dto;

namespace Car.Dal.Models;

/// <summary>
/// Таблица Car
/// </summary>
public class CarEntity()
{
    #region Конструктор

    public CarEntity(CarDto dto) : this()
    {
        Brand = dto.Brand!;
        Color = dto.Color!;
        Price = (decimal)dto.Price!;

        PrioritySale = (CarPrioritySale)dto.PrioritySale!; 
        CarCondition = (CarCondition)dto.Condition!;
    }
    
    public CarEntity(int id) : this() => Id = id;

    #endregion
    
    #region Поля
    
    public int Id { get; init; }
    
    [MaxLength(200)]
    public string? Brand { get; set; }
    
    [MaxLength(200)]
    public string? Color { get; set; }
    
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
    public PhotoEntity? Photo { get; init; }
    
    /// <summary>
    /// Любые дополнительные детали (UsedCarDetail, ManufacturingDetail, ManagerDetail и т.д.)
    /// будет храниться в виде JSON
    /// </summary>
    public Dictionary<string, object>? Details { get; init; }

    #endregion
}