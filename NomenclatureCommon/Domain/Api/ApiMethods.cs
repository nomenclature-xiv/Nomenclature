namespace NomenclatureCommon.Domain.Api;

public static class ApiMethods
{
    // Client -> Server
    public const string PublishNomenclature = "PublishNomenclature";
    public const string RemoveNomenclature = "RemoveNomenclature";
    public const string SyncNomenclatureUpdateSubscriptions = "SyncNomenclatureUpdateSubscriptions";
    
    // Server -> Client
    public const string RemoveNomenclatureEvent = "RemoveNomenclatureEvent";
    public const string UpdateNomenclatureEvent = "UpdateNomenclatureEvent";
    public const string UpdateUserCountEvent = "UpdateUserCountEvent";
}
