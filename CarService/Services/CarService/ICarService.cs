using CarService.Api.RestControllers.CarController;
using CarService.Models.Car;
using CarService.Models.Car.Requests;

namespace CarService.Services.CarService;

public interface ICarService
{
    Task<ICar> CreateCarAsync(CreateCarRequestDto requestDto);
    Task<bool?> DeleteCarAsync(int id);
    Task<ICar> UpdateCarAsync(int id, PatchUpdateCarRequestDto newCar);
    Task<ICar> GetCarByIdAsync(int id);
    Task<IList<ICar>> GetAllCarsAsync();
}