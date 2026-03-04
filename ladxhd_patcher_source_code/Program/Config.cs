using System.IO;
using System.Reflection;

namespace LADXHD_Patcher
{
    internal class Config
    {
        // The hash for "newHash" will need to be calculated for each new version.
        public const string Version = "1.6.4";

        public static string AppPath;
        public static string BaseFolder;
        public static string TempFolder;
        public static string ZeldaEXE;
        public static string BackupPath;

        public static string ContentPath;
        public static string DataPath;
        public static string PreviousModPath;
        public static string LAHDModPath;
        public static string GraphicsModPath;

        public enum Platform { Windows }
        public static Platform SelectedPlatform;

        public enum GraphicsAPI { DirectX, OpenGL }
        public static GraphicsAPI SelectedGraphics;

        public static void Initialize()
        {
            AppPath = Assembly.GetExecutingAssembly().Location;
            BaseFolder = Path.GetDirectoryName(AppPath);
            TempFolder = Path.Combine(BaseFolder, "~temp");
            ZeldaEXE = Path.Combine(BaseFolder, "Link's Awakening DX HD.exe");
            BackupPath = (Path.Combine(BaseFolder, "Data", "Backup")).CreatePath();

            ContentPath = Path.Combine(BaseFolder, "Content");
            DataPath = Path.Combine(BaseFolder, "Data");
            PreviousModPath = Path.Combine(DataPath, "Mods");
            LAHDModPath = Path.Combine(BaseFolder, "Mods", "LAHDMods");
            GraphicsModPath = Path.Combine(BaseFolder, "Mods", "Graphics");
        }
    }
}
