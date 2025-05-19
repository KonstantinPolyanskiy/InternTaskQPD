using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.BusinessModels.EmployerModels;

namespace QPDCar.ServiceInterfaces.UserServices;

public interface IEmployerService
{
    /// <summary> Возвращает привязанного к машине менеджера </summary>
    Task<ApplicationExecuteResult<DomainEmployer>> ManagerByCarId(int carId);
}