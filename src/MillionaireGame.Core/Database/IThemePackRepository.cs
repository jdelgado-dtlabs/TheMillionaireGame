using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Database;

public interface IThemePackRepository
{
    Task<List<ThemePack>> GetAllPacksAsync();
    Task<ThemePack?> GetPackByIdAsync(int packId);
    Task<ThemePack?> GetPackByNameAsync(string packName);
    Task<int> SavePackAsync(ThemePack pack);
    Task DeletePackAsync(int packId);
    Task<bool> PackExistsAsync(string packName);
}
