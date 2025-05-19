using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ErrorTypes;
using QPDCar.Models.BusinessModels.EmployerModels;
using QPDCar.Models.StorageModels;
using QPDCar.ServiceInterfaces;
using QPDCar.ServiceInterfaces.UserServices;

namespace QPDCar.Services.Services.UserServices;

public class RoleService(UserManager<ApplicationUserEntity>  userManager, ILogger<RoleService> logger) : IRoleService
{
    public async Task<ApplicationExecuteResult<List<ApplicationRoles>>> GetRolesByUser(ApplicationUserEntity user)
    {
        logger.LogInformation("Попытка получить роли пользователя с id {id}", user.Id);
        
        var roles = await userManager.GetRolesAsync(user);
        if (!roles.Any())
            return ApplicationExecuteResult<List<ApplicationRoles>>.Failure(new ApplicationError(
                UserErrors.NotFoundAnyRole, "Роли не найдены",
                $"Не найдено ни 1 роли для пользователя {user.UserName}", ErrorSeverity.Critical, HttpStatusCode.NotFound));
        
        logger.LogInformation("Для пользователя с id {id} найдено {count} ролей", user.Id, roles.Count);

        return ApplicationExecuteResult<List<ApplicationRoles>>
            .Success(roles.Select(Enum.Parse<ApplicationRoles>).ToList());
    }

    public async Task<ApplicationExecuteResult<Unit>> AddRolesToUser(ApplicationUserEntity user, IReadOnlyList<ApplicationRoles> roles)
    {
        if (!roles.Any())
            return ApplicationExecuteResult<Unit>.Failure(new ApplicationError(UserErrors.NotFoundAnyRole, "Нет ролей для назначения", 
                "Небыли переданы роли, которые необходимо назначить", ErrorSeverity.Critical, HttpStatusCode.NotFound));
        
        var res = await userManager.AddToRolesAsync(user, roles.Select(r => r.ToString()));
        if (res.Succeeded is not true)
        {
            logger.LogError("Ошибка назначения ролей - {@err}", res.Errors);
            return ApplicationExecuteResult<Unit>.Failure(new ApplicationError(UserErrors.UserRolesNotUpdated, "Роли не назначены",
                "При назначении ролей возникла непредвиденная ошибка", ErrorSeverity.Critical, HttpStatusCode.InternalServerError));
        }
        
        return ApplicationExecuteResult<Unit>.Success(Unit.Value);
    }

    public async Task<ApplicationExecuteResult<Unit>> RemoveRolesFromUser(ApplicationUserEntity user, IReadOnlyList<ApplicationRoles> roles)
    {
        if (!roles.Any())
            return ApplicationExecuteResult<Unit>.Failure(new ApplicationError(UserErrors.NotFoundAnyRole, "Нет ролей для удаления", 
                "Небыли переданы роли, которые необходимо снять", ErrorSeverity.Critical, HttpStatusCode.NotFound));
        
        var res = await userManager.RemoveFromRolesAsync(user, roles.Select(r => r.ToString()));
        if (res.Succeeded is not true)
        {
            logger.LogError("Ошибка назначения ролей - {@err}", res.Errors);
            return ApplicationExecuteResult<Unit>.Failure(new ApplicationError(UserErrors.UserRolesNotUpdated, "Роли не сняты",
                "При снятии ролей возникла непредвиденная ошибка", ErrorSeverity.Critical, HttpStatusCode.InternalServerError));
        }

        return ApplicationExecuteResult<Unit>.Success(Unit.Value);
    }
}