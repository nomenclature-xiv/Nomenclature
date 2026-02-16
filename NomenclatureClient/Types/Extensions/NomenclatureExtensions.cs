using NomenclatureCommon.Domain;

namespace NomenclatureClient.Types.Extensions;

/// <summary>
///     Extends the <see cref="Nomenclature"/> class
/// </summary>
public static class NomenclatureExtensions
{
    /// <summary>
    ///     Converts a Nomenclature into the corresponding Dto object
    /// </summary>
    public static NomenclatureDto ToNomenclatureDto(this Nomenclature nomenclature)
    {
        return new NomenclatureDto(nomenclature.Name, nomenclature.NameBehavior, nomenclature.World, nomenclature.WorldBehavior);
    }

    /// <summary>
    ///     Converts a Nomenclature Dto into the corresponding domain object
    /// </summary>
    public static Nomenclature ToNomenclature(this NomenclatureDto nomenclature)
    {
        return new Nomenclature(nomenclature.Name, nomenclature.NameBehavior, nomenclature.World, nomenclature.WorldBehavior);
    }
}