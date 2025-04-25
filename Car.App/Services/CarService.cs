using AutoMapper;
using Car.App.Models;
using Car.Dal.Models;
using Car.Dal.Repository;
using Contracts.Dtos;
using Contracts.Shared;
using Contracts.Types;
using Microsoft.VisualBasic;

namespace Car.App.Services;

/// <summary>
/// Сервис для работы с сущностью машины
/// </summary>
public class CarService(
    ICarRepository carRepository, IMapper mapper,
    IPhotoRepository photoRepository, PhotoProcessor photoProcessor
    )
{
    /// <summary> Создать (добавить) машину </summary>
    public async Task<int> CreateCarAsync(AddCarDomain carDto)
    {
        var dataDto = mapper.Map<AddCarEntity>(
            carDto,
            o => o.Items["CarType"] = 
                carDto.Mileage >= 0 && !string.IsNullOrWhiteSpace(carDto.CurrentOwner) ? CarTypes.NewCar : CarTypes.UsedCar);

        int? photoId = null;
        if (carDto.Photo is not null)
        {
            var photo = mapper.Map<CarPhoto>(dataDto.Photo);
            photoId = await photoRepository.SavePhotoAsync(photo);
        }
            
        return await carRepository.SaveCarAsync(dataDto, photoId);
    }

    /// <summary> Получить машину по ее id </summary>
    public async Task<DomainCar> GetCarByIdAsync(int carId) => await ,
    
    
    
    carRepository.GetCarByIdAsync(carId);
    {
        var carEntity = await carRepository.GetCarByIdAsync(carId);
        
        var car = mapper.Map<DomainCar>(carEntity);
        
        return car;
    }
    
    /// <summary>Получить способ получения фотографии</summary>
    public async Task<IPhotoGetter> GetPhotoGetter(int carId, PhotoMethod method)
    {
        ICarPhoto photo = new CarPhoto();
        
        // есть ли такая машина
        var car = await carRepository.GetCarByIdAsync(carId);
        if (car is not null)
        {
            carId = 0;
            
            // есть ли у нее фото 
            if (car.PhotoId is not null)
                photo = await photoRepository.GetPhotoAsync((int)car.PhotoId);
            // else - пробуем в minio 
        }
        
        return photoProcessor.ProcessPhoto(photo, method, carId);
    }

    /// <summary> Обновить машину согласно переданным полям </summary>
    public async Task<DomainCar> UpdateCarAsync(PatchCarDomain carDomain, int id) =>
        await carRepository.UpdateCarAsync(mapper.Map<PatchCarEntity>(carDomain,
            o => o.Items["id"] = id), id)
    /*{
     
        var dataDto = mapper.Map<PatchCarEntity>(carDomain, o => o.Items["Id"] = id);

        var updatedCarEntity = await carRepository.UpdateCarAsync(dataDto, id);
        
        return mapper.Map<DomainCar>(updatedCarEntity);
    }*/
    
    /// <summary> Удалить машину по id </summary>
    public async Task<bool> DeleteCarByIdAsync(int carId) => await carRepository.DeleteCarByIdAsync(carId);

    /// <summary> Получить все машины </summary>
    public async Task<IList<DomainCar>> GetCarsAsync() => (await carRepository.GetAllCarsAsync()).Select(mapper.Map<DomainCar>).ToList();
    
}