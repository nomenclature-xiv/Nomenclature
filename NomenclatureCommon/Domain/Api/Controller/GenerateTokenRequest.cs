namespace NomenclatureCommon.Domain.Api.Controller;

public record GenerateTokenRequest
{
    public string Secret { get; set; } = string.Empty;

    public GenerateTokenRequest()
    {
    }

    public GenerateTokenRequest(string secret)
    {
        Secret = secret;
    }
}