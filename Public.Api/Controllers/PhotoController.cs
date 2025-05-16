using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Public.Api.Extensions;
using Public.Models.CommonModels;
using Public.Models.DtoModels.PhotoDtoModels;
using Public.UseCase.UseCases.ManagerUseCases;

namespace Public.Api.Controllers;

[ApiController]
[Route("api/car/photo")]
public class PhotoController(PhotoEmployerUseCases photoEmployerUse, ILogger<PhotoController> logger) : ControllerBase
{
    [HttpPost("{carId:int}")]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = nameof(ApplicationUserRole.Manager))]
    public async Task<IActionResult> AddPhotoCar([FromRoute] int carId, IFormFile photoFile)
    {
        logger.LogInformation("Запрос на установку фото машине {carId}", carId);
        
        var ext = Path.GetExtension(photoFile.FileName).ToLowerInvariant();
        await using var ms = new MemoryStream();
        await photoFile.CopyToAsync(ms);
        
        var car = await photoEmployerUse.AddCarPhoto(carId, User, new DtoForAddPhoto
        {
            Data = ms.ToArray(),
            RawExtension = ext
        });

        return this.ToApiResult(car);
    }

    [HttpGet("{carId:int}")]
    [Authorize(Roles = nameof(ApplicationUserRole.Manager))]
    public async Task<IActionResult> GetPhotoCar([FromRoute] int carId)
    {
        logger.LogInformation("Запрос на получение фото машины {carId}", carId);
        
        var photo = await photoEmployerUse.GetCarPhoto(carId, User);
        
        return this.ToApiResult(photo);
    }

    [HttpDelete("{carId:int}")]
    [Authorize(Roles = nameof(ApplicationUserRole.Manager))]
    public async Task<IActionResult> DeletePhotoCar([FromRoute] int carId)
    {
        var result = await photoEmployerUse.DeleteCarPhoto(carId, User);
        
        return this.ToApiResult(result);
    }
}