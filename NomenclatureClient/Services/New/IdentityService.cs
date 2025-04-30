using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NomenclatureCommon.Domain;

namespace NomenclatureClient.Services.New;

public class IdentityService : IHostedService
{
    public static readonly Dictionary<string, Nomenclature> Identities = new ()
    {
        { "Mora Nightshade@Diabolos", new Nomenclature("Monster", "Cookie") },
        { "Lu'nara Sipros@Balmung", new Nomenclature("Big Boobs", "World") },
    };

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}