using System.IO.Compression;
using MillionaireGame.Core.Database;
using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Services;

/// <summary>
/// Handles theme pack import/export operations (ZIP file management)
/// </summary>
public class ThemePackHandler : IDisposable
{
    private readonly ThemeService _themeService;
    private readonly ThemePackParser _parser;
    private readonly string _connectionString;

    public ThemePackHandler(string connectionString)
    {
        _connectionString = connectionString;
        _themeService = new ThemeService(connectionString);
        _parser = new ThemePackParser();
    }

    /// <summary>
    /// Import a theme pack from a ZIP file
    /// </summary>
    /// <param name="zipPath">Path to the ZIP file</param>
    /// <param name="installPath">Directory to extract assets to</param>
    /// <returns>The imported theme pack metadata</returns>
    public async Task<ThemePack> ImportThemePackAsync(string zipPath, string installPath)
    {
        if (!File.Exists(zipPath))
            throw new FileNotFoundException($"Theme pack ZIP not found: {zipPath}");

        if (!Directory.Exists(installPath))
            Directory.CreateDirectory(installPath);

        // Create temporary extraction directory
        var tempDir = Path.Combine(Path.GetTempPath(), $"ThemePack_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Extract ZIP to temp directory
            ZipFile.ExtractToDirectory(zipPath, tempDir);

            // Find and parse theme pack XML
            var xmlPath = Path.Combine(tempDir, "theme_pack.xml");
            if (!File.Exists(xmlPath))
                throw new InvalidOperationException("Invalid theme pack: missing theme_pack.xml");

            var packData = _parser.ParsePackXml(xmlPath);

            // Create theme pack metadata
            var themePack = new ThemePack
            {
                PackName = packData.PackName,
                PackVersion = packData.PackVersion,
                Author = packData.Author,
                Description = packData.Description,
                InstallPath = installPath,
                ImportDate = DateTime.Now
            };

            // Save theme pack to database
            var packRepo = new ThemePackRepository(_connectionString);
            await packRepo.SavePackAsync(themePack);

            // Import each theme
            foreach (var themeData in packData.Themes)
            {
                await ImportThemeFromPackAsync(themeData, themePack.ThemePackId, tempDir, installPath);
            }

            return themePack;
        }
        finally
        {
            // Clean up temp directory
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    /// <summary>
    /// Export a theme or multiple themes to a ZIP file
    /// </summary>
    /// <param name="themeIds">Theme IDs to export</param>
    /// <param name="packName">Name for the theme pack</param>
    /// <param name="outputPath">Path where ZIP file will be created</param>
    /// <param name="author">Optional author name</param>
    /// <param name="description">Optional description</param>
    /// <returns>Path to created ZIP file</returns>
    public async Task<string> ExportThemePackAsync(
        List<int> themeIds,
        string packName,
        string outputPath,
        string? author = null,
        string? description = null)
    {
        if (themeIds.Count == 0)
            throw new ArgumentException("At least one theme ID is required", nameof(themeIds));

        // Create temporary directory for pack assembly
        var tempDir = Path.Combine(Path.GetTempPath(), $"ThemePackExport_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var packData = new ThemePackData
            {
                PackName = packName,
                PackVersion = "1.0.0",
                Author = author,
                Description = description
            };

            // Load each theme with all components
            foreach (var themeId in themeIds)
            {
                var theme = await _themeService.GetCompleteThemeAsync(themeId);
                if (theme != null)
                {
                    packData.Themes.Add(theme);

                    // Copy background images
                    await CopyThemeAssetsAsync(theme, tempDir);
                }
            }

            // Create theme pack XML
            var xml = _parser.CreatePackXml(packData);
            var xmlPath = Path.Combine(tempDir, "theme_pack.xml");
            xml.Save(xmlPath);

            // Create README
            var readmePath = Path.Combine(tempDir, "README.txt");
            await File.WriteAllTextAsync(readmePath, GeneratePackReadme(packData));

            // Create ZIP file
            var zipPath = Path.Combine(outputPath, $"{SanitizeFileName(packName)}.zip");
            if (File.Exists(zipPath))
                File.Delete(zipPath);

            ZipFile.CreateFromDirectory(tempDir, zipPath, CompressionLevel.Optimal, false);

            return zipPath;
        }
        finally
        {
            // Clean up temp directory
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    /// <summary>
    /// Uninstall a theme pack (removes all themes and assets)
    /// </summary>
    public async Task UninstallThemePackAsync(int packId)
    {
        var packRepo = new ThemePackRepository(_connectionString);
        var pack = await packRepo.GetPackByIdAsync(packId);
        if (pack == null)
            throw new InvalidOperationException($"Theme pack not found: {packId}");

        var themeRepo = new ThemeRepository(_connectionString);
        var themes = await themeRepo.GetAllThemesAsync();
        var packThemes = themes.Where(t => t.ThemePackId == packId).ToList();

        // Delete each theme (repositories handle cascade deletes)
        foreach (var theme in packThemes)
        {
            await _themeService.DeleteThemeAsync(theme.ThemeId);
        }

        // Delete theme pack metadata
        await packRepo.DeletePackAsync(packId);

        // Delete asset directory if it exists
        if (!string.IsNullOrEmpty(pack.InstallPath) && Directory.Exists(pack.InstallPath))
        {
            try
            {
                Directory.Delete(pack.InstallPath, true);
            }
            catch
            {
                // Ignore errors if directory can't be deleted
            }
        }
    }

    /// <summary>
    /// Validate a theme pack ZIP file without installing
    /// </summary>
    public async Task<ThemePackValidationResult> ValidateThemePackAsync(string zipPath)
    {
        var result = new ThemePackValidationResult { IsValid = true };

        if (!File.Exists(zipPath))
        {
            result.IsValid = false;
            result.Errors.Add($"File not found: {zipPath}");
            return result;
        }

        var tempDir = Path.Combine(Path.GetTempPath(), $"ThemePackValidation_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Extract ZIP
            ZipFile.ExtractToDirectory(zipPath, tempDir);

            // Check for theme_pack.xml
            var xmlPath = Path.Combine(tempDir, "theme_pack.xml");
            if (!File.Exists(xmlPath))
            {
                result.IsValid = false;
                result.Errors.Add("Missing theme_pack.xml file");
                return result;
            }

            // Parse XML
            var packData = _parser.ParsePackXml(xmlPath);
            result.PackName = packData.PackName;
            result.PackVersion = packData.PackVersion;
            result.ThemeCount = packData.Themes.Count;

            // Validate each theme
            foreach (var theme in packData.Themes)
            {
                ValidateThemeData(theme, tempDir, result);
            }
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Validation error: {ex.Message}");
        }
        finally
        {
            // Clean up temp directory
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }

        return result;
    }

    private async Task ImportThemeFromPackAsync(
        CompleteTheme themeData,
        int packId,
        string sourceDir,
        string installDir)
    {
        // Set pack ID
        themeData.Theme.ThemePackId = packId;
        themeData.Theme.ThemeType = "Custom"; // Imported themes are custom

        // Copy asset files from temp to install directory
        var assetsDir = Path.Combine(sourceDir, "assets");
        if (Directory.Exists(assetsDir))
        {
            var targetAssetsDir = Path.Combine(installDir, "assets");
            if (!Directory.Exists(targetAssetsDir))
                Directory.CreateDirectory(targetAssetsDir);

            // Copy all files
            foreach (var file in Directory.GetFiles(assetsDir, "*.*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(assetsDir, file);
                var targetPath = Path.Combine(targetAssetsDir, relativePath);
                var targetDir = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                File.Copy(file, targetPath, true);
            }

            // Update image paths to point to installed location
            UpdateAssetPaths(themeData, assetsDir, targetAssetsDir);
        }

        // Save theme to database
        await _themeService.SaveCompleteThemeAsync(themeData);
    }

    private void UpdateAssetPaths(CompleteTheme theme, string oldPath, string newPath)
    {
        // Update background image paths
        foreach (var bg in theme.Backgrounds)
        {
            if (!string.IsNullOrEmpty(bg.ImagePath))
            {
                bg.ImagePath = bg.ImagePath.Replace(oldPath, newPath);
            }
        }

        // Update money tree background path
        if (theme.MoneyTree != null && !string.IsNullOrEmpty(theme.MoneyTree.BackgroundImagePath))
        {
            theme.MoneyTree.BackgroundImagePath = theme.MoneyTree.BackgroundImagePath.Replace(oldPath, newPath);
        }
    }

    private async Task CopyThemeAssetsAsync(CompleteTheme theme, string targetDir)
    {
        var assetsDir = Path.Combine(targetDir, "assets");
        Directory.CreateDirectory(assetsDir);

        // Copy background images
        foreach (var bg in theme.Backgrounds)
        {
            if (!string.IsNullOrEmpty(bg.ImagePath) && File.Exists(bg.ImagePath))
            {
                var fileName = Path.GetFileName(bg.ImagePath);
                var targetPath = Path.Combine(assetsDir, fileName);
                File.Copy(bg.ImagePath, targetPath, true);

                // Update path in theme data to relative path
                bg.ImagePath = Path.Combine("assets", fileName);
            }
        }

        // Copy money tree background
        if (theme.MoneyTree != null && !string.IsNullOrEmpty(theme.MoneyTree.BackgroundImagePath))
        {
            if (File.Exists(theme.MoneyTree.BackgroundImagePath))
            {
                var fileName = Path.GetFileName(theme.MoneyTree.BackgroundImagePath);
                var targetPath = Path.Combine(assetsDir, fileName);
                File.Copy(theme.MoneyTree.BackgroundImagePath, targetPath, true);

                // Update path in theme data to relative path
                theme.MoneyTree.BackgroundImagePath = Path.Combine("assets", fileName);
            }
        }
    }

    private void ValidateThemeData(CompleteTheme theme, string packDir, ThemePackValidationResult result)
    {
        // Check theme name
        if (string.IsNullOrEmpty(theme.Theme.ThemeName))
        {
            result.Warnings.Add($"Theme has no name");
        }

        // Check for missing asset files
        foreach (var bg in theme.Backgrounds)
        {
            if (!string.IsNullOrEmpty(bg.ImagePath))
            {
                var fullPath = Path.Combine(packDir, bg.ImagePath);
                if (!File.Exists(fullPath))
                {
                    result.Warnings.Add($"Missing background image: {bg.ImagePath}");
                }
            }
        }

        if (theme.MoneyTree != null && !string.IsNullOrEmpty(theme.MoneyTree.BackgroundImagePath))
        {
            var fullPath = Path.Combine(packDir, theme.MoneyTree.BackgroundImagePath);
            if (!File.Exists(fullPath))
            {
                result.Warnings.Add($"Missing money tree background: {theme.MoneyTree.BackgroundImagePath}");
            }
        }
    }

    private string GeneratePackReadme(ThemePackData packData)
    {
        return $@"Theme Pack: {packData.PackName}
Version: {packData.PackVersion}
Author: {packData.Author ?? "Unknown"}

{packData.Description ?? "No description available."}

Included Themes:
{string.Join("\n", packData.Themes.Select(t => $"- {t.Theme.ThemeName}"))}

Installation:
This theme pack can be imported through the Millionaire Game Options > Themes panel.
";
    }

    private string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    }

    public void Dispose()
    {
        _themeService?.Dispose();
    }
}

/// <summary>
/// Theme pack validation result
/// </summary>
public class ThemePackValidationResult
{
    public bool IsValid { get; set; }
    public string? PackName { get; set; }
    public string? PackVersion { get; set; }
    public int ThemeCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
