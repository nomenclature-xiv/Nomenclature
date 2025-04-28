using MessagePack;

namespace NomenclatureCommon.Domain.Api.Server;

[MessagePackObject]
public record RemoveNomenclatureEventRequest([property: Key(0)] string CharacterIdentifier);