namespace Backend.App.Models.Business;

public class RefreshToken
{
    public int      Id           { get; set; }
    public string   UserId       { get; set; }      = null!;
    public string   Token        { get; set; }      = null!;
    
    public string Jti            { get; set; }      = null!;
    
    public DateTime ExpiresAtUtc { get; set; }

    public ApplicationUser? User    { get; set; }
}

public record TokenPair
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}