using Car.App.Models.Dto;
using Car.App.Repositories;
using Car.Dal.Models;
using Microsoft.EntityFrameworkCore;

namespace Car.Dal.Repository.PostgresRepository;

public class PostgresCarRepository(AppDbContext dbContext) : ICarRepository
{
    public async Task<CarResultDto> SaveCarAsync(CarDto dto)
    {
        var entity = new CarEntity(dto);

        await dbContext.Cars.AddAsync(entity);
        await dbContext.SaveChangesAsync();

        return new CarResultDto
        {
            Id = entity.Id,

            Brand = entity.Brand,
            Color = entity.Color,
            Price = entity.Price,

            PrioritySale = entity.PrioritySale,
            Condition = entity.CarCondition,
        };
    }

    public async Task<CarResultDto?> GetCarByIdAsync(int id)
    {
        var entity = await dbContext.Cars.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
            return null;

        return new CarResultDto
        {
            Id = entity.Id,

            PhotoTermId = entity.PhotoId.ToString(),
            StorageType = entity.StorageType,
            
            Brand = entity.Brand,
            Color = entity.Color,
            Price = entity.Price,

            PrioritySale = entity.PrioritySale,
            Condition = entity.CarCondition,
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

    public async Task<List<CarResultDto>> GetAllCarsAsync() => 
        await dbContext.Cars
            .Select(e => new CarResultDto {
                Id = e.Id,
                PhotoTermId = e.PhotoId.ToString(),
                StorageType = e.StorageType,
                
                Brand        = e.Brand,
                Color        = e.Color,
                Price        = e.Price,
                
                PrioritySale     = e.PrioritySale,
                Condition    = e.CarCondition
            })
            .ToListAsync();

    public async Task<CarResultDto?> UpdateCarAsync(CarDto dto, int id)
    {
        var entity = await dbContext.Cars.SingleOrDefaultAsync(x => x.Id == id);
        if (entity is null)
            return null;
        
        if (!string.IsNullOrWhiteSpace(dto.PhotoTermId)
            && int.TryParse(dto.PhotoTermId, out var photoId))
        {
            entity.PhotoId = photoId;
        }

        if (dto.Condition.HasValue)
            entity.CarCondition = dto.Condition.Value;

        if (dto.PrioritySale.HasValue)
            entity.PrioritySale = dto.PrioritySale.Value;

        if (!string.IsNullOrWhiteSpace(dto.Brand))
            entity.Brand = dto.Brand;

        if (!string.IsNullOrWhiteSpace(dto.Color))
            entity.Color = dto.Color;

        if (dto.Price.HasValue)
            entity.Price = dto.Price.Value;

        if (dto.StorageType.HasValue)
            entity.StorageType = dto.StorageType.Value;

        await dbContext.SaveChangesAsync();

        return new CarResultDto
        {
            Id           = entity.Id,
            Brand        = entity.Brand,
            Color        = entity.Color,
            Price        = entity.Price,
            Condition    = entity.CarCondition,
            PrioritySale = entity.PrioritySale,
            PhotoTermId  = entity.PhotoId?.ToString(),
            StorageType  = entity.StorageType
        };
    }
}