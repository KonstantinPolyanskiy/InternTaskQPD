using Public.Models.CommonModels;

namespace Public.UseCase.Services;

/// <summary> Сервис для управления ролями пользователей </summary>
public interface IRoleService
{
    /// <summary> Существует ли роль </summary>
    public Task<ApplicationExecuteLogicResult<bool>> RoleExistAsync(ApplicationUserRole role);
    
    /// <summary> Создать указанную роль </summary>
    public Task<ApplicationExecuteLogicResult<ApplicationUserRole>> RoleCreateAsync(ApplicationUserRole role);
}