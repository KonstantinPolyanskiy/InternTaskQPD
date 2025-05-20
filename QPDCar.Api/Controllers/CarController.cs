using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPDCar.Api.Extensions;
using QPDCar.Api.Models.Requests;
using QPDCar.Models.BusinessModels.EmployerModels;
using QPDCar.Models.BusinessModels.PhotoModels;
using QPDCar.Models.DtoModels.CarDtos;
using QPDCar.Models.DtoModels.PhotoDtos;
using QPDCar.UseCases.UseCases.ConsumerUseCases;
using QPDCar.UseCases.UseCases.EmployerUseCases;

namespace QPDCar.Api.Controllers;

[ApiController]
[Route("api/car")]
public class CarController(CarConsumerUseCases carConsumerUseCases, CarEmployerUseCases carEmployerUseCases) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = nameof(ApplicationRoles.Manager))]
    public async Task<IActionResult> PostCar([FromForm] AddCarRequest request)
    {
        var managerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var data = new DtoForSaveCar
        {
            ResponsiveManager = managerId,
            Brand = request.Brand,
            Color = request.Color,
            Price = request.Price,
            CurrentOwner = request.CurrentOwner,
            Mileage = request.Mileage,
        };

        if (request.Photo != null)
        {
            var extension = Path.GetExtension(request.Photo.FileName).ToLowerInvariant().TrimStart('.');

            await using var ms = new MemoryStream();
            await request.Photo.CopyToAsync(ms);

            data.Photo = new DtoForSavePhoto
            {
                Extension = Enum.Parse<ImageFileExtensions>(extension, true),
                PriorityStorageType = PhotoStorageTypes.Database,
                PhotoData = ms.ToArray(), 
            };
        }
        
        var result = await carEmployerUseCases.NewCar(data);

        return this.ToApiResult(result);
    }
    
    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(ApplicationRoles.Manager))]
    public async Task<IActionResult> DeleteCar([FromQuery] int id)
    {
        var deleted = await carEmployerUseCases.DeleteCar(id, User);
        
        return this.ToApiResult(deleted);
    }
    
    [HttpPatch("{id:int}")]
    [Authorize(Roles = nameof(ApplicationRoles.Manager))]
    public async Task<IActionResult> PatchCar([FromBody] PatchCarRequest req, [FromQuery] int id)
    {
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
        
        var car = await carEmployerUseCases.UpdateCar(dto, User);
        
        return this.ToApiResult(car);
    }
    
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCar(int id)
    {
        var isAuth = User.Identity?.IsAuthenticated == true;
        var isManagerOrAdmin = isAuth 
                               && (User.IsInRole(nameof(ApplicationRoles.Admin))
                                   || User.IsInRole(nameof(ApplicationRoles.Manager)));
        
        var car = isManagerOrAdmin ? 
            await carEmployerUseCases.GetCar(id, User) : 
            await carConsumerUseCases.CarById(id);
        
        return this.ToApiResult(car);
    }
    
    [HttpGet("cars")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCars([FromQuery] CarQueryRequest req)
    {
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
                               && (User.IsInRole(nameof(ApplicationRoles.Admin))
                                   || User.IsInRole(nameof(ApplicationRoles.Manager)));

        var cars = isManagerOrAdmin ? 
            await carEmployerUseCases.GetCars(data, User) : 
            await carConsumerUseCases.CarsByParams(data);
        
        return this.ToApiResult(cars);
    }
}