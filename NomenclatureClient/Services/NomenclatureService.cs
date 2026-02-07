using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureCommon.Domain;

// ReSharper disable RedundantBoolCompare

namespace NomenclatureClient.Services;

public class NomenclatureService(INamePlateGui namePlateGui) : IHostedService
{
    private readonly ConcurrentDictionary<(string Name, string World), Nomenclature> _nomenclatures = [];
    
    public void SetName(string characterName, string characterWorld, string newName, NomenclatureBehavior behavior)
    {
        if (_nomenclatures.TryGetValue((characterName, characterWorld), out var nomenclature))
        {
            nomenclature.Name = newName;
            nomenclature.NameBehavior = behavior;
        }
        else
        {
            _nomenclatures[(characterName, characterWorld)] = new Nomenclature(newName, behavior, string.Empty, NomenclatureBehavior.DisplayOriginal);
        }
        namePlateGui.RequestRedraw();
    }

    public void SetWorld(string characterName, string characterWorld, string newWorld, NomenclatureBehavior behavior)
    {
        if (_nomenclatures.TryGetValue((characterName, characterWorld), out var nomenclature))
        {
            nomenclature.World = newWorld;
            nomenclature.WorldBehavior = behavior;
        }
        else
        {
            _nomenclatures[(characterName, characterWorld)] = new Nomenclature(string.Empty, NomenclatureBehavior.DisplayOriginal, newWorld, behavior);
        }
        namePlateGui.RequestRedraw();
    }

    public void Set(string characterName, string characterWorld, Nomenclature newNomenclature)
    {
        if (_nomenclatures.TryGetValue((characterName, characterWorld), out var nomenclature))
        {
            nomenclature.Name = newNomenclature.Name;
            nomenclature.NameBehavior = newNomenclature.NameBehavior;
            nomenclature.World = newNomenclature.World;
            nomenclature.WorldBehavior = newNomenclature.WorldBehavior;
        }
        else
        {
            _nomenclatures[(characterName, characterWorld)] = newNomenclature;
        }
        namePlateGui.RequestRedraw();
    }
    
    public void Set(string characterName, string characterWorld, string name, NomenclatureBehavior nameBehavior, string world, NomenclatureBehavior worldBehavior)
    {
        if (_nomenclatures.TryGetValue((characterName, characterWorld), out var nomenclature))
        {
            nomenclature.Name = name;
            nomenclature.NameBehavior = nameBehavior;
            nomenclature.World = world;
            nomenclature.WorldBehavior = worldBehavior;
        }
        else
        {
            _nomenclatures[(characterName, characterWorld)] = new Nomenclature(name, worldBehavior, world, worldBehavior);
        }
        namePlateGui.RequestRedraw();
    }

    public void RemoveNomenclatureForCharacter(string characterName, string characterWorld)
    {
        _nomenclatures.TryRemove((characterName, characterWorld), out _);
        namePlateGui.RequestRedraw();
    }

    public Nomenclature? TryGetNomenclature(string name, string world)
    {
        return _nomenclatures.GetValueOrDefault((name, world));
    }
    
    public void Clear() => _nomenclatures.Clear();
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}