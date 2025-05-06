using System.ComponentModel.DataAnnotations;
using Backend.App.Models.Dto;
using Enum.Common;

namespace Backend.Dal.Models;

/// <summary>
/// Таблица Car
/// </summary>
public class CarEntity()
{
    #region Поля
    
    public int Id { get; init; }
    
    [MaxLength(200)]
    public string Brand { get; set; } = null!;
    
    [MaxLength(200)]
    public string Color { get; set; } = null!;
    
    public decimal Price { get; set; }
    
    [MaxLength(200)]
    public string? CurrentOwner { get; set; }
    
    public int? Mileage { get; set; }
    
    /// <summary>
    /// Приоритет продажи машины
    /// </summary>
    public PrioritySale PrioritySale { get; set; }
    
    /// <summary>
    /// Состояние машины
    /// </summary>
    public CarCondition CarCondition { get; set; }
    
    /// <summary>
    /// Id фото автомобиля, может быть fk для таблицы хранящей фото, или идентификатор на запись в другом хранилище
    /// </summary>
    public int? PhotoMetadataId { get; set; }
    
    public PhotoMetadataEntity? PhotoMetadata { get; set; }
    
    #endregion
}