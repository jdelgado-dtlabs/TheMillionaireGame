# GitHub Crash Reporting Implementation Plan

**Status**: Planning  
**Priority**: Medium  
**Target Version**: v1.1.0  
**Created**: 2026-01-09  
**Last Updated**: 2026-01-09

---

## Overview

Implement optional GitHub crash reporting with OAuth authentication to allow users to automatically submit crash reports as GitHub issues. This will enable better bug tracking and faster issue resolution while respecting user privacy and preventing issue flooding.

## Goals

1. **User Consent First** - Never submit without explicit user permission
2. **Privacy Protection** - Sanitize all personal/sensitive data before submission
3. **Lightweight Implementation** - Minimal overhead on watchdog performance
4. **Prevent Duplicate Issues** - Implement deduplication and rate limiting
5. **Seamless UX** - One-click GitHub authentication with OAuth

## Architecture

### Authentication: GitHub OAuth App (Option 2)

**Why OAuth?**
- ✅ Better user experience (one-click "Sign in with GitHub")
- ✅ No need for users to create/manage PATs
- ✅ Minimal permissions required (`public_repo` scope for issues)
- ✅ More secure than hardcoded tokens
- ❌ Slightly more complex to implement

### Component Structure

```
Watchdog Detects Crash
    |
    v
Generate Local Crash Report (existing)
    |
    v
[NEW] CrashReportDialog (WPF/WinForms)
    - Show crash summary
    - [Review Full Report] button
    - [Submit to GitHub] [Save Locally] [Cancel] buttons
    |
    v
User Clicks "Submit to GitHub"
    |
    v
Check if GitHub authenticated
    |
    ├─> Not Authenticated -> Launch OAuth flow
    |   |
    |   └─> Open browser to GitHub OAuth
    |       User authorizes app
    |       Callback receives token
    |       Store token securely (Windows Credential Manager)
    |
    └─> Authenticated -> Continue
    |
    v
[NEW] DataSanitizer
    - Strip absolute file paths
    - Remove usernames/machine names
    - Redact sensitive environment variables
    - Anonymize database connection strings
    |
    v
[NEW] GitHubIssueSubmitter
    - Check for duplicate issues (same exit code + last 7 days)
    - Create issue with sanitized report
    - Add labels: "bug", "crash-report", "automated"
    - Include system info, exit code, sanitized logs
    |
    v
Confirmation to User
    - "Report submitted: #123"
    - Option to open issue in browser
```

## Implementation Details

### Phase 1: Core Infrastructure (Week 1)

#### 1.1 GitHub OAuth Setup
- [ ] Register GitHub OAuth App at https://github.com/settings/developers
  - **Application name**: "Millionaire Game Crash Reporter"
  - **Homepage URL**: https://github.com/Macronair/TheMillionaireGame
  - **Authorization callback URL**: `http://localhost:8888/oauth/callback`
  - **Scopes**: `public_repo` (create issues only)
- [ ] Store Client ID in application (safe to embed)
- [ ] **DO NOT** embed Client Secret (handled server-side or via localhost callback)

#### 1.2 Token Storage
- [ ] Create `SecureTokenManager` class
  - Use Windows Credential Manager (CredWrite/CredRead APIs)
  - Target name: "TheMillionaireGame_GitHub"
  - Store encrypted token with machine-specific encryption
  - Implement token validation and refresh logic

#### 1.3 Configuration Settings
Add to application settings:
```csharp
public class CrashReportingSettings
{
    public bool EnableCrashReporting { get; set; } = false;
    public bool AutoSubmitCrashes { get; set; } = false; // Future: skip dialog if enabled
    public string? GitHubUsername { get; set; } // For display only
    public DateTime? LastSubmissionTime { get; set; }
    public int SubmissionCount { get; set; } = 0;
}
```

### Phase 2: Data Sanitization (Week 1)

#### 2.1 Create DataSanitizer Class
```csharp
public class DataSanitizer
{
    // Sanitization rules
    private readonly string _userProfile;
    private readonly string _appData;
    private readonly string _machineName;
    private readonly string _username;
    
    public string SanitizeCrashReport(string rawReport)
    {
        string sanitized = rawReport;
        
        // Replace absolute paths with relative
        sanitized = ReplaceAbsolutePaths(sanitized);
        
        // Remove machine-specific identifiers
        sanitized = sanitized.Replace(_machineName, "[MACHINE]");
        sanitized = sanitized.Replace(_username, "[USER]");
        
        // Redact sensitive patterns
        sanitized = RedactConnectionStrings(sanitized);
        sanitized = RedactApiKeys(sanitized);
        
        // Remove AppData paths
        sanitized = sanitized.Replace(_appData, "%LOCALAPPDATA%");
        sanitized = sanitized.Replace(_userProfile, "%USERPROFILE%");
        
        return sanitized;
    }
    
    private string ReplaceAbsolutePaths(string text)
    {
        // Replace C:\...\TheMillionaireGame\... with [APP_DIR]\...
        // Use regex to match drive letters and absolute paths
    }
    
    private string RedactConnectionStrings(string text)
    {
        // Regex to find connection strings with passwords
        // Replace with [REDACTED]
    }
    
    private string RedactApiKeys(string text)
    {
        // Redact any potential API keys or tokens in logs
    }
}
```

#### 2.2 Sanitization Testing
- [ ] Unit tests for each sanitization method
- [ ] Test with real crash reports (manual review)
- [ ] Verify no personal data leaks

### Phase 3: GitHub Integration (Week 2)

#### 3.1 OAuth Flow Implementation
```csharp
public class GitHubOAuthManager
{
    private const string ClientId = "[FROM_GITHUB_APP]";
    private const string CallbackUrl = "http://localhost:8888/oauth/callback";
    private const string AuthUrl = "https://github.com/login/oauth/authorize";
    
    public async Task<string> AuthenticateAsync()
    {
        // 1. Generate state token for CSRF protection
        string state = GenerateSecureRandomString();
        
        // 2. Start local HTTP listener for callback
        using var listener = new HttpListener();
        listener.Prefixes.Add(CallbackUrl + "/");
        listener.Start();
        
        // 3. Open browser to GitHub OAuth
        string authUrl = $"{AuthUrl}?client_id={ClientId}&scope=public_repo&state={state}";
        Process.Start(new ProcessStartInfo(authUrl) { UseShellExecute = true });
        
        // 4. Wait for callback
        var context = await listener.GetContextAsync();
        string? code = context.Request.QueryString["code"];
        string? returnedState = context.Request.QueryString["state"];
        
        // 5. Verify state matches (CSRF protection)
        if (returnedState != state)
            throw new SecurityException("State mismatch");
        
        // 6. Exchange code for token
        // NOTE: This requires GitHub OAuth App with device flow
        // OR a proxy service to handle client secret
        string token = await ExchangeCodeForToken(code);
        
        // 7. Store token securely
        SecureTokenManager.StoreToken(token);
        
        return token;
    }
}
```

#### 3.2 Issue Submission
```csharp
public class GitHubIssueSubmitter
{
    private const string RepoOwner = "Macronair";
    private const string RepoName = "TheMillionaireGame";
    private const string ApiUrl = "https://api.github.com";
    
    public async Task<int?> SubmitCrashReportAsync(
        string sanitizedReport,
        CrashInfo crashInfo)
    {
        string token = SecureTokenManager.GetToken();
        if (string.IsNullOrEmpty(token))
            return null;
        
        // Check for recent duplicates
        if (await IsDuplicateCrash(crashInfo))
        {
            Console.WriteLine("[Watchdog] Duplicate crash detected, skipping submission");
            return null;
        }
        
        // Create issue
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        client.DefaultRequestHeaders.Add("User-Agent", "MillionaireGame-CrashReporter");
        
        var issue = new
        {
            title = GenerateIssueTitle(crashInfo),
            body = FormatIssueBody(sanitizedReport, crashInfo),
            labels = new[] { "bug", "crash-report", "automated" }
        };
        
        string json = JsonSerializer.Serialize(issue);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await client.PostAsync(
            $"{ApiUrl}/repos/{RepoOwner}/{RepoName}/issues",
            content);
        
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[Watchdog] Failed to submit issue: {response.StatusCode}");
            return null;
        }
        
        var result = await response.Content.ReadAsStringAsync();
        var issueResponse = JsonSerializer.Deserialize<JsonElement>(result);
        int issueNumber = issueResponse.GetProperty("number").GetInt32();
        
        Console.WriteLine($"[Watchdog] Crash report submitted: #{issueNumber}");
        return issueNumber;
    }
    
    private async Task<bool> IsDuplicateCrash(CrashInfo crashInfo)
    {
        // Search for existing issues with same exit code in last 7 days
        // Use GitHub search API
        // Return true if found
    }
    
    private string GenerateIssueTitle(CrashInfo crashInfo)
    {
        string exitCodeMeaning = GetExitCodeMeaning(crashInfo.ExitCode);
        string activity = crashInfo.LastActivity ?? "Unknown";
        
        return $"Crash Report: {exitCodeMeaning} (0x{crashInfo.ExitCode:X8}) - {activity}";
    }
    
    private string FormatIssueBody(string sanitizedReport, CrashInfo crashInfo)
    {
        return $@"
## Automated Crash Report

**This issue was automatically generated by the Millionaire Game crash reporter.**

### Summary
- **Exit Code**: {crashInfo.ExitCode} (0x{crashInfo.ExitCode:X8})
- **Exit Code Meaning**: {GetExitCodeMeaning(crashInfo.ExitCode)}
- **Last Activity**: {crashInfo.LastActivity ?? "Unknown"}
- **Running Time**: {crashInfo.RunningTime}
- **Was Responsive**: {(crashInfo.WasResponsive ? "Yes" : "No (frozen/hung)")}

### System Information
- **OS**: {Environment.OSVersion.VersionString}
- **Processor Count**: {Environment.ProcessorCount}
- **.NET Runtime**: {Environment.Version}

### Full Crash Report
```
{sanitizedReport}
```

---
*Please provide any additional context or steps to reproduce if possible.*
";
    }
}
```

### Phase 4: User Interface (Week 2-3)

#### 4.1 Crash Report Dialog (WinForms)
```csharp
public partial class CrashReportDialog : Form
{
    private readonly string _crashReportPath;
    private readonly CrashInfo _crashInfo;
    private readonly DataSanitizer _sanitizer;
    
    public CrashReportDialog(string crashReportPath, CrashInfo crashInfo)
    {
        InitializeComponent();
        _crashReportPath = crashReportPath;
        _crashInfo = crashInfo;
        _sanitizer = new DataSanitizer();
        
        // Load sanitized report for preview
        LoadCrashSummary();
    }
    
    private void LoadCrashSummary()
    {
        // Show sanitized summary in text box
        string rawReport = File.ReadAllText(_crashReportPath);
        string sanitized = _sanitizer.SanitizeCrashReport(rawReport);
        
        txtSummary.Text = $@"
The application crashed unexpectedly.

Exit Code: {_crashInfo.ExitCode} ({GetExitCodeMeaning(_crashInfo.ExitCode)})
Last Activity: {_crashInfo.LastActivity ?? "Unknown"}
Running Time: {_crashInfo.RunningTime}

Would you like to submit this crash report to help improve the application?
";
    }
    
    private async void btnSubmit_Click(object sender, EventArgs e)
    {
        btnSubmit.Enabled = false;
        try
        {
            // Check authentication
            if (!GitHubOAuthManager.IsAuthenticated())
            {
                // Show auth dialog
                using var authDialog = new GitHubAuthDialog();
                if (authDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
            }
            
            // Submit report
            var submitter = new GitHubIssueSubmitter();
            string rawReport = File.ReadAllText(_crashReportPath);
            string sanitized = _sanitizer.SanitizeCrashReport(rawReport);
            
            int? issueNumber = await submitter.SubmitCrashReportAsync(sanitized, _crashInfo);
            
            if (issueNumber.HasValue)
            {
                MessageBox.Show(
                    $"Crash report submitted successfully!\n\nIssue #{issueNumber}\n\n" +
                    "Thank you for helping improve the application.",
                    "Report Submitted",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                
                // Option to open in browser
                if (MessageBox.Show(
                    "Would you like to view the issue on GitHub?",
                    "View Issue",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = $"https://github.com/Macronair/TheMillionaireGame/issues/{issueNumber}",
                        UseShellExecute = true
                    });
                }
                
                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show(
                    "Failed to submit crash report. The report has been saved locally.",
                    "Submission Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        finally
        {
            btnSubmit.Enabled = true;
        }
    }
    
    private void btnReview_Click(object sender, EventArgs e)
    {
        // Show full sanitized report in modal
        string rawReport = File.ReadAllText(_crashReportPath);
        string sanitized = _sanitizer.SanitizeCrashReport(rawReport);
        
        using var reviewDialog = new ReviewReportDialog(sanitized);
        reviewDialog.ShowDialog();
    }
    
    private void btnSaveLocally_Click(object sender, EventArgs e)
    {
        // Already saved, just close
        MessageBox.Show(
            $"Crash report saved to:\n{_crashReportPath}",
            "Report Saved",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        
        DialogResult = DialogResult.Cancel;
    }
}
```

#### 4.2 Settings Integration
Add to Settings dialog:
- [ ] Checkbox: "Enable crash reporting"
- [ ] Button: "Sign in with GitHub" / "Sign out"
- [ ] Label: Show connected GitHub username
- [ ] Info text: Explain data privacy and what gets submitted

### Phase 5: Integration with Watchdog (Week 3)

#### 5.1 Update ProcessMonitor
```csharp
private async Task HandleCrashAsync(CrashInfo crashInfo)
{
    // Generate crash report (existing)
    string reportPath = _crashReportGenerator.GenerateReport(crashInfo);
    
    // NEW: Check if crash reporting is enabled
    var settings = LoadCrashReportingSettings();
    if (settings.EnableCrashReporting)
    {
        // Show crash report dialog (on UI thread)
        // NOTE: Watchdog is console app, may need WinForms reference
        Application.EnableVisualStyles();
        using var dialog = new CrashReportDialog(reportPath, crashInfo);
        dialog.ShowDialog();
    }
    else
    {
        Console.WriteLine($"[Watchdog] Crash detected. Report saved: {reportPath}");
        Console.WriteLine("[Watchdog] Crash reporting is disabled. Enable in settings to submit reports.");
    }
}
```

### Phase 6: Security & Privacy (Week 4)

#### 6.1 Security Measures
- [ ] Implement rate limiting (max 5 submissions per day)
- [ ] Add submission cooldown (min 1 hour between identical crashes)
- [ ] Token expiration handling and refresh
- [ ] Secure token storage with Windows Credential Manager
- [ ] CSRF protection in OAuth flow

#### 6.2 Privacy Measures
- [ ] Clear privacy policy in dialog
- [ ] Option to review sanitized report before submission
- [ ] Explicit consent checkbox
- [ ] Ability to disable/revoke at any time
- [ ] No telemetry beyond crash reports

#### 6.3 Data Sanitization Checklist
- [ ] Remove absolute file paths
- [ ] Remove machine names
- [ ] Remove usernames
- [ ] Remove AppData paths
- [ ] Redact connection strings
- [ ] Redact API keys/tokens
- [ ] Remove IP addresses (if any)
- [ ] Remove email addresses (if any)

### Phase 7: Testing & Validation (Week 4)

#### 7.1 Unit Tests
- [ ] DataSanitizer tests (verify all PII removal)
- [ ] GitHubOAuthManager tests (mock OAuth flow)
- [ ] GitHubIssueSubmitter tests (mock API calls)
- [ ] Duplicate detection tests

#### 7.2 Integration Tests
- [ ] End-to-end OAuth flow
- [ ] Issue submission with real GitHub
- [ ] Duplicate detection with real issues
- [ ] Token storage and retrieval

#### 7.3 Manual Testing
- [ ] Test crash detection → dialog flow
- [ ] Test OAuth authentication
- [ ] Test issue submission
- [ ] Verify sanitization (manually review submitted issues)
- [ ] Test offline scenarios (no network)
- [ ] Test revocation/disable

## Known Issues to Fix

### Issue #1: Log Path in CrashReportGenerator
**Problem**: Crash handler looks for logs in wrong location
- **Current**: `AppDomain.CurrentDomain.BaseDirectory\GameConsole.log`
- **Actual**: `%LOCALAPPDATA%\TheMillionaireGame\Logs\GameConsole_*.log`

**Fix Required**: Update CrashReportGenerator.cs line 81-94 to use correct log path

## Performance Considerations

### Watchdog Performance
- OAuth flow runs **after** crash detection (non-blocking)
- Dialog is shown **after** crash report generation
- HTTP requests are async and timeout after 10 seconds
- Duplicate checking uses minimal GitHub API calls (search with filters)
- No impact on main application performance (watchdog is separate process)

### Rate Limiting
- Local rate limit: Max 5 submissions per day
- Cooldown: Min 1 hour between identical crashes (same exit code)
- Duplicate detection: Check last 7 days of issues

## Documentation Updates

- [ ] Update CRASH_HANDLER_DOCUMENTATION.md with GitHub reporting info
- [ ] Create user guide for enabling crash reporting
- [ ] Document OAuth setup for contributors
- [ ] Privacy policy section in README
- [ ] FAQ section about what data is submitted

## Dependencies

New NuGet packages required:
- `System.Text.Json` (already in project)
- `System.Security.Cryptography` (built-in)
- No external GitHub libraries needed (using direct HTTP API)

## Rollout Strategy

### Phase 1: Developer Testing (v1.1.0-beta)
- Internal testing only
- Verify sanitization works
- Test with real crashes

### Phase 2: Opt-in Beta (v1.1.0-rc)
- Release to beta testers
- Collect feedback on UX
- Monitor for privacy issues

### Phase 3: Public Release (v1.1.0)
- Default disabled (opt-in)
- Clear onboarding in settings
- Documentation published

## Success Metrics

- Number of users who enable crash reporting
- Number of crash reports submitted
- Duplicate detection rate (% of prevented duplicates)
- Issue resolution time improvement
- User feedback on privacy concerns

## Alternatives Considered

### Option 1: GitHub Personal Access Token
- ❌ Worse UX (users must create token manually)
- ❌ Security risk (users might use overprivileged tokens)
- ✅ Simpler implementation

### Option 3: Anonymous Submission Service
- ✅ No user GitHub account needed
- ✅ Most private (no user identity)
- ❌ Requires hosting infrastructure
- ❌ Monthly costs
- ❌ Maintenance burden

## Open Questions

1. **Should we include screenshots?**
   - Pro: More context for visual bugs
   - Con: Privacy concerns, larger uploads
   - **Decision**: No for v1.1.0, consider for v1.2.0

2. **Should auto-submit be an option?**
   - Pro: More data collection
   - Con: Privacy concerns
   - **Decision**: Add setting but default disabled

3. **Should we deduplicate based on stack traces?**
   - Pro: Better duplicate detection
   - Con: Exit codes may not have stack traces
   - **Decision**: Use exit code + activity for now

## Timeline

- **Week 1**: Core infrastructure + data sanitization
- **Week 2**: GitHub integration + UI
- **Week 3**: Watchdog integration + testing
- **Week 4**: Security review + documentation
- **Week 5**: Beta testing
- **Week 6**: Public release

**Total Estimated Time**: 6 weeks

---

## Next Steps

1. Fix log path issue in CrashReportGenerator
2. Register GitHub OAuth App
3. Begin Phase 1 implementation
