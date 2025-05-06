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
public class CarController(CarService carService, PhotoProcessor photoProcessor,
    IMapper mapper, ILogger<CarController> log) : ControllerBase
{
    /// <summary> HTTP API для создания машины <see cref="CarService.CreateCarAsync"/> </summary>
    [HttpPost]
    public async Task<IActionResult> AddCar([FromBody] AddCarRequest addRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        
        log.LogDebug("Запрос на добавление машины, данные - {request}", addRequest);
        
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
        
        log.LogDebug("Запрос на установку фото машине, id машины - {carId}, расширение фото - {extension}, размер фото (в МБ)- {data}", carId, extension, data.Length / 1024 / 1024);

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
        var car = await carService.GetCarByIdAsync(new SearchCarByIdCommand {CarId = id});

        return Ok(mapper.Map<CarResponse>(car));
    }

    /// <summary> Обновляет машину по id <see cref="CarService.UpdateCarAsync"/> </summary>
    [HttpPatch("{carId:int}")]
    public async Task<IActionResult> PatchCar(PatchCarRequest patchCarRequest, int carId)
    {
        log.LogDebug("Запрос на изменение машины, id машины - {carId}, данные запроса - {request}", carId, patchCarRequest);
        
        var cmd = mapper.Map<UpdateCarCommand>(patchCarRequest);
        cmd.Id = carId;
        
        var car = await carService.UpdateCarAsync(cmd);
        
        return Ok(mapper.Map<CarResponse>(car));
    }

    /// <summary> HTTP API для получения всех машин <see cref="CarService.GetCarsAsync"/> </summary>
    [HttpGet("cars")]
    public async Task<IActionResult> GetCarQuery([FromQuery] CarQueryRequest request, CancellationToken ct = default)
    {
        log.LogDebug("Параметризованный запрос машин, данные запроса - {request}", request);
        var cmd = mapper.Map<SearchCarByQueryCommand>(request);
        var page = await carService.GetCarsByQueryAsync(cmd);

        if (cmd.PhotoTerm is PhotoHavingTerm.WithPhoto || request.PhotoTerm is PhotoHavingTerm.NoMatter)
        {
            foreach (var car in page.Cars)
            {
                if (car.Photo is not null)
                    car.Photo.PhotoAccessor = photoProcessor.ProcessPhoto(car.Photo, PhotoMethod.DirectLink);
            }
        }
        
        var list = mapper.Map<List<CarResponse>>(page.Cars);
        
        

        return Ok(page);
    }


    /// <summary> HTTP API для hard удаления машины <see cref="CarService.DeleteCarByIdAsync"/> </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCar(int id)
    {
        log.LogDebug("Запрос на удаление машины, id машины - {carId}", id);
        var cmd = new DeleteCarCommand { Id = id, HardDelete = true };
        
        return Ok(new
        {
            is_deleted = await carService.DeleteCarByIdAsync(cmd)
        });
    }
}
