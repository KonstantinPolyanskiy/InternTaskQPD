using System.Text.Json.Serialization;

namespace CarService.Models.Car.Requests;

/// <summary>
/// Запрос на добавление автомобиля в сервис
/// </summary>
public class CreateCarRequestDto()
{
    [JsonPropertyName("Brand")]
    public string? Brand { get; set; }

    [JsonPropertyName("Color")]
    public string? Color { get; set; }

    [JsonPropertyName("Price")]
    public decimal Price { get; set; }

    [JsonPropertyName("Photo")]
    public string? Photo { get; set; } 
    
    [JsonPropertyName("Mileage")]
    public int? Mileage { get; set; }

    [JsonPropertyName("CurrentOwner")]
    public string? CurrentOwner { get; set; }
}