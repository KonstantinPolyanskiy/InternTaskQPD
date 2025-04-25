using Contracts.Types;

namespace Contracts.Shared;

/// <summary>
/// Описывает обязательный набор полей и сущность "Автомобиль" в целом 
/// </summary>
public interface ICar
{
    /// <summary> Марка </summary>
    public string Brand { get; set; }
    
    /// <summary> Цвет </summary>
    public string Color { get; set; }
    
    /// <summary> Цена </summary>
    public decimal? Price { get; set; }
}

/// <summary>
/// Описывает набор свойств для Б/У машины
/// </summary>
public interface IUsedCar
{
    public int? Mileage { get; set; }
    public string? CurrentOwner { get; set; }
}



/// <summary>
/// Тип автомобиля
/// </summary>
public interface ICarType
{
    /// <summary>
    /// Возвращает какого типа автомобиль: ноывый, б/у, etc...
    /// </summary>
    /// <returns>Тип автомобиля</returns>
    public CarTypes? CarType { get; set; }
}