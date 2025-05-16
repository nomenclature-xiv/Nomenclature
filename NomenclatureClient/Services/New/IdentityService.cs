using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NomenclatureCommon.Domain;

namespace NomenclatureClient.Services.New;

public class IdentityService() : IHostedService
{
    public static readonly ConcurrentDictionary<string, Nomenclature> Identities = new();
    public static Nomenclature? CurrentNomenclature = null;
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}