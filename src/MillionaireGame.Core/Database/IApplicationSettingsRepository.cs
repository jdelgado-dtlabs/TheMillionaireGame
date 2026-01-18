namespace MillionaireGame.Core.Database;

public interface IApplicationSettingsRepository
{
    Task<bool> SettingsTableExistsAsync();
    Task CreateSettingsTableAsync();
    Task<bool> SettingsDataExistsAsync();
    Task SaveSettingAsync(string key, string? value, string? category = null, string? description = null);
    Task<string?> GetSettingAsync(string key);
    Task<Dictionary<string, string>> GetAllSettingsAsync();
    Task<Dictionary<string, string>> GetSettingsByCategoryAsync(string category);
    Task DeleteAllSettingsAsync();
}
