using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.BusinessModels.EmployerModels;
using QPDCar.Models.BusinessModels.UserModels;
using QPDCar.Models.DtoModels.UserDtos;
using QPDCar.Models.StorageModels;

namespace QPDCar.UseCases.UseCases.EmployerUseCases.AdminUseCases;

/// <summary> Действия администратора </summary>
public class AdminUseCases
{
    /// <summary> Кейс создания пользователя администратором </summary>
    public async Task<ApplicationExecuteResult<UserSummary>> CreateUser(DtoForCreateUser data)
    {
        
    }

    /// <summary> Кейс получения пользователя по Id администратором </summary>
    public async Task<ApplicationExecuteResult<UserSummary>> GetUser(Guid id)
    {
        
    }

    /// <summary> Кейс получения всех пользователей администратором </summary>
    public async Task<ApplicationExecuteResult<List<UserSummary>>> GetUsers()
    {
        
    }

    /// <summary> Кейс обновления пользователя администратором </summary>
    public async Task<ApplicationExecuteResult<UserSummary>> UpdateUserAsync(DtoForUpdateUser data)
    {
        
    }

    /// <summary> Кейс блокировки/разблокировки пользователя и его глобальный логаут администратором </summary>
    public async Task<ApplicationExecuteResult<Unit>> ChangeBlockStatus(Guid id)
    {
        
    }

    /// <summary> Кейс глобального логаута пользователя администратором </summary>
    public async Task<ApplicationExecuteResult<Unit>> LogoutByUserId(Guid userId)
    {
        
    }
    
    private async Task<ApplicationExecuteResult<Unit>> RewriteUserRoles(List<ApplicationRoles> roles, ApplicationUserEntity user)
    {
        var currentRolesResult = await roleService.GetRolesByUser(user);
        if (currentRolesResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(currentRolesResult);
        var currentRoles = currentRolesResult.Value!;
        
        
        var toAdd = roles.Except(currentRoles).ToList();
        var toDel = currentRoles.Except(roles).ToList();
        
        if (toDel.Count > 0)
        {
            var delRes = await roleService.RemoveRolesFromUser(user, toDel.ToArray());
            if (!delRes.IsSuccess) return ApplicationExecuteLogicResult<Unit>.Failure().Merge(delRes);
        }

        if (toAdd.Count > 0)
        {
            var addRes = await roleService.AddRolesToUser(user, toAdd);
            if (!addRes.IsSuccess) return ApplicationExecuteLogicResult<Unit>.Failure().Merge(addRes);
        }
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }

}