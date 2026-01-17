using System.Collections.Concurrent;
using NomenclatureCommon.Domain;

namespace NomenclatureServer.Services;

public class NomenclatureService
{
    private readonly ConcurrentDictionary<string, Nomenclature> _nomenclatures = [];

    public void Upsert(string syncCode, Nomenclature nomenclature) => _nomenclatures[syncCode] = nomenclature;
    
    public bool Remove(string syncCode) => _nomenclatures.TryRemove(syncCode, out _);
    
    public Nomenclature? TryGet(string syncCode) => _nomenclatures.TryGetValue(syncCode, out var nomenclature) ? nomenclature : null;
}