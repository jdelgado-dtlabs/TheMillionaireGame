# Web Server Testing Script
# Tests embedded ASP.NET Core web server functionality

Write-Host "=== Millionaire Game Web Server Test ===" -ForegroundColor Cyan
Write-Host ""

# Configuration
$baseUrl = "http://localhost:5278"
$timeout = 5

function Test-Endpoint {
    param(
        [string]$Url,
        [string]$Description,
        [int]$ExpectedStatus = 200
    )
    
    Write-Host "Testing: $Description" -ForegroundColor Yellow
    Write-Host "  URL: $Url"
    
    try {
        $response = Invoke-WebRequest -Uri $Url -Method Get -TimeoutSec $timeout -UseBasicParsing -ErrorAction Stop
        
        if ($response.StatusCode -eq $ExpectedStatus) {
            Write-Host "  ✅ SUCCESS - Status: $($response.StatusCode)" -ForegroundColor Green
            Write-Host "  Content-Type: $($response.Headers['Content-Type'])"
            Write-Host "  Content-Length: $($response.Content.Length) bytes"
            return $true
        } else {
            Write-Host "  ❌ FAILED - Status: $($response.StatusCode) (Expected: $ExpectedStatus)" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "  ❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    finally {
        Write-Host ""
    }
}

function Test-ApiEndpoint {
    param(
        [string]$Url,
        [string]$Description,
        [string]$Method = "GET",
        [hashtable]$Body = $null
    )
    
    Write-Host "Testing API: $Description" -ForegroundColor Yellow
    Write-Host "  URL: $Url"
    Write-Host "  Method: $Method"
    
    try {
        $params = @{
            Uri = $Url
            Method = $Method
            TimeoutSec = $timeout
            ErrorAction = "Stop"
            ContentType = "application/json"
            UseBasicParsing = $true
        }
        
        if ($Body -ne $null) {
            $params.Body = ($Body | ConvertTo-Json)
        }
        
        $response = Invoke-RestMethod @params
        
        Write-Host "  ✅ SUCCESS - Response received" -ForegroundColor Green
        Write-Host "  Response: $($response | ConvertTo-Json -Compress)"
        return $true
    }
    catch {
        Write-Host "  ❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    finally {
        Write-Host ""
    }
}

# Test Results
$results = @{
    Passed = 0
    Failed = 0
    Total = 0
}

function Record-Result {
    param([bool]$Success)
    $results.Total++
    if ($Success) { $results.Passed++ } else { $results.Failed++ }
}

Write-Host "Starting tests..." -ForegroundColor Cyan
Write-Host "Base URL: $baseUrl" -ForegroundColor Cyan
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Check if application is running
Write-Host "Test 1: Application Process" -ForegroundColor Yellow
$process = Get-Process -Name "MillionaireGame" -ErrorAction SilentlyContinue
if ($process) {
    Write-Host "  ✅ MillionaireGame.exe is running (PID: $($process.Id))" -ForegroundColor Green
    Write-Host "  Memory: $([math]::Round($process.WorkingSet64 / 1MB, 2)) MB"
    Write-Host "  Start Time: $($process.StartTime)"
    Record-Result $true
} else {
    Write-Host "  ❌ MillionaireGame.exe is NOT running" -ForegroundColor Red
    Write-Host "  Please start the application first!" -ForegroundColor Red
    Record-Result $false
    exit 1
}
Write-Host ""

# Test 2: Static File - Landing Page
Record-Result (Test-Endpoint -Url "$baseUrl/" -Description "Landing Page (index.html)")

# Test 3: Health Check Endpoint
Record-Result (Test-Endpoint -Url "$baseUrl/health" -Description "Health Check Endpoint")

# Test 4: API - Get Random FFF Question
Record-Result (Test-ApiEndpoint -Url "$baseUrl/api/fff/random" -Description "Get Random FFF Question API")

# Test 5: API - Get Session (should return 404 if no session)
Write-Host "Testing API: Get Session" -ForegroundColor Yellow
Write-Host "  URL: $baseUrl/api/session/LIVE"
Write-Host "  Note: May return 404 if no active session"
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/session/LIVE" -Method Get -TimeoutSec $timeout -ErrorAction Stop
    Write-Host "  ✅ Session API accessible - Active session found" -ForegroundColor Green
    Record-Result $true
} catch {
    if ($_.Exception.Response.StatusCode.value__ -eq 404) {
        Write-Host "  ⚠️  404 Not Found - No active session (this is expected)" -ForegroundColor Yellow
        Record-Result $true
    } else {
        Write-Host "  ❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
        Record-Result $false
    }
}
Write-Host ""

# Test 6: SignalR FFF Hub Endpoint
Write-Host "Testing: SignalR FFF Hub Endpoint" -ForegroundColor Yellow
Write-Host "  URL: $baseUrl/hubs/fff"
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/hubs/fff/negotiate?negotiateVersion=1" -Method Post -TimeoutSec $timeout -UseBasicParsing -ErrorAction Stop
    Write-Host "  ✅ FFF hub is accessible (Status: $($response.StatusCode))" -ForegroundColor Green
    Record-Result $true
} catch {
    Write-Host "  ❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Record-Result $false
}
Write-Host ""

# Test 7: SignalR ATA Hub Endpoint
Write-Host "Testing: SignalR ATA Hub Endpoint" -ForegroundColor Yellow
Write-Host "  URL: $baseUrl/hubs/ata"
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/hubs/ata/negotiate?negotiateVersion=1" -Method Post -TimeoutSec $timeout -UseBasicParsing -ErrorAction Stop
    Write-Host "  ✅ ATA hub is accessible (Status: $($response.StatusCode))" -ForegroundColor Green
    Record-Result $true
} catch {
    Write-Host "  ❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Record-Result $false
}
Write-Host ""

# Test 9: Database File Check
Write-Host "Test: Database File" -ForegroundColor Yellow
$dbPath = "C:\Users\djtam\OneDrive\Documents\Coding\Project\Millionaire\TheMillionaireGame\src\MillionaireGame\bin\Debug\net8.0-windows\waps.db"
if (Test-Path $dbPath) {
    $dbFile = Get-Item $dbPath
    Write-Host "  ✅ waps.db exists" -ForegroundColor Green
    Write-Host "  Location: $dbPath"
    Write-Host "  Size: $([math]::Round($dbFile.Length / 1KB, 2)) KB"
    Write-Host "  Modified: $($dbFile.LastWriteTime)"
    Record-Result $true
} else {
    Write-Host "  ⚠️  waps.db not found (will be created on first session)" -ForegroundColor Yellow
    Write-Host "  Expected location: $dbPath"
    Record-Result $true  # Not a failure, just not created yet
}
Write-Host ""

# Summary
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "TEST SUMMARY" -ForegroundColor Cyan
Write-Host "  Total Tests: $($results.Total)" -ForegroundColor White
Write-Host "  Passed: $($results.Passed)" -ForegroundColor Green
Write-Host "  Failed: $($results.Failed)" -ForegroundColor Red
Write-Host ""

if ($results.Failed -eq 0) {
    Write-Host "✅ ALL TESTS PASSED" -ForegroundColor Green
    Write-Host ""
    Write-Host "Web server is functioning correctly!" -ForegroundColor Green
    Write-Host "You can proceed with manual testing in the browser." -ForegroundColor Green
    exit 0
} elseif ($results.Failed -le 2) {
    Write-Host "⚠️  SOME TESTS FAILED" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Most functionality appears working." -ForegroundColor Yellow
    Write-Host "Check failed tests above for details." -ForegroundColor Yellow
    exit 0
} else {
    Write-Host "❌ MULTIPLE TESTS FAILED" -ForegroundColor Red
    Write-Host ""
    Write-Host "Web server may not be running or configured correctly." -ForegroundColor Red
    Write-Host "Please check the application and try again." -ForegroundColor Red
    exit 1
}
