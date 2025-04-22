using CarService.Models.Car;
using Microsoft.EntityFrameworkCore;

namespace CarService.Daos.CarDao.EntityCarDao;

public class EntityCarDao(CarDbContext carDbContext) : ICarDao
{
    public async Task<int?> SaveAsync(ICar car)
    {
        if (car is not BaseCar) throw new InvalidOperationException("only base cars are supported");
        await carDbContext.AddAsync(car);
        await carDbContext.SaveChangesAsync();
        
        var baseCar = car as BaseCar;

        return baseCar?.Id;
    }

    public async Task<bool?> DeleteAsync(int id)
    {
        var car = await GetByIdAsync(id);
        
        if (car is null) return false;
        
        carDbContext.Cars.Remove(car as BaseCar ?? throw new InvalidOperationException("only base cars are supported"));
        await carDbContext.SaveChangesAsync();
        
        return true;
    }

    public async Task<ICar?> GetByIdAsync(int id)
    {
        var car = await carDbContext.FindAsync<BaseCar>(id);
        return car;
    }

    public async Task<ICar?> UpdateAsync(int id, ICar newCar)
    {
        if (newCar is not BaseCar baseCar)
            throw new ArgumentException("newCar must be a BaseCar");

        carDbContext.Update(baseCar);
        await carDbContext.SaveChangesAsync();
        
        return baseCar;
    }

    public async Task<List<ICar>> GetAllCarsAsync()
    {
        var cars = await carDbContext.Cars.ToListAsync();
        
        return cars.Cast<ICar>().ToList();
    }
}