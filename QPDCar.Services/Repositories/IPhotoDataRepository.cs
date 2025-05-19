using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.StorageModels;

namespace QPDCar.Services.Repositories;

public interface IPhotoDataRepository
{
    Task<ApplicationExecuteResult<PhotoDataEntity>> SaveAsync(PhotoDataEntity photoData);
    Task<ApplicationExecuteResult<PhotoDataEntity>> ByIdAsync(Guid photoDataId);
}