using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Exceptions;
using NomenclatureServer.Domain;
using NomenclatureServer.Services;
using System.Collections.Concurrent;
using NomenclatureCommon.Domain.Network;
using NomenclatureCommon.Domain.Network.Base;
using NomenclatureCommon.Domain.Network.DeleteNomenclature;
using NomenclatureCommon.Domain.Network.UpdateNomenclature;
using NomenclatureCommon.Domain.Network.UpdateSubscriptions;

namespace NomenclatureServer.Hubs;

// ReSharper disable once ForCanBeConvertedToForeach

[Authorize]
public class NomenclatureHub(ILogger<NomenclatureHub> logger, ConnectionService connectionService) : Hub
{

    /// <summary>
    ///     List of currently applied nomenclatures
    /// </summary>
    private static readonly ConcurrentDictionary<string, Nomenclature> Nomenclatures = new();

    /// <summary>
    ///     Gets a character identifier from the claims provided by the sender
    /// </summary>
    private string CharacterIdentifier => Context.User?.FindFirst(AuthClaimType.CharacterIdentifier)?.Value ??
                                          throw new MissingExpectedClaimException(
                                              "CharacterIdentifier is not present in claims");
    
    [HubMethodName(HubMethod.UpdateNomenclature)]
    public Response UpdateNomenclature(UpdateNomenclatureRequest request)
    {
        // TODO: Remove after testing
        logger.LogInformation("{Request}", request);

        // Get identifier from claims
        var identifier = CharacterIdentifier;

        if (request.Name?.Length > 32 || request.World?.Length > 32)
            return new Response(false);

        // Check if the nomenclature already exists
        if (Nomenclatures.TryGetValue(identifier, out var existingNomenclature))
        {
            // Only update name if included in update mode
            var name = (request.Mode & UpdateNomenclatureMode.Name) == UpdateNomenclatureMode.Name
                ? request.Name
                : existingNomenclature.Name;
            
            // Only update world if included in update mode
            var world = (request.Mode & UpdateNomenclatureMode.World) == UpdateNomenclatureMode.World
                ? request.World
                : existingNomenclature.World;
            
            var nomenclature = new Nomenclature(name, world);

            // Update the identifier and nomenclature
            Nomenclatures[identifier] = nomenclature;

            // Notify everyone in the group that this nomenclature has been updated
            var update = new UpdateNomenclatureForwardedRequest(identifier, nomenclature);
            Clients.Group(identifier).SendAsync(HubMethod.UpdateNomenclature, update);
        }
        else
        {
            var nomenclature = new Nomenclature(request.Name, request.World);
            
            // Update the identifier and nomenclature
            Nomenclatures[identifier] = nomenclature;

            // Notify everyone in the group that this nomenclature has been updated
            var update = new UpdateNomenclatureForwardedRequest(identifier, nomenclature);
            Clients.Group(identifier).SendAsync(HubMethod.UpdateNomenclature, update);
        }

        // Return success
        return new Response(true);
    }

    [HubMethodName(HubMethod.DeleteNomenclature)]
    public Response DeleteNomenclature(DeleteNomenclatureRequest request)
    {
        // TODO: Remove after testing
        logger.LogInformation("{Request}", request);

        // Get identifier from claims
        var identifier = CharacterIdentifier;

        // Remove the identifier from our list of nomenclatures
        Nomenclatures.TryRemove(identifier, out _);

        // Notify everyone in the group that this nomenclature has been removed
        var delete = new DeleteNomenclatureForwardedRequest(identifier);
        Clients.Group(identifier).SendAsync(HubMethod.DeleteNomenclature, delete);

        // Return success
        return new Response(true);
    }

    [HubMethodName(HubMethod.UpdateSubscriptions)]
    public UpdateSubscriptionsResponse UpdateSubscriptions(UpdateSubscriptionsRequest request)
    {
        // TODO: Remove after testing
        logger.LogInformation("{Request}", request);
        
        // Process all the subscriptions to remove
        var unsubscriptions = request.CharacterToUnsubscribeFrom.AsSpan();

        for (var i = 0; i < unsubscriptions.Length; i++)
            Groups.RemoveFromGroupAsync(Context.ConnectionId, unsubscriptions[i]);

        // TODO: Check the number of subscriptions this client has and prevent them subscribing to more than 128

        // Create a dictionary map to get all the new identities subscribed to immediately
        var identities = new Dictionary<string, Nomenclature>();

        // Process all the subscriptions to add
        var subscriptions = request.CharactersToSubscribeTo.AsSpan();
        for (var i = 0; i < subscriptions.Length; i++)
        {
            // Add the caller to the group
            ref var identity = ref subscriptions[i];
            Groups.AddToGroupAsync(Context.ConnectionId, identity);

            // Check if there is a nomenclature, and if there is, add it to the return dictionary
            if (Nomenclatures.TryGetValue(identity, out var nomenclature))
                identities.Add(identity, nomenclature);
        }

        // Return result with newly subscribed to identities
        return new UpdateSubscriptionsResponse(true, identities);
    }
    
    public override Task OnConnectedAsync()
    {
        connectionService.Connections.Add(Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        connectionService.Connections.Remove(Context.ConnectionId);

        // Get identifier from claims
        var identifier = CharacterIdentifier;

        // Remove the identifier from our list of nomenclatures
        Nomenclatures.TryRemove(identifier, out _);

        // Notify everyone in the group that this nomenclature has been removed
        Clients.Group(identifier).SendAsync(HubMethod.DeleteNomenclature, identifier);

        return base.OnDisconnectedAsync(exception);
    }
}