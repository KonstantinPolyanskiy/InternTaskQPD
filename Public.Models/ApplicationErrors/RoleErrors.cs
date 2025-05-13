namespace Public.Models.ApplicationErrors;

/// <summary> Ошибки, связанные с ролями </summary>
public enum RoleErrors
{
    RoleNotFound,
    DontHaveEnoughPermissions,
    FailSaveRole
}