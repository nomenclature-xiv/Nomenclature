using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;
using MessagePack.Resolvers;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using NomenclatureClient.Managers;
using NomenclatureCommon.Domain;
using System.Threading;
using System.Threading.Tasks;
using static FFXIVClientStructs.FFXIV.Client.UI.Info.InfoProxyCommonList;

namespace NomenclatureClient.Ipc;

public class IpcHandler(IDalamudPluginInterface plugin, IObjectTable objectTable, IClientState clientState, IdentityManager identityManager) : IHostedService
{
    private const string Namespace = "Nomenclature";

    private static ICallGateProvider<string>? GetNomenclature;
    private static ICallGateProvider<int, string, uint, object>? SetNomenclature;
    private static ICallGateProvider<string, uint, object>? SetNomenclatureSelf;
    private static ICallGateProvider<string, object>? NomenclatureChanged;
    public Task StartAsync(CancellationToken cancellationToken)
    {
        GetNomenclature = plugin.GetIpcProvider<string>($"{Namespace}.{nameof(GetNomenclature)}");
        GetNomenclature.RegisterFunc(() =>
        {
            var nom = identityManager.GetNomenclature();
            var res = JsonConvert.SerializeObject(nom);
            return res;
        });
        SetNomenclature = plugin.GetIpcProvider<int, string, uint, object>($"{Namespace}.{nameof(SetNomenclature)}");
        SetNomenclature.RegisterAction((index, nomenJson, uflags) =>
        {
            var character = objectTable.Length > index && index >= 0 ? objectTable[index] : null;
            if (character is not IPlayerCharacter playerCharacter) return;
            DoSetNomenclature(playerCharacter, nomenJson, uflags);
        });
        SetNomenclatureSelf = plugin.GetIpcProvider<string, uint, object>($"{Namespace}.{nameof(SetNomenclatureSelf)}");
        SetNomenclatureSelf.RegisterAction((nomenJson, uflags) =>
        {
            DoSetNomenclature(clientState.LocalPlayer, nomenJson, uflags);
        });
        NomenclatureChanged = plugin.GetIpcProvider<string, object>($"{Namespace}.{nameof(NomenclatureChanged)}");
        return Task.CompletedTask;
    }

    private void DoSetNomenclature(IPlayerCharacter character, string nomenJson, uint uflags)
    {
        var flags = (UpdateNomenclatureFlag)uflags;
        if (nomenJson == string.Empty || flags is UpdateNomenclatureFlag.None) return;
        Nomenclature? nom = JsonConvert.DeserializeObject<Nomenclature>(nomenJson);
        if (nom == null) return;
        if (flags.HasFlag(UpdateNomenclatureFlag.Name))
            identityManager.SetName(nom.Name);
        if (flags.HasFlag(UpdateNomenclatureFlag.World))
            identityManager.SetWorld(nom.World);
    }

    internal static void ChangedNomenclature(Nomenclature? nomenclature)
    {
        var json = nomenclature is null ? string.Empty : JsonConvert.SerializeObject(nomenclature);
        NomenclatureChanged?.SendMessage(json);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        GetNomenclature?.UnregisterFunc();
        SetNomenclature?.UnregisterAction();
        NomenclatureChanged = null;
        return Task.CompletedTask;
    }
}