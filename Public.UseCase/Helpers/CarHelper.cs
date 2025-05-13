using Public.Models.BusinessModels.CarModels;
using Public.Models.BusinessModels.PhotoModels;
using Public.Models.BusinessModels.UserModels;
using Public.UseCase.Models.CarModels;
using Public.UseCase.Models.PhotoModels;
using Public.UseCase.Models.UserModels;

namespace Public.UseCase.Helpers;

internal static class CarHelper
{
    internal static CarUseCaseResponse BuildFullResponse(
        DomainCar car,
        DomainEmployer manager,
        DomainPhoto? photo)
    {
        var resp = new CarUseCaseResponse
        {
            Id = car.Id,
            Brand = car.Brand!,
            Color = car.Color!,
            Price = (decimal)car.Price!,
            CurrentOwner = car.CurrentOwner,
            Mileage = car.Mileage,
            CarCondition = car.CarCondition,
            PrioritySale = car.PrioritySale,
            Employer = new EmployerUseCaseResponse
            {
                Id = manager.Id,
                FirstName = manager.FirstName,
                LastName = manager.LastName,
                Email = manager.Email,
                Login = manager.Login,
            },
        };

        if (photo != null)
            resp.Photo = new PhotoUseCaseResponse
            {
                MetadataId = photo.Id,
                Extension = photo.Extension,
                PhotoDataId = photo.PhotoDataId,
                PhotoBytes = photo.PhotoData
            };
        
        return resp;
    }

    internal static CarUseCaseResponse BuildRestrictedResponse(
        DomainCar car,
        DomainEmployer manager,
        DomainPhoto? photo)
    {
        // Менеджер - только имя и email
        var limitedManager = new EmployerUseCaseResponse
        {
            FirstName = manager.FirstName,
            LastName  = manager.LastName,
            Email     = manager.Email,
        };

        var resp = new CarUseCaseResponse
        {
            Id = car.Id,
            Brand = car.Brand!,
            Color = car.Color!,
            Price = (decimal)car.Price!,
            CurrentOwner = car.CurrentOwner,
            Mileage = car.Mileage,
            CarCondition = car.CarCondition,
            PrioritySale = null, // не заполняем
            Employer = limitedManager,
        };
        
        if (photo != null)
            resp.Photo = new PhotoUseCaseResponse
            {
                MetadataId = photo.Id,
                Extension = photo.Extension,
                PhotoDataId = photo.PhotoDataId,
                PhotoBytes = photo.PhotoData
            };
        
        return resp;
    }
}