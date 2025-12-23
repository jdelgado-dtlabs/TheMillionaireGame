using System.Data.SqlClient;
using MillionaireGame.Web.Models;

namespace MillionaireGame.Web.Database;

/// <summary>
/// Repository for managing Fastest Finger First questions in the SQL Server database
/// </summary>
public class FFFQuestionRepository
{
    private readonly string _connectionString;

    public FFFQuestionRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Gets a random unused FFF question
    /// </summary>
    public async Task<FFFQuestion?> GetRandomQuestionAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            SELECT TOP 1 * FROM fff_questions 
            WHERE Used = 0 
            ORDER BY NEWID()";

        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapQuestion(reader);
        }

        return null;
    }

    /// <summary>
    /// Get specific FFF question by ID
    /// </summary>
    public async Task<FFFQuestion?> GetQuestionByIdAsync(int questionId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT * FROM fff_questions WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", questionId);
        
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapQuestion(reader);
        }

        return null;
    }

    /// <summary>
    /// Marks a question as used
    /// </summary>
    public async Task MarkQuestionAsUsedAsync(int questionId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "UPDATE fff_questions SET Used = 1 WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", questionId);

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Gets all FFF questions
    /// </summary>
    public async Task<List<FFFQuestion>> GetAllQuestionsAsync()
    {
        var questions = new List<FFFQuestion>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT * FROM fff_questions ORDER BY Id";
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            questions.Add(MapQuestion(reader));
        }

        return questions;
    }

    /// <summary>
    /// Gets the count of unused questions
    /// </summary>
    public async Task<int> GetUnusedQuestionCountAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT COUNT(*) FROM fff_questions WHERE Used = 0";
        using var command = new SqlCommand(query, connection);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    private FFFQuestion MapQuestion(SqlDataReader reader)
    {
        return new FFFQuestion
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            QuestionText = reader.GetString(reader.GetOrdinal("Question")),
            AnswerA = reader.GetString(reader.GetOrdinal("A")),
            AnswerB = reader.GetString(reader.GetOrdinal("B")),
            AnswerC = reader.GetString(reader.GetOrdinal("C")),
            AnswerD = reader.GetString(reader.GetOrdinal("D")),
            CorrectOrder = reader.GetString(reader.GetOrdinal("CorrectAnswer")),
            Used = reader.GetBoolean(reader.GetOrdinal("Used"))
        };
    }
}
