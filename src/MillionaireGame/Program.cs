using MillionaireGame.Core.Database;
using MillionaireGame.Core.Game;
using MillionaireGame.Core.Settings;
using MillionaireGame.Forms;
using MillionaireGame.Services;
using MillionaireGame.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace MillionaireGame;

internal static class Program
{
    public static bool DebugMode { get; private set; }
    
    private static GameLogWindow? _gameLogWindow;
    
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static async Task Main(string[] args)
    {
        // Check for debug mode argument or Debug build configuration
        DebugMode = args.Contains("--debug") || args.Contains("-d");
        
        #if DEBUG
        DebugMode = true;
        #endif
        
        // Settings are stored in XML for now (database migration disabled)
        // Load application settings from XML first to check console setting
        var appSettings = new ApplicationSettingsManager();
        appSettings.LoadSettings();
        
        // Initialize application first (required before creating any Forms)
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Load settings
        var sqlSettings = new SqlSettingsManager();
        sqlSettings.LoadSettings();

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
        var questionRepository = new QuestionRepository(sqlSettings.Settings.GetConnectionString("dbMillionaire"));
        var screenService = new ScreenUpdateService();
        var soundService = new SoundService();
        
        // Load sounds from settings
        soundService.LoadSoundsFromSettings(appSettings.Settings);

        // Setup dependency injection container
        var services = new ServiceCollection();
        services.AddSingleton(gameService);
        services.AddSingleton(appSettings);
        services.AddSingleton(sqlSettings);
        services.AddSingleton(questionRepository);
        services.AddSingleton(screenService);
        services.AddSingleton(soundService);
        
        ServiceProvider = services.BuildServiceProvider();

        // Create and run main control panel
        var controlPanel = new ControlPanelForm(gameService, appSettings, sqlSettings, questionRepository, screenService, soundService);
        
        // Create and show game log window after the main form is shown
        controlPanel.Shown += (s, e) =>
        {
            if (DebugMode || appSettings.Settings.ShowConsole)
            {
                _gameLogWindow = new GameLogWindow();
                _gameLogWindow.Log("THE MILLIONAIRE GAME - Debug Console");
                _gameLogWindow.Log($"Version: Debug Build");
                _gameLogWindow.Log($"Started: {DateTime.Now}");
                _gameLogWindow.LogSeparator();
                _gameLogWindow.Log("Application initialized successfully.");
                _gameLogWindow.Log("");
                _gameLogWindow.Show();
            }
        };
        
        Application.Run(controlPanel);
    }

    /// <summary>
    /// Shows or hides the console window based on the current setting
    /// </summary>
    public static void UpdateConsoleVisibility(bool showConsole)
    {
        #if DEBUG
        // In debug mode, console is always visible
        return;
        #else
        if (showConsole)
        {
            // Show game log window if it doesn't exist
            if (_gameLogWindow == null || _gameLogWindow.IsDisposed)
            {
                _gameLogWindow = new GameLogWindow();
            }
            
            if (!_gameLogWindow.Visible)
            {
                _gameLogWindow.Show();
            }
        }
        else
        {
            // Hide game log window
            _gameLogWindow?.Hide();
        }
        #endif
    }
}
