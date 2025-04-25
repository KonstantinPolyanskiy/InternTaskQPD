namespace Models.Bridge.Photo;

public class CarPhoto : ICarPhoto, IPhotoAccessMethod
{
    #region ICarPhoto

    public int? Id { get; set; }
    
    public string? PhotoName { get; set; }
    
    public PhotoData? Data { get; set; }
    
    #endregion

    public required PhotoMethod Method { get; set; }
    public required string Value { get; set; }
} 