using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using static LADXHD_Patcher.Config;

namespace LADXHD_Patcher
{
    public class ZipFunctions
    {
        private static Dictionary<string, object> resources = ResourceHelper.GetAllResources();

        private static string GetPatchesName()
        {
            // If Android is selected then choose its patches.
            if (Config.SelectedPlatform == Platform.Android)
                return "patches_android.zip";

            // If Android is selected then choose its patches.
            if (Config.SelectedPlatform == Platform.Linux_x86)
                return "patches_linux_x86.zip";

            // If Android is selected then choose its patches.
            if (Config.SelectedPlatform == Platform.Linux_Arm64)
                return "patches_linux_arm64.zip";

            // If Windows and OpenGL is selected choose those patches.
            if (Config.SelectedPlatform == Platform.Windows)
                if (Config.SelectedGraphics == GraphicsAPI.OpenGL)
                    return "patches_win_gl.zip";

            // Default to Windows Direct-X patches.
            return "patches_win_dx.zip";
        }

        public static void ExtractPatches()
        {
            // Get the patches file we need.
            string zipName = GetPatchesName();

            // Set the patches and zipfile paths.
            string patchesPath = Path.Combine(Config.TempFolder, "patches").CreatePath();
            string patchedPath = Path.Combine(Config.TempFolder, "patchedFiles").CreatePath();
            string zipFilePath = Path.Combine(Config.TempFolder, zipName);

            // Write the zipfile, extract it, then delete it.
            File.WriteAllBytes(zipFilePath, (byte[])resources[zipName]);
            ZipFile.ExtractToDirectory(zipFilePath, patchesPath);
            zipFilePath.RemovePath();
        }

        public static void ExtractAndroidIcons()
        {
            string stageRoot   = Path.Combine(Config.TempFolder, "android", "com.zelda.ladxhd");
            string buttonsPath = Path.Combine(stageRoot, "assets", "Data", "Buttons").CreatePath();
            string zipFilePath = Path.Combine(Config.TempFolder, "android_buttons.zip");

            File.WriteAllBytes(zipFilePath, (byte[])resources["android_buttons.zip"]);
            ZipFile.ExtractToDirectory(zipFilePath, buttonsPath);
            zipFilePath.RemovePath();
        }

        public static void ExtractAndroidBaseApk()
        {
            string androidPath = Path.Combine(Config.TempFolder, "android").CreatePath();
            string apkPath = Path.Combine(androidPath, "unsigned.apk");
            File.WriteAllBytes(apkPath, (byte[])resources["android_base.apk"]);
        }

        public static void UpdateApkAssets(string apkPath, string stageRoot)
        {
            RunFinishProcess(new ProcessStartInfo
            {
                FileName = Config.SevenZip,
                Arguments = $"a -tzip \"{apkPath}\" \"assets\\Content\\*\" \"assets\\Data\\*\" -r -mx=9 -mm=Deflate",
                WorkingDirectory = stageRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }

        public static void ExtractAndroidTools()
        {
            // Set the path to extract android tools.
            string androidPath = Path.Combine(Config.TempFolder, "android").CreatePath();
            string zipFilePath = Path.Combine(Config.TempFolder, "android_tools.zip");

            // Write the zipfile, extract it, then delete it.
            File.WriteAllBytes(zipFilePath, (byte[])resources["android_tools.zip"]);
            ZipFile.ExtractToDirectory(zipFilePath, androidPath);
            zipFilePath.RemovePath();
        }

        public static void ExtractSevenZip()
        {
            // Set the path to extract android tools.
            string sevenZipPath = Path.Combine(Config.TempFolder, "7zip.zip");

            // Write the zipfile, extract it, then delete it.
            File.WriteAllBytes(sevenZipPath, (byte[])resources["7zip.zip"]);
            ZipFile.ExtractToDirectory(sevenZipPath, Config.TempFolder);
            sevenZipPath.RemovePath();
        }

        private static void RunFinishProcess(ProcessStartInfo startInfo)
        {
            // Start a process, report any exceptions, and close out the process.
            using (var proc = new Process { StartInfo = startInfo })
            {
                proc.Start();

                string output = null, errors = null;
                var outTask = Task.Run(() => output = proc.StandardOutput.ReadToEnd());
                var errTask = Task.Run(() => errors = proc.StandardError.ReadToEnd());

                proc.WaitForExit();
                Task.WaitAll(outTask, errTask);

                if (proc.ExitCode != 0)
                    throw new Exception(startInfo.FileName + ":\nOUTPUT:\n" + output + "\nERRORS:\n" + errors);

                proc.Dispose();
            }
        }

        public static void ZipAlignAPK(string inputPath, string outputPath)
        {
            // Aligns the APK file in a way Android expects.
            RunFinishProcess(new ProcessStartInfo
            {
                FileName = Config.ZipAlign,
                Arguments = $"-P 16 -f -v 4 \"{inputPath}\" \"{outputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }

        public static void SignAPK(string inputPath, string outputPath)
        {
            // Signs the APK using the keystore provided in the "android_tools" zip file.
            RunFinishProcess(new ProcessStartInfo
            {
                FileName = Config.JavaExe,
                Arguments = $"-jar \"{Config.ApkSign}\" sign --ks \"{Config.KeyStore}\" --ks-key-alias zelda-la --ks-pass pass:zeldala --out \"{outputPath}\" \"{inputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }

        public static void VerifyAPK(string apkPath)
        {
            RunFinishProcess(new ProcessStartInfo
            {
                FileName = Config.ZipAlign,
                Arguments = $"-c -P 16 -v 4 \"{apkPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            RunFinishProcess(new ProcessStartInfo
            {
                FileName = Config.JavaExe,
                Arguments = $"-jar \"{Config.ApkSign}\" verify -v \"{apkPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
    }
}