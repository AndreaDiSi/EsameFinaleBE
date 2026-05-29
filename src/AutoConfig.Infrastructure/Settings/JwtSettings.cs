namespace AutoConfig.Infrastructure.Settings;

public class JwtSettings
{
    public required string Secret { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public int ExpiresInHours { get; init; } = 24;
}
