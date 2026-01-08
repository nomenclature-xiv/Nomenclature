using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace NomenclatureClient.Services;

/// <summary>
///     TODO
/// </summary>
public class PairsService : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}