using MillionaireGame.Core.Database;
using MillionaireGame.Core.Game;
using MillionaireGame.Core.Settings;
using MillionaireGame.Forms;
using MillionaireGame.Services;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;

namespace MillionaireGame;

internal static class Program
{
    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll")]
    private static extern bool FreeConsole();

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

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
        
        // Settings are stored in XML for now (database migration disabled)
        // Load application settings from XML first to check console setting
        var appSettings = new ApplicationSettingsManager();
        appSettings.LoadSettings();
        
        // Allocate console in debug mode or if ShowConsole is enabled in release mode
        if (DebugMode || appSettings.Settings.ShowConsole)
        {
            // Allocate console window for debug output
            AllocConsole();
            Console.WriteLine("===========================================");
            Console.WriteLine("  THE MILLIONAIRE GAME - DEBUG MODE");
            Console.WriteLine("===========================================");
            Console.WriteLine($"Application starting at {DateTime.Now}");
            Console.WriteLine();
        }

        // Initialize application
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Load settings
        var sqlSettings = new SqlSettingsManager();
        sqlSettings.LoadSettings();

        if (DebugMode || appSettings.Settings.ShowConsole)
        {
            Console.WriteLine("Loading SQL settings...");
            Console.WriteLine($"Connection string configured: {!string.IsNullOrEmpty(sqlSettings.Settings.GetConnectionString())}");
            Console.WriteLine();
        }

        if (DebugMode || appSettings.Settings.ShowConsole)
        {
            Console.WriteLine("Application settings loaded from XML.");
            Console.WriteLine();
        }

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
        IntPtr consoleWindow = GetConsoleWindow();
        
        if (showConsole)
        {
            // Show console if it doesn't exist
            if (consoleWindow == IntPtr.Zero)
            {
                AllocConsole();
                Console.WriteLine("===========================================");
                Console.WriteLine("  THE MILLIONAIRE GAME - CONSOLE");
                Console.WriteLine("===========================================");
                Console.WriteLine($"Console opened at {DateTime.Now}");
                Console.WriteLine();
            }
        }
        else
        {
            // Hide console by freeing it
            if (consoleWindow != IntPtr.Zero)
            {
                FreeConsole();
            }
        }
        #endif
    }
}
