using Car.App.Models;
using Car.App.Services.Repositories;
using Car.Dal.Models;
using Contracts.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Car.Dal.Repository.EntityFrameworkRepository;

public class PostgresCarRepository(AppDbContext dbContext) : ICarRepository
{
    public Task<CarResult> SaveCarAsync(CarData data)
    {
        throw new NotImplementedException();
    }

    public Task<CarResult> GetCarByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteCarByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<List<CarResult>> GetAllCarsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<CarResult> UpdateCarAsync(CarData dto, int id)
    {
        throw new NotImplementedException();
    }
}

/*public class PostgresCarRepository(AppDbContext dbContext) : ICarRepository
{
    public async Task<int> SaveCarAsync(AddCarEntity dto, int? photoId)
    {
        var entity = new Models.Car()
        {
            Brand = dto.Brand,
            Color = dto.Color,
            Price = (decimal)dto.Price!,
            CarType = (byte)dto.CarType,
            Mileage = dto.Mileage,
            CurrentOwner = dto.CurrentOwner,
            PhotoId = photoId
        };
        
        await dbContext.Cars.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        
        return entity.Id;
    }

    public async Task<Models.Car?> GetCarByIdAsync(int id)
    {
        var car = await dbContext.Cars.FirstOrDefaultAsync(x => x.Id == id);
        return car;
    }

    public async Task<bool> DeleteCarByIdAsync(int id)
    {
        var car = await dbContext.Cars.FindAsync(id);

        if (car is null)
            return false; 

        dbContext.Cars.Remove(car);
        await dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<List<Models.Car>> GetAllCarsAsync()
    {
        return await dbContext.Cars.ToListAsync();
    }

    public async Task<Models.Car?> UpdateCarAsync(PatchCarEntity updatingCar, int carId)
    {
        var car = await dbContext.Cars
            .Include(c => c.Photo)
            .FirstOrDefaultAsync(c => c.Id == carId);
        if (car is null) return null;
        
        dbContext.Update(updatingCar);
        await dbContext.SaveChangesAsync();

        return car;
    }
}*/