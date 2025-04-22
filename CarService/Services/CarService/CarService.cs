using AutoMapper;
using CarService.Daos.CarDao;
using CarService.Models.Car;
using CarService.Models.Car.Exceptions;
using CarService.Models.Car.Requests;
using Microsoft.VisualBasic;
using Serilog;

namespace CarService.Services.CarService;

public class CarService(ICarDao carDao, IMapper mapper) : ICarService
{
    public async Task<ICar> CreateCarAsync(CreateCarRequestDto requestDto)
    {
        try
        {
            ICar car; 

            // Считаем что в таком случае авто бу
            if (requestDto.Mileage.HasValue && !string.IsNullOrWhiteSpace(requestDto.CurrentOwner))
                car = mapper.Map<SecondHandCar>(requestDto);
            else 
                car = mapper.Map<BaseCar>(requestDto);

            await carDao.SaveAsync(car);
            
            return car;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while adding new car");
            throw;
        }
    }

    public async Task<bool?> DeleteCarAsync(int id)
    {
        try
        {
            return await carDao.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while deleting car");
            throw;
        }
    }

    public async Task<ICar> UpdateCarAsync(int id, PatchUpdateCarRequestDto updatingCarDto)
    {
        try
        {
            var existingCar = await carDao.GetByIdAsync(id);
            if (existingCar is null)
                throw new InvalidOperationException("Car does not exist");
            
            if (existingCar is SecondHandCar secondHandCar)
                mapper.Map(updatingCarDto, secondHandCar);
            else 
                mapper.Map(updatingCarDto, (BaseCar)existingCar);
            
            var updatedCar = await carDao.UpdateAsync(id, existingCar);
            
            return updatedCar;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while adding new car");
            throw;
        }
    }

    public async Task<ICar> GetCarByIdAsync(int id)
    {
        try
        {
            var car = await carDao.GetByIdAsync(id);
            if (car is null) throw new InvalidOperationException("Car not found");
            
            return car;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while getting car");
            throw;
        }
    }

    public async Task<IList<ICar>> GetAllCarsAsync()
    {
        try
        {
            return await carDao.GetAllCarsAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while getting all cars");
            throw;
        }
    }
}