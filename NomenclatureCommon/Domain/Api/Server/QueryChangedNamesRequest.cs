using MessagePack;

namespace NomenclatureCommon.Domain.Api.Server;

[MessagePackObject]
public record QueryChangedNamesRequest
{
    [Key(0)] public Character[] Add = [];
    [Key(1)] public Character[] Remove = [];
}