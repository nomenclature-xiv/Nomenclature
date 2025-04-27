using MessagePack;
using NomenclatureCommon.Domain.Api.Base;

namespace NomenclatureCommon.Domain.Api.Server;

[MessagePackObject]
public record QueryChangedNamesResponse : Response
{
    [Key(1)] public List<CharacterIdentity> Characters = [];
}