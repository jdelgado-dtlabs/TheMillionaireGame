using MillionaireGame.Core.Settings;

namespace MillionaireGame.Forms.Options;

public partial class DatabaseSettingsDialog : Form
{
    private readonly SqlConnectionSettings _sqlSettings;
    private readonly SqlSettingsManager _settingsManager;

    public DatabaseSettingsDialog(SqlConnectionSettings sqlSettings)
    {
        _sqlSettings = sqlSettings ?? throw new ArgumentNullException(nameof(sqlSettings));
        _settingsManager = new SqlSettingsManager();
        InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        // Local or Remote server
        radLocal.Checked = !_sqlSettings.UseRemoteServer;
        radRemote.Checked = _sqlSettings.UseRemoteServer;
        
        // LocalDB option
        chkUseLocalDB.Checked = _sqlSettings.UseLocalDB;
        
        // Hide at start option
        chkHideAtStart.Checked = _sqlSettings.HideAtStart;
        
        // Remote server settings
        txtServerInstance.Text = _sqlSettings.RemoteServer;
        numPort.Value = _sqlSettings.RemotePort;
        txtDatabase.Text = _sqlSettings.RemoteDatabase;
        txtUsername.Text = _sqlSettings.RemoteLogin;
        txtPassword.Text = _sqlSettings.RemotePassword;
        
        // Enable/disable remote group based on selection
        grpRemote.Enabled = radRemote.Checked;
    }

    private void SaveSettings()
    {
        _sqlSettings.UseRemoteServer = radRemote.Checked;
        _sqlSettings.UseLocalDB = chkUseLocalDB.Checked;
        _sqlSettings.HideAtStart = chkHideAtStart.Checked;
        
        _sqlSettings.RemoteServer = txtServerInstance.Text;
        _sqlSettings.RemotePort = (int)numPort.Value;
        _sqlSettings.RemoteDatabase = txtDatabase.Text;
        _sqlSettings.RemoteLogin = txtUsername.Text;
        _sqlSettings.RemotePassword = txtPassword.Text;
        
        _settingsManager.SaveSettings();
    }

    private void radRemote_CheckedChanged(object sender, EventArgs e)
    {
        grpRemote.Enabled = radRemote.Checked;
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        SaveSettings();
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private async void btnTest_Click(object sender, EventArgs e)
    {
        btnTest.Enabled = false;
        btnTest.Text = "Testing...";
        
        try
        {
            // Create temporary settings for testing
            var testSettings = new SqlConnectionSettings
            {
                UseRemoteServer = radRemote.Checked,
                UseLocalDB = chkUseLocalDB.Checked,
                RemoteServer = txtServerInstance.Text,
                RemotePort = (int)numPort.Value,
                RemoteDatabase = txtDatabase.Text,
                RemoteLogin = txtUsername.Text,
                RemotePassword = txtPassword.Text
            };
            
            var connectionString = testSettings.GetConnectionString();
            
            // Try to open connection
            await Task.Run(() =>
            {
                using var connection = new System.Data.SqlClient.SqlConnection(connectionString);
                connection.Open();
            });
            
            MessageBox.Show(
                "Connection successful!",
                "Database Test",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Connection failed:\n{ex.Message}",
                "Database Test",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            btnTest.Enabled = true;
            btnTest.Text = "Test Connection";
        }
    }
}
