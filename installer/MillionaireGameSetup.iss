; Inno Setup Script for The Millionaire Game
; Requires Inno Setup 6.0 or later: https://jrsoftware.org/isinfo.php

#define MyAppName "The Millionaire Game"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Jean Francois Delgado"
#define MyAppURL "https://github.com/Macronair/TheMillionaireGame"
#define MyAppExeName "MillionaireGame.exe"
#define MyAppIcon "..\src\MillionaireGame\lib\image\logo.ico"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
AppId={{8B7F3C2A-9D4E-4F1A-B8C6-5E2D1A9F7B3C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=..\LICENSE
OutputDir=..\installer\output
OutputBaseFilename=MillionaireGameSetup-v{#MyAppVersion}
SetupIconFile={#MyAppIcon}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
CloseApplications=yes
RestartApplications=no
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} Setup
VersionInfoCopyright=Copyright © 2025-2026 {#MyAppPublisher}
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "initializedb"; Description: "Initialize SQL Server database (creates dbMillionaire and imports questions)"; GroupDescription: "Database Setup:"; Flags: unchecked

[Files]
Source: "..\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Database Initialization Script"; Filename: "{app}\init_database.sql"
Name: "{group}\SQL Setup Instructions"; Filename: "{app}\README.md"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
var
  DotNetDownloadPage: TDownloadWizardPage;
  SqlExpressDownloadPage: TDownloadWizardPage;

const
  // .NET 8.0 Desktop Runtime x64 (evergreen link - always latest)
  DotNetRuntimeURL = 'https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe';
  DotNetInstallerName = 'windowsdesktop-runtime-8.0-win-x64.exe';
  
  // SQL Server Express (evergreen link - always latest)
  SqlExpressURL = 'https://go.microsoft.com/fwlink/?linkid=866658';
  SqlExpressInstallerName = 'SQLServer-Express-Setup.exe';

function GetUninstallString(): String;
var
  sUnInstPath: String;
  sUnInstallString: String;
begin
  sUnInstPath := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
  Result := sUnInstallString;
end;

function IsUpgrade(): Boolean;
begin
  Result := (GetUninstallString() <> '');
end;

function UnInstallOldVersion(): Integer;
var
  sUnInstallString: String;
  iResultCode: Integer;
begin
  Result := 0;
  sUnInstallString := GetUninstallString();
  if sUnInstallString <> '' then begin
    sUnInstallString := RemoveQuotes(sUnInstallString);
    if Exec(sUnInstallString, '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_HIDE, ewWaitUntilTerminated, iResultCode) then
      Result := 3
    else
      Result := 2;
  end else
    Result := 1;
end;

function InitializeSetup(): Boolean;
begin
  // Uninstall previous version if it exists
  if IsUpgrade() then
  begin
    if MsgBox('A previous version of {#MyAppName} is installed. It will be uninstalled automatically before proceeding with this installation.' + #13#10#13#10 + 'Continue with installation?', mbConfirmation, MB_YESNO) = IDYES then
    begin
      UnInstallOldVersion();
      Result := True;
    end
    else
      Result := False;
  end
  else
    Result := True;
end;

function IsDotNetInstalled(): Boolean;
var
  Version: String;
begin
  // Check for .NET 8.0 Desktop Runtime in registry
  Result := RegQueryStringValue(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedhost', 'Version', Version) and (Version >= '8.0.0');
  
  if not Result then
    Result := RegQueryStringValue(HKLM64, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedhost', 'Version', Version) and (Version >= '8.0.0');
end;

function IsSqlExpressInstalled(): Boolean;
var
  Names: TArrayOfString;
  I: Integer;
begin
  Result := False;
  
  // Check for SQL Server installations in registry
  if RegGetSubkeyNames(HKLM, 'SOFTWARE\Microsoft\Microsoft SQL Server', Names) then
  begin
    for I := 0 to GetArrayLength(Names) - 1 do
    begin
      // Look for SQLEXPRESS or any SQL Server instance
      if (Pos('SQLEXPRESS', Uppercase(Names[I])) > 0) or 
         (Pos('MSSQL', Uppercase(Names[I])) > 0) then
      begin
        Result := True;
        Break;
      end;
    end;
  end;
  
  // Also check 64-bit registry
  if not Result and RegGetSubkeyNames(HKLM64, 'SOFTWARE\Microsoft\Microsoft SQL Server', Names) then
  begin
    for I := 0 to GetArrayLength(Names) - 1 do
    begin
      if (Pos('SQLEXPRESS', Uppercase(Names[I])) > 0) or 
         (Pos('MSSQL', Uppercase(Names[I])) > 0) then
      begin
        Result := True;
        Break;
      end;
    end;
  end;
end;

procedure InitializeWizard();
begin
  // Create download page for .NET Runtime if needed
  if not IsDotNetInstalled() then
  begin
    DotNetDownloadPage := CreateDownloadPage(SetupMessage(msgWizardPreparing), SetupMessage(msgPreparingDesc), nil);
  end;
  
  // Create download page for SQL Server Express if needed
  if not IsSqlExpressInstalled() then
  begin
    SqlExpressDownloadPage := CreateDownloadPage(SetupMessage(msgWizardPreparing), SetupMessage(msgPreparingDesc), nil);
  end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  ResultCode: Integer;
begin
  Result := True;
  
  // Download .NET Runtime if needed
  if (CurPageID = wpReady) and not IsDotNetInstalled() and (DotNetDownloadPage <> nil) then
  begin
    DotNetDownloadPage.Clear;
    DotNetDownloadPage.Add(DotNetRuntimeURL, DotNetInstallerName, '');
    DotNetDownloadPage.Show;
    
    try
      try
        DotNetDownloadPage.Download;
        Result := True;
      except
        if DotNetDownloadPage.AbortedByUser then
        begin
          Result := False;
          MsgBox('The Millionaire Game requires .NET 8.0 Desktop Runtime to run. Setup cannot continue without it.', mbError, MB_OK);
        end
        else
        begin
          Result := False;
          MsgBox('Error downloading .NET 8.0 Desktop Runtime. Please check your internet connection and try again.', mbError, MB_OK);
        end;
      end;
    finally
      DotNetDownloadPage.Hide;
    end;
    
    // Install .NET Runtime if download succeeded
    if Result then
    begin
      if not ShellExec('', ExpandConstant('{tmp}\' + DotNetInstallerName), '/install /quiet /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
      begin
        Result := False;
        MsgBox('Failed to install .NET 8.0 Desktop Runtime. Error code: ' + IntToStr(ResultCode), mbError, MB_OK);
      end
      else if ResultCode <> 0 then
      begin
        // ResultCode 3010 means reboot required but installation succeeded
        if ResultCode = 3010 then
        begin
          MsgBox('.NET 8.0 Desktop Runtime has been installed successfully. A reboot may be required.', mbInformation, MB_OK);
          Result := True;
        end
        else
        begin
          Result := False;
          MsgBox('Failed to install .NET 8.0 Desktop Runtime. Error code: ' + IntToStr(ResultCode), mbError, MB_OK);
        end;
      end;
    end;
  end;
  
  // Download SQL Server Express if needed
  if Result and (CurPageID = wpReady) and not IsSqlExpressInstalled() and (SqlExpressDownloadPage <> nil) then
  begin
    if MsgBox('SQL Server Express is not installed. Would you like to download and install it now?' + #13#10 + 
              'This is required for the database functionality.' + #13#10#13#10 +
              'Note: The installer will download a small bootstrapper that will then download the full SQL Server Express.',
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      SqlExpressDownloadPage.Clear;
      SqlExpressDownloadPage.Add(SqlExpressURL, SqlExpressInstallerName, '');
      SqlExpressDownloadPage.Show;
      
      try
        try
          SqlExpressDownloadPage.Download;
        except
          if not SqlExpressDownloadPage.AbortedByUser then
            MsgBox('Error downloading SQL Server Express. You can install it manually later.', mbError, MB_OK);
        end;
      finally
        SqlExpressDownloadPage.Hide;
      end;
      
      // Launch SQL Server Express installer (bootstrapper)
      if FileExists(ExpandConstant('{tmp}\' + SqlExpressInstallerName)) then
      begin
        MsgBox('SQL Server Express installer will now launch. Please complete the installation.' + #13#10#13#10 +
               'Recommended settings:' + #13#10 +
               '- Choose "Download Media" or "Basic" installation' + #13#10 +
               '- Instance name: SQLEXPRESS (default)' + #13#10 +
               '- Authentication: Windows Authentication (default)' + #13#10#13#10 +
               'Click OK to continue...', mbInformation, MB_OK);
               
        ShellExec('', ExpandConstant('{tmp}\' + SqlExpressInstallerName), '', '', SW_SHOW, ewWaitUntilTerminated, ResultCode);
      end;
    end;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  PowerShellScript: String;
  ScriptFile: String;
begin
  if CurStep = ssPostInstall then
  begin
    // Check if user wants to initialize the database (check the task checkbox)
    if WizardIsTaskSelected('initializedb') then
    begin
      // Create PowerShell script to initialize database
      ScriptFile := ExpandConstant('{tmp}\InitializeDatabase.ps1');
      PowerShellScript := 
        '$ErrorActionPreference = "Stop"' + #13#10 +
        '$logFile = "' + ExpandConstant('{tmp}\database-init.log') + '"' + #13#10 +
        'Start-Transcript -Path $logFile -Append' + #13#10 +
        'Add-Type -AssemblyName System.Windows.Forms' + #13#10 +
        'try {' + #13#10 +
        '    Write-Host "=== Database Initialization Started ===" -ForegroundColor Cyan' + #13#10 +
        '    Write-Host "Script location: ' + ExpandConstant('{app}\init_database.sql') + '"' + #13#10 +
        '    Write-Host ""' + #13#10 +
        '    ' + #13#10 +
        '    Write-Host "Step 1: Connecting to SQL Server..." -ForegroundColor Yellow' + #13#10 +
        '    $connString = "Server=localhost\SQLEXPRESS;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;"' + #13#10 +
        '    $conn = New-Object System.Data.SqlClient.SqlConnection($connString)' + #13#10 +
        '    $conn.Open()' + #13#10 +
        '    Write-Host "Connected successfully!" -ForegroundColor Green' + #13#10 +
        '    Write-Host ""' + #13#10 +
        '    ' + #13#10 +
        '    Write-Host "Step 2: Checking if database exists..." -ForegroundColor Yellow' + #13#10 +
        '    $cmd = $conn.CreateCommand()' + #13#10 +
        '    $cmd.CommandText = "SELECT DB_ID(' + #39 + 'dbMillionaire' + #39 + ')"' + #13#10 +
        '    $result = $cmd.ExecuteScalar()' + #13#10 +
        '    ' + #13#10 +
        '    if ($null -eq $result) {' + #13#10 +
        '        Write-Host "Database does not exist. Creating..." -ForegroundColor Yellow' + #13#10 +
        '        $cmd.CommandText = "CREATE DATABASE dbMillionaire"' + #13#10 +
        '        $cmd.ExecuteNonQuery() | Out-Null' + #13#10 +
        '        Write-Host "Database created successfully!" -ForegroundColor Green' + #13#10 +
        '        ' + #13#10 +
        '        Write-Host "Granting permissions to current user..." -ForegroundColor Yellow' + #13#10 +
        '        $currentUser = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name' + #13#10 +
        '        $cmd.CommandText = "USE dbMillionaire; IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = ' + #39 + '$currentUser' + #39 + ') CREATE USER [$currentUser] FOR LOGIN [$currentUser]; ALTER ROLE db_owner ADD MEMBER [$currentUser];"' + #13#10 +
        '        $cmd.ExecuteNonQuery() | Out-Null' + #13#10 +
        '        Write-Host "Permissions granted successfully!" -ForegroundColor Green' + #13#10 +
        '    } else {' + #13#10 +
        '        Write-Host "Database already exists." -ForegroundColor Green' + #13#10 +
        '    }' + #13#10 +
        '    $conn.Close()' + #13#10 +
        '    Write-Host ""' + #13#10 +
        '    ' + #13#10 +
        '    Write-Host "Step 3: Reading SQL script..." -ForegroundColor Yellow' + #13#10 +
        '    $sqlFile = "' + ExpandConstant('{app}\init_database.sql') + '"' + #13#10 +
        '    if (-not (Test-Path $sqlFile)) {' + #13#10 +
        '        throw "SQL script not found at: $sqlFile"' + #13#10 +
        '    }' + #13#10 +
        '    $sqlScript = Get-Content $sqlFile -Raw -Encoding UTF8' + #13#10 +
        '    Write-Host "Script loaded. Size: $($sqlScript.Length) characters" -ForegroundColor Green' + #13#10 +
        '    Write-Host ""' + #13#10 +
        '    ' + #13#10 +
        '    Write-Host "Step 4: Connecting to dbMillionaire database..." -ForegroundColor Yellow' + #13#10 +
        '    $dbConnString = "Server=localhost\SQLEXPRESS;Database=dbMillionaire;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;"' + #13#10 +
        '    $dbConn = New-Object System.Data.SqlClient.SqlConnection($dbConnString)' + #13#10 +
        '    $dbConn.Open()' + #13#10 +
        '    Write-Host "Connected to database successfully!" -ForegroundColor Green' + #13#10 +
        '    Write-Host ""' + #13#10 +
        '    ' + #13#10 +
        '    Write-Host "Step 5: Executing SQL batches..." -ForegroundColor Yellow' + #13#10 +
        '    $batches = $sqlScript -split "(?m)^\s*GO\s*$"' + #13#10 +
        '    $batchCount = 0' + #13#10 +
        '    $totalBatches = ($batches | Where-Object { $_.Trim().Length -gt 0 }).Count' + #13#10 +
        '    Write-Host "Total batches to execute: $totalBatches"' + #13#10 +
        '    ' + #13#10 +
        '    foreach ($batch in $batches) {' + #13#10 +
        '        $batch = $batch.Trim()' + #13#10 +
        '        if ($batch.Length -gt 0) {' + #13#10 +
        '            $batchCount++' + #13#10 +
        '            Write-Host "Executing batch $batchCount/$totalBatches..." -NoNewline' + #13#10 +
        '            $dbCmd = $dbConn.CreateCommand()' + #13#10 +
        '            $dbCmd.CommandText = $batch' + #13#10 +
        '            $dbCmd.CommandTimeout = 120' + #13#10 +
        '            $rowsAffected = $dbCmd.ExecuteNonQuery()' + #13#10 +
        '            Write-Host " OK ($rowsAffected rows)" -ForegroundColor Green' + #13#10 +
        '        }' + #13#10 +
        '    }' + #13#10 +
        '    $dbConn.Close()' + #13#10 +
        '    Write-Host ""' + #13#10 +
        '    Write-Host "=== Database Initialized Successfully! ===" -ForegroundColor Green' + #13#10 +
        '    Write-Host "Database: dbMillionaire" -ForegroundColor Cyan' + #13#10 +
        '    Write-Host "Batches executed: $batchCount" -ForegroundColor Cyan' + #13#10 +
        '    Write-Host "Tables: questions (80 records), fff_questions (44 records)" -ForegroundColor Cyan' + #13#10 +
        '    Write-Host ""' + #13#10 +
        '    Write-Host "Press any key to continue..." -ForegroundColor Yellow' + #13#10 +
        '    Stop-Transcript' + #13#10 +
        '    $null = $host.UI.RawUI.ReadKey(' + #39 + 'NoEcho,IncludeKeyDown' + #39 + ')' + #13#10 +
        '    [System.Windows.Forms.MessageBox]::Show("Database initialized successfully!`n`nDatabase: dbMillionaire`nBatches executed: $batchCount`nTables created: questions (80 records), fff_questions (44 records)", "Database Initialization Complete", 0, 64)' + #13#10 +
        '    exit 0' + #13#10 +
        '} catch {' + #13#10 +
        '    Write-Host ""' + #13#10 +
        '    Write-Host "=== ERROR OCCURRED ===" -ForegroundColor Red' + #13#10 +
        '    Write-Host "Error: $_" -ForegroundColor Red' + #13#10 +
        '    Write-Host "Stack Trace: $($_.ScriptStackTrace)" -ForegroundColor Red' + #13#10 +
        '    Write-Host ""' + #13#10 +
        '    Write-Host "Log file saved to: $logFile" -ForegroundColor Yellow' + #13#10 +
        '    Write-Host "You can manually run: ' + ExpandConstant('{app}\init_database.sql') + '" -ForegroundColor Yellow' + #13#10 +
        '    Write-Host ""' + #13#10 +
        '    Write-Host "Press any key to continue..." -ForegroundColor Yellow' + #13#10 +
        '    Stop-Transcript' + #13#10 +
        '    $null = $host.UI.RawUI.ReadKey(' + #39 + 'NoEcho,IncludeKeyDown' + #39 + ')' + #13#10 +
        '    [System.Windows.Forms.MessageBox]::Show("Failed to initialize database:`n`n$_`n`nLog file: $logFile`n`nYou can manually run init_database.sql from the installation folder.", "Database Initialization Error", 0, 16)' + #13#10 +
        '    exit 1' + #13#10 +
        '}';
      
      SaveStringToFile(ScriptFile, PowerShellScript, False);
      
      // Execute PowerShell script with visible window
      Exec('powershell.exe', 
           '-ExecutionPolicy Bypass -File "' + ScriptFile + '"',
           '', SW_SHOW, ewWaitUntilTerminated, ResultCode);
           
      // Check if script failed
      if ResultCode <> 0 then
      begin
        MsgBox('Database initialization failed with exit code: ' + IntToStr(ResultCode) + #13#10#13#10 +
               'Log file saved to: ' + ExpandConstant('{tmp}\database-init.log') + #13#10#13#10 +
               'You can manually run init_database.sql from the installation folder.', mbError, MB_OK);
      end;
    end;
  end;
end;
