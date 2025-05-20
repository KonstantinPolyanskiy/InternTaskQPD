using Microsoft.Extensions.Logging;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ApplicationResult.Extensions;
using QPDCar.Models.ApplicationModels.ErrorTypes;
using QPDCar.Models.BusinessModels.EmployerModels;
using QPDCar.Models.BusinessModels.UserModels;
using QPDCar.Models.DtoModels.UserDtos;
using QPDCar.Models.StorageModels;
using QPDCar.ServiceInterfaces.UserServices;

namespace QPDCar.UseCases.UseCases.EmployerUseCases.AdminUseCases;

/// <summary> Действия администратора </summary>
public class AdminUseCases(IRoleService roleService, IUserService userService, 
    IAuthService authService, ILogger<AdminUseCases> logger)
{
    /// <summary> Кейс создания пользователя администратором </summary>
    public async Task<ApplicationExecuteResult<UserSummary>> CreateUser(DtoForCreateUser data)
    {
        logger.LogInformation("Создание пользователя администратором, данные - {@data}", data);

        var userResult = await userService.CreateAsync(data);
        if (userResult.IsSuccess is false)
            return ApplicationExecuteResult<UserSummary>.Failure().Merge(userResult);
        var user = userResult.Value!;
        
        var setRolesResult = await roleService.AddRolesToUser(user, data.InitialRoles.ToList());
        if (setRolesResult.IsSuccess is false)
            return ApplicationExecuteResult<UserSummary>.Failure().Merge(setRolesResult);

        var resp = new UserSummary
        {
            Id = user.Id,
            Login = user.UserName!,
            FirstName = user.FirstName,
            LastName = user.LastName!,
            Email = user.Email!,
            EmailConfirmed = user.EmailConfirmed,
            Roles = data.InitialRoles
        };
        
        return ApplicationExecuteResult<UserSummary>.Success(resp);
    }

    /// <summary> Кейс получения пользователя по Id администратором </summary>
    public async Task<ApplicationExecuteResult<UserSummary>> GetUser(Guid id)
    {
        logger.LogInformation("Получение пользователя {id} администратором", id);
        
        var userResult = await userService.ByLoginOrIdAsync(id.ToString());
        if (userResult.IsSuccess is false)
            return ApplicationExecuteResult<UserSummary>.Failure().Merge(userResult);
        var user = userResult.Value!;
        
        var rolesResult = await roleService.GetRolesByUser(user);
        if (rolesResult.IsSuccess is false)
            return ApplicationExecuteResult<UserSummary>.Failure().Merge(rolesResult);
        var roles = rolesResult.Value!;

        var resp = new UserSummary
        {
            Id = user.Id,
            Login = user.UserName!,
            FirstName = user.FirstName,
            LastName = user.LastName!,
            Email = user.Email!,
            EmailConfirmed = user.EmailConfirmed,
            Roles = roles
        };
        
        return ApplicationExecuteResult<UserSummary>.Success(resp);
    }

    /// <summary> Кейс получения всех пользователей администратором </summary>
    public async Task<ApplicationExecuteResult<List<UserSummary>>> GetUsers()
    {
        logger.LogInformation("Получение всех пользователей");
        
        var usersResult = await userService.AllUsers();
        if (usersResult.IsSuccess is false)
            return ApplicationExecuteResult<List<UserSummary>>.Failure().Merge(usersResult);
        var users = usersResult.Value!;

        var warns = new List<ApplicationError>();
        var resp = new List<UserSummary>();

        foreach (var user in users)
        {
            var rolesResult = await roleService.GetRolesByUser(user);
            if (rolesResult.IsSuccess is false)
                warns.Add(new ApplicationError(
                    UserErrors.NotFoundAnyRole, "Ошибка с получением ролей",
                    $"Для пользователя {user.UserName!} возникла проблема с получением ролей",
                    ErrorSeverity.NotImportant));
            var roles = rolesResult.Value!;
            
            resp.Add(new UserSummary
            {
                Id = user.Id,
                Login = user.UserName!,
                FirstName = user.FirstName,
                LastName = user.LastName!,
                Email = user.Email!,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles
            });
        }
        
        return ApplicationExecuteResult<List<UserSummary>>
            .Success(resp)
            .WithWarnings(warns);
    }

    /// <summary> Кейс обновления пользователя администратором </summary>
    public async Task<ApplicationExecuteResult<UserSummary>> UpdateUserAsync(DtoForUpdateUser data)
    {
        logger.LogInformation("Обновление пользователя {id} администратором", data.UserId);
        
        var userResult = await userService.ByLoginOrIdAsync(data.UserId.ToString());
        if (userResult.IsSuccess is false)
            return ApplicationExecuteResult<UserSummary>.Failure().Merge(userResult);
        var user = userResult.Value!;
        
        user.FirstName = data.FirstName;
        user.LastName = data.LastName;
        
        var updatedUserResult = await userService.UpdateAsync(user);
        if (updatedUserResult.IsSuccess is false)
            return ApplicationExecuteResult<UserSummary>.Failure().Merge(updatedUserResult);
        user = updatedUserResult.Value!;
        
        var updatedRolesResult = await RewriteUserRoles(data.NewRoles.ToList(), user);
        if (updatedRolesResult.IsSuccess is false)
            return ApplicationExecuteResult<UserSummary>.Failure().Merge(updatedRolesResult);

        var resp = new UserSummary
        {
            Id = user.Id,
            Login = user.UserName!,
            FirstName = user.FirstName,
            LastName = user.LastName!,
            Email = user.Email!,
            EmailConfirmed = user.EmailConfirmed,
            Roles = data.NewRoles
        };
        
        return ApplicationExecuteResult<UserSummary>.Success(resp);
    }

    /// <summary> Кейс блокировки/разблокировки пользователя и его глобальный логаут администратором </summary>
    public async Task<ApplicationExecuteResult<Unit>> ChangeBlockStatus(Guid id)
    {
        logger.LogInformation("Изменение статуса блокировки пользователя {id}", id);
        
        var userResult = await userService.ByLoginOrIdAsync(id.ToString());
        if (userResult.IsSuccess is false)
            return ApplicationExecuteResult<Unit>.Failure().Merge(userResult);
        var user = userResult.Value!;
        
        var logoutResult = await authService.LogoutUserAsync(user, true, "Бан пользователя");
        if (logoutResult.IsSuccess is false)
            return ApplicationExecuteResult<Unit>.Failure().Merge(logoutResult);

        var blockResult = await userService.BlockOrUnblockAsync(user);
        if (blockResult.IsSuccess is false)
            return ApplicationExecuteResult<Unit>.Failure().Merge(blockResult);

        return ApplicationExecuteResult<Unit>.Success(Unit.Value);
    }

    /// <summary> Кейс глобального логаута пользователя администратором </summary>
    public async Task<ApplicationExecuteResult<Unit>> LogoutByUserId(Guid userId)
    {
        logger.LogInformation("Логаут администратором пользователя {id}", userId);
        
        var userResult = await userService.ByLoginOrIdAsync(userId.ToString());
        if (userResult.IsSuccess is false)
            return ApplicationExecuteResult<Unit>.Failure().Merge(userResult);
        var user = userResult.Value!;
        
        var logoutResult = await authService.LogoutUserAsync(user, true, "Кик администратором");
        if (logoutResult.IsSuccess is false)
            return ApplicationExecuteResult<Unit>.Failure().Merge(logoutResult);
        
        return ApplicationExecuteResult<Unit>.Success(Unit.Value);
    }
    
    private async Task<ApplicationExecuteResult<Unit>> RewriteUserRoles(List<ApplicationRoles> roles, ApplicationUserEntity user)
    {
        var currentRolesResult = await roleService.GetRolesByUser(user);
        if (currentRolesResult.IsSuccess is not true)
            return ApplicationExecuteResult<Unit>.Failure().Merge(currentRolesResult);
        var currentRoles = currentRolesResult.Value!;
        
        
        var toAdd = roles.Except(currentRoles).ToList();
        var toDel = currentRoles.Except(roles).ToList();
        
        if (toDel.Count > 0)
        {
            var delRes = await roleService.RemoveRolesFromUser(user, toDel.ToArray());
            if (!delRes.IsSuccess) return ApplicationExecuteResult<Unit>.Failure().Merge(delRes);
        }

        if (toAdd.Count > 0)
        {
            var addRes = await roleService.AddRolesToUser(user, toAdd);
            if (!addRes.IsSuccess) return ApplicationExecuteResult<Unit>.Failure().Merge(addRes);
        }
        
        return ApplicationExecuteResult<Unit>.Success(Unit.Value);
    }
}