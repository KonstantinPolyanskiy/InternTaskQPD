using AutoMapper;
using Car.App.Models.CarModels;
using Car.App.Models.PhotoModels;
using Car.App.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarService.Controllers;

[ApiController]
[Route("api/car")]
public class CarController(Car.App.Services.CarService carService, IMapper mapper) : ControllerBase
{
    /// <summary> HTTP API для создания машины <see cref="CarService.CreateCarAsync"/> </summary>
    [HttpPost]
    public async Task<IActionResult> AddCar([FromBody] CarRequest carRequest) => Ok(new
    {
        car_id = await carService.CreateCarAsync(carRequest)
    });

    [HttpPost("photo/{carId}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddCarPhoto(IFormFile file, int carId)
    {
        if (file.Length == 0)
            return BadRequest("Файл не передан");
        
        var photoRequest = new PhotoRequest
        {
            CarId = carId,
            Content = file.OpenReadStream(),
            Extension = Path.GetExtension(file.FileName).TrimStart('.')
        };

        var photo = await carService.AddPhotoToCarAsync(photoRequest);

        return Ok(new
        {
            id = photo.Id,
            car_id = photo.Data?.CarId,
            
            name = photo.PhotoName, 
            
            method = photo.Method.ToString(),
            value = photo.Value,
            
            ext = photo.Data?.Extension
        });
    }

    /// <summary> HTTP API для получения машины по Id <see cref="CarService.GetCarByIdAsync"/> </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCar(int id, CancellationToken ct = default)
    {
        var car = await carService.GetCarByIdAsync(id);

        return Ok(new
        {
            id = car.Id,
            brand = car.Brand,
            color = car.Color,
            price = car.Price,
            car_photo = new
            {
                id = Convert.ToInt32(car.Photo?.Id),
                name = car.Photo?.PhotoName,
                access_method = car.Photo?.Method.ToString(),
                access_value = car.Photo?.Value,
            }
        });
    }

    /// <summary> Обновляет машину по id <see cref="CarService.UpdateCarAsync"/> </summary>
    [HttpPatch("{carId}")]
    public async Task<IActionResult> Patch(CarRequest request, int carId) => Ok(new
    {
        updated = await carService.UpdateCarAsync(request, carId)
    });

    /// <summary> HTTP API для получения всех машин <see cref="CarService.GetCarsAsync"/> </summary>
    [HttpGet("cars")]
    public async Task<IActionResult> GetCars(CancellationToken ct = default)
    {
        var cars = await carService.GetCarsAsync();
        
        var result = cars.Select(car => new
            {
                id           = car.Id,
                brand        = car.Brand,
                color        = car.Color,
                price        = car.Price,
                car_photo    = car.Photo is null
                    ? null
                    : new
                    {
                        id             = Convert.ToInt32(car.Photo.Id),
                        name           = car.Photo.PhotoName,
                        access_method  = car.Photo.Method.ToString(),
                        access_value   = car.Photo.Value
                    }
            })
            .ToList();

        return Ok(result);
    }
    

    /// <summary> HTTP API для hard удаления машины <see cref="CarService.DeleteCarByIdAsync"/> </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCar(int id) => Ok(await carService.DeleteCarByIdAsync(id));
}
