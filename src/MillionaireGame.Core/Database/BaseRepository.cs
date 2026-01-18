using Microsoft.Data.SqlClient;

namespace MillionaireGame.Core.Database;

/// <summary>
/// Base class for all ADO.NET-based repositories
/// Provides common database connection and error handling functionality
/// </summary>
public abstract class BaseRepository
{
    protected readonly string ConnectionString;

    protected BaseRepository(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            
        ConnectionString = connectionString;
    }

    /// <summary>
    /// Opens and returns a new SQL connection
    /// Caller is responsible for disposal (use with 'using' statement)
    /// </summary>
    protected async Task<SqlConnection> OpenConnectionAsync()
    {
        var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }
    
    /// <summary>
    /// Executes a scalar query and returns the result
    /// </summary>
    protected async Task<T?> ExecuteScalarAsync<T>(string query, params SqlParameter[] parameters)
    {
        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        if (parameters.Length > 0)
            command.Parameters.AddRange(parameters);
        
        var result = await command.ExecuteScalarAsync();
        return result == null ? default : (T)result;
    }
    
    /// <summary>
    /// Executes a non-query command (INSERT, UPDATE, DELETE)
    /// Returns number of rows affected
    /// </summary>
    protected async Task<int> ExecuteNonQueryAsync(string query, params SqlParameter[] parameters)
    {
        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        if (parameters.Length > 0)
            command.Parameters.AddRange(parameters);
        
        return await command.ExecuteNonQueryAsync();
    }
}
