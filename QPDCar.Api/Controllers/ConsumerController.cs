using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPDCar.Api.Extensions;
using QPDCar.Api.Models.Requests;
using QPDCar.Models.BusinessModels.EmployerModels;
using QPDCar.Models.DtoModels.UserDtos;
using QPDCar.UseCases.UseCases.ConsumerUseCases;

namespace QPDCar.Api.Controllers;

[ApiController]
[Route("api/consumer")]
[Authorize(Roles = nameof(ApplicationRoles.Client))]
public class ConsumerController(ConsumerUseCases consumerUseCases, CarConsumerUseCases carConsumerUseCases, ILogger<ConsumerController> logger) : ControllerBase
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

    [HttpPost("cart")]
    public async Task<IActionResult> AddCarToCart([FromQuery]int carId)
    {
        var cartResult = await carConsumerUseCases.AddCarToCart(carId, User);
        
        return this.ToApiResult(cartResult);
    }

    [HttpGet("cart")]
    public async Task<IActionResult> GetCart()
    {
        var cartResult = await carConsumerUseCases.Cart(User);
        
        return this.ToApiResult(cartResult);
    }

    [HttpDelete("cart")]
    public async Task<IActionResult> DeleteCarInCart([FromQuery]int carId)
    {
        var cartWithoutCar = await carConsumerUseCases.RemoveCarFromCart(carId, User);
        
        return this.ToApiResult(cartWithoutCar);
    }

    [HttpPost("cart/buy")]
    public async Task<IActionResult> BuyCars()
    {
        var soldResult = await carConsumerUseCases.BuyCarInCart(User);
        
        return this.ToApiResult(soldResult);
    }
}