using Microsoft.Extensions.Logging;
using Private.ServicesInterfaces;
using Private.StorageModels;
using Public.Models.BusinessModels.UserModels;
using Public.Models.CommonModels;
using Public.Models.DtoModels.UserDtoModels;
using Public.Models.Extensions;
using Public.UseCase.UseCases.UserUseCases;

namespace Public.UseCase.UseCases.AdminUseCases;

public class AdminUseCases
{
    private readonly IRoleService _roleService;
    private readonly IUserService _userService;
    private readonly UserUseCase _userUseCase;

    private readonly ILogger<AdminUseCases> _logger;

    public AdminUseCases(IUserService userService, IRoleService roleService,  
        UserUseCase userUseCase, ILogger<AdminUseCases> logger)
    {
        _userService = userService;
        _roleService = roleService;
        _userUseCase = userUseCase;
        
        _logger = logger;
    }
    
    /// <summary> Создание пользователя </summary>
    public async Task<ApplicationExecuteLogicResult<UserSummary>> CreateUserAsync(DataForCreateUser data)
    {
        _logger.LogInformation("Создание пользователя администратором");
        _logger.LogDebug("Данные для создания - {@data}", data);
        
        // Сохраняем пользователя
        var userResult = await _userService.CreateUserAsync(data);
        if (userResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserSummary>.Failure().Merge(userResult);
        var user = userResult.Value!;
        
        // Назначаем ему роли
        var setRolesResult = await _roleService.AddRolesToUser(user, (data.InitialRoles as IReadOnlyList<ApplicationUserRole>)!);
        if (setRolesResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserSummary>.Failure().Merge(setRolesResult);
        
        // Получаем роли созданного пользователя
        var currentRolesResult = await _roleService.GetRolesByUser(user);
        if (currentRolesResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserSummary>.Failure().Merge(currentRolesResult);
        var roles = currentRolesResult.Value!;
        
        return ApplicationExecuteLogicResult<UserSummary>
            .Success(new UserSummary
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

    /// <summary> Получить пользователя по Id </summary>
    public async Task<ApplicationExecuteLogicResult<UserSummary>> GetUserByIdAsync(Guid id)
    {
        _logger.LogInformation("Получение данных пользователя с id - {@id}", id);

        // Получаем пользователя
        var userResult = await _userService.UserByLoginOrIdAsync(id.ToString());
        if (userResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserSummary>.Failure().Merge(userResult);
        var user = userResult.Value!;
        
        // Получаем его роли
        var rolesResult = await _roleService.GetRolesByUser(userResult.Value!);
        if (rolesResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserSummary>.Failure().Merge(rolesResult);
        var roles = rolesResult.Value!;

        return ApplicationExecuteLogicResult<UserSummary>.Success(new UserSummary
        {
            Id = user.Id,
            Login = user.UserName!,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName!,
            EmailConfirmed = user.EmailConfirmed,
            Roles = roles
        });
    }

    /// <summary> Получить всех пользователей </summary>
    public async Task<ApplicationExecuteLogicResult<List<UserSummary>>> GetAllUsers()
    {
        _logger.LogInformation("Получение всех пользователей");

        // Получаем всех пользователей
        var usersResult = await _userService.UsersAll();
        if (usersResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<List<UserSummary>>.Failure().Merge(usersResult);
        var users = usersResult.Value!;
        
        var result = new List<UserSummary>(users.Count);
        
        // Подготавливаем ответ
        foreach (var user in users)
        {
            var summary = new UserSummary
            {
                Id = user.Id,
                Login = user.UserName!,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName!,
                EmailConfirmed = user.EmailConfirmed
            };
            
            // Получаем роли для каждого пользователя
            var rolesResult = await _roleService.GetRolesByUser(user);
            if (rolesResult.IsSuccess is not true)
                return ApplicationExecuteLogicResult<List<UserSummary>>.Failure().Merge(rolesResult);
            var roles = rolesResult.Value!;

            summary.Roles = roles;
            
            result.Add(summary);
        }
        
        return ApplicationExecuteLogicResult<List<UserSummary>>.Success(result);
    }

    /// <summary> Обновить пользователя </summary>
    public async Task<ApplicationExecuteLogicResult<UserSummary>> UpdateUserAsync(DataForUpdateUser data)
    {
        var userResult = await _userService.UserByLoginOrIdAsync(data.UserId.ToString());
        if (userResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserSummary>.Failure().Merge(userResult);
        var user = userResult.Value!;
        
        user.FirstName = data.FirstName;
        user.LastName = data.LastName;
        
        var newUserResult = await _userService.SaveUserAsync(user);
        if (newUserResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserSummary>.Failure().Merge(newUserResult);
        var newUser = userResult.Value!;
        
        var updatedRolesResult = await RewriteUserRoles((List<ApplicationUserRole>)data.NewRoles, newUserResult.Value!);
        if (updatedRolesResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserSummary>.Failure().Merge(updatedRolesResult);

        var result = new UserSummary
        {
            Id = newUser.Id,
            Login = newUser.UserName!,
            Email = newUser.Email!,
            FirstName = newUser.FirstName,
            LastName = newUser.LastName!,
            EmailConfirmed = newUser.EmailConfirmed,
            Roles = data.NewRoles
        };
        
        return ApplicationExecuteLogicResult<UserSummary>.Success(result);
    }
    
    /// <summary> Удалить пользователя и глобально его разлогинить </summary>
    public async Task<ApplicationExecuteLogicResult<Unit>> DeleteUserByIdAsync(Guid id)
    {
        // Находим пользователя
        var userResult = await _userService.UserByLoginOrIdAsync(id.ToString());
        if (userResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(userResult);
        var user = userResult.Value!;
        
        // Разлогиниваем
        var logoutResult = await _userUseCase.LogoutGlobally(user);
        if (logoutResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(logoutResult);
        
        // Удаляем пользователя
        var deleteResult = await _userService.DeleteUserByLoginOrIdAsync(id.ToString());
        if (deleteResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(deleteResult);
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }
    
    /// <summary> Разлогинить пользователя глобально по Id </summary>
    public async Task<ApplicationExecuteLogicResult<Unit>> LogoutByUserId(string userId)
    {
        var userResult = await _userService.UserByLoginOrIdAsync(userId);
        if (userResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(userResult);
        var user = userResult.Value!;
        
        var logoutResult = await _userUseCase.LogoutGlobally(user);
        if (logoutResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(logoutResult);
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }

    private async Task<ApplicationExecuteLogicResult<Unit>> RewriteUserRoles(List<ApplicationUserRole> roles, ApplicationUserEntity user)
    {
        var currentRolesResult = await _roleService.GetRolesByUser(user);
        if (currentRolesResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(currentRolesResult);
        var currentRoles = currentRolesResult.Value!;
        
        
        var toAdd = roles.Except(currentRoles).ToList();
        var toDel = currentRoles.Except(roles).ToList();
        
        if (toDel.Count > 0)
        {
            var delRes = await _roleService.RemoveRolesFromUser(user, toDel.ToArray());
            if (!delRes.IsSuccess) return ApplicationExecuteLogicResult<Unit>.Failure().Merge(delRes);
        }

        if (toAdd.Count > 0)
        {
            var addRes = await _roleService.AddRolesToUser(user, toAdd);
            if (!addRes.IsSuccess) return ApplicationExecuteLogicResult<Unit>.Failure().Merge(addRes);
        }
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }
}