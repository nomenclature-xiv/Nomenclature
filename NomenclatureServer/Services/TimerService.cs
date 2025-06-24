using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System.Timers;
using Microsoft.Extensions.Logging;
using NomenclatureCommon.Domain.Network;
using NomenclatureCommon.Domain.Network.UpdateUserCount;
using NomenclatureServer.Hubs;

namespace NomenclatureServer.Services;

public class TimerService(ConnectionService connections, IHubContext<NomenclatureHub> hub, ILogger<TimerService> logger)
    : IHostedService
{
    private const int UserCountInternal = 60000;
    private readonly System.Timers.Timer _userCountTimer = new() { Interval = UserCountInternal, Enabled = true };

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _userCountTimer.Elapsed += UserCount;
        _userCountTimer.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _userCountTimer.Elapsed -= UserCount;
        return Task.CompletedTask;
    }

    private void UserCount(object? sender, ElapsedEventArgs e)
    {
        try
        {
            var count = new UpdateUserCountForwardedRequest(connections.Connections.Count);
            hub.Clients.All.SendAsync(HubMethod.UpdateUserCount, count);
        }
        catch (Exception exception)
        {
            logger.LogError("Unknown exception sending updated user account to all clients, {}", exception);
        }
    }
}