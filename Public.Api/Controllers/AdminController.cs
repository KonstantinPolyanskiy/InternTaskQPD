using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Public.Api.Extensions;
using Public.Api.Models.Requests;
using Public.Models.CommonModels;
using Public.Models.DtoModels.UserDtoModels;
using Public.UseCase.UseCases.AdminUseCases;

namespace Public.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = nameof(ApplicationUserRole.Admin))]
public class AdminController(AdminUseCases adminUseCases, ILogger<AdminController> logger) : Controller
{
    [HttpPost("user")]
    public async Task<IActionResult> CreateUser([FromBody]DataForCreateUser req)
    {
        logger.LogInformation("Запрос на создание пользователя");
        
        var user = await adminUseCases.CreateUserAsync(req);

        return this.ToApiResult(user);
    }

    [HttpGet("user/{id}")]
    public async Task<IActionResult> GetUser([FromRoute] string id)
    {
        logger.LogInformation("Запрос на получение пользователя");
        
        var user = await adminUseCases.GetUserByIdAsync(Guid.Parse(id));
        
        return this.ToApiResult(user);
    }

    [HttpDelete("user/{id}")]
    public async Task<IActionResult> BlockUser([FromRoute] string id)
    {
        var deletedResult = await adminUseCases.BlockUserById(Guid.Parse(id));
        
        return this.ToApiResult(deletedResult);
    }

    [HttpPost("user/logout/{id}")]
    public async Task<IActionResult> LogoutUser([FromRoute] string id)
    {
        var logoutResult = await adminUseCases.LogoutByUserId(id);
        
        return this.ToApiResult(logoutResult);
    }

    [HttpGet("user/all")]
    public async Task<IActionResult> GetAllUsers()
    {
        logger.LogInformation("Запрос на получение всех пользователей");

        var users = await adminUseCases.GetAllUsers();
        
        return this.ToApiResult(users);
    }

    [HttpPatch("user/{id}")]
    public async Task<IActionResult> PatchUser([FromBody]UpdateUserRequest req, [FromRoute] string id)
    {
        logger.LogInformation("Обновление пользователя с данными - {@req}", req);

        var data = new DataForUpdateUser
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