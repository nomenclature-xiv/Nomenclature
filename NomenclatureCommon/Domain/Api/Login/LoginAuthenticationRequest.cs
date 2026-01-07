namespace NomenclatureCommon.Domain.Api.Login;

public record LoginAuthenticationRequest
{
    public Version Version { get; init; } = new();
    public string Secret { get; init; } = string.Empty;
    
    public LoginAuthenticationRequest()
    {
    }

    public LoginAuthenticationRequest(Version version, string secret)
    {
        Version = version;
        Secret = secret;
    }
}