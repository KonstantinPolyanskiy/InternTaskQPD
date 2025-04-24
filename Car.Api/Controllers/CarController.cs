using AutoMapper;
using Car.Api.Profiles.Extensions;
using Car.Api.Profiles.Models;
using Contracts.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CarService.Controllers;

[ApiController]
[Route("api/car")]
public class CarController(Car.App.Services.CarService carService, IMapper mapper) : ControllerBase
{
    /// <summary>
    /// HTTP API для создания машины
    /// <see cref="Car.App.Services.CarService.CreateCarAsync"/>
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns>Созданная машина</returns>
    [HttpPost]
    public async Task<IActionResult> AddCar([FromForm] AddCarRequest request, CancellationToken ct = default)
    {
        var dto = mapper.Map<AddedCarServicesDto>(request);
        
        var car = await carService.CreateCarAsync(dto);
        
        var response = mapper.Map<CarResponse>(car);
        
        if (car.Photo?.Content != null)
            response.StandardParameters!.PhotoBase64 = await car.Photo.Content.ToBase64StringAsync(ct);
        
        return Ok(response);
    }
    
    /// <summary>
    /// Обновляет машину по id
    /// </summary>
    /// <param name="request"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<IActionResult> EditCar(PatchCarRequest request, int id)
    {
        var dto = mapper.Map<PatchCarServicesDto>(request);
        
        var updatedCar = await carService.UpdateCarAsync(dto, id);
        
        var response = mapper.Map<CarResponse>(updatedCar);
        
        // Возможно не очень хорошо, но можно возвращать обновленную машину как созданную
        return Ok(response);
    }
    
    /// <summary>
    /// HTTP API для получения машины по Id
    /// <see cref="Car.App.Services.CarService.GetCarByIdAsync"/>
    /// </summary>
    /// <param name="id">Id запрашиваемой машины</param>
    /// <param name="ct">Контекст отмены</param>
    /// <returns>Машина с переданным Id</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCar(int id, CancellationToken ct = default)
    {
        var car = await carService.GetCarByIdAsync(id);
        
        var response = mapper.Map<CarResponse>(car);
        
        if (car.Photo?.Content != null)
            response.StandardParameters!.PhotoBase64 = await car.Photo.Content.ToBase64StringAsync(ct);
        
        return Ok(response);
    }
    
    /// <summary>
    /// HTTP API для получения всех машин
    /// <see cref="Car.App.Services.CarService.GetCarsAsync"/>
    /// </summary>
    /// <param name="ct">Контекст отмены</param>
    /// <returns>Все машины</returns>
    [HttpGet("cars")]
    public async Task<IActionResult> GetCars(CancellationToken ct = default)
    {
        IList<CarResponse> response = new List<CarResponse>();

        foreach (var car in await carService.GetCarsAsync())
        {
            var r = mapper.Map<CarResponse>(car);
            
            if (car.Photo?.Content != null)
                r.StandardParameters!.PhotoBase64 = await car.Photo.Content.ToBase64StringAsync(ct);
            
            response.Add(r);            
        }
        
        return Ok(response);
    }
    
    /// <summary>
    /// HTTP API для hard удаления машины
    /// <see cref="Car.App.Services.CarService.DeleteCarByIdAsync"/>
    /// </summary>
    /// <param name="id">id машины для удаления</param>
    /// <returns>Удалена ли машина</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCar(int id)
    {
        var deleted = await carService.DeleteCarByIdAsync(id);
        
        return Ok(new
        {
            id,
            is_deleted = deleted
        });
    }
}