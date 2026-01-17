using MessagePack;

namespace NomenclatureCommon.Domain.Network.RemoveNomenclature;

[MessagePackObject]
public record RemoveNomenclatureForwardedRequest
{
    [Key(0)] public string SyncCode { get; set; } = string.Empty;

    public RemoveNomenclatureForwardedRequest()
    {
    }
    
    public RemoveNomenclatureForwardedRequest(string syncCode)
    {
        SyncCode = syncCode;
    }
}