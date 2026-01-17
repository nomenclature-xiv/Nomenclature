using NomenclatureCommon.Domain;

namespace NomenclatureClient.Types.Configurations;

public class CharacterConfigurationV2
{
    /// <summary>
    ///     The expected version. Any versions lower than this number will need to be converted upon loading
    /// </summary>
    public const int ExpectedVersion = 1;
    
    /// <summary>
    ///     Configuration version
    /// </summary>
    public int Version = ExpectedVersion;

    /// <summary>
    ///     Name of the character this configuration belongs to
    /// </summary>
    public string Name = string.Empty;

    /// <summary>
    ///     World of the character this configuration belongs to
    /// </summary>
    public string World = string.Empty;

    /// <summary>
    ///     The secret id this character should connect with
    /// </summary>
    public string? SecretId;

    /// <summary>
    ///     Should this character automatically try to connect
    /// </summary>
    public bool AutoConnect;

    /// <summary>
    ///     The saved / active nomenclature
    /// </summary>
    public Nomenclature Nomenclature = new();
}