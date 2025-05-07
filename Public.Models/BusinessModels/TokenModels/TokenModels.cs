namespace Public.Models.BusinessModels.TokenModels;

public record ConfirmEmailToken
{
    public int Id { get; init; }
    public Guid UserId { get; init; } 
    public string Token { get; set; } = null!;
}