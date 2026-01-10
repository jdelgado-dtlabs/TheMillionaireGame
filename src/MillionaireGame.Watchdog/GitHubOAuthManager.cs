using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MillionaireGame.Watchdog;

/// <summary>
/// Manages GitHub OAuth authentication using device flow
/// User authorizes the app via browser, and we receive a token
/// </summary>
public class GitHubOAuthManager
{
    private const string ClientId = "Ov23liYOUR_CLIENT_ID_HERE"; // TODO: Replace with actual Client ID from GitHub OAuth App
    private const string DeviceCodeUrl = "https://github.com/login/device/code";
    private const string AccessTokenUrl = "https://github.com/login/oauth/access_token";
    private const string AuthorizeUrl = "https://github.com/login/device";
    
    private readonly HttpClient _httpClient;
    private readonly SecureTokenManager _tokenManager;
    private DeviceCodeResponse? _currentDeviceCodeResponse; // Store for UI access
    
    public GitHubOAuthManager()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "TheMillionaireGame-CrashReporter/1.0");
        _tokenManager = new SecureTokenManager();
    }
    
    /// <summary>
    /// Checks if the user is authenticated with a valid token
    /// </summary>
    public bool IsAuthenticated()
    {
        return SecureTokenManager.HasToken();
    }
    
    /// <summary>
    /// Gets the current device code response for UI display
    /// </summary>
    public DeviceCodeResponse? GetDeviceCodeResponse()
    {
        return _currentDeviceCodeResponse;
    }
    
    /// <summary>
    /// Initiates the OAuth device flow authentication process
    /// Opens browser for user to authorize, then polls for token
    /// </summary>
    /// <returns>AuthResult with success status and any error message</returns>
    public async Task<AuthResult> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            WatchdogConsole.Info("[GitHubOAuth] Starting device flow authentication...");
            
            // Step 1: Request device and user codes
            var deviceCodeResponse = await RequestDeviceCodeAsync(cancellationToken);
            if (deviceCodeResponse == null)
            {
                return AuthResult.Failure("Failed to request device code from GitHub");
            }
            
            WatchdogConsole.Info($"[GitHubOAuth] Device code received. User code: {deviceCodeResponse.UserCode}");
            WatchdogConsole.Info($"[GitHubOAuth] Opening browser to: {deviceCodeResponse.VerificationUri}");
            
            // Step 2: Open browser for user authorization
            OpenBrowser(deviceCodeResponse.VerificationUri);
            
            // Step 3: Poll for access token
            var token = await PollForAccessTokenAsync(
                deviceCodeResponse.DeviceCode,
                deviceCodeResponse.Interval,
                deviceCodeResponse.ExpiresIn,
                cancellationToken);
            
            if (string.IsNullOrEmpty(token))
            {
                return AuthResult.Failure("Authentication timed out or was denied");
            }
            
            // Step 4: Store token securely
            if (!SecureTokenManager.StoreToken(token))
            {
                return AuthResult.Failure("Failed to securely store authentication token");
            }
            
            WatchdogConsole.Info("[GitHubOAuth] Authentication successful!");
            return AuthResult.Success();
        }
        catch (Exception ex)
        {
            WatchdogConsole.Error($"[GitHubOAuth] Authentication failed: {ex.Message}");
            return AuthResult.Failure($"Authentication error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Gets the stored access token, or null if not authenticated
    /// </summary>
    public string? GetAccessToken()
    {
        return SecureTokenManager.RetrieveToken();
    }
    
    /// <summary>
    /// Clears the stored authentication token
    /// </summary>
    public void Logout()
    {
        SecureTokenManager.DeleteToken();
        WatchdogConsole.Info("[GitHubOAuth] User logged out");
    }
    
    private async Task<DeviceCodeResponse?> RequestDeviceCodeAsync(CancellationToken cancellationToken)
    {
        try
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("scope", "public_repo") // Only need to create issues
            });
            
            var response = await _httpClient.PostAsync(DeviceCodeUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            _currentDeviceCodeResponse = await response.Content.ReadFromJsonAsync<DeviceCodeResponse>(cancellationToken);
            return _currentDeviceCodeResponse;
        }
        catch (Exception ex)
        {
            WatchdogConsole.Error($"[GitHubOAuth] Failed to request device code: {ex.Message}");
            return null;
        }
    }
    
    private async Task<string?> PollForAccessTokenAsync(
        string deviceCode,
        int intervalSeconds,
        int expiresInSeconds,
        CancellationToken cancellationToken)
    {
        var expirationTime = DateTime.UtcNow.AddSeconds(expiresInSeconds);
        var pollInterval = TimeSpan.FromSeconds(intervalSeconds);
        
        while (DateTime.UtcNow < expirationTime)
        {
            if (cancellationToken.IsCancellationRequested)
                return null;
            
            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", ClientId),
                    new KeyValuePair<string, string>("device_code", deviceCode),
                    new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:device_code")
                });
                
                var response = await _httpClient.PostAsync(AccessTokenUrl, content, cancellationToken);
                var tokenResponse = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(cancellationToken);
                
                if (tokenResponse == null)
                {
                    WatchdogConsole.Warn("[GitHubOAuth] Received null token response");
                    await Task.Delay(pollInterval, cancellationToken);
                    continue;
                }
                
                // Check for errors
                if (!string.IsNullOrEmpty(tokenResponse.Error))
                {
                    switch (tokenResponse.Error)
                    {
                        case "authorization_pending":
                            // User hasn't authorized yet, keep polling
                            WatchdogConsole.Debug("[GitHubOAuth] Authorization pending...");
                            break;
                        
                        case "slow_down":
                            // We're polling too fast, increase interval
                            pollInterval = pollInterval.Add(TimeSpan.FromSeconds(5));
                            WatchdogConsole.Warn($"[GitHubOAuth] Slowing down poll interval to {pollInterval.TotalSeconds}s");
                            break;
                        
                        case "expired_token":
                            WatchdogConsole.Error("[GitHubOAuth] Device code expired");
                            return null;
                        
                        case "access_denied":
                            WatchdogConsole.Error("[GitHubOAuth] User denied authorization");
                            return null;
                        
                        default:
                            WatchdogConsole.Error($"[GitHubOAuth] Unknown error: {tokenResponse.Error}");
                            return null;
                    }
                }
                else if (!string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    // Success!
                    return tokenResponse.AccessToken;
                }
                
                await Task.Delay(pollInterval, cancellationToken);
            }
            catch (Exception ex)
            {
                WatchdogConsole.Error($"[GitHubOAuth] Error polling for token: {ex.Message}");
                await Task.Delay(pollInterval, cancellationToken);
            }
        }
        
        WatchdogConsole.Error("[GitHubOAuth] Authentication timed out");
        return null;
    }
    
    private static void OpenBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            WatchdogConsole.Error($"[GitHubOAuth] Failed to open browser: {ex.Message}");
            WatchdogConsole.Info($"[GitHubOAuth] Please manually navigate to: {url}");
        }
    }
}

#region Response Models

public class DeviceCodeResponse
{
    [JsonPropertyName("device_code")]
    public string DeviceCode { get; set; } = string.Empty;
    
    [JsonPropertyName("user_code")]
    public string UserCode { get; set; } = string.Empty;
    
    [JsonPropertyName("verification_uri")]
    public string VerificationUri { get; set; } = string.Empty;
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    
    [JsonPropertyName("interval")]
    public int Interval { get; set; } = 5; // Default to 5 seconds
}

public class AccessTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
    
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
    
    [JsonPropertyName("error")]
    public string? Error { get; set; }
    
    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }
}

public class AuthResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    
    public static AuthResult Success() => new() { IsSuccess = true };
    public static AuthResult Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
}

#endregion
