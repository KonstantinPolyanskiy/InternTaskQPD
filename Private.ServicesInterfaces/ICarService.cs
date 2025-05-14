using Private.StorageModels;
using Public.Models.ApplicationErrors;
using Public.Models.BusinessModels.CarModels;
using Public.Models.BusinessModels.PhotoModels;
using Public.Models.CommonModels;
using Public.Models.DtoModels.CarDtoModels;
using Public.Models.DtoModels.PhotoDtoModels;

namespace Private.ServicesInterfaces;

public interface ICarService
{
    public Task<ApplicationExecuteLogicResult<DomainCar>> CreateCarAsync(DtoForSaveCar carData);
    public Task<ApplicationExecuteLogicResult<DomainCar>> UpdateCarAsync(DtoForUpdateCar carData);
    public Task<ApplicationExecuteLogicResult<DomainCarsPage>> CarsByParams(DtoForSearchCars searchDto);
    public Task<ApplicationExecuteLogicResult<DomainCar>> CarById(int id);
    public Task<ApplicationExecuteLogicResult<Unit>> DeleteCarById(int id);
    
    public Task<ApplicationExecuteLogicResult<DomainCar>> SetCarPhoto(DtoForSavePhoto photoDto);
    public Task<ApplicationExecuteLogicResult<DomainPhoto>> CarPhotoById(int metadataId);
    public Task<ApplicationExecuteLogicResult<Unit>> DeleteCarPhoto(int metadataId);
    
}