namespace Public.Models.BusinessModels.UserModels;

public class DomainEmployer
{
    public Guid Id { get; set; }
    
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    public string Login { get; set; }
    
    public string Email { get; set; }
}