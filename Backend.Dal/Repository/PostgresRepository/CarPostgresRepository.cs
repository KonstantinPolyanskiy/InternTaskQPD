using Backend.App.Models;
using Backend.App.Models.Dto;
using Backend.App.Repositories;
using Backend.Dal.Models;
using Backend.Dal.QueryFilters;
using Enum.Common;
using Microsoft.EntityFrameworkCore;

namespace Backend.Dal.Repository.PostgresRepository;

public class PostgresCarRepository(AppDbContext dbContext) : ICarRepository
{
    public async Task<CarDto> CreateCarAsync(CarDto dto)
    {
        await dbContext.Cars.AddAsync(new CarEntity
        {
            Id = dto.Id,
            
        });
        
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

    public async Task<bool> Exists(int id)
    {
        return await dbContext.Cars.AnyAsync(x => x.Id == id);
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

    public async Task<CarPageDto> GetCarsByQueryAsync(CarQueryDto dto)
    {
        var query = dbContext.Cars.AsNoTracking().AsQueryable()
            .FilterByBrands(dto.Brands)
            .FilterByColors(dto.Colors)
            .FilterByCondition(dto.Condition)
            .FilterBySortingTermination(dto.SortTerm, dto.Direction);
        
        var result = await query.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize)
            .Select(c => new CarDto
            {
                Id = c.Id,
                PhotoMetadataId = c.PhotoMetadataId,
                Brand = c.Brand,
                Color = c.Color,
                Price = c.Price,
                CurrentOwner = c.CurrentOwner,
                Mileage = c.Mileage,
                Condition = c.CarCondition,
                PrioritySale = c.PrioritySale
            })
            .ToListAsync();

        return new CarPageDto
        {
            Cars = result,
            TotalCount = await query.CountAsync(),
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize,
        };
    }

    public async Task UpdateCarAsync(int id, CarData dto)
    {
        var updatedCar = dbContext.Update(new CarEntity
        {
            Id = id,
            Brand = dto.Brand,
            Color = dto.Color,
            Price = dto.Price,
            CurrentOwner = dto.CurrentOwner,
            Mileage = dto.Millage
        });
        
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteMetadataAsync(int id)
    {
        var entity = await dbContext.Cars.SingleOrDefaultAsync(x => x.Id == id);
        if (entity is null) return;
        
        entity.PhotoMetadataId = null;
        entity.PhotoMetadata = null;
        
        await dbContext.SaveChangesAsync();
    }

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