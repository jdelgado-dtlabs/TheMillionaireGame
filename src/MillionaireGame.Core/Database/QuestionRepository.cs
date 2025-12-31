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
    /// Gets a random question for the specified level
    /// </summary>
    public async Task<Question?> GetRandomQuestionAsync(int level, DifficultyType difficultyType = DifficultyType.Specific)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        string query;
        SqlCommand command;
        
        if (difficultyType == DifficultyType.Specific)
        {
            query = @"
                SELECT TOP 1 * FROM questions 
                WHERE Level = @Level 
                AND Difficulty_Type = 'Specific' 
                AND Used = 0 
                ORDER BY NEWID()";
            
            command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Level", level);
        }
        else
        {
            // Determine level range based on level number
            // Level 1-5: Easy, 6-10: Medium, 11-14: Hard, 15: Million
            int minLevel, maxLevel;
            if (level >= 1 && level <= 5)
            {
                minLevel = 1;
                maxLevel = 5;
            }
            else if (level >= 6 && level <= 10)
            {
                minLevel = 6;
                maxLevel = 10;
            }
            else if (level >= 11 && level <= 14)
            {
                minLevel = 11;
                maxLevel = 14;
            }
            else // level == 15
            {
                minLevel = 15;
                maxLevel = 15;
            }
            
            query = @"
                SELECT TOP 1 * FROM questions 
                WHERE Level BETWEEN @MinLevel AND @MaxLevel
                AND Difficulty_Type = 'Range' 
                AND Used = 0 
                ORDER BY NEWID()";
            
            command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@MinLevel", minLevel);
            command.Parameters.AddWithValue("@MaxLevel", maxLevel);
        }

        using (command)
        {
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapQuestion(reader);
            }
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
            (Question, A, B, C, D, CorrectAnswer, Difficulty_Type, Level, Note)
            VALUES 
            (@Question, @A, @B, @C, @D, @CorrectAnswer, @DifficultyType, @Level, @Note);
            SELECT CAST(SCOPE_IDENTITY() as int)";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Question", question.QuestionText);
        command.Parameters.AddWithValue("@A", question.AnswerA);
        command.Parameters.AddWithValue("@B", question.AnswerB);
        command.Parameters.AddWithValue("@C", question.AnswerC);
        command.Parameters.AddWithValue("@D", question.AnswerD);
        command.Parameters.AddWithValue("@CorrectAnswer", question.CorrectAnswer);
        command.Parameters.AddWithValue("@DifficultyType", question.DifficultyType.ToString());
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
                Difficulty_Type = @DifficultyType,
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
        command.Parameters.AddWithValue("@DifficultyType", question.DifficultyType.ToString());
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
    public async Task<(int total, int unused)> GetQuestionCountAsync(int level)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            SELECT 
                COUNT(*) as Total,
                SUM(CASE WHEN Used = 0 THEN 1 ELSE 0 END) as Unused
            FROM questions 
            WHERE Level = @Level";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Level", level);

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
        // Parse difficulty type safely
        var difficultyTypeStr = reader.GetString(reader.GetOrdinal("Difficulty_Type"));
        var difficultyType = difficultyTypeStr.Equals("Specific", StringComparison.OrdinalIgnoreCase) 
            ? DifficultyType.Specific 
            : DifficultyType.Range;

        return new Question
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            QuestionText = reader.GetString(reader.GetOrdinal("Question")),
            AnswerA = reader.GetString(reader.GetOrdinal("A")),
            AnswerB = reader.GetString(reader.GetOrdinal("B")),
            AnswerC = reader.GetString(reader.GetOrdinal("C")),
            AnswerD = reader.GetString(reader.GetOrdinal("D")),
            CorrectAnswer = reader.GetString(reader.GetOrdinal("CorrectAnswer")),
            DifficultyType = difficultyType,
            Level = reader.GetInt32(reader.GetOrdinal("Level")),
            Note = string.Empty,
            Used = reader.GetBoolean(reader.GetOrdinal("Used")),
            Explanation = reader.IsDBNull(reader.GetOrdinal("Note")) 
                ? string.Empty 
                : reader.GetString(reader.GetOrdinal("Note"))
        };
    }
}
