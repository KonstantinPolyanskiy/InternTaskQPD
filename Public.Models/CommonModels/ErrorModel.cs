using System.Net;

namespace Public.Models.CommonModels;

/// <summary> Различные уровни ошибок </summary>
public enum ErrorSeverity
{
    /// <summary> Неважная ошибка, ближе к замечанию </summary>
    NotImportant,
    
    /// <summary> Критическая ошибка - ответ 2xx (Ok) невозможен </summary>
    Critical,
}

/// <summary> Ошибка приложения </summary>
public sealed class ApplicationError
{
    /// <summary> Http код который необходимо отдать </summary>
    public HttpStatusCode? HttpStatusCode { get; } = null!;
    
    public Enum ErrorType { get; }
    
    public string Title { get; }
    public string Message { get; }
    public ErrorSeverity Severity { get; }
    
    public ApplicationError(
        Enum errorType,
        string title,
        string message,
        ErrorSeverity severity,
        HttpStatusCode? httpStatusCode = null)
    {
        ErrorType = errorType;
        Title           = title;
        Message         = message;
        Severity        = severity;
        HttpStatusCode  = httpStatusCode;
    }
}