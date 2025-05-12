using Private.StorageModels;
using Public.Models.BusinessModels.CarModels;
using Public.Models.CommonModels;

namespace Private.Storages.FilterHelpers;

public static class CarFilterHelper
{
    public static IQueryable<CarEntity> FilterByBrands(this IQueryable<CarEntity> query, string[]? brands)
    {
        if (brands is null || brands.Length == 0) return query;
        
        var lower = brands.Select(x => x.ToLower()).ToArray();
        return query.Where(x => lower.Contains(x.Brand.ToLower()));
    }

    public static IQueryable<CarEntity> FilterByColors(this IQueryable<CarEntity> query, string[]? colors) 
    {
        if (colors is null || colors.Length == 0) return query;
        
        var lower = colors.Select(x => x.ToLower()).ToArray();
        return query.Where(x => lower.Contains(x.Color.ToLower()));
    }
    
    
    public static IQueryable<CarEntity> FilterByCondition(this IQueryable<CarEntity> query, CarConditionTypes? condition) 
        => condition is not null ? query.Where(c => c.CarCondition == condition) : query;

    public static IQueryable<CarEntity> FilterBySortingTermination(this IQueryable<CarEntity> query, CarSortTermination? term, SortDirection direction)
        => term == null ? query : term switch
        {
            CarSortTermination.Id => direction is SortDirection.Ascending ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id), 
            CarSortTermination.Price => direction is SortDirection.Ascending ? query.OrderBy(x => x.Price) : query.OrderByDescending(x => x.Price),
            CarSortTermination.Mileage => direction is SortDirection.Ascending ?
                query.OrderBy(x => x.Mileage).Where(c => c.Mileage.HasValue && c.Mileage > 0) : 
                query.OrderByDescending(x => x.Mileage).Where(c => c.Mileage.HasValue && c.Mileage > 0),
            _ => query,
        };
}