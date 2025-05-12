using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Private.Services.Repositories;
using Private.StorageModels;
using Private.Storages.DbContexts;
using Private.Storages.ErrorHelpers;
using Private.Storages.FilterHelpers;
using Public.Models.CommonModels;
using Public.Models.DtoModels.CarDtoModels;

namespace Private.Storages.Repositories.CarRepository;

public class CarRepository(AppDbContext db, ILogger<CarRepository> logger) : ICarRepository
{
    private const string EntityName = "Car";
    
    public async Task<ApplicationExecuteLogicResult<CarEntity>> SaveCarAsync(CarEntity car)
    {
        try
        {
            await db.Cars.AddAsync(car);
            await db.SaveChangesAsync();
            
            return ApplicationExecuteLogicResult<CarEntity>.Success(car);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<CarEntity>.Failure(ErrorHelper.PrepareStorageException(EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<CarEntity>> GetCarByIdAsync(int id)
    {
        try
        {
            var entity = await db.Cars.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return ApplicationExecuteLogicResult<CarEntity>.Failure(ErrorHelper.PrepareNotFoundError(EntityName));
            
            return ApplicationExecuteLogicResult<CarEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<CarEntity>.Failure(ErrorHelper.PrepareStorageException(EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<CarsEntityPage>> GetCarsByQueryAsync(DtoForSearchCars dto)
    {
        try
        {
            var query = db.Cars.AsNoTracking().AsQueryable()
                .FilterByBrands(dto.Brands)
                .FilterByColors(dto.Colors)
                .FilterByCondition(dto.Condition)
                .FilterBySortingTermination(dto.SortTerm, dto.Direction);

            var result = await query.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize).ToListAsync();

            return ApplicationExecuteLogicResult<CarsEntityPage>.Success(new CarsEntityPage
            {
                Cars = result,
                TotalCount = query.Count(),
                PageNumber = dto.PageNumber,
                PageSize = dto.PageSize,
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<CarsEntityPage>.Failure(ErrorHelper.PrepareStorageException(EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<CarEntity>> RewriteCarAsync(CarEntity car)
    {
        try
        {
            var exist = await db.Cars.FirstOrDefaultAsync(e => e.Id == car.Id);
            if (exist == null)
                return ApplicationExecuteLogicResult<CarEntity>.Failure(ErrorHelper.PrepareNotFoundError(EntityName));

            db.Entry(exist).CurrentValues.SetValues(car);
            await db.SaveChangesAsync();

            return ApplicationExecuteLogicResult<CarEntity>.Success(car);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<CarEntity>.Failure(ErrorHelper.PrepareStorageException(EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> DeleteCarAsync(int id)
    {
        try
        {
            var entity = await db.Cars.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return ApplicationExecuteLogicResult<Unit>.Failure(ErrorHelper.PrepareNotFoundError(EntityName));
            
            db.Cars.Remove(entity);
            await db.SaveChangesAsync();
            
            return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<Unit>.Failure(ErrorHelper.PrepareStorageException(EntityName));
        }
    }
}