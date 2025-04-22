namespace CarService.Models.Car;

public interface ISecondHandCar : ICar
{
    /// <summary>
    /// Пробег автомобиля в киломметрах
    /// </summary>
    public int? Mileage { get; set; }
    
    /// <summary>
    /// Имя текущего владельца автомобиля
    /// </summary>
    public string? CurrentOwner  { get; set; }
}