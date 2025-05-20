using System.Collections.Concurrent;
using System.Net;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ApplicationResult.Extensions;
using QPDCar.Models.BusinessModels.CarModels;
using QPDCar.ServiceInterfaces;

namespace QPDCar.Services.Services;

public class InMemoryCartService(ICarService carService) : ICartService
{
    private static readonly ConcurrentDictionary<Guid, HashSet<int>> Carts = new();
    
    public async Task<ApplicationExecuteResult<Unit>> AddCarAsync(Guid userId, int carId)
    {
        var set = Carts.GetOrAdd(userId, id => []);
        lock (set) set.Add(carId);
        
        return await Task.FromResult(ApplicationExecuteResult<Unit>.Success(new Unit()));
    }

    public async Task<ApplicationExecuteResult<Unit>> RemoveCarAsync(Guid userId, int carId)
    {
        if (Carts.TryGetValue(userId, out var set))
            lock (set) set.Remove(carId);
        
        return await Task.FromResult(ApplicationExecuteResult<Unit>.Success(new Unit()));
    }

    public async Task<ApplicationExecuteResult<List<int>>> CarsAsync(Guid userId)
    {
        var items = Carts.TryGetValue(userId, out var set) ? set.ToArray() : [];
        
        return await Task.FromResult(ApplicationExecuteResult<List<int>>.Success(items.ToList()));
    }

    public async Task<ApplicationExecuteResult<Unit>> CheckoutAsync(Guid userId)
    {
        if (!Carts.TryRemove(userId, out var set) || set.Count == 0) 
            return await Task.FromResult(ApplicationExecuteResult<Unit>.Failure(new ApplicationError(
                CartErrors.NoOneCarInCart, "Нет машин",
                $"В корзине у пользователя {userId} нет ни 1 машины",
                ErrorSeverity.Critical, HttpStatusCode.BadRequest)));
        
        var rawCarsResult = await CarsAsync(userId);
        if (rawCarsResult.IsSuccess is false)
            return ApplicationExecuteResult<Unit>.Failure().Merge(rawCarsResult);
        var rawCars = rawCarsResult.Value!;

        var warns = new List<ApplicationError>();
        
        foreach (var rawCarId in rawCars)
        {
            var carResult = await carService.ByIdAsync(rawCarId);
            if (carResult.IsSuccess is false)
                warns.Add(new ApplicationError(
                    CarErrors.CarNotFound, "Машина не найдена",
                    $"Id машины в корзине {rawCarId} не найдена в системе",
                    ErrorSeverity.NotImportant));
            var car = carResult.Value!;

            if (car.IsSold is false)
            {
                var soldResult = await carService.SoldCarAsync(car);
                if (soldResult.IsSuccess is false)
                    warns.Add(new ApplicationError(
                        CarErrors.CarNotUpdated, "Машина не продана",
                        $"Не получилось продать машину {car.Id}",
                        ErrorSeverity.NotImportant));
            }
        }
        
        return ApplicationExecuteResult<Unit>.Success(Unit.Value).WithWarnings(warns);
    }
}