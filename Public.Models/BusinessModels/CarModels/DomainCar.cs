using Public.Models.BusinessModels.PhotoModels;

namespace Public.Models.BusinessModels.CarModels;

public class DomainCar
{
    public int Id { get; set; }
    
    public string? Brand { get; set; }
    public string? Color { get; set; }
    public decimal? Price { get; set; }
    
    public string? CurrentOwner { get; set; }
    public int? Mileage { get; set; }
    
    public CarConditionTypes CarCondition { get; set; }
    
    public PrioritySaleTypes PrioritySale { get; set; }
    
    public DomainPhoto? Photo { get; set; }
}