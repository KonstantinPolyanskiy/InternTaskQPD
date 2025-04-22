namespace CarService.Models.Role;

public static class ApplicationRoles
{
    public const string Administrator = "Administrator";
    public const string Moderator = "Manager";
    public const string User = "User";
    
    public static List<string> All() => Enum.GetNames(typeof(ApplicationRoles)).ToList();
}