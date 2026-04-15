@echo off
cd /d "%~dp0"
Title LADXHD: Game Publish Script

::───────────────────────────────────────────────────────────────────────────────────────────────────────────────────
:: Configuration
::───────────────────────────────────────────────────────────────────────────────────────────────────────────────────

set RunCreatePatches=true

::───────────────────────────────────────────────────────────────────────────────────────────────────────────────────
:: Path Variables for WSL
::───────────────────────────────────────────────────────────────────────────────────────────────────────────────────

SET "WIN_PATH=%~dp0"
SET "WIN_PATH=%WIN_PATH:~0,-1%"
FOR /F "usebackq delims=" %%i IN (`wsl wslpath -u "%WIN_PATH%"`) DO SET "WSL_PATH=%%i"

SET "WSL_HOME=$HOME"
SET "WSL_DOTNET=$HOME/.dotnet/dotnet"
SET "MGFXC_PATH=$HOME/.wine-mgfxc"
SET "WSL_PREFIX=export MGFXC_WINE_PATH=$HOME/.wine-mgfxc && cd %WSL_PATH% && $HOME/.dotnet/dotnet"

::───────────────────────────────────────────────────────────────────────────────────────────────────────────────────
:: Clean Previous Builds
::───────────────────────────────────────────────────────────────────────────────────────────────────────────────────

if exist "%~dp0~Publish" (
    echo Cleaning previous builds...
    rd /s /q "%~dp0~Publish"
)

::───────────────────────────────────────────────────────────────────────────────────────────────────────────────────
:: Publish all Builds
::───────────────────────────────────────────────────────────────────────────────────────────────────────────────────

echo Publishing game builds...
echo.

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
wsl bash -c "%WSL_PREFIX% restore ProjectZ.Linux/ProjectZ.Linux.csproj"
if %errorlevel% neq 0 ( echo Linux restore failed! & pause & exit /b 1 )

echo Publishing Linux x64...
wsl bash -c "%WSL_PREFIX% publish ProjectZ.Linux/ProjectZ.Linux.csproj -c Release -f net8.0 -r linux-x64 --no-restore -p:PublishProfile=FolderProfile_Linux"
if %errorlevel% neq 0 ( echo Linux x86_64 build failed! & pause & exit /b 1 )

echo Publishing Linux Arm64...
wsl bash -c "%WSL_PREFIX% publish ProjectZ.Linux/ProjectZ.Linux.csproj -c Release -f net8.0 -r linux-arm64 --no-restore -p:PublishProfile=FolderProfile_Linux_Arm"
if %errorlevel% neq 0 ( echo Linux Arm64 build failed! & pause & exit /b 1 )

echo Restoring MacOS packages in WSL...
wsl bash -c "%WSL_PREFIX% restore ProjectZ.MacOS/ProjectZ.MacOS.csproj"
if %errorlevel% neq 0 ( echo MacOS restore failed! & pause & exit /b 1 )

echo Publishing MacOS arm64...
wsl bash -c "%WSL_PREFIX% publish ProjectZ.MacOS/ProjectZ.MacOS.csproj -c Release -f net8.0 -r osx-arm64 --no-restore -p:PublishProfile=FolderProfile_MacOS"
if %errorlevel% neq 0 ( echo MacOS Arm64 build failed! & pause & exit /b 1 )

echo Publishing MacOS x64...
wsl bash -c "%WSL_PREFIX% publish ProjectZ.MacOS/ProjectZ.MacOS.csproj -c Release -f net8.0 -r osx-x64 --no-restore -p:PublishProfile=FolderProfile_MacOS_x64"
if %errorlevel% neq 0 ( echo MacOS x86_64 build failed! & pause & exit /b 1 )

::───────────────────────────────────────────────────────────────────────────────────────────────────────────────────
:: Clean up unnecessary files
::───────────────────────────────────────────────────────────────────────────────────────────────────────────────────

echo.
echo Cleaning up junk files...
for /r "%~dp0~Publish" %%f in (nfd.lib nfd.pdb sosdocsunix.txt com.zelda.ladxhd.apk _Microsoft.Android.Resource.Designer.dll) do (
  if exist "%%f" (
    echo Deleting: %%f
    del "%%f"
  )
)

::───────────────────────────────────────────────────────────────────────────────────────────────────────────────────
:: Create Patches
::───────────────────────────────────────────────────────────────────────────────────────────────────────────────────

if [%RunCreatePatches%]==[true] (
    echo Running CreatePatches.ps1...
    powershell -ExecutionPolicy Bypass -File "%~dp0..\ladxhd_patcher_source_code\CreatePatches.ps1"
    if %errorlevel% neq 0 ( echo CreatePatches failed! & pause & exit /b 1 )
)

::───────────────────────────────────────────────────────────────────────────────────────────────────────────────────
:: Finish
::───────────────────────────────────────────────────────────────────────────────────────────────────────────────────

echo.
if [%RunCreatePatches%]==[true] (
    echo Done! Game built, patches created, and launcher published.
) else (
    echo Done! Builds are in the Publish folder.
    pause >nul
)