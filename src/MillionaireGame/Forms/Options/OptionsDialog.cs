using MillionaireGame.Core.Settings;
using MillionaireGame.Core.Helpers;
using MillionaireGame.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MillionaireGame.Forms.Options;

public partial class OptionsDialog : Form
{
    private readonly ApplicationSettings _settings;
    private readonly ApplicationSettingsManager _settingsManager;
    private readonly MoneyTreeService _moneyTreeService;
    private bool _hasChanges;
    
    /// <summary>
    /// Event fired when settings are applied (via Apply button or OK button)
    /// </summary>
    public event EventHandler? SettingsApplied;

    public OptionsDialog(ApplicationSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _settingsManager = new ApplicationSettingsManager(); // For saving
        _moneyTreeService = new MoneyTreeService(); // Load money tree settings
        InitializeComponent();
        IconHelper.ApplyToForm(this);
        
        // Initialize Money Tree tab dynamically
        InitializeMoneyTreeTab();
        
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
        
        // Load money tree settings
        LoadMoneyTreeSettings();

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

        // Save money tree settings
        SaveMoneyTreeSettings();

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
        SettingsApplied?.Invoke(this, EventArgs.Empty);
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
        // Check for unsaved changes and show error/warning
        if (_hasChanges)
        {
            var result = MessageBox.Show(
                "You have unsaved changes. Are you sure you want to cancel?",
                "Unsaved Changes",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                return; // Don't close the form
            }
        }
        
        DialogResult = DialogResult.Cancel;
        Close();
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

    private void InitializeMoneyTreeTab()
    {
        // Create main groups
        var grpPrizes = new GroupBox
        {
            Text = "Prize Values",
            Location = new Point(16, 16),
            Size = new Size(320, 420)
        };
        
        var grpCurrency1 = new GroupBox
        {
            Text = "Currency 1",
            Location = new Point(356, 16),
            Size = new Size(200, 180)
        };
        
        var grpCurrency2 = new GroupBox
        {
            Text = "Currency 2",
            Location = new Point(356, 216),
            Size = new Size(200, 200)
        };
        
        // Initialize 15 prize value inputs (DESCENDING - Q15 to Q1)
        int yPos = 25;
        for (int i = 15; i >= 1; i--)
        {
            var lblQuestion = new Label
            {
                Text = $"Q{i}:",
                Location = new Point(10, yPos + 3),
                Size = new Size(35, 15),
                TextAlign = ContentAlignment.MiddleRight
            };
            
            var numValue = new NumericUpDown
            {
                Name = $"numLevel{i:D2}",
                Location = new Point(50, yPos),
                Size = new Size(120, 23),
                Maximum = 10000000,
                ThousandsSeparator = true,
                Tag = i // Store level number in tag
            };
            numValue.ValueChanged += Control_Changed;
            
            // Safety net checkboxes for Q5 and Q10
            if (i == 5 || i == 10)
            {
                var chkSafetyNet = new CheckBox
                {
                    Name = $"chkSafetyNet{i}",
                    Text = "Safety Net",
                    Location = new Point(180, yPos + 2),
                    Size = new Size(90, 20),
                    Tag = i
                };
                chkSafetyNet.CheckedChanged += Control_Changed;
                grpPrizes.Controls.Add(chkSafetyNet);
            }
            
            // Currency selector for each question
            var cmbCurrency = new ComboBox
            {
                Name = $"cmbCurrency{i:D2}",
                Location = new Point(280, yPos),
                Size = new Size(30, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Tag = i
            };
            cmbCurrency.Items.AddRange(new object[] { "1", "2" });
            cmbCurrency.SelectedIndex = 0;
            cmbCurrency.SelectedIndexChanged += Control_Changed;
            
            grpPrizes.Controls.Add(lblQuestion);
            grpPrizes.Controls.Add(numValue);
            grpPrizes.Controls.Add(cmbCurrency);
            yPos += 26;
        }
        
        // Currency 1 selection radio buttons
        var radDollar1 = new RadioButton
        {
            Name = "radCurrency1Dollar",
            Text = "Dollar ($)",
            Location = new Point(15, 25),
            Size = new Size(170, 20),
            Tag = "$"
        };
        radDollar1.CheckedChanged += Currency_CheckedChanged;
        
        var radEuro1 = new RadioButton
        {
            Name = "radCurrency1Euro",
            Text = "Euro (€)",
            Location = new Point(15, 50),
            Size = new Size(170, 20),
            Tag = "€"
        };
        radEuro1.CheckedChanged += Currency_CheckedChanged;
        
        var radPound1 = new RadioButton
        {
            Name = "radCurrency1Pound",
            Text = "Pound (£)",
            Location = new Point(15, 75),
            Size = new Size(170, 20),
            Tag = "£"
        };
        radPound1.CheckedChanged += Currency_CheckedChanged;
        
        var radYen1 = new RadioButton
        {
            Name = "radCurrency1Yen",
            Text = "Yen (¥)",
            Location = new Point(15, 100),
            Size = new Size(170, 20),
            Tag = "¥"
        };
        radYen1.CheckedChanged += Currency_CheckedChanged;
        
        var radOther1 = new RadioButton
        {
            Name = "radCurrency1Other",
            Text = "Other:",
            Location = new Point(15, 125),
            Size = new Size(60, 20)
        };
        radOther1.CheckedChanged += Currency_CheckedChanged;
        
        var txtCustomCurrency1 = new TextBox
        {
            Name = "txtCustomCurrency1",
            Location = new Point(80, 123),
            Size = new Size(100, 23),
            MaxLength = 5
        };
        txtCustomCurrency1.TextChanged += Control_Changed;
        
        var chkSuffix1 = new CheckBox
        {
            Name = "chkCurrency1AtSuffix",
            Text = "Show after value",
            Location = new Point(15, 155),
            Size = new Size(170, 20)
        };
        chkSuffix1.CheckedChanged += Control_Changed;
        
        grpCurrency1.Controls.AddRange(new Control[] { radDollar1, radEuro1, radPound1, radYen1, radOther1, txtCustomCurrency1, chkSuffix1 });
        
        // Currency 2 controls with Enable checkbox
        var chkEnableCurrency2 = new CheckBox
        {
            Name = "chkEnableCurrency2",
            Text = "Enable Second Currency",
            Location = new Point(15, 20),
            Size = new Size(170, 20)
        };
        chkEnableCurrency2.CheckedChanged += EnableCurrency2_CheckedChanged;
        
        var radDollar2 = new RadioButton
        {
            Name = "radCurrency2Dollar",
            Text = "Dollar ($)",
            Location = new Point(15, 50),
            Size = new Size(170, 20),
            Tag = "$",
            Enabled = false
        };
        radDollar2.CheckedChanged += Currency_CheckedChanged;
        
        var radEuro2 = new RadioButton
        {
            Name = "radCurrency2Euro",
            Text = "Euro (€)",
            Location = new Point(15, 75),
            Size = new Size(170, 20),
            Tag = "€",
            Enabled = false
        };
        radEuro2.CheckedChanged += Currency_CheckedChanged;
        
        var radPound2 = new RadioButton
        {
            Name = "radCurrency2Pound",
            Text = "Pound (£)",
            Location = new Point(15, 100),
            Size = new Size(170, 20),
            Tag = "£",
            Enabled = false
        };
        radPound2.CheckedChanged += Currency_CheckedChanged;
        
        var radYen2 = new RadioButton
        {
            Name = "radCurrency2Yen",
            Text = "Yen (¥)",
            Location = new Point(15, 125),
            Size = new Size(170, 20),
            Tag = "¥",
            Enabled = false
        };
        radYen2.CheckedChanged += Currency_CheckedChanged;
        
        var radOther2 = new RadioButton
        {
            Name = "radCurrency2Other",
            Text = "Other:",
            Location = new Point(15, 150),
            Size = new Size(60, 20),
            Enabled = false
        };
        radOther2.CheckedChanged += Currency_CheckedChanged;
        
        var txtCustomCurrency2 = new TextBox
        {
            Name = "txtCustomCurrency2",
            Location = new Point(80, 148),
            Size = new Size(100, 23),
            MaxLength = 5,
            Enabled = false
        };
        txtCustomCurrency2.TextChanged += Control_Changed;
        
        var chkSuffix2 = new CheckBox
        {
            Name = "chkCurrency2AtSuffix",
            Text = "Show after value",
            Location = new Point(15, 175),
            Size = new Size(170, 20),
            Enabled = false
        };
        chkSuffix2.CheckedChanged += Control_Changed;
        
        grpCurrency2.Controls.AddRange(new Control[] { chkEnableCurrency2, radDollar2, radEuro2, radPound2, radYen2, radOther2, txtCustomCurrency2, chkSuffix2 });
        
        // Add all groups to tab
        tabMoneyTree.Controls.AddRange(new Control[] { grpPrizes, grpCurrency1, grpCurrency2 });
    }
    
    private void EnableCurrency2_CheckedChanged(object? sender, EventArgs e)
    {
        if (sender is CheckBox chk)
        {
            // Enable/disable all currency 2 controls
            var grpCurrency2 = tabMoneyTree.Controls.OfType<GroupBox>().FirstOrDefault(g => g.Text == "Currency 2");
            if (grpCurrency2 != null)
            {
                foreach (Control ctrl in grpCurrency2.Controls)
                {
                    if (ctrl.Name != "chkEnableCurrency2")
                    {
                        ctrl.Enabled = chk.Checked;
                    }
                }
            }
            
            // Enable/disable all currency selector dropdowns
            for (int i = 1; i <= 15; i++)
            {
                var cmbCurrency = tabMoneyTree.Controls.Find($"cmbCurrency{i:D2}", true).FirstOrDefault() as ComboBox;
                if (cmbCurrency != null)
                {
                    cmbCurrency.Enabled = chk.Checked;
                    if (!chk.Checked)
                    {
                        cmbCurrency.SelectedIndex = 0; // Reset to Currency 1
                    }
                }
            }
            
            _hasChanges = true;
        }
    }
    
    private void Currency_CheckedChanged(object? sender, EventArgs e)
    {
        if (sender is RadioButton radio && radio.Checked)
        {
            _hasChanges = true;
        }
    }
    
    private void LoadMoneyTreeSettings()
    {
        var settings = _moneyTreeService.Settings;
        
        // Load prize values
        for (int i = 1; i <= 15; i++)
        {
            var numControl = tabMoneyTree.Controls.Find($"numLevel{i:D2}", true).FirstOrDefault() as NumericUpDown;
            if (numControl != null)
            {
                numControl.Value = settings.GetLevelValue(i);
            }
        }
        
        // Load safety net checkboxes
        var chkNet5 = tabMoneyTree.Controls.Find("chkSafetyNet5", true).FirstOrDefault() as CheckBox;
        var chkNet10 = tabMoneyTree.Controls.Find("chkSafetyNet10", true).FirstOrDefault() as CheckBox;
        if (chkNet5 != null) chkNet5.Checked = (settings.SafetyNet1 == 5 || settings.SafetyNet2 == 5);
        if (chkNet10 != null) chkNet10.Checked = (settings.SafetyNet1 == 10 || settings.SafetyNet2 == 10);
        
        // Load Currency 1 settings
        var radDollar1 = tabMoneyTree.Controls.Find("radCurrency1Dollar", true).FirstOrDefault() as RadioButton;
        var radEuro1 = tabMoneyTree.Controls.Find("radCurrency1Euro", true).FirstOrDefault() as RadioButton;
        var radPound1 = tabMoneyTree.Controls.Find("radCurrency1Pound", true).FirstOrDefault() as RadioButton;
        var radYen1 = tabMoneyTree.Controls.Find("radCurrency1Yen", true).FirstOrDefault() as RadioButton;
        var radOther1 = tabMoneyTree.Controls.Find("radCurrency1Other", true).FirstOrDefault() as RadioButton;
        var txtCustom1 = tabMoneyTree.Controls.Find("txtCustomCurrency1", true).FirstOrDefault() as TextBox;
        var chkSuffix1 = tabMoneyTree.Controls.Find("chkCurrency1AtSuffix", true).FirstOrDefault() as CheckBox;
        
        if (radDollar1 != null && radEuro1 != null && radPound1 != null && radYen1 != null && radOther1 != null && txtCustom1 != null && chkSuffix1 != null)
        {
            // Select the appropriate radio button for Currency 1
            switch (settings.Currency)
            {
                case "$": radDollar1.Checked = true; break;
                case "€": radEuro1.Checked = true; break;
                case "£": radPound1.Checked = true; break;
                case "¥": radYen1.Checked = true; break;
                default:
                    radOther1.Checked = true;
                    txtCustom1.Text = settings.Currency;
                    break;
            }
            
            chkSuffix1.Checked = settings.CurrencyAtSuffix;
        }
        
        // Load Currency 2 settings
        var chkEnableCurrency2 = tabMoneyTree.Controls.Find("chkEnableCurrency2", true).FirstOrDefault() as CheckBox;
        var radDollar2 = tabMoneyTree.Controls.Find("radCurrency2Dollar", true).FirstOrDefault() as RadioButton;
        var radEuro2 = tabMoneyTree.Controls.Find("radCurrency2Euro", true).FirstOrDefault() as RadioButton;
        var radPound2 = tabMoneyTree.Controls.Find("radCurrency2Pound", true).FirstOrDefault() as RadioButton;
        var radYen2 = tabMoneyTree.Controls.Find("radCurrency2Yen", true).FirstOrDefault() as RadioButton;
        var radOther2 = tabMoneyTree.Controls.Find("radCurrency2Other", true).FirstOrDefault() as RadioButton;
        var txtCustom2 = tabMoneyTree.Controls.Find("txtCustomCurrency2", true).FirstOrDefault() as TextBox;
        var chkSuffix2 = tabMoneyTree.Controls.Find("chkCurrency2AtSuffix", true).FirstOrDefault() as CheckBox;
        
        // Load Currency 2 enabled state
        if (chkEnableCurrency2 != null)
        {
            chkEnableCurrency2.Checked = settings.Currency2Enabled;
        }
        
        // Load Currency 2 selection
        if (radDollar2 != null && radEuro2 != null && radPound2 != null && radYen2 != null && radOther2 != null && txtCustom2 != null && chkSuffix2 != null)
        {
            switch (settings.Currency2)
            {
                case "$": radDollar2.Checked = true; break;
                case "€": radEuro2.Checked = true; break;
                case "£": radPound2.Checked = true; break;
                case "¥": radYen2.Checked = true; break;
                default:
                    radOther2.Checked = true;
                    txtCustom2.Text = settings.Currency2;
                    break;
            }
            
            chkSuffix2.Checked = settings.Currency2AtSuffix;
        }
        
        // Load currency selectors for each question with saved values
        // Disable if Currency 2 is not enabled
        bool isCurrency2Enabled = chkEnableCurrency2?.Checked ?? false;
        for (int i = 1; i <= 15; i++)
        {
            var cmbCurrency = tabMoneyTree.Controls.Find($"cmbCurrency{i:D2}", true).FirstOrDefault() as ComboBox;
            if (cmbCurrency != null)
            {
                // Load saved currency (1 or 2), default to Currency 1 if invalid
                int savedCurrency = settings.LevelCurrencies[i - 1];
                cmbCurrency.SelectedIndex = (savedCurrency == 2) ? 1 : 0;
                cmbCurrency.Enabled = isCurrency2Enabled; // Disable if Currency 2 not enabled
            }
        }
    }
    
    private void SaveMoneyTreeSettings()
    {
        var settings = _moneyTreeService.Settings;
        
        // Save prize values
        for (int i = 1; i <= 15; i++)
        {
            var numControl = tabMoneyTree.Controls.Find($"numLevel{i:D2}", true).FirstOrDefault() as NumericUpDown;
            if (numControl != null)
            {
                settings.SetLevelValue(i, (int)numControl.Value);
            }
        }
        
        // Save safety nets from checkboxes
        var chkNet5 = tabMoneyTree.Controls.Find("chkSafetyNet5", true).FirstOrDefault() as CheckBox;
        var chkNet10 = tabMoneyTree.Controls.Find("chkSafetyNet10", true).FirstOrDefault() as CheckBox;
        
        // Set safety nets based on checkboxes
        if (chkNet5?.Checked == true && chkNet10?.Checked == true)
        {
            settings.SafetyNet1 = 5;
            settings.SafetyNet2 = 10;
        }
        else if (chkNet5?.Checked == true)
        {
            settings.SafetyNet1 = 5;
            settings.SafetyNet2 = 0; // No second safety net
        }
        else if (chkNet10?.Checked == true)
        {
            settings.SafetyNet1 = 10;
            settings.SafetyNet2 = 0; // No second safety net
        }
        else
        {
            settings.SafetyNet1 = 0;
            settings.SafetyNet2 = 0;
        }
        
        // Save Currency 1
        var radDollar1 = tabMoneyTree.Controls.Find("radCurrency1Dollar", true).FirstOrDefault() as RadioButton;
        var radEuro1 = tabMoneyTree.Controls.Find("radCurrency1Euro", true).FirstOrDefault() as RadioButton;
        var radPound1 = tabMoneyTree.Controls.Find("radCurrency1Pound", true).FirstOrDefault() as RadioButton;
        var radYen1 = tabMoneyTree.Controls.Find("radCurrency1Yen", true).FirstOrDefault() as RadioButton;
        var radOther1 = tabMoneyTree.Controls.Find("radCurrency1Other", true).FirstOrDefault() as RadioButton;
        var txtCustom1 = tabMoneyTree.Controls.Find("txtCustomCurrency1", true).FirstOrDefault() as TextBox;
        
        if (radDollar1?.Checked == true) settings.Currency = "$";
        else if (radEuro1?.Checked == true) settings.Currency = "€";
        else if (radPound1?.Checked == true) settings.Currency = "£";
        else if (radYen1?.Checked == true) settings.Currency = "¥";
        else if (radOther1?.Checked == true && txtCustom1 != null) settings.Currency = txtCustom1.Text;
        
        // Save currency position
        var chkSuffix1 = tabMoneyTree.Controls.Find("chkCurrency1AtSuffix", true).FirstOrDefault() as CheckBox;
        if (chkSuffix1 != null)
        {
            settings.CurrencyAtSuffix = chkSuffix1.Checked;
        }
        
        // Save Currency 2 enabled state
        var chkEnableCurrency2 = tabMoneyTree.Controls.Find("chkEnableCurrency2", true).FirstOrDefault() as CheckBox;
        if (chkEnableCurrency2 != null)
        {
            settings.Currency2Enabled = chkEnableCurrency2.Checked;
        }
        
        // Save Currency 2 settings
        var radDollar2 = tabMoneyTree.Controls.Find("radCurrency2Dollar", true).FirstOrDefault() as RadioButton;
        var radEuro2 = tabMoneyTree.Controls.Find("radCurrency2Euro", true).FirstOrDefault() as RadioButton;
        var radPound2 = tabMoneyTree.Controls.Find("radCurrency2Pound", true).FirstOrDefault() as RadioButton;
        var radYen2 = tabMoneyTree.Controls.Find("radCurrency2Yen", true).FirstOrDefault() as RadioButton;
        var radOther2 = tabMoneyTree.Controls.Find("radCurrency2Other", true).FirstOrDefault() as RadioButton;
        var txtCustom2 = tabMoneyTree.Controls.Find("txtCustomCurrency2", true).FirstOrDefault() as TextBox;
        
        if (radDollar2?.Checked == true) settings.Currency2 = "$";
        else if (radEuro2?.Checked == true) settings.Currency2 = "€";
        else if (radPound2?.Checked == true) settings.Currency2 = "£";
        else if (radYen2?.Checked == true) settings.Currency2 = "¥";
        else if (radOther2?.Checked == true && txtCustom2 != null) settings.Currency2 = txtCustom2.Text;
        
        // Save Currency 2 position
        var chkSuffix2 = tabMoneyTree.Controls.Find("chkCurrency2AtSuffix", true).FirstOrDefault() as CheckBox;
        if (chkSuffix2 != null)
        {
            settings.Currency2AtSuffix = chkSuffix2.Checked;
        }
        
        // Save currency assignments for each question
        for (int i = 1; i <= 15; i++)
        {
            var cmbCurrency = tabMoneyTree.Controls.Find($"cmbCurrency{i:D2}", true).FirstOrDefault() as ComboBox;
            if (cmbCurrency != null && cmbCurrency.SelectedIndex >= 0)
            {
                settings.LevelCurrencies[i - 1] = cmbCurrency.SelectedIndex + 1; // Store 1 or 2
            }
        }
        
        // Save to file
        _moneyTreeService.SaveSettings();
    }
}
