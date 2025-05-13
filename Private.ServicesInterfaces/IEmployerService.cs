using Public.Models.BusinessModels.CarModels;
using Public.Models.BusinessModels.UserModels;
using Public.Models.CommonModels;

namespace Private.ServicesInterfaces;

public interface IEmployerService
{
    /// <summary> Возвращает менеджера по его Id. Если пользователь найден, но не с ролью менеджера - ошибка </summary>
    public Task<ApplicationExecuteLogicResult<DomainEmployer>> ManagerByUserIdAsync(Guid userId);
}