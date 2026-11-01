namespace NomenclatureClient.Network;

public static class NetworkConstants
{
#if DEBUG
    public const string BaseUrl = "https://dotnet.local:5006";
    
#else
    // Server URL 
    public const string BaseUrl = "https://nomenclature.mistydawnsvc.com";
#endif
    public const string HubUrl = "/nomenclature";
    public const string AuthPostUrl = "/api/auth/login";
    public const string RegisterPostUrlInit = "/registration/initiate";
    public const string RegisterPostUrlPoll = "/registration/validate";
}