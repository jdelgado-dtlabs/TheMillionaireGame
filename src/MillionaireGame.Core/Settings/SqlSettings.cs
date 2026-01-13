using System.Xml.Serialization;

namespace MillionaireGame.Core.Settings;

/// <summary>
/// SQL Server connection settings
/// </summary>
[XmlRoot("SQLInfo")]
public class SqlConnectionSettings
{
    public bool UseRemoteServer { get; set; }
    public bool UseLocalDB { get; set; }
    public string LocalInstance { get; set; } = "SQLEXPRESS";
    public string RemoteServer { get; set; } = string.Empty;
    public int RemotePort { get; set; } = 1433;
    public string RemoteDatabase { get; set; } = string.Empty;
    public string RemoteLogin { get; set; } = string.Empty;
    public string RemotePassword { get; set; } = string.Empty;
    public bool HideAtStart { get; set; }

    /// <summary>
    /// Gets the connection string based on current settings (without database name)
    /// </summary>
    public string GetConnectionString()
    {
        if (UseRemoteServer)
        {
            return $"Server={RemoteServer},{RemotePort};User Id={RemoteLogin};Password={RemotePassword};TrustServerCertificate=True;";
        }
        else if (UseLocalDB)
        {
            return "Server=(LocalDB)\\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=True;";
        }
        else
        {
            return $"Server=localhost\\{LocalInstance};Trusted_Connection=true;TrustServerCertificate=True;";
        }
    }

    /// <summary>
    /// Gets the full connection string including the database name
    /// </summary>
    /// <param name="databaseName">Name of the database to connect to</param>
    public string GetConnectionString(string databaseName)
    {
        if (UseRemoteServer)
        {
            return $"Server={RemoteServer},{RemotePort};Database={databaseName};User Id={RemoteLogin};Password={RemotePassword};TrustServerCertificate=True;";
        }
        else if (UseLocalDB)
        {
            return $"Server=(LocalDB)\\MSSQLLocalDB;Database={databaseName};Integrated Security=true;TrustServerCertificate=True;";
        }
        else
        {
            return $"Server=localhost\\{LocalInstance};Database={databaseName};Trusted_Connection=true;TrustServerCertificate=True;";
        }
    }
}

/// <summary>
/// Manager for SQL settings persistence
/// </summary>
public class SqlSettingsManager
{
    private const string FileName = "sql.xml";
    private readonly string _filePath;

    public SqlConnectionSettings Settings { get; set; }

    public SqlSettingsManager(string? basePath = null)
    {
        // Use LocalAppData folder to avoid permission issues (consistent with logs/telemetry)
        if (basePath == null)
        {
            var appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TheMillionaireGame");
            Directory.CreateDirectory(appDataFolder); // Ensure directory exists
            basePath = appDataFolder;
        }
        _filePath = Path.Combine(basePath, FileName);
        Settings = new SqlConnectionSettings();
    }

    public void LoadSettings()
    {
        if (!File.Exists(_filePath))
        {
            SaveDefaultSettings();
            return;
        }

        try
        {
            var serializer = new XmlSerializer(typeof(SqlConnectionSettings));
            using var reader = new StreamReader(_filePath);
            var loadedSettings = (SqlConnectionSettings?)serializer.Deserialize(reader);
            if (loadedSettings != null)
            {
                Settings = loadedSettings;
            }
        }
        catch
        {
            // Use default settings on load error
            SaveDefaultSettings();
        }
    }

    public void SaveSettings()
    {
        try
        {
            var serializer = new XmlSerializer(typeof(SqlConnectionSettings));
            using var writer = new StreamWriter(_filePath);
            serializer.Serialize(writer, Settings);
        }
        catch
        {
            // Re-throw to caller
            throw;
        }
    }

    private void SaveDefaultSettings()
    {
        Settings = new SqlConnectionSettings
        {
            UseRemoteServer = false,
            UseLocalDB = false,
            LocalInstance = "SQLEXPRESS",
            RemoteServer = string.Empty,
            RemotePort = 1433,
            RemoteDatabase = string.Empty,
            RemoteLogin = string.Empty,
            RemotePassword = string.Empty,
            HideAtStart = false
        };

        SaveSettings();
    }
}
