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
    private const string RegisteredCharacterParam = "@RegisteredCharacter";

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

    public async Task<bool> AddRegisteredCharacter(string characterName, string secret)
    {
        await using var command = _db.CreateCommand();
        command.CommandText = $"INSERT INTO {RegisteredCharactersTable} (RegisteredCharacter, Secret) VALUES ({RegisteredCharacterParam}, {SecretParam})";
        command.Parameters.AddWithValue(RegisteredCharacterParam, characterName);
        command.Parameters.AddWithValue(SecretParam, secret);

        try
        {
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to add registered character for secret {Secret}, character {RegisteredCharacter}: {Exception}", secret, characterName, e);
            return false;
        }
    }

    /// <summary>
    ///     Retrieves a registered character from the database by secret
    /// </summary>
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
                     RegisteredCharacter TEXT NOT NULL UNIQUE
                 )
             """;

        initializerValidUsersTable.ExecuteNonQuery();
    }
}