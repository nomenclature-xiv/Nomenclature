using MessagePack;

namespace NomenclatureCommon.Domain;

[MessagePackObject]
public record NomenclatureDto(
    [property: Key(0)] string Name,
    [property: Key(1)] NomenclatureBehavior NameBehavior,
    [property: Key(2)] string World,
    [property: Key(3)] NomenclatureBehavior WorldBehavior
);