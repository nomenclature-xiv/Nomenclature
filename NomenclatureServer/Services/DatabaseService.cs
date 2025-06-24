using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace NomenclatureServer.Services;

/// <summary>
///     Provides methods for interacting with the underlying Sqlite3 database
/// </summary>
public class DatabaseService
{
    // Const
    private const string RegisteredCharactersTable = "RegisteredCharacters";
    private const string SecretParam = "@Secret";
    private const string CharacterIdentifierParam = "@CharacterIdentifier";

    // Injected
    private readonly ILogger<DatabaseService> _logger;

    // Instantiated
    private readonly SqliteConnection _db;

    /// <summary>
    ///     <inheritdoc cref="DatabaseService"/>
    /// </summary>
    public DatabaseService(ILogger<DatabaseService> logger)
    {
        // Inject
        _logger = logger;

        // Check if the database exists, otherwise create it
        var path = Path.Combine(Directory.GetCurrentDirectory(), "database", "nomenclature.db");
        if (File.Exists(path) is false)
        {
            _logger.LogInformation("Database file not found at \"{Directory}\", creating...", path);
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "database"));
            File.WriteAllBytes(path, []);
        }

        // Open Database
        _db = new SqliteConnection($"Data Source={path}");
        _db.Open();

        // Initialize
        InitializeDatabaseTables();
    }

    /// <summary>
    ///     Registers a character in the database by secret
    /// </summary>
    public async Task<bool> RegisterCharacter(string secret, string name, string world)
    {
        var identifier = string.Concat(name, "@", world);
        await using var command = _db.CreateCommand();
        command.CommandText = $"""
                                    INSERT INTO {RegisteredCharactersTable} (Secret, CharacterIdentifier)
                                    VALUES ({SecretParam}, {CharacterIdentifierParam})
                                    ON CONFLICT(CharacterIdentifier) DO UPDATE SET Secret=excluded.Secret
                              """;
        command.Parameters.AddWithValue(SecretParam, secret);
        command.Parameters.AddWithValue(CharacterIdentifierParam, identifier);
        
        try
        {
            var result = await command.ExecuteNonQueryAsync() is 1;
            _logger.LogInformation("{Result}", result);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to add character {CharacterIdentifier} for secret {Secret}, {Exception}", identifier, secret,  e);
            return false;
        }
    }

    /// <summary>
    ///     Retrieves a registered character from the database by secret
    /// </summary>
    /// <returns>A character identifier of the format [CharacterName]@[World] or null</returns>
    public async Task<string?> GetRegisteredCharacter(string secret)
    {
        await using var command = _db.CreateCommand();
        command.CommandText = $"SELECT * FROM {RegisteredCharactersTable} WHERE Secret = {SecretParam}";
        command.Parameters.AddWithValue(SecretParam, secret);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? reader.GetString(1) : null;
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to get registered character for secret {Secret}, {Exception}", secret, e);
            return null;
        }
    }

    /// <summary>
    ///     Creates database tables if they do not exist already
    /// </summary>
    private void InitializeDatabaseTables()
    {
        using var initializerValidUsersTable = _db.CreateCommand();
        initializerValidUsersTable.CommandText =
            $"""
                 CREATE TABLE IF NOT EXISTS {RegisteredCharactersTable} (
                     Secret TEXT PRIMARY KEY,
                     CharacterIdentifier TEXT NOT NULL UNIQUE
                 )
             """;

        initializerValidUsersTable.ExecuteNonQuery();
    }
}