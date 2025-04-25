namespace Models.Bridge.Car;

public interface ICarDetail {}

/// <summary>
/// Детали БУ авто
/// </summary>
public interface IUsedCarDetail : ICarDetail
{
    public int Mileage { get; set; }
    public string CurrentOwner { get; set; }
}

public class UsedCarDetail : IUsedCarDetail
{
    public int Mileage { get; set; }
    public required string CurrentOwner { get; set; }
}

/// <summary>
/// Детали производства
/// </summary>
public interface IManufacturingDetail : ICarDetail
{
    public string Country { get; set; }
    public string LotNumber { get; set; }
    public DateTime ManufacturingDate { get; set; }
}

public class ManufacturingDetail : IManufacturingDetail
{
    public required string Country { get; set; }
    public required string LotNumber { get; set; }
    public DateTime ManufacturingDate { get; set; }
}

/// <summary>
/// Детали назначенного менеджера
/// </summary>
public interface IManagerDetail : ICarDetail
{
    public int ManagerId { get; set; }
    public string DisplayName { get; set; }
}

public class ManagerDetail : IManagerDetail
{
    public int ManagerId { get; set; }
    public required string  DisplayName { get; set; }
}