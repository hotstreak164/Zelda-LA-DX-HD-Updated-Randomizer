using System;
using System.IO;
using System.Reflection;
using LADXHD_ModMaker.Program;

namespace LADXHD_ModMaker
{
    internal class Config
    {
        public const string Version = "1.1.1";

        public static string AppName;
        public static string AppPath;
        public static string BaseFolder;

        public static string ModName = "";
        public static string Description = "";

        public static string GamePath;
        public static string DataPath;
        public static string BackupPath;
        public static string ModsPath;
        public static string GraphicsPath;
        public static string LahdmodPath;

        public static string ImagePath;
        public static string OutputPath;
        public static string TempPath;
        public static string PatchesPath;
        public static string OutLahdmodPath;

        public static bool PatchMode;

        public static void Initialize()
        {
            AppName = AppDomain.CurrentDomain.FriendlyName;
            AppPath = Assembly.GetExecutingAssembly().Location;
            BaseFolder = Path.GetDirectoryName(AppPath);

            string IniPath = Path.Combine(BaseFolder, "LAHDMOD.ini");

            if (IniPath.TestPath())
            {
                LADXHD_IniFile.Initialize(IniPath);
                LADXHD_IniFile.LoadINIValues();
                PatchMode = true;
            }
        }

        public static void UpdateGamePaths(string input)
        {
            GamePath = input;
            DataPath = Path.Combine(GamePath, "Data");
            BackupPath = Path.Combine(DataPath, "Backup");
            ModsPath = Path.Combine(GamePath, "Mods");
            GraphicsPath = Path.Combine(ModsPath, "Graphics");
            LahdmodPath = Path.Combine(ModsPath, "LAHDMods");
        }

        public static void UpdateOutputPaths(string output)
        {
            // The paths when creating patches.
            OutputPath = Path.Combine(output, "~ModOutput");
            TempPath = Path.Combine(OutputPath, "~temp");
            PatchesPath = Path.Combine(OutputPath, "Graphics");
            OutLahdmodPath = Path.Combine(OutputPath, "LAHDMods");
        }

        public static void UpdateOutputPaths_ApplyPatches()
        {
            // The paths when applying patches.
            OutputPath = Path.Combine(GamePath, "Mods", "Graphics");
            TempPath = Path.Combine(BaseFolder, "~temp");
            PatchesPath = Path.Combine(BaseFolder, "Graphics");
            OutLahdmodPath = Path.Combine(BaseFolder, "LAHDMods");
        }
    }
}
