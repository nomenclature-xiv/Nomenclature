using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Handlers.Network;
using NomenclatureClient.Network;
using NomenclatureCommon.Domain.Network;
using NomenclatureCommon.Domain.Network.DeleteNomenclature;
using NomenclatureCommon.Domain.Network.UpdateNomenclature;
using NomenclatureCommon.Domain.Network.UpdateUserCount;

namespace NomenclatureClient.Handlers;

public class NetworkHandler(
    NetworkService network,
    DeleteNomenclatureHandler deleteNomenclatureHandler,
    UpdateNomenclatureHandler updateNomenclatureHandler,
    UpdateUserCountHandler updateUserCountHandler) : IHostedService
{
    /// <summary>
    ///     List of subscriptions to incoming server events
    /// </summary>
    private readonly List<IDisposable> _handlers = [];

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _handlers.Add(network.Connection.On<DeleteNomenclatureForwardedRequest>(HubMethod.DeleteNomenclature, deleteNomenclatureHandler.Handle));
        _handlers.Add(network.Connection.On<UpdateNomenclatureForwardedRequest>(HubMethod.UpdateNomenclature, updateNomenclatureHandler.Handle));
        _handlers.Add(network.Connection.On<UpdateUserCountForwardedRequest>(HubMethod.UpdateUserCount, updateUserCountHandler.Handle));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var subscription in _handlers)
            subscription.Dispose();
        return Task.CompletedTask;
    }
}