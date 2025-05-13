using Public.Models.CommonModels;

namespace Public.Models.BusinessModels.UserModels;

public class UserSummary
{
    public string Id { get; init; } = null!;

    public string Login { get; init; } = null!;
    
    public string FirstName { get; init; } = null!;
    
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    
    public bool EmailConfirmed { get; init; }

    public IReadOnlyCollection<ApplicationUserRole> Roles { get; set; } = [];
}

public class UsersSummaryPage
{
    public List<UserSummary> Users { get; set; } = [];

    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}