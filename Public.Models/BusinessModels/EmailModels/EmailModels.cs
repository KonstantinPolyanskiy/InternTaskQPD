namespace Public.Models.BusinessModels.EmailModels;

public record EmailRecipient
{
    public required string EmailAddress { get; init; }
    public required string FirstAndLastName { get; init; } = string.Empty;
}

public record AccountConfirmationMail
{
    public required EmailRecipient Recipient { get; init; }
    public required Uri ConfirmationLink { get; init; }
}