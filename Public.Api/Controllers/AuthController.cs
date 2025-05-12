using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Public.Api.Extensions;
using Public.Api.Models.Requests;
using Public.Models.CommonModels;
using Public.Models.DtoModels.UserDtoModels;
using Public.UseCase.UseCases.UserUseCases;

namespace Public.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(UserUseCase userUseCases, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromQuery] ClientLoginRequest req)
    {
        logger.LogInformation("Запрос на авторизацию пользователя с логином {login}", req.Login);
        
        var authTokens = await userUseCases.LoginUserAsync(req.Login, req.Password);
        
        return this.ToApiResult(authTokens);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromQuery] bool all = false)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        logger.LogInformation("Запрос на логаут пользователя с id {id}, глобально - {is global}", userId, all);

        var result = await userUseCases.LogoutUserAsync(User, all);
        
        return this.ToApiResult(result);
    }

    [HttpPost("Refresh")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken)
    {
        logger.LogInformation("Запрос на получение новой пары по refresh токену {refreshToken}", refreshToken);
        
        var newPair = await userUseCases.RefreshAuthPairAsync(refreshToken);
        
        return this.ToApiResult(newPair);
    }
}