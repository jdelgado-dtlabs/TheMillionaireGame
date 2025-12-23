using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using MillionaireGame.Web.Data;
using MillionaireGame.Web.Database;
using MillionaireGame.Web.Hubs;
using MillionaireGame.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure to work behind reverse proxy (Nginx)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR
builder.Services.AddSignalR();

// Add database context
builder.Services.AddDbContext<WAPSDbContext>(options =>
{
    var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "waps.db");
    options.UseSqlite($"Data Source={dbPath}");
});

// Get SQL Server connection string for FFF questions
var sqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=(local);Database=dbMillionaire;Integrated Security=true;TrustServerCertificate=true;";

// Add repositories and services
builder.Services.AddScoped<FFFQuestionRepository>(provider => new FFFQuestionRepository(sqlConnectionString));
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<FFFService>();
builder.Services.AddScoped<NameValidationService>();
builder.Services.AddScoped<StatisticsService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WAPSDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use forwarded headers from reverse proxy (MUST be before other middleware)
app.UseForwardedHeaders();

app.UseCors("AllowAll");

// Serve static files (wwwroot) - order matters!
app.UseDefaultFiles();  // Must be before UseStaticFiles
app.UseStaticFiles();

// Map controllers
app.MapControllers();

// Map SignalR hubs
app.MapHub<FFFHub>("/hubs/fff");
app.MapHub<ATAHub>("/hubs/ata");

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Service = "WAPS (Web-Based Audience Participation System)",
    Timestamp = DateTime.UtcNow
}));

app.Run();
