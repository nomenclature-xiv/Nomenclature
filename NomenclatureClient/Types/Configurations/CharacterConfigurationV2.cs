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
    ///     Should this character override their name
    /// </summary>
    public bool ShouldOverrideName;

    /// <summary>
    ///     Should this character override their world
    /// </summary>
    public bool ShouldOverrideWorld;

    /// <summary>
    ///     The overwritten name of the character
    /// </summary>
    public string? OverrideName;
    
    /// <summary>
    ///     The overwritten world of the character
    /// </summary>
    public string? OverrideWorld;
}