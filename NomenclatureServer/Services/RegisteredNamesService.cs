namespace NomenclatureServer.Services;

public class RegisteredNamesService
{
    /// <summary>
    ///     Dictionary mapping [CharacterName]@[HomeWorld] to [ModifiedCharacterName]
    /// </summary>
    public readonly Dictionary<string, string> ActiveNameChanges = new();
}