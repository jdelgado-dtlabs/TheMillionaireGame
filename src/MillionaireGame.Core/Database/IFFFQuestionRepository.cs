using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Database;

public interface IFFFQuestionRepository
{
    Task<FFFQuestion?> GetRandomQuestionAsync();
    Task<FFFQuestion?> GetQuestionByIdAsync(int questionId);
    Task MarkQuestionAsUsedAsync(int questionId);
    Task<List<FFFQuestion>> GetAllQuestionsAsync();
    Task<int> AddQuestionAsync(FFFQuestion question);
    Task UpdateQuestionAsync(FFFQuestion question);
    Task DeleteQuestionAsync(int questionId);
    Task ResetAllQuestionsAsync();
    Task<int> GetUnusedQuestionCountAsync();
}
