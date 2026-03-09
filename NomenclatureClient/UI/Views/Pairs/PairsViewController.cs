using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Network;
using NomenclatureCommon.Domain.Network.Pairs;
using NomenclatureCommon.Domain.Network.Pairs.AddPair;
using NomenclatureCommon.Domain.Network.Pairs.RemovePair;
using NomenclatureCommon.Domain.Network.Responses;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace NomenclatureClient.UI.Views.Pairs;

public class PairsViewController(IPluginLog pluginLog, PairService pairService, NetworkService networkService)
{
    public string guh = "";
    private IReadOnlyDictionary<string, PairDto> _pairs => pairService.PairsBySyncCode;
    public IReadOnlyDictionary<string, PairDto> OnlinePairs => _pairs.Where(i => i.Value is OnlinePairDto).ToImmutableDictionary();
    public IReadOnlyDictionary<string, PairDto> OfflinePairs => _pairs.Where(i => i.Value is OfflinePairDto).ToImmutableDictionary();
    public IReadOnlyDictionary<string, PairDto> PendingPairs => _pairs.Where(i => i.Value is PendingPairDto).ToImmutableDictionary();

    public async Task Remove(string item)
    {
        var res = await networkService.InvokeAsync<PairResponse>(HubMethod.RemovePair, new RemovePairRequest(item));
        if(res.Code == PairResponseErrorCode.Success)
        {
            pairService.Remove(item);
        }
    }

    public async void Add()
    {
        var res = await networkService.InvokeAsync<PairResponse<PairDto>>(HubMethod.AddPair, new AddPairRequest(guh));
        if(res.Code == PairResponseErrorCode.Success || res.Code == PairResponseErrorCode.PairPending)
        {
            if(res.Value is null)
            {
                pluginLog.Error("Pair added succesfully, but no data was returned! Did something go wrong?");
            }
            else
            {
                pairService.Add(res.Value);
            }
        }
    }

}