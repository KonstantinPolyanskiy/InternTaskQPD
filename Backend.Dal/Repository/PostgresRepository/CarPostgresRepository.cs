using Backend.App.Models.Dto;
using Backend.App.Repositories;
using Backend.Dal.Models;
using Enum.Common;
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

    public async Task<CarPageDto> GetCarsByQueryAsync(CarQueryDto dto)
    {
        var query = dbContext.Cars.AsNoTracking().AsQueryable();
        
        if (dto.Brands is not null && dto.Brands.Length > 0)
            query = query.Where(c => dto.Brands.Contains(c.Brand.ToLower()));
        
        if (dto.Colors is not null && dto.Colors.Length > 0)
            query = query.Where(c => dto.Colors.Contains(c.Color.ToLower()));
        
        if (dto.Condition is not null)
            query = query.Where(c => c.CarCondition == dto.Condition.Value);

        switch (dto.SortTerm)
        {
            case CarSortTerm.Price:
                query = dto.Direction == SortDirection.Ascending ? query.OrderBy(c => c.Price) : query.OrderByDescending(c => c.Price);
                break;
            case CarSortTerm.Mileage:
                query = query.Where(c => c.Mileage.HasValue && c.Mileage > 0);
                query = dto.Direction == SortDirection.Ascending ? query.OrderBy(c => c.Mileage!.Value) : query.OrderByDescending(c => c.Mileage!.Value);
                break;
            case CarSortTerm.Id:
                query = dto.Direction == SortDirection.Ascending ? query.OrderBy(c => c.Id) : query.OrderByDescending(c => c.Id);
                break;
            default: throw new ApplicationException("Unknown SortTerm");
        }
        
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