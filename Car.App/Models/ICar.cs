using Contracts.Shared;
using Contracts.Types;

namespace Car.App.Models;

/// <summary>
/// Описывает обязательный набор полей и сущность "Автомобиль" в целом 
/// </summary>
public interface ICar
{
    /// <summary> Идентификатор </summary>
    public int? Id { get; set; }
    
    /// <summary> Марка </summary>
    public string Brand { get; set; }
    
    /// <summary> Цвет </summary>
    public string Color { get; set; }
    
    /// <summary> Цена </summary>
    public decimal Price { get; set; }
    
    /// <summary> Фото </summary>
    public ApplicationPhotoModel? Photo { get; set; }

    /// <summary>
    /// Возвращает какого типа автомобиль: ноывый, б/у, etc...
    /// </summary>
    /// <returns>Тип автомобиля</returns>
    public CarTypes CarType();
}