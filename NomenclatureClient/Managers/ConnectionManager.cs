using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Network;
using NomenclatureClient.Services;

namespace NomenclatureClient.Managers;

public class ConnectionManager : IHostedService
{
    private readonly NetworkService _network;
    private readonly PairsService _pairs;
    
    public ConnectionManager(NetworkService network, PairsService pairs)
    {
        _network = network;
        _pairs = pairs;

        _network.Connected += OnConnected;
        _network.Disconnected += OnDisconnected;
    }
    
    private Task OnConnected()
    {
        // TODO: Get pair data here by calling the server with our secret
        return Task.CompletedTask;
    }

    private Task OnDisconnected()
    {
        // TODO: Clear pair data here
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _network.Connected -= OnConnected;
        _network.Disconnected -= OnDisconnected;
        return Task.CompletedTask;
    }
}