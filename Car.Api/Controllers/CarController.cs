using AutoMapper;
using CarService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarService.Controllers;

[ApiController]
[Route("api/car")]
public class CarController(Car.App.Services.CarService carService, IMapper mapper) : ControllerBase
{
    /// <summary> HTTP API для создания машины <see cref="Car.App.Services.CarService.CreateCarAsync"/> </summary>
    [HttpPost]
    public async Task<IActionResult> AddCar([FromForm] AddCarRequest request, CancellationToken ct = default)
    {
        var carId = await carService.CreateCarAsync(request);

        return Ok(new
        {
            car_id = carId,
        });
    }
    
    /// <summary> HTTP API для получения машины по Id <see cref="Car.App.Services.CarService.GetCarByIdAsync"/> </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCar(int id, CancellationToken ct = default)
    {
        var car = await carService.GetCarByIdAsync(id);
        
        var response = mapper.Map<CarResponse>(car);
        
        return Ok(new
        {
            car_property = response,
            photo_id = car.PhotoId,
        });
    }
    
    /// <summary> Обновляет машину по id <see cref="Car.App.Services.CarService.UpdateCarAsync"/> </summary>
    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(PatchCarRequest request, int id)
    {
        var dto = mapper.Map<PatchCarDomain>(request);
        
        var updatedCar = await carService.UpdateCarAsync(dto, id);
        
        var response = mapper.Map<CarResponse>(updatedCar);
        
        return Ok(response);
    }
    
    /// <summary> HTTP API для получения всех машин <see cref="Car.App.Services.CarService.GetCarsAsync"/> </summary>
    [HttpGet("cars")]
    public async Task<IActionResult> GetCars(CancellationToken ct = default) => Ok(await carService.GetCarsAsync());
    /*{
        IList<CarResponse> response = new List<CarResponse>();

        foreach (var car in await carService.GetCarsAsync())
        {
            var r = mapper.Map<CarResponse>(car);
            
            response.Add(r);            
        }
        
        return Ok(response);
    }*/

    /// <summary> HTTP API для hard удаления машины <see cref="Car.App.Services.CarService.DeleteCarByIdAsync"/> </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCar(int id) => Ok(await carService.DeleteCarByIdAsync(id));
}
