using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace NomenclatureClient.Ipc;

public class IpcManager : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // TODO
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // TODO
        return Task.CompletedTask;
    }
}