using MessagePack;

namespace NomenclatureCommon.Domain.Network.UpdateNomenclature;

[MessagePackObject]
public record UpdateNomenclatureRequest(
    [property: Key(0)] NomenclatureDto NomenclatureDto
);