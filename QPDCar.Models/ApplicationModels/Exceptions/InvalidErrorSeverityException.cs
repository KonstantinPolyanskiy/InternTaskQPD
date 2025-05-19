namespace QPDCar.Models.ApplicationModels.Exceptions;

/// <summary> Попытка добавить невалидную ошибку  </summary>
public class InvalidErrorSeverityException : Exception
{
    public InvalidErrorSeverityException() { }

    public InvalidErrorSeverityException(string message)
        : base(message) { }

    public InvalidErrorSeverityException(string message, Exception innerException)
        : base(message, innerException) { }
}