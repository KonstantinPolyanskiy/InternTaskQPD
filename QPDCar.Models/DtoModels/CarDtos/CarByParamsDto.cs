using QPDCar.Models.ApplicationModels.FilterModels;
using QPDCar.Models.ApplicationModels.SortingModels;
using QPDCar.Models.BusinessModels.CarModels;

namespace QPDCar.Models.DtoModels.CarDtos;

/// <summary> DTO для поиска машин по параметрам и с фильтрами </summary>
public record DtoForSearchCars
{
    public string[]? Brands { get; init; }
    public string[]? Colors { get; init; }
    
    public ConditionTypes? Condition { get; init; } 
    
    public CarSortTermination? SortTerm { get; init; } = CarSortTermination.Id;
    public HavePhotoTermination? PhotoTerm { get; init; } = HavePhotoTermination.NoMatter;
    
    public SortDirection? Direction { get; init; } = SortDirection.Ascending;
    
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}