using MillionaireGame.Core.Settings;
using MillionaireGame.Core.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace MillionaireGame.Forms.Options;

public partial class OptionsDialog : Form
{
    private readonly ApplicationSettings _settings;
    private readonly ApplicationSettingsManager _settingsManager;
    private bool _hasChanges;

    public OptionsDialog(ApplicationSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _settingsManager = new ApplicationSettingsManager(); // For saving
        InitializeComponent();
        IconHelper.ApplyToForm(this);
        
        // Handle form closing to respect user's choice about unsaved changes
        FormClosing += OptionsDialog_FormClosing;
        
        LoadSettings();
    }

    private void LoadSettings()
    {
        // Suspend change tracking while loading
        _hasChanges = false;
        // Screen settings
        chkAutoShowHostScreen.Checked = _settings.AutoShowHostScreen;
        chkAutoShowGuestScreen.Checked = _settings.AutoShowGuestScreen;
        chkAutoShowTVScreen.Checked = _settings.AutoShowTVScreen;
        chkFullScreenHostScreen.Checked = _settings.FullScreenHostScreenEnable;
        chkFullScreenGuestScreen.Checked = _settings.FullScreenGuestScreenEnable;
        chkFullScreenTVScreen.Checked = _settings.FullScreenTVScreenEnable;

        // Lifeline settings
        numTotalLifelines.Value = _settings.TotalLifelines;
        UpdateLifelineGroupStates(); // Enable/disable based on total
        
        // Lifeline 1 configuration
        cmbLifeline1Type.SelectedIndex = MapLifelineToIndex(_settings.Lifeline1);
        cmbLifeline1Availability.SelectedIndex = _settings.Lifeline1Available;
        
        // Lifeline 2 configuration
        cmbLifeline2Type.SelectedIndex = MapLifelineToIndex(_settings.Lifeline2);
        cmbLifeline2Availability.SelectedIndex = _settings.Lifeline2Available;
        
        // Lifeline 3 configuration
        cmbLifeline3Type.SelectedIndex = MapLifelineToIndex(_settings.Lifeline3);
        cmbLifeline3Availability.SelectedIndex = _settings.Lifeline3Available;
        
        // Lifeline 4 configuration
        cmbLifeline4Type.SelectedIndex = MapLifelineToIndex(_settings.Lifeline4);
        cmbLifeline4Availability.SelectedIndex = _settings.Lifeline4Available;

        // Load available soundpacks
        LoadAvailableSoundPacks();

        // Select current soundpack
        cmbSoundPack.SelectedItem = _settings.SelectedSoundPack ?? "Default";
        if (cmbSoundPack.SelectedIndex == -1 && cmbSoundPack.Items.Count > 0)
        {
            cmbSoundPack.SelectedIndex = 0; // Default to first pack if selected pack not found
        }

        _hasChanges = false;
    }

    private void UpdateLifelineGroupStates()
    {
        int total = (int)numTotalLifelines.Value;
        
        grpLifeline1.Enabled = total >= 1;
        grpLifeline2.Enabled = total >= 2;
        grpLifeline3.Enabled = total >= 3;
        grpLifeline4.Enabled = total >= 4;
    }

    private void SetLifelineAvailability(ComboBox cmbAvailability, int availability)
    {
        cmbAvailability.SelectedIndex = availability;
    }

    private int GetLifelineAvailability(ComboBox cmbAvailability)
    {
        return cmbAvailability.SelectedIndex;
    }

    private int MapLifelineToIndex(string lifelineName)
    {
        return lifelineName.ToLowerInvariant() switch
        {
            "5050" => 0,
            "plusone" => 1,
            "ata" => 2,
            "switch" => 3,
            _ => 0
        };
    }

    private string MapIndexToLifeline(int index)
    {
        return index switch
        {
            0 => "5050",
            1 => "plusone",
            2 => "ata",
            3 => "switch",
            _ => "5050"
        };
    }

    private void SaveSettings()
    {
        // Screen settings
        _settings.AutoShowHostScreen = chkAutoShowHostScreen.Checked;
        _settings.AutoShowGuestScreen = chkAutoShowGuestScreen.Checked;
        _settings.AutoShowTVScreen = chkAutoShowTVScreen.Checked;
        _settings.FullScreenHostScreenEnable = chkFullScreenHostScreen.Checked;
        _settings.FullScreenGuestScreenEnable = chkFullScreenGuestScreen.Checked;
        _settings.FullScreenTVScreenEnable = chkFullScreenTVScreen.Checked;

        // Lifeline settings
        _settings.TotalLifelines = (int)numTotalLifelines.Value;
        _settings.Lifeline1 = MapIndexToLifeline(cmbLifeline1Type.SelectedIndex);
        _settings.Lifeline1Available = GetLifelineAvailability(cmbLifeline1Availability);
        _settings.Lifeline2 = MapIndexToLifeline(cmbLifeline2Type.SelectedIndex);
        _settings.Lifeline2Available = GetLifelineAvailability(cmbLifeline2Availability);
        _settings.Lifeline3 = MapIndexToLifeline(cmbLifeline3Type.SelectedIndex);
        _settings.Lifeline3Available = GetLifelineAvailability(cmbLifeline3Availability);
        _settings.Lifeline4 = MapIndexToLifeline(cmbLifeline4Type.SelectedIndex);
        _settings.Lifeline4Available = GetLifelineAvailability(cmbLifeline4Availability);

        // Sound settings (main game sounds)
        // Save selected soundpack
        if (cmbSoundPack.SelectedItem != null)
        {
            _settings.SelectedSoundPack = cmbSoundPack.SelectedItem.ToString() ?? "Default";
        }

        // Save settings through the manager
        _settingsManager.SaveSettings();
        _hasChanges = false;
    }

    private void MarkChanged()
    {
        _hasChanges = true;
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        SaveSettings();
        DialogResult = DialogResult.OK;
        Close();
    }

    private void OptionsDialog_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // Only prompt if there are unsaved changes and user didn't click OK
        if (_hasChanges && DialogResult != DialogResult.OK)
        {
            var result = MessageBox.Show(
                "You have unsaved changes. Do you want to discard them?",
                "Unsaved Changes",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                e.Cancel = true; // Cancel the form closing
                DialogResult = DialogResult.None; // Reset DialogResult so form stays open
            }
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        // Simply close the form - FormClosing will handle unsaved changes prompt
        Close();
    }

    private void btnApply_Click(object sender, EventArgs e)
    {
        SaveSettings();
        MessageBox.Show("Settings saved successfully.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnBrowseSoundFile_Click(object sender, EventArgs e)
    {
        if (sender is not Button btn)
            return;

        // Find the textbox by its name from the tag
        var textBoxName = btn.Tag as string;
        if (string.IsNullOrEmpty(textBoxName))
            return;

        // Get the textbox control
        var textBox = FindControl<TextBox>(this, textBoxName);
        if (textBox == null)
            return;

        using var dialog = new OpenFileDialog
        {
            Filter = "Sound Files (*.wav;*.mp3)|*.wav;*.mp3|All Files (*.*)|*.*",
            Title = "Select Sound File"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            textBox.Text = dialog.FileName;
            MarkChanged();
        }
    }

    private static T? FindControl<T>(Control parent, string name) where T : Control
    {
        foreach (Control control in parent.Controls)
        {
            if (control is T typedControl && control.Name == name)
                return typedControl;

            if (control.HasChildren)
            {
                var found = FindControl<T>(control, name);
                if (found != null)
                    return found;
            }
        }
        return null;
    }

    // Update lifeline group box enabled states based on total lifelines setting
    private void UpdateLifelineGroupBoxStates()
    {
        int totalLifelines = (int)numTotalLifelines.Value;
        grpLifeline1.Enabled = totalLifelines >= 1;
        grpLifeline2.Enabled = totalLifelines >= 2;
        grpLifeline3.Enabled = totalLifelines >= 3;
        grpLifeline4.Enabled = totalLifelines >= 4;
    }

    // Event handler for total lifelines value changed
    private void numTotalLifelines_ValueChanged(object sender, EventArgs e)
    {
        UpdateLifelineGroupBoxStates();
        MarkChanged();
    }

    // Change event handlers
    private void Control_Changed(object sender, EventArgs e)
    {
        MarkChanged();
    }

    // Soundpack management methods
    private void LoadAvailableSoundPacks()
    {
        try
        {
            var soundService = Program.ServiceProvider?.GetRequiredService<Services.SoundService>();
            if (soundService == null) return;
            
            var soundPackManager = soundService.GetSoundPackManager();
            if (soundPackManager == null) return;
            
            var packs = soundPackManager.GetAvailableSoundPacks();
            cmbSoundPack.Items.Clear();
            
            foreach (var pack in packs)
            {
                cmbSoundPack.Items.Add(pack);
            }
            
            // Select current soundpack
            var currentPack = _settings.SelectedSoundPack ?? "Default";
            var index = cmbSoundPack.Items.IndexOf(currentPack);
            if (index >= 0)
            {
                cmbSoundPack.SelectedIndex = index;
            }
            else if (cmbSoundPack.Items.Count > 0)
            {
                cmbSoundPack.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading soundpacks: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void cmbSoundPack_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            if (cmbSoundPack.SelectedItem == null) return;

            var soundService = Program.ServiceProvider?.GetRequiredService<Services.SoundService>();
            if (soundService == null)
            {
                lstSoundPackInfo.Items.Add("Error: Sound service not available");
                return;
            }
            
            var soundPackManager = soundService.GetSoundPackManager();
            if (soundPackManager == null)
            {
                lstSoundPackInfo.Items.Add("Error: Sound pack manager not available");
                return;
            }
            
            string selectedPack = cmbSoundPack.SelectedItem.ToString() ?? "Default";
            
            if (soundPackManager.LoadSoundPack(selectedPack))
            {
                var sounds = soundPackManager.CurrentSounds;
                if (sounds != null && sounds.Count > 0)
                {
                    UpdateSoundPackInfo(sounds);
                    MarkChanged();
                }
                else
                {
                    lstSoundPackInfo.Items.Clear();
                    lstSoundPackInfo.Items.Add($"No sounds found in pack '{selectedPack}'");
                }
            }
            else
            {
                lstSoundPackInfo.Items.Clear();
                lstSoundPackInfo.Items.Add($"Failed to load soundpack '{selectedPack}'");
            }
        }
        catch (Exception ex)
        {
            lstSoundPackInfo.Items.Clear();
            lstSoundPackInfo.Items.Add($"Error: {ex.Message}");
            MessageBox.Show($"Error loading soundpack: {ex.Message}\n\n{ex.StackTrace}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateSoundPackInfo(IReadOnlyDictionary<string, string> sounds)
    {
        lstSoundPackInfo.Items.Clear();

        if (sounds == null || sounds.Count == 0)
        {
            lstSoundPackInfo.Items.Add("No sounds loaded");
            return;
        }

        // Group sounds by category
        var categories = new Dictionary<string, List<string>>
        {
            ["General/Broadcast"] = new List<string>(),
            ["Fastest Finger First"] = new List<string>(),
            ["Lifelines"] = new List<string>(),
            ["Question Lights Down"] = new List<string>(),
            ["Question Bed Music"] = new List<string>(),
            ["Final Answer"] = new List<string>(),
            ["Correct Answer"] = new List<string>(),
            ["Wrong Answer"] = new List<string>()
        };

        // Categorize sounds (skip empty sound paths)
        foreach (var sound in sounds)
        {
            // Skip sounds with empty file paths
            if (string.IsNullOrWhiteSpace(sound.Value))
                continue;

            string key = sound.Key;
            string fileName = Path.GetFileName(sound.Value);
            
            if (key.Contains("Host") || key.Contains("Opening") || key.Contains("Explain") || 
                key.Contains("Quit") || key.Contains("Walk") || key.Contains("Game") || 
                key.Contains("Close") || key.Contains("Commercial") || key.Contains("Risk") ||
                key.Contains("Random") || key.Contains("Safety") || key.Contains("ToHotSeat"))
                categories["General/Broadcast"].Add($"  {key}: {fileName}");
            else if (key.Contains("FFF") || key.Contains("Fastest"))
                categories["Fastest Finger First"].Add($"  {key}: {fileName}");
            else if (key.Contains("5050") || key.Contains("PAF") || key.Contains("ATA") || 
                     key.Contains("Switch") || key.Contains("Lifeline") || key.Contains("Double"))
                categories["Lifelines"].Add($"  {key}: {fileName}");
            else if (key.Contains("LightsDown"))
                categories["Question Lights Down"].Add($"  {key}: {fileName}");
            else if (key.Contains("Bed"))
                categories["Question Bed Music"].Add($"  {key}: {fileName}");
            else if (key.Contains("Final"))
                categories["Final Answer"].Add($"  {key}: {fileName}");
            else if (key.Contains("Correct"))
                categories["Correct Answer"].Add($"  {key}: {fileName}");
            else if (key.Contains("Wrong"))
                categories["Wrong Answer"].Add($"  {key}: {fileName}");
        }

        // Display categorized sounds
        foreach (var category in categories)
        {
            if (category.Value.Count > 0)
            {
                lstSoundPackInfo.Items.Add($"[{category.Key}]");
                foreach (var sound in category.Value.OrderBy(s => s))
                {
                    lstSoundPackInfo.Items.Add(sound);
                }
                lstSoundPackInfo.Items.Add(""); // Empty line between categories
            }
        }
    }

    private void btnImportPack_Click(object sender, EventArgs e)
    {
        Console.WriteLine("[OptionsDialog] === IMPORT BUTTON CLICKED ===");
        Console.WriteLine($"[OptionsDialog] Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
        Console.WriteLine($"[OptionsDialog] IsHandleCreated: {IsHandleCreated}");
        Console.WriteLine($"[OptionsDialog] InvokeRequired: {InvokeRequired}");
        
        try
        {
            Console.WriteLine("[OptionsDialog] Step 1: Entering try block");
            Console.WriteLine("[OptionsDialog] Step 2: About to create OpenFileDialog");
            
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "Zip files (*.zip)|*.zip|All files (*.*)|*.*",
                Title = "Import Sound Pack",
                RestoreDirectory = true
            };
            
            Console.WriteLine("[OptionsDialog] Step 3: OpenFileDialog created successfully");
            Console.WriteLine($"[OptionsDialog] Step 4: Dialog filter: {openFileDialog.Filter}");
            Console.WriteLine($"[OptionsDialog] Step 5: Dialog title: {openFileDialog.Title}");
            Console.WriteLine("[OptionsDialog] Step 6: Running dialog on separate thread");
            
            // Save current change state - DoEvents might trigger spurious change events
            bool originalHasChanges = _hasChanges;
            
            // Run on separate thread to avoid modal deadlock
            DialogResult result = DialogResult.Cancel;
            var thread = new System.Threading.Thread(() =>
            {
                Console.WriteLine("[OptionsDialog] Step 6.5: In new thread");
                result = openFileDialog.ShowDialog();
                Console.WriteLine($"[OptionsDialog] Step 6.75: Dialog returned {result}");
            });
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            
            // Keep UI responsive while waiting
            while (thread.IsAlive)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }
            
            // Restore original change state
            _hasChanges = originalHasChanges;
            
            Console.WriteLine($"[OptionsDialog] Step 7: ShowDialog() returned with result: {result}");
            
            if (result == DialogResult.OK)
            {
                Console.WriteLine($"[OptionsDialog] Step 8: User selected file: {openFileDialog.FileName}");
                var soundService = Program.ServiceProvider?.GetRequiredService<Services.SoundService>();
                if (soundService == null)
                {
                    MessageBox.Show("Sound service not available.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                var soundPackManager = soundService.GetSoundPackManager();
                if (soundPackManager == null)
                {
                    MessageBox.Show("Sound pack manager not available.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                Cursor = Cursors.WaitCursor;
                (bool success, string message) = soundPackManager.ImportSoundPack(openFileDialog.FileName);
                Cursor = Cursors.Default;
                
                MessageBox.Show(message, success ? "Success" : "Error", 
                    MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                
                if (success)
                {
                    LoadAvailableSoundPacks();
                }
            }
        }
        catch (Exception ex)
        {
            Cursor = Cursors.Default;
            MessageBox.Show($"Error importing soundpack: {ex.Message}\n\n{ex.StackTrace}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnRemovePack_Click(object sender, EventArgs e)
    {
        if (cmbSoundPack.SelectedItem == null) return;

        string selectedPack = cmbSoundPack.SelectedItem.ToString() ?? "Default";
        if (selectedPack == "Default")
        {
            MessageBox.Show("The Default soundpack cannot be removed.", "Cannot Remove", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var result = MessageBox.Show($"Are you sure you want to remove the '{selectedPack}' soundpack?", 
            "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            var soundService = Program.ServiceProvider.GetRequiredService<Services.SoundService>();
            var soundPackManager = soundService.GetSoundPackManager();
            
            if (soundPackManager != null)
            {
                // TODO: Implement RemoveSoundPack in SoundPackManager
                MessageBox.Show("Pack removal will be implemented in a future update.", 
                    "Not Yet Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // When implemented:
                // bool success = soundPackManager.RemoveSoundPack(selectedPack);
                // if (success)
                // {
                //     LoadAvailableSoundPacks();
                //     cmbSoundPack.SelectedItem = "Default";
                // }
            }
        }
    }

    private void btnExportExample_Click(object sender, EventArgs e)
    {
        Console.WriteLine("[OptionsDialog] === EXPORT EXAMPLE BUTTON CLICKED ===");
        Console.WriteLine($"[OptionsDialog] Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
        Console.WriteLine($"[OptionsDialog] IsHandleCreated: {IsHandleCreated}");
        Console.WriteLine($"[OptionsDialog] InvokeRequired: {InvokeRequired}");
        
        try
        {
            Console.WriteLine("[OptionsDialog] Step 1: Entering try block");
            Console.WriteLine("[OptionsDialog] Step 2: About to create SaveFileDialog");
            
            using var saveFileDialog = new SaveFileDialog
            {
                Filter = "Zip files (*.zip)|*.zip",
                Title = "Export Example Sound Pack",
                FileName = "ExampleSoundPack.zip",
                RestoreDirectory = true
            };
            
            Console.WriteLine("[OptionsDialog] Step 3: SaveFileDialog created successfully");
            Console.WriteLine($"[OptionsDialog] Step 4: Dialog filter: {saveFileDialog.Filter}");
            Console.WriteLine($"[OptionsDialog] Step 5: Dialog title: {saveFileDialog.Title}");
            Console.WriteLine($"[OptionsDialog] Step 6: Dialog filename: {saveFileDialog.FileName}");
            Console.WriteLine("[OptionsDialog] Step 7: Running dialog on separate thread");
            
            // Save current change state - DoEvents might trigger spurious change events
            bool originalHasChanges = _hasChanges;
            
            // Run on separate thread to avoid modal deadlock
            DialogResult result = DialogResult.Cancel;
            var thread = new System.Threading.Thread(() =>
            {
                Console.WriteLine("[OptionsDialog] Step 7.5: In new thread");
                result = saveFileDialog.ShowDialog();
                Console.WriteLine($"[OptionsDialog] Step 7.75: Dialog returned {result}");
            });
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            
            // Keep UI responsive while waiting
            while (thread.IsAlive)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }
            
            // Restore original change state
            _hasChanges = originalHasChanges;
            
            Console.WriteLine($"[OptionsDialog] Step 8: ShowDialog() returned with result: {result}");
            
            if (result == DialogResult.OK)
            {
                Console.WriteLine($"[OptionsDialog] Step 9: User selected file: {saveFileDialog.FileName}");
                var soundService = Program.ServiceProvider?.GetRequiredService<Services.SoundService>();
                if (soundService == null)
                {
                    MessageBox.Show("Sound service not available.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                var soundPackManager = soundService.GetSoundPackManager();
                if (soundPackManager == null)
                {
                    MessageBox.Show("Sound pack manager not available.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                bool success = soundPackManager.ExportExamplePack(saveFileDialog.FileName);
                MessageBox.Show(success ? "Example soundpack exported successfully!" : "Failed to export example soundpack.",
                    success ? "Success" : "Error", 
                    MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error exporting soundpack: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

