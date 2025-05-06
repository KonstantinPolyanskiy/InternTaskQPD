using Enum.Common;

namespace Backend.App.Models.Commands;

public record CreateCarCommand
{
    public required string Brand { get; init; }
    
    public required string Color { get; init; }
    
    public required decimal Price { get; init; }
    
    public string? CurrentOwner { get; init; }
    
    public int? Mileage { get; init; }
}

public record CreateCarWithPhotoCommand
{
    public required string Brand { get; init; }
    
    public required string Color { get; init; }
    
    public required decimal Price { get; init; }
    
    public string? CurrentOwner { get; init; }
    
    public int? Mileage { get; init; }
    
    public SetPhotoToCarCommand PhotoCmd {get; init; }
}

public record UpdateCarCommand
{
    public int Id { get; set; }
    
    public Guid? PhotoId { get; init; }
    
    public string? Brand { get; init; }
    public string? Color { get; init; }
    public decimal? Price { get; init; }
    
    public string? CurrentOwner { get; init; }
    public int? Mileage { get; init; }
    
    public CarCondition? Condition { get; init; }
    public PrioritySale? PrioritySale { get; init; }
}

public record SearchCarByIdCommand
{
    public required int CarId { get; init; }
    public bool NeedPhoto { get; init; }
}

public record SearchCarByQueryCommand
{
    public string[]? Brands { get; init; }
    public string[]? Colors { get; init; }
    
    public CarCondition? Condition { get; init; }
    
    public CarSortTerm? SortTerm { get; init; }
    public PhotoHavingTerm PhotoTerm { get; init; } = PhotoHavingTerm.NoMatter;
    public SortDirection Direction { get; init; } = SortDirection.Ascending;
    
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public record DeleteCarCommand
{
    public int Id { get; init; }
    public bool HardDelete { get; init; }
}