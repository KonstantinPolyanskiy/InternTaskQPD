using Microsoft.AspNetCore.Identity;

namespace CarService.Models.User;

public class ApplicationUser : IdentityUser<int>
{
    public ApplicationUser() { }
    
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}