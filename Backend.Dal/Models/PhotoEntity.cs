using System.ComponentModel.DataAnnotations;
using Backend.App.Models.Dto;
using Enum.Common;

namespace Backend.Dal.Models;

/// <summary>
/// Таблица Photo для непосредственного хранения фото 
/// </summary>
public class PhotoEntity()
{
    public PhotoEntity(PhotoDto data) : this()
    {
        PhotoBytes = data.Data ?? throw new ArgumentNullException(nameof(data.Data));
    }
    
    public Guid Id {get; init;}
    
    public byte[] PhotoBytes { get; init; } = null!;
}

