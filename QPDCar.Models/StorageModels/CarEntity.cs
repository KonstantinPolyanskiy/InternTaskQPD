using QPDCar.Models.BusinessModels.CarModels;

namespace QPDCar.Models.StorageModels;

/// <summary> Описание таблицы с машиной </summary>
public class CarEntity()
{
    public int Id { get; init; }
    
    public string Brand { get; set; } = null!;

    public string Color { get; set; } = null!;

    public decimal Price { get; set; }
    
    public string? CurrentOwner { get; set; }
    
    public int? Mileage { get; set; }
    
    public bool IsSold { get; set; } = false;
    
    public PrioritySaleTypes PrioritySale { get; set; }
    
    public ConditionTypes CarCondition { get; set; }
    
    public Guid ResponsiveManagerId { get; set; }
    public int? PhotoMetadataId { get; set; }
    
    public PhotoMetadataEntity? PhotoMetadata { get; set; }
}

/// <summary> Результат запроса таблицы машин по параметрам </summary>
public class CarEntityPage
{
    public List<CarEntity> Cars { get; init; } = [];
    
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}