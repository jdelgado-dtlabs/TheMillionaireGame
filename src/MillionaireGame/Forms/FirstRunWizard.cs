using System.Data;
using Microsoft.Data.SqlClient;
using MillionaireGame.Core.Database;
using MillionaireGame.Core.Helpers;
using MillionaireGame.Core.Settings;
using MillionaireGame.Utilities;

namespace MillionaireGame.Forms;

/// <summary>
/// First-run wizard for setting up database connection
/// Appears when sql.xml doesn't exist, guides users through initial database setup
/// </summary>
public partial class FirstRunWizard : Form
{
    private readonly SqlSettingsManager _settingsManager;
    private bool _isTestingConnection;
    private bool _connectionTestPassed;
    private bool _databaseExists;
    private readonly string? _preselectedDbType;

    public FirstRunWizard(string? preselectedDbType = null)
    {
        InitializeComponent();
        IconHelper.ApplyToForm(this);
        _settingsManager = new SqlSettingsManager();
        _preselectedDbType = preselectedDbType;
        
        // Set initial state based on preselected type or default to LocalDB
        if (preselectedDbType?.ToLowerInvariant() == "sqlexpress")
        {
            radSqlServer.Checked = true; // SQL Server Express was installed
            GameConsole.Info("[FirstRunWizard] Preselected SQL Server Express from installer");
        }
        else if (preselectedDbType?.ToLowerInvariant() == "remote")
        {
            radSqlServer.Checked = true; // Remote SQL Server selected
            GameConsole.Info("[FirstRunWizard] Preselected remote SQL Server from installer");
        }
        else
        {
            radLocalDB.Checked = true; // LocalDB is default
            GameConsole.Info("[FirstRunWizard] Defaulting to LocalDB");
        }
        
        btnFinish.Enabled = false;
        chkLoadSampleData.Enabled = false;
        grpConnectionDetails.Visible = false;
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        
        try
        {
            // Start enumerating SQL instances (may take 10-15 seconds)
            await EnumerateSqlInstancesAsync();
        }
        catch (Exception ex)
        {
            GameConsole.Error($"Error in FirstRunWizard.OnLoad: {ex.Message}");
            // Ensure UI is still functional even if enumeration fails
            if (cmbServerInstance.Items.Count == 0)
            {
                cmbServerInstance.Items.Add("<Browse for more...>");
                cmbServerInstance.SelectedIndex = 0;
            }
            cmbServerInstance.Enabled = true;
        }
    }

    #region SQL Server Instance Enumeration

    /// <summary>
    /// Enumerates SQL Server instances on local machine (SSMS-style)
    /// Populates dropdown with common instances + "Browse for more..." option
    /// NOTE: SqlDataSourceEnumerator is unreliable and can crash, so we use common instances
    /// </summary>
    private async Task EnumerateSqlInstancesAsync()
    {
        lblSqlServerStatus.Text = "Loading SQL Server instances...";
        lblSqlServerStatus.ForeColor = Color.Blue;
        cmbServerInstance.Enabled = false;
        
        await Task.Run(() =>
        {
            // SqlDataSourceEnumerator.Instance.GetDataSources() is problematic:
            // - Can take 15+ seconds to complete
            // - Often throws unhandled exceptions
            // - Unreliable on Windows 11 and with LocalDB installed
            // Instead, provide common instance names that users can select or browse
            
            this.Invoke((MethodInvoker)delegate
            {
                try
                {
                    cmbServerInstance.Items.Clear();
                    
                    // Add common SQL Server instance names
                    cmbServerInstance.Items.Add(".\\SQLEXPRESS");
                    cmbServerInstance.Items.Add("localhost\\SQLEXPRESS");
                    cmbServerInstance.Items.Add($"{Environment.MachineName}\\SQLEXPRESS");
                    cmbServerInstance.Items.Add(".\\MSSQLSERVER");
                    cmbServerInstance.Items.Add("localhost\\MSSQLSERVER");
                    cmbServerInstance.Items.Add("<Browse for more...>");
                    
                    lblSqlServerStatus.Text = "Select an instance or use Browse";
                    lblSqlServerStatus.ForeColor = Color.Green;
                    cmbServerInstance.SelectedIndex = 0;
                    cmbServerInstance.Enabled = true;
                    
                    GameConsole.Info("Populated common SQL Server instances");
                }
                catch (Exception ex)
                {
                    lblSqlServerStatus.Text = "Error loading instances";
                    lblSqlServerStatus.ForeColor = Color.Red;
                    cmbServerInstance.Items.Clear();
                    cmbServerInstance.Items.Add("<Browse for more...>");
                    cmbServerInstance.SelectedIndex = 0;
                    cmbServerInstance.Enabled = true;
                    GameConsole.Error($"Failed to populate instances: {ex.Message}");
                }
            });
        });
    }

    #endregion

    #region Connection Testing

    /// <summary>
    /// Tests the database connection based on selected options
    /// Checks if database exists, enables Finish button on success
    /// </summary>
    private async Task<bool> TestConnectionAsync()
    {
        if (_isTestingConnection)
            return false;

        _isTestingConnection = true;
        btnTest.Enabled = false;
        btnTest.Text = "Testing...";
        lblConnectionStatus.Text = "Testing connection...";
        lblConnectionStatus.ForeColor = Color.Blue;
        
        try
        {
            // Build connection settings from UI
            var testSettings = new SqlConnectionSettings();
            
            if (radLocalDB.Checked)
            {
                testSettings.UseLocalDB = true;
                testSettings.UseRemoteServer = false;
            }
            else if (radSqlServer.Checked)
            {
                testSettings.UseLocalDB = false;
                testSettings.UseRemoteServer = false;
                
                // Check if user selected "Browse for more..."
                if (cmbServerInstance.SelectedItem?.ToString() == "<Browse for more...>")
                {
                    // Use manual connection details
                    if (radSqlAuth.Checked)
                    {
                        testSettings.UseRemoteServer = true;
                        testSettings.RemoteServer = txtServer.Text;
                        testSettings.RemotePort = (int)numPort.Value;
                        testSettings.RemoteDatabase = "dbMillionaire";
                        testSettings.RemoteLogin = txtUsername.Text;
                        testSettings.RemotePassword = txtPassword.Text;
                    }
                    else
                    {
                        // Windows Auth with manual server
                        testSettings.LocalInstance = txtServer.Text;
                    }
                }
                else
                {
                    // Use selected instance
                    string selectedInstance = cmbServerInstance.SelectedItem?.ToString() ?? "";
                    if (selectedInstance.Contains("\\"))
                    {
                        testSettings.LocalInstance = selectedInstance.Split('\\')[1];
                    }
                    else
                    {
                        testSettings.LocalInstance = "MSSQLSERVER"; // Default instance
                    }
                }
            }
            
            // Test connection (without database name first)
            string connectionString = testSettings.GetConnectionString();
            
            await Task.Run(() =>
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();
            });
            
            GameConsole.Info("Connection test successful");
            
            // Check if database exists
            var dbContext = new GameDatabaseContext(connectionString);
            _databaseExists = await dbContext.DatabaseExistsAsync();
            
            if (_databaseExists)
            {
                lblConnectionStatus.Text = "✓ Server connected! Database 'dbMillionaire' already exists.";
                lblConnectionStatus.ForeColor = Color.Green;
                chkLoadSampleData.Enabled = false; // Can't load sample data if DB exists
                chkLoadSampleData.Checked = false;
                GameConsole.Info("Database 'dbMillionaire' already exists");
            }
            else
            {
                lblConnectionStatus.Text = "✓ Server connected! Database will be created on finish.";
                lblConnectionStatus.ForeColor = Color.Green;
                chkLoadSampleData.Enabled = true; // Allow sample data loading
                GameConsole.Info("Database 'dbMillionaire' does not exist - will be created");
            }
            
            _connectionTestPassed = true;
            btnFinish.Enabled = true;
            
            return true;
        }
        catch (Exception ex)
        {
            lblConnectionStatus.Text = $"✗ Connection failed: {ex.Message}";
            lblConnectionStatus.ForeColor = Color.Red;
            _connectionTestPassed = false;
            btnFinish.Enabled = false;
            chkLoadSampleData.Enabled = false;
            GameConsole.Error($"Connection test failed: {ex.Message}");
            return false;
        }
        finally
        {
            _isTestingConnection = false;
            btnTest.Enabled = true;
            btnTest.Text = "Test Connection";
        }
    }

    #endregion

    #region Database Creation Flow

    /// <summary>
    /// Executes when Finish button is clicked
    /// Saves configuration, creates database, loads sample data (optional)
    /// </summary>
    private async void btnFinish_Click(object sender, EventArgs e)
    {
        if (!_connectionTestPassed)
        {
            MessageBox.Show(
                "Please test the connection before finishing setup.",
                "Connection Not Tested",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }
        
        btnFinish.Enabled = false;
        btnTest.Enabled = false;
        btnCancel.Enabled = false;
        lblConnectionStatus.Text = "Setting up database...";
        lblConnectionStatus.ForeColor = Color.Blue;
        
        try
        {
            // 1. Build and save configuration
            var settings = new SqlConnectionSettings();
            
            if (radLocalDB.Checked)
            {
                settings.UseLocalDB = true;
                settings.UseRemoteServer = false;
            }
            else if (radSqlServer.Checked)
            {
                settings.UseLocalDB = false;
                
                if (cmbServerInstance.SelectedItem?.ToString() == "<Browse for more...>")
                {
                    if (radSqlAuth.Checked)
                    {
                        settings.UseRemoteServer = true;
                        settings.RemoteServer = txtServer.Text;
                        settings.RemotePort = (int)numPort.Value;
                        settings.RemoteDatabase = "dbMillionaire";
                        settings.RemoteLogin = txtUsername.Text;
                        settings.RemotePassword = txtPassword.Text;
                    }
                    else
                    {
                        settings.UseRemoteServer = false;
                        settings.LocalInstance = txtServer.Text;
                    }
                }
                else
                {
                    settings.UseRemoteServer = false;
                    string selectedInstance = cmbServerInstance.SelectedItem?.ToString() ?? "";
                    if (selectedInstance.Contains("\\"))
                    {
                        settings.LocalInstance = selectedInstance.Split('\\')[1];
                    }
                    else
                    {
                        settings.LocalInstance = "MSSQLSERVER";
                    }
                }
            }
            
            _settingsManager.Settings = settings;
            _settingsManager.SaveSettings();
            GameConsole.Info("Database configuration saved to sql.xml");
            
            // 2. Create database (if doesn't exist)
            if (!_databaseExists)
            {
                lblConnectionStatus.Text = "Creating database...";
                string connStr = settings.GetConnectionString();
                var dbContext = new GameDatabaseContext(connStr);
                await dbContext.CreateDatabaseAsync();
                GameConsole.Info("Database 'dbMillionaire' created successfully");
                
                // 3. Load sample data (if requested)
                if (chkLoadSampleData.Checked)
                {
                    lblConnectionStatus.Text = "Loading sample data...";
                    await LoadSampleDataAsync(settings);
                    GameConsole.Info("Sample data loaded successfully");
                }
            }
            
            lblConnectionStatus.Text = "✓ Setup complete!";
            lblConnectionStatus.ForeColor = Color.Green;
            
            await Task.Delay(1000); // Brief pause to show success message
            
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            lblConnectionStatus.Text = $"✗ Setup failed: {ex.Message}";
            lblConnectionStatus.ForeColor = Color.Red;
            
            MessageBox.Show(
                $"Failed to complete database setup:\n\n{ex.Message}",
                "Setup Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            
            btnFinish.Enabled = _connectionTestPassed;
            btnTest.Enabled = true;
            btnCancel.Enabled = true;
            
            GameConsole.Error($"Database setup failed: {ex.Message}");
        }
    }

    #endregion

    #region Sample Data Loading

    /// <summary>
    /// Loads sample trivia data from init_database.sql
    /// Handles GO statement splitting and batch execution
    /// </summary>
    private async Task LoadSampleDataAsync(SqlConnectionSettings settings)
    {
        // Find init_database.sql file
        string sqlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib", "sql", "init_database.sql");
        
        if (!File.Exists(sqlFilePath))
        {
            throw new FileNotFoundException("Could not find init_database.sql file", sqlFilePath);
        }
        
        GameConsole.Debug($"Loading sample data from: {sqlFilePath}");
        
        // Read SQL file
        string sqlContent = await File.ReadAllTextAsync(sqlFilePath);
        
        // Split by GO statements using regex to handle any whitespace/newline combination
        string[] batches = System.Text.RegularExpressions.Regex.Split(
            sqlContent,
            @"^\s*GO\s*$",
            System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );

        GameConsole.Debug($"[SampleData] Total batches found: {batches.Length}");

        string connectionString = settings.GetConnectionString("dbMillionaire");

        await Task.Run(() =>
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            int batchNumber = 0;
            int executedBatches = 0;

            foreach (var batch in batches)
            {
                batchNumber++;
                string trimmedBatch = batch.Trim();
                GameConsole.Debug($"[SampleData] Batch {batchNumber} (first 100 chars): {trimmedBatch.Substring(0, Math.Min(100, trimmedBatch.Length))}");

                // Only skip if truly empty
                if (string.IsNullOrWhiteSpace(trimmedBatch))
                {
                    GameConsole.Debug($"[SampleData] Skipping empty batch {batchNumber}");
                    continue;
                }

                // Skip USE statements (already connected to database)
                if (trimmedBatch.StartsWith("USE ", StringComparison.OrdinalIgnoreCase))
                {
                    GameConsole.Debug($"[SampleData] Skipping USE statement in batch {batchNumber}");
                    continue;
                }

                try
                {
                    using var command = new SqlCommand(trimmedBatch, connection);
                    command.CommandTimeout = 120; // 2 minutes for large INSERT statements
                    command.ExecuteNonQuery();
                    executedBatches++;
                    GameConsole.Debug($"[SampleData] Executed SQL batch {batchNumber} successfully");
                }
                catch (Exception ex)
                {
                    GameConsole.Error($"[SampleData] Failed to execute SQL batch {batchNumber}: {ex.Message}");
                    GameConsole.Debug($"[SampleData] Batch content (first 200 chars): {trimmedBatch.Substring(0, Math.Min(200, trimmedBatch.Length))}...");
                    throw new Exception($"Failed to execute SQL batch {batchNumber}: {ex.Message}", ex);
                }
            }

            GameConsole.Debug($"[SampleData] Successfully loaded {executedBatches} SQL batches (skipped {batchNumber - executedBatches})");
        });
    }

    #endregion

    #region UI Event Handlers

    private void radLocalDB_CheckedChanged(object sender, EventArgs e)
    {
        if (radLocalDB.Checked)
        {
            cmbServerInstance.Enabled = false;
            grpConnectionDetails.Visible = false;
            lblSqlServerStatus.Visible = false;
            
            // Reset connection test
            _connectionTestPassed = false;
            btnFinish.Enabled = false;
            chkLoadSampleData.Enabled = false;
            lblConnectionStatus.Text = "Ready to test connection";
            lblConnectionStatus.ForeColor = Color.Gray;
        }
    }

    private void radSqlServer_CheckedChanged(object sender, EventArgs e)
    {
        if (radSqlServer.Checked)
        {
            cmbServerInstance.Enabled = true;
            lblSqlServerStatus.Visible = true;
            
            // Check if "Browse for more..." is selected
            if (cmbServerInstance.SelectedItem?.ToString() == "<Browse for more...>")
            {
                grpConnectionDetails.Visible = true;
            }
            
            // Reset connection test
            _connectionTestPassed = false;
            btnFinish.Enabled = false;
            chkLoadSampleData.Enabled = false;
            lblConnectionStatus.Text = "Ready to test connection";
            lblConnectionStatus.ForeColor = Color.Gray;
        }
    }

    private void cmbServerInstance_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cmbServerInstance.SelectedItem?.ToString() == "<Browse for more...>")
        {
            grpConnectionDetails.Visible = true;
        }
        else
        {
            grpConnectionDetails.Visible = false;
        }
        
        // Reset connection test when instance changes
        _connectionTestPassed = false;
        btnFinish.Enabled = false;
        chkLoadSampleData.Enabled = false;
        lblConnectionStatus.Text = "Ready to test connection";
        lblConnectionStatus.ForeColor = Color.Gray;
    }

    private void radWindowsAuth_CheckedChanged(object sender, EventArgs e)
    {
        txtUsername.Enabled = false;
        txtPassword.Enabled = false;
    }

    private void radSqlAuth_CheckedChanged(object sender, EventArgs e)
    {
        txtUsername.Enabled = radSqlAuth.Checked;
        txtPassword.Enabled = radSqlAuth.Checked;
    }

    private async void btnTest_Click(object sender, EventArgs e)
    {
        await TestConnectionAsync();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to cancel the setup?\n\nThe application cannot run without a database connection.",
            "Cancel Setup?",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        
        if (result == DialogResult.Yes)
        {
            GameConsole.Info("First-run wizard cancelled by user");
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    #endregion
}
