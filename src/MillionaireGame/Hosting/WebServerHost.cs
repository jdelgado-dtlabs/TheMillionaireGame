using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MillionaireGame.Web.Data;
using MillionaireGame.Web.Database;
using MillionaireGame.Web.Hubs;
using MillionaireGame.Web.Models;
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
                    // Set the content root to the application's base directory
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    webBuilder.UseContentRoot(baseDir);
                    // Note: wwwroot files are embedded as resources, no physical folder needed
                    
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

            // Clean up WAPS data on startup
            // Strategy: Archive ALL participants to history table for telemetry preservation,
            // then clear live Participants table to prevent stale "live" status
            using (var scope = _host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<WAPSDbContext>();
                
                try
                {
                    // Step 1: Archive ALL existing participants to ParticipantHistory for telemetry
                    var existingParticipants = await context.Participants.ToListAsync();
                    if (existingParticipants.Any())
                    {
                        var now = DateTime.UtcNow;
                        var historyRecords = existingParticipants.Select(p => new ParticipantHistory
                        {
                            ParticipantId = p.Id,
                            SessionId = p.SessionId,
                            DisplayName = p.DisplayName,
                            JoinedAt = p.JoinedAt,
                            LastSeenAt = p.LastSeenAt,
                            DisconnectedAt = p.DisconnectedAt,
                            State = p.State.ToString(),
                            HasPlayedFFF = p.HasPlayedFFF,
                            HasUsedATA = p.HasUsedATA,
                            SelectedForFFFAt = p.SelectedForFFFAt,
                            BecameWinnerAt = p.BecameWinnerAt,
                            DeviceType = p.DeviceType,
                            OSType = p.OSType,
                            OSVersion = p.OSVersion,
                            BrowserType = p.BrowserType,
                            BrowserVersion = p.BrowserVersion,
                            HasAgreedToPrivacy = p.HasAgreedToPrivacy,
                            GameSessionId = p.GameSessionId,
                            ArchivedAt = now
                        }).ToList();
                        
                        await context.ParticipantHistory.AddRangeAsync(historyRecords);
                        await context.SaveChangesAsync();
                    }
                    
                    // Step 2: Clear ALL participants (live state resets on app restart)
                    var deletedAllParticipants = await context.Participants.ExecuteDeleteAsync();
                    
                    // Step 3: Reset LIVE session to Active status (or delete it to start fresh)
                    var liveSession = await context.Sessions.FindAsync("LIVE");
                    if (liveSession != null)
                    {
                        liveSession.Status = SessionStatus.Active;
                        await context.SaveChangesAsync();
                        WebServerConsole.Info("[WebServer] Reset LIVE session to Active status");
                    }
                    
                    // Step 4: Find incomplete sessions (PreGame or legacy Waiting with no game progress)
                    var incompleteSessions = await context.Sessions
                        .Where(s => s.Status == SessionStatus.PreGame || s.Status == SessionStatus.Waiting)
                        .Select(s => s.Id)
                        .ToListAsync();
                    
                    if (incompleteSessions.Any())
                    {
                        // Delete related data for incomplete sessions only
                        var deletedVotes = await context.ATAVotes
                            .Where(v => incompleteSessions.Contains(v.SessionId))
                            .ExecuteDeleteAsync();
                        var deletedAnswers = await context.FFFAnswers
                            .Where(a => incompleteSessions.Contains(a.SessionId))
                            .ExecuteDeleteAsync();
                        var deletedSessions = await context.Sessions
                            .Where(s => incompleteSessions.Contains(s.Id))
                            .ExecuteDeleteAsync();
                        
                        WebServerConsole.Info($"[WebServer] Startup cleanup: {existingParticipants.Count} participants archived, {deletedAllParticipants} live records cleared, {deletedSessions} incomplete sessions removed ({deletedAnswers} FFF answers, {deletedVotes} ATA votes)");
                    }
                    else
                    {
                        WebServerConsole.Info($"[WebServer] Startup cleanup: {existingParticipants.Count} participants archived, {deletedAllParticipants} live records cleared, no incomplete sessions found");
                    }
                }
                catch (Exception ex)
                {
                    WebServerConsole.Error($"[WebServer] Failed to clean up incomplete sessions: {ex.Message}");
                    // Don't throw - allow server to start even if cleanup fails
                }
            }

            await _host.StartAsync();

            // Wait a moment for the server to fully initialize and start accepting requests
            // This prevents "message channel closed" errors when browsers connect too early
            await Task.Delay(500);
            WebServerConsole.Info("[WebServer] Server ready to accept connections");

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
            WebServerConsole.Debug("WebServerHost.StopAsync: Host is already null, nothing to stop.");
            return;
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        WebServerConsole.Info("=== WebServer Shutdown Started ===");

        try
        {
            // Step 1: Notify all SignalR clients to disconnect gracefully
            WebServerConsole.Info("Step 1: Notifying SignalR clients to disconnect...");
            try
            {
                var hubContext = _host.Services.GetService(typeof(IHubContext<GameHub>)) as IHubContext<GameHub>;

                if (hubContext != null)
                {
                    await hubContext.Clients.All.SendAsync("ServerShuttingDown");
                    WebServerConsole.Debug("  - Sent shutdown notification to game hub clients");
                }

                // Give clients a moment to disconnect gracefully
                await Task.Delay(500);
                WebServerConsole.Info($"  Completed in {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                WebServerConsole.Warn($"  Failed to notify clients: {ex.Message}");
            }

            // Step 2: Delete LIVE session from database
            stopwatch.Restart();
            WebServerConsole.Info("Step 2: Cleaning up LIVE session...");
            try
            {
                var serviceScopeFactory = _host.Services.GetService<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
                if (serviceScopeFactory != null)
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetService<MillionaireGame.Web.Data.WAPSDbContext>();
                    if (context != null)
                    {
                        var liveSession = await context.Sessions.FindAsync("LIVE");
                        if (liveSession != null)
                        {
                            context.Sessions.Remove(liveSession);
                            await context.SaveChangesAsync();
                            WebServerConsole.Info("  - Deleted LIVE session");
                        }
                    }
                }
                WebServerConsole.Info($"  Completed in {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                WebServerConsole.Warn($"  Failed to clean up LIVE session: {ex.Message}");
            }

            // Step 3: Stop the ASP.NET Core host
            stopwatch.Restart();
            WebServerConsole.Info("Step 3: Stopping ASP.NET Core host...");
            await _host.StopAsync(TimeSpan.FromSeconds(5));
            WebServerConsole.Info($"  Completed in {stopwatch.ElapsedMilliseconds}ms");

            // Step 4: Dispose resources
            stopwatch.Restart();
            WebServerConsole.Info("Step 4: Disposing host resources...");
            _host.Dispose();
            _host = null;
            _baseUrl = null;
            WebServerConsole.Info($"  Completed in {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Stop();
            WebServerConsole.Info($"=== WebServer Shutdown Complete (Total: {stopwatch.ElapsedMilliseconds}ms) ===");

            ServerStopped?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            WebServerConsole.Error($"=== WebServer Shutdown Failed after {stopwatch.ElapsedMilliseconds}ms: {ex.Message} ===");
            ServerError?.Invoke(this, ex);
            throw;
        }
    }

    /// <summary>
    /// Broadcast game state change to all connected web clients
    /// </summary>
    public async Task BroadcastGameStateAsync(string? sessionId, GameStateType state, string? message = null, object? data = null)
    {
        if (_host == null)
        {
            WebServerConsole.Warn("[WebServer] Cannot broadcast game state - server not running");
            return;
        }

        try
        {
            var hubContext = _host.Services.GetService<IHubContext<GameHub>>();
            if (hubContext == null)
            {
                WebServerConsole.Error("[WebServer] Failed to get GameHub context");
                return;
            }

            var stateData = new GameStateData
            {
                State = state,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };

            if (string.IsNullOrEmpty(sessionId))
            {
                // Broadcast to all connected clients when no session ID specified
                await hubContext.Clients.All.SendAsync("GameStateChanged", stateData);
                WebServerConsole.Debug($"[WebServer] Broadcasted state {state} to all clients");
            }
            else
            {
                // Broadcast to specific session group
                await hubContext.Clients.Group(sessionId).SendAsync("GameStateChanged", stateData);
                WebServerConsole.Debug($"[WebServer] Broadcasted state {state} to session {sessionId}");
            }
        }
        catch (Exception ex)
        {
            WebServerConsole.Error($"[WebServer] Error broadcasting game state: {ex.Message}");
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

        // Add services - include MillionaireGame.Web assembly for controllers
        services.AddControllers()
            .AddApplicationPart(typeof(MillionaireGame.Web.Controllers.HostController).Assembly);
        services.AddEndpointsApiExplorer();
        // Note: Swagger removed - not needed in embedded hosting (dev tool only)

        // Add SignalR
        services.AddSignalR();

        // Add database context - using SQL Server (same database as main application)
        services.AddDbContext<WAPSDbContext>(options =>
        {
            options.UseSqlServer(_sqlConnectionString);
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

        // Serve static files from embedded resources
        var webAssembly = typeof(MillionaireGame.Web.Hubs.GameHub).Assembly;
        var embeddedProvider = new ManifestEmbeddedFileProvider(webAssembly, "wwwroot");
        
        var fileServerOptions = new FileServerOptions
        {
            FileProvider = embeddedProvider,
            RequestPath = "",
            EnableDefaultFiles = true,
            EnableDirectoryBrowsing = false
        };
        
        // Ensure MIME types for all needed file types
        var contentTypeProvider = new FileExtensionContentTypeProvider();
        contentTypeProvider.Mappings[".js"] = "application/javascript";
        contentTypeProvider.Mappings[".css"] = "text/css";
        contentTypeProvider.Mappings[".html"] = "text/html";
        contentTypeProvider.Mappings[".json"] = "application/json";
        contentTypeProvider.Mappings[".png"] = "image/png";
        contentTypeProvider.Mappings[".jpg"] = "image/jpeg";
        contentTypeProvider.Mappings[".svg"] = "image/svg+xml";
        fileServerOptions.StaticFileOptions.ContentTypeProvider = contentTypeProvider;
        
        app.UseFileServer(fileServerOptions);

        // Use routing
        app.UseRouting();

        // Map endpoints
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHub<GameHub>("/hubs/game");

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
/// Custom logger provider that writes to WebServerConsole
/// </summary>
internal class WebServerConsoleLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new WebServerConsoleLogger(categoryName);
    }

    // ILoggerProvider interface requirement - no resources to dispose
    public void Dispose() { }
}

/// <summary>
/// Custom logger that writes to WebServerConsole
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
        
        WebServerConsole.Log($"{logPrefix} [{shortCategory}] {message}", ourLevel);

        if (exception != null)
        {
            WebServerConsole.Error($"  Exception: {exception.Message}");
        }
    }
}

