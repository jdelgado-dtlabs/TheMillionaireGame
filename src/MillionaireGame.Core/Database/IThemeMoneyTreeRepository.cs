using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Database;

public interface IThemeMoneyTreeRepository
{
    Task<ThemeMoneyTree?> GetMoneyTreeByThemeIdAsync(int themeId);
    Task<int> SaveMoneyTreeAsync(ThemeMoneyTree moneyTree);
    Task DeleteMoneyTreeAsync(int moneyTreeId);
    Task DeleteMoneyTreeByThemeIdAsync(int themeId);
}
