namespace Public.Models.BusinessModels.UserModels;

public class DomainEmployer
{
    public Guid Id { get; set; }
    
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    public string Login { get; set; } = null!;

    public string Email { get; set; } = null!;
}