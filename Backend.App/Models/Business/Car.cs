using Enum.Common;

namespace Backend.App.Models.Business;

public class Car
{
    public int Id { get; init; }
    
    public required string Brand { get; set; }
    public required string Color { get; set; }
    public required decimal Price { get; set; }
    
    public string? CurrentOwner { get; set; }
    public int? Mileage { get; set; }
    
    public Photo? Photo { get; set; }
    
    public PrioritySale PrioritySale { get; set; }
    public CarCondition Condition { get; set; }
}