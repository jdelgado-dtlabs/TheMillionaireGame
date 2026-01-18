using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Database;

public interface IQuestionRepository
{
    Task<Question?> GetRandomQuestionAsync(int questionNumber);
    Task MarkQuestionAsUsedAsync(int questionId);
    Task<List<Question>> GetAllQuestionsAsync();
    Task<int> AddQuestionAsync(Question question);
    Task UpdateQuestionAsync(Question question);
    Task DeleteQuestionAsync(int questionId);
    Task ResetAllQuestionsAsync();
    Task<(int total, int unused)> GetQuestionCountAsync(int questionNumber);
}
