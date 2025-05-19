using QPDCar.Models.DtoModels.PhotoDtos;

namespace QPDCar.Models.DtoModels.CarDtos;

/// <summary> DTO для сохранения данных о добавляемой машине </summary>
public record DtoForSaveCar
{
    public Guid ResponsiveManager { get; init; }
    
    public required string Brand { get; init; }
    public required string Color { get; init; }
    public required decimal Price { get; init; }
    
    public string? CurrentOwner { get; init; }
    public int? Mileage { get; init; }
    
    public DtoForSavePhoto? Photo { get; set; }
}

/// <summary> DTO для обновления данных существующей машины </summary>
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