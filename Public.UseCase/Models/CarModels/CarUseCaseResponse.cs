using Public.Models.BusinessModels.CarModels;
using Public.UseCase.Models.PhotoModels;
using Public.UseCase.Models.UserModels;

namespace Public.UseCase.Models.CarModels;

public record CarUseCaseResponse
{
    public int Id { get; init; }
    
    public required string Brand { get; init; }
    public required string Color { get; init; }
    public required decimal Price { get; init; }
    public string? CurrentOwner { get; init; }
    public int? Mileage { get; init; }
    
    public CarConditionTypes? CarCondition { get; init; }
    public PrioritySaleTypes? PrioritySale { get; init; }
    
    public required EmployerUseCaseResponse Employer { get; set; }
    
    public PhotoUseCaseResponse? Photo { get; set; }
}