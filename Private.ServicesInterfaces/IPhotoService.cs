using Public.Models.BusinessModels.PhotoModels;
using Public.Models.CommonModels;
using Public.Models.DtoModels.PhotoDtoModels;

namespace Private.ServicesInterfaces;

public interface IPhotoService
{
    public Task<ApplicationExecuteLogicResult<DomainPhoto>> CreatePhotoAsync(DtoForCreatePhoto data);
    public Task<ApplicationExecuteLogicResult<DomainPhoto>> GetPhotoByMetadataIdAsync(int id);
    public Task<ApplicationExecuteLogicResult<List<DomainPhoto>>> GetPhotosByMetadataIdAsync(int[] ids);
}