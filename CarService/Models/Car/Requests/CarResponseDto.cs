namespace CarService.Models.Car.Requests;

public class CarResponseDto
{
    public int Id { get; set; }
    public string Brand { get; set; } = null!;
    public string Color { get; set; } = null!;
    public decimal Price { get; set; }
    public string Photo { get; set; } = null!;

    // only for SecondHandCar
    public int? Mileage { get; set; }
    public string? CurrentOwner { get; set; }
}