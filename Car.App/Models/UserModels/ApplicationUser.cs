using Microsoft.AspNetCore.Identity;

namespace Car.App.Models.UserModels;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName  { get; set; } = null!;
}