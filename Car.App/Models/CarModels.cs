using Car.App.Exceptions;
using Contracts.Shared;
using Contracts.Types;

namespace Car.App.Models;

public class DomainCar : ICar, IUsedCar, ICarType
{
    public int Id { get; set; }
    public int? PhotoId { get; set; }
    public CarTypes? CarType { get; set; }

    #region ICar

    public string Brand { get; set; }
    public string Color { get; set; }
    public decimal? Price { get; set; }

    #endregion
    
    #region IUsedCar
    
    public int? Mileage { get; set; }
    public string? CurrentOwner { get; set; }
    
    #endregion
}