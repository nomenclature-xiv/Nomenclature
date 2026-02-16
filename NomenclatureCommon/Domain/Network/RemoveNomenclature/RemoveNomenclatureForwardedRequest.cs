using MessagePack;

namespace NomenclatureCommon.Domain.Network.RemoveNomenclature;

[MessagePackObject]
public record RemoveNomenclatureForwardedRequest(
    [property: Key(0)] string SyncCode
);