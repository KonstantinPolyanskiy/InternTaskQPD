using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Enum.Common;

namespace Backend.Api.Models.Requests;

/// <summary> Запрос на добавление новой машины </summary>
public record AddCarRequest
{
    [Required]
    public required string Brand { get; init; }
    
    [Required]
    public required string Color { get; init; }
    
    [Required]
    public required decimal Price { get; init; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CurrentOwner { get; init; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Mileage { get; init; }
}

public record PatchCarRequest
{
    public string? Brand { get; init; }
    public string? Color { get; init; }
    public decimal? Price { get; init; }
    public string? CurrentOwner { get; init; }
    public int? Mileage { get; init; }
    public CarCondition? Condition { get; init; }
    public PrioritySale? PrioritySale { get; init; }
}