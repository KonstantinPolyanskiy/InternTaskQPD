using Microsoft.AspNetCore.Mvc;
using QPDCar.Api.Extensions;
using QPDCar.Api.Models.Requests;
using QPDCar.Models.DtoModels.UserDtos;
using QPDCar.UseCases.UseCases.ConsumerUseCases;

namespace QPDCar.Api.Controllers;

[ApiController]
[Route("api/consumer")]
public class ConsumerController(ConsumerUseCases consumerUseCases, ILogger<ConsumerController> logger) : ControllerBase
{
    [HttpPost("registration")]
    public async Task<IActionResult> ConsumerRegistration([FromQuery] ClientRegistrationRequest req)
    {
        logger.LogInformation("Запрос на регистрацию клиента");
        logger.LogDebug("Запрос на регистрацию клиента, данные запроса - {@request}", req);
        
        var data = new DtoForCreateConsumer()
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            Login = req.Login,
            Password = req.Password,
            Email = req.Email,
        };

        var createdClient = await consumerUseCases.RegisterUser(data);
        
        return this.ToApiResult(createdClient);
    }
}