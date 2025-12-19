using System.Data;
using System.Data.SqlClient;
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
        if (difficultyType == DifficultyType.Specific)
        {
            query = @"
                SELECT TOP 1 * FROM questions 
                WHERE Level = @Level 
                AND Difficulty_Type = 'Specific' 
                AND Used = 0 
                ORDER BY NEWID()";
        }
        else
        {
            var levelRange = GetLevelRangeForLevel(level);
            // Convert LevelRange enum to database format (Lvl1, Lvl2, etc.)
            var levelRangeStr = levelRange switch
            {
                LevelRange.Level1 => "Lvl1",
                LevelRange.Level2 => "Lvl2",
                LevelRange.Level3 => "Lvl3",
                LevelRange.Level4 => "Lvl4",
                _ => "Lvl1"
            };
            
            query = @"
                SELECT TOP 1 * FROM questions 
                WHERE LevelRange = @LevelRange 
                AND Difficulty_Type = 'Range' 
                AND Used = 0 
                ORDER BY NEWID()";
        }

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Level", level);
        command.Parameters.AddWithValue("@LevelRange", difficultyType == DifficultyType.Range ? 
            GetLevelRangeString(level) : (object)DBNull.Value);

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
            (Question, A, B, C, D, CorrectAnswer, Difficulty_Type, Level, LevelRange, Note, ATA_A, ATA_B, ATA_C, ATA_D)
            VALUES 
            (@Question, @A, @B, @C, @D, @CorrectAnswer, @DifficultyType, @Level, @LevelRange, @Note, @ATA_A, @ATA_B, @ATA_C, @ATA_D);
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
        command.Parameters.AddWithValue("@LevelRange", question.LevelRange?.ToString() ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Note", question.Explanation ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ATA_A", question.ATAPercentageA ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ATA_B", question.ATAPercentageB ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ATA_C", question.ATAPercentageC ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ATA_D", question.ATAPercentageD ?? (object)DBNull.Value);

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
                LevelRange = @LevelRange,
                Note = @Note,
                ATA_A = @ATA_A,
                ATA_B = @ATA_B,
                ATA_C = @ATA_C,
                ATA_D = @ATA_D
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
        command.Parameters.AddWithValue("@LevelRange", question.LevelRange?.ToString() ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Note", question.Explanation ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ATA_A", question.ATAPercentageA ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ATA_B", question.ATAPercentageB ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ATA_C", question.ATAPercentageC ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ATA_D", question.ATAPercentageD ?? (object)DBNull.Value);

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

        // Parse level range safely (handle both "Lvl1" and "Level1" formats)
        LevelRange? levelRange = null;
        if (!reader.IsDBNull(reader.GetOrdinal("LevelRange")))
        {
            var levelRangeStr = reader.GetString(reader.GetOrdinal("LevelRange"));
            levelRange = levelRangeStr.ToLower() switch
            {
                "lvl1" or "level1" => LevelRange.Level1,
                "lvl2" or "level2" => LevelRange.Level2,
                "lvl3" or "level3" => LevelRange.Level3,
                "lvl4" or "level4" => LevelRange.Level4,
                _ => null
            };
        }

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
            LevelRange = levelRange,
            Note = string.Empty,
            Used = reader.GetBoolean(reader.GetOrdinal("Used")),
            Explanation = reader.IsDBNull(reader.GetOrdinal("Note")) 
                ? string.Empty 
                : reader.GetString(reader.GetOrdinal("Note")),
            ATAPercentageA = reader.IsDBNull(reader.GetOrdinal("ATA_A")) 
                ? null 
                : reader.GetInt32(reader.GetOrdinal("ATA_A")),
            ATAPercentageB = reader.IsDBNull(reader.GetOrdinal("ATA_B")) 
                ? null 
                : reader.GetInt32(reader.GetOrdinal("ATA_B")),
            ATAPercentageC = reader.IsDBNull(reader.GetOrdinal("ATA_C")) 
                ? null 
                : reader.GetInt32(reader.GetOrdinal("ATA_C")),
            ATAPercentageD = reader.IsDBNull(reader.GetOrdinal("ATA_D")) 
                ? null 
                : reader.GetInt32(reader.GetOrdinal("ATA_D"))
        };
    }

    private LevelRange GetLevelRangeForLevel(int level)
    {
        return level switch
        {
            >= 1 and <= 5 => LevelRange.Level1,
            >= 6 and <= 10 => LevelRange.Level2,
            >= 11 and <= 14 => LevelRange.Level3,
            15 => LevelRange.Level4,
            _ => LevelRange.Level1
        };
    }

    private string GetLevelRangeString(int level)
    {
        return level switch
        {
            >= 1 and <= 5 => "Lvl1",
            >= 6 and <= 10 => "Lvl2",
            >= 11 and <= 14 => "Lvl3",
            15 => "Lvl4",
            _ => "Lvl1"
        };
    }
}
