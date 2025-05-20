using QPDCar.Models.ApplicationModels.ApplicationResult;

namespace QPDCar.ServiceInterfaces;

public interface ICartService
{
    Task<ApplicationExecuteResult<Unit>> AddCarAsync(Guid userId, int carId);
    Task<ApplicationExecuteResult<Unit>> RemoveCarAsync(Guid userId, int carId);
    
    Task<ApplicationExecuteResult<List<int>>> CarsAsync(Guid userId);
    
    Task<ApplicationExecuteResult<Unit>> CheckoutAsync(Guid userId);
}