namespace NomenclatureCommon.Domain.Network;

public static class HubMethod
{
    public const string InitializeSession = "InitializeSession";
    public const string UpdateOnlineStatus = "UpdateOnlineStatus";
    public const string UpdateNomenclature = "UpdateNomenclature";
    public const string RemoveNomenclature = "RemoveNomenclature";
    public const string AddPair = "AddPair";
    public const string RemovePair = "RemovePair";
    public const string PausePair = "PausePair";
}