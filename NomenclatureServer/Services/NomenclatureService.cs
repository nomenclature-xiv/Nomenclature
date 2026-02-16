using System.Collections.Concurrent;
using NomenclatureCommon.Domain;

namespace NomenclatureServer.Services;

public class NomenclatureService
{
    private readonly ConcurrentDictionary<string, NomenclatureDto> _nomenclatures = [];

    public void Upsert(string syncCode, NomenclatureDto nomenclatureDto) => _nomenclatures[syncCode] = nomenclatureDto;
    
    public bool Remove(string syncCode) => _nomenclatures.TryRemove(syncCode, out _);
    
    public NomenclatureDto? TryGet(string syncCode) => _nomenclatures.TryGetValue(syncCode, out var nomenclature) ? nomenclature : null;
}