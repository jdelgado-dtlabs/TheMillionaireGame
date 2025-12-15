using MillionaireGame.Core.Database;
using MillionaireGame.Core.Game;
using MillionaireGame.Core.Settings;

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
                dbContext.CreateDatabaseAsync().Wait();
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

        // Initialize game service
        var gameService = new GameService();

        // TODO: Show main form when it's created
        // Application.Run(new MainForm(gameService, appSettings, sqlSettings));
        
        MessageBox.Show(
            "The Millionaire Game - C# Migration\n\n" +
            "This is the modern C# version of The Millionaire Game.\n" +
            "The migration is currently in progress.\n\n" +
            "Database initialized successfully!",
            "The Millionaire Game",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}
