namespace CarService.Models.Car.Exceptions;

/// <summary>
/// Исключение при попытке создать БУ автомобиль с неправильными полями
/// </summary>
public class InvalidDataSecondHandCar : Exception
{
    public InvalidDataSecondHandCar() { }
    
    public InvalidDataSecondHandCar(string message) : base(message) { }
    
    public InvalidDataSecondHandCar(string message, Exception inner) : base(message, inner) { }
    
}