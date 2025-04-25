using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Car.App.Models;
using Contracts.Shared;
using Contracts.Types;

namespace CarService.Models;

/// <summary>
/// Описывает запрос с данными на добавление новой машины
/// </summary>
public class AddCarRequest : ICar, IUsedCar
{
    #region Обязательные поля для создания

    [Required]
    [JsonPropertyName("brand")]
    public required string Brand { get; set; }
    
    [Required]
    [JsonPropertyName("color")]
    public required string Color { get; set; }
    
    [Required]
    [JsonPropertyName("price")]
    public required decimal? Price { get; set; }
    
    #endregion
    
    [JsonPropertyName("photo")]
    public IFormFile? Photo { get; set; }
    
    #region Опциональные поля для БУ машины

    [JsonPropertyName("mileage")]
    public int? Mileage { get; set; }
    
    [JsonPropertyName("current_owner")]
    public string? CurrentOwner { get; set; }
    
    #endregion
}

/// <summary>
/// Описывает ответ с машиной
/// </summary>
public class CarResponse : ICar, IUsedCar, ICarType
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }
    
    [JsonPropertyName("car_type")]
    public CarTypes? CarType { get; set; }

    #region Стандартные хар-ки машины
    
    [JsonPropertyName("brand")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Brand { get; set; }
    
    [JsonPropertyName("color")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; set; }
    
    [JsonPropertyName("price")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? Price { get; set; }

    #endregion

    #region Хар-ки БУ машины
    
    [JsonPropertyName("mileage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Mileage { get; set; }
    
    [JsonPropertyName("current_owner")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CurrentOwner { get; set; }

    #endregion
}

/// <summary>
/// Описывает запрос в данными для обновления машины
/// </summary>
public class PatchCarRequest : ICar, IUsedCar
{
    [JsonPropertyName("brand")]
    public string? Brand { get; set; }
    
    [JsonPropertyName("color")]
    public string? Color { get; set; }
    
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }
    
    [JsonPropertyName("mileage")]
    public int? Mileage { get; set; }
    
    [JsonPropertyName("current_owner")]
    public string? CurrentOwner { get; set; }
}
