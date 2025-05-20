using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.BusinessModels.CarModels;
using QPDCar.Models.BusinessModels.PhotoModels;
using QPDCar.Models.DtoModels.CarDtos;
using QPDCar.Models.DtoModels.PhotoDtos;

namespace QPDCar.ServiceInterfaces;

public interface ICarService
{
    Task<ApplicationExecuteResult<DomainCar>> CreateCarAsync(DtoForSaveCar carData);
    Task<ApplicationExecuteResult<DomainCar>> UpdateCarAsync(DtoForUpdateCar carData);
    Task<ApplicationExecuteResult<Unit>> DeleteCarAsync(int carId);
    
    Task<ApplicationExecuteResult<DomainCar>> SoldCarAsync(DomainCar car);    
    
    Task<ApplicationExecuteResult<DomainCar>> ByIdAsync(int carId);
    Task<ApplicationExecuteResult<DomainCarPage>> ByParamsAsync(DtoForSearchCars parameters);
    
    Task<ApplicationExecuteResult<DomainPhoto>> SetCarPhotoAsync(int carId, DtoForSavePhoto photoDto);
    Task<ApplicationExecuteResult<DomainCar>> DeleteCarPhotoAsync(int carId);
    Task<ApplicationExecuteResult<DomainPhoto>> PhotoByCarIdAsync(int carId);
}