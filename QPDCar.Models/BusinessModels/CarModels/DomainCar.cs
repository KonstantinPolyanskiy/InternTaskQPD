using QPDCar.Models.BusinessModels.EmployerModels;
using QPDCar.Models.BusinessModels.PhotoModels;

namespace QPDCar.Models.BusinessModels.CarModels;

/// <summary> Бизнес модель машины </summary>
public class DomainCar 
{
    public int Id { get; set; }
    
    public string? Brand { get; set; }
    public string? Color { get; set; }
    public decimal? Price { get; set; }
    
    public string? CurrentOwner { get; set; }
    public int? Mileage { get; set; }
    
    public bool IsSold { get; set; }
    
    public ConditionTypes CarCondition { get; set; }
    
    public PrioritySaleTypes PrioritySale { get; set; }
    
    public DomainEmployer? Manager { get; set; }
    
    public DomainPhoto? Photo { get; set; }
}

/// <summary> Результат параметризированного запроса с машинами </summary> 
public class DomainCarPage 
{
    public List<DomainCar> Cars { get; init; } = [];
    
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}