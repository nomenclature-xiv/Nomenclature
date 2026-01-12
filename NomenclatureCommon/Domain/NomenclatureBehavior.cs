namespace NomenclatureCommon.Domain;

public enum NomenclatureBehavior
{
    /// <summary>
    ///     Display the original name as-is
    /// </summary>
    DisplayOriginal,
    
    /// <summary>
    ///     Overwrite the original value of this field
    /// </summary>
    OverrideOriginal,
    
    /// <summary>
    ///     Do not display this field
    /// </summary>
    DisplayNothing
}