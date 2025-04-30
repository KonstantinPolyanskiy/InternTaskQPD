using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Models.Bridge.Car;

/// <summary>
/// Запрос на добавление новой машины
/// </summary>
public record AddCarRequest
{
    [Required]
    public required string Brand { get; init; }
    
    [Required]
    public required string Color { get; init; }
    
    [Required]
    public required decimal Price { get; init; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UsedCarDetailDto? UsedCarDetail { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ManufacturingDetailDto? ManufacturingDetail { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ManagerDetailDto? ManagerDetail { get; set; }
    
}

public class UsedCarDetailDto
{
    public int Mileage { get; set; }
    public string CurrentOwner { get; set; } = null!;
}

public class ManufacturingDetailDto
{
    public string Country { get; set; } = null!;
    public string LotNumber { get; set; } = null!;
    public DateTime ManufacturingDate { get; set; }
}

public class ManagerDetailDto
{
    public int ManagerId { get; set; }
    public string DisplayName { get; set; } = null!;
}