namespace Car.Dal.Models;

/// <summary>
/// Таблица Photo для непосредственного хранения фото 
/// </summary>
public class Photo
{
    public int Id {get; set;}
    
    public byte[]? PhotoBytes { get; set; }
    
    public string? FileName { get; set; }
    
    public string? Extension { get; set; }
    
    public Car? Car { get; set; }
}