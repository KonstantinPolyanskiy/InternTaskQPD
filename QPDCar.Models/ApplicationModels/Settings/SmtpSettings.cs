namespace QPDCar.Models.ApplicationModels.Settings;

public class SmtpSettings
{
    public required string Host { get; init; }
    public required int Port { get; init; }
        
    public required bool UseSsl { get; init; }
        
    public required string UserName { get; init; }
    public required string Password { get; init; }
        
    public required string FromAddress { get; init; }
        
    public string? FromName { get; init; }
}