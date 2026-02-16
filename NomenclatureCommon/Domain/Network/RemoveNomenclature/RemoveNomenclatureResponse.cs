using MessagePack;

namespace NomenclatureCommon.Domain.Network.RemoveNomenclature;

[MessagePackObject]
public record RemoveNomenclatureResponse(
    [property: Key(0)] RequestErrorCode ErrorCode
);