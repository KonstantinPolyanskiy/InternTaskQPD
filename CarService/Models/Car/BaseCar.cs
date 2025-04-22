using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CarService.Models.Car.Requests;

namespace CarService.Models.Car;

public class BaseCar : ICar
{
    public BaseCar() { }
    
    public int? Id { get; set; }
    public string? Brand { get; set; }
    public string? Color { get; set; }
    public decimal Price { get; set; }
    public string? Photo { get; set; }
}