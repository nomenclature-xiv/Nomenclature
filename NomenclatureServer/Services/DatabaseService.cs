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
    private const string PermissionsTable = "Permissions";
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
        command.CommandText = "SELECT Id, SyncCode FROM Accounts WHERE Secret = @secret LIMIT 1";
        command.Parameters.AddWithValue("@secret", secret);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync()
                ? new Account(reader.GetInt32(0), secret, reader.GetString(1))
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
    public async Task<DatabaseResultEc> CreatePair(int senderAccountId, int targetAccountId)
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
                    INSERT INTO Permissions (AccountId, TargetAccountId, Paused, Permissions)
                    SELECT @accountId, @targetAccountId, @paused, @permissions
                    WHERE EXISTS (
                        SELECT 1 FROM Accounts WHERE Id = @targetAccountId
                    )
                 """;
            command.Parameters.AddWithValue("@accountId", senderAccountId);
            command.Parameters.AddWithValue("@targetAccountId", targetAccountId);
            command.Parameters.AddWithValue("@paused", 0);
            command.Parameters.AddWithValue("@permissions", 0);
            
            // If nothing was added, that means we're already paired or the target account doesn't exist
            if (await command.ExecuteNonQueryAsync() is 0)
            {
                // Check to see if the account id exists, SenderAccountId will always exist because it is a requirement to connect and use the plugin
                var failure = _database.CreateCommand();
                failure.Transaction = transaction;
                failure.CommandText = "SELECT 1 FROM Accounts WHERE AccountId = @targetAccountId LIMIT 1";
                failure.Parameters.AddWithValue("@targetAccountId", targetAccountId);
                result = await failure.ExecuteScalarAsync() is null ? DatabaseResultEc.NoSuchSyncCode : DatabaseResultEc.AlreadyPaired;
            }
            else
            {
                // Otherwise, check to see if they added us back
                var pair = _database.CreateCommand();
                pair.Transaction = transaction;
                pair.CommandText = "SELECT 1 FROM Permissions WHERE AccountId = @targetAccountId AND TargetAccountId = @senderAccountId LIMIT 1";
                pair.Parameters.AddWithValue("@senderAccountId", senderAccountId);
                pair.Parameters.AddWithValue("@targetAccountId", targetAccountId);
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
    public async Task<DatabaseResultEc> PausePair(int senderAccountId, int targetAccountId, bool paused)
    {
        await using var command = _database.CreateCommand();
        command.CommandText = "UPDATE Permissions SET Paused = @pause WHERE AccountId = @senderAccountId AND TargetAccountId = @targetAccountId";
        command.Parameters.AddWithValue("@senderAccountId", senderAccountId);
        command.Parameters.AddWithValue("@targetAccountId", targetAccountId);
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
    public async Task<List<Pair>?> GetAllPairs(int accountId)
    {
        await using var command = _database.CreateCommand();
        command.CommandText =
            """
                SELECT
                p.TargetAccountId,
                p.Paused,
                p.Permissions,
                r.Paused AS PausedByThem,
                r.Permissions AS PermissionsByThem,
                FROM Permissions AS p LEFT JOIN Permissions AS r ON r.AccountId = p.TargetAccountId AND r.TargetAccountId = p.AccountId
                WHERE p.AccountId = @accountId;
            """;
        command.Parameters.AddWithValue("@accountId", accountId);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            
            var results = new List<Pair>();
            while (reader.Read())
            {
                // Always get the target account id
                var targetAccountId = reader.GetInt32(0);
                
                // Get our pair data for them
                var paused = reader.GetBoolean(1);
                var permissions = reader.GetInt32(2);

                // If 4 is null, 5 and 6 will also be null because that means they have not paired with us
                if (reader.IsDBNull(3))
                {
                    results.Add(new Pair(accountId, targetAccountId, paused, permissions, null, null));
                    continue;
                }
                
                // Get their pair data for us
                var paused2 = reader.GetBoolean(3);
                var permissions2 = reader.GetInt32(4);
                results.Add(new Pair(accountId, targetAccountId, paused, permissions, paused2, permissions2));
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
    
    // TODO: Move this to the shared package once structure is defined
    public record Pair(int AccountId, int TargetAccountId, bool Paused, int Permissions, bool? Paused2, int? Permissions2);

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
        index.CommandText = $"CREATE INDEX IF NOT EXISTS idx_accounts_lodestoneid ON {AccountsTable} (LodestoneId)";
        index.ExecuteNonQuery();
        
        using var permissions = _database.CreateCommand();
        permissions.CommandText =
            $"""
               CREATE TABLE IF NOT EXISTS {PermissionsTable} (
                   AccountId TEXT NOT NULL,
                   TargetAccountId TEXT NOT NULL,
                   Paused INTEGER NOT NULL DEFAULT 0,
                   Permissions INTEGER NOT NULL DEFAULT 0,
                   PRIMARY KEY (AccountId, TargetAccountId),
                   FOREIGN KEY (AccountId) REFERENCES {AccountsTable} (Id) ON UPDATE CASCADE ON DELETE CASCADE,
                   FOREIGN KEY (TargetAccountId) REFERENCES {AccountsTable} (Id) ON UPDATE CASCADE ON DELETE CASCADE
               )
             """;
        permissions.ExecuteNonQuery();
    }
}