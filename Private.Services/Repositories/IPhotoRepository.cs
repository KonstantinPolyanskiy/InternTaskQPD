using Private.StorageModels;
using Public.Models.BusinessModels.StorageModels;
using Public.Models.CommonModels;

namespace Private.Services.Repositories;

public interface IPhotoRepository
{
    public Task<ApplicationExecuteLogicResult<PhotoEntity>> SavePhotoAsync(PhotoEntity entity, StorageTypes priorityStorage, ImageFileExtensions? ext = null);
    public Task<ApplicationExecuteLogicResult<PhotoEntity>> GetPhotoByIdAsync(Guid id);
    public Task<ApplicationExecuteLogicResult<List<PhotoEntity>>> GetPhotosByIdsAsync(Guid[] ids);
    public Task<ApplicationExecuteLogicResult<Unit>> DeletePhotoByIdAsync(Guid id, StorageTypes storageType);
}