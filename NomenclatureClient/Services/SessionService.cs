using NomenclatureClient.Types;
using NomenclatureCommon.Domain;

namespace NomenclatureClient.Services;

public class SessionService(Configuration configuration)
{
    public SessionInfo CurrentSession { get; set; } = new(new Character(), new CharacterConfiguration());
    
    /// <summary>
    ///     Save the configuration
    /// </summary>
    public void Save()
    {
        // So, any character configurations loaded into the current session will always be from 
        // the configuration file. If they are not, they will not be saved, but there should never be a
        // situation where this happens.
        configuration.Save();
    }
}