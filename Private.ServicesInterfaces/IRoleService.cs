using Private.StorageModels;
using Public.Models.CommonModels;

namespace Private.ServicesInterfaces;

/// <summary> Сервис для управления ролями пользователей </summary>
public interface IRoleService
{
    /// <summary> Получить роли пользователя </summary>
    public Task<ApplicationExecuteLogicResult<List<ApplicationUserRole>>> GetRolesByUser(ApplicationUserEntity user);
    
    /// <summary> Назначить роли пользователю </summary>
    public Task<ApplicationExecuteLogicResult<Unit>> AddRolesToUser(ApplicationUserEntity user, IReadOnlyList<ApplicationUserRole> roles);
    
    /// <summary> Убрать роли пользователя </summary>
    public Task<ApplicationExecuteLogicResult<Unit>> RemoveRolesFromUser(ApplicationUserEntity user, IReadOnlyList<ApplicationUserRole> roles);
}