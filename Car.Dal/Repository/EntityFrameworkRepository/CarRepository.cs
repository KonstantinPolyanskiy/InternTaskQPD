using Car.Dal.Models;
using Contracts.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Car.Dal.Repository.EntityFrameworkRepository;


public class PostgresCarRepository(AppDbContext dbContext) : ICarRepository
{
    public async Task<int> SaveCarAsync(AddedCarDataLayerDto dto, int? photoId)
    {
        var entity = new Models.Car()
        {
            Brand = dto.Brand,
            Color = dto.Color,
            Price = dto.Price,
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
        return await dbContext.Cars.FindAsync(id);
    }

    public async Task<bool> DeleteCarByIdAsync(int id)
    {
        dbContext.Cars.Remove(new Models.Car{ Id = id });
        await dbContext.SaveChangesAsync();
        
        var car = await GetCarByIdAsync(id);

        return car is null;
    }

    public async Task<List<Models.Car>> GetAllCarsAsync()
    {
        return await dbContext.Cars.ToListAsync();
    }

    public async Task<Models.Car?> UpdateCarAsync(Models.Car updatingCar, int carId)
    {
        dbContext.Update(updatingCar);
        await dbContext.SaveChangesAsync();
     
        var car = await dbContext.Cars
            .Include(c => c.Photo)
            .FirstOrDefaultAsync(c => c.Id == carId);
        if (car is null) return null;
        
        return car;
    }
}