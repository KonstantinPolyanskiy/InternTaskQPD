using Private.StorageModels;

namespace Private.ServicesInterfaces.Commands.AuthTokenServiceCommands;

/// <summary> Команда для блокировки access токена </summary>
public record BlockAccessTokenCommand
{
    public string? AccessJti { get; init; } = null!;
    
    public ApplicationUserEntity? User { get; init; } = null!;
    
    public bool LogoutAll { get; init; } = false;
    
    public string? Reason { get; init; }
}

public record BlockRefreshTokenCommand
{
    public ApplicationUserEntity User { get; init; } = null!;
    
    public bool LogoutAll { get; init; } = false;
}