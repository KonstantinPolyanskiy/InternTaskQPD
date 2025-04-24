using Car.App.Exceptions;
using Contracts.Shared;

namespace Car.App.Models;

/// <summary>
/// Б/У автомобиль
/// </summary>
public class UsedCar : ICar
{
    /// <summary>
    /// Текущий владелец автомобиля
    /// </summary>
    /// <exception cref="NullReferenceException">Недопустимые данные - владелец null</exception>>
    /// <exception cref="InvalidDataException">Недопустимые данные - владельца нет (пустые строки, etc...)</exception>
    public required string CurrentOwner
    {
        get
        {
            if (_currentOwner is null) throw new NullReferenceException("current owner is null");
            
            // Для пустых строк и пробелов
            if (_currentOwner.Trim() == string.Empty) throw new InvalidUsedCarException("current owner is empty");
            
            return _currentOwner;
        }
        init
        {
            if (string.IsNullOrEmpty(value)) throw new InvalidUsedCarException("for used car current owner cannot be empty or null");
            _currentOwner = value;
        }
    }
    private string? _currentOwner;

    /// <summary>
    /// Пробег автомобиля
    /// </summary>
    /// <exception cref="InvalidUsedCarException">Недопустимые данные - пробег null</exception>
    public int Mileage
    {
        get
        {
            if (_mileage is null) throw new NullReferenceException("mileage is null");
            return (int)_mileage;
        }
        set
        {
            if (value <= 0) throw new InvalidUsedCarException("mileage used car cannot be negative or zero");
        }
    }
    private int? _mileage;    
    
    #region Поля ICar

    public int? Id { get; set; }
    
    public required string Brand { get; set; }
    
    public required string Color { get; set; }
    
    public required decimal Price { get; set; }
    
    public ApplicationPhotoModel? Photo { get; set; }
    public CarTypes CarType() => CarTypes.UsedCar;

    #endregion
}

/// <summary>
/// Новый автомобиль
/// </summary>
public class NewCar : ICar
{
    #region Поля ICar

    public int? Id { get; set; }
    
    public required string Brand { get; set; }
    
    public required string Color { get; set; }
    
    public required decimal Price { get; set; }
    
    public ApplicationPhotoModel? Photo { get; set; }
    
    public CarTypes CarType()
    {
        return CarTypes.NewCar;
    }

    #endregion
}