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

        public static string LAHDModPath;
        public static string GraphicsModPath;

        public static string ApkSign;
        public static string ZipAlign;
        public static string KeyStore;
        public static string JavaExe;
        public static string SevenZip;

        public enum Platform { Windows, Android, Linux }
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

            LAHDModPath = Path.Combine(BaseFolder, "Mods", "LAHDMods");
            GraphicsModPath = Path.Combine(BaseFolder, "Mods", "Graphics");

            ApkSign  = Path.Combine(TempFolder, "android", "apksigner.jar");
            ZipAlign = Path.Combine(TempFolder, "android", "zipalign.exe");
            KeyStore = Path.Combine(TempFolder, "android", "keystore.jks");
            JavaExe  = Path.Combine(TempFolder, "android", "java", "bin", "java.exe");
            SevenZip = Path.Combine(TempFolder, "android", "7z.exe");
        }
    }
}
