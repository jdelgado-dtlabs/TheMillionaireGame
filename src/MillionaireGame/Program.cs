using MillionaireGame.Core.Database;
using MillionaireGame.Core.Game;
using MillionaireGame.Core.Settings;
using MillionaireGame.Forms;
using MillionaireGame.Services;

namespace MillionaireGame;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Initialize application
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Load settings
        var sqlSettings = new SqlSettingsManager();
        sqlSettings.LoadSettings();

        var appSettings = new ApplicationSettingsManager();
        appSettings.LoadSettings();

        // Initialize database
        var dbContext = new GameDatabaseContext(sqlSettings.Settings.GetConnectionString());
        
        try
        {
            // Check if database exists, create if not
            if (!dbContext.DatabaseExistsAsync().Result)
            {
                var result = MessageBox.Show(
                    "Database not found. Would you like to create it?",
                    "Database Setup",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    dbContext.CreateDatabaseAsync().Wait();
                    MessageBox.Show(
                        "Database created successfully!",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error initializing database: {ex.Message}\n\nPlease check your SQL Server connection settings.",
                "Database Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        // Initialize services
        var gameService = new GameService();
        var questionRepository = new QuestionRepository(dbContext.GetFullConnectionString());
        var screenService = new ScreenUpdateService();
        var soundService = new SoundService();

        // Create and run main control panel
        var controlPanel = new ControlPanelForm(gameService, appSettings, sqlSettings, questionRepository, screenService, soundService);
        Application.Run(controlPanel);
    }
}
