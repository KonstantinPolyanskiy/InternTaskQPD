using System.Text.Json.Serialization;
using Car.App.Models.CarModels;
using Models.Bridge.Car;

namespace Car.App.Models.Dto;

/// <summary>
/// Данные сохраненной машины
/// </summary>
public record CarResultDto
{
    public int Id { get; init; }
    
    #region Обязательные данные

    public string? Brand { get; init; }
    public string? Color { get; init; }
    public decimal? Price { get; init; }

    #endregion
    
    #region Свойства машины

    public CarCondition? Condition { get; init; }
    
    public CarPrioritySale? PrioritySale { get; init; }

    #endregion
    
    #region Фото

    public string? PhotoTermId { get; init; }
    
    public PhotoStorageType? StorageType { get; init; }

    #endregion
}

/// <summary>
/// Данные для сохранения машины
/// </summary>
public record CarDto
{
    #region Обязательные данные

    public string? Brand { get; set; }
    public string? Color { get; set; }
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

/// <summary>
/// Данные для создания машины в системе
/// </summary>
public record CarRequestDataDto 
{
    #region Обязательные данные

    public string? Brand { get; set; }
    public string? Color { get; set; }
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



