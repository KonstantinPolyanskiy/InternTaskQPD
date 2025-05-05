using Enum.Common;

namespace Backend.App.Models.Commands;

public record CreateCarCommand
{
    public required string Brand { get; init; }
    
    public required string Color { get; init; }
    
    public required decimal Price { get; init; }
    
    
    public string? CurrentOwner { get; init; }
    
    public int? Mileage { get; init; }
}

public record UpdateCarCommand
{
    public int Id { get; set; }
    
    public Guid? PhotoId { get; init; }
    
    public string? Brand { get; init; }
    public string? Color { get; init; }
    public decimal? Price { get; init; }
    
    public string? CurrentOwner { get; init; }
    public int? Mileage { get; init; }
    
    public CarCondition? Condition { get; init; }
    public PrioritySale? PrioritySale { get; init; }
}

public record SearchCarCommand
{
    public required int CarId { get; init; }
    public bool NeedPhoto { get; init; }
}

public record DeleteCarCommand
{
    public int Id { get; init; }
    public bool HardDelete { get; init; }
}