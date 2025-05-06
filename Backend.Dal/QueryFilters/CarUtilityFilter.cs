using System.Linq.Expressions;
using Backend.App.Models.Business;
using Backend.Dal.Models;
using Enum.Common;

namespace Backend.Dal.QueryFilters;

public static class CarUtilityFilter
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
    
    
    public static IQueryable<CarEntity> FilterByCondition(this IQueryable<CarEntity> query, CarCondition? condition) 
        => condition is not null ? query.Where(c => c.CarCondition == condition) : query;

    public static IQueryable<CarEntity> FilterBySortingTermination(this IQueryable<CarEntity> query, CarSortTerm? term, SortDirection direction)
        => term == null ? query : term switch
        {
            CarSortTerm.Id => direction is SortDirection.Ascending ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id), 
            CarSortTerm.Price => direction is SortDirection.Ascending ? query.OrderBy(x => x.Price) : query.OrderByDescending(x => x.Price),
            CarSortTerm.Mileage => direction is SortDirection.Ascending ?
                query.OrderBy(x => x.Mileage).Where(c => c.Mileage.HasValue && c.Mileage > 0) : 
                query.OrderByDescending(x => x.Mileage).Where(c => c.Mileage.HasValue && c.Mileage > 0),
            _ => query,
        };
}