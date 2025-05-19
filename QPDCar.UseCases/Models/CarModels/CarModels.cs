using QPDCar.Models.BusinessModels.CarModels;
using QPDCar.UseCases.Models.PhotoModels;
using QPDCar.UseCases.Models.UserModels;

namespace QPDCar.UseCases.Models.CarModels;

public record CarUseCaseResponse
{
    public int Id { get; init; }
    
    public required string Brand { get; init; }
    public required string Color { get; init; }
    public required decimal Price { get; init; }
    public string? CurrentOwner { get; init; }
    public int? Mileage { get; init; }
    
    public ConditionTypes? CarCondition { get; init; }
    public PrioritySaleTypes? PrioritySale { get; init; }
    
    public required EmployerUseCaseResponse Employer { get; set; }
    
    public PhotoUseCaseResponse? Photo { get; set; }
}

public record CarUseCaseResponsePage
{
    public List<CarUseCaseResponse>? Cars { get; set; }
    
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}