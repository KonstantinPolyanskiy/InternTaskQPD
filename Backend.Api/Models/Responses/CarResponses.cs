using System.Text.Json.Serialization;
using Enum.Common;

namespace Backend.Api.Models.Responses;

public record CarResponse
{
    public int Id { get; init; }
    
    public required string Brand { get; init; } 
    public required string Color { get; init; }
    public decimal Price { get; init; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CurrentOwner { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Mileage { get; init; }
    
    public PrioritySale PrioritySale { get; init; }
    public CarCondition Condition { get; init; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PhotoResponse? Photo { get; init; }
}

