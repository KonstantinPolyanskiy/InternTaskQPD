using Microsoft.Extensions.Logging;
using Private.ServicesInterfaces;
using Public.Models.ApplicationErrors;
using Public.Models.BusinessModels.CarModels;
using Public.Models.BusinessModels.PhotoModels;
using Public.Models.BusinessModels.StorageModels;
using Public.Models.CommonModels;
using Public.Models.DtoModels.CarDtoModels;
using Public.Models.DtoModels.PhotoDtoModels;
using Public.Models.Extensions;
using Public.UseCase.Models;

namespace Public.UseCase.UseCases.CarUseCases;

public class CarUseCase
{
    private readonly ICarService _carService;
    private readonly IPhotoService _photoService;
    
    private readonly ILogger<CarUseCase> _logger;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CarUseCase(ICarService carService, IPhotoService photoService, ILogger<CarUseCase> logger)
    {
        _carService = carService;
        _photoService = photoService;
        
        _logger = logger;
    }

    public async Task<ApplicationExecuteLogicResult<CarAddedResponse>> CreateCarAsync(DataForAddCar dataCar)
    {
        _logger.LogInformation("Попытка внести в систему продаж новую машину");
        _logger.LogDebug("Данные для внесения - {@data}", dataCar);
        
        var warnings = new List<ApplicationError>();
        
        // Сохраняем машину
        // TODO: на mapper
        var createdCar = await _carService.CreateCarAsync(new DtoForCreateCar
        {
            Brand = dataCar.Brand,
            Color = dataCar.Color,
            Price = dataCar.Price,
            Mileage = dataCar.Mileage,
            CurrentOwner = dataCar.CurrentOwner,
        });
        if (createdCar.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarAddedResponse>.Failure().Merge(createdCar);
        warnings.AddRange(createdCar.GetWarnings);
        
        // TODO: add mapper
        var result = new CarAddedResponse
        {
            CarId = createdCar.Value!.Id,
            Brand = createdCar.Value.Brand!,
            Color = createdCar.Value.Color!,
            Price = (decimal)createdCar.Value.Price!,
            Mileage = createdCar.Value.Mileage,
            CurrentOwner = createdCar.Value.CurrentOwner,
        };
        
        // Сохраняем фото
        // TODO: на mapper 
        if (dataCar.AddingPhoto is null)
        {
            return ApplicationExecuteLogicResult<CarAddedResponse>.Success(result).WithWarning(new ApplicationError(
                CarErrors.CarAddedWithoutPhoto, "Машина без фото", "Машина успешно добавлена, но у нее нет фото",
                ErrorSeverity.NotImportant));
        }
        
        // TODO: написать converter и добавить mapper
        var createdPhoto = await _photoService.CreatePhotoAsync(new DtoForCreatePhoto
        {
            CarId = createdCar.Value!.Id,
            PhotoData = dataCar.AddingPhoto.Data,
            PriorityStorageType = StorageTypes.Database,
            Extension = dataCar.AddingPhoto.RawExtension == ".png" ? ImageFileExtensions.Png : ImageFileExtensions.Jpg
        });
        if (createdPhoto.IsSuccess is not true) // если при добавлении машины ее фото не получилось сохранить - это не крит ошибка
        {
            warnings.AddRange(new ApplicationError(
                CarErrors.CarAddedWithoutPhoto, "Машина без фото", "Машина успешно добавлена, но при сохранении ее фото возникла ошибка",
                ErrorSeverity.NotImportant));
        }

        if (createdPhoto.IsSuccess)
        {
            var updatedCar = await _carService.SetPhotoMetadataToCarAsync(createdCar.Value!, createdPhoto.Value!.Id);
            if (updatedCar.IsSuccess is not true) // если при добавлении машины ее фото не получилось сохранить - это не крит ошибка
            {
                warnings.AddRange(new ApplicationError(
                    CarErrors.CarAddedWithoutPhoto, "Машина без фото", "Машина успешно добавлена, но при сохранении ее фото возникла ошибка",
                    ErrorSeverity.NotImportant));
            }
            
            // TODO: add mapper
            result.Photo = new PhotoResponse
            {
                Image = new PhotoDataResponse
                {
                    Id = createdPhoto.Value!.PhotoDataId,
                    Data = createdPhoto.Value!.PhotoData
                },
                Metadata = new PhotoMetadataResponse
                {
                    Id = createdPhoto.Value.Id,
                    Extension = createdPhoto.Value.Extension,
                }
            };
        }
        
        return ApplicationExecuteLogicResult<CarAddedResponse>.Success(result).WithWarnings(warnings);
    }

    public async Task<ApplicationExecuteLogicResult<CarGetResponse>> GetCarAsync(int carId)
    {
        _logger.LogInformation("Попытка получить машину с id {id}", carId);
        
        var warnings = new List<ApplicationError>();

        var car = await _carService.GetCarAsyncById(carId);
        if (car.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarGetResponse>.Failure().Merge(car);

        var photo = await _photoService.GetPhotoByMetadataIdAsync(car.Value!.Photo!.Id);
        if (photo.IsSuccess is not true)
            warnings.Add(new ApplicationError(CarErrors.CarNotFoundPhoto, "Фото не найдено", "Возникла ошибка при получении фото/метаданных фото машины",
                ErrorSeverity.NotImportant));

        // TODO: add mapper
        var response = new CarGetResponse
        {
            CarId = car.Value!.Id,
            Brand = car.Value!.Brand!,
            Color = car.Value.Color!,
            Price = (decimal)car.Value.Price!,
            CurrentOwner = car.Value.CurrentOwner,
            Mileage = car.Value.Mileage,
            Condition = car.Value.CarCondition,
            PrioritySale = car.Value.PrioritySale,
            Photo = new PhotoResponse
            {
                Metadata = new PhotoMetadataResponse
                {
                    Id = photo.Value!.Id,
                    Extension = photo.Value.Extension,
                },
                Image = new PhotoDataResponse
                {
                    Id = photo.Value!.PhotoDataId,
                    Data = photo.Value.PhotoData,
                }
            },
        };

        return ApplicationExecuteLogicResult<CarGetResponse>.Success(response).WithWarnings(warnings);
    }

    public async Task<ApplicationExecuteLogicResult<GetCarsResponse>> GetCarsAsync(DataForSearchCars dataSearch)
    {
        _logger.LogInformation("Попытка поиска машин по параметрам");
        _logger.LogDebug("Параметры - {@data}", dataSearch);
        
        // TODO: add automapper
        var queryForCars = new DtoForSearchCars
        {
            Brands = dataSearch.Brands,
            Colors = dataSearch.Colors, 
            Condition = dataSearch.Condition,
            SortTerm = dataSearch.SortTerm,
            Direction = dataSearch.Direction ?? SortDirection.Ascending,
            PageNumber = dataSearch.PageNumber ?? 1,
            PageSize = dataSearch.PageSize ?? 10,
        };
        
        var domainCars = await _carService.GetCarsAsync(queryForCars);
        if (domainCars.IsSuccess is not true)
            return ApplicationExecuteLogicResult<GetCarsResponse>.Failure().Merge(domainCars);
        
        // Фото не нужны, возвращаем ответ 
        if (dataSearch.PhotoTerm is HavePhotoTermination.WithoutPhoto)
        {
            var cars = domainCars.Value!.DomainCars
                .Select(c => new CarGetResponse
                {
                    CarId        = c.Id,
                    Brand        = c.Brand!,               
                    Color        = c.Color!,              
                    Price        = c.Price!.Value,              
                    CurrentOwner = c.CurrentOwner,
                    Mileage      = c.Mileage,
                    Condition    = c.CarCondition,
                    PrioritySale = c.PrioritySale,
                    Photo        = null                
                })
                .ToArray();

            return ApplicationExecuteLogicResult<GetCarsResponse>.Success(new GetCarsResponse
            {
                Cars       = cars,
                PageNumber = domainCars.Value.PageNumber,
                PageSize   = domainCars.Value.PageSize,
            });
        }
        
        // Отгружаем с фото
        var domainCarsWithPhoto = await GetCarsWithPhoto(domainCars.Value!.DomainCars.ToArray(), dataSearch.PhotoTerm ?? HavePhotoTermination.NoMatter);
        if (!domainCarsWithPhoto.IsSuccess)
            return ApplicationExecuteLogicResult<GetCarsResponse>.Failure().Merge(domainCarsWithPhoto);


        domainCarsWithPhoto.Value!.PageNumber = domainCars.Value.PageNumber;
        domainCarsWithPhoto.Value!.PageSize   = domainCars.Value.PageSize;

        return domainCarsWithPhoto;
    }

    #region Вспомогательные методы

    private async Task<ApplicationExecuteLogicResult<GetCarsResponse>> GetCarsWithPhoto(DomainCar[] cars, HavePhotoTermination photoTerm)
    {
        var carsWithMeta = cars.Where(c => c.Photo != null).ToArray();
        var metaIds      = carsWithMeta.Select(c => c.Photo!.Id).Distinct().ToArray();
        
        Dictionary<int, DomainPhoto> photoDict = new();

        if (metaIds.Length > 0)
        {
            var photos = await _photoService.GetPhotosByMetadataIdAsync(metaIds);
            if (!photos.IsSuccess)
                return ApplicationExecuteLogicResult<GetCarsResponse>.Failure().Merge(photos);

            photoDict = photos.Value!.ToDictionary(p => p.Id);
        }
        
        IEnumerable<DomainCar> carsForResponse =
            photoTerm is HavePhotoTermination.WithPhoto
                ? carsWithMeta               
                : cars;   
        
        var mappedCars = carsForResponse.Select(c =>
        {
            PhotoResponse? photoResp = null;

            if (c.Photo != null && photoDict.TryGetValue(c.Photo.Id, out var fullPhoto))
            {
                photoResp = new PhotoResponse
                {
                    Metadata = new PhotoMetadataResponse
                    {
                        Id = fullPhoto.Id,
                        Extension = fullPhoto.Extension,
                    },
                    Image = new PhotoDataResponse
                    {
                        Id = fullPhoto.PhotoDataId,
                        Data = fullPhoto.PhotoData,
                    }
                };
            }

            return new CarGetResponse
            {
                CarId        = c.Id,
                Brand        = c.Brand!,
                Color        = c.Color!,
                Price        = c.Price!.Value,
                CurrentOwner = c.CurrentOwner,
                Mileage      = c.Mileage,
                Condition    = c.CarCondition,
                PrioritySale = c.PrioritySale,
                Photo        = photoResp 
            };
        }).ToArray();
        
        return ApplicationExecuteLogicResult<GetCarsResponse>.Success(new GetCarsResponse
        {
            Cars       = mappedCars,
            PageNumber = 0,
            PageSize   = 0
        });
    }

    #endregion
}