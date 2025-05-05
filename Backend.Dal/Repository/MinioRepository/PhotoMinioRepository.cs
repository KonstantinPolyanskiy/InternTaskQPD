using Backend.App.Models.Dto;
using Backend.App.Repositories;
using Enum.Common;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Backend.Dal.Repository.MinioRepository;

public class PhotoMinioRepository(IMinioClient minioClient) : IPhotoRepository
{
    private const string PhotoBucket = "minio";
    private PhotoStorageType PhotoStorageConst => PhotoStorageType.Minio;
    
    public async Task<PhotoDto> SavePhotoAsync(PhotoDto data, CancellationToken ct = default)
    {
        bool exist = await minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(PhotoBucket), ct);
        if (!exist)
            await minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(PhotoBucket), ct);

        var id = Guid.NewGuid();
        
        using var ms = new MemoryStream(data.Data!);

        await minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(PhotoBucket)
            .WithObject(id.ToString())
            .WithStreamData(ms)
            .WithObjectSize(ms.Length)
            .WithContentType("image/" + data.Extension),
            ct);

        return new PhotoDto
        {
            Id = id,
            StorageType = PhotoStorageConst,
            Data = ms.ToArray(),
            Extension = data.Extension
        };
    }

    public async Task<PhotoDto?> GetPhotoAsync(Guid id, PhotoStorageType storageType, CancellationToken ct = default)
    {
        try
        {
            var stat = await minioClient.StatObjectAsync(
                new StatObjectArgs()
                    .WithBucket(PhotoBucket)
                    .WithObject(id.ToString()),
                ct);
            
            var ext = stat.ContentType.Split('/').TakeLast(1).ToString();
            
            await using var ms = new MemoryStream();
            await minioClient.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(PhotoBucket)
                    .WithObject(id.ToString())
                    .WithCallbackStream(async stream =>
                    {
                        await stream.CopyToAsync(ms, ct);
                    }),
                ct);

            return new PhotoDto
            {
                Id = id,
                Extension = PhotoFileExtensionHelper.MapExtension(ext!),
                Data = ms.ToArray(),
                StorageType = PhotoStorageConst
            };
        }
        catch (ObjectNotFoundException)
        {
            return null!;
        }
    }

    public async Task DeletePhotoAsync(Guid id, PhotoStorageType storageType, CancellationToken ct = default)
    {
        await minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(PhotoBucket)
                .WithObject(id.ToString()),
            ct);
    }
}