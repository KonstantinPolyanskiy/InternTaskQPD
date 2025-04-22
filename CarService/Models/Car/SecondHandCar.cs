using System.Runtime.CompilerServices;
using CarService.Models.Car.Exceptions;
using CarService.Models.Car.Requests;

namespace CarService.Models.Car;

public class SecondHandCar : BaseCar, ISecondHandCar
{
    public SecondHandCar() { }
        
    private int? _mileage;
    
    /// <summary>
    /// Пробег автомобиля 
    /// </summary>
    public int? Mileage
    {
        get => _mileage;
        set
        {
            if (value is null || value <= 0)
                throw new InvalidDataSecondHandCar("Mileage must be greater than 0");
            _mileage = value;
        }
    }

    private string? _currentOwner;

    /// <summary>
    /// Текущий владелец автомобиля
    /// </summary>
    public string? CurrentOwner
    {
        get => _currentOwner;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidDataSecondHandCar("CurrentOwner is required");
            _currentOwner = value;
        }
    }
}