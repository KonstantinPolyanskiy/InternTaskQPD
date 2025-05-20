using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using QPDCar.Models.ApplicationModels.FilterModels;
using QPDCar.Models.ApplicationModels.SortingModels;
using QPDCar.Models.BusinessModels.CarModels;

namespace QPDCar.Api.Models.Requests;

/// <summary> Запрос на добавление машины </summary>
public record AddCarRequest
{
    [Required]
    public required string Brand { get; init; }
    
    [Required]
    public required string Color { get; init; }
    
    [Required]
    public required decimal Price { get; init; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CurrentOwner { get; init; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Mileage { get; init; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IFormFile? Photo { get; init; }
}

/// <summary> Запрос на обновление машины </summary>
public record PatchCarRequest
{
    [Required]
    public required string Brand { get; init; }
    
    [Required]
    public required string Color { get; init; }
    
    [Required]
    public required decimal Price { get; init; }
    
    [Required]
    public required string CurrentOwner { get; init; }
    
    [Required]
    public required int Mileage { get; init; }
    
    public string? NewManager { get; init; }
}

/// <summary> Параметры и фильтры на запрос м-ва машин </summary>
public record CarQueryRequest
{
    public string[]? Brands { get; init; }
    public string[]? Colors { get; init; }
    public ConditionTypes? Condition { get; init; }
    public CarSortTermination? SortTerm { get; init; }
    public HavePhotoTermination? PhotoTerm { get; init; }
    public SortDirection? Direction { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}