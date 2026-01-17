using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NomenclatureCommon.Domain.Network.Pairs;

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
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}