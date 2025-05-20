using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPDCar.Api.Extensions;
using QPDCar.Api.Models.Requests;
using QPDCar.Models.BusinessModels.EmployerModels;
using QPDCar.Models.DtoModels.UserDtos;
using QPDCar.UseCases.UseCases.EmployerUseCases.AdminUseCases;

namespace QPDCar.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = nameof(ApplicationRoles.Admin))]
public class AdminController(AdminUseCases adminUseCases, ILogger<AdminController> logger) : Controller
{
    [HttpPost("user")]
    public async Task<IActionResult> CreateUser([FromBody]DtoForCreateUser req)
    {
        logger.LogInformation("Запрос на создание пользователя");
        
        var user = await adminUseCases.CreateUser(req);

        return this.ToApiResult(user);
    }

    [HttpGet("user/{id}")]
    public async Task<IActionResult> GetUser([FromRoute] string id)
    {
        logger.LogInformation("Запрос на получение пользователя");
        
        var user = await adminUseCases.GetUser(Guid.Parse(id));
        
        return this.ToApiResult(user);
    }

    [HttpDelete("user/{id}")]
    public async Task<IActionResult> BlockUser([FromRoute] string id)
    {
        var deletedResult = await adminUseCases.ChangeBlockStatus(Guid.Parse(id));
        
        return this.ToApiResult(deletedResult);
    }

    [HttpPost("user/logout/{id}")]
    public async Task<IActionResult> LogoutUser([FromRoute] Guid id)
    {
        var logoutResult = await adminUseCases.LogoutByUserId(id);
        
        return this.ToApiResult(logoutResult);
    }

    [HttpGet("user/all")]
    public async Task<IActionResult> GetAllUsers()
    {
        logger.LogInformation("Запрос на получение всех пользователей");

        var users = await adminUseCases.GetUsers();
        
        return this.ToApiResult(users);
    }

    [HttpPatch("user/{id}")]
    public async Task<IActionResult> PatchUser([FromBody]UpdateUserRequest req, [FromRoute] string id)
    {
        logger.LogInformation("Обновление пользователя с данными - {@req}", req);

        var data = new DtoForUpdateUser()
        {
            UserId = Guid.Parse(id),
            FirstName = req.FirstName,
            LastName = req.LastName,
            NewRoles = req.NewRoles,
        };
        
        var user = await adminUseCases.UpdateUserAsync(data);
        
        return this.ToApiResult(user);
    }
}