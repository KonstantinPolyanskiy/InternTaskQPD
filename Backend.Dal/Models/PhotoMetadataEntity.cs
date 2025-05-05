using System.ComponentModel.DataAnnotations;
using Backend.App.Models.Dto;
using Enum.Common;

namespace Backend.Dal.Models;

public class PhotoMetadataEntity()
{
    public PhotoMetadataEntity(PhotoMetadataDto dto) : this()
    {
        PhotoId = dto.PhotoId;
        StorageType = dto.StorageType ?? PhotoStorageType.NotExists;
        Extension = dto.Extension ?? PhotoFileExtension.EmptyOrUnknown;
    }
    
    public int Id { get; set; }
    
    public Guid? PhotoId { get; set; }
    
    public PhotoStorageType StorageType { get; set; }
    
    [StringLength(20)]
    public PhotoFileExtension Extension { get; init; }
    
    public int CarId { get; init; }

    public CarEntity Car { get; init; } = null!;
}