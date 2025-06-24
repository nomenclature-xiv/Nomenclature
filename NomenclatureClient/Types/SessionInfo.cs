using NomenclatureCommon.Domain;

namespace NomenclatureClient.Types;

/// <summary>
///     Represents information about the currently logged in character session
/// </summary>
public record SessionInfo(Character Character, CharacterConfiguration CharacterConfiguration);