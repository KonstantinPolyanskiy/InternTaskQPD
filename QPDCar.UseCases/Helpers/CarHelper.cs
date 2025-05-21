using QPDCar.Models.BusinessModels.CarModels;
using QPDCar.UseCases.Models.CarModels;
using QPDCar.UseCases.Models.PhotoModels;
using QPDCar.UseCases.Models.UserModels;

namespace QPDCar.UseCases.Helpers;

internal static class CarHelper
{
    /// <summary> Строит полный ответ о машине и ее частях для уполномоченного сотрудника </summary>
    internal static CarUseCaseResponse BuildFullResponse(DomainCar car) 
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
            Employer = new EmployerUseCaseResponse()
        };

        if (car.Manager is not null)
        {
            resp.Employer = new EmployerUseCaseResponse
            {
                Id = car.Manager.Id,
                FirstName = car.Manager.FirstName,
                LastName = car.Manager.LastName,
                Email = car.Manager.Email,
                Login = car.Manager.Login,
            };
        }

        if (car.Photo is not null)
        {
            resp.Photo = new PhotoUseCaseResponse
            {
                MetadataId = car.Photo.Id,
                Extension = car.Photo.Extension,
                PhotoDataId = car.Photo.PhotoDataId,
                PhotoBytes = car.Photo.PhotoData
            };
        }
        
        return resp;
    }
    
    /// <summary> Строит частичный ответ о машине и ее частях для клиента </summary>
    internal static CarUseCaseResponse BuildRestrictedResponse(DomainCar car)
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
            PrioritySale = null, // не заполняем
            Employer = new EmployerUseCaseResponse()
        };

        if (car.Manager is not null)
        {
            resp.Employer = new EmployerUseCaseResponse
            {
                FirstName = car.Manager.FirstName,
                LastName = car.Manager.LastName,
                Email = car.Manager.Email,
            };
        }

        if (car.Photo is not null)
        {
            resp.Photo = new PhotoUseCaseResponse
            {
                MetadataId = car.Photo.Id,
                Extension = car.Photo.Extension,
                PhotoDataId = car.Photo.PhotoDataId,
                PhotoBytes = car.Photo.PhotoData
            };
        }
            
        return resp;
    }
}