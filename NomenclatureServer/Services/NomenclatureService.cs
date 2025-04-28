using NomenclatureCommon.Domain;

namespace NomenclatureServer.Services;

public class NomenclatureService
{
    public readonly Dictionary<string, Nomenclature> Nomenclatures = new();
}