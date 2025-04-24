using System.Buffers.Text;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Car.App.Models;

namespace CarService.Models;

/// <summary>
/// Описывает запрос с данными на добавление новой машины
/// </summary>
public class AddCarRequest
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
    public required decimal Price { get; set; }
    
    [JsonPropertyName("photo")]
    public IFormFile? Photo { get; set; }

    #endregion
    
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
public class CarResponse
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }
    
    [JsonPropertyName("car_type")]
    public required string CarType { get; init; }
    
    [JsonPropertyName("standard_car_parameters")]
    public StandardCarParameters? StandardParameters { get; init; }
    
    [JsonPropertyName("used_car_parameters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UsedCarParameters? UsedParameters { get; set; }
}

/// <summary>
/// Значения свойственные всем машинам
/// </summary>
public class StandardCarParameters
{
    [JsonPropertyName("brand")]
    public string? Brand { get; set; }
    
    [JsonPropertyName("color")]
    public string? Color { get; set; }
    
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
    
    [JsonPropertyName("photo")]
    public string? PhotoBase64 { get; set; }
}

/// <summary>
/// Значения свойственные БУ машинам
/// </summary>
public class UsedCarParameters
{
    [JsonPropertyName("mileage")]
    public int? Mileage { get; set; }
    
    [JsonPropertyName("current_owner")]
    public string? CurrentOwner { get; set; }
}

/// <summary>
/// Описывает запрос в данными для обновления машины
/// </summary>
public class PatchCarRequest
{
    [JsonPropertyName("brand")]
    public string? Brand { get; set; }
    
    [JsonPropertyName("color")]
    public string? Color { get; set; }
    
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }
    
    [JsonPropertyName("photo")]
    public IFormFile? Photo { get; set; }
    
    public UsedCarParameters? UsedParameters { get; init; }
}