; Inno Setup Script for The Millionaire Game
; Requires Inno Setup 6.0 or later: https://jrsoftware.org/isinfo.php

; Auto-download SqlLocalDB.msi if missing
#define SqlLocalDBPath "lib\sql\SqlLocalDB.msi"
#define SqlLocalDBUrl "https://download.microsoft.com/download/3/8/d/38de7036-2433-4207-8eae-06e247e17b25/SqlLocalDB.msi"

#if !FileExists(SqlLocalDBPath)
  #pragma message "SqlLocalDB.msi not found. Downloading..."
  #expr Exec("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command ""New-Item -ItemType Directory -Path 'lib\sql' -Force | Out-Null; Invoke-WebRequest -Uri '" + SqlLocalDBUrl + "' -OutFile '" + SqlLocalDBPath + "' -UseBasicParsing""", "", SW_HIDE, ewWaitUntilTerminated)
  #if FileExists(SqlLocalDBPath)
    #pragma message "SqlLocalDB.msi downloaded successfully!"
  #else
    #error "Failed to download SqlLocalDB.msi. Please download manually from: " + SqlLocalDBUrl
  #endif
#else
  #pragma message "SqlLocalDB.msi found - using existing file"
#endif

#define MyAppName "The Millionaire Game"
#define MyAppVersion "1.0.6"
#define MyAppPublisher "Jean Francois Delgado"
#define MyAppURL "https://github.com/jdelgado-dtlabs/TheMillionaireGame"
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
; SQL Server LocalDB (bundled with installer)
Source: "lib\sql\SqlLocalDB.msi"; DestDir: "{app}\lib\sql"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Database Initialization Script"; Filename: "{app}\lib\sql\init_database.sql"
Name: "{group}\SQL Setup Instructions"; Filename: "{app}\README.md"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Auto-detected database installations (first-time install with existing SQL)
; No flag = LocalDB (default), --db-type=sqlserver for any SQL Server installation
Filename: "{app}\{#MyAppExeName}"; Parameters: "--db-type=sqlserver"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent; Check: IsDetectedSqlServer
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent; Check: IsDetectedLocalDB

; User-selected database options (from wizard)
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent; Check: IsLocalDBChoice
Filename: "{app}\{#MyAppExeName}"; Parameters: "--db-type=sqlserver"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent; Check: IsSqlExpressChoice
Filename: "{app}\{#MyAppExeName}"; Parameters: "--db-type=sqlserver"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent; Check: IsRemoteServerChoice

[Code]
var
  DotNetDownloadPage: TDownloadWizardPage;
  SqlDownloadPage: TDownloadWizardPage;
  DatabaseChoicePage: TWizardPage;
  RadLocalDB: TRadioButton;
  RadSqlExpress: TRadioButton;
  RadRemoteServer: TRadioButton;
  LblDatabaseChoice: TLabel;
  LblLocalDBDesc: TLabel;
  LblSqlExpressDesc: TLabel;
  LblRemoteServerDesc: TLabel;
  UserDatabaseChoice: Integer; // 0=LocalDB, 1=SqlExpress, 2=Remote
  DetectedDatabaseType: String; // Stores auto-detected database type for first-run

const
  // .NET 8.0 Desktop Runtime x64 (evergreen link - always latest)
  DotNetRuntimeURL = 'https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe';
  DotNetInstallerName = 'windowsdesktop-runtime-8.0-win-x64.exe';
  
  // SQL Server 2022 Express with Advanced Services (1.5 GB download, includes full SQL Server features)
  SqlExpressURL = 'https://download.microsoft.com/download/3/8/d/38de7036-2433-4207-8eae-06e247e17b25/SQLEXPR_x64_ENU.exe';
  SqlExpressInstallerName = 'SQLEXPR_x64_ENU.exe';
  
  // SQL Server LocalDB (bundled with installer - no download needed)
  SqlLocalDBInstallerPath = '{app}\lib\sql\SqlLocalDB.msi';

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

// Check functions for [Run] section - Auto-detected installations
function IsDetectedSqlServer(): Boolean;
begin
  // Covers both SQL Server Express and full SQL Server
  Result := (DetectedDatabaseType = 'sqlserver') and (DatabaseChoicePage = nil);
end;

function IsDetectedLocalDB(): Boolean;
begin
  Result := (DetectedDatabaseType = 'localdb') and (DatabaseChoicePage = nil);
end;

// Check functions for [Run] section - User-selected from wizard
function IsLocalDBChoice(): Boolean;
begin
  Result := (UserDatabaseChoice = 0) and (DatabaseChoicePage <> nil);
end;

function IsSqlExpressChoice(): Boolean;
begin
  // SQL Express choice triggers --db-type=sqlserver (works same as full SQL Server)
  Result := (UserDatabaseChoice = 1) and (DatabaseChoicePage <> nil);
end;

function IsRemoteServerChoice(): Boolean;
begin
  // Remote server choice also triggers --db-type=sqlserver
  Result := (UserDatabaseChoice = 2) and (DatabaseChoicePage <> nil);
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

function IsLocalDBInstalled(): Boolean;
var
  LocalDBPath: String;
begin
  Result := False;
  
  // Check for SqlLocalDB.exe in common installation paths
  // SQL Server 2022
  LocalDBPath := ExpandConstant('{pf}\Microsoft SQL Server\160\Tools\Binn\SqlLocalDB.exe');
  if FileExists(LocalDBPath) then
  begin
    Result := True;
    Exit;
  end;
  
  // SQL Server 2019
  LocalDBPath := ExpandConstant('{pf}\Microsoft SQL Server\150\Tools\Binn\SqlLocalDB.exe');
  if FileExists(LocalDBPath) then
  begin
    Result := True;
    Exit;
  end;
  
  // SQL Server 2017
  LocalDBPath := ExpandConstant('{pf}\Microsoft SQL Server\140\Tools\Binn\SqlLocalDB.exe');
  if FileExists(LocalDBPath) then
  begin
    Result := True;
    Exit;
  end;
end;

function IsSqlExpressInstalled(): Boolean;
var
  SqlExpressPath: String;
begin
  Result := False;
  
  // Check for SQLEXPRESS instance in common installation paths
  // SQL Server 2022
  SqlExpressPath := ExpandConstant('{pf}\Microsoft SQL Server\160\Setup Bootstrap\SQLExpress');
  if DirExists(SqlExpressPath) then
  begin
    Result := True;
    Exit;
  end;
  
  // SQL Server 2019
  SqlExpressPath := ExpandConstant('{pf}\Microsoft SQL Server\150\Setup Bootstrap\SQLExpress');
  if DirExists(SqlExpressPath) then
  begin
    Result := True;
    Exit;
  end;
  
  // SQL Server 2017
  SqlExpressPath := ExpandConstant('{pf}\Microsoft SQL Server\140\Setup Bootstrap\SQLExpress');
  if DirExists(SqlExpressPath) then
  begin
    Result := True;
    Exit;
  end;
end;

function IsSqlServerInstalled(): Boolean;
var
  SqlServerPath: String;
  InstanceNames: String;
begin
  Result := False;
  
  // Check for full SQL Server installation (not Express or LocalDB)
  // Look for MSSQLSERVER (default instance) or any named instances
  
  // Check registry for installed instances
  if RegQueryStringValue(HKLM, 'SOFTWARE\Microsoft\Microsoft SQL Server', 'InstalledInstances', InstanceNames) then
  begin
    // If we find instances other than just 'SQLEXPRESS', it's full SQL Server
    if (Pos('MSSQLSERVER', InstanceNames) > 0) or 
       ((Length(InstanceNames) > 0) and (Pos('SQLEXPRESS', InstanceNames) = 0)) then
    begin
      Result := True;
      Exit;
    end;
  end;
  
  // Additional check: Look for SQL Server setup bootstrap (non-Express)
  // SQL Server 2022
  SqlServerPath := ExpandConstant('{pf}\Microsoft SQL Server\160\Setup Bootstrap\SQL2022');
  if DirExists(SqlServerPath) then
  begin
    Result := True;
    Exit;
  end;
  
  // SQL Server 2019
  SqlServerPath := ExpandConstant('{pf}\Microsoft SQL Server\150\Setup Bootstrap\SQL2019');
  if DirExists(SqlServerPath) then
  begin
    Result := True;
    Exit;
  end;
  
  // SQL Server 2017
  SqlServerPath := ExpandConstant('{pf}\Microsoft SQL Server\140\Setup Bootstrap\SQL2017');
  if DirExists(SqlServerPath) then
  begin
    Result := True;
    Exit;
  end;
end;

function GetDetectedDatabaseType(): String;
begin
  // Detection hierarchy:
  // 1. SQL Server (Express or full) - both work the same way
  // 2. LocalDB (only if SQL Server not installed)
  // 3. None - show installation wizard
  
  if IsSqlServerInstalled() or IsSqlExpressInstalled() then
  begin
    Result := 'sqlserver';
    Exit;
  end;
  
  if IsLocalDBInstalled() then
  begin
    Result := 'localdb';
    Exit;
  end;
  
  // No SQL Server installation detected
  Result := '';
end;

procedure InitializeWizard();
begin
  UserDatabaseChoice := 0; // Default to LocalDB
  DetectedDatabaseType := GetDetectedDatabaseType(); // Detect existing SQL installations
  
  // Create custom database choice page (only if no SQL Server is installed)
  // If SQL Server is detected, we'll auto-select it and skip the wizard page
  if DetectedDatabaseType = '' then
  begin
    DatabaseChoicePage := CreateCustomPage(wpWelcome, 'Database Selection', 'Choose your SQL Server database option');
    
    // Title label
    LblDatabaseChoice := TLabel.Create(WizardForm);
    LblDatabaseChoice.Parent := DatabaseChoicePage.Surface;
    LblDatabaseChoice.Left := 0;
    LblDatabaseChoice.Top := 0;
    LblDatabaseChoice.Width := DatabaseChoicePage.SurfaceWidth;
    LblDatabaseChoice.Caption := 'The Millionaire Game requires a SQL Server database.' + #13#10 + 'Please select a database option:';
    LblDatabaseChoice.WordWrap := True;
    LblDatabaseChoice.AutoSize := True;
    
    // LocalDB radio button
    RadLocalDB := TRadioButton.Create(WizardForm);
    RadLocalDB.Parent := DatabaseChoicePage.Surface;
    RadLocalDB.Left := 0;
    RadLocalDB.Top := LblDatabaseChoice.Top + LblDatabaseChoice.Height + 20;
    RadLocalDB.Width := DatabaseChoicePage.SurfaceWidth;
    RadLocalDB.Caption := 'SQL Server Express LocalDB (Recommended)';
    RadLocalDB.Checked := True;
    RadLocalDB.Font.Style := [fsBold];
    
    // LocalDB description
    LblLocalDBDesc := TLabel.Create(WizardForm);
    LblLocalDBDesc.Parent := DatabaseChoicePage.Surface;
    LblLocalDBDesc.Left := 20;
    LblLocalDBDesc.Top := RadLocalDB.Top + RadLocalDB.Height + 5;
    LblLocalDBDesc.Width := DatabaseChoicePage.SurfaceWidth - 20;
    LblLocalDBDesc.Caption := 'Lightweight database for single-user applications. Best for standalone use.' + #13#10 + 
                              'Download size: ~50 MB | No remote access | Automatic startup';
    LblLocalDBDesc.WordWrap := True;
    LblLocalDBDesc.AutoSize := True;
    
    // SQL Express radio button
    RadSqlExpress := TRadioButton.Create(WizardForm);
    RadSqlExpress.Parent := DatabaseChoicePage.Surface;
    RadSqlExpress.Left := 0;
    RadSqlExpress.Top := LblLocalDBDesc.Top + LblLocalDBDesc.Height + 20;
    RadSqlExpress.Width := DatabaseChoicePage.SurfaceWidth;
    RadSqlExpress.Caption := 'SQL Server 2022 Express (Advanced)';
    RadSqlExpress.Font.Style := [fsBold];
    
    // SQL Express description
    LblSqlExpressDesc := TLabel.Create(WizardForm);
    LblSqlExpressDesc.Parent := DatabaseChoicePage.Surface;
    LblSqlExpressDesc.Left := 20;
    LblSqlExpressDesc.Top := RadSqlExpress.Top + RadSqlExpress.Height + 5;
    LblSqlExpressDesc.Width := DatabaseChoicePage.SurfaceWidth - 20;
    LblSqlExpressDesc.Caption := 'Full-featured database with remote access and advanced features.' + #13#10 + 
                                 'Download size: ~1.5 GB | Supports remote connections | Requires manual configuration';
    LblSqlExpressDesc.WordWrap := True;
    LblSqlExpressDesc.AutoSize := True;
    
    // Remote Server radio button
    RadRemoteServer := TRadioButton.Create(WizardForm);
    RadRemoteServer.Parent := DatabaseChoicePage.Surface;
    RadRemoteServer.Left := 0;
    RadRemoteServer.Top := LblSqlExpressDesc.Top + LblSqlExpressDesc.Height + 20;
    RadRemoteServer.Width := DatabaseChoicePage.SurfaceWidth;
    RadRemoteServer.Caption := 'Remote SQL Server (I already have a server)';
    RadRemoteServer.Font.Style := [fsBold];
    
    // Remote Server description
    LblRemoteServerDesc := TLabel.Create(WizardForm);
    LblRemoteServerDesc.Parent := DatabaseChoicePage.Surface;
    LblRemoteServerDesc.Left := 20;
    LblRemoteServerDesc.Top := RadRemoteServer.Top + RadRemoteServer.Height + 5;
    LblRemoteServerDesc.Width := DatabaseChoicePage.SurfaceWidth - 20;
    LblRemoteServerDesc.Caption := 'Connect to an existing SQL Server instance (local network or remote).' + #13#10 + 
                                   'No installation required | You''ll configure connection details after setup';
    LblRemoteServerDesc.WordWrap := True;
    LblRemoteServerDesc.AutoSize := True;
  end;
  
  // Create download page for .NET Runtime if needed
  if not IsDotNetInstalled() then
  begin
    DotNetDownloadPage := CreateDownloadPage(SetupMessage(msgWizardPreparing), SetupMessage(msgPreparingDesc), nil);
  end;
  
  // Create download page for SQL Server (will be used for either LocalDB or Express)
  // Only create if no SQL Server installation was detected
  if DetectedDatabaseType = '' then
  begin
    SqlDownloadPage := CreateDownloadPage(SetupMessage(msgWizardPreparing), SetupMessage(msgPreparingDesc), nil);
  end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  ResultCode: Integer;
  SqlUrl: String;
  SqlInstaller: String;
begin
  Result := True;
  
  // Save user's database choice
  if (DatabaseChoicePage <> nil) and (CurPageID = DatabaseChoicePage.ID) then
  begin
    if RadSqlExpress.Checked then
      UserDatabaseChoice := 1
    else if RadRemoteServer.Checked then
      UserDatabaseChoice := 2
    else
      UserDatabaseChoice := 0; // LocalDB (default)
  end;
  
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
  
  // Download and install SQL Server Express (only if user chose Express)
  // Skip if user chose Remote Server option OR LocalDB (bundled) OR if SQL Server is already detected
  if Result and (CurPageID = wpReady) and (DetectedDatabaseType = '') and (SqlDownloadPage <> nil) and (UserDatabaseChoice = 1) then
  begin
    SqlUrl := SqlExpressURL;
    SqlInstaller := SqlExpressInstallerName;
    
    if MsgBox('SQL Server 2022 Express will be downloaded and installed.' + #13#10 + 
              'Download size: ~1.5 GB' + #13#10#13#10 +
              'Note: This will take several minutes. Continue?',
              mbConfirmation, MB_YESNO) <> IDYES then
    begin
      Result := False;
      Exit;
    end;
    
    SqlDownloadPage.Clear;
    SqlDownloadPage.Add(SqlUrl, SqlInstaller, '');
    SqlDownloadPage.Show;
    
    try
      try
        SqlDownloadPage.Download;
      except
        if not SqlDownloadPage.AbortedByUser then
        begin
          MsgBox('Error downloading SQL Server Express. You can install it manually later.', mbError, MB_OK);
        end;
      end;
    finally
      SqlDownloadPage.Hide;
    end;
    
    // Install SQL Server Express if download succeeded
    if FileExists(ExpandConstant('{tmp}\' + SqlInstaller)) then
    begin
      // SQL Server Express installation
      MsgBox('SQL Server 2022 Express will now install. This may take 10-15 minutes...' + #13#10#13#10 +
             'The installer will run in a separate window. Please wait for it to complete.', mbInformation, MB_OK);
      
      if ShellExec('', ExpandConstant('{tmp}\' + SqlInstaller), 
                   '/Q /IACCEPTSQLSERVERLICENSETERMS /ACTION=Install /FEATURES=SQLEngine /INSTANCENAME=SQLEXPRESS /SECURITYMODE=SQL /SAPWD="MillionaireGame2026!" /TCPENABLED=1', 
                   '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
      begin
        if ResultCode = 0 then
          MsgBox('SQL Server 2022 Express installed successfully!', mbInformation, MB_OK)
        else if ResultCode = 3010 then
          MsgBox('SQL Server 2022 Express installed successfully. A reboot may be required.', mbInformation, MB_OK)
        else
          MsgBox('SQL Server Express installation completed with code: ' + IntToStr(ResultCode), mbInformation, MB_OK);
      end
      else
        MsgBox('Failed to launch SQL Server Express installer.', mbError, MB_OK);
    end;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  PowerShellScript: String;
  ScriptFile: String;
  LocalDBPath: String;
begin
  if CurStep = ssPostInstall then
  begin
    // Install SQL Server LocalDB if user chose LocalDB option and it's not already installed
    if (DetectedDatabaseType = '') and (UserDatabaseChoice = 0) and not IsLocalDBInstalled() then
    begin
      LocalDBPath := ExpandConstant(SqlLocalDBInstallerPath);
      if FileExists(LocalDBPath) then
      begin
        MsgBox('SQL Server LocalDB will now install silently. This may take a few minutes...', mbInformation, MB_OK);
        
        if ShellExec('', 'msiexec.exe', '/i "' + LocalDBPath + '" /quiet IACCEPTSQLLOCALDBLICENSETERMS=YES', 
                     '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
        begin
          if ResultCode = 0 then
            MsgBox('SQL Server LocalDB installed successfully!', mbInformation, MB_OK)
          else if ResultCode = 3010 then
            MsgBox('SQL Server LocalDB installed successfully! A reboot may be required.', mbInformation, MB_OK)
          else
            MsgBox('SQL Server LocalDB installation completed with code: ' + IntToStr(ResultCode), mbInformation, MB_OK);
        end
        else
          MsgBox('Failed to launch SQL Server LocalDB installer.', mbError, MB_OK);
      end
      else
        MsgBox('SQL Server LocalDB installer not found at: ' + LocalDBPath, mbError, MB_OK);
    end;
    
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
        '    Write-Host "Script location: ' + ExpandConstant('{app}\lib\sql\init_database.sql') + '"' + #13#10 +
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
        '    if ([string]::IsNullOrEmpty($result)) {' + #13#10 +
        '        Write-Host "Database does not exist. Creating..." -ForegroundColor Yellow' + #13#10 +
        '        $cmd.CommandText = "CREATE DATABASE dbMillionaire"' + #13#10 +
        '        $rowsAffected = $cmd.ExecuteNonQuery()' + #13#10 +
        '        Write-Host "Database created successfully! (Rows affected: $rowsAffected)" -ForegroundColor Green' + #13#10 +
        '        ' + #13#10 +
        '        # Verify database was created' + #13#10 +
        '        $cmd.CommandText = "SELECT DB_ID(' + #39 + 'dbMillionaire' + #39 + ')"' + #13#10 +
        '        $verifyResult = $cmd.ExecuteScalar()' + #13#10 +
        '        if ([string]::IsNullOrEmpty($verifyResult)) {' + #13#10 +
        '            throw "Database creation appeared to succeed but database still does not exist!"' + #13#10 +
        '        }' + #13#10 +
        '        Write-Host "Database existence verified (DB_ID: $verifyResult)" -ForegroundColor Green' + #13#10 +
        '    } else {' + #13#10 +
        '        Write-Host "Database already exists (DB_ID: $result)" -ForegroundColor Green' + #13#10 +
        '    }' + #13#10 +
        '    $conn.Close()' + #13#10 +
        '    Write-Host ""' + #13#10 +
        '    ' + #13#10 +
        '    Write-Host "Step 3: Reading SQL script..." -ForegroundColor Yellow' + #13#10 +
        '    $sqlFile = "' + ExpandConstant('{app}\lib\sql\init_database.sql') + '"' + #13#10 +
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
        '    Write-Host "You can manually run: ' + ExpandConstant('{app}\lib\sql\init_database.sql') + '" -ForegroundColor Yellow' + #13#10 +
        '    Write-Host ""' + #13#10 +
        '    Write-Host "Press any key to continue..." -ForegroundColor Yellow' + #13#10 +
        '    Stop-Transcript' + #13#10 +
        '    $null = $host.UI.RawUI.ReadKey(' + #39 + 'NoEcho,IncludeKeyDown' + #39 + ')' + #13#10 +
        '    [System.Windows.Forms.MessageBox]::Show("Failed to initialize database:`n`n$_`n`nLog file: $logFile`n`nYou can manually run lib\sql\init_database.sql from the installation folder.", "Database Initialization Error", 0, 16)' + #13#10 +
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
               'You can manually run lib\sql\init_database.sql from the installation folder.', mbError, MB_OK);
      end;
    end;
  end;
end;
