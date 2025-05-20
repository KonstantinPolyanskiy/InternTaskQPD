using System.Net;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.StorageModels.ErrorTypes;

namespace QPDCar.Repositories.ErrorHelpers;

public static class ErrorHelper
{
    /// <summary> Возвращает ошибку, что объект не найден </summary>
    internal static ApplicationError PrepareNotFoundErrorSingle(string entityName)
        => new ApplicationError(DatabaseErrors.EntityByIdNotFound, $"{entityName} не найден", $"Объект {entityName} не найден или не существует",
            ErrorSeverity.Critical, HttpStatusCode.NotFound);
    
    /// <summary> Возвращает ошибку, что объекты не найден </summary>
    internal static ApplicationError PrepareNotFoundErrorMany(string entityName)
        => new ApplicationError(DatabaseErrors.EntityByParamsNotFound, $"Множество {entityName} не найдены", $"Ни 1 объект по параметрам типа {entityName} не найден или не существует",
            ErrorSeverity.Critical, HttpStatusCode.NotFound);
    
    /// <summary> Возвращает ошибку, что объект не сохранен </summary>
    internal static ApplicationError PrepareNotSavedError(string entityName)
        => new ApplicationError(DatabaseErrors.EntityNotSaved, $"{entityName} не сохранен", $"Объект {entityName} не сохранен",
            ErrorSeverity.Critical, HttpStatusCode.InternalServerError);
    
    /// <summary> Возвращает ошибку, что объект не удален </summary>
    internal static ApplicationError PrepareNotDeletedError(string entityName)
        => new ApplicationError(DatabaseErrors.EntityNotDeleted, $"{entityName} не удален", $"Объект {entityName} не удален",
            ErrorSeverity.Critical, HttpStatusCode.InternalServerError); 
    
    /// <summary> Возвращает ошибку, что объект не обновлен </summary>
    internal static ApplicationError PrepareNotUpdatedError(string entityName)
        => new ApplicationError(DatabaseErrors.EntityNotUpdated, $"{entityName} не обновлен", $"Объект {entityName} не обновлен",
            ErrorSeverity.Critical, HttpStatusCode.InternalServerError); 
}