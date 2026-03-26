@echo off
cd /d "%~dp0"

echo Restoring Windows/Android projects...
dotnet restore
if %errorlevel% neq 0 ( echo Restore failed! & pause & exit /b 1 )

echo Building Windows DirectX...
dotnet build --nologo ProjectZ.Desktop\ProjectZ.Desktop.csproj -c Release -f net8.0-windows -r win-x64 --no-restore
if %errorlevel% neq 0 ( echo DX prebuild failed! & pause & exit /b 1 )

echo Publishing Windows DirectX...
dotnet publish ProjectZ.Desktop\ProjectZ.Desktop.csproj -c Release -f net8.0-windows -r win-x64 --no-restore -p:PublishProfile=FolderProfile_DX
if %errorlevel% neq 0 ( echo DX publish failed! & pause & exit /b 1 )

echo Building Windows OpenGL...
dotnet build --nologo ProjectZ.Desktop\ProjectZ.Desktop.csproj -c Release -f net8.0 -r win-x64 --no-restore
if %errorlevel% neq 0 ( echo GL prebuild failed! & pause & exit /b 1 )

echo Publishing Windows OpenGL...
dotnet publish ProjectZ.Desktop\ProjectZ.Desktop.csproj -c Release -f net8.0 -r win-x64 --no-restore -p:PublishProfile=FolderProfile_GL
if %errorlevel% neq 0 ( echo GL publish failed! & pause & exit /b 1 )

echo Publishing Android APK...
dotnet publish ProjectZ.Android\ProjectZ.Android.csproj -c Release -f net8.0-android --no-restore -p:PublishProfile=FolderProfile_Android
if %errorlevel% neq 0 ( echo Android build failed! & pause & exit /b 1 )

echo Restoring Linux packages in WSL...
wsl bash -c "export MGFXC_WINE_PATH=/home/bighead/.wine-mgfxc && cd /mnt/c/Users/Bighead/source/repos/Zelda-LA-DX-HD-Updated/ladxhd_game_source_code && dotnet restore ProjectZ.Linux/ProjectZ.Linux.csproj"
if %errorlevel% neq 0 ( echo Linux restore failed! & pause & exit /b 1 )

echo Publishing Linux x64...
wsl bash -c "export MGFXC_WINE_PATH=/home/bighead/.wine-mgfxc && cd /mnt/c/Users/Bighead/source/repos/Zelda-LA-DX-HD-Updated/ladxhd_game_source_code && dotnet publish ProjectZ.Linux/ProjectZ.Linux.csproj -c Release -f net8.0 -r linux-x64 --no-restore -p:PublishProfile=FolderProfile_Linux"
if %errorlevel% neq 0 ( echo Linux x64 build failed! & pause & exit /b 1 )

echo Publishing Linux Arm64...
wsl bash -c "export MGFXC_WINE_PATH=/home/bighead/.wine-mgfxc && cd /mnt/c/Users/Bighead/source/repos/Zelda-LA-DX-HD-Updated/ladxhd_game_source_code && dotnet publish ProjectZ.Linux/ProjectZ.Linux.csproj -c Release -f net8.0 -r linux-arm64 --no-restore -p:PublishProfile=FolderProfile_Linux_Arm"
if %errorlevel% neq 0 ( echo Linux arm64 build failed! & pause & exit /b 1 )

echo.
echo Cleaning up junk files...
for /r "%~dp0~Publish" %%f in (nfd.lib nfd.pdb sosdocsunix.txt com.zelda.ladxhd.apk _Microsoft.Android.Resource.Designer.dll) do (
  if exist "%%f" (
    echo Deleting: %%f
    del "%%f"
  )
)

echo.
echo Done! Builds are in the Publish folder. Press a key to close the window.
pause >nul