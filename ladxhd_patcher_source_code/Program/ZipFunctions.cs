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
            if (Config.SelectedPlatform == Platform.Linux)
                return "patches_linux.zip";

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

        public static void ExtractAndroidFiles()
        {
            // Set the path to extract android files.
            string androidPath = Path.Combine(Config.TempFolder, "android").CreatePath();
            string zipFilePath = Path.Combine(Config.TempFolder, "android_files.zip");

            // Write the zipfile, extract it, then delete it.
            File.WriteAllBytes(zipFilePath, (byte[])resources["android_files.zip"]);
            ZipFile.ExtractToDirectory(zipFilePath, androidPath);
            zipFilePath.RemovePath();
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

        private static void SevenZipProcess(string inputPath, string outputPath, List<string> files, string compression)
        {
            // Write out the file list to HDD which is then read into 7z.
            string listPath = Path.Combine(Config.TempFolder, "7z_filelist.txt");
            File.WriteAllLines(listPath, files);

            // Run the 7zip process.
            RunFinishProcess(new ProcessStartInfo
            {
                FileName = Config.SevenZip,
                Arguments = $"a -tzip \"{outputPath}\" @\"{listPath}\" {compression}",
                WorkingDirectory = inputPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            // Remove the list for the next run.
            listPath.RemovePath();
        }

        public static void CreateUnsignedAPK(string inputPath, string outputPath)
        {
            // Remove any previous APK so 7z starts from a clean file.
            outputPath.RemovePath();

            // Old signature files should not be carried into the rebuilt APK.
            string metaInfPath = Path.Combine(inputPath, "META-INF");
            if (Directory.Exists(metaInfPath))
                Directory.Delete(metaInfPath, true);

            // Android is specific about how files are stored in the APK.
            var deflateFiles = new List<string>();
            var storeFiles = new List<string>();

            // Loop through all files and split them by desired compression method.
            foreach (string file in Directory.GetFiles(inputPath, "*", SearchOption.AllDirectories))
            {
                string relativePath = file.Substring(inputPath.Length).TrimStart('\\', '/');
                string entryName = relativePath.Replace("\\", "/");

                bool store = entryName.Equals("resources.arsc", StringComparison.OrdinalIgnoreCase) ||
                             entryName.Equals("assemblies/assemblies.arm64_v8a.blob", StringComparison.OrdinalIgnoreCase) ||
                             entryName.Equals("assemblies/assemblies.blob", StringComparison.OrdinalIgnoreCase) ||
                             entryName.Equals("assemblies/rc.bin", StringComparison.OrdinalIgnoreCase) ||
                             entryName.EndsWith(".png", StringComparison.OrdinalIgnoreCase);

                // Some files must be stored (no compression), others can be compressed.
                if (store)
                    storeFiles.Add(relativePath);
                else
                    deflateFiles.Add(relativePath);
            }
            // Add all normal files first using Deflate.
            if (deflateFiles.Count > 0)
                SevenZipProcess(inputPath, outputPath, deflateFiles, "-mx=9 -mm=Deflate");

            // Add all stored files second with no compression.
            if (storeFiles.Count > 0)
                SevenZipProcess(inputPath, outputPath, storeFiles, "-mx=0 -mm=Copy");
        }

        public static void ZipAlignAPK(string inputPath, string outputPath)
        {
            // Aligns the APK file in a way Android expects.
            RunFinishProcess(new ProcessStartInfo
            {
                FileName = Config.ZipAlign,
                Arguments = $"-f -v 4 \"{inputPath}\" \"{outputPath}\"",
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

        public static void ExtractLinuxFiles()
        {
            // Set the patches and zipfile paths.
            string zipFilePath = Path.Combine(Config.TempFolder, "linux_files.zip");

            // Write the zipfile, extract it, then delete it.
            File.WriteAllBytes(zipFilePath, (byte[])resources["linux_files.zip"]);
            ZipFile.ExtractToDirectory(zipFilePath, Config.BaseFolder);
            zipFilePath.RemovePath();
        }
    }
}