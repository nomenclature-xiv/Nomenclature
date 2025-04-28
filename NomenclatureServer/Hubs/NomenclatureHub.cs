using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Api;
using NomenclatureCommon.Domain.Api.Base;
using NomenclatureCommon.Domain.Api.Server;
using NomenclatureCommon.Domain.Exceptions;
using NomenclatureServer.Domain;
using NomenclatureServer.Services;

namespace NomenclatureServer.Hubs;

// ReSharper disable once ForCanBeConvertedToForeach

[Authorize]
public class NomenclatureHub(NomenclatureService nomenclatureService, ILogger<NomenclatureHub> logger) : Hub
{
    /// <summary>
    ///     Gets a character identifier from the claims provided by the sender
    /// </summary>
    private string CharacterIdentifier => Context.User?.FindFirst(AuthClaimType.CharacterIdentifier)?.Value ??
                                          throw new MissingExpectedClaimException(
                                              "CharacterIdentifier is not present in claims");

    [HubMethodName(ApiMethods.PublishNomenclature)]
    public Response PublishNomenclature(PublishNomenclatureRequest request)
    {
        // TODO: Remove after testing
        logger.LogInformation("{Request}", request);

        // Get identifier from claims
        var identifier = CharacterIdentifier;

        // Check if the nomenclature already exists
        if (nomenclatureService.Nomenclatures.TryGetValue(identifier, out var existingNomenclature))
        {
            // If it does, construct a new nomenclature, only updating values that are non-null
            var name = request.Nomenclature.Name ?? existingNomenclature.Name;
            var world = request.Nomenclature.World ?? existingNomenclature.World;
            var nomenclature = new Nomenclature(name, world);

            // Update the identifier and nomenclature
            nomenclatureService.Nomenclatures[identifier] = nomenclature;

            // Notify everyone in the group that this nomenclature has been updated
            Clients.Group(identifier).SendAsync(ApiMethods.UpdateNomenclatureEvent, nomenclature);
        }
        else
        {
            // Update the identifier and nomenclature
            nomenclatureService.Nomenclatures[identifier] = request.Nomenclature;

            // Notify everyone in the group that this nomenclature has been updated
            Clients.Group(identifier).SendAsync(ApiMethods.UpdateNomenclatureEvent, request.Nomenclature);
        }

        // Return success
        return new Response { Success = true };
    }

    [HubMethodName(ApiMethods.RemoveNomenclature)]
    public Response RemoveNomenclature(RemoveNomenclatureRequest request)
    {
        // TODO: Remove after testing
        logger.LogInformation("{Request}", request);

        // Get identifier from claims
        var identifier = CharacterIdentifier;

        // Remove the identifier from our list of nomenclatures
        nomenclatureService.Nomenclatures.Remove(identifier);

        // Notify everyone in the group that this nomenclature has been removed
        Clients.Group(identifier)
            .SendAsync(ApiMethods.RemoveNomenclatureEvent, new RemoveNomenclatureEventRequest(identifier));

        // Return success
        return new Response { Success = true };
    }

    [HubMethodName(ApiMethods.SyncNomenclatureUpdateSubscriptions)]
    public SyncNomenclatureUpdateSubscriptionsResponse SyncNomenclatureUpdateSubscriptions(SyncNomenclatureUpdateSubscriptionsRequest request)
    {
        // TODO: Remove after testing
        logger.LogInformation("{Request}", request);

        // Process all the subscriptions to remove
        var unsubscriptions = CollectionsMarshal.AsSpan(request.CharacterIdentitiesToUnsubscribeFrom);
        
        for (var i = 0; i < unsubscriptions.Length; i++)
            Groups.RemoveFromGroupAsync(Context.ConnectionId, unsubscriptions[i]);
        
        // TODO: Check the number of subscriptions this client has and prevent them subscribing to more than 128

        // Create a dictionary map to get all the new identities subscribed to immediately
        var identities = new Dictionary<string, Nomenclature>();
        
        // Process all the subscriptions to add
        var subscriptions = CollectionsMarshal.AsSpan(request.CharacterIdentitiesToSubscribeTo);
        for (var i = 0; i < subscriptions.Length; i++)
        {
            // Add the caller to the group
            ref var identity = ref subscriptions[i];
            Groups.AddToGroupAsync(Context.ConnectionId, identity);
            
            // Check if there is a nomenclature, and if there is, add it to the return dictionary
            if (nomenclatureService.Nomenclatures.TryGetValue(identity, out var nomenclature))
                identities.Add(identity, nomenclature);
        }

        // Return result with newly subscribed to identities
        return new SyncNomenclatureUpdateSubscriptionsResponse
        {
            Success = true,
            NewlySubscribedNomenclatures = identities
        };
    }
}