using System.Data;
using Microsoft.Data.SqlClient;
using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Database;

/// <summary>
/// Repository for managing questions in the database
/// </summary>
public class QuestionRepository
{
    private readonly string _connectionString;

    public QuestionRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Gets a random question for the specified game question number (1-15)
    /// Maps to database levels: 1-5 -> Level 1, 6-10 -> Level 2, 11-14 -> Level 3, 15 -> Level 4
    /// </summary>
    public async Task<Question?> GetRandomQuestionAsync(int questionNumber)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // Map game question number (1-15) to database level (1-4)
        int dbLevel = questionNumber switch
        {
            >= 1 and <= 5 => 1,
            >= 6 and <= 10 => 2,
            >= 11 and <= 14 => 3,
            15 => 4,
            _ => 1
        };

        var query = @"
            SELECT TOP 1 * FROM questions 
            WHERE Level = @Level
            AND Used = 0 
            ORDER BY NEWID()";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Level", dbLevel);

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

        var query = "UPDATE questions SET Used = 1 WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", questionId);

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Gets all questions
    /// </summary>
    public async Task<List<Question>> GetAllQuestionsAsync()
    {
        var questions = new List<Question>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT * FROM questions ORDER BY Level, Id";
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            questions.Add(MapQuestion(reader));
        }

        return questions;
    }

    /// <summary>
    /// Adds a new question to the database
    /// </summary>
    public async Task<int> AddQuestionAsync(Question question)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            INSERT INTO questions 
            (Question, A, B, C, D, CorrectAnswer, Level, Note)
            VALUES 
            (@Question, @A, @B, @C, @D, @CorrectAnswer, @Level, @Note);
            SELECT CAST(SCOPE_IDENTITY() as int)";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Question", question.QuestionText);
        command.Parameters.AddWithValue("@A", question.AnswerA);
        command.Parameters.AddWithValue("@B", question.AnswerB);
        command.Parameters.AddWithValue("@C", question.AnswerC);
        command.Parameters.AddWithValue("@D", question.AnswerD);
        command.Parameters.AddWithValue("@CorrectAnswer", question.CorrectAnswer);
        command.Parameters.AddWithValue("@Level", question.Level);
        command.Parameters.AddWithValue("@Note", question.Explanation ?? (object)DBNull.Value);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Updates an existing question
    /// </summary>
    public async Task UpdateQuestionAsync(Question question)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            UPDATE questions SET
                Question = @Question,
                A = @A,
                B = @B,
                C = @C,
                D = @D,
                CorrectAnswer = @CorrectAnswer,
                Level = @Level,
                Note = @Note
            WHERE Id = @Id";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", question.Id);
        command.Parameters.AddWithValue("@Question", question.QuestionText);
        command.Parameters.AddWithValue("@A", question.AnswerA);
        command.Parameters.AddWithValue("@B", question.AnswerB);
        command.Parameters.AddWithValue("@C", question.AnswerC);
        command.Parameters.AddWithValue("@D", question.AnswerD);
        command.Parameters.AddWithValue("@CorrectAnswer", question.CorrectAnswer);
        command.Parameters.AddWithValue("@Level", question.Level);
        command.Parameters.AddWithValue("@Note", question.Explanation ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Deletes a question
    /// </summary>
    public async Task DeleteQuestionAsync(int questionId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "DELETE FROM questions WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", questionId);

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Resets all questions to unused
    /// </summary>
    public async Task ResetAllQuestionsAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "UPDATE questions SET Used = 0";
        using var command = new SqlCommand(query, connection);

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Gets count of questions by level and used status for diagnostics
    /// </summary>
    public async Task<(int total, int unused)> GetQuestionCountAsync(int questionNumber)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // Map game question number (1-15) to database level (1-4)
        int dbLevel = questionNumber switch
        {
            >= 1 and <= 5 => 1,
            >= 6 and <= 10 => 2,
            >= 11 and <= 14 => 3,
            15 => 4,
            _ => 1
        };

        var query = @"
            SELECT 
                COUNT(*) as Total,
                SUM(CASE WHEN Used = 0 THEN 1 ELSE 0 END) as Unused
            FROM questions 
            WHERE Level = @Level";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Level", dbLevel);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var total = reader.GetInt32(0);
            var unused = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
            return (total, unused);
        }

        return (0, 0);
    }

    private Question MapQuestion(SqlDataReader reader)
    {
        return new Question
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            QuestionText = reader.GetString(reader.GetOrdinal("Question")),
            AnswerA = reader.GetString(reader.GetOrdinal("A")),
            AnswerB = reader.GetString(reader.GetOrdinal("B")),
            AnswerC = reader.GetString(reader.GetOrdinal("C")),
            AnswerD = reader.GetString(reader.GetOrdinal("D")),
            CorrectAnswer = reader.GetString(reader.GetOrdinal("CorrectAnswer")),
            Level = reader.GetInt32(reader.GetOrdinal("Level")),
            Note = string.Empty,
            Used = reader.GetBoolean(reader.GetOrdinal("Used")),
            Explanation = reader.IsDBNull(reader.GetOrdinal("Note")) 
                ? string.Empty 
                : reader.GetString(reader.GetOrdinal("Note"))
        };
    }
}
