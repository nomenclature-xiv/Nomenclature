using System.Collections.Generic;

namespace NomenclatureClient.Types.Configurations;

public class ConfigurationV2
{
    /// <summary>
    ///     The expected version. Any versions lower than this number will need to be converted upon loading
    /// </summary>
    public const int ExpectedVersion = 1;
    
    /// <summary>
    ///     The configuration version
    /// </summary>
    public int Version = ExpectedVersion;

    /// <summary>
    ///     A dictionary mapping SecretId to Secret
    /// </summary>
    public Dictionary<string, string> Secrets = [];
}