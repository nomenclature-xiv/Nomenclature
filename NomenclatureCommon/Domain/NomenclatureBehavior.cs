using System.ComponentModel.DataAnnotations;

namespace NomenclatureCommon.Domain;

public enum NomenclatureBehavior
{
    /// <summary>
    ///     Display the original name as-is
    /// </summary>
    [Display(Name = "No Change")]
    DisplayOriginal,
    
    /// <summary>
    ///     Overwrite the original value of this field
    /// </summary>
    [Display(Name = "Override Original")]
    OverrideOriginal,
    
    /// <summary>
    ///     Do not display this field
    /// </summary>
    [Display(Name = "Display Nothing")]
    DisplayNothing
}