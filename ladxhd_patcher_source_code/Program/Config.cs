using System;
using System.IO;
using System.Reflection;
using LADXHD_Migrater;

namespace LADXHD_Patcher
{
    public class Config
    {
        public const string Version = "1.7.5-mt2";

        public static string AppPath;
        public static string BaseFolder;
        public static string TempFolder;
        public static string ZeldaEXE;
        public static string BackupPath;

        public static string AnimationMods;
        public static string GraphicsMods;
        public static string MusicMods;
        public static string LanguageMods;
        public static string SoundsMods;
        public static string LAHDModPath;

        public static string Launcher;
        public static string WLauncher;

        public static string ApkSign;
        public static string ZipAlign;
        public static string KeyStore;
        public static string JavaExe;
        public static string SevenZip;

        public enum Platform { Windows, Android, Linux_x86, Linux_Arm64, MacOS_x86, MacOS_Arm64 }
        public static Platform SelectedPlatform;

        public enum GraphicsAPI { DirectX, OpenGL }
        public static GraphicsAPI SelectedGraphics;

        public static void Initialize()
        {
            AppPath      = Assembly.GetExecutingAssembly().Location;
            BaseFolder   = AppContext.BaseDirectory;;
            TempFolder   = Path.Combine(BaseFolder, "~temp");
            ZeldaEXE     = Path.Combine(BaseFolder, "Link's Awakening DX HD.exe");
            BackupPath   = Path.Combine(BaseFolder, "Data", "Backup");

            AnimationMods = Path.Combine(BaseFolder, "Mods", "Animations");
            GraphicsMods  = Path.Combine(BaseFolder, "Mods", "Graphics");
            MusicMods     = Path.Combine(BaseFolder, "Mods", "Music");
            LanguageMods  = Path.Combine(BaseFolder, "Mods", "Languages");
            SoundsMods    = Path.Combine(BaseFolder, "Mods", "SoundEffects");
            LAHDModPath   = Path.Combine(BaseFolder, "Mods", "LAHDMods");

            Launcher     = Path.Combine(BaseFolder, "Launcher");
            WLauncher    = Path.Combine(BaseFolder, "Launcher.exe");

            ApkSign      = Path.Combine(TempFolder, "android", "apksigner.jar");
            ZipAlign     = Path.Combine(TempFolder, "android", "zipalign.exe");
            KeyStore     = Path.Combine(TempFolder, "android", "keystore.jks");
            JavaExe      = Path.Combine(TempFolder, "android", "java", "bin", "java.exe");
            SevenZip     = Path.Combine(TempFolder, "7z.exe");

            CleanUp.Init();
        }
    }
}