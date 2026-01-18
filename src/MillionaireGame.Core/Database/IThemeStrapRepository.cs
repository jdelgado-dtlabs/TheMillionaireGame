using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Database;

public interface IThemeStrapRepository
{
    Task<List<ThemeStrap>> GetStrapsByThemeIdAsync(int themeId);
    Task<ThemeStrap?> GetStrapByTypeAsync(int themeId, string strapType);
    Task<int> SaveStrapAsync(ThemeStrap strap);
    Task DeleteStrapAsync(int strapId);
    Task DeleteStrapsByThemeIdAsync(int themeId);
}
