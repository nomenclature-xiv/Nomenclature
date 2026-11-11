using MessagePack;

namespace NomenclatureCommon.Domain.Network.Zones;

[MessagePackObject]
public record ZonemenclatureDto(
    [property: Key(0)] string CharacterName,
    [property: Key(1)] string CharacterWorld,
    [property: Key(2)] NomenclatureDto Nomenclature
    );