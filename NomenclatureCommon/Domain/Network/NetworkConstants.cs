namespace NomenclatureClient.Network;

public static class NetworkConstants
{
    public const string BaseUrl = "https://nomenclature.mistydawnsvc.com";

    public const string HubUrl = "/nomenclature";
    public const string AuthPostUrl = "/api/auth/login";
    public const string RegisterPostUrlInit = "/registration/initiate";
    public const string RegisterPostUrlValidate = "/registration/validate";
}