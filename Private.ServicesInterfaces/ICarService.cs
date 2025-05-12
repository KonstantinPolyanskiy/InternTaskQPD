using Private.StorageModels;
using Public.Models.ApplicationErrors;
using Public.Models.BusinessModels.CarModels;
using Public.Models.CommonModels;
using Public.Models.DtoModels.CarDtoModels;

namespace Private.ServicesInterfaces;

public interface ICarService
{
    public Task<ApplicationExecuteLogicResult<DomainCar>> CreateCarAsync(DtoForCreateCar data);
    public Task<ApplicationExecuteLogicResult<DomainCar>> GetCarAsyncById(int id);
    public Task<ApplicationExecuteLogicResult<DomainCarsPage>> GetCarsAsync(DtoForSearchCars data);
    public Task<ApplicationExecuteLogicResult<DomainCar>> SetPhotoMetadataToCarAsync(DomainCar car, int photoMetadataId);
}