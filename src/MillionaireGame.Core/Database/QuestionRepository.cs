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
            query = @"
                SELECT TOP 1 * FROM questions 
                WHERE LevelRange = @LevelRange 
                AND Difficulty_Type = 'Range' 
                AND Used = 0 
                ORDER BY NEWID()";
        }

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Level", level);
        command.Parameters.AddWithValue("@LevelRange", GetLevelRangeForLevel(level).ToString());

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
            (Question, A, B, C, D, CorrectAnswer, Difficulty_Type, Level, LevelRange, Note, Explanation, ATA_A, ATA_B, ATA_C, ATA_D)
            VALUES 
            (@Question, @A, @B, @C, @D, @CorrectAnswer, @DifficultyType, @Level, @LevelRange, @Note, @Explanation, @ATA_A, @ATA_B, @ATA_C, @ATA_D);
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
        command.Parameters.AddWithValue("@Note", question.Note ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Explanation", question.Explanation ?? (object)DBNull.Value);
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
                Explanation = @Explanation,
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
        command.Parameters.AddWithValue("@Note", question.Note ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Explanation", question.Explanation ?? (object)DBNull.Value);
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
            DifficultyType = Enum.Parse<DifficultyType>(reader.GetString(reader.GetOrdinal("Difficulty_Type"))),
            Level = reader.GetInt32(reader.GetOrdinal("Level")),
            LevelRange = reader.IsDBNull(reader.GetOrdinal("LevelRange")) 
                ? null 
                : Enum.Parse<LevelRange>(reader.GetString(reader.GetOrdinal("LevelRange"))),
            Note = reader.IsDBNull(reader.GetOrdinal("Note")) 
                ? string.Empty 
                : reader.GetString(reader.GetOrdinal("Note")),
            Used = reader.GetBoolean(reader.GetOrdinal("Used")),
            Explanation = reader.IsDBNull(reader.GetOrdinal("Explanation")) 
                ? string.Empty 
                : reader.GetString(reader.GetOrdinal("Explanation")),
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
}
