using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureCommon.Domain.Network;
using NomenclatureCommon.Domain.Network.InitializeSession;

namespace NomenclatureClient.Managers;

public class ConnectionManager : IHostedService
{
    private readonly ConfigurationService _configuration;
    private readonly NetworkService _network;
    private readonly PairsService _pairs;
    private readonly NomenclatureManager _nomenclatures;
    
    public ConnectionManager(ConfigurationService configuration, NetworkService network, PairsService pairs, NomenclatureManager nomenclatures)
    {
        _configuration = configuration;
        _network = network;
        _pairs = pairs;
        _nomenclatures = nomenclatures;

        _network.Connected += OnConnected;
        _network.Disconnected += OnDisconnected;
    }
    
    private async Task OnConnected()
    {
        if (_configuration.CharacterConfiguration is not { } player)
            return;

        var request = new InitializeSessionRequest(player.Name, player.World, player.Nomenclature);
        var response = await _network.InvokeAsync<InitializeSessionResponse>(HubMethod.InitializeSession, request).ConfigureAwait(false);
        if (response.ErrorCode is not RequestErrorCode.Success)
            return;

        foreach (var pair in response.Pairs)
        {
        }
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