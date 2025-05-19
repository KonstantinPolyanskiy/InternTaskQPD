using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.StorageModels;

namespace QPDCar.Services.Repositories;

public interface IPhotoMetadataRepository
{
    Task<ApplicationExecuteResult<PhotoMetadataEntity>> SaveAsync(PhotoMetadataEntity photoMetadata);

    Task<ApplicationExecuteResult<PhotoMetadataEntity>> ByIdAsync(int id);
    Task<ApplicationExecuteResult<PhotoMetadataEntity>> ByDataIdAsync(Guid photoDataId);
}