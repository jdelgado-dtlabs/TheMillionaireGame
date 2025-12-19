@echo off
echo ========================================
echo  The Millionaire Game - DEBUG MODE
echo ========================================
echo.

cd /d "%~dp0"
dotnet run --project src\MillionaireGame\MillionaireGame.csproj -- --debug

pause
