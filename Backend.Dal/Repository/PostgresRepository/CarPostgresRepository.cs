using Backend.App.Models.Dto;
using Backend.App.Repositories;
using Backend.Dal.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Dal.Repository.PostgresRepository;

public class PostgresCarRepository(AppDbContext dbContext) : ICarRepository
{
    public async Task<CarDto > SaveCarAsync(CarDto dto)
    {
        var entity = new CarEntity(dto);

        await dbContext.Cars.AddAsync(entity);
        await dbContext.SaveChangesAsync();

        return new CarDto
        {
            Id = entity.Id,
            PhotoMetadataId = entity.PhotoMetadataId,
            Brand = entity.Brand,
            Color = entity.Color,
            Price = entity.Price,
            CurrentOwner = entity.CurrentOwner,
            Mileage = entity.Mileage,
            Condition = entity.CarCondition,
            PrioritySale = entity.PrioritySale,
        };
    }

    public async Task<CarDto?> GetCarByIdAsync(int id)
    {
        var entity = await dbContext.Cars.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
            return null;

        return new CarDto
        {
            Id = entity.Id,
            PhotoMetadataId = entity.PhotoMetadataId,
            Brand = entity.Brand,
            Color = entity.Color,
            Price = entity.Price,
            CurrentOwner = entity.CurrentOwner,
            Mileage = entity.Mileage,
            Condition = entity.CarCondition,
            PrioritySale = entity.PrioritySale,
        };
    }


    public async Task DeleteCarByIdAsync(int id)
    {
        var entity  = await dbContext.Cars.FindAsync(id);
        if (entity is not null)
        {
            dbContext.Cars.Remove(entity);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<List<CarDto>> GetAllCarsAsync() => 
        await dbContext.Cars
            .Select(entity => new CarDto {
                Id = entity.Id,
                PhotoMetadataId = entity.PhotoMetadataId,
                Brand = entity.Brand,
                Color = entity.Color,
                Price = entity.Price,
                CurrentOwner = entity.CurrentOwner,
                Mileage = entity.Mileage,
                Condition = entity.CarCondition,
                PrioritySale = entity.PrioritySale,
            })
            .ToListAsync();

    public async Task<CarDto?> UpdateCarAsync(CarDto dto)
    {
        var entity = await dbContext.Cars.SingleOrDefaultAsync(x => x.Id == dto.Id);
        if (entity is null)
            return null;
        
        if (dto.PhotoMetadataId.HasValue)
            entity.PhotoMetadataId = dto.PhotoMetadataId;

        if (dto.Condition.HasValue)
            entity.CarCondition = dto.Condition.Value;

        if (dto.PrioritySale.HasValue)
            entity.PrioritySale = dto.PrioritySale.Value;

        if (!string.IsNullOrWhiteSpace(dto.Brand))
            entity.Brand = dto.Brand;

        if (!string.IsNullOrWhiteSpace(dto.Color))
            entity.Color = dto.Color;
        
        if (!string.IsNullOrWhiteSpace(dto.CurrentOwner))
            entity.CurrentOwner = dto.CurrentOwner;
        
        if (dto.Mileage.HasValue)
            entity.Mileage = dto.Mileage.Value;
            
        if (dto.Price.HasValue)
            entity.Price = dto.Price.Value;

        await dbContext.SaveChangesAsync();

        return new CarDto
        {
            Id = entity.Id,
            PhotoMetadataId = entity.PhotoMetadataId,
            Brand = entity.Brand,
            Color = entity.Color,
            Price = entity.Price,
            CurrentOwner = entity.CurrentOwner,
            Mileage = entity.Mileage,
            Condition = entity.CarCondition,
            PrioritySale = entity.PrioritySale,
        };
    }
}