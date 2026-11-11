using MessagePack;

namespace NomenclatureCommon.Domain.Network.RemoveZone;

[MessagePackObject]
public record RemoveNomenclatureZoneForwardedRequest(
    [property: Key(0)] string CharacterName,
    [property: Key(1)] string CharacterWorld
    );