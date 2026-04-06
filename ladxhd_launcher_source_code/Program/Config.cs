using System;
using System.Collections.Generic;
using System.IO;

namespace LADXHD_Launcher
{
    internal class Config
    {
        public const string Version = "1.0.0";

        public static string AppPath;
        public static string BaseFolder;
        public static string TempFolder;
        public static string ZeldaEXE;

        public static string LauncherConfig => Path.Combine(
            File.Exists(Path.Combine(BaseFolder, "portable.txt"))
                ? BaseFolder
                : Path.Combine(Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData), "Zelda_LA"), "launcher");

        private static Dictionary<string, object> resources = ResourceHelper.GetAllResources();

        public static void Initialize()
        {
            BaseFolder = AppContext.BaseDirectory;
            TempFolder = Path.Combine(BaseFolder, "~temp");

            #if WINDOWS
                AppPath  = Path.Combine(BaseFolder, "LADXHD_Launcher.exe");
                ZeldaEXE = Path.Combine(BaseFolder, "Link's Awakening DX HD.exe");
            #elif LINUX
                AppPath  = Path.Combine(BaseFolder, "LADXHD_Launcher");
                ZeldaEXE = Path.Combine(BaseFolder, "Link's Awakening DX HD");
            #elif MACOS
                AppPath  = Path.Combine(BaseFolder, "LADXHD_Launcher");
                ZeldaEXE = Path.Combine(BaseFolder, "Link's Awakening DX HD");
            #endif

            CreateDefaultFiles();
        }

        private static void CreateDefaultFiles()
        {
            // Check to see if the user has a "portable.txt" in their game directory.
            string portable = Path.Combine(BaseFolder, "portable.txt");
            string targetDir;

            // Use it to determine the target directory.
            if (File.Exists(portable))
                targetDir = BaseFolder;
            else
                targetDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Zelda_LA");

            // Ensure the target directory exists before writing into it
            Directory.CreateDirectory(targetDir);

            // Set the path to the files.
            string settingsPath = Path.Combine(targetDir, "settings");
            string advancedPath = Path.Combine(targetDir, "advanced");

            // If the doesn't exist we create them.
            if (!File.Exists(settingsPath) && resources.ContainsKey("settings"))
                File.WriteAllBytes(settingsPath, (byte[])resources["settings"]);

            if (!File.Exists(advancedPath) && resources.ContainsKey("advanced"))
                File.WriteAllBytes(advancedPath, (byte[])resources["advanced"]);
        }

        public static void SaveLauncherConfig()
        {
            File.WriteAllLines(LauncherConfig, new[]
            {
                $"SoundEnabled={XnbAudio.Enabled}"
            });
        }

        public static void LoadLauncherConfig()
        {
            if (!File.Exists(LauncherConfig)) return;

            foreach (string line in File.ReadAllLines(LauncherConfig))
            {
                int eq = line.IndexOf('=');
                if (eq < 0) continue;
                string key   = line[..eq].Trim();
                string value = line[(eq + 1)..].Trim();

                switch (key)
                {
                    case "SoundEnabled":
                        XnbAudio.Enabled = value.Equals("True", StringComparison.OrdinalIgnoreCase);
                        break;
                }
            }
        }
    }
}
