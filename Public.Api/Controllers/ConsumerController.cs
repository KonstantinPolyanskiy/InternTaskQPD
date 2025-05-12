using Microsoft.AspNetCore.Mvc;
using Public.Api.Extensions;
using Public.Api.Models.Requests;
using Public.Models.CommonModels;
using Public.Models.DtoModels.UserDtoModels;
using Public.UseCase.Models;
using Public.UseCase.UseCases.ConsumerUseCases;
using Public.UseCase.UseCases.UserUseCases;

namespace Public.Api.Controllers;

[ApiController]
[Route("api/consumer")]
public class ConsumerController(ConsumerUseCases consumerUseCases, ILogger<ConsumerController> logger) : ControllerBase
{
    [HttpPost("registration")]
    public async Task<IActionResult> ConsumerRegistration([FromQuery] ClientRegistrationRequest req)
    {
        logger.LogInformation("Запрос на регистрацию клиента");
        logger.LogDebug("Запрос на регистрацию клиента, данные запроса - {@request}", req);
        
        // TODO: add mapper
        var data = new DataForConsumerRegistration
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            Login = req.Login,
            Password = req.Password,
            Email = req.Email,
            RequestedUserRole = ApplicationUserRole.Client
        };

        var createdClient = await consumerUseCases.RegistrationClientAsync(data);
        
        return this.ToApiResult(createdClient);
    }
}