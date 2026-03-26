using System.IO;
using System.Reflection;
using LADXHD_Migrater;

namespace LADXHD_Patcher
{
    internal class Config
    {
        public const string Version = "1.6.9";

        public static string AppPath;
        public static string BaseFolder;
        public static string TempFolder;
        public static string ZeldaEXE;
        public static string BackupPath;

        public static string LAHDModPath;
        public static string Graphics;

        public static string ApkSign;
        public static string ZipAlign;
        public static string KeyStore;
        public static string JavaExe;
        public static string SevenZip;

        public enum Platform { Windows, Android, Linux_x86, Linux_Arm64 }
        public static Platform SelectedPlatform;

        public enum GraphicsAPI { DirectX, OpenGL }
        public static GraphicsAPI SelectedGraphics;

        public static void Initialize()
        {
            AppPath     = Assembly.GetExecutingAssembly().Location;
            BaseFolder  = Path.GetDirectoryName(AppPath);
            TempFolder  = Path.Combine(BaseFolder, "~temp");
            ZeldaEXE    = Path.Combine(BaseFolder, "Link's Awakening DX HD.exe");
            BackupPath  = Path.Combine(BaseFolder, "Data", "Backup");
            LAHDModPath = Path.Combine(BaseFolder, "Mods", "LAHDMods");
            Graphics    = Path.Combine(BaseFolder, "Mods", "Graphics");
            ApkSign     = Path.Combine(TempFolder, "android", "apksigner.jar");
            ZipAlign    = Path.Combine(TempFolder, "android", "zipalign.exe");
            KeyStore    = Path.Combine(TempFolder, "android", "keystore.jks");
            JavaExe     = Path.Combine(TempFolder, "android", "java", "bin", "java.exe");
            SevenZip    = Path.Combine(TempFolder, "7z.exe");
            CleanUp.Init();
        }
    }
}
