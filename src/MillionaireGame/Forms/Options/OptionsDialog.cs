using MillionaireGame.Core.Settings;
using MillionaireGame.Core.Helpers;

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
        LoadSettings();
    }

    private void LoadSettings()
    {
        // General settings
        chkAutoShowHostScreen.Checked = _settings.AutoShowHostScreen;
        chkAutoShowGuestScreen.Checked = _settings.AutoShowGuestScreen;
        chkAutoShowTVScreen.Checked = _settings.AutoShowTVScreen;
        chkFullScreenHostScreen.Checked = _settings.FullScreenHostScreenEnable;
        chkFullScreenGuestScreen.Checked = _settings.FullScreenGuestScreenEnable;
        chkFullScreenTVScreen.Checked = _settings.FullScreenTVScreenEnable;
        chkShowAnswerOnlyAtFinal.Checked = _settings.ShowAnswerOnlyOnHostScreenAtFinal;
        chkAutoShowTotalWinnings.Checked = _settings.AutoShowTotalWinnings;
        chkAutoHideQuestionAtWalkAway.Checked = _settings.AutoHideQuestionAtPlusOne;
        chkHideAnswersOnNewQuestion.Checked = _settings.HideAnswerInControlPanelAtNewQ;

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

        // Sound settings (main game sounds)
        txtSoundQuestionCue.Text = _settings.SoundQ1to5Bed ?? string.Empty;
        txtSoundFinalAnswer.Text = _settings.SoundQ1Final ?? string.Empty;
        txtSoundWrongAnswer.Text = _settings.SoundQ1to5Wrong ?? string.Empty;
        txtSoundCorrectAnswer.Text = _settings.SoundQ1to4Correct ?? string.Empty;
        txtSoundWalkAway.Text = _settings.SoundWalkAway1 ?? string.Empty;

        // Lifeline sounds
        txtSound5050.Text = _settings.Sound5050 ?? string.Empty;
        txtSoundPhone.Text = _settings.SoundPlusOneStart ?? string.Empty;
        txtSoundATA.Text = _settings.SoundATAStart ?? string.Empty;
        txtSoundSwitch.Text = _settings.SoundSwitchActivate ?? string.Empty;

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
        // General settings
        _settings.AutoShowHostScreen = chkAutoShowHostScreen.Checked;
        _settings.AutoShowGuestScreen = chkAutoShowGuestScreen.Checked;
        _settings.AutoShowTVScreen = chkAutoShowTVScreen.Checked;
        _settings.FullScreenHostScreenEnable = chkFullScreenHostScreen.Checked;
        _settings.FullScreenGuestScreenEnable = chkFullScreenGuestScreen.Checked;
        _settings.FullScreenTVScreenEnable = chkFullScreenTVScreen.Checked;
        _settings.ShowAnswerOnlyOnHostScreenAtFinal = chkShowAnswerOnlyAtFinal.Checked;
        _settings.AutoShowTotalWinnings = chkAutoShowTotalWinnings.Checked;
        _settings.AutoHideQuestionAtPlusOne = chkAutoHideQuestionAtWalkAway.Checked;
        _settings.HideAnswerInControlPanelAtNewQ = chkHideAnswersOnNewQuestion.Checked;

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
        _settings.SoundQ1to5Bed = txtSoundQuestionCue.Text;
        _settings.SoundQ1Final = txtSoundFinalAnswer.Text;
        _settings.SoundQ1to5Wrong = txtSoundWrongAnswer.Text;
        _settings.SoundQ1to4Correct = txtSoundCorrectAnswer.Text;
        _settings.SoundWalkAway1 = txtSoundWalkAway.Text;

        // Sound settings (lifeline sounds)
        _settings.Sound5050 = txtSound5050.Text;
        _settings.SoundPlusOneStart = txtSoundPhone.Text;
        _settings.SoundATAStart = txtSoundATA.Text;
        _settings.SoundSwitchActivate = txtSoundSwitch.Text;

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

    private void btnCancel_Click(object sender, EventArgs e)
    {
        if (_hasChanges)
        {
            var result = MessageBox.Show(
                "You have unsaved changes. Do you want to discard them?",
                "Unsaved Changes",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                return;
            }
        }

        DialogResult = DialogResult.Cancel;
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
}
