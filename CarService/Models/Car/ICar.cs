using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CarService.Models.Car;

public interface ICar
{
    /// <summary>
    /// Идетификатор автомобиля
    /// </summary>
    [JsonPropertyName("Id")]
    [Required]
    public int? Id { get; set; }
    
    /// <summary>
    /// Марка
    /// </summary>
    [JsonPropertyName("Brand")]
    [Required]
    public string? Brand { get; set; }
    
    /// <summary>
    /// Цвет 
    /// </summary>
    [JsonPropertyName("Color")]
    [Required]
    public string? Color { get; set; }
    
    /// <summary>
    /// Цена
    /// </summary>
    [JsonPropertyName("Price")]
    [Required]
    public decimal Price { get; set; }
    
    /// <summary>
    /// Фото автомобиля not implemented 
    /// </summary>
    [JsonPropertyName("Photo")]
    [Required]
    public string? Photo { get; set; } 
}