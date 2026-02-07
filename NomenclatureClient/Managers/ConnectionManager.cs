using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureCommon.Domain.Network;
using NomenclatureCommon.Domain.Network.InitializeSession;
using NomenclatureCommon.Domain.Network.Pairs;

namespace NomenclatureClient.Managers;

public class ConnectionManager : IHostedService
{
    private readonly ConfigurationService _configuration;
    private readonly NetworkService _network;
    private readonly NomenclatureService _nomenclatures;
    private readonly PairService _pairs;
    
    public ConnectionManager(ConfigurationService configuration, NetworkService network, NomenclatureService nomenclatures, PairService pairs)
    {
        _configuration = configuration;
        _network = network;
        _nomenclatures = nomenclatures;
        _pairs = pairs;

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
            _pairs.Add(pair);
            if (pair is OnlinePairDto onlinePairDto)
                _nomenclatures.Set(onlinePairDto.CharacterName, onlinePairDto.CharacterWorld, onlinePairDto.Nomenclature);
        }
        _nomenclatures.Set(_configuration.CharacterConfiguration.Name, _configuration.CharacterConfiguration.World, _configuration.CharacterConfiguration.Nomenclature);
    }

    private Task OnDisconnected()
    {
        _pairs.Clear();
        _nomenclatures.Clear();
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