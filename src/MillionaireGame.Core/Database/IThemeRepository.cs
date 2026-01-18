using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Database;

public interface IThemeRepository
{
    Task<Theme?> GetActiveThemeAsync();
    Task<Theme?> GetThemeByIdAsync(int themeId);
    Task<List<Theme>> GetAllThemesAsync();
    Task<List<Theme>> GetThemesByTypeAsync(string themeType);
    Task<int> SaveThemeAsync(Theme theme);
    Task SetActiveThemeAsync(int themeId);
    Task DeleteThemeAsync(int themeId);
    Task<bool> ThemeExistsAsync(string themeName);
}
