using System.Net;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ApplicationResult.Extensions;
using QPDCar.Models.ApplicationModels.Events;
using QPDCar.Models.BusinessModels.CarModels;
using QPDCar.Models.DtoModels.CarDtos;
using QPDCar.ServiceInterfaces;
using QPDCar.ServiceInterfaces.Publishers;
using QPDCar.UseCases.Helpers;
using QPDCar.UseCases.Models.CarModels;

namespace QPDCar.UseCases.UseCases.ConsumerUseCases;

/// <summary> Кейсы с машинами для клиента </summary>
public class CarConsumerUseCases(ICarService carService, ICartService cartService, INotificationPublisher publisher, ILogger<CarConsumerUseCases> logger)
{
    /// <summary> Машина по Id для клиента </summary>
    public async Task<ApplicationExecuteResult<CarUseCaseResponse>> CarById(int carId)
    {
        logger.LogInformation("Попытка клиентом получить машину {id}", carId);
        
        var carResult = await carService.ByIdAsync(carId);
        if (carResult.IsSuccess is false)
            return ApplicationExecuteResult<CarUseCaseResponse>.Failure().Merge(carResult);
        var car = carResult.Value!;
        
        if (car.IsSold)
            return ApplicationExecuteResult<CarUseCaseResponse>.Failure()
                .WithCritical(CarErrorHelper.ErrorCarIsSoldWarning(car.Id));

        var resp = CarHelper.BuildRestrictedResponse(car);

        return ApplicationExecuteResult<CarUseCaseResponse>.Success(resp);
    }

    /// <summary> Машины по параметрам для клиента </summary>
    public async Task<ApplicationExecuteResult<CarUseCaseResponsePage>> CarsByParams(DtoForSearchCars parameters)
    {
        logger.LogInformation("Попытка получить данные о машинах по параметрам клиентом");

        var warns = new List<ApplicationError>();
        
        var carsPageResult = await carService.ByParamsAsync(parameters);
        if (carsPageResult.IsSuccess is false)
            return ApplicationExecuteResult<CarUseCaseResponsePage>.Failure().Merge(carsPageResult);
        var carsPage = carsPageResult.Value!;
        warns.AddRange(carsPageResult.GetWarnings);
        
        var response = new CarUseCaseResponsePage
        {
            PageSize = carsPage.PageSize,
            PageNumber = carsPage.PageNumber,
            Cars = carsPage.Cars.Where(c => c.IsSold == false).Select(CarHelper.BuildRestrictedResponse).ToList()
        };

        return ApplicationExecuteResult<CarUseCaseResponsePage>.Success(response).WithWarnings(warns);
    }

    /// <summary> Добавить машину в корзину </summary>
    public async Task<ApplicationExecuteResult<DomainCarsInCart>> AddCarToCart(int carId, ClaimsPrincipal consumerClaims)
    {
        var consumerId = Guid.Parse(consumerClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        // Добавляем машину
        var addingResult = await cartService.AddCarAsync(consumerId, carId);
        if (addingResult.IsSuccess is false)
            return ApplicationExecuteResult<DomainCarsInCart>.Failure().Merge(addingResult);
        
        var carsInCartResult = await CarsInCart(consumerId);
        if (carsInCartResult.IsSuccess is false)
            return ApplicationExecuteResult<DomainCarsInCart>.Failure().Merge(carsInCartResult);
        var cart = carsInCartResult.Value!;
        
        return ApplicationExecuteResult<DomainCarsInCart>.Success(cart).WithWarnings(carsInCartResult.GetWarnings);
    }

    /// <summary> Удалить машину из корзины </summary>
    public async Task<ApplicationExecuteResult<DomainCarsInCart>> RemoveCarFromCart(int carId, ClaimsPrincipal consumerClaims)
    {
        var consumerId = Guid.Parse(consumerClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var removingResult = await cartService.RemoveCarAsync(consumerId, carId);
        if (removingResult.IsSuccess is false)
            return ApplicationExecuteResult<DomainCarsInCart>.Failure().Merge(removingResult);
        
        var carsInCartResult = await CarsInCart(consumerId);
        if (carsInCartResult.IsSuccess is false)
            return ApplicationExecuteResult<DomainCarsInCart>.Failure().Merge(carsInCartResult);
        var cart = carsInCartResult.Value!;
        
        return ApplicationExecuteResult<DomainCarsInCart>.Success(cart).WithWarnings(carsInCartResult.GetWarnings);
    }
    
    /// <summary> Получить корзину </summary>
    public async Task<ApplicationExecuteResult<DomainCarsInCart>> Cart(ClaimsPrincipal consumerClaims)
    {
        var consumerId = Guid.Parse(consumerClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var cartResult = await CarsInCart(consumerId);
        if (cartResult.IsSuccess is false)
            return ApplicationExecuteResult<DomainCarsInCart>.Failure().Merge(cartResult);
        var cart = cartResult.Value!;
        
        return ApplicationExecuteResult<DomainCarsInCart>.Success(cart).WithWarnings(cartResult.GetWarnings);
    }

    /// <summary> Выкупить корзину </summary>
    public async Task<ApplicationExecuteResult<SoldDomainCarsInfo>> BuyCarInCart(ClaimsPrincipal consumerClaims)
    {
        var consumerId = Guid.Parse(consumerClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        // Получаем корзину пользователя
        var cartResult = await CarsInCart(consumerId);
        if (cartResult.IsSuccess is false)
            return ApplicationExecuteResult<SoldDomainCarsInfo>.Failure().Merge(cartResult);
        var cart = cartResult.Value!;
        
        // Продаем машины в ней
        var checkoutResult = await cartService.CheckoutAsync(consumerId);
        if (checkoutResult.IsSuccess is false)
            return ApplicationExecuteResult<SoldDomainCarsInfo>.Failure().Merge(checkoutResult);
        
        // Отправляем email менеджерам
        foreach (var car in cart.Cars)
        {
            await publisher.NotifyAsync(new EmailNotificationEvent
            {
                MessageId = Guid.NewGuid(),
                To = car.Manager!.Email,
                Subject = "Машина продана",
                BodyHtml = $"Вашу машину {car.Id} купил клиент {consumerId}"
            });
        }

        var info = new SoldDomainCarsInfo
        {
            TotalCount = cart.TotalCount,
            TotalCost = cart.TotalCost
        };
        
        return ApplicationExecuteResult<SoldDomainCarsInfo>.Success(info).
            WithWarnings(cartResult.GetWarnings)
            .WithWarnings(checkoutResult.GetWarnings);
    }
    
    private async Task<ApplicationExecuteResult<DomainCarsInCart>> CarsInCart(Guid userId)
    {
        // Получаем Id машин в карзине
        var carsIdInCartResult = await cartService.CarsAsync(userId);
        if (carsIdInCartResult.IsSuccess is false)
            return ApplicationExecuteResult<DomainCarsInCart>.Failure().Merge(carsIdInCartResult);
        var ids = carsIdInCartResult.Value!;

        var cars = new List<DomainCar>();
        var warns = new List<ApplicationError>();
        
        decimal cost = 0;
        
        // Заполняем ответ и считаем общую стоимость
        foreach (var id in ids)
        {
            var carResult = await carService.ByIdAsync(id);
            if (carResult.IsSuccess is false)
                warns.AddRange(carResult.GetWarnings);
            var car = carResult.Value!;

            cost += (decimal)car.Price!;
            cars.Add(car);
        }
        
        var resp = new DomainCarsInCart
        {
            Cars = cars,
            TotalCount = cars.Count,
            TotalCost = cost
        };
        
        return ApplicationExecuteResult<DomainCarsInCart>.Success(resp).WithWarnings(warns);
    }
}