using MessagePack;
using NomenclatureCommon.Domain.Network.Base;

namespace NomenclatureCommon.Domain.Network.UpdateSubscriptions;

[MessagePackObject]
public record UpdateSubscriptionsResponse : Response
{
    [Key(1)]
    public Dictionary<string, Nomenclature> SubscribedNomenclatures { get; set; } = [];

    public UpdateSubscriptionsResponse()
    {
    }

    public UpdateSubscriptionsResponse(bool success, Dictionary<string, Nomenclature> subscribedNomenclatures)
    {
        Success = success;
        SubscribedNomenclatures = subscribedNomenclatures;
    }
}