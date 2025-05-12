using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Private.ServicesInterfaces;
using Public.Models.ApplicationErrors;
using Public.Models.CommonModels;

namespace Private.Services.RoleServices;

public class IdentityRoleService : IRoleService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<IdentityRoleService> _logger;
    
    // ReSharper disable once ConvertToPrimaryConstructor
    public IdentityRoleService(RoleManager<IdentityRole> roleManager, ILogger<IdentityRoleService> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<ApplicationExecuteLogicResult<bool>> RoleExistAsync(ApplicationUserRole role)
    {
        _logger.LogInformation("Проверка существования роли {role}", role);
        
        var exist = await _roleManager.RoleExistsAsync(role.ToString());
        if (exist is not true)
            return ApplicationExecuteLogicResult<bool>.Failure(new ApplicationError(
                RoleErrors.RoleNotFound, "Роль не существует",
                $"Роль по указанному названию {role.ToString()} не получилось найти", ErrorSeverity.NotImportant));
        
        _logger.LogInformation("Роль {role} - {exist}", role, exist ? "Да" : "Нет");
        
        return ApplicationExecuteLogicResult<bool>.Success(exist);
    }

    public async Task<ApplicationExecuteLogicResult<ApplicationUserRole>> RoleCreateAsync(ApplicationUserRole role)
    {
        _logger.LogInformation("Создание роли {role}", role);

        var created = await _roleManager.CreateAsync(new IdentityRole(role.ToString()));
        if (created.Succeeded is not true)
        {
            _logger.LogError("Не получилось создать роль, ошибки - {@errors}", created.Errors);
            return ApplicationExecuteLogicResult<ApplicationUserRole>.Failure(new ApplicationError(
                RoleErrors.FailSaveRole, "Роль не создана", 
                "В процессе создания роли возникла неизвестная ошибка", ErrorSeverity.Critical, HttpStatusCode.InternalServerError));
        }

        return ApplicationExecuteLogicResult<ApplicationUserRole>.Success(role);
    }
}