using Microsoft.AspNetCore.Mvc;
using Public.Api.Extensions;
using Public.UseCase.UseCases.UserUseCases;

namespace Public.Api.Controllers;

[ApiController]
[Route("api/user")]
public class UserController(UserUseCase userUseCases, ILogger<UserController> logger) : ControllerBase
{
    [HttpPost("email_confirm")]
    public async Task<IActionResult> EmailConfirmation([FromQuery] Guid userId, [FromQuery] string token)
    {
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(token))
        {
            logger.LogWarning("Попытка подтверждения почты с пустыми параметрами userId/token");
            return BadRequest("Пустой userId или token");
        }
        
        var result = await userUseCases.ConfirmEmailAsync(userId, token);
        
        return this.ToApiResult(result);
    }
}