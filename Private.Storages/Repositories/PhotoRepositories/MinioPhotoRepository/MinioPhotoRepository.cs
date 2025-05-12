using Microsoft.Extensions.Logging;
using Private.Services.Repositories;
using Private.StorageModels;
using Public.Models.CommonModels;

namespace Private.Storages.Repositories.PhotoRepositories.MinioPhotoRepository;

public class MinioPhotoRepository(ILogger<MinioPhotoRepository> logger) : IPhotoRepository
{
    public Task<ApplicationExecuteLogicResult<PhotoEntity>> SavePhotoAsync(PhotoEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<ApplicationExecuteLogicResult<PhotoEntity>> GetPhotoByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<ApplicationExecuteLogicResult<Unit>> DeletePhotoByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}