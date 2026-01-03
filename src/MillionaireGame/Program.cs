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
        
        // Setup global exception handlers
        Application.ThreadException += Application_ThreadException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        
        // Load SQL connection settings from sql.xml
        var sqlSettings = new SqlSettingsManager();
        sqlSettings.LoadSettings();

        // Initialize database
        var dbContext = new GameDatabaseContext(sqlSettings.Settings.GetConnectionString());
        
        try
        {
            // Check if database exists, create if not
            bool dbExists = dbContext.DatabaseExistsAsync().Result;
            if (!dbExists)
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
            else
            {
                // Database exists - ensure all tables exist (including WAPS tables)
                // CreateDatabaseAsync has IF NOT EXISTS checks, so it's safe to run
                dbContext.CreateDatabaseAsync().Wait();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error initializing database: {ex.Message}\\n\\nPlease check your SQL Server connection settings.",
                "Database Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        // Initialize application settings from database
        var appSettings = new ApplicationSettingsManager(sqlSettings.Settings.GetConnectionString("dbMillionaire"));
        appSettings.LoadSettings();

        // Initialize application first (required before creating any Forms)
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Initialize services
        var gameService = new GameService();
        var questionRepository = new QuestionRepository(sqlSettings.Settings.GetConnectionString("dbMillionaire"));
        var screenService = new ScreenUpdateService();
        
        SoundService soundService;
        try
        {
            GameConsole.Debug("[Program] Creating SoundService...");
            soundService = new SoundService(appSettings);
            GameConsole.Debug("[Program] SoundService created successfully");
            
            // Load sounds from settings
            GameConsole.Debug("[Program] Loading sounds from settings...");
            soundService.LoadSoundsFromSettings(appSettings.Settings);
            GameConsole.Debug("[Program] Sounds loaded successfully");
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[Program] CRITICAL ERROR creating SoundService: {ex.Message}");
            GameConsole.Error($"[Program] Exception type: {ex.GetType().Name}");
            GameConsole.Error($"[Program] Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                GameConsole.Error($"[Program] Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }

        // Setup dependency injection container
        var services = new ServiceCollection();
        services.AddSingleton(gameService);
        services.AddSingleton(appSettings);
        services.AddSingleton(sqlSettings);
        services.AddSingleton(questionRepository);
        services.AddSingleton(screenService);
        services.AddSingleton(soundService);
        
        ServiceProvider = services.BuildServiceProvider();

        // Create and run main control panel (FIRST window to show)
        var controlPanel = new ControlPanelForm(gameService, appSettings, sqlSettings, questionRepository, screenService, soundService);
        
        Application.Run(controlPanel);
    }

    /// <summary>
    /// Handles unhandled exceptions in Windows Forms UI threads
    /// </summary>
    private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
    {
        LogUnhandledException(e.Exception, "UI Thread Exception");
    }

    /// <summary>
    /// Handles unhandled exceptions in non-UI threads
    /// </summary>
    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        LogUnhandledException(exception, "AppDomain Unhandled Exception");
    }

    /// <summary>
    /// Logs unhandled exceptions to GameConsole and displays error dialog
    /// </summary>
    private static void LogUnhandledException(Exception? exception, string source)
    {
        if (exception == null)
        {
            GameConsole.Error($"[{source}] Unknown exception occurred (null exception object)");
            return;
        }

        // Log to GameConsole
        GameConsole.LogSeparator();
        GameConsole.Error($"[UNHANDLED EXCEPTION] {source}");
        GameConsole.LogSeparator();
        GameConsole.Error($"Exception Type: {exception.GetType().FullName}");
        GameConsole.Error($"Message: {exception.Message}");
        GameConsole.Error($"Source: {exception.Source}");
        
        if (exception.TargetSite != null)
        {
            GameConsole.Error($"Method: {exception.TargetSite.DeclaringType?.FullName}.{exception.TargetSite.Name}");
        }
        
        GameConsole.Error($"Stack Trace:");
        GameConsole.Error(exception.StackTrace ?? "(No stack trace available)");
        
        // Log inner exceptions
        if (exception.InnerException != null)
        {
            GameConsole.Error($"Inner Exception: {exception.InnerException.GetType().FullName}");
            GameConsole.Error($"Inner Message: {exception.InnerException.Message}");
            GameConsole.Error($"Inner Stack Trace:");
            GameConsole.Error(exception.InnerException.StackTrace ?? "(No stack trace available)");
        }
        
        GameConsole.LogSeparator();

        // Show error dialog to user
        var message = $"An unhandled exception has occurred:\n\n{exception.Message}\n\n" +
                     $"Source: {source}\n" +
                     $"Type: {exception.GetType().Name}\n\n" +
                     $"This error has been logged to the console and log file.\n\n" +
                     $"Click OK to continue or Cancel to exit the application.";

        var result = MessageBox.Show(
            message,
            "Unhandled Exception",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Error);

        if (result == DialogResult.Cancel)
        {
            GameConsole.Error("User chose to exit application after unhandled exception.");
            Application.Exit();
        }
        else
        {
            GameConsole.Info("User chose to continue after unhandled exception.");
        }
    }
}
