@echo off
REM Millionaire Game Launcher with Watchdog
REM This script starts the application through the watchdog monitor

echo ====================================
echo Millionaire Game
echo Starting with crash monitoring...
echo ====================================
echo.

REM Get script directory
set "SCRIPT_DIR=%~dp0"

REM Set paths
set "WATCHDOG=%SCRIPT_DIR%bin\Debug\net8.0\MillionaireGame.Watchdog.exe"
set "MAIN_APP=%SCRIPT_DIR%bin\Debug\net8.0-windows\MillionaireGame.exe"

REM Check if watchdog exists
if not exist "%WATCHDOG%" (
    echo ERROR: Watchdog not found at: %WATCHDOG%
    echo Please build the solution first.
    pause
    exit /b 1
)

REM Check if main app exists
if not exist "%MAIN_APP%" (
    echo ERROR: Main application not found at: %MAIN_APP%
    echo Please build the solution first.
    pause
    exit /b 1
)

REM Start watchdog (which will start the main application)
echo Starting watchdog...
start "Millionaire Game Watchdog" "%WATCHDOG%" "%MAIN_APP%"

echo.
echo Watchdog started. The main application will launch shortly.
echo Close this window if you want to exit.
