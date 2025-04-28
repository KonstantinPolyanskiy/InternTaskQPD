using System.ComponentModel.DataAnnotations;
namespace Car.Dal.Models;

/// <summary>
/// Таблица Photo для непосредственного хранения фото 
/// </summary>
public class Photo()
{
    public Photo(byte[] bytes, string ext) : this()
    {
        PhotoBytes = bytes;
        Extension = ext;
    }

    public Photo(int id) : this()
    {
        Id = id;
    }
    
    public int Id {get; init;}
    
    public byte[]? PhotoBytes { get; init; }
    
    [StringLength(20)]
    public string? Extension { get; init; }
    
    public Car? Car { get; init; }
}

