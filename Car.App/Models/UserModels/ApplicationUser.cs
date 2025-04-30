using Models.Shared.User;

namespace Car.App.Models.UserModels;

public class LoginServiceResponse
{
    public ApplicationUser? User { get; set; }
    public bool PasswordIsValid { get; set; }
} 