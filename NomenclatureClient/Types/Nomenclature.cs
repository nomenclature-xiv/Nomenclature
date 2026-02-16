using NomenclatureCommon.Domain;

namespace NomenclatureClient.Types;

public class Nomenclature(string name, NomenclatureBehavior nameBehavior, string world, NomenclatureBehavior worldBehavior)
{
    public string Name = name;
    public NomenclatureBehavior NameBehavior = nameBehavior;
    public string World = world;
    public NomenclatureBehavior WorldBehavior = worldBehavior;
    
    /// <summary>
    ///     Create an empty nomenclature object
    /// </summary>
    public static Nomenclature Empty => new(string.Empty, NomenclatureBehavior.DisplayOriginal, string.Empty, NomenclatureBehavior.DisplayOriginal);
}