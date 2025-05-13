using Private.StorageModels;
using Public.Models.ApplicationErrors;
using Public.Models.BusinessModels.CarModels;
using Public.Models.BusinessModels.PhotoModels;
using Public.Models.CommonModels;
using Public.Models.DtoModels.CarDtoModels;

namespace Private.ServicesInterfaces;

public interface ICarService
{
    public Task<ApplicationExecuteLogicResult<DomainCar>> CreateCarAsync(DtoForSaveCar data);
    public Task<ApplicationExecuteLogicResult<DomainCar>> SetPhotoToCarAsync(DomainCar car, DomainPhoto photo);
    public Task<ApplicationExecuteLogicResult<DomainCar>> GetCarByIdAsync(int id);
    public Task<ApplicationExecuteLogicResult<DomainCar>> UpdateCarAsync(DtoForUpdateCar carDto);
    public Task<ApplicationExecuteLogicResult<Unit>> DeleteCarByIdAsync(int id);
    public Task<ApplicationExecuteLogicResult<DomainCarsPage>> GetCarsAsync(DtoForSearchCars data);
}