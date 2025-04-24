using Car.App.Models;
using Contracts.Shared;

namespace Contracts.Dtos;

/// <summary>
/// DTO добавляемой машины для сервиса
/// </summary>
public class AddedCarServicesDto
{
    #region Обязательные поля для создания

    public string? Brand { get; set; }
    
    public string? Color { get; set; }
    
    public decimal? Price { get; set; }
    
    public ApplicationPhotoModel? Photo { get; set; }

    #endregion
    
    #region Опциональные поля для БУ машины

    public int? Mileage { get; set; }
    
    public string? CurrentOwner { get; set; }
    
    #endregion
}

/// <summary>
/// DTO обновляемой машины для сервиса
/// </summary>
public class PatchCarServicesDto : AddedCarServicesDto {}

/// <summary>
/// DTO сохраняемой машины в слое данных
/// </summary>
public class AddedCarDataLayerDto
{
    public required string Brand { get; set; }
    
    public required string Color { get; set; }
    
    public required decimal Price { get; set; }
    
    public ApplicationPhotoModel? Photo { get; set; }
    
    public int? Mileage { get; set; }
    
    public string? CurrentOwner { get; set; }
    
    public CarTypes CarType { get; set; }
}

/// <summary>
/// DTO обновляемой машины в слое данных  
/// </summary>
public class UpdatedCarDataLayerDto : AddedCarDataLayerDto {}
