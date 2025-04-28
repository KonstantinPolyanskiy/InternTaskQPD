using System.Text.Json.Serialization;

namespace Models.Bridge.Car;



public class UsedCarDetailDto
{
    public int Mileage { get; set; }
    public string CurrentOwner { get; set; } = null!;
}

public class ManufacturingDetailDto
{
    public string Country { get; set; } = null!;
    public string LotNumber { get; set; } = null!;
    public DateTime ManufacturingDate { get; set; }
}

public class ManagerDetailDto
{
    public int ManagerId { get; set; }
    public string DisplayName { get; set; } = null!;
}