using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Private.ServicesInterfaces;
using Private.StorageModels;
using Public.Models.ApplicationErrors;
using Public.Models.CommonModels;

namespace Private.Services.RoleServices;

public class IdentityRoleService : IRoleService
{
    private readonly UserManager<ApplicationUserEntity> _userManager;
    private readonly ILogger<IdentityRoleService> _logger;
    
    // ReSharper disable once ConvertToPrimaryConstructor
    public IdentityRoleService(UserManager<ApplicationUserEntity> userManager, ILogger<IdentityRoleService> logger)
    {
        _userManager = userManager;
        
        _logger = logger;
    }

    public async Task<ApplicationExecuteLogicResult<List<ApplicationUserRole>>> GetRolesByUser(ApplicationUserEntity user)
    {
        _logger.LogInformation("Попытка получить роли пользователя с id {id}", user.Id);
        
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Any())
            return ApplicationExecuteLogicResult<List<ApplicationUserRole>>.Failure(new ApplicationError(
                UserErrors.NotFoundAnyRoleForUser, "Роли не найдены",
                $"Не найдено ни 1 роли для пользователя {user.UserName}", ErrorSeverity.Critical, HttpStatusCode.NotFound));
        
        _logger.LogInformation("Для пользователя с id {id} найдено {count} ролей", user.Id, roles.Count);

        return ApplicationExecuteLogicResult<List<ApplicationUserRole>>
            .Success(roles.Select(Enum.Parse<ApplicationUserRole>).ToList());
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> AddRolesToUser(ApplicationUserEntity user, IReadOnlyList<ApplicationUserRole> roles)
    {
        if (!roles.Any())
            return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(UserErrors.NotFoundAnyRoleForUser, "Нет ролей для назначения", 
                "Небыли переданы роли, которые необходимо назначить", ErrorSeverity.Critical, HttpStatusCode.NotFound));
        
        var res = await _userManager.AddToRolesAsync(user, roles.Select(r => r.ToString()));
        if (res.Succeeded is not true)
        {
            _logger.LogError("Ошибка назначения ролей - {@err}", res.Errors);
            return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(UserErrors.FailSaveUser, "Роли не назначены",
                "При назначении ролей возникла непредвиденная ошибка", ErrorSeverity.Critical, HttpStatusCode.InternalServerError));
        }
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> RemoveRolesFromUser(ApplicationUserEntity user, IReadOnlyList<ApplicationUserRole> roles)
    {
        if (!roles.Any())
            return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(UserErrors.NotFoundAnyRoleForUser, "Нет ролей для удаления", 
                "Небыли переданы роли, которые необходимо снять", ErrorSeverity.Critical, HttpStatusCode.NotFound));
        
        var res = await _userManager.RemoveFromRolesAsync(user, roles.Select(r => r.ToString()));
        if (res.Succeeded is not true)
        {
            _logger.LogError("Ошибка назначения ролей - {@err}", res.Errors);
            return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(UserErrors.FailSaveUser, "Роли не сняты",
                "При снятии ролей возникла непредвиденная ошибка", ErrorSeverity.Critical, HttpStatusCode.InternalServerError));
        }
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }
}