namespace Car.App.Models;

public class NewCar
{
    public int Id { get; private set; }
    public string Brand { get; private set; }
    public string Color { get; private set; }
    public decimal Price { get;  private set; }
    public string? Photo { get; private set; }

    public static NewCar Create(CreateParams parameters, CurentUser user)
    {
        if (user.Role != "Admin")
            new ArgumentException();
        
        return new NewCar
        {
            Id = parameters.Id,
             Brand = parameters.Brand,
             Color = parameters.Color,
        };
    }

    public class CurentUser
    {
        public string Role { get;  set; }
    }

    public class CreateParams
    {
        public int Id { get; set; }
        public required string Brand { get; set; }
        public required string Color { get; set; }
        public decimal Price { get;  set; }
        public string? Photo { get; set; }
    }
}

public class SecondHandCar
{
    public int Id { get; set; }
    public required string Brand { get; set; }
    public required string Color { get; set; }
    public decimal Price { get; set; }
    public string? Photo { get; set; }
    public int? Mileage { get; set; }
    public string? CurrentOwner { get; set; }
}