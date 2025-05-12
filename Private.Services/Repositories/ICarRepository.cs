using Private.StorageModels;
using Public.Models.CommonModels;
using Public.Models.DtoModels.CarDtoModels;

namespace Private.Services.Repositories;

public interface ICarRepository
{
    public Task<ApplicationExecuteLogicResult<CarEntity>> SaveCarAsync(CarEntity car);
    public Task<ApplicationExecuteLogicResult<CarEntity>> GetCarByIdAsync(int id);
    public Task<ApplicationExecuteLogicResult<List<CarEntity>>> GetCarsByQueryAsync(DtoForSearchCars dto);
    public Task<ApplicationExecuteLogicResult<CarEntity>> RewriteCarAsync(CarEntity car);
    public Task<ApplicationExecuteLogicResult<Unit>> DeleteCarAsync(int id);
}