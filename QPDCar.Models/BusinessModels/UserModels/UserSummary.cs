using QPDCar.Models.BusinessModels.EmployerModels;

namespace QPDCar.Models.BusinessModels.UserModels;

/// <summary> Данные пользователя в приложении </summary>
public class UserSummary
{
    public string Id { get; init; } = null!;

    public string Login { get; init; } = null!;
    
    public string FirstName { get; init; } = null!;
    
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    
    public bool EmailConfirmed { get; init; }

    public IReadOnlyCollection<ApplicationRoles> Roles { get; set; } = [];
}
