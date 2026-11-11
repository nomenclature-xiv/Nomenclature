using MessagePack;
using NomenclatureCommon.Domain.Network.Zones;

namespace NomenclatureCommon.Domain.Network.InitializeSession;

[MessagePackObject]
public record InitializeSessionRequest(
    [property: Key(0)] string CharacterName,
    [property: Key(1)] string CharacterWorld,
    [property: Key(2)] NomenclatureDto? NomenclatureDto,
    [property: Key(3)] ZoneDto? ZoneDto
);