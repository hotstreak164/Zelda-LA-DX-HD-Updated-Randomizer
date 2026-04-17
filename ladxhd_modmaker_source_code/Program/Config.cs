using System;
using System.IO;
using System.Reflection;
using LADXHD_ModMaker.Program;

namespace LADXHD_ModMaker
{
    internal class Config
    {
        public const string Version = "1.3.0";

        public static bool PatchMode;

        public static string AppName;
        public static string AppPath;
        public static string BaseFolder;

        public static string ModName = "";
        public static string Description = "";

        public static string GamePath;
        public static string DataPath;
        public static string BackupPath;

        public static string ImagePath;
        public static string TempPath;
        public static string OutputPath;

        public static string AnimationMods;
        public static string DungeonMods;
        public static string GraphicsMods;
        public static string MusicMods;
        public static string LanguageMods;
        public static string MapsMods;
        public static string SoundsMods;
        public static string LAHDModPath;
        public static string ZScripts;

        public static string OutAnimationMods;
        public static string OutDungeonMods;
        public static string OutGraphicsMods;
        public static string OutMusicMods;
        public static string OutLanguageMods;
        public static string OutMapsMods;
        public static string OutSoundsMods;
        public static string OutLAHDModPath;
        public static string OutZScripts;

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
            GamePath      = input;
            DataPath      = Path.Combine(GamePath, "Data");
            BackupPath    = Path.Combine(DataPath, "Backup");
            AnimationMods = Path.Combine(GamePath, "Mods", "Animations");
            DungeonMods   = Path.Combine(GamePath, "Mods", "Dungeon");
            GraphicsMods  = Path.Combine(GamePath, "Mods", "Graphics");
            MusicMods     = Path.Combine(GamePath, "Mods", "Music");
            LanguageMods  = Path.Combine(GamePath, "Mods", "Languages");
            MapsMods      = Path.Combine(GamePath, "Mods", "Maps");
            SoundsMods    = Path.Combine(GamePath, "Mods", "SoundEffects");
            LAHDModPath   = Path.Combine(GamePath, "Mods", "LAHDMods");
            ZScripts      = Path.Combine(GamePath, "Mods", "scripts.zScript");
        }

        public static void UpdateOutputPaths(string output)
        {
            // The paths when creating patches.
            OutputPath       = Path.Combine(output, "~ModOutput");
            TempPath         = Path.Combine(OutputPath, "~temp");
            OutAnimationMods = Path.Combine(OutputPath, "Mods", "Animations");
            OutDungeonMods   = Path.Combine(OutputPath, "Mods", "Dungeon");
            OutGraphicsMods  = Path.Combine(OutputPath, "Mods", "Graphics");
            OutMusicMods     = Path.Combine(OutputPath, "Mods", "Music");
            OutLanguageMods  = Path.Combine(OutputPath, "Mods", "Languages");
            OutMapsMods      = Path.Combine(OutputPath, "Mods", "Maps");
            OutSoundsMods    = Path.Combine(OutputPath, "Mods", "SoundEffects");
            OutLAHDModPath   = Path.Combine(OutputPath, "Mods", "LAHDMods");
            OutZScripts      = Path.Combine(OutputPath, "Mods", "scripts.zScript");
        }

        public static void UpdateOutputPaths_ApplyPatches()
        {
            // The paths when applying patches.
            OutputPath       = Path.Combine(GamePath, "Mods", "Graphics");
            TempPath         = Path.Combine(BaseFolder, "~temp");
            OutAnimationMods = Path.Combine(BaseFolder, "Mods", "Animations");
            OutDungeonMods   = Path.Combine(BaseFolder, "Mods", "Dungeon");
            OutGraphicsMods  = Path.Combine(BaseFolder, "Mods", "Graphics");
            OutMusicMods     = Path.Combine(BaseFolder, "Mods", "Music");
            OutLanguageMods  = Path.Combine(BaseFolder, "Mods", "Languages");
            OutMapsMods      = Path.Combine(BaseFolder, "Mods", "Maps");
            OutSoundsMods    = Path.Combine(BaseFolder, "Mods", "SoundEffects");
            OutLAHDModPath   = Path.Combine(BaseFolder, "Mods", "LAHDMods");
            OutZScripts      = Path.Combine(BaseFolder, "Mods", "scripts.zScript");
        }
    }
}
