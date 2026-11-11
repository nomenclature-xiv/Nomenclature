using MessagePack;

namespace NomenclatureCommon.Domain.Network.UpdateZone;

[MessagePackObject]
public record UpdateNomenclatureZoneForwardedRequest(
    [property: Key(0)] string CharacterName,
    [property: Key(1)] string CharacterWorld,
    [property: Key(2)] NomenclatureDto? NomenclatureDto);