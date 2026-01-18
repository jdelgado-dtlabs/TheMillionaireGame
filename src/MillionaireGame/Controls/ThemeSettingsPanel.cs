using MillionaireGame.Core.Models;
using MillionaireGame.Core.Services;
using MillionaireGame.Core.Graphics;
using MillionaireGame.Utilities;

namespace MillionaireGame.Controls;

/// <summary>
/// Theme settings panel for OptionsDialog - manages theme selection, preview, and pack operations
/// </summary>
public partial class ThemeSettingsPanel : UserControl
{
    private readonly ThemeService _themeService;
    private readonly SvgStrapRenderer _renderer;
    private List<Theme> _allThemes = new();
    private Theme? _selectedTheme;
    

    public event EventHandler? ThemeChanged;
    public event EventHandler? SettingsChanged;

    public ThemeSettingsPanel(string connectionString)
    {
        _themeService = new ThemeService(connectionString);
        _renderer = new SvgStrapRenderer();
        
        InitializeComponent();
        InitializeControls();
    }

    private void InitializeControls()
    {
        // Initialize theme list columns
        lstThemes.View = View.Details;
        lstThemes.FullRowSelect = true;
        lstThemes.MultiSelect = false;
        lstThemes.Columns.Add("Theme Name", 250);
        lstThemes.Columns.Add("Type", 100);
        lstThemes.Columns.Add("Author", 150);

        // Event handlers
        lstThemes.SelectedIndexChanged += LstThemes_SelectedIndexChanged;
        btnApplyTheme.Click += BtnApplyTheme_Click;
        btnDuplicateTheme.Click += BtnDuplicateTheme_Click;
        btnDeleteTheme.Click += BtnDeleteTheme_Click;
        btnImportPack.Click += BtnImportPack_Click;
        btnExportTheme.Click += BtnExportTheme_Click;
        btnExportExample.Click += BtnExportExample_Click;
        btnRefresh.Click += async (s, e) => await LoadThemesAsync();
        
        // Add Create Classic Black button (programmatic — avoids designer changes)
        var btnCreateClassicBlack = new Button
        {
            Text = "Create Classic Black",
            AutoSize = true,
            Location = new Point(10, lstThemes.Bottom + 8)
        };
        btnCreateClassicBlack.Click += BtnCreateClassicBlack_Click;
        Controls.Add(btnCreateClassicBlack);
    }

    /// <summary>
    /// Load settings into the panel
    /// </summary>
    public async Task LoadSettingsAsync()
    {
        try
        {
            await _themeService.LoadActiveThemeAsync();
            await LoadThemesAsync();
        }
        finally
        {
        }
    }

    /// <summary>
    /// Load all themes from database
    /// </summary>
    private async Task LoadThemesAsync()
    {
        try
        {
            _allThemes = await _themeService.GetAllThemesAsync();
            
            lstThemes.Items.Clear();
            
            foreach (var theme in _allThemes.OrderBy(t => t.ThemeType).ThenBy(t => t.ThemeName))
            {
                var item = new ListViewItem(theme.ThemeName ?? "Unknown");
                item.SubItems.Add(theme.ThemeType ?? "Unknown");
                item.SubItems.Add(theme.Author ?? "Unknown");
                item.Tag = theme;
                
                // Mark active theme
                if (theme.IsActive)
                {
                    item.Font = new Font(item.Font, FontStyle.Bold);
                    item.BackColor = Color.LightGreen;
                }
                
                lstThemes.Items.Add(item);
            }

            // Select active theme
            var activeTheme = _allThemes.FirstOrDefault(t => t.IsActive);
            if (activeTheme != null)
            {
                    var activeItem = lstThemes.Items.Cast<ListViewItem>()
                        .FirstOrDefault(i => (i.Tag as Theme)?.ThemeId == activeTheme.ThemeId);
                if (activeItem != null)
                {
                    activeItem.Selected = true;
                }
            }
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[ThemeSettingsPanel] Failed to load themes: {ex.Message}");
            MessageBox.Show($"Failed to load themes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Handle theme selection changed
    /// </summary>
    private async void LstThemes_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (lstThemes.SelectedItems.Count == 0)
        {
            _selectedTheme = null;
            UpdatePreview(null);
            UpdateButtons();
            return;
        }
        _selectedTheme = lstThemes.SelectedItems[0].Tag as Theme;
        if (_selectedTheme != null)
        {
            await LoadThemePreviewAsync(_selectedTheme.ThemeId);
        }
        UpdateButtons();
    }

    /// <summary>
    /// Load and display theme preview
    /// </summary>
    private async Task LoadThemePreviewAsync(int themeId)
    {
        try
        {
            var completeTheme = await _themeService.GetCompleteThemeAsync(themeId);
            if (completeTheme == null)
                return;

            UpdatePreview(completeTheme);
            DisplayThemeDetails(completeTheme);
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[ThemeSettingsPanel] Failed to load theme preview: {ex.Message}");
        }
    }

    /// <summary>
    /// Update preview image
    /// </summary>
    private void UpdatePreview(CompleteTheme? theme)
    {
        if (picPreview.Image != null)
        {
            picPreview.Image.Dispose();
            picPreview.Image = null;
        }

        if (theme == null)
            return;

        try
        {
            // Get question and answer straps
            var questionStrap = theme.Straps.FirstOrDefault(s => s.StrapType == "Question");
            var answerStrap = theme.Straps.FirstOrDefault(s => s.StrapType == "Answer");

            if (questionStrap != null && answerStrap != null)
            {
                picPreview.Image = _renderer.RenderStrapPreview(
                    questionStrap,
                    answerStrap,
                    picPreview.Width,
                    picPreview.Height);
            }
        }
        catch (Exception ex)
        {
            GameConsole.Warn($"[ThemeSettingsPanel] Failed to render preview: {ex.Message}");
        }
    }

    /// <summary>
    /// Display theme details in text box
    /// </summary>
    private void DisplayThemeDetails(CompleteTheme theme)
    {
        var details = new System.Text.StringBuilder();
        details.AppendLine($"Theme: {theme.Theme.ThemeName ?? "Unknown"}");
        details.AppendLine($"Type: {theme.Theme.ThemeType ?? "Unknown"}");
        details.AppendLine($"Author: {theme.Theme.Author ?? "Unknown"}");
        details.AppendLine($"Version: {theme.Theme.Version ?? "1.0.0"}");
        details.AppendLine($"Description: {theme.Theme.Description ?? "No description"}");
        details.AppendLine();
        details.AppendLine($"Backgrounds: {theme.Backgrounds.Count}");
        details.AppendLine($"Straps: {theme.Straps.Count}");
        details.AppendLine($"Money Tree: {(theme.MoneyTree != null ? "Configured" : "Not configured")}");
        
        txtThemeDetails.Text = details.ToString();
    }

    /// <summary>
    /// Update button states based on selection
    /// </summary>
    private void UpdateButtons()
    {
        bool hasSelection = _selectedTheme != null;
        bool isActive = _selectedTheme?.IsActive == true;
        bool isPreset = _selectedTheme?.ThemeType == "Preset";
        bool isCustom = _selectedTheme?.ThemeType == "Custom";

        btnApplyTheme.Enabled = hasSelection && !isActive;
        btnDuplicateTheme.Enabled = hasSelection;
        btnDeleteTheme.Enabled = hasSelection && !isActive && !isPreset;
        btnExportTheme.Enabled = hasSelection && isCustom; // Only custom themes can be exported
    }

    /// <summary>
    /// Apply selected theme
    /// </summary>
    private async void BtnApplyTheme_Click(object? sender, EventArgs e)
    {
        if (_selectedTheme == null)
            return;

        try
        {
            await _themeService.ApplyThemeAsync(_selectedTheme.ThemeId);
            
            GameConsole.Info($"[ThemeSettingsPanel] Applied theme: {_selectedTheme.ThemeName}");
            MessageBox.Show($"Theme '{_selectedTheme.ThemeName}' has been applied.", "Theme Applied", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            await LoadThemesAsync(); // Refresh list
            ThemeChanged?.Invoke(this, EventArgs.Empty);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[ThemeSettingsPanel] Failed to apply theme: {ex.Message}");
            MessageBox.Show($"Failed to apply theme: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Duplicate selected theme
    /// </summary>
    private async void BtnDuplicateTheme_Click(object? sender, EventArgs e)
    {
        if (_selectedTheme == null)
            return;

        var newName = Microsoft.VisualBasic.Interaction.InputBox(
            "Enter name for the duplicated theme:",
            "Duplicate Theme",
            $"{_selectedTheme.ThemeName} (Copy)");

        if (string.IsNullOrWhiteSpace(newName))
            return;

        try
        {
            var duplicatedTheme = await _themeService.DuplicateThemeAsync(
                _selectedTheme.ThemeId,
                newName,
                "Custom");

            GameConsole.Info($"[ThemeSettingsPanel] Duplicated theme: {newName}");
            MessageBox.Show($"Theme '{newName}' has been created.", "Theme Duplicated", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            await LoadThemesAsync();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[ThemeSettingsPanel] Failed to duplicate theme: {ex.Message}");
            MessageBox.Show($"Failed to duplicate theme: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnCreateClassicBlack_Click(object? sender, EventArgs e)
    {
        try
        {
            // Prevent duplicate creation
            var existing = _allThemes.FirstOrDefault(t => string.Equals(t.ThemeName, "Classic Black", StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                MessageBox.Show("Classic Black already exists.", "Create Theme", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var newId = await _themeService.CreateClassicBlackVariantAsync("Classic Gold", "Classic Black");
            GameConsole.Info($"[ThemeSettingsPanel] Created Classic Black theme (ID: {newId})");
            MessageBox.Show("Classic Black theme created.", "Create Theme", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadThemesAsync();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[ThemeSettingsPanel] Failed to create Classic Black: {ex.Message}");
            MessageBox.Show($"Failed to create Classic Black: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Delete selected theme
    /// </summary>
    private async void BtnDeleteTheme_Click(object? sender, EventArgs e)
    {
        if (_selectedTheme == null)
            return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete the theme '{_selectedTheme.ThemeName}'?\n\nThis action cannot be undone.",
            "Confirm Delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
            return;

        try
        {
            await _themeService.DeleteThemeAsync(_selectedTheme.ThemeId);
            
            GameConsole.Info($"[ThemeSettingsPanel] Deleted theme: {_selectedTheme.ThemeName}");
            MessageBox.Show($"Theme '{_selectedTheme.ThemeName}' has been deleted.", "Theme Deleted", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            await LoadThemesAsync();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[ThemeSettingsPanel] Failed to delete theme: {ex.Message}");
            MessageBox.Show($"Failed to delete theme: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Import theme pack (modeled after soundpack import)
    /// </summary>
    private async void BtnImportPack_Click(object? sender, EventArgs e)
    {
        try
        {
            var dialog = new OpenFileDialog
            {
                Title = "Import Theme Pack",
                Filter = "Theme Pack Files (*.zip)|*.zip|All Files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            // Run on separate STA thread to avoid modal deadlock
            DialogResult result = DialogResult.Cancel;
            var thread = new System.Threading.Thread(() =>
            {
                result = dialog.ShowDialog();
            });
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            
            // Keep UI responsive while waiting
            while (thread.IsAlive)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }

            if (result != DialogResult.OK)
                return;

            var packManager = new ThemePackManager(_themeService);
            var (success, message) = await packManager.ImportThemePackAsync(dialog.FileName);

            if (success)
            {
                GameConsole.Info($"[ThemeSettingsPanel] {message}");
                MessageBox.Show(message, "Import Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadThemesAsync(); // Refresh theme list
                SettingsChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                GameConsole.Error($"[ThemeSettingsPanel] Import failed: {message}");
                MessageBox.Show(message, "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[ThemeSettingsPanel] Failed to import theme pack: {ex.Message}");
            MessageBox.Show($"Failed to import theme pack: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Export selected theme (modeled after soundpack export)
    /// </summary>
    private async void BtnExportTheme_Click(object? sender, EventArgs e)
    {
        if (_selectedTheme == null)
            return;

        try
        {
            var dialog = new SaveFileDialog
            {
                Title = "Export Theme",
                Filter = "Theme Pack Files (*.zip)|*.zip",
                FileName = $"{_selectedTheme.ThemeName}.zip",
                RestoreDirectory = true
            };

            // Run on separate STA thread to avoid modal deadlock
            DialogResult result = DialogResult.Cancel;
            var thread = new System.Threading.Thread(() =>
            {
                result = dialog.ShowDialog();
            });
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            
            // Keep UI responsive while waiting
            while (thread.IsAlive)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }

            if (result != DialogResult.OK)
                return;

            var packManager = new ThemePackManager(_themeService);
            var (success, message) = await packManager.ExportThemePackAsync(_selectedTheme.ThemeId, dialog.FileName);

            if (success)
            {
                GameConsole.Info($"[ThemeSettingsPanel] {message}");
                MessageBox.Show($"{message}\n\nLocation: {dialog.FileName}", "Export Complete", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                GameConsole.Error($"[ThemeSettingsPanel] Export failed: {message}");
                MessageBox.Show(message, "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[ThemeSettingsPanel] Failed to export theme: {ex.Message}");
            MessageBox.Show($"Failed to export theme: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Export example theme pack template (modeled after soundpack export)
    /// </summary>
    private void BtnExportExample_Click(object? sender, EventArgs e)
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Theme Pack Files (*.zip)|*.zip",
                Title = "Export Example Theme Pack",
                FileName = "ExampleThemePack.zip",
                RestoreDirectory = true
            };
            
            // Run on separate STA thread to avoid modal deadlock
            DialogResult result = DialogResult.Cancel;
            var thread = new System.Threading.Thread(() =>
            {
                result = dialog.ShowDialog();
            });
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            
            // Keep UI responsive while waiting
            while (thread.IsAlive)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }

            if (result != DialogResult.OK)
                return;

            var packManager = new ThemePackManager(_themeService);
            var success = packManager.ExportExamplePack(dialog.FileName);

            if (success)
            {
                GameConsole.Info($"[ThemeSettingsPanel] Exported example theme pack to: {dialog.FileName}");
                MessageBox.Show(
                    $"Example theme pack exported successfully!\n\n" +
                    $"Location: {dialog.FileName}\n\n" +
                    $"Edit the themepack.xml file inside the ZIP to customize your theme, then import it back.",
                    "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                GameConsole.Error($"[ThemeSettingsPanel] Failed to export example pack");
                MessageBox.Show("Failed to export example pack.", "Export Failed", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[ThemeSettingsPanel] Failed to export example: {ex.Message}");
            MessageBox.Show($"Failed to export example: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Get connection string from app settings
    /// </summary>
    private string GetConnectionString()
    {
        // This will be injected properly when integrated with OptionsDialog
        return _themeService.GetType()
            .GetField("_connectionString", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(_themeService) as string ?? throw new InvalidOperationException("Connection string not available");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _themeService?.Dispose();
            _renderer?.Dispose();
            picPreview.Image?.Dispose();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }
}
