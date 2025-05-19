using System.Net;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ApplicationResult.Extensions;
using QPDCar.Models.StorageModels.ErrorTypes;

namespace QPDCar.Services.ErrorHelpers;

public static class DbErrorHelper
{
    internal static ApplicationExecuteResult<TOut> WrapAllDbErrors<TIn, TOut>(
        Enum errorType,
        ApplicationExecuteResult<TIn> source,
        string? objectNameAndId)
    {
        ApplicationExecuteResult<TOut> result = null!;
        foreach (var dbErrorType in Enum.GetValues<DatabaseErrors>())
        {
            if (source.ContainsError(dbErrorType))
            {
                source.DeleteError(dbErrorType);

                switch (dbErrorType)
                {
                    case DatabaseErrors.EntityNotDeleted:
                        result = WrapNotDeleteError<TIn, TOut>(errorType, source, objectNameAndId);
                        break;
                    case DatabaseErrors.EntityNotSaved:
                        result = WrapNotCreatedError<TIn, TOut>(errorType, source, objectNameAndId);
                        break;
                    case DatabaseErrors.EntityNotUpdated:
                        result = WrapNotUpdatedError<TIn, TOut>(errorType, source, objectNameAndId);
                        break;
                    case DatabaseErrors.EntityByIdNotFound:
                        result = WrapNotFoundError<TIn, TOut>(errorType, source, objectNameAndId);
                        break;
                    case DatabaseErrors.EntityByParamsNotFound:
                        result = WrapNotFoundError<TIn, TOut>(errorType, source, objectNameAndId);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        return result;
    }
    
    internal static ApplicationExecuteResult<TOut> WrapNotFoundError<TIn, TOut>(
        Enum errorType,
        ApplicationExecuteResult<TIn> source,
        string? objectNameAndId)
    {
        source.DeleteError(DatabaseErrors.EntityByIdNotFound);
        
        var err = new ApplicationError(
            errorType,
            $"{objectNameAndId} не найден",
            $"Не удалось найти сущность по переданному признаку",
            ErrorSeverity.Critical,
            HttpStatusCode.NotFound);
        
        return ApplicationExecuteResult<TOut>.Failure(err).Merge(source);
    }
    
    internal static ApplicationExecuteResult<TOut> WrapNotDeleteError<TIn, TOut>(
        Enum errorType,
        ApplicationExecuteResult<TIn> source,
        string? objectNameAndId)
    {
        source.DeleteError(DatabaseErrors.EntityNotDeleted);
        
        var err = new ApplicationError(
            errorType,
            $"{objectNameAndId} не удален",
            $"Не удалось удалить сущность",
            ErrorSeverity.Critical,
            HttpStatusCode.NotFound);
        
        return ApplicationExecuteResult<TOut>.Failure(err).Merge(source);
    }
    
    internal static ApplicationExecuteResult<TOut> WrapNotUpdatedError<TIn, TOut>(
        Enum errorType,
        ApplicationExecuteResult<TIn> source,
        string? objectNameAndId)
    {
        source.DeleteError(DatabaseErrors.EntityNotUpdated);
        
        var err = new ApplicationError(
            errorType,
            $"{objectNameAndId} не обновлен",
            $"Не удалось обновить сущность",
            ErrorSeverity.Critical,
            HttpStatusCode.NotFound);
        
        return ApplicationExecuteResult<TOut>.Failure(err).Merge(source);
    }
    
    internal static ApplicationExecuteResult<TOut> WrapNotCreatedError<TIn, TOut>(
        Enum errorType,
        ApplicationExecuteResult<TIn> source,
        string? objectNameAndId)
    {
        source.DeleteError(DatabaseErrors.EntityNotSaved);
        
        var err = new ApplicationError(
            errorType,
            $"{objectNameAndId} не создана",
            $"Не удалось создать сущность",
            ErrorSeverity.Critical,
            HttpStatusCode.NotFound);
        
        return ApplicationExecuteResult<TOut>.Failure(err).Merge(source);
    }
}