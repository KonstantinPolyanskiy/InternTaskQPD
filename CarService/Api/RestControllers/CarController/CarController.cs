using System.Text.Json;
using CarService.Models.Car.Requests;
using CarService.Services.CarService;
using Microsoft.AspNetCore.Mvc;

namespace CarService.Api.RestControllers.CarController;

[ApiController]
[Route("api/car")]
public class CarController(ICarService carService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create()
    {
        var form = await HttpContext.Request.ReadFormAsync();

        var photoFile = form.Files["photo"];
        var carJson = form["json"];

        if (photoFile is null || string.IsNullOrEmpty(carJson))
            return BadRequest("Missing photo or car data");

        var carRequest = JsonSerializer.Deserialize<CreateCarRequestDto>(carJson);
        if (carRequest is null)
            return BadRequest("Invalid car data");

        carRequest.Photo = photoFile.FileName; 

        var createdCar = await carService.CreateCarAsync(carRequest);

        return Ok(createdCar);
    }
    
    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] int id)
    {
        if (id <= 0) return BadRequest("Invalid id, can't <= 0");
        
        var isDeleted = await carService.DeleteCarAsync(id);
        
        return Ok(new { id, isDeleted});
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchUpdateCar(int id, [FromBody] PatchUpdateCarRequestDto requestDto)
    {
        var updated = await carService.UpdateCarAsync(id, requestDto);

        return Ok(new { IsUpdated = true, Id = id });
    }
}