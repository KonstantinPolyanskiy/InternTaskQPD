using Microsoft.Extensions.Logging;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ApplicationResult.Extensions;
using QPDCar.Models.DtoModels.CarDtos;
using QPDCar.ServiceInterfaces;
using QPDCar.UseCases.Helpers;
using QPDCar.UseCases.Models.CarModels;

namespace QPDCar.UseCases.UseCases.ConsumerUseCases;

/// <summary> Кейсы с машинами для клиента </summary>
public class CarConsumerUseCases(ICarService carService, ILogger<CarConsumerUseCases> logger)
{
    /// <summary> Машина по Id для клиента </summary>
    public async Task<ApplicationExecuteResult<CarUseCaseResponse>> CarById(int carId)
    {
        logger.LogInformation("Попытка клиентом получить машину {id}", carId);
        
        var carResult = await carService.ByIdAsync(carId);
        if (carResult.IsSuccess is false)
            return ApplicationExecuteResult<CarUseCaseResponse>.Failure().Merge(carResult);
        var car = carResult.Value!;

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
            Cars = carsPage.Cars.Select(CarHelper.BuildRestrictedResponse).ToList()
        };

        return ApplicationExecuteResult<CarUseCaseResponsePage>.Success(response).WithWarnings(warns);
    }
    
}