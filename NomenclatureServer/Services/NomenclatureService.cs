using NomenclatureCommon.Domain;

namespace NomenclatureServer.Services;

public class NomenclatureService
{
    public readonly Dictionary<Character, Character> Nomenclatures = new();
}