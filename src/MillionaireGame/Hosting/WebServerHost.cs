using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MillionaireGame.Web.Data;
using MillionaireGame.Web.Database;
using MillionaireGame.Web.Hubs;
using MillionaireGame.Web.Services;
using MillionaireGame.Utilities;

namespace MillionaireGame.Hosting;

/// <summary>
/// Hosts the Web-Based Audience Participation System (WAPS) inside the WinForms application
/// </summary>
public class WebServerHost : IDisposable
{
    private IHost? _host;
    private string? _baseUrl;
    private readonly string _sqlConnectionString;
    private bool _isDisposed;

    public bool IsRunning => _host != null;
    public string? BaseUrl => _baseUrl;

    /// <summary>
    /// Fired when the server starts successfully
    /// </summary>
    public event EventHandler<string>? ServerStarted;

    /// <summary>
    /// Fired when the server stops
    /// </summary>
    public event EventHandler? ServerStopped;

    /// <summary>
    /// Fired when the server encounters an error
    /// </summary>
    public event EventHandler<Exception>? ServerError;

    public WebServerHost(string sqlConnectionString)
    {
        _sqlConnectionString = sqlConnectionString;
    }

    /// <summary>
    /// Gets a service from the web server's dependency injection container
    /// </summary>
    /// <typeparam name="T">The service type to resolve</typeparam>
    /// <returns>The service instance, or null if not found or server not running</returns>
    public T? GetService<T>() where T : class
    {
        if (_host == null)
            return null;

        try
        {
            using var scope = _host.Services.CreateScope();
            return scope.ServiceProvider.GetService<T>();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Starts the web server on the specified IP address and port
    /// </summary>
    /// <param name="ipAddress">IP address to bind to (e.g., "0.0.0.0", "127.0.0.1", or local IP)</param>
    /// <param name="port">Port number to listen on</param>
    public async Task StartAsync(string ipAddress, int port)
    {
        if (_host != null)
        {
            throw new InvalidOperationException("Server is already running. Stop it before starting again.");
        }

        try
        {
            // Strip CIDR notation if present (e.g., "192.168.1.5/24" -> "192.168.1.5")
            var slashIndex = ipAddress.IndexOf('/');
            if (slashIndex >= 0)
            {
                ipAddress = ipAddress.Substring(0, slashIndex);
            }
            
            // Log input parameters for debugging
            WebServerConsole.Debug($"[WebServer] StartAsync called with IP: '{ipAddress}', Port: {port}");
            
            // Determine display URL - use public IP for 0.0.0.0
            string displayIP = ipAddress;
            if (ipAddress == "0.0.0.0")
            {
                // Try to get public IP for display purposes
                var publicIP = await NetworkHelper.GetPublicIPAddressAsync();
                displayIP = publicIP ?? "0.0.0.0";
            }
            
            // Build list of URLs to bind to
            // Always include localhost unless the selected IP already is localhost
            string bindUrl;
            
            if (ipAddress == "127.0.0.1" || ipAddress == "localhost")
            {
                // Localhost only
                bindUrl = $"http://127.0.0.1:{port}";
            }
            else if (ipAddress == "0.0.0.0")
            {
                // Bind to all interfaces
                bindUrl = $"http://0.0.0.0:{port}";
            }
            else
            {
                // For specific local IP, bind to both localhost and that IP
                bindUrl = $"http://127.0.0.1:{port};http://{ipAddress}:{port}";
            }
            
            // Display URL shows the public IP if available
            _baseUrl = $"http://{displayIP}:{port}";
            
            WebServerConsole.Debug($"[WebServer] Bind URL: '{bindUrl}'");
            WebServerConsole.Debug($"[WebServer] Display URL: '{_baseUrl}'");

            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls(bindUrl);
                    // Set the content root and web root to the application's base directory
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    webBuilder.UseContentRoot(baseDir);
                    webBuilder.UseWebRoot(Path.Combine(baseDir, "wwwroot"));
                    
                    webBuilder.ConfigureServices((context, services) =>
                    {
                        ConfigureServices(services);
                    });
                    webBuilder.Configure(app =>
                    {
                        ConfigureApp(app);
                    });
                })
                .ConfigureLogging(logging =>
                {
                    // Clear default providers
                    logging.ClearProviders();
                    
                    // Add custom WebServerConsole logger
                    logging.AddProvider(new WebServerConsoleLoggerProvider());
                    
                    // Set minimum level to Information to see participant joins
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
                    
                    // Filter out noisy Microsoft logs
                    logging.AddFilter("Microsoft.AspNetCore", Microsoft.Extensions.Logging.LogLevel.Warning);
                    logging.AddFilter("Microsoft.Hosting", Microsoft.Extensions.Logging.LogLevel.Warning);
                    logging.AddFilter("Microsoft.EntityFrameworkCore", Microsoft.Extensions.Logging.LogLevel.Warning);
                });

            _host = builder.Build();

            // Delete old database to ensure clean state on startup
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "waps.db");
            if (File.Exists(dbPath))
            {
                try
                {
                    File.Delete(dbPath);
                    WebServiceConsole.Info($"[WebServer] Deleted existing database: {dbPath}");
                }
                catch (Exception ex)
                {
                    WebServiceConsole.Warn($"[WebServer] Could not delete database: {ex.Message}");
                }
            }

            // Ensure database is created
            using (var scope = _host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<WAPSDbContext>();
                context.Database.EnsureCreated();
                WebServiceConsole.Info("[WebServer] Database created with clean state");
            }

            await _host.StartAsync();

            // Wait a moment for the server to fully initialize and start accepting requests
            // This prevents "message channel closed" errors when browsers connect too early
            await Task.Delay(500);
            WebServiceConsole.Info("[WebServer] Server ready to accept connections");

            ServerStarted?.Invoke(this, _baseUrl);
        }
        catch (Exception ex)
        {
            _host = null;
            _baseUrl = null;
            ServerError?.Invoke(this, ex);
            throw;
        }
    }

    /// <summary>
    /// Stops the web server
    /// </summary>
    public async Task StopAsync()
    {
        if (_host == null)
        {
            WebServiceConsole.Debug("WebServerHost.StopAsync: Host is already null, nothing to stop.");
            return;
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        WebServiceConsole.Info("=== WebServer Shutdown Started ===");

        try
        {
            // Step 1: Notify all SignalR clients to disconnect gracefully
            WebServiceConsole.Info("Step 1: Notifying SignalR clients to disconnect...");
            try
            {
                var hubContext = _host.Services.GetService(typeof(IHubContext<FFFHub>)) as IHubContext<FFFHub>;
                var ataHubContext = _host.Services.GetService(typeof(IHubContext<ATAHub>)) as IHubContext<ATAHub>;

                if (hubContext != null)
                {
                    await hubContext.Clients.All.SendAsync("ServerShuttingDown");
                    WebServiceConsole.Debug("  - Sent shutdown notification to FFF hub clients");
                }

                if (ataHubContext != null)
                {
                    await ataHubContext.Clients.All.SendAsync("ServerShuttingDown");
                    WebServiceConsole.Debug("  - Sent shutdown notification to ATA hub clients");
                }

                // Give clients a moment to disconnect gracefully
                await Task.Delay(500);
                WebServiceConsole.Info($"  Completed in {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                WebServiceConsole.Warn($"  Failed to notify clients: {ex.Message}");
            }

            // Step 2: Stop the ASP.NET Core host
            stopwatch.Restart();
            WebServiceConsole.Info("Step 2: Stopping ASP.NET Core host...");
            await _host.StopAsync(TimeSpan.FromSeconds(5));
            WebServiceConsole.Info($"  Completed in {stopwatch.ElapsedMilliseconds}ms");

            // Step 3: Dispose resources
            stopwatch.Restart();
            WebServiceConsole.Info("Step 3: Disposing host resources...");
            _host.Dispose();
            _host = null;
            _baseUrl = null;
            WebServiceConsole.Info($"  Completed in {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Stop();
            WebServiceConsole.Info($"=== WebServer Shutdown Complete (Total: {stopwatch.ElapsedMilliseconds}ms) ===");

            ServerStopped?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            WebServiceConsole.Error($"=== WebServer Shutdown Failed after {stopwatch.ElapsedMilliseconds}ms: {ex.Message} ===");
            ServerError?.Invoke(this, ex);
            throw;
        }
    }

    /// <summary>
    /// Configures services for the web application (matches Program.cs)
    /// </summary>
    private void ConfigureServices(IServiceCollection services)
    {
        // Configure to work behind reverse proxy (Nginx)
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        // Add services
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        // Note: Swagger removed - not needed in embedded hosting (dev tool only)

        // Add SignalR
        services.AddSignalR();

        // Add database context
        services.AddDbContext<WAPSDbContext>(options =>
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "waps.db");
            options.UseSqlite($"Data Source={dbPath}");
        });

        // Add repositories and services
        services.AddScoped<FFFQuestionRepository>(provider => new FFFQuestionRepository(_sqlConnectionString));
        services.AddScoped<SessionService>();
        services.AddScoped<FFFService>();
        services.AddScoped<NameValidationService>();
        services.AddScoped<StatisticsService>();

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
    }

    /// <summary>
    /// Configures the HTTP request pipeline (matches Program.cs)
    /// </summary>
    private void ConfigureApp(IApplicationBuilder app)
    {
        // Configure the HTTP request pipeline
        app.UseForwardedHeaders();
        app.UseCors("AllowAll");

        // Add cache prevention headers for privacy/security
        app.Use(async (context, next) =>
        {
            var path = context.Request.Path.Value?.ToLowerInvariant();
            if (path != null && (path.EndsWith(".html") || path.EndsWith(".js") || path.EndsWith(".css") || path == "/"))
            {
                context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
                context.Response.Headers["Pragma"] = "no-cache";
                context.Response.Headers["Expires"] = "0";
            }
            await next();
        });

        // Serve static files (wwwroot)
        app.UseDefaultFiles();
        app.UseStaticFiles();

        // Use routing
        app.UseRouting();

        // Map endpoints
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHub<FFFHub>("/hubs/fff");
            endpoints.MapHub<ATAHub>("/hubs/ata");

            // Health check endpoint
            endpoints.MapGet("/health", async context =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    Status = "Healthy",
                    Service = "WAPS (Web-Based Audience Participation System)",
                    Timestamp = DateTime.UtcNow
                });
            });
        });
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _host?.Dispose();
        _host = null;
        _isDisposed = true;

        GC.SuppressFinalize(this);
    }
}
/// <summary>
/// Custom logger provider that writes to WebServiceConsole
/// </summary>
internal class WebServerConsoleLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new WebServerConsoleLogger(categoryName);
    }

    public void Dispose() { }
}

/// <summary>
/// Custom logger that writes to WebServiceConsole
/// </summary>
internal class WebServerConsoleLogger : ILogger
{
    private readonly string _categoryName;

    public WebServerConsoleLogger(string categoryName)
    {
        _categoryName = categoryName;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => logLevel >= Microsoft.Extensions.Logging.LogLevel.Information;

    public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        var logPrefix = logLevel switch
        {
            Microsoft.Extensions.Logging.LogLevel.Information => "[INFO]",
            Microsoft.Extensions.Logging.LogLevel.Warning => "[WARN]",
            Microsoft.Extensions.Logging.LogLevel.Error => "[ERROR]",
            Microsoft.Extensions.Logging.LogLevel.Critical => "[CRITICAL]",
            _ => $"[{logLevel}]"
        };

        // Get short category name
        var shortCategory = _categoryName.Split('.').LastOrDefault() ?? _categoryName;
        
        // Map ASP.NET log level to our LogLevel
        var ourLevel = logLevel switch
        {
            Microsoft.Extensions.Logging.LogLevel.Warning => Utilities.LogLevel.WARN,
            Microsoft.Extensions.Logging.LogLevel.Error => Utilities.LogLevel.ERROR,
            Microsoft.Extensions.Logging.LogLevel.Critical => Utilities.LogLevel.ERROR,
            _ => Utilities.LogLevel.INFO
        };
        
        WebServiceConsole.Log($"{logPrefix} [{shortCategory}] {message}", ourLevel);

        if (exception != null)
        {
            WebServiceConsole.Error($"  Exception: {exception.Message}");
        }
    }
}