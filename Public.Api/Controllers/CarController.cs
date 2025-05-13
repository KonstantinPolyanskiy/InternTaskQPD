using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Public.Api.Extensions;
using Public.Api.Models.Requests;
using Public.Models.CommonModels;
using Public.Models.DtoModels.CarDtoModels;
using Public.Models.DtoModels.PhotoDtoModels;
using Public.UseCase.UseCases.ConsumerUseCases;
using Public.UseCase.UseCases.ManagerUseCases;

namespace Public.Api.Controllers;

[ApiController]
[Route("api/car")]
public class CarController(EmployerUseCases employerUseCases, ConsumerUseCases consumerUseCases, ILogger<CarController> logger) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = nameof(ApplicationUserRole.Admin) + "," + nameof(ApplicationUserRole.Manager))]
    public async Task<IActionResult> PostCar([FromForm] AddCarRequest req)
    {
        logger.LogInformation("Запрос на внесение новой машины");
        logger.LogDebug("Данные запроса - {@data}", req);
        
        var data = new DtoForAddCar()
        {
            Brand = req.Brand,
            Color = req.Color,
            Price = req.Price,
            CurrentOwner = req.CurrentOwner,
            Mileage = req.Mileage,
            EmployerClaims = User
        };

        if (req.Photo is not null)
        {
            var extension = Path.GetExtension(req.Photo.FileName).ToLowerInvariant();

            await using var ms = new MemoryStream();
            await req.Photo.CopyToAsync(ms);

            data.Photo = new DtoForAddPhoto
            {
                Data = ms.ToArray(),
                RawExtension = extension,
            };
        }

        var addedCar = await employerUseCases.AddNewCar(data);
        
        return this.ToApiResult(addedCar);
    }
    
    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(ApplicationUserRole.Admin) + "," + nameof(ApplicationUserRole.Manager))]
    public async Task<IActionResult> DeleteCar([FromQuery] int id)
    {
        logger.LogInformation("Запрос на удаление машины {id}", id);
        
        var deleted = await employerUseCases.DeleteCarByCarId(id, User);
        
        return this.ToApiResult(deleted);
    }

    [HttpPatch("{id:int}")]
    [Authorize(Roles = nameof(ApplicationUserRole.Admin) + "," + nameof(ApplicationUserRole.Manager))]
    public async Task<IActionResult> PatchCar([FromBody] PatchCarRequest req, [FromQuery] int id)
    {
        logger.LogInformation("Запрос на обновление машины {id}", id);

        var dto = new DtoForUpdateCar
        {
            CarId = id,
            Brand = req.Brand,
            Color = req.Color,
            Price = req.Price,
            CurrentOwner = req.CurrentOwner,
            Mileage = req.Mileage,
            NewManager = Guid.Parse(req.NewManager!),
        };
        
        var car = await employerUseCases.UpdateCarById(dto, User);
        
        return this.ToApiResult(car);
    }
    
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCar(int id)
    {
        logger.LogInformation("Запрос на получение машины с id {id}", id);
        
        var isAuth = User.Identity?.IsAuthenticated == true;
        var isManagerOrAdmin = isAuth 
                               && (User.IsInRole(nameof(ApplicationUserRole.Admin))
                                   || User.IsInRole(nameof(ApplicationUserRole.Manager)));
        
        var car = isManagerOrAdmin ? 
            await employerUseCases.GetCarById(id, User) : 
            await consumerUseCases.GetCarById(id);
        
        return this.ToApiResult(car);
    }

    [HttpGet("params")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCars([FromQuery] CarQueryRequest req)
    {
        logger.LogInformation("Запрос на получение машин по параметрам - {params}", req);
        
        var data = new DtoForSearchCars()
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
        
        var isAuth = User.Identity?.IsAuthenticated == true;
        var isManagerOrAdmin = isAuth 
                               && (User.IsInRole(nameof(ApplicationUserRole.Admin))
                                   || User.IsInRole(nameof(ApplicationUserRole.Manager)));

        var cars = isManagerOrAdmin ? 
            await employerUseCases.GetCarsByParams(data, User) : 
            await consumerUseCases.GetCarsByParams(data);
        
        return this.ToApiResult(cars);
    }
}