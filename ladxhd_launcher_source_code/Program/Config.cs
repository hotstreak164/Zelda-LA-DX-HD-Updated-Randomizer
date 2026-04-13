using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        private static Dictionary<string, string> LoadRawValues(string path)
        {
            // Preserve the user's stored values in a dictionary.
            var values = new Dictionary<string, string>();
            string currentSection = null;

            // Read through each line of the "advanced" file.
            foreach (string raw in File.ReadAllLines(path))
            {
                // Trim whitespace and get the current section.
                string line = raw.Trim();
                if (line.StartsWith("[") && line.EndsWith("]"))
                    currentSection = line[1..^1];

                // Look for a line with a key-value pair that is not a comment and is part of a section.
                if (line.Contains('=') && !line.StartsWith("//") && currentSection != null)
                {
                    // Crop out the key and value.
                    int eq     = line.IndexOf('=');
                    string key = line[..eq].Trim();
                    string val = line[(eq + 1)..].Trim();

                    // Store the current key-value pair.
                    if (!string.IsNullOrEmpty(key))
                        values[$"{currentSection}|{key}"] = val;
                }
            }
            // Return the dictionary.
            return values;
        }

        private static void MergeValuesIntoFile(string path, Dictionary<string, string> oldValues)
        {
            // Read the new file and create a list to merge old and new.
            var lines  = File.ReadAllLines(path);
            var output = new List<string>();
            string currentSection = null;

            // Loop through each line in the new file.
            foreach (string raw in lines)
            {
                // Trim white space and find current section.
                string line = raw.Trim();
                if (line.StartsWith("[") && line.EndsWith("]"))
                    currentSection = line[1..^1];

                // Look for a line with a key-value pair that is not a comment and is part of a section.
                if (line.Contains('=') && !line.StartsWith("//") && currentSection != null)
                {
                    // Crop out the key and value.
                    int eq      = line.IndexOf('=');
                    string key  = line[..eq].Trim();
                    string newVal = line[(eq + 1)..].Trim();
                    string vk   = $"{currentSection}|{key}";

                    // Get the old value from the key in the current section.
                    if (oldValues.TryGetValue(vk, out string oldVal))
                    {
                        // Variable names may remain the same but value types might change (ex: int to float).
                        bool oldIsBool  = oldVal.Equals("true", StringComparison.OrdinalIgnoreCase) || oldVal.Equals("false", StringComparison.OrdinalIgnoreCase);
                        bool newIsBool  = newVal.Equals("true", StringComparison.OrdinalIgnoreCase) || newVal.Equals("false", StringComparison.OrdinalIgnoreCase);
                        bool oldIsFloat = !oldIsBool && oldVal.Contains('.');
                        bool newIsFloat = !newIsBool && newVal.Contains('.');

                        // Only restore the old value if types match.
                        if (oldIsBool == newIsBool && oldIsFloat == newIsFloat)
                        {
                            int rawEq  = raw.IndexOf('=');
                            string lhs = raw[..rawEq];
                            output.Add($"{lhs}= {oldVal}");
                            continue;
                        }
                    }
                }
                // Add the line the output.
                output.Add(raw);
            }
            // Write the merged file to the output.
            File.WriteAllLines(path, output);
        }

        private static void CreateDefaultFiles()
        {
            // Get the target path. User may store saves locally with portable.txt file.
            string portable  = Path.Combine(BaseFolder, "portable.txt");
            string targetDir = File.Exists(portable)
                ? BaseFolder
                : Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData), "Zelda_LA");

            // Make sure the directory exists.
            Directory.CreateDirectory(targetDir);

            // Set the paths to the two save files.
            string settingsPath = Path.Combine(targetDir, "settings");
            string advancedPath = Path.Combine(targetDir, "advanced");

            // Create "settings" file if it is missing.
            if (!File.Exists(settingsPath) && resources.ContainsKey("settings"))
                File.WriteAllBytes(settingsPath, (byte[])resources["settings"]);

            // Create "advanced" if missing or merge new with old if it exists.
            if (resources.ContainsKey("advanced"))
            {
                // It doesn't exist so simply create a new one.
                if (!File.Exists(advancedPath))
                {
                    File.WriteAllBytes(advancedPath, (byte[])resources["advanced"]);
                }
                // It does exist so it's about to get complicated...
                else
                {
                    // Load the user's stored values and merge them into the new file.
                    var oldValues = LoadRawValues(advancedPath);
                    File.WriteAllBytes(advancedPath, (byte[])resources["advanced"]);
                    MergeValuesIntoFile(advancedPath, oldValues);
                }
            }
        }

        public static void SaveLauncherConfig()
        {
            // Saves the launcher's values to the config file.
            File.WriteAllLines(LauncherConfig, new[]
            {
                $"SoundEnabled={XnbAudio.Enabled}",
                $"WindowHeight={App.MainWindowInstance?.Height ?? 768}"
            });
        }

        public static void LoadLauncherConfig()
        {
            // If there is no config there is nothing to load.
            if (!File.Exists(LauncherConfig)) 
                return;

            // Loop through the lines in the config.
            foreach (string line in File.ReadAllLines(LauncherConfig))
            {
                // Get the keys and values.
                int eq = line.IndexOf('=');
                if (eq < 0) continue;
                string key   = line[..eq].Trim();
                string value = line[(eq + 1)..].Trim();

                // Load the user's settings and apply to the menu.
                switch (key)
                {
                    case "SoundEnabled":
                        XnbAudio.Enabled = value.Equals("True", StringComparison.OrdinalIgnoreCase);
                        break;
                    case "WindowHeight":
                        if (double.TryParse(value, out double h))
                            App.SavedWindowHeight = h;
                        break;
                }
            }
        }
    }
}
