namespace Car.App.Models.Dto;

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


