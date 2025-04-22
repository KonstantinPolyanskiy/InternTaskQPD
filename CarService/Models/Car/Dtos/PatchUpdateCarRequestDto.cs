using System.Text.Json.Serialization;

namespace CarService.Models.Car.Requests;

/// <summary>
/// DTO для частичного обновления машины
/// </summary>
public class PatchUpdateCarRequestDto
{
    [JsonPropertyName("Brand")]
    public string? Brand { get; set; }
    
    [JsonPropertyName("Color")]
    public string? Color { get; set; }
    
    [JsonPropertyName("Price")]
    public decimal? Price { get; set; }
    
    [JsonPropertyName("Photo")]
    public string? Photo { get; set; }
    
    [JsonPropertyName("Mileage")]
    public int? Mileage { get; set; }
    
    [JsonPropertyName("Model")]
    public string? CurrentOwner { get; set; }
}