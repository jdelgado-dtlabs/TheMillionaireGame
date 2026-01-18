using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Database;

public interface IThemeBackgroundRepository
{
    Task<List<ThemeBackground>> GetBackgroundsByThemeIdAsync(int themeId);
    Task<ThemeBackground?> GetBackgroundByComponentAsync(int themeId, string componentType);
    Task<int> SaveBackgroundAsync(ThemeBackground background);
    Task DeleteBackgroundAsync(int backgroundId);
    Task DeleteBackgroundsByThemeIdAsync(int themeId);
}
