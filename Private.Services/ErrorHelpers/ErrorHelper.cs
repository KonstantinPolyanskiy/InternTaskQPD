using System.ComponentModel;
using System.Net;
using Public.Models.ApplicationErrors;
using Public.Models.CommonModels;
using Public.Models.Extensions;

namespace Private.Services.ErrorHelpers;

internal static class ErrorHelper
{
    internal static ApplicationExecuteLogicResult<TOut> WrapDbExceptionError<TIn, TOut>(
        Enum errorType,
        ApplicationExecuteLogicResult<TIn> source)
    {
        source.DeleteError(DatabaseErrors.DatabaseException);

        var err = new ApplicationError(
            errorType,
            "Неизвестная ошибка",
            "При выполнении запроса к базе данных произошла неизвестная ошибка.",
            ErrorSeverity.Critical,
            HttpStatusCode.InternalServerError);

        return ApplicationExecuteLogicResult<TOut>.Failure(err).Merge(source);
    }

    internal static ApplicationExecuteLogicResult<TOut> WrapNotFoundError<TIn, TOut>(
        Enum errorType,
        string objectName,
        string objectId,
        ApplicationExecuteLogicResult<TIn> source)
    {
        source.DeleteError(DatabaseErrors.NotFound);
        
        var err = new ApplicationError(
            errorType,
            $"Не получилось найти {objectName}",
            $"Не удалось найти {objectId} с указанным признаком - {objectId}",
            ErrorSeverity.Critical,
            HttpStatusCode.NotFound);
        
       return ApplicationExecuteLogicResult<TOut>.Failure(err).Merge(source);
    }
}
