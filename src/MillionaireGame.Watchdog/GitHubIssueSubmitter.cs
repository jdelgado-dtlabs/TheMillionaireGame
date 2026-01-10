using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MillionaireGame.Watchdog
{
    /// <summary>
    /// Handles GitHub API integration for crash report submission
    /// Creates issues, checks for duplicates, and formats crash reports
    /// </summary>
    public class GitHubIssueSubmitter
    {
        private const string RepoOwner = "jdelgado-dtlabs";
        private const string RepoName = "TheMillionaireGame";
        private const string ApiUrl = "https://api.github.com";
        
        private readonly HttpClient _httpClient;
        private readonly GitHubOAuthManager _oauthManager;

        public GitHubIssueSubmitter()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "TheMillionaireGame-CrashReporter/1.0");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            _oauthManager = new GitHubOAuthManager();
        }

        /// <summary>
        /// Submits a crash report to GitHub as an issue
        /// </summary>
        /// <param name="crashInfo">Information about the crash</param>
        /// <param name="userContext">User-provided context and preferences</param>
        /// <param name="sanitizedReport">The sanitized crash report content</param>
        /// <returns>Result of the submission attempt</returns>
        public async Task<SubmissionResult> SubmitCrashReportAsync(
            CrashInfo crashInfo,
            UserCrashContext userContext,
            string sanitizedReport)
        {
            try
            {
                WatchdogConsole.Info("[GitHubIssue] Starting crash report submission...");

                // Step 1: Check authentication
                string? token = _oauthManager.GetAccessToken();
                if (string.IsNullOrEmpty(token))
                {
                    return new SubmissionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Not authenticated with GitHub"
                    };
                }

                // Step 2: Check for duplicate crashes
                var duplicate = await CheckForDuplicateAsync(crashInfo, token);
                if (duplicate.IsDuplicate)
                {
                    WatchdogConsole.Info($"[GitHubIssue] Duplicate crash detected: Issue #{duplicate.ExistingIssueNumber}");
                    return duplicate;
                }

                // Step 3: Format issue title and body
                string title = GenerateIssueTitle(crashInfo);
                string body = FormatIssueBody(crashInfo, userContext, sanitizedReport);

                // Step 4: Create the GitHub issue
                var issueRequest = new
                {
                    title,
                    body,
                    labels = new[] { "bug", "crash-report", "automated" }
                };

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync(
                    $"{ApiUrl}/repos/{RepoOwner}/{RepoName}/issues",
                    issueRequest);

                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    WatchdogConsole.Error($"[GitHubIssue] Failed to create issue: {response.StatusCode} - {error}");
                    return new SubmissionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"GitHub API error: {response.StatusCode}"
                    };
                }

                // Step 5: Parse response
                var result = await response.Content.ReadFromJsonAsync<GitHubIssueResponse>();
                if (result == null)
                {
                    return new SubmissionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Failed to parse GitHub response"
                    };
                }

                WatchdogConsole.Info($"[GitHubIssue] Successfully created issue #{result.Number}");
                return new SubmissionResult
                {
                    IsSuccess = true,
                    IssueNumber = result.Number,
                    IssueUrl = result.HtmlUrl
                };
            }
            catch (Exception ex)
            {
                WatchdogConsole.Error($"[GitHubIssue] Submission failed: {ex.Message}");
                return new SubmissionResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Submission error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Checks if a similar crash has been reported recently
        /// </summary>
        private async Task<SubmissionResult> CheckForDuplicateAsync(CrashInfo crashInfo, string token)
        {
            try
            {
                // Search for issues with the same exit code in the last 7 days
                string exitCodeHex = $"0x{crashInfo.ExitCode:X8}";
                string query = $"repo:{RepoOwner}/{RepoName} is:issue is:open label:crash-report {exitCodeHex} in:title";
                
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var searchUrl = $"{ApiUrl}/search/issues?q={Uri.EscapeDataString(query)}&sort=created&order=desc&per_page=5";
                var response = await _httpClient.GetAsync(searchUrl);

                if (!response.IsSuccessStatusCode)
                {
                    WatchdogConsole.Warn($"[GitHubIssue] Could not check for duplicates: {response.StatusCode}");
                    // Don't fail submission if we can't check - just proceed
                    return new SubmissionResult { IsDuplicate = false };
                }

                var searchResult = await response.Content.ReadFromJsonAsync<GitHubSearchResponse>();
                if (searchResult == null || searchResult.TotalCount == 0)
                {
                    return new SubmissionResult { IsDuplicate = false };
                }

                // Check if any recent issues match (within 7 days)
                var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
                foreach (var item in searchResult.Items)
                {
                    if (DateTime.TryParse(item.CreatedAt, out var createdDate) && 
                        createdDate > sevenDaysAgo)
                    {
                        WatchdogConsole.Info($"[GitHubIssue] Found duplicate: Issue #{item.Number}");
                        return new SubmissionResult
                        {
                            IsSuccess = false,
                            IsDuplicate = true,
                            ExistingIssueNumber = item.Number,
                            IssueUrl = item.HtmlUrl,
                            ErrorMessage = $"A similar crash was already reported in issue #{item.Number}"
                        };
                    }
                }

                return new SubmissionResult { IsDuplicate = false };
            }
            catch (Exception ex)
            {
                WatchdogConsole.Warn($"[GitHubIssue] Error checking duplicates: {ex.Message}");
                // Don't fail submission if duplicate check fails
                return new SubmissionResult { IsDuplicate = false };
            }
        }

        /// <summary>
        /// Generates a concise, informative issue title
        /// </summary>
        private string GenerateIssueTitle(CrashInfo crashInfo)
        {
            string exitCodeMeaning = crashInfo.ExitCodeMeaning ?? 
                                     ProcessMonitor.GetExitCodeMeaning(crashInfo.ExitCode);
            string activity = crashInfo.LastActivity ?? "Unknown Activity";
            
            // Truncate activity if too long
            if (activity.Length > 50)
                activity = activity.Substring(0, 47) + "...";
            
            return $"Crash: {exitCodeMeaning} (0x{crashInfo.ExitCode:X8}) during {activity}";
        }

        /// <summary>
        /// Formats the complete issue body with crash details
        /// </summary>
        private string FormatIssueBody(
            CrashInfo crashInfo,
            UserCrashContext userContext,
            string sanitizedReport)
        {
            var body = new StringBuilder();
            
            body.AppendLine("## 🚨 Automated Crash Report");
            body.AppendLine();
            body.AppendLine("*This issue was automatically generated by the Millionaire Game crash reporter.*");
            body.AppendLine();
            
            // User-provided description
            if (!string.IsNullOrWhiteSpace(userContext.Description))
            {
                body.AppendLine("### 📝 What Happened");
                body.AppendLine();
                body.AppendLine($"> {userContext.Description}");
                body.AppendLine();
            }
            
            // Reproduction steps
            if (!string.IsNullOrWhiteSpace(userContext.ReproductionSteps))
            {
                body.AppendLine("### 🔁 Steps to Reproduce");
                body.AppendLine();
                body.AppendLine(userContext.ReproductionSteps);
                body.AppendLine();
            }
            
            // Crash summary
            body.AppendLine("### 💥 Crash Summary");
            body.AppendLine();
            body.AppendLine("| Property | Value |");
            body.AppendLine("|----------|-------|");
            body.AppendLine($"| **Exit Code** | `{crashInfo.ExitCode}` (`0x{crashInfo.ExitCode:X8}`) |");
            body.AppendLine($"| **Exit Code Meaning** | {crashInfo.ExitCodeMeaning ?? ProcessMonitor.GetExitCodeMeaning(crashInfo.ExitCode)} |");
            body.AppendLine($"| **Last Activity** | {crashInfo.LastActivity ?? "Unknown"} |");
            body.AppendLine($"| **Running Time** | {FormatTimeSpan(crashInfo.RunningTime)} |");
            body.AppendLine($"| **Was Responsive** | {(crashInfo.WasResponsive ? "✅ Yes" : "❌ No (frozen/hung)")} |");
            body.AppendLine($"| **Crash Time** | {crashInfo.CrashTime:yyyy-MM-dd HH:mm:ss UTC} |");
            
            if (!string.IsNullOrWhiteSpace(crashInfo.AppVersion))
            {
                body.AppendLine($"| **App Version** | {crashInfo.AppVersion} |");
            }
            body.AppendLine();
            
            // System information
            if (userContext.IncludeSystemInfo)
            {
                body.AppendLine("### 💻 System Information");
                body.AppendLine();
                var systemInfo = DataSanitizer.GetSanitizedSystemInfo();
                foreach (var kvp in systemInfo)
                {
                    body.AppendLine($"- **{kvp.Key}:** {kvp.Value}");
                }
                body.AppendLine();
            }
            
            // Sanitized logs
            if (userContext.IncludeLogs && !string.IsNullOrWhiteSpace(sanitizedReport))
            {
                body.AppendLine("### 📄 Crash Report (Sanitized)");
                body.AppendLine();
                body.AppendLine("<details>");
                body.AppendLine("<summary>Click to expand full crash report</summary>");
                body.AppendLine();
                body.AppendLine("```");
                // Limit report size to prevent huge issues (max 10KB)
                if (sanitizedReport.Length > 10000)
                {
                    body.AppendLine(sanitizedReport.Substring(0, 10000));
                    body.AppendLine();
                    body.AppendLine("... (truncated for length)");
                }
                else
                {
                    body.AppendLine(sanitizedReport);
                }
                body.AppendLine("```");
                body.AppendLine();
                body.AppendLine("</details>");
                body.AppendLine();
            }
            
            // Contact information
            if (!string.IsNullOrWhiteSpace(userContext.Email))
            {
                body.AppendLine("---");
                body.AppendLine();
                body.AppendLine($"📧 **Contact for follow-up:** {userContext.Email}");
                body.AppendLine();
            }
            
            body.AppendLine("---");
            body.AppendLine();
            body.AppendLine("*🙏 Thank you for helping improve the Millionaire Game!*");
            body.AppendLine();
            body.AppendLine("*Note: All personal information has been automatically removed from this report.*");
            
            return body.ToString();
        }

        private static string FormatTimeSpan(TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
                return $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s";
            else if (ts.TotalMinutes >= 1)
                return $"{ts.Minutes}m {ts.Seconds}s";
            else
                return $"{ts.Seconds}s";
        }
    }

    #region GitHub API Response Models

    public class GitHubIssueResponse
    {
        [JsonPropertyName("number")]
        public int Number { get; set; }
        
        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
    }

    public class GitHubSearchResponse
    {
        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }
        
        [JsonPropertyName("items")]
        public GitHubIssueItem[] Items { get; set; } = Array.Empty<GitHubIssueItem>();
    }

    public class GitHubIssueItem
    {
        [JsonPropertyName("number")]
        public int Number { get; set; }
        
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        
        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;
        
        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;
    }

    #endregion
}
