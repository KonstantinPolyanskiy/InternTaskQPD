using Enum.Common;

namespace Backend.App.Models.Dto;

public record CarDto
{
    public int? Id { get; init; }
    
    public int? PhotoMetadataId { get; init; }
    
    public string? Brand { get; init; }
    public string? Color { get; init; }
    public decimal? Price { get; init; }
    
    public string? CurrentOwner { get; init; }
    public int? Mileage { get; init; }
    
    public CarCondition? Condition { get; init; }
    public PrioritySale? PrioritySale { get; init; }
}

public record CarPageDto
{
    public List<CarDto> Cars { get; init; } = [];
    
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public record CarQueryDto
{
    public string[]? Brands { get; set; }
    public string[]? Colors { get; set; }
    
    public CarCondition? Condition { get; init; }
    
    public CarSortTerm? SortTerm { get; init; }
    public PhotoHavingTerm PhotoTerm { get; init; } = PhotoHavingTerm.NoMatter;
    public SortDirection Direction { get; init; } = SortDirection.Ascending;
    
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

