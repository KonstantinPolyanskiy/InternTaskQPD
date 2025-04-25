using Contracts.Shared;
using Contracts.Types;

namespace Contracts.Dtos;

/// <summary>
/// DTO добавляемой машины для сервиса
/// </summary>
public class AddCarDomain : ICar, IUsedCar
{
    public ApplicationPhotoModel? Photo { get; set; }

    #region Обязательные поля для создания

    public string? Brand { get; set; }
    
    public string? Color { get; set; }
    
    public decimal? Price { get; set; }
    
    #endregion
    
    #region Опциональные поля для БУ машины

    public int? Mileage { get; set; }
    
    public string? CurrentOwner { get; set; }
    
    #endregion
}

/// <summary>
/// DTO сохраняемой машины в слое данных
/// </summary>
public class AddCarEntity : ICar, IUsedCar
{
    #region #region Обязательные поля для создания

    public required string Brand { get; set; }
    
    public required string Color { get; set; }
    
    public required decimal? Price { get; set; }
    
    #endregion
    
    public ApplicationPhotoModel? Photo { get; set; }
    
    public int? Mileage { get; set; }
    
    public string? CurrentOwner { get; set; }
    
    public CarTypes CarType { get; set; }
}

/// <summary>
/// DTO обновляемой машины для сервиса (фото обновляется отдельным методом)
/// </summary>
public class PatchCarDomain : ICar, IUsedCar
{
    #region Обязательные поля для создания

    public string? Brand { get; set; }
    
    public string? Color { get; set; }
    
    public decimal? Price { get; set; }
    
    #endregion
    
    #region Опциональные поля для БУ машины

    public int? Mileage { get; set; }
    
    public string? CurrentOwner { get; set; }
    
    #endregion
}

/// <summary>
/// DTO обновляемой машины в слое данных  
/// </summary>
public class PatchCarEntity : ICar, IUsedCar
{
    /// <summary>
    /// Id обновляемой машины
    /// </summary>
    public int Id { get; set; }
    
    #region Обязательные поля для создания

    public string? Brand { get; set; }
    
    public string? Color { get; set; }
    
    public decimal? Price { get; set; }
    
    #endregion
    
    #region Опциональные поля для БУ машины

    public int? Mileage { get; set; }
    
    public string? CurrentOwner { get; set; }
    
    #endregion
}
