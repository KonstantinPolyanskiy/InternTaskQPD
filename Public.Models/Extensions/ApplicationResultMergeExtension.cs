using Public.Models.CommonModels;

namespace Public.Models.Extensions;

public static class ApplicationExecuteLogicResultExtensions
{
    public static ApplicationExecuteLogicResult<TTarget> Merge<TTarget, TSource>(
        this ApplicationExecuteLogicResult<TTarget> target,
        ApplicationExecuteLogicResult<TSource>?        source)
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

        return target;
    }
}