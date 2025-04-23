namespace Car.Dal;

public class Car
{
    /// <summary>Идентификатор</summary>
    public int Id { get; set; }
    
    /// <summary>Бренд</summary>
    public required string Brand { get; set; }
    
    
    public required string Color { get; set; }
    public decimal Price { get; set; }
    public string? Photo { get; set; }
    public int? Mileage { get; set; }
    public string? CurrentOwner { get; set; }
    public CarType CarType { get; set; }
}

public enum CarType
{
    NewCar,
    SecondHandCar,
}