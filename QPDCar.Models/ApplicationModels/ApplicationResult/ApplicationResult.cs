using QPDCar.Models.ApplicationModels.Exceptions;

namespace QPDCar.Models.ApplicationModels.ApplicationResult;

/// <summary> Общий ответ содержащий бизнес результат и возможные ошибки </summary>
public sealed class ApplicationExecuteResult<T>
{
    private readonly List<ApplicationError> _errors = [];

    public T? Value { get; set; }

    public IList<ApplicationError> GetWarnings => _errors.Where(e => e.Severity is ErrorSeverity.NotImportant).ToList();
    
    public IList<ApplicationError> GetCriticalErrors => _errors.Where(e => e.Severity is ErrorSeverity.Critical).ToList();

    /// <summary> Содержит ли модель Critical ошибки </summary>
    public bool IsSuccess => _errors.All(e => e.Severity != ErrorSeverity.Critical);
    
    public bool ContainsError<TEnum>(TEnum errorType) where TEnum : Enum =>
        _errors.Any(e => e.ErrorType is TEnum en && en.Equals(errorType));
    
    public bool ContainsError(Enum errorType) =>
        _errors.Any(e => e.ErrorType.Equals(errorType));

    private ApplicationExecuteResult(T? value) => Value = value;

    public static ApplicationExecuteResult<T> Success(T value) => new(value);

    public static ApplicationExecuteResult<T> Failure(params ApplicationError[] errors)
    {
        var r = new ApplicationExecuteResult<T>(default);
        r._errors.AddRange(errors);
        return r;
    }

    public ApplicationExecuteResult<T> DeleteError(Enum errorType)
    {
        _errors.RemoveAll(e => e.ErrorType.Equals(errorType));
        return this;
    }

    private ApplicationExecuteResult<T> WithError(ApplicationError error)
    {
        _errors.Add(error);
        return this;
    }

    private ApplicationExecuteResult<T> WithErrors(IEnumerable<ApplicationError> errors)
    {
        _errors.AddRange(errors);
        return this;
    }
    
    public ApplicationExecuteResult<T> WithWarning(ApplicationError warning)
    {
        if (warning.Severity is not ErrorSeverity.NotImportant      
            || warning.HttpStatusCode is not null)
            throw new InvalidErrorSeverityException(
                "Попытка добавить Warning ошибку с недопустимым уровнем или c not-null HttpStatusCod'ом");

        _errors.Add(warning);
        return this;
    }
    
    public ApplicationExecuteResult<T> WithPossiblyWarning(ApplicationError? warning)
    {
        if (warning is null) return this;
        
        if (warning.Severity is not ErrorSeverity.Critical || warning.HttpStatusCode is not null)
            throw new InvalidErrorSeverityException("Попытка добавить Warning ошибку с недопустимым уровнем или c not-null HttpStatusCod'ом");
        
        WithError(warning);
        return this;
    }
    
    public ApplicationExecuteResult<T> WithWarnings(IEnumerable<ApplicationError> warnings)
    {
        var warns = warnings.ToList();
        if (warns.Any(e => e.Severity is not ErrorSeverity.NotImportant || e.HttpStatusCode is not null))
            throw new InvalidErrorSeverityException("Попытка добавить Warning ошибку с недопустимым уровнем или c not-null HttpStatusCod'ом");
        
        WithErrors(warns);
        return this;
    }

    public ApplicationExecuteResult<T> WithCritical(ApplicationError criticalError) 
    {
        if (criticalError.Severity is not ErrorSeverity.Critical 
            || criticalError.HttpStatusCode is null ||
            string.IsNullOrWhiteSpace(criticalError.Title))
            throw new InvalidErrorSeverityException("Попытка добавить Critical ошибку с недопустимым уровнем/без названия/без HttpStatusCode");
        
        WithError(criticalError);
        return this;
    }
    
    public ApplicationExecuteResult<T> WithCriticals(IEnumerable<ApplicationError> criticalErrors) 
    {
        var criticals = criticalErrors.ToList();
        if (criticals.Any(e => e.Severity is not ErrorSeverity.Critical
            || e.HttpStatusCode is null
            || string.IsNullOrWhiteSpace(e.Title)))
            throw new InvalidErrorSeverityException("Попытка добавить Critical ошибку с недопустимым уровнем/без названия/без HttpStatusCode");

        WithErrors(criticals);
        return this;
    }
}