using MessagePack;

namespace NomenclatureCommon.Domain.Network.UpdateSubscriptions;

[MessagePackObject]
public record UpdateSubscriptionsRequest
{
    /// <summary>
    ///     Full name of the characters to subscribe to name@world
    /// </summary>
    [Key(0)]
    public string[] CharactersToSubscribeTo { get; set; } = [];
    
    /// <summary>
    ///     Full name of the characters to unsubscribe to name@world
    /// </summary>
    [Key(1)]
    public string[] CharacterToUnsubscribeFrom { get; set; } = [];

    public UpdateSubscriptionsRequest()
    {
    }

    public UpdateSubscriptionsRequest(string[] charactersToSubscribeTo, string[] charactersToUnsubscribeFrom)
    {
        CharactersToSubscribeTo = charactersToSubscribeTo;
        CharacterToUnsubscribeFrom = charactersToUnsubscribeFrom;
    }
}