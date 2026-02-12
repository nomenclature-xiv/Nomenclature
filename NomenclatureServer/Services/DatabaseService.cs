using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NomenclatureServer.Domain;

// ReSharper disable RedundantBoolCompare

namespace NomenclatureServer.Services;

/// <summary>
///     Provides methods for interacting with the underlying Sqlite3 database
/// </summary>
public class DatabaseService
{
    // Const
    private const string AccountsTable = "Accounts";
    private const string PairsTable = "Pairs";
    private static readonly string DatabasePath = Path.Combine(Directory.GetCurrentDirectory(), "Database", "Nomenclature.db");

    // Injected
    private readonly ILogger<DatabaseService> _logger;

    // Instantiated
    private readonly SqliteConnection _database = new($"Data Source={DatabasePath}");

    /// <summary>
    ///     <inheritdoc cref="DatabaseService"/>
    /// </summary>
    public DatabaseService(ILogger<DatabaseService> logger)
    {
        // Inject
        _logger = logger;

        // Check if the database exists, otherwise create it
        if (File.Exists(DatabasePath) is false)
        {
            _logger.LogInformation("Database file not found at \"{Directory}\", creating...", DatabasePath);
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Database"));
            File.WriteAllBytes(DatabasePath, []);
        }

        // Open Database
        _database.Open();

        // Initialize
        InitializeDatabaseTables();
    }
    
    /// <summary>
    ///     Gets a user entry from the valid users table by secret 
    /// </summary>
    public async Task<Account?> GetAccountBySecret(string secret)
    {
        await using var command = _database.CreateCommand();
        command.CommandText = "SELECT SyncCode FROM Accounts WHERE Secret = @secret LIMIT 1";
        command.Parameters.AddWithValue("@secret", secret);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync()
                ? new Account(secret, reader.GetString(0))
                : null;
        }
        catch (Exception e)
        {
            _logger.LogWarning("Unable to get user with secret {Secret}, {Exception}", secret, e.Message);
            return null;
        }
    }
    
    /// <summary>
    ///     Creates a one-direction pair with target account id
    /// </summary>
    public async Task<DatabaseResultEc> CreatePair(string senderSyncCode, string targetSyncCode)
    {
        await using var transaction = (SqliteTransaction)await _database.BeginTransactionAsync();

        try
        {
            // Result object awaiting population
            DatabaseResultEc result;
            
            // Initial add command
            var command = _database.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = 
                 """
                    INSERT INTO Pairs (SyncCode, TargetSyncCode, Paused)
                    SELECT @syncCode, @targetSyncCode, @paused
                    WHERE EXISTS (
                        SELECT 1 FROM Accounts WHERE Id = @targetSyncCode
                    )
                 """;
            command.Parameters.AddWithValue("@syncCode", senderSyncCode);
            command.Parameters.AddWithValue("@targetSyncCode", targetSyncCode);
            command.Parameters.AddWithValue("@paused", 0);
            
            // If nothing was added, that means we're already paired or the target account doesn't exist
            if (await command.ExecuteNonQueryAsync() is 0)
            {
                // Check to see if the account id exists, SenderAccountId will always exist because it is a requirement to connect and use the plugin
                var failure = _database.CreateCommand();
                failure.Transaction = transaction;
                failure.CommandText = "SELECT 1 FROM Accounts WHERE SyncCode = @targetSyncCode LIMIT 1";
                failure.Parameters.AddWithValue("@targetSyncCode", targetSyncCode);
                result = await failure.ExecuteScalarAsync() is null ? DatabaseResultEc.NoSuchSyncCode : DatabaseResultEc.AlreadyPaired;
            }
            else
            {
                // Otherwise, check to see if they added us back
                var pair = _database.CreateCommand();
                pair.Transaction = transaction;
                pair.CommandText = "SELECT 1 FROM Pairs WHERE SyncCode = @targetSyncCode AND TargetSyncCode = @senderSyncCode LIMIT 1";
                pair.Parameters.AddWithValue("@senderSyncCode", senderSyncCode);
                pair.Parameters.AddWithValue("@targetSyncCode", targetSyncCode);
                result = await pair.ExecuteScalarAsync() is null ? DatabaseResultEc.Pending : DatabaseResultEc.Success;
            }
            
            // Commit changes and return what happened
            await transaction.CommitAsync();
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError("[CreatePair] {Error}", e);
            await transaction.RollbackAsync();
            return DatabaseResultEc.Unknown;
        }
    }

    /// <summary>
    ///     Pauses a one-sided pair
    /// </summary>
    public async Task<DatabaseResultEc> PausePair(string senderSyncCode, string targetSyncCode, bool paused)
    {
        await using var command = _database.CreateCommand();
        command.CommandText = "UPDATE Pairs SET Paused = @pause WHERE SyncCode = @senderSyncCode AND TargetSyncCode = @targetSyncCode";
        command.Parameters.AddWithValue("@senderSyncCode", senderSyncCode);
        command.Parameters.AddWithValue("@targetSyncCode", targetSyncCode);
        command.Parameters.AddWithValue("@pause", paused);
        
        try
        {
            return await command.ExecuteNonQueryAsync() is 0 ? DatabaseResultEc.NoOp : DatabaseResultEc.Success;
        }
        catch (Exception e)
        {
            _logger.LogError("[PausePair] {Error}", e);
            return DatabaseResultEc.Unknown;
        }
    }

    /// <summary>
    ///     Get all the pairs for a provided account id
    /// </summary>
    public async Task<List<Pair>> GetAllPairs(string syncCode)
    {
        await using var command = _database.CreateCommand();
        command.CommandText =
            """
                SELECT
                p.TargetSyncCode,
                p.Paused,
                r.Paused AS PausedByThem
                FROM Pairs p LEFT JOIN Pairs r ON r.SyncCode = p.TargetSyncCode AND r.TargetSyncCode = p.SyncCode
                WHERE p.SyncCode = @syncCode;
            """;
        command.Parameters.AddWithValue("@syncCode", syncCode);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            
            var results = new List<Pair>();
            while (reader.Read())
            {
                // Always get the target account id
                var targetSyncCode = reader.GetString(0);
                
                // Get our pair data for them
                var leftSidePaused = reader.GetBoolean(1);
                var rightSidePaused = reader.IsDBNull(2) ? null : (bool?)reader.GetBoolean(2);
                
                // Add pair
                results.Add(new Pair(targetSyncCode, leftSidePaused, rightSidePaused));
            }

            return results;
        }
        catch (Exception e)
        {
            _logger.LogError("[GetAllPairs] {Error}", e);
            return [];
        }
    }

    /// <summary>
    ///     Creates a new account in the database
    /// </summary>
    /// <param name="lodestone">The lodestone id of the character this account belongs to</param>
    /// <param name="code">The sync code of this account</param>
    /// <param name="secret">The secret of this account</param>
    public async Task<DatabaseResultEc> CreateAccount(string lodestone, string code, string secret)
    {
        await using var command = _database.CreateCommand();
        command.CommandText =
            """
                INSERT OR IGNORE INTO Accounts (LodestoneId, SyncCode, Secret) 
                VALUES (@lodestoneId, @syncCode, @secret)
                RETURNING 1;
            """;
        command.Parameters.AddWithValue("@lodestoneId", lodestone);
        command.Parameters.AddWithValue("@syncCode", code);
        command.Parameters.AddWithValue("@secret", secret);

        try
        {
            return await command.ExecuteScalarAsync() is null ? DatabaseResultEc.SyncCodeAlreadyExists : DatabaseResultEc.Success;
        }
        catch (Exception e)
        {
            _logger.LogError("[CreateAccount] Failed to register lodestone id {Lodestone}, {Error}", lodestone, e);
            return DatabaseResultEc.Unknown;
        }
    }
    
    // TODO: Get all accounts (By LodestoneId)
    
    // TODO: Update Account (Sync Code)
    
    // TODO: Delete Account
    
    /// <summary>
    ///     Creates database tables if they do not exist already
    /// </summary>
    private void InitializeDatabaseTables()
    {
        using var accounts = _database.CreateCommand();
        accounts.CommandText =
            $"""
                CREATE TABLE IF NOT EXISTS {AccountsTable} (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    LodestoneId INTEGER NOT NULL,
                    SyncCode TEXT NOT NULL UNIQUE,
                    Secret TEXT NOT NULL UNIQUE
                )
             """;
        accounts.ExecuteNonQuery();
        
        using var index = _database.CreateCommand();
        index.CommandText = $"CREATE INDEX IF NOT EXISTS idx_syncCode_lodestoneid ON {AccountsTable} (LodestoneId)";
        index.ExecuteNonQuery();
        
        using var pairs = _database.CreateCommand();
        pairs.CommandText =
            $"""
               CREATE TABLE IF NOT EXISTS {PairsTable} (
                   SyncCode TEXT NOT NULL,
                   TargetSyncCode TEXT NOT NULL,
                   Paused INTEGER NOT NULL DEFAULT 0,
                   PRIMARY KEY (SyncCode, TargetSyncCode),
                   FOREIGN KEY (SyncCode) REFERENCES {AccountsTable} (SyncCode) ON UPDATE CASCADE ON DELETE CASCADE,
                   FOREIGN KEY (TargetSyncCode) REFERENCES {AccountsTable} (SyncCode) ON UPDATE CASCADE ON DELETE CASCADE
               )
             """;
        pairs.ExecuteNonQuery();
    }
}