using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using NomenclatureCommon.Domain.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Timers;
using NomenclatureServer.Hubs;

namespace NomenclatureServer.Services
{
    public class TimerService(IHubContext<NomenclatureHub> hub, ConnectionService connections) : IHostedService
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

        public void UserCount(object? sender, ElapsedEventArgs e)
        {
            hub.Clients.All.SendAsync(ApiMethods.UpdateUserCountEvent, connections.Connections.Count);
        }
    }
}
