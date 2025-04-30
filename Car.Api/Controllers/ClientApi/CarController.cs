using AutoMapper;
using Car.App.Models.Dto;
using Car.App.Services.CarService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Bridge.Car;

namespace CarService.Controllers.ClientApi;

[ApiController]
[Route("api/car")]
public class CarController(Car.App.Services.CarService.CarService carService, IMapper mapper) : ControllerBase
{
    /// <summary> HTTP API для создания машины <see cref="CarService.CreateCarAsync"/> </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddCar([FromBody] AddCarRequest addRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        
        var data = mapper.Map<CarRequestDataDto>(addRequest);
        
        var carId = await carService.CreateCarAsync(data);
        
        return Ok(new {car_id = carId});
    }

    [HttpPost("photo/{carId}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddCarPhoto(IFormFile file, int carId)
    {
        if (file.Length == 0 || carId <= 0)
            return BadRequest("file is empty or carId is invalid");

        var photo = await carService.AddPhotoToCarAsync(new PhotoRequestDto
        {
            CarId = carId,
            Content = file.OpenReadStream(),
            PhotoExtension = Path.GetExtension(file.FileName),
        });

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
    [HttpGet("{id:int}")]
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
    [HttpPatch("{carId:int}")]
    public async Task<IActionResult> Patch(CarRequestDataDto requestDataDto, int carId) => Ok(new
    {
        updated = await carService.UpdateCarAsync(requestDataDto, carId)
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
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCar(int id) => Ok(await carService.DeleteCarByIdAsync(id));
}
