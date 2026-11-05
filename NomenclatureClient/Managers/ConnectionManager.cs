using System.Threading;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game.Emj;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureClient.Types.Extensions;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Network;
using NomenclatureCommon.Domain.Network.InitializeSession;
using NomenclatureCommon.Domain.Network.Pairs;

namespace NomenclatureClient.Managers;

public class ConnectionManager : IHostedService
{
    public bool IsConnected = false;
    
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
        NomenclatureDto? nom = null;
        if (player.Nomenclature.NameBehavior != NomenclatureBehavior.DisplayOriginal &&
            player.Nomenclature.WorldBehavior != NomenclatureBehavior.OverrideOriginal)
        {
            nom = player.Nomenclature.ToNomenclatureDto();
        }
        var request = new InitializeSessionRequest(player.Name, player.World, nom);
        var response = await _network.InvokeAsync<InitializeSessionResponse>(HubMethod.InitializeSession, request).ConfigureAwait(false);
        if (response.ErrorCode is not RequestErrorCode.Success)
            return;

        _pairs.CurrentSyncCode = response.SyncCode;
        foreach (var pair in response.Pairs)
        {
            _pairs.Add(pair);
            if (pair is OnlinePairDto onlinePairDto && onlinePairDto.NomenclatureDto is not null)
                _nomenclatures.Set(onlinePairDto.CharacterName, onlinePairDto.CharacterWorld, onlinePairDto.NomenclatureDto.ToNomenclature());
        }
        _nomenclatures.Set(_configuration.CharacterConfiguration.Name, _configuration.CharacterConfiguration.World, _configuration.CharacterConfiguration.Nomenclature);
        IsConnected = true;
    }

    private Task OnDisconnected()
    {
        _pairs.Clear();
        _nomenclatures.Clear();
        IsConnected = false;
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