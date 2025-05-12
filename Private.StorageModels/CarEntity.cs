using Public.Models.BusinessModels.CarModels;

namespace Private.StorageModels;

/// <summary> Таблица Car </summary>
public class CarEntity
{
    public int Id { get; init; }
    
    public string Brand { get; set; } = null!;

    public string Color { get; set; } = null!;

    public decimal Price { get; set; }
    
    public string? CurrentOwner { get; set; }
    
    public int? Mileage { get; set; }
    
    public PrioritySaleTypes PrioritySale { get; set; }
    
    public CarConditionTypes CarCondition { get; set; }
    
    public int? PhotoMetadataId { get; set; }
    public PhotoMetadataEntity? PhotoMetadata { get; set; }
}

public class CarsEntityPage
{
    public List<CarEntity> Cars { get; init; } = [];
    
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}