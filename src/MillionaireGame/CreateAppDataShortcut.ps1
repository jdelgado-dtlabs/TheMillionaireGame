# CreateAppDataShortcut.ps1
# Creates a shortcut to the MillionaireGame AppData folder in the build output directory

param(
    [Parameter(Mandatory=$true)]
    [string]$OutputPath
)

try {
    # Remove any quotes from the parameter
    $OutputPath = $OutputPath.Trim('"').TrimEnd('\')
    
    $WshShell = New-Object -ComObject WScript.Shell
    $AppDataPath = [System.Environment]::GetFolderPath('LocalApplicationData')
    $TargetPath = Join-Path $AppDataPath "TheMillionaireGame"
    
    # Get absolute path
    Push-Location $PSScriptRoot
    $OutputPath = (Resolve-Path -Path $OutputPath -ErrorAction Stop).Path
    Pop-Location
    
    # Ensure output path exists
    if (-not (Test-Path -LiteralPath $OutputPath)) {
        Write-Host "Output path does not exist: $OutputPath"
        exit 0
    }
    
    $ShortcutPath = Join-Path $OutputPath "AppData Folder.lnk"

    # Create AppData directory if it doesn't exist
    if (-not (Test-Path $TargetPath)) {
        New-Item -ItemType Directory -Path $TargetPath -Force | Out-Null
        Write-Host "Created AppData directory: $TargetPath"
    }

    # Create shortcut
    $Shortcut = $WshShell.CreateShortcut($ShortcutPath)
    $Shortcut.TargetPath = $TargetPath
    $Shortcut.Description = "Shortcut to MillionaireGame AppData folder (settings, database, logs)"
    $Shortcut.IconLocation = "shell32.dll,3"
    $Shortcut.Save()

    Write-Host "Created shortcut: AppData Folder.lnk -> $TargetPath"
}
catch {
    $ErrorMessage = $_.Exception.Message
    Write-Warning "Failed to create AppData shortcut: $ErrorMessage"
    exit 0
}

