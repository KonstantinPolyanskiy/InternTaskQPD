namespace QPDCar.Models.BusinessModels.PhotoModels;

/// <summary> Бизнес модель фото машины </summary>
public class DomainPhoto 
{
    public int Id { get; set; }
    
    public int CarId { get; set; }
    
    
    public ImageFileExtensions Extension { get; set; }
    
    
    public Guid PhotoDataId { get; set; }

    public byte[] PhotoData { get; set; } = null!;
}