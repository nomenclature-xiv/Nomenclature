using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureClient.Types;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Network;
using NomenclatureCommon.Domain.Network.RemoveZone;
using NomenclatureCommon.Domain.Network.UpdateZone;
using NomenclatureCommon.Domain.Network.Zones;

namespace NomenclatureClient.Managers;

public class ZoneManager(IClientState clientState, IDataManager dataManager, IPluginLog pluginLog, IFramework framework, ICondition condition, IPlayerState playerState, NetworkService network, ConfigurationService config, NomenclatureService nomenclatures) : IHostedService
{
    private List<Character> zonemenclatures = new();
    
    private async void OnConditionChange(ConditionFlag flag, bool state)
    {
        if (!config.CharacterConfiguration?.GlobalSync ?? true)
            return;
        if (flag is not ConditionFlag.BetweenAreas || state is not false)
        {
            return;
        }

        ClearZonemenclatures();
        await UpdateCurrentZone();
    }

    public async Task UpdateCurrentZone()
    {
        var dto = await framework.RunOnFrameworkThread(GetZoneDto);
        await network.InvokeAsync<UpdateZoneResponse>(HubMethod.UpdateZone, new UpdateZoneRequest(dto));
    }

    public async Task RemoveCurrentZone()
    {
        await network.InvokeAsync<RemoveZoneResponse>(HubMethod.RemoveZone, new RemoveZoneRequest());
        ClearZonemenclatures();
    }

    public unsafe ZoneDto GetZoneDto()
    {
        var territory = dataManager.GetExcelSheet<TerritoryType>().GetRow(clientState.TerritoryType);
        var h = HousingManager.Instance();
        var world = playerState.CurrentWorld.RowId;
        ZoneDto dto;
        if (territory.TerritoryIntendedUse.RowId is 13)
        {
            sbyte ward = h->GetCurrentWard();
            byte division = h->GetCurrentDivision();
            dto = new OutsideZoneDto(world, territory.RowId, ward, division);
        }
        else if (h->IsInside())
        {
            ulong house = h->GetCurrentIndoorHouseId().Id;
            dto = new InsideZoneDto(world, territory.RowId, house);
        }
        else if (condition.OnlyAny([ConditionFlag.BoundByDuty]))
        {
            var datacenter = dataManager.GetExcelSheet<World>().GetRow(world).DataCenter.RowId;
            dto = new DutyZoneDto(datacenter, territory.RowId);
        }
        else
        {
            dto = new OutsideZoneDto(world, territory.RowId, null, null);
        }

        return dto;
    }

    public void AddZonemenclature(Character character, Nomenclature nomenclature)
    {
        zonemenclatures.Add(character);
        nomenclatures.Set(character.Name, character.World, nomenclature);
    }

    public void RemoveZonemenclature(Character character)
    {
        zonemenclatures.Remove(character);
        nomenclatures.RemoveNomenclatureForCharacter(character.Name, character.World);
    }

    public void ClearZonemenclatures()
    {
        foreach (var character in zonemenclatures)
        {
            nomenclatures.RemoveNomenclatureForCharacter(character.Name, character.World);
        }
        zonemenclatures.Clear();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        condition.ConditionChange += OnConditionChange;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        condition.ConditionChange -= OnConditionChange;
        return Task.CompletedTask;
    }
}