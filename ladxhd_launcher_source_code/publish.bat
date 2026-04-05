@echo off
cd /d "%~dp0"

echo Building LADXHD Launcher...

dotnet publish LADXHD_Launcher.csproj -c Release -f net8.0 -r win-x64 -p:PublishProfile=Windows -p:PublishDir="%~dp0~Publish\Windows\"

if %errorlevel% neq 0 (
    echo.
    echo BUILD FAILED with error code %errorlevel%
) else (
    echo.
    echo BUILD SUCCEEDED
    echo Output: %~dp0~Publish\Windows\
)

pause