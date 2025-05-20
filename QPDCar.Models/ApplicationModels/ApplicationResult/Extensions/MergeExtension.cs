namespace QPDCar.Models.ApplicationModels.ApplicationResult.Extensions;

/// <summary> Мердж двух ApplicationExecuteResult в одну в один с warn и critical ошибками </summary>
public static class ApplicationExecuteLogicResultExtensions
{
    public static ApplicationExecuteResult<TTarget> Merge<TTarget, TSource>(
        this ApplicationExecuteResult<TTarget> target,
        ApplicationExecuteResult<TSource>?        source)
    {
        if (source is null) 
            return target;

        foreach (var warning in source.GetWarnings)
        {
            if (!target.ContainsError(warning.ErrorType))
                target.WithWarning(warning);
        }

        foreach (var critical in source.GetCriticalErrors)
        {
            if (!target.ContainsError(critical.ErrorType))
                target.WithCritical(critical);
        }
        
        if (target.Value == null && source.Value is TTarget v)
            target.Value = v;

        return target;
    }
}