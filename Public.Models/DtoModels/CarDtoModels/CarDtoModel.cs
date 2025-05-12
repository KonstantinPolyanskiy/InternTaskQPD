using Public.Models.BusinessModels.CarModels;
using Public.Models.CommonModels;

namespace Public.Models.DtoModels.CarDtoModels;

public record DtoForCreateCar
{
    public required string Brand { get; init; }
    public required string Color { get; init; }
    public required decimal Price { get; init; }
    
    public string? CurrentOwner { get; init; }
    public int? Mileage { get; init; }
}

public record DtoForSearchCars
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