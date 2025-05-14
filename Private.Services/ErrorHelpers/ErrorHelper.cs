using System.ComponentModel;
using System.Net;
using Public.Models.ApplicationErrors;
using Public.Models.CommonModels;
using Public.Models.Extensions;

namespace Private.Services.ErrorHelpers;

internal static class ErrorHelper
{
    internal static ApplicationExecuteLogicResult<TOut> WrapAllDbErrors<TIn, TOut>(
        Enum errorType,
        ApplicationExecuteLogicResult<TIn> source,
        string? objectNameAndId)
    {
        ApplicationExecuteLogicResult<TOut> result = null!;
        foreach (var dbErrorType in Enum.GetValues<DatabaseErrors>())
        {
            if (source.ContainsError(dbErrorType))
            {
                source.DeleteError(dbErrorType);

                switch (dbErrorType)
                {
                    case DatabaseErrors.NotFound:
                        result = WrapDbExceptionError<TIn, TOut>(errorType, source, objectNameAndId);
                        break;
                    case DatabaseErrors.DatabaseException:
                        result = WrapDbExceptionError<TIn, TOut>(errorType, source, objectNameAndId);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        return result;
    }
    
    internal static ApplicationExecuteLogicResult<TOut> WrapDbExceptionError<TIn, TOut>(
        Enum errorType,
        ApplicationExecuteLogicResult<TIn> source,
        string? objectNameAndId)
    {
        source.DeleteError(DatabaseErrors.DatabaseException);

        var err = new ApplicationError(
            errorType,
            "Ошибка при работе с базой данных",
            $"При работе с {objectNameAndId} в базе данных возникла непредвиденная ошибка",
            ErrorSeverity.Critical,
            HttpStatusCode.InternalServerError);

        return ApplicationExecuteLogicResult<TOut>.Failure(err).Merge(source);
    }

    internal static ApplicationExecuteLogicResult<TOut> WrapNotFoundError<TIn, TOut>(
        Enum errorType,
        ApplicationExecuteLogicResult<TIn> source,
        string? objectNameAndId)
    {
        source.DeleteError(DatabaseErrors.NotFound);
        
        var err = new ApplicationError(
            errorType,
            $"{objectNameAndId} не найден",
            $"Не удалось найти сущность по переданному признаку",
            ErrorSeverity.Critical,
            HttpStatusCode.NotFound);
        
       return ApplicationExecuteLogicResult<TOut>.Failure(err).Merge(source);
    }
}
