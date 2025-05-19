using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.DtoModels.UserDtos;
using QPDCar.Models.StorageModels;

namespace QPDCar.ServiceInterfaces.UserServices;

public interface IUserService
{
    /// <summary> Создать пользователя </summary>
    Task<ApplicationExecuteResult<ApplicationUserEntity>> CreateAsync(DtoForCreateUser dtoUser);

    /// <summary> Обновить пользователя </summary>
    Task<ApplicationExecuteResult<ApplicationUserEntity>> UpdateAsync(ApplicationUserEntity user);
    
    /// <summary> Получить пользователя по логину или Id </summary>
    Task<ApplicationExecuteResult<ApplicationUserEntity>> ByLoginOrIdAsync(string loginOrId);
    
    /// <summary> Получить всех пользователей </summary>
    Task<ApplicationExecuteResult<List<ApplicationUserEntity>>> AllUsers();
    
    /// <summary> Меняет статус пользователя: заблокирован <-> разблокирован </summary>
    Task<ApplicationExecuteResult<Unit>> BlockOrUnblockAsync(ApplicationUserEntity user);
}