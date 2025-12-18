namespace CA2SOA.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; init; } = "";
    public string Issuer { get; init; } = "CA2SOA";
    public string Audience { get; init; } = "CA2SOA";
    public int ExpiryMinutes { get; init; } = 120;
}