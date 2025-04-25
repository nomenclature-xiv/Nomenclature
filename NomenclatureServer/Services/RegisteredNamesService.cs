namespace NomenclatureServer.Services;

public class RegisteredNamesService
{
    /// <summary>
    ///     Dictionary mapping [CharacterName]@[HomeWorld] to [ModifiedCharacterName]
    /// </summary>
    public readonly Dictionary<string, string> ActiveNameChanges = new();
    
    /// <summary>
    ///     Dictionary containing any active registrations that have yet to be resolved
    /// </summary>
    public readonly Dictionary<string, string> PendingRegistrations = new();
}