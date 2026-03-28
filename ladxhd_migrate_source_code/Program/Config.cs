using System.IO;
using System.Reflection;

namespace LADXHD_Migrater
{
    internal class Config
    {
        public static string AppPath;
        public static string BaseFolder;
        public static string Patches;
        public static string Orig_Content;
        public static string Orig_Data;
        public static string Game_Source;
        public static string Migrate_Source;
        public static string Patcher_Source;
        public static string ModMaker_Source;
        public static string Update_Content;
        public static string Update_Data;
        public static string Publish_Path;
        public static string Build_Path;

        public enum Platform { Windows, Android, Linux_x86, Linux_Arm64, MacOS_x64, MacOS_Arm64  }
        public static Platform SelectedPlatform;

        public enum GraphicsAPI { DirectX, OpenGL }
        public static GraphicsAPI SelectedGraphics;

        public static void Initialize()
        {
            AppPath         = Assembly.GetExecutingAssembly().Location;
            BaseFolder      = Path.GetDirectoryName(AppPath);
            Patches         = Path.Combine(BaseFolder, "assets_patches");
            Orig_Content    = Path.Combine(BaseFolder, "assets_original", "Content");
            Orig_Data       = Path.Combine(BaseFolder, "assets_original", "Data");
            Game_Source     = Path.Combine(BaseFolder, "ladxhd_game_source_code");
            Migrate_Source  = Path.Combine(BaseFolder, "ladxhd_migrate_source_code");
            Patcher_Source  = Path.Combine(BaseFolder, "ladxhd_patcher_source_code");
            ModMaker_Source = Path.Combine(BaseFolder, "ladxhd_modmaker_source_code");
            Update_Content  = Path.Combine(Game_Source, "ProjectZ.Core", "Content");
            Update_Data     = Path.Combine(Game_Source, "ProjectZ.Core", "Data");
            Publish_Path    = Path.Combine(Game_Source, "~Publish");
            CleanUp.Init();
        }
    }
}
