using MessagePack;

namespace NomenclatureCommon.Domain.Api.Server;

[MessagePackObject]
public record SyncNomenclatureUpdateSubscriptionsRequest
{
    [Key(0)] public List<string> namesToSubscribeTo;
    [Key(1)] public List<string> namesToUnsubscribeTo;
}