using System.ComponentModel.DataAnnotations;
using Car.App.Models.Dto;

namespace Car.Dal.Models;

/// <summary>
/// Таблица Photo для непосредственного хранения фото 
/// </summary>
public class PhotoEntity()
{
    public PhotoEntity(PhotoDataDto data) : this()
    {
        PhotoBytes = data.PhotoBytes;
        Extension = data.Extension;
    }

    public PhotoEntity(int id) : this()
    {
        Id = id;
    }
    
    public int Id {get; init;}
    
    public byte[]? PhotoBytes { get; init; }
    
    [StringLength(20)]
    public string? Extension { get; init; }
    
    public int CarId { get; init; }
    
    public CarEntity? Car { get; init; }
}

