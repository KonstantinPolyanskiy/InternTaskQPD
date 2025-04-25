namespace Car.App.Models;

public class CarResult : CarData
{
    public int Id { get; set; }
}

public class CarData 
{
    public string? IdTermPhoto { get; set; }
    
    public string? Brand { get; set; }
    
    public string? Color { get; set; }
    
    public decimal? Price { get; set; }
}