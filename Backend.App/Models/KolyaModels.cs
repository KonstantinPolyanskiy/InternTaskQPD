namespace Backend.App.Models;


// 
public sealed class CarData
{
    public required string Brand { get; init; }
    public required string Color { get; init; }
    public required decimal Price { get; init; }
    
    public required string? CurrentOwner { get; init; }
    public required int? Millage { get; init; }
}

// Получение с филтрами / обвноление / создание / просто получение машины
public sealed class CarResponse
{
    public required int Id { get; init; }
    public required string Brand { get; init; }
    public required string Color { get; init; }
    public required decimal Price { get; init; }
    
    
    public required string? CurrentOwner { get; init; }
    public required int? Millage { get; init; }
    
    public Manager? Manager { get; init; }
}


public sealed class Manager
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}

public record SomeCommand
{
    public 
    
    public record SomeDTO
    {
        public string Brand { get; init; }
    }
}
