namespace QPDCar.Models.BusinessModels.EmployerModels;

/// <summary> Бизнес модель сотрудника </summary>
public class DomainEmployer
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    
    public string Email { get; set; } = null!;
    public string Login { get; set; } = null!;

    public List<ApplicationRoles> Roles { get; set; } = new List<ApplicationRoles>();
}