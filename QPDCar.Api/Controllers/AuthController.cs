using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPDCar.Api.Extensions;
using QPDCar.Api.Models.Requests;
using QPDCar.UseCases.UseCases.UserUseCases;

namespace QPDCar.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(UserUseCases userUseCases, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromQuery] ClientLoginRequest req)
    {
        logger.LogInformation("Запрос на авторизацию пользователя с логином {login}", req.Login);
        
        var authTokens = await userUseCases.Login(req.Login, req.Password);
        
        return this.ToApiResult(authTokens);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromQuery] bool all = false)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        logger.LogInformation("Запрос на логаут пользователя с id {id}, глобально - {is global}", userId, all);

        var result = await userUseCases.Logout(User, all);
        
        return this.ToApiResult(result);
    }

    [HttpPost("Refresh")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken)
    {
        logger.LogInformation("Запрос на получение новой пары по refresh токену {refreshToken}", refreshToken);
        
        var newPair = await userUseCases.Refresh(refreshToken);
        
        return this.ToApiResult(newPair);
    }
}