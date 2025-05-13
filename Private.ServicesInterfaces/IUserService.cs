using Public.Models.CommonModels;
using Public.Models.DtoModels.UserDtoModels;
using Private.StorageModels;

namespace Private.ServicesInterfaces;

/// <summary> Сервис для управления пользователями и их данными </summary>
public interface IUserService
{
    /// <summary> Создать и сохранить пользователя </summary>
    public Task<ApplicationExecuteLogicResult<ApplicationUserEntity>> CreateUserAsync(DataForCreateUser data);
    
    /// <summary> Получить пользователя по его Id или UserName (логину) </summary>
    public Task<ApplicationExecuteLogicResult<ApplicationUserEntity>> UserByLoginOrIdAsync(string userIdentifier);
    
    /// <summary> Получить всех пользователей </summary>
    public Task<ApplicationExecuteLogicResult<List<ApplicationUserEntity>>> UsersAll();
    
    public Task<ApplicationExecuteLogicResult<Unit>> DeleteUserByLoginOrIdAsync(string userIdentifier);
    
    /// <summary> Обновить пользователя</summary>
    public Task<ApplicationExecuteLogicResult<ApplicationUserEntity>> SaveUserAsync(ApplicationUserEntity user);
    
    /// <summary> Валиден ли пароль для пользователя </summary>
    public Task<ApplicationExecuteLogicResult<bool>> CheckPasswordForUserAsync(ApplicationUserEntity user, string password);
    
    /// <summary> Меняет почту пользователя на подтвержденную </summary>
    public Task<ApplicationExecuteLogicResult<Unit>> SetEmailAddressAsConfirmedAsync(ApplicationUserEntity user);
    
    /// <summary> Обновляет security stamp пользователя, побочно делая все выданные ранее access токены невалидными </summary>
    public Task<ApplicationExecuteLogicResult<Unit>> UpdateUserSecurityStampAsync(ApplicationUserEntity user);
}