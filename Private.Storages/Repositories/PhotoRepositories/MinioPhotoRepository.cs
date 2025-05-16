using Microsoft.Extensions.Logging;
using Private.Services.Repositories;
using Private.StorageModels;
using Public.Models.CommonModels;

namespace Private.Storages.Repositories.PhotoRepositories;

public class MinioPhotoRepository(ILogger<MinioPhotoRepository> logger) : IPhotoRepository
{
    public async Task<ApplicationExecuteLogicResult<PhotoEntity>> SavePhotoAsync(PhotoEntity entity)
    {
        throw new NotImplementedException();
    }

    public async Task<ApplicationExecuteLogicResult<PhotoEntity>> GetPhotoByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<ApplicationExecuteLogicResult<List<PhotoEntity>>> GetPhotosByIdsAsync(Guid[] ids)
    {
        throw new NotImplementedException();
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> DeletePhotoByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}