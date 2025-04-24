namespace NomenclatureCommon.Domain.Api;

/// <summary>
///     Requests a token from the server
/// </summary>
public record TokenRequest
{
    public string Secret { get; set; } = string.Empty;
}