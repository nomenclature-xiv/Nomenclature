using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using MessagePack.Resolvers;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using NomenclatureClient.Managers;
using NomenclatureCommon.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace NomenclatureClient.Ipc;

public class IpcHandler(IDalamudPluginInterface plugin, IdentityManager identityManager) : IHostedService
{
    private const string Namespace = "Nomenclature";

    private static ICallGateProvider<string>? GetLocalNomenclature;
    private static ICallGateProvider<string, uint, object>? SetNomenclature;
    private static ICallGateProvider<string, object>? NomenclatureChanged;
    public Task StartAsync(CancellationToken cancellationToken)
    {
        GetLocalNomenclature = plugin.GetIpcProvider<string>($"{Namespace}.{nameof(GetLocalNomenclature)}");
        GetLocalNomenclature.RegisterFunc(() =>
        {
            var nom = identityManager.GetNomenclature();
            var res = JsonConvert.SerializeObject(nom);
            return res;
        });
        SetNomenclature = plugin.GetIpcProvider<string, uint, object>($"{Namespace}.{nameof(SetNomenclature)}");
        SetNomenclature.RegisterAction((nomenJson, uflags) =>
        {
            var flags = (UpdateNomenclatureFlag)uflags;
            if (nomenJson == string.Empty || flags is UpdateNomenclatureFlag.None) return;
            Nomenclature? nom = JsonConvert.DeserializeObject<Nomenclature>(nomenJson);
            if (nom == null) return;
            if (flags.HasFlag(UpdateNomenclatureFlag.Name))
                identityManager.SetName(nom.Name);
            if (flags.HasFlag(UpdateNomenclatureFlag.World))
                identityManager.SetWorld(nom.World);
        });
        NomenclatureChanged = plugin.GetIpcProvider<string, object>($"{Namespace}.{nameof(NomenclatureChanged)}");
        return Task.CompletedTask;
    }

    internal static void ChangedNomenclature(Nomenclature? nomenclature)
    {
        var json = nomenclature is null ? string.Empty : JsonConvert.SerializeObject(nomenclature);
        NomenclatureChanged?.SendMessage(json);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        GetLocalNomenclature?.UnregisterFunc();
        SetNomenclature?.UnregisterAction();
        NomenclatureChanged = null;
        return Task.CompletedTask;
    }
}