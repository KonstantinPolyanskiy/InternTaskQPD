namespace Backend.App.Models.Dto;

public record RefreshTokenDto
{
    public int? Id { get; init; }
    public string? UserId { get; init; }
    public string? RefreshToken { get; init; }
    public string? Jti { get; init; }
    public DateTime? ExpiresUtc { get; init; }
}

/// <summary>
/// Данные для добавления токена в черный список
/// </summary>
public record BlacklistTokenDto
{
    public required string Jti { get; init; }
    public DateTime ExpiresAt { get; init; }
}

/// <summary>
/// Добавленный в черный список токен
/// </summary>
public record BlacklistTokenResultDto
{
    public int Id { get; init; }
    public required string Jti { get; init; }
    public DateTime ExpiresAt { get; init; }
}


