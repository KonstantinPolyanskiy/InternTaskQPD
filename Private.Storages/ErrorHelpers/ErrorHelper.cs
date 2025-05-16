using System.Net;
using Public.Models.ApplicationErrors;
using Public.Models.CommonModels;

namespace Private.Storages.ErrorHelpers;

internal static class ErrorHelper
{
    /// <summary> Возвращает ошибку, что объект не найден </summary>
    internal static ApplicationError PrepareNotFoundError(string entityName)
        => new ApplicationError(DatabaseErrors.NotFound, $"{entityName} не найден", $"Объект {entityName} не найден или не существует",
            ErrorSeverity.Critical, HttpStatusCode.NotFound);

    /// <summary> Возвращает ошибку, если на слое Storage возникло исключение </summary>
    internal static ApplicationError PrepareStorageException(string entityName) 
        => new ApplicationError(DatabaseErrors.DatabaseException, $"Возникло исключение", $"При работе с {entityName} возникло исключение",
            ErrorSeverity.Critical, HttpStatusCode.InternalServerError);
    
    internal static ApplicationError PrepareNotDeletedError(string entityName)
        => new ApplicationError(DatabaseErrors.NotDeleted, $"{entityName} не удален", $"Объект {entityName} не удален",
            ErrorSeverity.Critical, HttpStatusCode.InternalServerError); 
}