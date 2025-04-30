using Car.App.Models.CarModels;
using Car.App.Models.Dto;
using Car.App.Repositories;
using Minio;
using Minio.DataModel.Args;

namespace Car.Dal.Repository.MinioRepository;

public class PhotoMinioRepository(IMinioClient minioClient) : IPhotoRepository
{
    private const string PhotoBucket = "minio";
    private PhotoStorageType PhotoStorageConst => PhotoStorageType.Minio;
    
    public async Task<PhotoResultDto> SavePhotoAsync(PhotoDataDto data, CancellationToken ct = default)
    {
        bool exist = await minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(PhotoBucket), ct);
        if (!exist)
            await minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(PhotoBucket), ct);
        
        var photoName = Guid.NewGuid() + "." + data.Extension;
        
        using var ms = new MemoryStream(data.PhotoBytes);

        await minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(PhotoBucket)
            .WithObject(photoName)
            .WithStreamData(ms)
            .WithObjectSize(ms.Length)
            .WithContentType(data.Extension), ct);

        return new PhotoResultDto
        {
            TermId = photoName,
            PhotoStorage = PhotoStorageConst,
            Bytes = ms.ToArray(),
            Name = photoName,
            Extension = data.Extension
        };
    }

    public Task<PhotoResultDto?> GetPhotoAsync(string searchTerm)
    {
        throw new NotImplementedException();
    }

    public Task DeletePhotoAsync(string searchTerm, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}