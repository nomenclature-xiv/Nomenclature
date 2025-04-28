using MessagePack;

namespace NomenclatureCommon.Domain.Api.Server;

[MessagePackObject]
public record SyncNameUpdateSubscriptions
{
    [Key(0)] public List<string> namesToSubscribeTo;
    [Key(1)] public List<string> namesToUnsubscribeTo;
}