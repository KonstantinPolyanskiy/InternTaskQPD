using Microsoft.Extensions.Logging;
using Private.ServicesInterfaces;
using Private.StorageModels;
using Public.Models.BusinessModels.UserModels;
using Public.Models.CommonModels;
using Public.Models.DtoModels.UserDtoModels;
using Public.Models.Extensions;

namespace Public.UseCase.UseCases.AdminUseCases;

public class AdminUseCases
{
    private readonly ILogger<AdminUseCases> _logger;
    private readonly IUserService _userService;

    public AdminUseCases(ILogger<AdminUseCases> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }
    
    /// <summary> Создание пользователя </summary>
    public async Task<ApplicationExecuteLogicResult<UserSummary>> CreateUserAsync(DataForCreateUser data)
    {
        _logger.LogInformation("Создание пользователя администратором");
        _logger.LogDebug("Данные для создания - {@data}", data);
        
        var user = await _userService.CreateUserAsync(data);
        if (user.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserSummary>.Failure().Merge(user);
        
        var setRoles = await _userService.AddRolesToUser(user.Value!, (data.InitialRoles as IReadOnlyList<ApplicationUserRole>)!);
        if (setRoles.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserSummary>.Failure().Merge(setRoles);
        
        var currentRoles = await _userService.GetRolesByUser(user.Value!);
        if (currentRoles.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserSummary>.Failure().Merge(currentRoles);

        var result = new UserSummary
        {
            Id = user.Value!.Id,
            Login = user.Value.UserName!,
            Email = user.Value.Email!,
            EmailConfirmed = user.Value.EmailConfirmed,
            Roles = currentRoles.Value!.Select(r => Enum.Parse<ApplicationUserRole>(r, ignoreCase: true)).ToList()
        };

        return ApplicationExecuteLogicResult<UserSummary>.Success(result);
    }

    /// <summary> Получить пользователя по Id </summary>
    public async Task<ApplicationExecuteLogicResult<UserSummary>> GetUserByIdAsync(Guid id)
    {
        _logger.LogInformation("Получение данных пользователя с id - {@id}", id);

        var user = await _userService.UserByIdAsync(id);
        if (user.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserSummary>.Failure().Merge(user);
        
        var roles = await _userService.GetRolesByUser(user.Value!);
        if (roles.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserSummary>.Failure().Merge(roles);

        var result = new UserSummary
        {
            Id = user.Value!.Id,
            Login = user.Value!.UserName!,
            Email = user.Value.Email!,
            EmailConfirmed = user.Value.EmailConfirmed,
            Roles = roles.Value!.Select(r => Enum.Parse<ApplicationUserRole>(r, ignoreCase: true)).ToList()
        };
        
        return ApplicationExecuteLogicResult<UserSummary>.Success(result);
    }

    /// <summary> Получить всех пользователей </summary>
    public async Task<ApplicationExecuteLogicResult<List<UserSummary>>> GetAllUsers()
    {
        _logger.LogInformation("Получение всех пользователей");

        var users = await _userService.UsersAll();
        if (users.IsSuccess is not true)
            return ApplicationExecuteLogicResult<List<UserSummary>>.Failure().Merge(users);
        
        var result = new List<UserSummary>(users.Value!.Count);

        foreach (var user in users.Value!)
        {
            var summary = new UserSummary
            {
                Id = user.Id,
                Login = user.UserName!,
                Email = user.Email!,
                EmailConfirmed = user.EmailConfirmed
            };

            var roles = await _userService.GetRolesByUser(user);
            if (roles.IsSuccess is not true)
                return ApplicationExecuteLogicResult<List<UserSummary>>.Failure().Merge(roles);
            
            summary.Roles = roles.Value!.Select(r => Enum.Parse<ApplicationUserRole>(r, ignoreCase: true)).ToList();
            
            result.Add(summary);
        }
        
        return ApplicationExecuteLogicResult<List<UserSummary>>.Success(result);
    }

    /// <summary> Обновить пользователя </summary>
    public async Task<ApplicationExecuteLogicResult<UserSummary>> UpdateUserAsync(DataForUpdateUser data)
    {
        var user = await _userService.UserByIdAsync(data.UserId);
        if (user.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserSummary>.Failure().Merge(user);
        
        var u = user.Value!;
        u.FirstName = data.FirstName;
        u.LastName = data.LastName;
        
        var newUser = await _userService.SaveUserAsync(u);
        var updatedRoles = await RewriteUserRoles((List<ApplicationUserRole>)data.NewRoles, newUser.Value!);
        if (newUser.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserSummary>.Failure().Merge(updatedRoles);

        var result = new UserSummary
        {
            Id = newUser.Value!.Id,
            Login = newUser.Value!.UserName!,
            Email = newUser.Value!.Email!,
            EmailConfirmed = newUser.Value!.EmailConfirmed,
            Roles = data.NewRoles
        };
        
        return ApplicationExecuteLogicResult<UserSummary>.Success(result);
    }

    private async Task<ApplicationExecuteLogicResult<Unit>> RewriteUserRoles(List<ApplicationUserRole> roles, ApplicationUserEntity user)
    {
        var current = await _userService.GetRolesByUser(user);
        if (current.IsSuccess is not true)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(current);
        
        var c = current.Value!.Select(r => Enum.Parse<ApplicationUserRole>(r, ignoreCase: true)).ToList();
        
        
        var toAdd = roles.Except(current.Value!.Select(r => Enum.Parse<ApplicationUserRole>(r, ignoreCase:true))).ToList();
        var toDel = c.Except(roles).ToList();
        
        if (toDel.Count > 0)
        {
            var delRes = await _userService.RemoveRolesFromUser(user, toDel.ToArray());
            if (!delRes.IsSuccess) return ApplicationExecuteLogicResult<Unit>.Failure().Merge(delRes);
        }

        if (toAdd.Count > 0)
        {
            var addRes = await _userService.AddRolesToUser(user, toAdd);
            if (!addRes.IsSuccess) return ApplicationExecuteLogicResult<Unit>.Failure().Merge(addRes);
        }
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }
}