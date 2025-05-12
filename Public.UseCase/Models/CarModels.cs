using Public.Models.BusinessModels.CarModels;
using Public.Models.CommonModels;

namespace Public.UseCase.Models;

public record DataForAddCar
{
    public required string Brand { get; init; }
    public required string Color { get; init; }
    public required decimal Price { get; init; }
    
    public string? CurrentOwner { get; init; }
    public int? Mileage { get; init; }
    
    public DataForAddPhoto? AddingPhoto { get; init; }
}

public record DataForSearchCars
{
    public string[]? Brands { get; init; }
    public string[]? Colors { get; init; }
    public CarConditionTypes? Condition { get; init; }
    public CarSortTermination? SortTerm { get; init; }
    public HavePhotoTermination? PhotoTerm { get; init; }
    public SortDirection? Direction { get; init; }
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
}

public record CarAddedResponse
{
    public int CarId { get; init; }
    
    public string Brand { get; init; } = null!;
    public string Color { get; init; } = null!;
    public decimal Price { get; init; }
    
    public string? CurrentOwner { get; init; }
    public int? Mileage { get; init; }
    
    public PhotoResponse? Photo { get; set; }
}

public record CarGetResponse
{
    public int CarId { get; init; }
    
    public string Brand { get; init; } = null!;
    public string Color { get; init; } = null!;
    public decimal Price { get; init; }
    
    public string? CurrentOwner { get; init; }
    public int? Mileage { get; init; }
    
    public CarConditionTypes? Condition { get; init; }
    public PrioritySaleTypes? PrioritySale { get; init; }
    
    public PhotoResponse? Photo { get; set; }
}

public record GetCarsResponse
{
    public CarGetResponse[] Cars { get; init; } = null!;
    
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}