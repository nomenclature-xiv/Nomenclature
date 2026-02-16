using MessagePack;

namespace NomenclatureCommon.Domain.Network.UpdateNomenclature;

[MessagePackObject]
public record UpdateNomenclatureForwardedRequest(
    [property: Key(0)] string SyncCode,
    [property: Key(1)] NomenclatureDto NomenclatureDto
);