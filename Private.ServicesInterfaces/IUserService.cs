using Public.Models.CommonModels;
using Public.Models.DtoModels.UserDtoModels;
using Private.StorageModels;

namespace Private.ServicesInterfaces;

/// <summary> Сервис для управления пользователями и их данными </summary>
public interface IUserService
{
    /// <summary> Создать и сохранить пользователя </summary>
    public Task<ApplicationExecuteLogicResult<ApplicationUserEntity>> CreateUserAsync(DataForCreateUser data);
    
    /// <summary> Получить пользователя по его логину </summary>
    public Task<ApplicationExecuteLogicResult<ApplicationUserEntity>> UserByLoginAsync(string login);
    
    /// <summary> Получить пользователя по его Id </summary>
    public Task<ApplicationExecuteLogicResult<ApplicationUserEntity>> UserByIdAsync(Guid userId);
    
    /// <summary> Получить всех пользователей </summary>
    public Task<ApplicationExecuteLogicResult<List<ApplicationUserEntity>>> UsersAll();
    
    public Task<ApplicationExecuteLogicResult<ApplicationUserEntity>> SaveUserAsync(ApplicationUserEntity user);
    
    /// <summary> Валиден ли пароль для пользователя </summary>
    public Task<ApplicationExecuteLogicResult<bool>> CheckPasswordForUserAsync(ApplicationUserEntity user, string password);
    
    /// <summary> Меняет почту пользователя на подтвержденную </summary>
    public Task<ApplicationExecuteLogicResult<Unit>> SetEmailAddressAsConfirmedAsync(ApplicationUserEntity user);
    
    /// <summary> Обновляет security stamp пользователя, побочно делая все выданные ранее access токены невалидными </summary>
    public Task<ApplicationExecuteLogicResult<Unit>> UpdateUserSecurityStampAsync(ApplicationUserEntity user);
    
    /// <summary> Получить роли пользователя </summary>
    public Task<ApplicationExecuteLogicResult<List<string>>> GetRolesByUser(ApplicationUserEntity user);
    
    /// <summary> Назначить роли пользователю </summary>
    public Task<ApplicationExecuteLogicResult<Unit>> AddRolesToUser(ApplicationUserEntity user, IReadOnlyList<ApplicationUserRole> roles);
    
    /// <summary> Убрать роли пользователя </summary>
    public Task<ApplicationExecuteLogicResult<Unit>> RemoveRolesFromUser(ApplicationUserEntity user, IReadOnlyList<ApplicationUserRole> roles);
}