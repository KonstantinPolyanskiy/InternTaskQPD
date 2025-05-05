using AutoMapper;
using Backend.Api.Models.Requests;
using Backend.Api.Models.Responses;
using Backend.Api.Processors;
using Backend.App.Models.Commands;
using Backend.App.Services.CarService;
using Enum.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.ClientApi;

[ApiController]
[Route("api/car")]
public class CarController(CarService carService, IMapper mapper, PhotoProcessor photoProcessor) : ControllerBase
{
    /// <summary> HTTP API для создания машины <see cref="CarService.CreateCarAsync"/> </summary>
    [HttpPost]
    public async Task<IActionResult> AddCar([FromBody] AddCarRequest addRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        
        var cmd = mapper.Map<CreateCarCommand>(addRequest);
        
        var car = await carService.CreateCarAsync(cmd);
        
        return Ok(mapper.Map<CarResponse>(car));
    }

    [HttpPost("photo/{carId:int}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddCarPhoto([FromForm] SetPhotoToCarRequest request, int carId)
    {
        if (request.Photo.Length == 0 || carId <= 0)
            return BadRequest("нет фотографии или CarId невалидный");
        
        var extension = Path.GetExtension(request.Photo.FileName).TrimStart('.').ToLowerInvariant();
        
        await using var ms = new MemoryStream();
        await request.Photo.CopyToAsync(ms);
        var data = ms.ToArray();

        var cmd = new SetPhotoToCarCommand
        {
            CarId = carId,
            Data = data,
            RawExtension = extension
        };

        var car = await carService.AddPhotoToCarAsync(cmd);
        if (car.Photo is not null)
            car.Photo.PhotoAccessor = photoProcessor.ProcessPhoto(car.Photo, PhotoMethod.Base64);
        
        return Ok(mapper.Map<CarResponse>(car));
    }

    /// <summary> HTTP API для получения машины по Id <see cref="CarService.GetCarByIdAsync"/> </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCar(int id, CancellationToken ct = default)
    {
        var car = await carService.GetCarByIdAsync(new SearchCarCommand {CarId = id});

        return Ok(mapper.Map<CarResponse>(car));
    }

    /// <summary> Обновляет машину по id <see cref="CarService.UpdateCarAsync"/> </summary>
    [HttpPatch("{carId:int}")]
    public async Task<IActionResult> PatchCar(PatchCarRequest patchCarRequest, int carId)
    {
        var cmd = mapper.Map<UpdateCarCommand>(patchCarRequest);
        cmd.Id = carId;
        
        var car = await carService.UpdateCarAsync(cmd);
        
        return Ok(mapper.Map<CarResponse>(car));
    }

    /// <summary> HTTP API для получения всех машин <see cref="CarService.GetCarsAsync"/> </summary>
    [HttpGet("cars")]
    public async Task<IActionResult> GetCars(CancellationToken ct = default)
    {
        var cars = await carService.GetCarsAsync();

        return Ok(cars.Select(mapper.Map<CarResponse>));
    }


    /// <summary> HTTP API для hard удаления машины <see cref="CarService.DeleteCarByIdAsync"/> </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCar(int id)
    {
        var cmd = new DeleteCarCommand { Id = id, HardDelete = true };
        return Ok(new
        {
            is_deleted = await carService.DeleteCarByIdAsync(cmd)
        });
    }
}
