using Enum.Common;

namespace Backend.App.Models.Dto;

public record CarDto
{
    public int? Id { get; init; }
    
    public int? PhotoMetadataId { get; init; }
    
    public string? Brand { get; init; }
    public string? Color { get; init; }
    public decimal? Price { get; init; }
    
    public string? CurrentOwner { get; init; }
    public int? Mileage { get; init; }
    
    public CarCondition? Condition { get; init; }
    public PrioritySale? PrioritySale { get; init; }
}

