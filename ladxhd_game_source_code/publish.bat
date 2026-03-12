@echo off
cd /d "%~dp0"

echo Building Windows DirectX...
dotnet publish ProjectZ.Desktop\ProjectZ.Desktop.csproj -c Release -f net8.0-windows -r win-x64 -p:PublishProfile=FolderProfile_DX
if %errorlevel% neq 0 ( echo DX build failed! & pause & exit /b 1 )

echo Building Windows OpenGL...
dotnet publish ProjectZ.Desktop\ProjectZ.Desktop.csproj -c Release -f net8.0 -r win-x64 -p:PublishProfile=FolderProfile_GL
if %errorlevel% neq 0 ( echo GL build failed! & pause & exit /b 1 )

echo Building Android APK...
dotnet publish ProjectZ.Android\ProjectZ.Android.csproj -c Release -f net8.0-android -p:PublishProfile=FolderProfile_Android
if %errorlevel% neq 0 ( echo Android build failed! & pause & exit /b 1 )

echo Building Linux x64...
wsl bash -c "export MGFXC_WINE_PATH=/home/bighead/.wine-mgfxc && cd /mnt/c/Users/Bighead/source/repos/Zelda-LA-DX-HD-Updated/ladxhd_game_source_code && dotnet publish ProjectZ.Linux/ProjectZ.Linux.csproj -c Release -f net8.0 -r linux-x64 -p:PublishProfile=FolderProfile_Linux"

echo Building Linux Arm64...
wsl bash -c "export MGFXC_WINE_PATH=/home/bighead/.wine-mgfxc && cd /mnt/c/Users/Bighead/source/repos/Zelda-LA-DX-HD-Updated/ladxhd_game_source_code && dotnet publish ProjectZ.Linux/ProjectZ.Linux.csproj -c Release -f net8.0 -r linux-arm64 -p:PublishProfile=FolderProfile_Linux_Arm"

echo Done! Builds are in the Publish folder.
pause