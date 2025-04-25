using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Models.Bridge.Car;

namespace CarService.Models;

/// <summary>
/// Описывает запрос на создание машины
/// </summary>
public class AddCarRequest 
{
    #region Обязательные поля для создания

    [Required]
    public required string Brand { get; set; }
    
    [Required]
    public required string Color { get; set; }
    
    [Required]
    public required decimal? Price { get; set; }
    
    #endregion
    
    [JsonPropertyName("photo")]
    public IFormFile? Photo { get; set; }
    
    public int? Mileage { get; set; }
    public string? CurrentOwner { get; set; }
    public string? Country { get; set; }
    public string? LotNumber { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public int? ManagerId { get; set; }
    public string? DisplayName { get; set; }
}

/// <summary>
/// Описывает запрос на обновление машины
/// </summary>
public class PatchCarRequest
{
    public required string Brand { get; set; }
    
    public required string Color { get; set; }
    
    public required decimal? Price { get; set; }
}
