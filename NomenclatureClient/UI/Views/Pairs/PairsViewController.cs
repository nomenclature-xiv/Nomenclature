using Microsoft.Extensions.Hosting;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureCommon.Domain.Network.Pairs;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NomenclatureClient.UI.Views.Pairs;

public class PairsViewController(PairService pairService, NetworkService networkService)
{
    private IReadOnlyDictionary<string, PairDto> _pairs => pairService.PairsBySyncCode;
    public IReadOnlyDictionary<string, PairDto> OnlinePairs => _pairs.Where(i => i.Value is OnlinePairDto).ToImmutableDictionary();
    public IReadOnlyDictionary<string, PairDto> OfflinePairs => _pairs.Where(i => i.Value is OfflinePairDto).ToImmutableDictionary();

    public void Pause(string item)
    {
    }

    public void Remove(string item)
    {

    }

}