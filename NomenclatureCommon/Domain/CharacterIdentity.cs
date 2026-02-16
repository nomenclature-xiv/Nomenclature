using MessagePack;

namespace NomenclatureCommon.Domain;

[MessagePackObject]
public record CharacterIdentity(
    [property: Key(0)] Character Character,
    [property: Key(1)] NomenclatureDto NomenclatureDto
);