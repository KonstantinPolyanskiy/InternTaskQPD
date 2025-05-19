using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QPDCar.Infrastructure.DbContexts;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.DtoModels.CarDtos;
using QPDCar.Models.StorageModels;
using QPDCar.Repositories.ErrorHelpers;
using QPDCar.Repositories.Filters;
using QPDCar.Services.Repositories;

namespace QPDCar.Repositories.Repositories;

public class CarRepository(AppDbContext db, ILogger<CarRepository> logger) : ICarRepository
{
    private const string EntityName = "Car";

    public async Task<ApplicationExecuteResult<CarEntity>> SaveAsync(CarEntity car)
    {
        try
        {
            await db.Cars.AddAsync(car);
            await db.SaveChangesAsync();
            
            return ApplicationExecuteResult<CarEntity>.Success(car);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteResult<CarEntity>.Failure(
                ErrorHelper.PrepareNotSavedError(EntityName));
        }
    }

    public async Task<ApplicationExecuteResult<CarEntity>> RewriteAsync(CarEntity car)
    {
        try
        {
            var exist = await db.Cars.FirstOrDefaultAsync(e => e.Id == car.Id);
            if (exist == null)
                return ApplicationExecuteResult<CarEntity>.Failure(ErrorHelper.PrepareNotFoundErrorSingle(EntityName));

            db.Entry(exist).CurrentValues.SetValues(car);
            await db.SaveChangesAsync();

            return ApplicationExecuteResult<CarEntity>.Success(car);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteResult<CarEntity>.Failure(ErrorHelper.PrepareNotUpdatedError(EntityName));
        }
    }

    public async Task<ApplicationExecuteResult<Unit>> DeleteAsync(int id)
    {
        try
        {
            var entity = await db.Cars.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return ApplicationExecuteResult<Unit>.Failure(ErrorHelper.PrepareNotFoundErrorSingle(EntityName));
            
            db.Cars.Remove(entity);
            await db.SaveChangesAsync();
            
            return ApplicationExecuteResult<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteResult<Unit>.Failure(ErrorHelper.PrepareNotDeletedError(EntityName));
        }
    }

    public async Task<ApplicationExecuteResult<CarEntity>> ByIdAsync(int id)
    {
        try
        {
            var entity = await db.Cars.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return ApplicationExecuteResult<CarEntity>.Failure(ErrorHelper.PrepareNotFoundErrorSingle(EntityName));
            
            return ApplicationExecuteResult<CarEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteResult<CarEntity>.Failure(ErrorHelper.PrepareNotFoundErrorSingle(EntityName));
        }
    }

    public async Task<ApplicationExecuteResult<CarEntityPage>> ByParamsAsync(DtoForSearchCars parameters)
    {
        try
        {
            var query = db.Cars.AsNoTracking().AsQueryable()
                .FilterByBrands(parameters.Brands)
                .FilterByColors(parameters.Colors)
                .FilterByCondition(parameters.Condition)
                .FilterBySortingTermination(parameters.SortTerm, parameters.Direction);

            var result = await query.Skip((parameters.PageNumber - 1) * parameters.PageSize).Take(parameters.PageSize).ToListAsync();

            return ApplicationExecuteResult<CarEntityPage>.Success(new CarEntityPage()
            {
                Cars = result,
                TotalCount = query.Count(),
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteResult<CarEntityPage>.Failure(ErrorHelper.PrepareNotFoundErrorMany(EntityName));
        }
    }
}