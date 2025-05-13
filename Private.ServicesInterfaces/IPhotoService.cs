using Public.Models.BusinessModels.PhotoModels;
using Public.Models.CommonModels;
using Public.Models.DtoModels.PhotoDtoModels;

namespace Private.ServicesInterfaces;

public interface IPhotoService
{
    public Task<ApplicationExecuteLogicResult<DomainPhoto>> CreatePhotoAsync(DtoForSavePhoto data);
    public Task<ApplicationExecuteLogicResult<DomainPhoto>> GetPhotoByCarIdAsync(int carId);
}