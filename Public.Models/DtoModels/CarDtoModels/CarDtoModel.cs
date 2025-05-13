using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Public.Models.BusinessModels.CarModels;
using Public.Models.CommonModels;
using Public.Models.DtoModels.PhotoDtoModels;

namespace Public.Models.DtoModels.CarDtoModels;

/// <summary> DTO для добавления машину в систему (в целом) </summary>
public record DtoForAddCar
{
    public required string Brand { get; init; }
    public required string Color { get; init; }
    public required decimal Price { get; init; }
    
    public string? CurrentOwner { get; init; }
    public int? Mileage { get; init; }
    
    public DtoForAddPhoto? Photo { get; set; }
}

/// <summary> DTO для сохранения данных о добавляемой машине </summary>
public record DtoForSaveCar
{
    public Guid ResponsiveManager { get; init; }
    
    public required string Brand { get; init; }
    public required string Color { get; init; }
    public required decimal Price { get; init; }
    
    public string? CurrentOwner { get; init; }
    public int? Mileage { get; init; }
}

/// <summary> DTO для поиска машин по параметрам и с фильтрами </summary>
public record DtoForSearchCars
{
    public string[]? Brands { get; init; }
    public string[]? Colors { get; init; }
    public CarConditionTypes? Condition { get; init; } 
    public CarSortTermination? SortTerm { get; init; } = CarSortTermination.Id;
    public HavePhotoTermination? PhotoTerm { get; init; } = HavePhotoTermination.NoMatter;
    public SortDirection? Direction { get; init; } = SortDirection.Ascending;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public record DtoForUpdateCar
{
    public required int CarId { get; init; }
    
    public required string Brand { get; init; }
    
    public required string Color { get; init; }
    
    public required decimal Price { get; init; }
    
    public string? CurrentOwner { get; init; }
    
    public int? Mileage { get; init; }
    
    public Guid? NewManager { get; set; }
}