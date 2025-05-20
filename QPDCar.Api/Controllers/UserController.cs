using Microsoft.AspNetCore.Mvc;
using QPDCar.Api.Extensions;
using QPDCar.UseCases.UseCases.UserUseCases;

namespace QPDCar.Api.Controllers;

[ApiController]
[Route("api/user")]
public class UserController(UserUseCases userUseCase) : ControllerBase
{
    [HttpPost("email-confirm")]
    public async Task<IActionResult> EmailConfirmation([FromQuery] Guid userId, [FromQuery] string emailToken)
    {
        if (userId == Guid.Empty || string.IsNullOrEmpty(emailToken))
            return BadRequest("Пустой User Id или Email Token");
        
        var result = await userUseCase.EmailConfirm(userId, emailToken);
        
        return this.ToApiResult(result);
    }
}