using System.Text.Json.Serialization;
using Models.Bridge.Car;

namespace Car.App.Models.CarModels;

public class CarResult : CarData
{
    public int Id { get; set; }
}

/// <summary> DTO для передачи данных api -> service </summary>
public class CarRequest 
{
    #region Обязательные данные

    /// <summary> Марка </summary>
    public string? Brand { get; set; }

    /// <summary> Цвет </summary>
    public string? Color { get; set; }

    /// <summary> Цена </summary>
    public decimal Price { get; set; }

    #endregion
    
    #region Детали машины
        
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UsedCarDetailDto? UsedCarDetail { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ManufacturingDetailDto? ManufacturingDetail { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ManagerDetailDto? ManagerDetail { get; set; }
    
    #endregion
}

/// <summary> Данные для сохранения машины </summary>
public class CarData
{
    #region Обязательные данные

    /// <summary> Марка </summary>
    public string? Brand { get; set; }

    /// <summary> Цвет </summary>
    public string? Color { get; set; }

    /// <summary> Цена </summary>
    public decimal? Price { get; set; }

    #endregion

    #region Свойства машины

    public CarCondition? Condition { get; set; }
    
    public CarPrioritySale? PrioritySale { get; set; }

    #endregion

    #region Фото

    public string? PhotoTermId { get; set; }
    
    public PhotoStorageType? StorageType { get; set; }

    #endregion
}