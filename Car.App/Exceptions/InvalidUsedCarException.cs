namespace Car.App.Exceptions;

/// <summary>
/// Исключение с невалидными данными для работы с БУ авто
/// </summary>
public class InvalidUsedCarException : Exception
{
    public InvalidUsedCarException() { }
    
    public InvalidUsedCarException(string message) : base(message) { }
    
    public InvalidUsedCarException(string message, Exception inner) : base(message, inner) { }
}