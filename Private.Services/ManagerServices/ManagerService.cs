using System.Net;
using Microsoft.Extensions.Logging;
using Private.ServicesInterfaces;
using Public.Models.ApplicationErrors;
using Public.Models.BusinessModels.UserModels;
using Public.Models.CommonModels;
using Public.Models.Extensions;

namespace Private.Services.ManagerServices;

public class EmployerService : IEmployerService
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    
    private readonly ILogger<EmployerService> _logger;

    // ReSharper disable once ConvertToPrimaryConstructor
    public EmployerService(IUserService userService, IRoleService roleService, ILogger<EmployerService> logger)
    {
        _userService = userService;
        _roleService = roleService;
        
        _logger = logger;
    }

    public async Task<ApplicationExecuteLogicResult<DomainEmployer>> ManagerByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Попытка найти менеджера UserId - {@UserId}", userId);
        
        var userResult = await _userService.ByLoginOrIdAsync(userId.ToString());
        if (userResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<DomainEmployer>.Failure(new ApplicationError(
                UserErrors.UserNotFound, "Менеджер не найден",
                $"Менеджер с UserId {userId} не найден", ErrorSeverity.Critical, HttpStatusCode.Forbidden));
        var user = userResult.Value!;
        
        var rolesResult = await _roleService.GetRolesByUser(user);
        if (rolesResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<DomainEmployer>.Failure().Merge(rolesResult);
        var roles = rolesResult.Value!;
        
        if (roles.Contains(ApplicationUserRole.Manager) is not true)
            return ApplicationExecuteLogicResult<DomainEmployer>.Failure(new ApplicationError(
                UserErrors.ForbiddenRole, "Нет роли",
                $"Пользователь по id {user.Id} найден, но у него нет роли {ApplicationUserRole.Manager.ToString()}",
                ErrorSeverity.Critical, HttpStatusCode.Forbidden));
        
        return ApplicationExecuteLogicResult<DomainEmployer>.Success(new DomainEmployer
        {
            Id = Guid.Parse(user.Id),
            FirstName = user.FirstName,
            LastName = user.LastName!, 
            Login = user.UserName!,
            Email = user.Email!,
        }); 
    }
}