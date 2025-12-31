using MillionaireGame.Core.Settings;
using MillionaireGame.Core.Helpers;
using MillionaireGame.Core.Services;
using MillionaireGame.Utilities;
using MillionaireGame.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Management;
using System.Data;

namespace MillionaireGame.Forms.Options;

public partial class OptionsDialog : Form
{
    private readonly ApplicationSettings _settings;
    private readonly ApplicationSettingsManager _settingsManager;
    private readonly MoneyTreeService _moneyTreeService;
    private bool _hasChanges;
    private readonly DataTable _soundPackDataTable = new DataTable();
    private readonly DataView _soundPackDataView;
    
    /// <summary>
    /// Event fired when settings are applied (via Apply button or OK button)
    /// </summary>
    public event EventHandler? SettingsApplied;

    public OptionsDialog(ApplicationSettings settings, ApplicationSettingsManager settingsManager)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
        _moneyTreeService = new MoneyTreeService(); // Load money tree settings
        
        // Initialize DataView before InitializeComponent (which may trigger events)
        _soundPackDataView = new DataView(_soundPackDataTable);
        
        InitializeComponent();
        IconHelper.ApplyToForm(this);
        
        // Initialize Money Tree tab dynamically
        InitializeMoneyTreeTab();
        
        // Show debug mode label if in DEBUG configuration
#if DEBUG
        lblDebugMode.Visible = true;
#else
        lblDebugMode.Visible = false;
#endif
        
        // Populate monitor dropdowns
        PopulateMonitorDropdowns();
        
        // Update monitor count display and enforce restrictions
        UpdateMonitorStatus();
        
        // Handle form closing to respect user's choice about unsaved changes
        FormClosing += OptionsDialog_FormClosing;
        
        // Initialize soundpack DataGridView
        InitializeSoundPackDataGrid();
        
        // Wire up audio device refresh button
        btnRefreshDevices.Click += (s, e) => LoadAudioDevices();
        
        LoadSettings();
    }

    private void LoadSettings()
    {
        // Suspend change tracking while loading
        _hasChanges = false;
        // Screen settings
        chkEnablePreviewAutomatically.Checked = _settings.EnablePreviewAutomatically;
        cmbPreviewOrientation.SelectedItem = _settings.PreviewOrientation;
        if (cmbPreviewOrientation.SelectedIndex == -1)
            cmbPreviewOrientation.SelectedIndex = 0; // Default to Vertical
        chkFullScreenHostScreen.Checked = _settings.FullScreenHostScreenEnable;
        chkFullScreenGuestScreen.Checked = _settings.FullScreenGuestScreenEnable;
        chkFullScreenTVScreen.Checked = _settings.FullScreenTVScreenEnable;
        
        // Monitor selections
        SelectMonitorIndex(cmbMonitorHost, _settings.FullScreenHostScreenMonitor);
        SelectMonitorIndex(cmbMonitorGuest, _settings.FullScreenGuestScreenMonitor);
        SelectMonitorIndex(cmbMonitorTV, _settings.FullScreenTVScreenMonitor);

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
        
        // Load audio devices for mixer
        LoadAudioDevices();
        
        // Load audio settings (silence detection, crossfade, processing)
        LoadAudioSettings();
        
        // Load broadcast settings (TV screen background)
        LoadBroadcastSettings();
        
        // Console windows are always available via buttons (removed settings)
        
        // Load money tree settings
        LoadMoneyTreeSettings();
        
        // Load audience/web server settings
        LoadAudienceSettings();
        
        // Update dropdown enabled states based on full-screen checkboxes
        UpdateDropdownEnabledStates();

        _hasChanges = false;
    }
    
    private void UpdateDropdownEnabledStates()
    {
        // Enable dropdown when checkbox is checked (opposite of before)
        cmbMonitorHost.Enabled = chkFullScreenHostScreen.Checked;
        cmbMonitorGuest.Enabled = chkFullScreenGuestScreen.Checked;
        cmbMonitorTV.Enabled = chkFullScreenTVScreen.Checked;
    }
    
    #region Full Screen Checkbox Event Handlers
    
    private void chkFullScreenHost_CheckedChanged(object sender, EventArgs e)
    {
        // Mark settings as changed
        MarkChanged();
        
        // Update dropdown enabled state
        cmbMonitorHost.Enabled = chkFullScreenHostScreen.Checked;
        
        // Update checkbox enabled states
        UpdateCheckboxEnabledStates();
        
        // Refresh monitor dropdowns to exclude selected monitors
        RefreshMonitorDropdowns();
        
        // Apply full-screen immediately if checked and host screen is open
        ApplyFullScreenToOpenScreen("Host");
    }
    
    private void chkFullScreenGuest_CheckedChanged(object sender, EventArgs e)
    {
        // Mark settings as changed
        MarkChanged();
        
        // Update dropdown enabled state
        cmbMonitorGuest.Enabled = chkFullScreenGuestScreen.Checked;
        
        // Update checkbox enabled states
        UpdateCheckboxEnabledStates();
        
        // Refresh monitor dropdowns to exclude selected monitors
        RefreshMonitorDropdowns();
        
        // Apply full-screen immediately if checked and guest screen is open
        ApplyFullScreenToOpenScreen("Guest");
    }
    
    private void chkFullScreenTV_CheckedChanged(object sender, EventArgs e)
    {
        // Mark settings as changed
        MarkChanged();
        
        // Update dropdown enabled state
        cmbMonitorTV.Enabled = chkFullScreenTVScreen.Checked;
        
        // Update checkbox enabled states
        UpdateCheckboxEnabledStates();
        
        // Refresh monitor dropdowns to exclude selected monitors
        RefreshMonitorDropdowns();
        
        // Apply full-screen immediately if checked and TV screen is open
        ApplyFullScreenToOpenScreen("TV");
    }
    
    private void ApplyFullScreenToOpenScreen(string screenType)
    {
        // Get the main control panel form
        var controlPanel = Application.OpenForms.OfType<ControlPanelForm>().FirstOrDefault();
        if (controlPanel == null) return;
        
        // Update menu item enabled states
        controlPanel.UpdateScreenMenuItemStates();
        
        // Apply full-screen state to the screen if it's open
        switch (screenType)
        {
            case "Host":
                controlPanel.ApplyFullScreenToHostScreen(
                    chkFullScreenHostScreen.Checked, 
                    GetSelectedMonitorIndex(cmbMonitorHost));
                break;
            case "Guest":
                controlPanel.ApplyFullScreenToGuestScreen(
                    chkFullScreenGuestScreen.Checked, 
                    GetSelectedMonitorIndex(cmbMonitorGuest));
                break;
            case "TV":
                controlPanel.ApplyFullScreenToTVScreen(
                    chkFullScreenTVScreen.Checked, 
                    GetSelectedMonitorIndex(cmbMonitorTV));
                break;
        }
    }
    
    #endregion

    private void UpdateLifelineGroupStates()
    {
        int total = (int)numTotalLifelines.Value;
        
        // Enable/disable lifeline 1 controls
        lblLifeline1.Enabled = total >= 1;
        cmbLifeline1Type.Enabled = total >= 1;
        cmbLifeline1Availability.Enabled = total >= 1;
        
        // Enable/disable lifeline 2 controls
        lblLifeline2.Enabled = total >= 2;
        cmbLifeline2Type.Enabled = total >= 2;
        cmbLifeline2Availability.Enabled = total >= 2;
        
        // Enable/disable lifeline 3 controls
        lblLifeline3.Enabled = total >= 3;
        cmbLifeline3Type.Enabled = total >= 3;
        cmbLifeline3Availability.Enabled = total >= 3;
        
        // Enable/disable lifeline 4 controls
        lblLifeline4.Enabled = total >= 4;
        cmbLifeline4Type.Enabled = total >= 4;
        cmbLifeline4Availability.Enabled = total >= 4;
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
            "ath" => 4,
            "dd" => 5,
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
            4 => "ath",
            5 => "dd",
            _ => "5050"
        };
    }

    private void SaveSettings()
    {
        // Screen settings
        _settings.EnablePreviewAutomatically = chkEnablePreviewAutomatically.Checked;
        _settings.PreviewOrientation = cmbPreviewOrientation.SelectedItem?.ToString() ?? "Vertical";
        _settings.FullScreenHostScreenEnable = chkFullScreenHostScreen.Checked;
        _settings.FullScreenGuestScreenEnable = chkFullScreenGuestScreen.Checked;
        _settings.FullScreenTVScreenEnable = chkFullScreenTVScreen.Checked;
        
        // Monitor selections
        _settings.FullScreenHostScreenMonitor = GetSelectedMonitorIndex(cmbMonitorHost);
        _settings.FullScreenGuestScreenMonitor = GetSelectedMonitorIndex(cmbMonitorGuest);
        _settings.FullScreenTVScreenMonitor = GetSelectedMonitorIndex(cmbMonitorTV);

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
        
        // Save audio device selection
        if (cmbAudioDevice.SelectedItem is Services.AudioDeviceInfo selectedDevice)
        {
            // Get the new device ID (null for default)
            var newDeviceId = selectedDevice.IsDefault ? null : selectedDevice.DeviceId;
            
            // Only apply device change if it actually changed
            if (newDeviceId != _settings.AudioOutputDevice)
            {
                _settings.AudioOutputDevice = newDeviceId;
                
                // Apply device change to SoundService immediately
                try
                {
                    var soundService = Program.ServiceProvider?.GetRequiredService<Services.SoundService>();
                    soundService?.SetAudioOutputDevice(_settings.AudioOutputDevice);
                    
                    if (Program.DebugMode)
                    {
                        GameConsole.Debug($"[OptionsDialog] Audio output device changed to: {selectedDevice.FriendlyName}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to change audio device: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                if (Program.DebugMode)
                {
                    GameConsole.Debug($"[OptionsDialog] Audio device unchanged: {selectedDevice.FriendlyName}");
                }
            }
        }

        // Save audio settings (silence detection, crossfade, processing)
        SaveAudioSettings();

        // Save broadcast settings (TV screen background)
        SaveBroadcastSettings();

        // Console windows are opened via buttons, no settings to save

        // Save money tree settings
        SaveMoneyTreeSettings();
        
        // Save audience/web server settings
        SaveAudienceSettings();

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
#if !DEBUG
        // In release mode, validate that no two screens are assigned to the same monitor
        if (chkFullScreenHostScreen.Checked && chkFullScreenGuestScreen.Checked && 
            GetSelectedMonitorIndex(cmbMonitorHost) == GetSelectedMonitorIndex(cmbMonitorGuest))
        {
            MessageBox.Show(
                "You cannot assign more than one screen to the same monitor!",
                "Invalid Monitor Assignment",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }
        
        if (chkFullScreenHostScreen.Checked && chkFullScreenTVScreen.Checked && 
            GetSelectedMonitorIndex(cmbMonitorHost) == GetSelectedMonitorIndex(cmbMonitorTV))
        {
            MessageBox.Show(
                "You cannot assign more than one screen to the same monitor!",
                "Invalid Monitor Assignment",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }
        
        if (chkFullScreenGuestScreen.Checked && chkFullScreenTVScreen.Checked && 
            GetSelectedMonitorIndex(cmbMonitorGuest) == GetSelectedMonitorIndex(cmbMonitorTV))
        {
            MessageBox.Show(
                "You cannot assign more than one screen to the same monitor!",
                "Invalid Monitor Assignment",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }
#endif
        
        SaveSettings();
        
        // Update console visibility based on new setting
        Program.UpdateConsoleVisibility(_settings.ShowConsole);
        
        SettingsApplied?.Invoke(this, EventArgs.Empty);
        DialogResult = DialogResult.OK;
        Close();
    }

    private void OptionsDialog_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // Only prompt if there are unsaved changes and user didn't click OK or Cancel
        if (_hasChanges && DialogResult != DialogResult.OK && DialogResult != DialogResult.Cancel)
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
        // Set DialogResult first to prevent FormClosing from showing another dialog
        DialogResult = DialogResult.Cancel;
        
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
                DialogResult = DialogResult.None; // Reset so form stays open
                return; // Don't close the form
            }
        }
        
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
        
        // Enable/disable lifeline 1 controls
        lblLifeline1.Enabled = totalLifelines >= 1;
        cmbLifeline1Type.Enabled = totalLifelines >= 1;
        cmbLifeline1Availability.Enabled = totalLifelines >= 1;
        
        // Enable/disable lifeline 2 controls
        lblLifeline2.Enabled = totalLifelines >= 2;
        cmbLifeline2Type.Enabled = totalLifelines >= 2;
        cmbLifeline2Availability.Enabled = totalLifelines >= 2;
        
        // Enable/disable lifeline 3 controls
        lblLifeline3.Enabled = totalLifelines >= 3;
        cmbLifeline3Type.Enabled = totalLifelines >= 3;
        cmbLifeline3Availability.Enabled = totalLifelines >= 3;
        
        // Enable/disable lifeline 4 controls
        lblLifeline4.Enabled = totalLifelines >= 4;
        cmbLifeline4Type.Enabled = totalLifelines >= 4;
        cmbLifeline4Availability.Enabled = totalLifelines >= 4;
    }

    // Event handler for total lifelines value changed
    private void numTotalLifelines_ValueChanged(object sender, EventArgs e)
    {
        UpdateLifelineGroupBoxStates();
        MarkChanged();
    }

    #region Audience/Web Server Settings

    private void LoadAudienceSettings()
    {
        // Populate IP address dropdown
        PopulateIPAddresses();
        
        // Load port
        txtServerPort.Text = _settings.AudienceServerPort.ToString();
        
        // Load auto-start checkbox
        chkAutoStart.Checked = _settings.AudienceServerAutoStart;
        
        // Update control states based on auto-start and server status
        UpdateAudienceControlStates();
        
        // Update server status label
        UpdateServerStatusLabel();
    }

    private void SaveAudienceSettings()
    {
        // Save IP address
        if (cmbServerIP.SelectedItem != null)
        {
            var selectedItem = cmbServerIP.SelectedItem.ToString() ?? "127.0.0.1";
            // Extract just the IP address (before the dash separator if present)
            var parts = selectedItem.Split(new[] { " - " }, StringSplitOptions.None);
            var ipAddress = parts[0].Trim();
            
            // Strip CIDR notation if present (e.g., "192.168.1.5/24" -> "192.168.1.5")
            var slashIndex = ipAddress.IndexOf('/');
            if (slashIndex >= 0)
            {
                ipAddress = ipAddress.Substring(0, slashIndex);
            }
            
            _settings.AudienceServerIP = ipAddress;
        }
        
        // Save port (with validation)
        if (int.TryParse(txtServerPort.Text, out var port) && NetworkHelper.IsValidPort(port))
        {
            _settings.AudienceServerPort = port;
        }
        
        // Save auto-start
        _settings.AudienceServerAutoStart = chkAutoStart.Checked;
    }

    private void PopulateIPAddresses()
    {
        cmbServerIP.Items.Clear();
        
        // Add all interfaces option
        cmbServerIP.Items.Add("0.0.0.0 - All Interfaces (Open to All)");
        
        // Add local IP addresses
        var localIPs = NetworkHelper.GetLocalIPAddresses();
        foreach (var ip in localIPs)
        {
            // IP is now in format "192.168.1.10/24"
            cmbServerIP.Items.Add($"{ip} - Local Network");
        }
        
        // Add localhost option
        cmbServerIP.Items.Add("127.0.0.1 - Localhost Only");
        
        // Select the current setting
        var currentIP = _settings.AudienceServerIP;
        for (int i = 0; i < cmbServerIP.Items.Count; i++)
        {
            var item = cmbServerIP.Items[i]!.ToString()!;
            if (item.StartsWith(currentIP))
            {
                cmbServerIP.SelectedIndex = i;
                return;
            }
        }
        
        // Default to localhost if not found
        cmbServerIP.SelectedIndex = cmbServerIP.Items.Count - 1;
    }

    private void UpdateAudienceControlStates()
    {
        bool isServerRunning = GetWebServerHost()?.IsRunning ?? false;
        
        // IP and Port controls: Only disabled when server is running
        cmbServerIP.Enabled = !isServerRunning;
        txtServerPort.Enabled = !isServerRunning;
        btnCheckPort.Enabled = !isServerRunning;
        
        // Auto-start checkbox: Always enabled (removed server running constraint)
        
        // Server control buttons
        btnStartServer.Enabled = !isServerRunning;
        btnStopServer.Enabled = isServerRunning;
    }

    private void UpdateServerStatusLabel()
    {
        var webServerHost = GetWebServerHost();
        if (webServerHost != null && webServerHost.IsRunning)
        {
            lblServerStatus.Text = $"Server started at {webServerHost.BaseUrl}";
            lblServerStatus.ForeColor = Color.Green;
        }
        else
        {
            lblServerStatus.Text = "Server Stopped";
            lblServerStatus.ForeColor = SystemColors.ControlDarkDark;
        }
    }

    private WebServerHost? GetWebServerHost()
    {
        // Get the main control panel form and retrieve the web server host
        var controlPanel = Application.OpenForms.OfType<ControlPanelForm>().FirstOrDefault();
        return controlPanel?.WebServerHost;
    }

    private void btnCheckPort_Click(object sender, EventArgs e)
    {
        if (!int.TryParse(txtServerPort.Text, out var port))
        {
            lblPortStatus.Text = "❌";
            lblPortStatus.ForeColor = Color.Red;
            MessageBox.Show("Invalid port number. Please enter a number between 1 and 65535.", 
                "Invalid Port", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        
        if (!NetworkHelper.IsValidPort(port))
        {
            lblPortStatus.Text = "❌";
            lblPortStatus.ForeColor = Color.Red;
            MessageBox.Show("Port number must be between 1 and 65535.", 
                "Invalid Port", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        
        bool isAvailable = NetworkHelper.IsPortAvailable(port);
        
        if (isAvailable)
        {
            lblPortStatus.Text = "✓";
            lblPortStatus.ForeColor = Color.Green;
        }
        else
        {
            lblPortStatus.Text = "❌";
            lblPortStatus.ForeColor = Color.Red;
            MessageBox.Show($"Port {port} is already in use.", 
                "Port In Use", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void chkAutoStart_CheckedChanged(object sender, EventArgs e)
    {
        UpdateAudienceControlStates();
        MarkChanged();
    }

    private async void btnStartServer_Click(object sender, EventArgs e)
    {
        // Validate port
        if (!int.TryParse(txtServerPort.Text, out var port) || !NetworkHelper.IsValidPort(port))
        {
            MessageBox.Show("Invalid port number. Please enter a valid port between 1 and 65535.", 
                "Invalid Port", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        
        // Get IP address
        var selectedItem = cmbServerIP.SelectedItem?.ToString() ?? "";
        var parts = selectedItem.Split(new[] { " - " }, StringSplitOptions.None);
        var ipPart = parts[0].Trim();
        
        // Extract just the IP address (remove /prefix if present)
        var ipAddress = ipPart.Split('/')[0];
        
        // Get web server host from control panel
        var controlPanel = Application.OpenForms.OfType<ControlPanelForm>().FirstOrDefault();
        if (controlPanel == null)
        {
            MessageBox.Show("Cannot start server: Control Panel not found.", 
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        
        try
        {
            await controlPanel.StartWebServerAsync(ipAddress, port);
            UpdateServerStatusLabel();
            UpdateAudienceControlStates();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to start server: {ex.Message}", 
                "Server Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnStopServer_Click(object sender, EventArgs e)
    {
        var controlPanel = Application.OpenForms.OfType<ControlPanelForm>().FirstOrDefault();
        if (controlPanel == null)
        {
            MessageBox.Show("Cannot stop server: Control Panel not found.", 
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        
        try
        {
            await controlPanel.StopWebServerAsync();
            UpdateServerStatusLabel();
            UpdateAudienceControlStates();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to stop server: {ex.Message}", 
                "Server Stop Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #endregion

    private void btnOpenGameConsole_Click(object sender, EventArgs e)
    {
        GameConsole.Show();
    }

    private void btnOpenWebServerConsole_Click(object sender, EventArgs e)
    {
        WebServerConsole.Show();
    }

    // Change event handlers
    private void Control_Changed(object? sender, EventArgs e)
    {
        MarkChanged();
    }

    #region Audio Settings Event Handlers

    private void chkEnableSilenceDetection_CheckedChanged(object sender, EventArgs e)
    {
        var enabled = chkEnableSilenceDetection.Checked;
        trackBarSilenceThreshold.Enabled = enabled;
        numSilenceDuration.Enabled = enabled;
        numInitialDelay.Enabled = enabled;
        numFadeoutDuration.Enabled = enabled;
        MarkChanged();
    }

    private void trackBarSilenceThreshold_ValueChanged(object sender, EventArgs e)
    {
        lblSilenceThresholdValue.Text = $"{trackBarSilenceThreshold.Value} dB";
        MarkChanged();
    }

    private void chkEnableCrossfade_CheckedChanged(object sender, EventArgs e)
    {
        numCrossfadeDuration.Enabled = chkEnableCrossfade.Checked;
        MarkChanged();
    }

    private void trackBarGain_ValueChanged(object sender, EventArgs e)
    {
        if (sender == trackBarMasterGain)
        {
            var value = trackBarMasterGain.Value;
            var sign = value >= 0 ? "+" : "";
            lblMasterGainValue.Text = $"{sign}{value} dB";
        }
        else if (sender == trackBarEffectsGain)
        {
            var value = trackBarEffectsGain.Value;
            var sign = value >= 0 ? "+" : "";
            lblEffectsGainValue.Text = $"{sign}{value} dB";
        }
        else if (sender == trackBarMusicGain)
        {
            var value = trackBarMusicGain.Value;
            var sign = value >= 0 ? "+" : "";
            lblMusicGainValue.Text = $"{sign}{value} dB";
        }
        MarkChanged();
    }

    private void LoadAudioSettings()
    {
        // Silence Detection Settings
        chkEnableSilenceDetection.Checked = _settings.SilenceDetection.Enabled;
        trackBarSilenceThreshold.Value = (int)_settings.SilenceDetection.ThresholdDb;
        numSilenceDuration.Value = _settings.SilenceDetection.SilenceDurationMs;
        numInitialDelay.Value = _settings.SilenceDetection.InitialDelayMs;
        numFadeoutDuration.Value = _settings.SilenceDetection.FadeoutDurationMs;

        // Update threshold label
        lblSilenceThresholdValue.Text = $"{trackBarSilenceThreshold.Value} dB";

        // Enable/disable controls based on checkbox
        var silenceEnabled = chkEnableSilenceDetection.Checked;
        trackBarSilenceThreshold.Enabled = silenceEnabled;
        numSilenceDuration.Enabled = silenceEnabled;
        numInitialDelay.Enabled = silenceEnabled;
        numFadeoutDuration.Enabled = silenceEnabled;

        // Crossfade Settings
        chkEnableCrossfade.Checked = _settings.Crossfade.Enabled;
        numCrossfadeDuration.Value = _settings.Crossfade.CrossfadeDurationMs;

        // Enable/disable crossfade duration based on checkbox
        numCrossfadeDuration.Enabled = chkEnableCrossfade.Checked;

        // Audio Processing Settings
        trackBarMasterGain.Value = (int)_settings.AudioProcessing.MasterGainDb;
        trackBarEffectsGain.Value = (int)_settings.AudioProcessing.EffectsGainDb;
        trackBarMusicGain.Value = (int)_settings.AudioProcessing.MusicGainDb;
        chkEnableLimiter.Checked = _settings.AudioProcessing.EnableLimiter;

        // Update gain labels
        UpdateAllGainLabels();
    }

    private void SaveAudioSettings()
    {
        // Silence Detection Settings
        _settings.SilenceDetection.Enabled = chkEnableSilenceDetection.Checked;
        _settings.SilenceDetection.ThresholdDb = trackBarSilenceThreshold.Value;
        _settings.SilenceDetection.SilenceDurationMs = (int)numSilenceDuration.Value;
        _settings.SilenceDetection.InitialDelayMs = (int)numInitialDelay.Value;
        _settings.SilenceDetection.FadeoutDurationMs = (int)numFadeoutDuration.Value;

        // Crossfade Settings
        _settings.Crossfade.Enabled = chkEnableCrossfade.Checked;
        _settings.Crossfade.CrossfadeDurationMs = (int)numCrossfadeDuration.Value;

        // Audio Processing Settings
        _settings.AudioProcessing.MasterGainDb = (float)trackBarMasterGain.Value;
        _settings.AudioProcessing.EffectsGainDb = (float)trackBarEffectsGain.Value;
        _settings.AudioProcessing.MusicGainDb = (float)trackBarMusicGain.Value;
        _settings.AudioProcessing.EnableLimiter = chkEnableLimiter.Checked;

        // Apply audio settings to SoundService
        ApplyAudioSettings();
    }

    private void ApplyAudioSettings()
    {
        try
        {
            // Notify SoundService to reload settings
            var soundService = Program.ServiceProvider?.GetRequiredService<Services.SoundService>();
            if (soundService != null)
            {
                // SoundService will automatically pick up the new settings from ApplicationSettings.Instance
                // No explicit refresh needed as it already references the singleton
                if (Program.DebugMode)
                {
                    GameConsole.Debug("[OptionsDialog] Audio settings applied successfully");
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to apply audio settings: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateAllGainLabels()
    {
        var masterValue = trackBarMasterGain.Value;
        var masterSign = masterValue >= 0 ? "+" : "";
        lblMasterGainValue.Text = $"{masterSign}{masterValue} dB";

        var effectsValue = trackBarEffectsGain.Value;
        var effectsSign = effectsValue >= 0 ? "+" : "";
        lblEffectsGainValue.Text = $"{effectsSign}{effectsValue} dB";

        var musicValue = trackBarMusicGain.Value;
        var musicSign = musicValue >= 0 ? "+" : "";
        lblMusicGainValue.Text = $"{musicSign}{musicValue} dB";
    }

    #endregion

    // Broadcast Settings methods
    private void LoadBroadcastSettings()
    {
        // Populate background dropdown with thumbnails
        PopulateBackgroundDropdown();

        // Load background mode
        if (_settings.Broadcast.Mode == Core.Settings.BackgroundMode.Prerendered)
        {
            radModePrerendered.Checked = true;
        }
        else
        {
            radModeChromaKey.Checked = true;
        }

        // Load selected background
        SelectBackgroundInDropdown(_settings.Broadcast.SelectedBackgroundPath);

        // Load chroma key color
        var chromaColor = _settings.Broadcast.ChromaKeyColor;
        lblChromaColorPreview.BackColor = chromaColor;

        // Update control visibility based on mode
        UpdateBroadcastControlVisibility();
    }

    private void SaveBroadcastSettings()
    {
        // Save background mode
        _settings.Broadcast.Mode = radModePrerendered.Checked
            ? Core.Settings.BackgroundMode.Prerendered
            : Core.Settings.BackgroundMode.ChromaKey;

        // Save selected background
        if (cmbBackground.SelectedItem is BackgroundItem bgItem)
        {
            _settings.Broadcast.SelectedBackgroundPath = bgItem.Path;
            GameConsole.Debug($"[OptionsDialog] Saved background path: {bgItem.Path}");
        }

        // Save chroma key color (convert from Color to hex)
        _settings.Broadcast.ChromaKeyColor = lblChromaColorPreview.BackColor;
        
        GameConsole.Debug($"[OptionsDialog] Saved broadcast mode: {_settings.Broadcast.Mode}");
    }

    private void UpdateBroadcastControlVisibility()
    {
        bool isPrerendered = radModePrerendered.Checked;

        // Show/hide controls based on mode
        lblBackground.Visible = isPrerendered;
        cmbBackground.Visible = isPrerendered;
        btnSelectBackground.Visible = isPrerendered;

        lblChromaColor.Visible = !isPrerendered;
        btnChromaColor.Visible = !isPrerendered;
        lblChromaColorPreview.Visible = !isPrerendered;

        // Enable Select button only when Custom is selected
        if (isPrerendered && cmbBackground.SelectedItem is BackgroundItem bgItem)
        {
            btnSelectBackground.Enabled = bgItem.IsCustom;
        }
        else
        {
            btnSelectBackground.Enabled = false;
        }
    }

    private void radModePrerendered_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateBroadcastControlVisibility();
        MarkChanged();
    }

    private void radModeChromaKey_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateBroadcastControlVisibility();
        MarkChanged();
    }

    private void btnChromaColor_Click(object? sender, EventArgs e)
    {
        using var colorDialog = new ColorDialog();
        colorDialog.Color = lblChromaColorPreview.BackColor;
        colorDialog.FullOpen = true;

        if (colorDialog.ShowDialog() == DialogResult.OK)
        {
            var selectedColor = colorDialog.Color;

            // Check for color conflicts
            if (Core.Settings.BroadcastSettings.IsColorConflict(selectedColor, out string conflictingElement))
            {
                var result = MessageBox.Show(
                    $"Warning: The selected color is similar to {conflictingElement} used in the game UI.\n\n" +
                    $"This may cause parts of the game UI to be removed during chroma keying.\n\n" +
                    $"Recommended colors: Blue (#0000FF) or Magenta (#FF00FF)\n\n" +
                    $"Do you want to use this color anyway?",
                    "Color Conflict Warning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            lblChromaColorPreview.BackColor = selectedColor;
            MarkChanged();
        }
    }

    private void PopulateBackgroundDropdown()
    {
        cmbBackground.Items.Clear();

        // Add "Black" (None) option with black thumbnail
        cmbBackground.Items.Add(new BackgroundItem
        {
            DisplayName = "Black",
            Path = "",
            IsEmbedded = false,
            IsCustom = false
        });

        // Add embedded backgrounds (01_bkg.png through 06_bkg.png)
        for (int i = 1; i <= 6; i++)
        {
            string resourceName = $"0{i}_bkg.png";
            cmbBackground.Items.Add(new BackgroundItem
            {
                DisplayName = $"Background {i}",
                Path = $"embedded://{resourceName}",
                ResourceName = resourceName,
                IsEmbedded = true,
                IsCustom = false
            });
        }

        // Add "Custom" option with white box thumbnail
        cmbBackground.Items.Add(new BackgroundItem
        {
            DisplayName = "Custom",
            Path = "custom://",
            IsEmbedded = false,
            IsCustom = true
        });

        // Select first item (Black) by default
        if (cmbBackground.Items.Count > 0)
        {
            cmbBackground.SelectedIndex = 0;
        }
    }

    private void SelectBackgroundInDropdown(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            // Select "Black" option
            cmbBackground.SelectedIndex = 0;
            return;
        }

        // Find matching background item
        for (int i = 0; i < cmbBackground.Items.Count; i++)
        {
            if (cmbBackground.Items[i] is BackgroundItem bgItem)
            {
                if (bgItem.Path == path)
                {
                    cmbBackground.SelectedIndex = i;
                    return;
                }
            }
        }

        // If path not found and it's an absolute path, treat as custom
        if (Path.IsPathRooted(path))
        {
            // Find Custom item and set its path
            for (int i = 0; i < cmbBackground.Items.Count; i++)
            {
                if (cmbBackground.Items[i] is BackgroundItem bgItem && bgItem.IsCustom)
                {
                    bgItem.Path = path;
                    cmbBackground.SelectedIndex = i;
                    return;
                }
            }
        }

        // Default to Black if not found
        cmbBackground.SelectedIndex = 0;
    }

    private void cmbBackground_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        e.DrawBackground();

        if (cmbBackground.Items[e.Index] is BackgroundItem bgItem)
        {
            // Draw thumbnail (71x40 box on the left in 16:9 aspect ratio)
            var thumbnailRect = new Rectangle(e.Bounds.Left + 2, e.Bounds.Top + 2, 67, 36);
            
            if (bgItem.IsCustom)
            {
                // White box with "Custom" text
                using var whiteBrush = new SolidBrush(Color.White);
                e.Graphics.FillRectangle(whiteBrush, thumbnailRect);
                using var borderPen = new Pen(Color.Gray);
                e.Graphics.DrawRectangle(borderPen, thumbnailRect);
                
                using var font = new Font("Arial", 6, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.Black);
                var textFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString("Custom", font, textBrush, thumbnailRect, textFormat);
            }
            else if (string.IsNullOrWhiteSpace(bgItem.Path))
            {
                // Black box for "Black" option
                using var blackBrush = new SolidBrush(Color.Black);
                e.Graphics.FillRectangle(blackBrush, thumbnailRect);
                using var borderPen = new Pen(Color.Gray);
                e.Graphics.DrawRectangle(borderPen, thumbnailRect);
            }
            else if (bgItem.IsEmbedded)
            {
                // Load and draw embedded resource thumbnail
                try
                {
                    var image = LoadEmbeddedBackground(bgItem.ResourceName!);
                    if (image != null)
                    {
                        e.Graphics.DrawImage(image, thumbnailRect);
                    }
                    else
                    {
                        // Fallback: gray box
                        using var grayBrush = new SolidBrush(Color.Gray);
                        e.Graphics.FillRectangle(grayBrush, thumbnailRect);
                    }
                }
                catch
                {
                    // Fallback: gray box
                    using var grayBrush = new SolidBrush(Color.Gray);
                    e.Graphics.FillRectangle(grayBrush, thumbnailRect);
                }
            }
            else
            {
                // Custom file path - try to load thumbnail
                try
                {
                    if (File.Exists(bgItem.Path))
                    {
                        using var image = Image.FromFile(bgItem.Path);
                        e.Graphics.DrawImage(image, thumbnailRect);
                    }
                    else
                    {
                        // File not found - red box
                        using var redBrush = new SolidBrush(Color.DarkRed);
                        e.Graphics.FillRectangle(redBrush, thumbnailRect);
                    }
                }
                catch
                {
                    // Error loading - red box
                    using var redBrush = new SolidBrush(Color.DarkRed);
                    e.Graphics.FillRectangle(redBrush, thumbnailRect);
                }
            }

            // Draw display name
            var textRect = new Rectangle(e.Bounds.Left + 75, e.Bounds.Top, e.Bounds.Width - 75, e.Bounds.Height);
            using var brush = new SolidBrush(e.ForeColor);
            var format = new StringFormat { LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString(bgItem.DisplayName, e.Font!, brush, textRect, format);
        }

        e.DrawFocusRectangle();
    }

    private Image? LoadEmbeddedBackground(string resourceName)
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourcePath = $"MillionaireGame.lib.textures.{resourceName}";
            
            using var stream = assembly.GetManifestResourceStream(resourcePath);
            if (stream != null)
            {
                return Image.FromStream(stream);
            }
        }
        catch
        {
            // Silently fail
        }
        return null;
    }

    private void cmbBackground_SelectedIndexChanged(object? sender, EventArgs e)
    {
        UpdateBroadcastControlVisibility();
        MarkChanged();
    }

    private void btnSelectBackground_Click(object? sender, EventArgs e)
    {
        try
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*",
                Title = "Select Custom Background",
                RestoreDirectory = true
            };

            // Save current change state - DoEvents might trigger spurious change events
            bool originalHasChanges = _hasChanges;

            // Run on separate thread to avoid modal deadlock
            DialogResult result = DialogResult.Cancel;
            var thread = new System.Threading.Thread(() =>
            {
                result = openFileDialog.ShowDialog();
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

            if (result == DialogResult.OK)
            {
                // Update the Custom item with the selected path
                if (cmbBackground.SelectedItem is BackgroundItem bgItem && bgItem.IsCustom)
                {
                    bgItem.Path = openFileDialog.FileName;
                    bgItem.DisplayName = $"Custom: {Path.GetFileName(openFileDialog.FileName)}";
                    
                    // Refresh the dropdown to show the updated item
                    cmbBackground.Invalidate();
                    MarkChanged();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error selecting background: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Background item class for dropdown
    private class BackgroundItem
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string? ResourceName { get; set; }
        public bool IsEmbedded { get; set; }
        public bool IsCustom { get; set; }
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

    private void LoadAudioDevices()
    {
        try
        {
            var devices = Services.AudioDeviceManager.GetAudioOutputDevices();
            cmbAudioDevice.Items.Clear();
            
            foreach (var device in devices)
            {
                cmbAudioDevice.Items.Add(device);
            }
            
            // Select current device or default
            var currentDeviceId = _settings.AudioOutputDevice;
            if (string.IsNullOrWhiteSpace(currentDeviceId))
            {
                // Select system default
                cmbAudioDevice.SelectedIndex = 0;
            }
            else
            {
                // Find matching device
                for (int i = 0; i < cmbAudioDevice.Items.Count; i++)
                {
                    if (cmbAudioDevice.Items[i] is Services.AudioDeviceInfo device && 
                        device.DeviceId == currentDeviceId)
                    {
                        cmbAudioDevice.SelectedIndex = i;
                        break;
                    }
                }
                
                // If not found, select default
                if (cmbAudioDevice.SelectedIndex == -1 && cmbAudioDevice.Items.Count > 0)
                {
                    cmbAudioDevice.SelectedIndex = 0;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading audio devices: {ex.Message}", "Error", 
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
                MessageBox.Show("Sound service not available", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            var soundPackManager = soundService.GetSoundPackManager();
            if (soundPackManager == null)
            {
                MessageBox.Show("Sound pack manager not available", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    _soundPackDataTable.Clear();
                    MessageBox.Show($"No sounds found in pack '{selectedPack}'", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                _soundPackDataTable.Clear();
                MessageBox.Show($"Failed to load soundpack '{selectedPack}'", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            _soundPackDataTable.Clear();
            MessageBox.Show($"Error loading soundpack: {ex.Message}\n\n{ex.StackTrace}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateSoundPackInfo(IReadOnlyDictionary<string, string> sounds)
    {
        _soundPackDataTable.Clear();

        if (sounds == null || sounds.Count == 0)
        {
            return;
        }

        var soundService = Program.ServiceProvider?.GetRequiredService<Services.SoundService>();
        var soundPackManager = soundService?.GetSoundPackManager();
        var packDirectory = soundPackManager != null 
            ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib", "sounds", soundPackManager.CurrentPackName)
            : string.Empty;

        foreach (var sound in sounds.OrderBy(s => s.Key))
        {
            var key = sound.Key;
            var relativePath = sound.Value;
            var fileName = Path.GetFileName(relativePath);
            var fullPath = !string.IsNullOrWhiteSpace(packDirectory) && !string.IsNullOrWhiteSpace(relativePath)
                ? Path.Combine(packDirectory, relativePath)
                : string.Empty;

            // Determine status
            string status;
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                status = "❌ Empty";
            }
            else if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
            {
                status = "⚠️ Missing";
            }
            else
            {
                status = "✅ OK";
            }

            // Categorize sound
            string category = CategorizeSound(key);

            _soundPackDataTable.Rows.Add(key, fileName, status, category, fullPath);
        }
    }

    private string CategorizeSound(string key)
    {
        if (key.Contains("Host") || key.Contains("Opening") || key.Contains("Explain") ||
            key.Contains("Quit") || key.Contains("Walk") || key.Contains("Game") ||
            key.Contains("Close") || key.Contains("Commercial") || key.Contains("Risk") ||
            key.Contains("Random") || key.Contains("Safety") || key.Contains("ToHotSeat"))
            return "General/Broadcast";
        else if (key.Contains("FFF") || key.Contains("Fastest"))
            return "Fastest Finger First";
        else if (key.Contains("5050") || key.Contains("PAF") || key.Contains("ATA") ||
                 key.Contains("Switch") || key.Contains("Lifeline") || key.Contains("Double") ||
                 key.Contains("ATH"))
            return "Lifelines";
        else if (key.Contains("LightsDown"))
            return "Lights Down";
        else if (key.Contains("Bed"))
            return "Bed Music";
        else if (key.Contains("Final"))
            return "Final Answer";
        else if (key.Contains("Correct"))
            return "Correct Answer";
        else if (key.Contains("Wrong"))
            return "Wrong Answer";
        else
            return "Other";
    }

    private void InitializeSoundPackDataGrid()
    {
        // Setup DataTable columns
        _soundPackDataTable.Columns.Add("Key", typeof(string));
        _soundPackDataTable.Columns.Add("FileName", typeof(string));
        _soundPackDataTable.Columns.Add("Status", typeof(string));
        _soundPackDataTable.Columns.Add("Category", typeof(string));
        _soundPackDataTable.Columns.Add("FullPath", typeof(string));

        // Bind to DataGridView (DataView already initialized in constructor)
        dgvSoundPackInfo.AutoGenerateColumns = false;
        dgvSoundPackInfo.DataSource = _soundPackDataView;

        // Manually create columns
        dgvSoundPackInfo.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Key",
            HeaderText = "Sound Key",
            DataPropertyName = "Key",
            Width = 180
        });

        dgvSoundPackInfo.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "FileName",
            HeaderText = "File Name",
            DataPropertyName = "FileName",
            Width = 200
        });

        dgvSoundPackInfo.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Status",
            HeaderText = "Status",
            DataPropertyName = "Status",
            Width = 80
        });

        dgvSoundPackInfo.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Category",
            HeaderText = "Category",
            DataPropertyName = "Category",
            Width = 150
        });

        dgvSoundPackInfo.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "FullPath",
            HeaderText = "FullPath",
            DataPropertyName = "FullPath",
            Visible = false
        });

        // Style status column with colors
        dgvSoundPackInfo.CellFormatting += (s, e) =>
        {
            if (e.ColumnIndex == dgvSoundPackInfo.Columns["Status"].Index && e.Value != null)
            {
                var status = e.Value.ToString();
                if (status?.StartsWith("✅") == true)
                {
                    e.CellStyle!.ForeColor = Color.Green;
                    e.CellStyle!.Font = new Font(dgvSoundPackInfo.Font, FontStyle.Bold);
                }
                else if (status?.StartsWith("⚠️") == true)
                {
                    e.CellStyle!.ForeColor = Color.Orange;
                    e.CellStyle!.Font = new Font(dgvSoundPackInfo.Font, FontStyle.Bold);
                }
                else if (status?.StartsWith("❌") == true)
                {
                    e.CellStyle!.ForeColor = Color.Red;
                    e.CellStyle!.Font = new Font(dgvSoundPackInfo.Font, FontStyle.Bold);
                }
            }
        };
    }

    private void txtSearchSounds_TextChanged(object? sender, EventArgs e)
    {
        var searchText = txtSearchSounds.Text;
        if (string.IsNullOrWhiteSpace(searchText))
        {
            _soundPackDataView.RowFilter = string.Empty;
        }
        else
        {
            // Filter by key or filename
            _soundPackDataView.RowFilter = $"Key LIKE '%{searchText.Replace("'", "''")}%' OR FileName LIKE '%{searchText.Replace("'", "''")}%'";
        }
    }

    private void btnPlaySelected_Click(object? sender, EventArgs e)
    {
        try
        {
            if (dgvSoundPackInfo.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a sound to play.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRow = dgvSoundPackInfo.SelectedRows[0];
            var key = selectedRow.Cells["Key"].Value?.ToString();
            var status = selectedRow.Cells["Status"].Value?.ToString();
            var fullPath = selectedRow.Cells["FullPath"].Value?.ToString();

            if (string.IsNullOrWhiteSpace(key))
            {
                MessageBox.Show("Invalid sound key.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (status?.StartsWith("❌") == true || status?.StartsWith("⚠️") == true)
            {
                MessageBox.Show($"Cannot play this sound - file is missing or invalid.\n\nPath: {fullPath}",
                    "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var soundService = Program.ServiceProvider?.GetRequiredService<Services.SoundService>();
            if (soundService == null)
            {
                MessageBox.Show("Sound service not available.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Play the sound using the soundpack key
            if (Program.DebugMode)
            {
                GameConsole.Debug($"[OptionsDialog] Play button clicked for key: {key}");
                GameConsole.Debug($"[OptionsDialog] File path: {fullPath}");
            }
            
            var resultId = soundService.PlaySoundByKey(key, loop: false);
            
            if (Program.DebugMode)
            {
                GameConsole.Debug($"[OptionsDialog] PlaySoundByKey returned: {(string.IsNullOrEmpty(resultId) ? "EMPTY" : resultId)}");
            }

            if (Program.DebugMode)
            {
                GameConsole.Info($"[OptionsDialog] Testing sound: {key}");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error playing sound: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnImportPack_Click(object sender, EventArgs e)
    {
        GameConsole.Debug("[OptionsDialog] === IMPORT BUTTON CLICKED ===");
        GameConsole.Debug($"[OptionsDialog] Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
        GameConsole.Debug($"[OptionsDialog] IsHandleCreated: {IsHandleCreated}");
        GameConsole.Debug($"[OptionsDialog] InvokeRequired: {InvokeRequired}");
        
        try
        {
            GameConsole.Debug("[OptionsDialog] Step 1: Entering try block");
            GameConsole.Debug("[OptionsDialog] Step 2: About to create OpenFileDialog");
            
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
                bool success = soundPackManager.RemoveSoundPack(selectedPack);
                if (success)
                {
                    LoadAvailableSoundPacks();
                    cmbSoundPack.SelectedItem = "Default";
                    MessageBox.Show($"Sound pack '{selectedPack}' has been removed successfully.", 
                        "Removal Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Failed to remove sound pack '{selectedPack}'.", 
                        "Removal Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
        // Create currency groups
        var grpCurrency1 = new GroupBox
        {
            Text = "Currency 1",
            Location = new Point(356, 16),
            Size = new Size(280, 200)
        };
        
        var grpCurrency2 = new GroupBox
        {
            Text = "Currency 2",
            Location = new Point(356, 226),
            Size = new Size(280, 200)
        };
        
        // Add header row labels directly to tab (not in group)
        var lblPrizeHeader = new Label
        {
            Text = "Prize",
            Location = new Point(66, 20),
            Size = new Size(120, 15),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        
        var lblCurrencyHeader = new Label
        {
            Text = "Currency",
            Location = new Point(265, 20), // Centered over currency column
            Size = new Size(60, 15),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };
        
        // Initialize 15 prize value inputs (DESCENDING - Q15 to Q1)
        int yPos = 45; // Starting Y position directly on tab
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
#pragma warning disable CS8622 // Nullability of reference types in parameter doesn't match delegate
            numValue.ValueChanged += Control_Changed;
#pragma warning restore CS8622
            
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
#pragma warning disable CS8622 // Nullability of reference types in parameter doesn't match delegate
                chkSafetyNet.CheckedChanged += Control_Changed;
#pragma warning restore CS8622
                tabMoneyTree.Controls.Add(chkSafetyNet);
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
#pragma warning disable CS8622 // Nullability of reference types in parameter doesn't match delegate
            cmbCurrency.SelectedIndexChanged += Control_Changed;
#pragma warning restore CS8622
            
            tabMoneyTree.Controls.Add(lblQuestion);
            tabMoneyTree.Controls.Add(numValue);
            tabMoneyTree.Controls.Add(cmbCurrency);
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
#pragma warning disable CS8622 // Nullability of reference types in parameter doesn't match delegate
        txtCustomCurrency1.TextChanged += Control_Changed;
#pragma warning restore CS8622
        
        var chkSuffix1 = new CheckBox
        {
            Name = "chkCurrency1AtSuffix",
            Text = "Show after value",
            Location = new Point(15, 155),
            Size = new Size(170, 20)
        };
#pragma warning disable CS8622 // Nullability of reference types in parameter doesn't match delegate
        chkSuffix1.CheckedChanged += Control_Changed;
#pragma warning restore CS8622
        
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
#pragma warning disable CS8622 // Nullability of reference types in parameter doesn't match delegate
        txtCustomCurrency2.TextChanged += Control_Changed;
#pragma warning restore CS8622
        
        var chkSuffix2 = new CheckBox
        {
            Name = "chkCurrency2AtSuffix",
            Text = "Show after value",
            Location = new Point(15, 175),
            Size = new Size(170, 20),
            Enabled = false
        };
#pragma warning disable CS8622 // Nullability of reference types in parameter doesn't match delegate
        chkSuffix2.CheckedChanged += Control_Changed;
#pragma warning restore CS8622
        
        grpCurrency2.Controls.AddRange(new Control[] { chkEnableCurrency2, radDollar2, radEuro2, radPound2, radYen2, radOther2, txtCustomCurrency2, chkSuffix2 });
        
        // Add header labels and currency groups to tab (note: prize controls already added individually in loop above)
        tabMoneyTree.Controls.AddRange(new Control[] { lblPrizeHeader, lblCurrencyHeader, grpCurrency1, grpCurrency2 });
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

    #region Monitor Management

    private void PopulateMonitorDropdowns()
    {
        var screens = Screen.AllScreens;
        
        // Clear existing items
        cmbMonitorHost.Items.Clear();
        cmbMonitorGuest.Items.Clear();
        cmbMonitorTV.Items.Clear();
        
        // Add each monitor to the dropdowns
        for (int i = 0; i < screens.Length; i++)
        {
            // Skip Display 1 unless in DEBUG mode
#if DEBUG
            bool includeThisDisplay = true;
#else
            bool includeThisDisplay = i > 0; // Skip index 0 (Display 1) in release mode
#endif
            
            if (includeThisDisplay)
            {
                var screen = screens[i];
                var (manufacturer, modelName) = GetMonitorModelName(screen);
                string displayText = $"{i + 1}:{manufacturer}:{modelName} ({screen.Bounds.Width}x{screen.Bounds.Height})";
                
                cmbMonitorHost.Items.Add(displayText);
                cmbMonitorGuest.Items.Add(displayText);
                cmbMonitorTV.Items.Add(displayText);
            }
        }
        
        // Select first item by default if available
        if (cmbMonitorHost.Items.Count > 0)
        {
            cmbMonitorHost.SelectedIndex = 0;
            cmbMonitorGuest.SelectedIndex = 0;
            cmbMonitorTV.SelectedIndex = 0;
        }
    }
    
    private void RefreshMonitorDropdowns()
    {
        var screens = Screen.AllScreens;
        
        // Get currently selected monitor indices before refresh
        int hostSelectedIndex = GetSelectedMonitorIndex(cmbMonitorHost);
        int guestSelectedIndex = GetSelectedMonitorIndex(cmbMonitorGuest);
        int tvSelectedIndex = GetSelectedMonitorIndex(cmbMonitorTV);
        
        // Clear and repopulate each dropdown
        RefreshSingleDropdown(cmbMonitorHost, screens, guestSelectedIndex, tvSelectedIndex, chkFullScreenHostScreen.Checked);
        RefreshSingleDropdown(cmbMonitorGuest, screens, hostSelectedIndex, tvSelectedIndex, chkFullScreenGuestScreen.Checked);
        RefreshSingleDropdown(cmbMonitorTV, screens, hostSelectedIndex, guestSelectedIndex, chkFullScreenTVScreen.Checked);
        
        // Restore selections
        SelectMonitorIndex(cmbMonitorHost, hostSelectedIndex);
        SelectMonitorIndex(cmbMonitorGuest, guestSelectedIndex);
        SelectMonitorIndex(cmbMonitorTV, tvSelectedIndex);
    }
    
    private void RefreshSingleDropdown(ComboBox dropdown, Screen[] screens, int excludeIndex1, int excludeIndex2, bool isEnabled)
    {
        if (!isEnabled)
        {
            // If checkbox is not checked, don't populate dropdown
            dropdown.Items.Clear();
            return;
        }
        
        dropdown.Items.Clear();
        
        for (int i = 0; i < screens.Length; i++)
        {
            // Skip Display 1 unless in DEBUG mode
#if DEBUG
            bool includeThisDisplay = true;
#else
            bool includeThisDisplay = i > 0; // Skip index 0 (Display 1) in release mode
#endif
            
            // Also skip monitors that are already selected by other screens
            if (includeThisDisplay && i != excludeIndex1 && i != excludeIndex2)
            {
                var screen = screens[i];
                var (manufacturer, modelName) = GetMonitorModelName(screen);
                string displayText = $"{i + 1}:{manufacturer}:{modelName} ({screen.Bounds.Width}x{screen.Bounds.Height})";
                
                dropdown.Items.Add(displayText);
            }
        }
        
        // Select first item if available and nothing is currently selected
        if (dropdown.Items.Count > 0 && dropdown.SelectedIndex == -1)
        {
            dropdown.SelectedIndex = 0;
        }
    }

    private void UpdateMonitorStatus()
    {
        var monitorCount = Screen.AllScreens.Length;
        
        // Update label based on monitor count
        if (monitorCount >= 4)
        {
            lblMonitorCount.Text = $"Number of Monitors: {monitorCount} (All screens can be enabled)";
        }
        else if (monitorCount == 3)
        {
            lblMonitorCount.Text = $"Number of Monitors: {monitorCount} (Up to 2 screens can be enabled)";
        }
        else if (monitorCount == 2)
        {
            lblMonitorCount.Text = $"Number of Monitors: {monitorCount} (Only 1 screen can be enabled)";
        }
        else
        {
            lblMonitorCount.Text = $"Number of Monitors: {monitorCount} (At least 2 monitors are required for this feature)";
        }
        
        // In DEBUG mode, always enable controls regardless of monitor count
        // In RELEASE mode, require 2+ monitors
#if DEBUG
        bool hasEnoughMonitors = true;
#else
        bool hasEnoughMonitors = monitorCount >= 2;
#endif
        
        // Enable/disable controls based on monitor count
        chkFullScreenHostScreen.Enabled = hasEnoughMonitors;
        chkFullScreenGuestScreen.Enabled = hasEnoughMonitors;
        chkFullScreenTVScreen.Enabled = hasEnoughMonitors;
        cmbMonitorHost.Enabled = hasEnoughMonitors && chkFullScreenHostScreen.Checked;
        cmbMonitorGuest.Enabled = hasEnoughMonitors && chkFullScreenGuestScreen.Checked;
        cmbMonitorTV.Enabled = hasEnoughMonitors && chkFullScreenTVScreen.Checked;
        btnIdentifyMonitors.Enabled = hasEnoughMonitors;
        
        if (!hasEnoughMonitors)
        {
            chkFullScreenHostScreen.Checked = false;
            chkFullScreenGuestScreen.Checked = false;
            chkFullScreenTVScreen.Checked = false;
        }
        
        // Update checkbox enabled states based on how many are already checked
        UpdateCheckboxEnabledStates();
    }
    
    private void UpdateCheckboxEnabledStates()
    {
        var monitorCount = Screen.AllScreens.Length;
        
        // Count how many checkboxes are currently checked
        int checkedCount = 0;
        if (chkFullScreenHostScreen.Checked) checkedCount++;
        if (chkFullScreenGuestScreen.Checked) checkedCount++;
        if (chkFullScreenTVScreen.Checked) checkedCount++;
        
        // Determine how many screens can be enabled based on monitor count
        int maxScreens = monitorCount >= 4 ? 3 : monitorCount - 1;
        
        // If we've reached the maximum, disable unchecked checkboxes
        bool canCheckMore = checkedCount < maxScreens;
        
        if (!chkFullScreenHostScreen.Checked)
            chkFullScreenHostScreen.Enabled = canCheckMore;
        if (!chkFullScreenGuestScreen.Checked)
            chkFullScreenGuestScreen.Enabled = canCheckMore;
        if (!chkFullScreenTVScreen.Checked)
            chkFullScreenTVScreen.Enabled = canCheckMore;
    }

    private (string manufacturer, string modelName) GetMonitorModelName(Screen screen)
    {
        try
        {
            // Try to get monitor friendly name from WMI
            var scope = new System.Management.ManagementScope("\\\\.\\root\\wmi");
            var query = new System.Management.ObjectQuery("SELECT * FROM WmiMonitorID");
            using (var searcher = new System.Management.ManagementObjectSearcher(scope, query))
            {
                var monitors = searcher.Get().Cast<System.Management.ManagementObject>().ToList();
                
                if (monitors.Count > 0)
                {
                    // Get the device index from the screen device name
                    var deviceName = screen.DeviceName.Replace("\\\\.\\DISPLAY", "");
                    if (int.TryParse(deviceName, out int displayIndex) && displayIndex > 0 && displayIndex <= monitors.Count)
                    {
                        var monitor = monitors[displayIndex - 1];
                        
                        string manufacturer = "";
                        string modelName = "";
                        
                        // Get ManufacturerName
                        var manufacturerNameData = monitor["ManufacturerName"] as ushort[];
                        if (manufacturerNameData != null && manufacturerNameData.Length > 0)
                        {
                            manufacturer = new string(manufacturerNameData.Where(c => c != 0).Select(c => (char)c).ToArray()).Trim();
                        }
                        
                        // Get UserFriendlyName (model)
                        var userFriendlyName = monitor["UserFriendlyName"] as ushort[];
                        if (userFriendlyName != null && userFriendlyName.Length > 0)
                        {
                            modelName = new string(userFriendlyName.Where(c => c != 0).Select(c => (char)c).ToArray()).Trim();
                        }
                        
                        // If we have at least one value, return it
                        if (!string.IsNullOrEmpty(manufacturer) || !string.IsNullOrEmpty(modelName))
                        {
                            return (manufacturer, modelName);
                        }
                    }
                }
            }
        }
        catch
        {
            // If WMI fails, fall back to device name
        }
        
        // Default fallback: use device name
        string fallbackName = screen.Primary ? "Primary Monitor" : screen.DeviceName.Replace("\\\\.\\DISPLAY", "Display ");
        return ("", fallbackName);
    }

    private void SelectMonitorIndex(ComboBox comboBox, int monitorIndex)
    {
        if (monitorIndex >= 0 && monitorIndex < comboBox.Items.Count)
        {
            comboBox.SelectedIndex = monitorIndex;
        }
        else if (comboBox.Items.Count > 0)
        {
            comboBox.SelectedIndex = 0;
        }
    }

    private int GetSelectedMonitorIndex(ComboBox comboBox)
    {
        return comboBox.SelectedIndex >= 0 ? comboBox.SelectedIndex : 0;
    }

    private void btnIdentifyMonitors_Click(object? sender, EventArgs e)
    {
        var screens = Screen.AllScreens;
        var identifyForms = new List<Form>();
        
        try
        {
            // Create identification form for each screen
            for (int i = 0; i < screens.Length; i++)
            {
                var screen = screens[i];
                var form = new Form
                {
                    FormBorderStyle = FormBorderStyle.None,
                    BackColor = Color.Black,
                    Opacity = 0.8,
                    StartPosition = FormStartPosition.Manual,
                    Location = screen.Bounds.Location,
                    Size = screen.Bounds.Size,
                    TopMost = true,
                    ShowInTaskbar = false
                };
                
                var label = new Label
                {
                    Text = $"{i + 1}",
                    Font = new Font("Segoe UI", 200, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                
                var infoLabel = new Label
                {
                    Text = $"{screen.DeviceName}\n{screen.Bounds.Width} x {screen.Bounds.Height}" +
                           (screen.Primary ? "\n[Primary Monitor]" : ""),
                    Font = new Font("Segoe UI", 20, FontStyle.Regular),
                    ForeColor = Color.White,
                    AutoSize = false,
                    Dock = DockStyle.Bottom,
                    Height = 150,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                
                form.Controls.Add(infoLabel);
                form.Controls.Add(label);
                
                // Close on click
                form.Click += (s, args) => { foreach (var f in identifyForms) f.Close(); };
                label.Click += (s, args) => { foreach (var f in identifyForms) f.Close(); };
                infoLabel.Click += (s, args) => { foreach (var f in identifyForms) f.Close(); };
                
                identifyForms.Add(form);
                form.Show();
            }
            
            // Auto-close after 5 seconds
            var timer = new System.Windows.Forms.Timer { Interval = 5000 };
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                timer.Dispose();
                foreach (var form in identifyForms)
                {
                    form.Close();
                }
            };
            timer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error identifying monitors: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            
            // Cleanup any opened forms
            foreach (var form in identifyForms)
            {
                form.Close();
            }
        }
    }

    #endregion
}
