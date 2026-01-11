using MillionaireGame.Core.Database;
using MillionaireGame.Core.Game;
using MillionaireGame.Core.Settings;
using MillionaireGame.Core.Services;
using MillionaireGame.Core.Models.Telemetry;
using MillionaireGame.Database;
using MillionaireGame.Forms;
using MillionaireGame.Services;
using MillionaireGame.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace MillionaireGame;

internal static class Program
{
    public static bool DebugMode { get; private set; }
    
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    
    private static HeartbeatService? _heartbeatService;

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static async Task Main(string[] args)
    {
        // Check if we're being launched by the watchdog
        const string WatchdogMarker = "--watchdog-child";
        bool launchedByWatchdog = args.Contains(WatchdogMarker);
        
        if (!launchedByWatchdog)
        {
            // We need to launch through the watchdog
            if (!LaunchThroughWatchdog(args))
            {
                // Watchdog not available, continue without it
                GameConsole.Warn("[Startup] Watchdog not available - running without crash monitoring");
            }
            else
            {
                // Watchdog will launch us again with the marker argument
                return;
            }
        }
        
        // Check for debug mode argument or Debug build configuration
        DebugMode = args.Contains("--debug") || args.Contains("-d");
        
        #if DEBUG
        DebugMode = true;
        #endif
        
        // NOTE: Removed global exception handlers to allow watchdog to detect crashes
        // The watchdog now handles crash detection and reporting
        
        // Load SQL connection settings from sql.xml
        var sqlSettings = new SqlSettingsManager();
        
        // Check for first-run (no sql.xml) - show wizard
        // MUST match SqlSettingsManager's path (LocalApplicationData, not ApplicationData)
        string settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TheMillionaireGame", "sql.xml");
        
        if (!File.Exists(settingsPath))
        {
            GameConsole.Info("[Startup] First run detected - launching database setup wizard");
            
            // Start heartbeat service for wizard to prevent watchdog timeout
            _heartbeatService = new HeartbeatService();
            _heartbeatService.SetActivity("Database Setup Wizard");
            
            using var wizard = new FirstRunWizard();
            
            // Start heartbeat with wizard form
            _heartbeatService.Start(wizard);
            
            var wizardResult = wizard.ShowDialog();
            
            // Stop heartbeat after wizard closes
            _heartbeatService.Stop();
            _heartbeatService.Dispose();
            _heartbeatService = null;
            
            if (wizardResult != DialogResult.OK)
            {
                GameConsole.Info("[Startup] Database setup cancelled by user - exiting application");
                return;
            }
            
            GameConsole.Info("[Startup] Database setup completed successfully");
        }
        
        sqlSettings.LoadSettings();

        // Initialize database
        var dbContext = new GameDatabaseContext(sqlSettings.Settings.GetConnectionString());
        
        try
        {
            // Database should now exist (created by wizard or already exists)
            // Just verify it exists, don't show MessageBox
            bool dbExists = await dbContext.DatabaseExistsAsync();
            if (!dbExists)
            {
                GameConsole.Error("[Startup] Database does not exist after setup - this should not happen");
                MessageBox.Show(
                    "Database configuration error. Please restart the application.",
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            
            GameConsole.Info("[Startup] Database connection verified");
            
            // Ensure all tables exist (including WAPS tables)
            // CreateDatabaseAsync has IF NOT EXISTS checks, so it's safe to run
            await dbContext.CreateDatabaseAsync();

            // Run database migrations (after database exists)
            try

            {
                GameConsole.Info("[Startup] Running database migrations...");
                var migrationRunner = new MigrationRunner(sqlSettings.Settings.GetConnectionString("dbMillionaire"));
                var migrationSuccess = await migrationRunner.RunMigrationsAsync();

                if (!migrationSuccess)
                {
                    GameConsole.Warn("[Startup] Database migrations encountered errors - proceeding with caution");
                }
            }
            catch (Exception migEx)
            {
                GameConsole.Error($"[Startup] Failed to run database migrations: {migEx.Message}");
                // Continue anyway - app might still work with existing schema
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
        gameService.MoneyTree.Initialize(); // Load settings from database
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
        services.AddSingleton<MonitorInfoService>(); // v1.0.5: Multi-monitor support
        
        ServiceProvider = services.BuildServiceProvider();
        
        // Pre-load monitor information at startup for immediate availability
        var monitorInfoService = ServiceProvider.GetRequiredService<MonitorInfoService>();
        GameConsole.Info("[Startup] Pre-loading monitor information...");
        _ = Task.Run(async () => 
        {
            try
            {
                var monitors = await monitorInfoService.GetAllMonitorsAsync();
                GameConsole.Info($"[Startup] Pre-loaded {monitors.Count} monitors successfully");
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[Startup] Failed to pre-load monitors: {ex.Message}");
            }
        });
        
        // Initialize telemetry service with database connection
        TelemetryService.Instance.Initialize(sqlSettings.Settings.GetConnectionString("dbMillionaire"));
        
        // Wire up telemetry bridge callbacks
        SetupTelemetryBridge();
        
        // Start heartbeat service for watchdog monitoring
        _heartbeatService = new HeartbeatService();
        _heartbeatService.SetActivity("Initializing");
        GameConsole.Debug("[Heartbeat] Service created");

        // Create and run main control panel (FIRST window to show)
        var controlPanel = new ControlPanelForm(gameService, appSettings, sqlSettings, questionRepository, screenService, soundService);
        
        // Now that we have the main form, start heartbeat with UI monitoring
        _heartbeatService.Start(controlPanel);
        GameConsole.Debug("[Heartbeat] Service started with UI monitoring");
        
        // Register application exit handler for telemetry completion
        Application.ApplicationExit += (s, e) =>
        {
            var telemetryService = TelemetryService.Instance;
            var currentGame = telemetryService.GetCurrentGameData();
            
            // If game started but not completed, mark end time
            if (currentGame.GameStartTime != default && currentGame.GameEndTime == default)
            {
                telemetryService.CompleteGame();
            }
        };
        
        _heartbeatService.SetActivity("Running");
        Application.Run(controlPanel);
        
        // Cleanup heartbeat service on exit
        _heartbeatService.Stop();
        _heartbeatService.Dispose();
        GameConsole.Debug("[Heartbeat] Service stopped");
    }
    
    /// <summary>
    /// Setup telemetry bridge callbacks to receive data from web server
    /// </summary>
    private static void SetupTelemetryBridge()
    {
        var telemetryService = TelemetryService.Instance;
        
        // Wire up participant stats callback
        Web.Services.TelemetryBridge.OnParticipantStats = (totalCount, deviceTypes, browserTypes, osTypes) =>
        {
            telemetryService.UpdateParticipantStats(totalCount, deviceTypes, browserTypes, osTypes);
            GameConsole.Debug($"[Telemetry Bridge] Participant stats updated: {totalCount} total");
        };
        
        // Wire up FFF stats callback
        Web.Services.TelemetryBridge.OnFFFStats = (fffData) =>
        {
            var fffStats = new FFFStats
            {
                TotalSubmissions = fffData.TotalSubmissions,
                CorrectSubmissions = fffData.CorrectSubmissions,
                IncorrectSubmissions = fffData.IncorrectSubmissions,
                WinnerName = fffData.WinnerName,
                WinnerTimeMs = fffData.WinnerTimeMs,
                AverageResponseTimeMs = fffData.AverageResponseTimeMs,
                FastestResponseTimeMs = fffData.FastestResponseTimeMs,
                SlowestResponseTimeMs = fffData.SlowestResponseTimeMs
            };
            telemetryService.SetFFFStats(fffStats);
            GameConsole.Debug($"[Telemetry Bridge] FFF stats updated: {fffStats.TotalSubmissions} submissions");
        };
        
        // Wire up ATA stats callback
        Web.Services.TelemetryBridge.OnATAStats = (ataData) =>
        {
            var ataStats = new ATAStats
            {
                TotalVotesCast = ataData.TotalVotesCast,
                VotesForA = ataData.VotesForA,
                VotesForB = ataData.VotesForB,
                VotesForC = ataData.VotesForC,
                VotesForD = ataData.VotesForD,
                PercentageA = ataData.PercentageA,
                PercentageB = ataData.PercentageB,
                PercentageC = ataData.PercentageC,
                PercentageD = ataData.PercentageD,
                VotingCompletionRate = ataData.VotingCompletionRate,
                Mode = ataData.Mode
            };
            telemetryService.SetATAStats(ataStats);
            GameConsole.Debug($"[Telemetry Bridge] ATA stats updated: {ataStats.TotalVotesCast} votes");
        };
        
        GameConsole.Debug("[Telemetry Bridge] Callbacks registered successfully");
    }
    
    /// <summary>
    /// Launches the application through the watchdog for crash monitoring
    /// </summary>
    /// <param name="originalArgs">Original command-line arguments to pass through</param>
    /// <returns>True if watchdog was launched successfully, false if not available</returns>
    private static bool LaunchThroughWatchdog(string[] originalArgs)
    {
        try
        {
            // Get the path to the current executable
            var currentExePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(currentExePath))
            {
                GameConsole.Error("[Startup] Failed to get current executable path");
                return false;
            }
            
            // Get the directory containing the executable
            var exeDirectory = Path.GetDirectoryName(currentExePath);
            if (string.IsNullOrEmpty(exeDirectory))
            {
                GameConsole.Error("[Startup] Failed to get executable directory");
                return false;
            }
            
            // Look for the watchdog executable
            var watchdogPath = Path.Combine(exeDirectory, "MillionaireGame.Watchdog.exe");
            if (!File.Exists(watchdogPath))
            {
                GameConsole.Debug("[Startup] Watchdog not found at: " + watchdogPath);
                return false;
            }
            
            // Build arguments: watchdog takes the app path, and we add --watchdog-child plus original args
            var appArgs = new List<string> { "--watchdog-child" };
            
            // Filter out debug arguments and pass them through
            appArgs.AddRange(originalArgs.Where(arg => arg == "--debug" || arg == "-d"));
            
            var watchdogArgs = $"\"{currentExePath}\" {string.Join(" ", appArgs)}";
            
            // Launch the watchdog
            var startInfo = new ProcessStartInfo
            {
                FileName = watchdogPath,
                Arguments = watchdogArgs,
                UseShellExecute = false,
                CreateNoWindow = false
            };
            
            Process.Start(startInfo);
            GameConsole.Info("[Startup] Launched through watchdog for crash monitoring");
            return true;
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[Startup] Failed to launch watchdog: {ex.Message}");
            return false;
        }
    }}