using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.DtoModels.CarDtos;
using QPDCar.Models.StorageModels;

namespace QPDCar.Services.Repositories;

public interface ICarRepository
{
    Task<ApplicationExecuteResult<CarEntity>> SaveAsync(CarEntity car);
    Task<ApplicationExecuteResult<CarEntity>> RewriteAsync(CarEntity car);
    Task<ApplicationExecuteResult<Unit>> DeleteAsync(int id);
    
    Task<ApplicationExecuteResult<CarEntity>> ByIdAsync(int id);
    Task<ApplicationExecuteResult<CarEntityPage>> ByParamsAsync(DtoForSearchCars parameters);
}