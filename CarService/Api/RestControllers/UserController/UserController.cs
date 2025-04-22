using System.Text.Json;
using CarService.Models.User.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CarService.Api.RestControllers.UserController;

[ApiController]
[Route("api/user")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpPost("signup")]
    public async Task<IActionResult> Registration()
    {
        var body =  HttpContext.Request.Body;
        
        var signUpData = JsonSerializer.Deserialize<RegistrationRequestDto>(body);
        
        if (signUpData is null)
            return BadRequest();
        
        var user = await userService.CreateUserAsync(signUpData);
    }
}