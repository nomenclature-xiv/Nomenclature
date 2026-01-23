using Microsoft.Extensions.Hosting;
using NomenclatureCommon.Domain.Network.Pairs;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using NomenclatureCommon.Domain;

namespace NomenclatureClient.Services;

/// <summary>
///     TODO
/// </summary>
public class PairService : IHostedService
{
    private readonly ConcurrentDictionary<string, PairDto> _pairsBySyncCode = [];
    
    public void Add(PairDto pair) => _pairsBySyncCode[pair.SyncCode] = pair;
    public void Clear() => _pairsBySyncCode.Clear();
    public bool Remove(string syncCode) => _pairsBySyncCode.TryRemove(syncCode, out _);
    public PairDto? TryGet(string syncCode) => _pairsBySyncCode.GetValueOrDefault(syncCode);
    public IReadOnlyDictionary<string, PairDto> PairsBySyncCode => _pairsBySyncCode.ToImmutableDictionary();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _pairsBySyncCode["ABCD12345"] = new OnlinePairDto("ABCD12345", false, false, new Nomenclature("", NomenclatureBehavior.DisplayOriginal, "", NomenclatureBehavior.DisplayNothing), "Silent Lightbond", "Diabolos");
        _pairsBySyncCode["12345ABCD"] = new OfflinePairDto("12345ABCD");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}