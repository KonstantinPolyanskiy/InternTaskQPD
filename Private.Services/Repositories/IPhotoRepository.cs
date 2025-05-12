using Private.StorageModels;
using Public.Models.CommonModels;

namespace Private.Services.Repositories;

public interface IPhotoRepository
{
    public Task<ApplicationExecuteLogicResult<PhotoEntity>> SavePhotoAsync(PhotoEntity entity);
    public Task<ApplicationExecuteLogicResult<PhotoEntity>> GetPhotoByIdAsync(Guid id);
    public Task<ApplicationExecuteLogicResult<Unit>> DeletePhotoByIdAsync(Guid id);
}