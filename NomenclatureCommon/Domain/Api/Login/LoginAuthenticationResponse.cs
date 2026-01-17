namespace NomenclatureCommon.Domain.Api.Login;

public record LoginAuthenticationResponse
{
    public LoginAuthenticationErrorCode ErrorCode { get; init; }
    public string Secret { get; init; } = string.Empty;

    public LoginAuthenticationResponse()
    {
    }
    
    public LoginAuthenticationResponse(LoginAuthenticationErrorCode errorCode, string? secret = null)
    {
        ErrorCode = errorCode;
        Secret = secret ?? string.Empty;
    }
}