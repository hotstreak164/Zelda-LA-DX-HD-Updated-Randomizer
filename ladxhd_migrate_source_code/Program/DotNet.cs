using System;
using System.IO;
using System.Diagnostics;
using static LADXHD_Migrater.Config;

namespace LADXHD_Migrater
{
    internal class DotNet
    {
        private static string WinePath = "/home/" + Environment.UserName + "/.wine-mgfxc";

        public static bool BuildGame()
        {
            // If the game path is invalid just cancel.
            if (!Config.Game_Source.TestPath()) return false;

            try
            {
                if (Config.SelectedPlatform == Platform.Windows)
                {
                    if (!RunProcess("dotnet", "restore", false, "Restore Error")) return false;

                    if (Config.SelectedGraphics == GraphicsAPI.DirectX)
                    {
                        Config.Build_Path = Path.Combine(Config.Publish_Path, "Windows-DX");

                        if (!RunProcess("dotnet",
                            "build ProjectZ.Desktop\\ProjectZ.Desktop.csproj -c Release -f net8.0-windows -r win-x64 --no-restore",
                            false, "Build Error")) return false;

                        if (!RunProcess("dotnet",
                            "publish ProjectZ.Desktop\\ProjectZ.Desktop.csproj -c Release -f net8.0-windows -r win-x64 --no-restore -p:PublishProfile=FolderProfile_DX",
                            false, "Build Error")) return false;
                    }
                    else if (Config.SelectedGraphics == GraphicsAPI.OpenGL)
                    {
                        Config.Build_Path = Path.Combine(Config.Publish_Path, "Windows-GL");

                        if (!RunProcess("dotnet",
                            "build ProjectZ.Desktop\\ProjectZ.Desktop.csproj -c Release -f net8.0 -r win-x64 --no-restore",
                            false, "Build Error")) return false;

                        if (!RunProcess("dotnet",
                            "publish ProjectZ.Desktop\\ProjectZ.Desktop.csproj -c Release -f net8.0 -r win-x64 --no-restore -p:PublishProfile=FolderProfile_GL",
                            false, "Build Error")) return false;
                    }
                }

                else if (Config.SelectedPlatform == Platform.Android)
                {
                    Config.Build_Path = Path.Combine(Config.Publish_Path, "Android");

                    if (!RunProcess("dotnet", "restore", false, "Restore Error")) return false;

                    if (!RunProcess("dotnet",
                        "publish ProjectZ.Android\\ProjectZ.Android.csproj -c Release -f net8.0-android --no-restore -p:PublishProfile=FolderProfile_Android",
                        false, "Build Error")) return false;
                }

                else if (Config.SelectedPlatform == Platform.Linux_x86)
                {
                    Config.Build_Path = Path.Combine(Config.Publish_Path, "Linux-x86_64");
                    string wslPath = ToWslPath(Config.Game_Source);

                    if (!RunProcess("wsl",
                        $"bash -c \"export MGFXC_WINE_PATH={DotNet.WinePath} && " +
                        $"cd {wslPath} && " +
                        $"dotnet restore ProjectZ.Linux/ProjectZ.Linux.csproj\"",
                        true, "Restore Error")) return false;

                    if (!RunProcess("wsl",
                        $"bash -c \"export MGFXC_WINE_PATH={DotNet.WinePath} && " +
                        $"cd {wslPath} && " +
                        $"dotnet publish ProjectZ.Linux/ProjectZ.Linux.csproj -c Release -f net8.0 -r linux-x64 --no-restore -p:PublishProfile=FolderProfile_Linux\"",
                        true, "Build Error")) return false;
                }

                else if (Config.SelectedPlatform == Platform.Linux_Arm64)
                {
                    Config.Build_Path = Path.Combine(Config.Publish_Path, "Linux-Arm64");
                    string wslPath = ToWslPath(Config.Game_Source);

                    if (!RunProcess("wsl",
                        $"bash -c \"export MGFXC_WINE_PATH={DotNet.WinePath} && " +
                        $"cd {wslPath} && " +
                        $"dotnet restore ProjectZ.Linux/ProjectZ.Linux.csproj\"",
                        true, "Restore Error")) return false;

                    if (!RunProcess("wsl",
                        $"bash -c \"export MGFXC_WINE_PATH={DotNet.WinePath} && " +
                        $"cd {wslPath} && " +
                        $"dotnet publish ProjectZ.Linux/ProjectZ.Linux.csproj -c Release -f net8.0 -r linux-arm64 --no-restore -p:PublishProfile=FolderProfile_Linux_Arm\"",
                        true, "Build Error")) return false;
                }

                else if (Config.SelectedPlatform == Platform.MacOS_Arm64)
                {
                    Config.Build_Path = Path.Combine(Config.Publish_Path, "MacOS-Arm64");
                    string wslPath = ToWslPath(Config.Game_Source);

                    if (!RunProcess("wsl",
                        $"bash -c \"export MGFXC_WINE_PATH={DotNet.WinePath} && " +
                        $"cd {wslPath} && " +
                        $"dotnet restore ProjectZ.MacOS/ProjectZ.MacOS.csproj\"",
                        true, "Restore Error")) return false;

                    if (!RunProcess("wsl",
                        $"bash -c \"export MGFXC_WINE_PATH={DotNet.WinePath} && " +
                        $"cd {wslPath} && " +
                        $"dotnet publish ProjectZ.MacOS/ProjectZ.MacOS.csproj -c Release -f net8.0 -r osx-arm64 --no-restore -p:PublishProfile=FolderProfile_MacOS\"",
                        true, "Build Error")) return false;
                }

                else if (Config.SelectedPlatform == Platform.MacOS_x64)
                {
                    Config.Build_Path = Path.Combine(Config.Publish_Path, "MacOS-x64");
                    string wslPath = ToWslPath(Config.Game_Source);

                    if (!RunProcess("wsl",
                        $"bash -c \"export MGFXC_WINE_PATH={DotNet.WinePath} && " +
                        $"cd {wslPath} && " +
                        $"dotnet restore ProjectZ.MacOS/ProjectZ.MacOS.csproj\"",
                        true, "Restore Error")) return false;

                    if (!RunProcess("wsl",
                        $"bash -c \"export MGFXC_WINE_PATH={DotNet.WinePath} && " +
                        $"cd {wslPath} && " +
                        $"dotnet publish ProjectZ.MacOS/ProjectZ.MacOS.csproj -c Release -f net8.0 -r osx-x64 --no-restore -p:PublishProfile=FolderProfile_MacOS_x64\"",
                        true, "Build Error")) return false;
                }
            }

            // If something didn't work out.
            catch (Exception ex)
            {
                Forms.OkayDialog.Display("Exception Caught", 250, 40, 27, 9, 15, "Exception: " + ex.Message);
            }

            // Return whether or not it actually succeeded.
            if (Config.SelectedPlatform == Platform.Windows)
            {
                string exePath = Path.Combine(Config.Build_Path, "Link's Awakening DX HD.exe");
                return exePath.TestPath();
            }
            else if (Config.SelectedPlatform == Platform.Android)
            {
                string apkPath = Path.Combine(Config.Build_Path, "com.zelda.ladxhd-Signed.apk");
                return apkPath.TestPath();
            }
            else if (Config.SelectedPlatform == Platform.Linux_x86 || Config.SelectedPlatform == Platform.Linux_Arm64)
            {
                string linuxBinPath = Path.Combine(Config.Build_Path, "Link's Awakening DX HD");
                return linuxBinPath.TestPath();
            }
            else if (Config.SelectedPlatform == Platform.MacOS_Arm64 || Config.SelectedPlatform == Platform.MacOS_x64)
            {
                string macBinPath = Path.Combine(Config.Build_Path, "Link's Awakening DX HD");
                return macBinPath.TestPath();
            }
            return false;
        }

        private static bool RunProcess(string executable, string arguments, bool isLinux, string errorTitle)
        {
            using (Process dotnet = new Process())
            {
                dotnet.StartInfo = new ProcessStartInfo
                {
                    // WSL manages its own working directory via the bash -c command,
                    // so only set WorkingDirectory for non-Linux builds.
                    WorkingDirectory = isLinux ? "" : Config.Game_Source,
                    FileName = executable,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                dotnet.Start();

                string output = dotnet.StandardOutput.ReadToEnd();
                string error = dotnet.StandardError.ReadToEnd();

                dotnet.WaitForExit();

                if (dotnet.ExitCode != 0)
                {
                    string message = string.IsNullOrWhiteSpace(error) ? output : error;
                    Forms.OkayDialog.Display(errorTitle, 250, 40, 27, 9, 15, message);
                    return false;
                }
            }

            return true;
        }

        // Converts a Windows path like C:\Users\Bighead\source to /mnt/c/Users/Bighead/source
        private static string ToWslPath(string windowsPath)
        {
            string full = Path.GetFullPath(windowsPath);
            string drive = full.Substring(0, 1).ToLower();
            string rest = full.Substring(2).Replace('\\', '/');
            return $"/mnt/{drive}{rest}";
        }
    }
}