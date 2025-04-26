using MessagePack;
using NomenclatureCommon.Domain.Api.Base;

namespace NomenclatureCommon.Domain.Api.Server;

[MessagePackObject]
public record QueryChangedNamesResponse : Response
{
    [Key(0)] public Dictionary<Character, Character> Characters = [];
}