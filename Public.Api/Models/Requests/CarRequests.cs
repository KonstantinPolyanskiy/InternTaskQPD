using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Public.Models.BusinessModels.CarModels;
using Public.Models.CommonModels;

namespace Public.Api.Models.Requests;

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
    
    public required IFormFile? Photo { get; init; }
}

public record CarQueryRequest
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