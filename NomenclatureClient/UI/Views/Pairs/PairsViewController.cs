using Microsoft.Extensions.Hosting;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureCommon.Domain.Network;
using NomenclatureCommon.Domain.Network.Pairs;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace NomenclatureClient.UI.Views.Pairs;

public class PairsViewController(PairService pairService, NetworkService networkService)
{
    public string guh = "";
    private IReadOnlyDictionary<string, PairDto> _pairs => pairService.PairsBySyncCode;
    public IReadOnlyDictionary<string, PairDto> OnlinePairs => _pairs.Where(i => i.Value is OnlinePairDto).ToImmutableDictionary();
    public IReadOnlyDictionary<string, PairDto> OfflinePairs => _pairs.Where(i => i.Value is OfflinePairDto).ToImmutableDictionary();
    public IReadOnlyDictionary<string, PairDto> PendingPairs => _pairs.Where(i => i.Value is PendingPairDto).ToImmutableDictionary();

    public async Task Pause(string item)
    {
        var res = await networkService.InvokeAsync<bool>(HubMethod.PausePair, item);
        if(res)
        {
            pairService.
        }
    }

    public async Task Remove(string item)
    {
        var res = await networkService.InvokeAsync<bool>(HubMethod.RemovePair, item);
    }

    public async void Add()
    {
        var res = await networkService.InvokeAsync<bool>(HubMethod.AddPair, guh);
    }

}