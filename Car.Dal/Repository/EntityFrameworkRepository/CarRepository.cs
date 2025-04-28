using Car.App.Models;
using Car.App.Models.CarModels;
using Car.App.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Car.Dal.Repository.EntityFrameworkRepository;

public class PostgresCarRepository(AppDbContext dbContext) : ICarRepository
{
    public async Task<CarResult> SaveCarAsync(CarData data)
    {
        var entity = new Models.Car(data);

        await dbContext.Cars.AddAsync(entity);
        await dbContext.SaveChangesAsync();

        return new CarResult
        {
            Id = entity.Id,

            Brand = entity.Brand,
            Color = entity.Color,
            Price = entity.Price,

            PrioritySale = entity.PrioritySale,
            Condition = entity.CarCondition,
        };
    }

    public async Task<CarResult?> GetCarByIdAsync(int id)
    {
        var entity = await dbContext.Cars.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
            return null;

        return new CarResult
        {
            Id = entity.Id,
            PhotoTermId = entity.PhotoId.ToString(),

            Brand = entity.Brand,
            Color = entity.Color,
            Price = entity.Price,

            PrioritySale = entity.PrioritySale,
            Condition = entity.CarCondition,
        };
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

    public async Task<List<CarResult>> GetAllCarsAsync() => 
        await dbContext.Cars
            .Select(e => new CarResult {
                Id           = e.Id,
                PhotoTermId  = e.PhotoId.ToString(),
                Brand        = e.Brand,
                Color        = e.Color,
                Price        = e.Price,
                PrioritySale     = e.PrioritySale,
                Condition    = e.CarCondition
            })
            .ToListAsync();

    public async Task<CarResult?> UpdateCarAsync(CarData dto, int id)
    {
        var entity = await dbContext.Cars.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
            return null;
        
        if (dto.PhotoTermId is not null)
            entity.PhotoId = Convert.ToInt32(dto.PhotoTermId);
        if (dto.Condition is not null)
            entity.CarCondition = (CarCondition)dto.Condition;
        if (dto.PrioritySale is not null)
            entity.PrioritySale = (CarPrioritySale)dto.PrioritySale;
        if (dto.Brand is not null)
            entity.Brand = dto.Brand;
        if (dto.Color is not null)
            entity.Color = dto.Color;
        if (dto.Price is not null)
            entity.Price = (decimal)dto.Price;
        
        dbContext.Cars.Update(entity);
        await dbContext.SaveChangesAsync();
        
        return new CarResult
        {
            Id = entity.Id,
            PhotoTermId = entity.PhotoId.ToString(),

            Brand = entity.Brand,
            Color = entity.Color,
            Price = entity.Price,

            PrioritySale = entity.PrioritySale,
            Condition = entity.CarCondition,
        };
    }
}