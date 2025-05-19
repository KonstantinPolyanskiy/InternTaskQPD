using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.BusinessModels.EmployerModels;
using QPDCar.Models.StorageModels;

namespace QPDCar.ServiceInterfaces.UserServices;

/// <summary> Сервис для управления ролями пользователей </summary>
public interface IRoleService
{
    /// <summary> Получить роли пользователя </summary>
    Task<ApplicationExecuteResult<List<ApplicationRoles>>> GetRolesByUser(ApplicationUserEntity user);
    
    /// <summary> Назначить роли пользователю </summary>
    Task<ApplicationExecuteResult<Unit>> AddRolesToUser(ApplicationUserEntity user, IReadOnlyList<ApplicationRoles> roles);
    
    /// <summary> Убрать роли пользователя </summary>
    Task<ApplicationExecuteResult<Unit>> RemoveRolesFromUser(ApplicationUserEntity user, IReadOnlyList<ApplicationRoles> roles);
}