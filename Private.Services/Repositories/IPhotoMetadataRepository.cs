using Private.StorageModels;
using Public.Models.CommonModels;

namespace Private.Services.Repositories;

public interface IPhotoMetadataRepository
{
    public Task<ApplicationExecuteLogicResult<PhotoMetadataEntity>> SavePhotoMetadataAsync(PhotoMetadataEntity entity);
    
    public Task<ApplicationExecuteLogicResult<PhotoMetadataEntity>> GetPhotoMetadataByIdAsync(int id);
    public Task<ApplicationExecuteLogicResult<List<PhotoMetadataEntity>>> GetPhotosMetadataByIdsAsync(int[] ids);
    public Task<ApplicationExecuteLogicResult<PhotoMetadataEntity>> GetPhotoMetadataByPhotoIdAsync(Guid photoId);
    
    public Task<ApplicationExecuteLogicResult<PhotoMetadataEntity>> RewritePhotoMetadataAsync(PhotoMetadataEntity entity);
    
    public Task<ApplicationExecuteLogicResult<Unit>> DeletePhotoMetadataAsync(int id);
}