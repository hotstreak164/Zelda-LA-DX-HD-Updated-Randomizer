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

        public static void Initialize()
        {
            AppPath         = Assembly.GetExecutingAssembly().Location;
            BaseFolder      = Path.GetDirectoryName(AppPath);
            Patches         = BaseFolder + "\\assets_patches";
            Orig_Content    = BaseFolder + "\\assets_original\\Content";
            Orig_Data       = BaseFolder + "\\assets_original\\Data";
            Game_Source     = BaseFolder + "\\ladxhd_game_source_code";
            Migrate_Source  = BaseFolder + "\\ladxhd_migrate_source_code";
            Patcher_Source  = BaseFolder + "\\ladxhd_patcher_source_code";
            ModMaker_Source = BaseFolder + "\\ladxhd_modmaker_source_code";
            Update_Content  = Game_Source + "\\ProjectZ.Core\\Content";
            Update_Data     = Game_Source + "\\ProjectZ.Core\\Data";
            Publish_Path    = Game_Source + "\\ProjectZ.Core\\Publish";
        }
    }
}
