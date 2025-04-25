using AutoMapper;
using Models.Bridge.Photo;

namespace CarService.Profiles;

public class CarProfileForApi : Profile
{
    public CarProfileForApi()
    {
        CreateMap<IFormFile, PhotoData>()
            .ConvertUsing((src, dest, ctx) =>
            {
                if (src is null)
                    throw new ArgumentNullException(nameof(src));
            
                return new PhotoData
                {
                    Content       = src.OpenReadStream(),
                    FileExtension = Path.GetExtension(src.FileName)
                };
            });
    }
}