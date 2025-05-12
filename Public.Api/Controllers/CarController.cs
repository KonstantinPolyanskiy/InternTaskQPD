using Microsoft.AspNetCore.Mvc;
using Public.Api.Extensions;
using Public.Api.Models.Requests;
using Public.UseCase.Models;
using Public.UseCase.UseCases.CarUseCases;

namespace Public.Api.Controllers;

[ApiController]
[Route("api/car")]
public class CarController(CarUseCase carUseCases, ILogger<CarController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> AddNewCar([FromBody] AddCarRequest req)
    {
        logger.LogInformation("Запрос на внесение новой машины");
        logger.LogDebug("Данные запроса - {@data}", req);
        
        // TODO: add mapper
        var data = new DataForAddCar
        {
            Brand = req.Brand,
            Color = req.Color,
            Price = req.Price,
            CurrentOwner = req.CurrentOwner,
            Mileage = req.Mileage,
        };

        if (req.Photo is not null)
        {
            var extension = Path.GetExtension(req.Photo.FileName).ToLowerInvariant();

            await using var ms = new MemoryStream();
            await req.Photo.CopyToAsync(ms);

            data.AddingPhoto = new DataForAddPhoto
            {
                Data = ms.ToArray(),
                RawExtension = extension,
            };
        }

        var addedCar = await carUseCases.CreateCarAsync(data);
        
        return Ok(this.ToApiResult(addedCar));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetCar(int id)
    {
        logger.LogInformation("Запрос на получение машины с id {id}", id);
        
        var car = await carUseCases.GetCarAsync(id);
        
        return Ok(this.ToApiResult(car));
    }

    [HttpGet("many")]
    public async Task<ActionResult> GetCarsByParams([FromQuery] CarQueryRequest req)
    {
        logger.LogInformation("Запрос на получение машин по параметрам - {params}", req);
        
        // TODO: add mapper
        var data = new DataForSearchCars
        {
            Brands = req.Brands,
            Colors = req.Colors,
            Condition = req.Condition,
            SortTerm = req.SortTerm,
            PhotoTerm = req.PhotoTerm,
            Direction = req.Direction,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
        };

        var cars = await carUseCases.GetCarsAsync(data);
        
        return Ok(this.ToApiResult(cars));
    }
}