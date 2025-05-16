using Private.Services.Repositories;
using Private.StorageModels;
using Public.Models.BusinessModels.StorageModels;
using Public.Models.CommonModels;
using Serilog;

namespace Private.Storages.Repositories.PhotoRepositories;

public class PhotoRepository(PhotoPostgresRepository postgresRepository, MinioPhotoRepository minioPhotoRepository)
{
    internal const string EntityName = "PhotoData";
    
}