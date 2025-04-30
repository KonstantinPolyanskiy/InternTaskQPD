using Car.App.Models.CarModels;
using Car.App.Models.Dto;
using Car.App.Models.Photo;
using Car.App.Models.PhotoModels;

namespace Car.App.Services.CarService
{
    /// <summary>
    /// Класс с вспомогательными методами для облегчения чтения логики самого сервиса
    /// </summary>
    internal static class CarServiceHelper
    {
        /// <summary>
        /// Конвертирует запрос на сохранение фото в данные для его сохранения 
        /// - <see cref="PhotoStorageType.Database"/> в []byte
        /// </summary>
        /// <exception cref="NotSupportedException">Неподдерживаемый <see cref="PhotoStorageType"/></exception>
        /// <returns>Данные для сохранения в хранилище</returns>
        public static PhotoDataDto PreparePhotoDataDto(PhotoRequestDto dto, PhotoStorageType st, bool onlyPriority)
        {
            if (st is not PhotoStorageType.Database)
                throw new NotSupportedException($"Тип хранилища {st} не поддерживается");
            
            using var ms = new MemoryStream();
            dto.Content.CopyTo(ms);
            var bytes = ms.ToArray();

            return new PhotoDataDto
            {
                PhotoBytes               = bytes,
                Extension                = dto.PhotoExtension,
                PriorityPhotoStorage     = st
            };
        }

        public static CarPhoto PrepareCarPhoto(PhotoResultDto? pr, PhotoProcessor processor, PhotoMethod method)
        {
            ArgumentNullException.ThrowIfNull(pr);

            var pd = new PhotoData
            {
                CarId = pr.CarId ?? throw new ArgumentException("Id машины было null или 0"),
                Extension = pr.Extension ?? throw new ArgumentException("У фото нет расширения"),
                PriorityPhotoStorage = pr.PhotoStorage,
            };

            if (pr.Bytes is not null)
                pd.Content = new MemoryStream(pr.Bytes);

            var carPhoto = new CarPhoto
            {
                Id = pr.CarId.ToString(),
                PhotoName = pr.Name,
                Data = pd
            };
            
            var accessor = processor.ProcessPhoto(carPhoto, method, carPhoto.Id);
            carPhoto.Method = accessor.Method;
            carPhoto.Value = accessor.Value;
            
            return carPhoto;
        }
    }
}