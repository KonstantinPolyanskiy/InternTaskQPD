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
    public Task<ApplicationExecuteLogicResult<ApplicationUserEntity>> ByLoginOrIdAsync(string userIdentifier);
    
    /// <summary> Получить всех пользователей </summary>
    public Task<ApplicationExecuteLogicResult<List<ApplicationUserEntity>>> UsersAll();

    public Task<ApplicationExecuteLogicResult<Unit>> BlockUser(ApplicationUserEntity user);
    public Task<ApplicationExecuteLogicResult<Unit>> UnblockUser(ApplicationUserEntity user);
    
    /// <summary> Обновить пользователя</summary>
    public Task<ApplicationExecuteLogicResult<ApplicationUserEntity>> UpdateAsync(ApplicationUserEntity user);
    
}