using MessagePack;

namespace NomenclatureCommon.Domain.Api.Server;

[MessagePackObject]
public record QueryChangedNamesRequest
{
    [Key(0)] public Character[] Characters = [];
}