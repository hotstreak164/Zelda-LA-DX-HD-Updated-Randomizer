using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LADXHD_Migrater;
using static LADXHD_Patcher.Config;
using static LADXHD_Patcher.XDelta3;

namespace LADXHD_Patcher
{
    internal class Functions
    {
        private static int    _patchProgress;
        private static int    _fileCount;
        private static int    _totalCount;
        private static int    _filesPatched;
        private static bool   _silentMode;
        private static bool   _patchFromBackup;
        private static string _executable;

        private static Dictionary<string, object> resources = ResourceHelper.GetAllResources();

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        FILE MAPPING CODE : NOT ALL FILES AND PATCHES ARE 1:1 FROM ORIGINAL GAME VERSION. NEW FILES NEED A "BASE" TO BE CREATED FROM USING A PATCH
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        // SOME RESOURCES ARE USED TO CREATE MULTIPLE FILES OR PATCHES. EACH ARRAY BELOW HOLDS ALL VERSIONS OF A FILE THAT IS
        // BASED OFF OF ANOTHER FILE. THE "MASTER" FILE THAT CREATES THESE VERSIONS IS LINKED TO THEM IN THE DICTIONARY BELOW

        private static string[] langFiles  = new[] { "chn.lng", "deu.lng", "esp.lng", "fre.lng", "ind.lng", "ita.lng", "por.lng", "rus.lng" };
        private static string[] langDialog = new[] { "dialog_chn.lng", "dialog_deu.lng", "dialog_esp.lng", "dialog_fre.lng", "dialog_ind.lng", "dialog_ita.lng", "dialog_por.lng", "dialog_rus.lng" };
        private static string[] smallFonts = new[] { "smallFont_redux.xnb", "smallFont_vwf.xnb", "smallFont_vwf_redux.xnb", "smallFont_chn.xnb", "smallFont_chn_0.xnb", "smallFont_chn_redux.xnb", "smallFont_chn_redux_0.xnb" };
        private static string[] backGround = new[] { "menuBackgroundB.xnb", "menuBackgroundC.xnb", "sgb_border.xnb" };
        private static string[] lighting   = new[] { "mamuLight.xnb" };
        private static string[] linkImages = new[] { "link1.png" };
        private static string[] npcImages  = new[] { "npcs_redux.png" };
        private static string[] itemImages = new[] { "items_chn.png", "items_deu.png", "items_esp.png", "items_fre.png", "items_ind.png", "items_ita.png", "items_por.png", "items_rus.png", "items_redux.png", 
                                                     "items_redux_chn.png", "items_redux_deu.png", "items_redux_esp.png", "items_redux_fre.png", "items_redux_ind.png", "items_redux_ita.png", "items_redux_por.png", "items_redux_rus.png" };
        private static string[] introImage = new[] { "intro_chn.png", "intro_deu.png", "intro_esp.png", "intro_fre.png", "intro_ind.png", "intro_ita.png", "intro_por.png", "intro_rus.png" };
        private static string[] introAtlas = new[] { "intro_chn.atlas" };
        private static string[] miniMapImg = new[] { "minimap_chn.png", "minimap_deu.png", "minimap_esp.png", "minimap_fre.png", "minimap_ind.png", "minimap_ita.png", "minimap_por.png", "minimap_rus.png" };
        private static string[] objectsImg = new[] { "objects_chn.png", "objects_deu.png", "objects_esp.png", "objects_fre.png", "objects_ind.png", "objects_ita.png", "objects_por.png", "objects_rus.png" };
        private static string[] photograph = new[] { "photos_chn.png", "photos_deu.png", "photos_esp.png", "photos_fre.png",  "photos_ind.png", "photos_ita.png", "photos_por.png", "photos_rus.png", "photos_redux.png", 
                                                     "photos_redux_chn.png", "photos_redux_deu.png", "photos_redux_esp.png", "photos_redux_fre.png", "photos_redux_ind.png", "photos_redux_ita.png", "photos_redux_por.png", "photos_redux_rus.png" };
        private static string[] uiImages   = new[] { "ui_chn.png", "ui_deu.png", "ui_esp.png", "ui_fre.png", "ui_ind.png", "ui_ita.png", "ui_por.png", "ui_rus.png" };
        private static string[] musicTile  = new[] { "musicOverworldClassic.data" };
        private static string[] dungeon3M  = new[] { "dungeon3.map" };
        private static string[] dungeon3D  = new[] { "dungeon3.map.data" };
        private static string[] bowwowanim = new[] { "bowwow_water.ani" };
        private static string[] dungeonani = new[] { "mapDungeon.ani", "mapManboPond.ani" };

        // THE "KEY" IS THE MASTER FILE THAT CREATES OTHER FILES FROM IT. THE "VALUE" IS THE STRING ARRAY THAT HOLDS THOSE FILES

        private static readonly Dictionary<string, string[]> fileTargets = new Dictionary<string, string[]>
        {
            { "eng.lng",              langFiles },
            { "dialog_eng.lng",      langDialog },
            { "smallFont.xnb",       smallFonts },
            { "menuBackground.xnb",  backGround },
            { "ligth room.xnb",        lighting },
            { "link0.png",           linkImages },
            { "npcs.png",             npcImages },
            { "items.png",           itemImages },
            { "intro.png",           introImage },
            { "intro.atlas",         introAtlas },
            { "minimap.png",         miniMapImg },
            { "objects.png",         objectsImg },
            { "photos.png",          photograph },
            { "ui.png",                uiImages },
            { "musicOverworld.data",  musicTile },
            { "dungeon3_1.map",       dungeon3M },
            { "dungeon3_1.map.data",  dungeon3D },
            { "BowWow.ani",          bowwowanim },
            { "mapPlayer.ani",       dungeonani }
        };

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        PROGRESS CODE : TRACK AND UPDATE THE PROGRESS BAR BY KEEPING TRACK OF THE FILE COUNTS.
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private static void ShowWarning(string title, string message, int width, int height, int titleSize, int titleHeight, int bodySize)
        {
            if (_silentMode)
                Console.WriteLine("WARNING: " + message);
            else
                Forms.OkayDialog.Display(title, width, height, titleSize, titleHeight, bodySize, message);
        }

        private static void ResetProgress()
        {
            // Reset the variables.
            _fileCount = 0;
            _totalCount = 0;
            _patchProgress = 0;

            // Update the bar to have zero progress.
            Forms.MainDialog.UpdateProgressBar(_patchProgress);
        }

        private static void UpdateProgress()
        {
            // Update the file count.
            _fileCount++;

            // Get the percentage of progress.
            int progress = (int)(_fileCount * 100.0 / _totalCount);

            // Update the progress bar and process UI events (GUI mode only).
            if (!_silentMode)
            {
                if (_fileCount % 10 == 0 || _fileCount == _totalCount)
                    Forms.MainDialog.UpdateProgressBar(progress);

                Application.DoEvents();
            }
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        POST PATCHING CODE : GENERATE ANDROID APK
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private static void GenerateAPKFile()
        {
            // Paths to the temporary and final APK files.
            string androidPath = Path.Combine(Config.TempFolder, "android").CreatePath();
            string stageRoot   = Path.Combine(androidPath, "com.zelda.ladxhd");
            string apkUnsigned = Path.Combine(androidPath, "unsigned.apk");
            string apkAligned  = Path.Combine(androidPath, "aligned.apk");
            string apkSigned   = Path.Combine(androidPath, "signed.apk");
            string apkFinalize = Path.Combine(Config.BaseFolder, "zelda.ladxhd.apk");

            // Clean up any previous APK files.
            apkUnsigned.RemovePath();
            apkAligned.RemovePath();
            apkSigned.RemovePath();
            apkFinalize.RemovePath();

            // Extract tools and the stripped base APK.
            Utilities.ExtractResourcesZip("android_tools.zip", androidPath);;
            Utilities.ExtractResourcesZip("7zip.zip", Config.TempFolder);;

            // Write the base APK from resources.
            File.WriteAllBytes(apkUnsigned, (byte[])resources["android_base.apk"]);

            // Inject only Content/Data into the base APK, then align/sign/verify.
            Utilities.RunProcess(Config.SevenZip, stageRoot, $"a -tzip \"{apkUnsigned}\" \"assets\\Content\\*\" \"assets\\Data\\*\" -r -mx=9 -mm=Deflate");
            Utilities.RunProcess(Config.ZipAlign, stageRoot, $"-P 16 -f -v 4 \"{apkUnsigned}\" \"{apkAligned}\"");
            Utilities.RunProcess(Config.JavaExe,  stageRoot, $"-jar \"{Config.ApkSign}\" sign --ks \"{Config.KeyStore}\" --ks-key-alias zelda-la --ks-pass pass:zeldala --out \"{apkSigned}\" \"{apkAligned}\"");
            Utilities.RunProcess(Config.ZipAlign, stageRoot, $"-c -P 16 -v 4 \"{apkSigned}\"");
            Utilities.RunProcess(Config.JavaExe,  stageRoot, $"-jar \"{Config.ApkSign}\" verify -v \"{apkSigned}\"");

            // Remove the temporary APK files we no longer need.
            apkUnsigned.RemovePath();
            apkAligned.RemovePath();

            // Move the final APK to the root folder.
            apkSigned.MovePath(apkFinalize, true);
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        POST PATCHING CODE : DUNGEON 3 FIX
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private static void Dungeon3PatchFix()
        {
            // I fucked up. After the dungeon name change the file "dungeon3_1.map" no longer exists.
            string d3map = Path.Combine(Config.BaseFolder, "Data", "Maps", "dungeon3_1.map");

            // If it doesn't exist in the original path, check the backup folder.
            if (!d3map.TestPath())
                d3map = Path.Combine(Config.BackupPath, "dungeon3_1.map");

            // Look for the backup dungeon 3 file as it should still exist.
            if (d3map.TestPath())
            {
                // Patch the file directly into the maps folder.
                string xdelta3File = Path.Combine(Config.TempFolder, "patches", "dungeon3.map.xdelta");
                string patchedFile = Path.Combine(Config.BaseFolder, "Data", "Maps", "dungeon3.map");

                if (Config.SelectedPlatform == Platform.Android)
                    patchedFile = Path.Combine(Config.TempFolder, "android", "com.zelda.ladxhd", "assets", "Data", "Maps", "dungeon3.map");

                XDelta3.Execute(Operation.Apply, d3map, xdelta3File, patchedFile);
            }
            // If the dungeon 3 map file was not able to be patched, output an error message.
            else
            {
                ShowWarning("Patching \"dungeon3.map\" Failed", "Unable to locate or patch the correct map file for dungeon 3. The patch may succeed but this dungeon will crash the game!", 260, 40, 34, 16, 10);
            }
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        POST PATCHING CODE : LINUX / MACOS FINALIZATION SCRIPTS
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        // Converts a Wine Windows path (e.g. "Z:\Users\foo\bar") to a native Unix path ("/Users/foo/bar").
        // Strips the leading drive letter and colon (always a single letter under Wine/Windows) via regex.
        private static string ToUnixPath(string windowsPath)
        {
            return Regex.Replace(windowsPath, @"^[A-Za-z]:", "").Replace('\\', '/');
        }

        // Writes a finalization script to TempFolder and fires it via /bin/sh, then polls for the
        // sentinel file the script writes on success. Shared by the Linux and macOS paths.
        // Wine's WaitForExit() is unreliable for native processes, so we use fire-and-forget
        // plus a sentinel file written by the script as its very last step.
        private static void RunUnixFinalizeScript(string scriptResource)
        {
            if (!HostEnvironment.IsWine)
                return;

            // Normalize line endings to LF in case the embedded resource contains CRLFs
            // then write raw bytes so File.WriteAllText's StreamWriter cannot convert \n → \r\n.
            byte[] scriptBytes   = (byte[])resources[scriptResource];
            string scriptContent = System.Text.Encoding.UTF8.GetString(scriptBytes).Replace("\r\n", "\n");
            scriptBytes          = System.Text.Encoding.UTF8.GetBytes(scriptContent);
            string scriptWinPath = Path.Combine(Config.TempFolder, "finalize.sh");
            File.WriteAllBytes(scriptWinPath, scriptBytes);

            string scriptNativePath = ToUnixPath(scriptWinPath);
            string baseFolder       = ToUnixPath(Config.BaseFolder);
            string executableName   = Path.GetFileNameWithoutExtension(Config.ZeldaEXE);
            string sentinelWinPath  = Path.Combine(Config.TempFolder, "finalize.done");

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName        = "/bin/sh",
                    Arguments       = $"\"{scriptNativePath}\" \"{baseFolder}\" \"{executableName}\"",
                    UseShellExecute = false,
                    CreateNoWindow  = true,
                };
                Process.Start(psi);
            }
            catch
            {
                return;
            }

            // Poll for the sentinel file written by the script on successful completion.
            // This is the synchronisation mechanism — without it the patcher exits before the
            // script finishes chmod, codesign, and the app bundle copy.
            const int interval =   500; // ms between checks
            const int timeout  = 60000; // ms — generous to allow for cp -rp of large game data
            int elapsed = 0;
            while (!File.Exists(sentinelWinPath) && elapsed < timeout)
            {
                System.Threading.Thread.Sleep(interval);
                elapsed += interval;
            }
        }

        private static void RunLinuxFinalizeScript()
        {
            RunUnixFinalizeScript("finalize_linux.sh");
        }

        private static void RunMacOSFinalizeScript()
        {
            // Write Icon.icns and the rendered Info.plist to TempFolder so the script can copy
            // them into the bundle without needing access to managed resources.
            string executableName = Path.GetFileNameWithoutExtension(Config.ZeldaEXE);
            string arch           = Config.SelectedPlatform == Platform.MacOS_x86 ? "x86_64" : "arm64";

            File.WriteAllBytes(Path.Combine(Config.TempFolder, "Icon.icns"),
                               (byte[])resources["Icon.icns"]);

            string template = System.Text.Encoding.UTF8.GetString((byte[])resources["Info.plist.template"]);
            string plist = template
                .Replace("{EXECUTABLE}", executableName)
                .Replace("{VERSION}",    Config.Version)
                .Replace("{ARCH}",       arch);
            File.WriteAllText(Path.Combine(Config.TempFolder, "Info.plist"), plist,
                              new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            RunUnixFinalizeScript("finalize_macos.sh");
        }

        private static void HostFinalizationFunctions()
        {
            bool isLinux = Config.SelectedPlatform == Platform.Linux_x86 || Config.SelectedPlatform == Platform.Linux_Arm64;
            bool isMacOS = Config.SelectedPlatform == Platform.MacOS_x86 || Config.SelectedPlatform == Platform.MacOS_Arm64;
            
            // Finalization steps are platform-specific and should only run when patching on that platform.
            if (isLinux && HostEnvironment.IsLinux)
            {
                RunLinuxFinalizeScript();
            }
            else if (isMacOS && HostEnvironment.IsMacOS)
            {
                RunMacOSFinalizeScript();
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

                POST PATCHING CODE : ADDITIONAL FILE AND FOLDER HANDLING

        -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private static void CopyNewFiles()
        {
            string dataPath;

            // Set the path to "Data" based on platform selected.
            if (Config.SelectedPlatform == Platform.Android)
                dataPath = Path.Combine(Config.TempFolder, "android", "com.zelda.ladxhd", "assets", "Data");
            else
                dataPath = Path.Combine(Config.BaseFolder, "Data");

            // Set up the path to the Icon.
            string iconPath = Path.Combine(dataPath, "Icon").CreatePath();

            // Write the icon to the "Data\Icon" folder.
            string iconFile = Path.Combine(iconPath, "Icon.ico");
            File.WriteAllBytes(iconFile, (byte[])resources["Icon.ico"]);

            // Write the bitmap icon to the "Data\Icon" folder.
            string iconBmpFile = Path.Combine(iconPath, "Icon.bmp");
            File.WriteAllBytes(iconBmpFile, (byte[])resources["Icon.bmp"]);

            // Write the png icon to the the "Data\Icon" folder.
            string iconPngFile = Path.Combine(iconPath, "Icon.png");
            File.WriteAllBytes(iconPngFile, (byte[])resources["Icon.png"]);

            // If it's the Windows OpenGL build then it needs SDL2.dll.
            if (Config.SelectedPlatform == Platform.Windows && Config.SelectedGraphics == GraphicsAPI.OpenGL)
            {
                string SdlPath = Path.Combine(Config.BaseFolder, "SDL2.dll");
                File.WriteAllBytes(SdlPath, (byte[])resources["SDL2.dll"]);
            }
        }

        private static void RemoveBadBackupFiles()
        {
            // Because old versions of the patchers saved "new" files, we need to remove them or they will cause problems.
            string[][] list = { langFiles, langDialog, smallFonts, backGround, lighting, linkImages, npcImages, itemImages, introImage, introAtlas, 
                                miniMapImg, objectsImg, photograph, uiImages, musicTile, dungeon3M, dungeon3D, bowwowanim, dungeonani };

            string[] remove = list.SelectMany(x => x).ToArray();

            // Loop through the files in the backup folder.
            foreach (string file in Config.BackupPath.GetFiles("*", true))
            {
                // Get the file as a file item which gives us some cool properties to reference.
                FileItem fileItem = new FileItem(file);

                // If the current array file exists then remove it.
                if (remove.Contains(fileItem.Name))
                    fileItem.FullName.RemovePath();
            }
        }

        private static void CreateModFolders()
        {
            // The path to where Mods used to be located.
            string previousModPath = Path.Combine(Config.BaseFolder, "Data", "Mods");

            // Create the new mods folders.
            Config.LAHDModPath.CreatePath(true);
            Config.Graphics.CreatePath(true);

            // Find the old "Mods" path for lahdmods and exit if it doesn't exist.
            if (!Directory.Exists(previousModPath))
                return;

            // Move any lahdmods in the old "Mods" folder to the new location.
            foreach (string file in Directory.GetFiles(previousModPath, "*", SearchOption.AllDirectories))
            {
                FileItem fileItem = new FileItem(file);
                string newModLoc = Path.Combine(Config.LAHDModPath, fileItem.Name);
                fileItem.FullName.MovePath(newModLoc, true);
            }
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        POST PATCHING CODE : MASTER FUNCTION : STUFF THAT IS DONE AFTER PATCHING HAS FINISHED.
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private static void PostPatchingFunctions()
        {
            // Because of a mistake I made not keeping "dungeon3_1.map" around, it now needs a special fix.
            Dungeon3PatchFix();

            // We need the new FNT files for Chinese font, the new editor fonts, and a bitmap icon for OpenGL.
            CopyNewFiles();

            // They will probably be there again so remove them one more time.
            RemoveBadBackupFiles();

            // After migration, some map files are not needed.
            CleanUp.RemoveJunkMapFiles();

            CreateModFolders();

            // Finish up. Android needs the controller buttons and to be made into an APK.
            if (Config.SelectedPlatform == Platform.Android)
            {
                // Extract the android buttons to the game directory in "Data/Buttons".
                string stageRoot   = Path.Combine(Config.TempFolder, "android", "com.zelda.ladxhd");
                string buttonsPath = Path.Combine(stageRoot, "assets", "Data", "Buttons").CreatePath();
                Utilities.ExtractResourcesZip("android_buttons.zip", buttonsPath);

                // Generate the APK file.
                GenerateAPKFile();
            }
            else if (Config.SelectedPlatform == Platform.MacOS_x86 || Config.SelectedPlatform == Platform.MacOS_Arm64)
            {
                // The files are different depending on MacOS CPU.
                string zipName = Config.SelectedPlatform == Platform.MacOS_x86
                    ? "macos_x86_files.zip" 
                    : "macos_arm64_files.zip";

                // Extract the zip containing the MacOS files.
                Utilities.ExtractResourcesZip(zipName, Config.TempFolder);
            }
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        PATCHING CODE : PATCH FILES USING XDELTA PATCHES FROM "Resources.resx" TO UPDATE TO THE LATEST VERSION.
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private static Dictionary<string, string> _gameFileLookup;

        private static void BuildGameFileLookup()
        {
            // Create a dictionary to store both file name (key) and the path to it (value).
            _gameFileLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Loop through all files in the base folder.
            foreach (string file in Directory.EnumerateFiles(Config.BaseFolder, "*", SearchOption.AllDirectories))
            {
                // Get a file item of the file.
                var fileItem = new FileItem(file);

                // Always skip files in the "Backup" and "Temp" folders.
                if (fileItem.IsInFolder("Backup") || fileItem.IsInFolder("~temp"))
                    continue;

                // Get the name of the file for the key.
                string name = Path.GetFileName(file);

                // Store the key if it doesn't exist and set the value to the full path.
                if (!_gameFileLookup.ContainsKey(name))
                    _gameFileLookup[name] = file;
                // If there is a duplicate key, add a hashtag that will be trimmed later.
                else
                    _gameFileLookup[name + "#"] = file;
            }
        }

        private static void HandleMultiFilePatches(string filePath, string overridePath = "")
        {
            // Use the input path to get a file item.
            FileItem fileItem = new FileItem(filePath);

            // Use the file name to get the files that it creates.
            if (!fileTargets.TryGetValue(fileItem.Name, out var targets))
                return;

            // Loop through the target file names.
            foreach (string newFile in targets)
            {
                // Set up the path to the patch.
                string xdelta3File = Path.Combine(Config.TempFolder, "patches", newFile + ".xdelta");

                // Make sure a patch exists.
                if (!xdelta3File.TestPath())
                    continue;

                // Where the patched file will be temporarily held.
                string patchedFile = Path.Combine(Config.TempFolder, "patchedFiles", newFile);

                // The path to move the file. 
                string newFilePath = Path.Combine(fileItem.DirectoryName, newFile);

                // If an override is provided overwrite the path with it.
                if (overridePath != "")
                    newFilePath = Path.Combine(overridePath, newFile);

                // Apply the patch to the file.
                XDelta3.Execute(Operation.Apply, fileItem.FullName, xdelta3File, patchedFile, newFilePath);
            }
        }

        private static void VerifyCreateDungeon3Map()
        {
            // Check for either instance of the original "dungeon3_1.map" which is required for patching.
            string realD3 = Path.Combine(Config.BaseFolder, "Data", "Maps", "dungeon3_1.map");
            string backD3 = Path.Combine(Config.BackupPath, "dungeon3_1.map");

            // If neither exist then recreate the original map file for patching.
            if (!realD3.TestPath() && !backD3.TestPath())
            {
                Config.BackupPath.CreatePath(false);
                File.WriteAllBytes(backD3, (byte[])resources["dungeon3_1.map"]);
            }
        }

        private static void PrepareLinuxFiles()
        {
            // The path where we want the extensionless executable.
            string exePath = Config.ZeldaEXE.Substring(0, Config.ZeldaEXE.Length - 4);

            // If the correct file isn't already in place move it.
            if (_executable != exePath)
                _executable.MovePath(exePath, true);

            // Remove any Linux specific files as they are not needed.
            string[] linuxFiles = new string[]{ Config.ZeldaEXE, "System.IO.Compression.Native.a", "System.Native.a", 
                "System.Net.Http.Native.a", "System.Net.Security.Native.a", "System.Security.Cryptography.Native.OpenSsl.a",
                "libopenal.dylib", "libSDL2-2.0.0.dylib"};

            // Remove the patched executable and any linux specific files.
            foreach (string file in linuxFiles) 
            {
                string filePath = Path.Combine(Config.BaseFolder, file);
                filePath.RemovePath();
            }
        }

        private static void PatchGameFiles()
        {
            // Check to see if we are on Android.
            bool isAndroid = Config.SelectedPlatform == Platform.Android;
            bool isWindows = Config.SelectedPlatform == Platform.Windows;
            bool isLinux   = Config.SelectedPlatform == Platform.Linux_x86 || Config.SelectedPlatform == Platform.Linux_Arm64;
            bool isMacOS   = Config.SelectedPlatform == Platform.MacOS_x86 || Config.SelectedPlatform == Platform.MacOS_Arm64;

            // Create the backup path if it doesn't exist.
            Config.BackupPath.CreatePath();

            // The v1.0.0 executable must be in the base path for Linux without the extension.
            if (isLinux || isMacOS)
                PrepareLinuxFiles();

            // Remove any garbage files that will just mess up the patcher.
            RemoveBadBackupFiles();
            _filesPatched = 0;

            // Dungeon 3 map has caused me grief. Let's put an end to it.
            VerifyCreateDungeon3Map();

            // Build fast lookup of game files (name -> full path)
            BuildGameFileLookup();

            // Get a count of how many files there are.
            _totalCount = _gameFileLookup.Count;

            // Loop through all files in the collection.
            foreach (var kvp in _gameFileLookup)
            {
                // We don't need a perfect representation of progress just a rough idea of the time left.
                UpdateProgress();

                string fileName = kvp.Key.TrimEnd('#');
                string fullPath = kvp.Value;

                // Get the file as a file item which gives us some cool properties to reference.
                FileItem fileItem = new FileItem(fullPath);

                // On Android we only want files in Content or Data folders.
                if (isAndroid && (fileItem.IsInFolder("Mods") || (!fileItem.IsInFolder("Content") && !fileItem.IsInFolder("Data"))))
                    continue;
    
                // On Windows we skip the patcher or the Mods folder.
                else if ((isWindows || isLinux || isMacOS) && (fileItem.Name == "xdelta3.exe" || fileItem.IsInFolder("Mods")))
                    continue;

                // Get the backup path to test for existing backups and create new ones to it.
                string backupPath = Path.Combine(Config.BackupPath, fileName);
                string xdelta3File = Path.Combine(Config.TempFolder, "patches", fileName + ".xdelta");
                bool patchExists = xdelta3File.TestPath();

                // Default both input and output to the file item path.
                string inputFile = fullPath;
                string outputFile = fullPath;
                string overridePath = "";

                // Android handles backups and multi-files a bit differently.
                if (isAndroid)
                {
                    // If a patch and a backup exist set the input to the backup file.
                    if (patchExists && backupPath.TestPath())
                        inputFile = backupPath;

                    // Set the destination path to the Android folder.
                    string relative = fileItem.DirectoryName.Replace(Config.BaseFolder, "").TrimStart('\\');
                    string destPath = Path.Combine(Config.TempFolder, "android", "com.zelda.ladxhd", "assets", relative).CreatePath();

                    // Update the output file using the destination path and set the override for MultiFilePatches.
                    outputFile = Path.Combine(destPath, fileName);
                    overridePath = destPath;

                    // If the patch doesn't exist, we can just copy the file to the Android folder.
                    if (!patchExists)
                        fullPath.CopyPath(outputFile, true);
                }
                // Windows is a bit simpler.
                else if (isWindows || isLinux || isMacOS)
                {
                    // If a patch file exists.
                    if (patchExists)
                    {
                        // If a backup doesn't exist, create one. If one does exist, overwrite the current file with it.
                        if (!backupPath.TestPath())
                            fullPath.CopyPath(backupPath, true);
                        else
                            backupPath.CopyPath(fullPath, true);
                    }
                }
                // If this file creates other files do so now.
                if (fileTargets.TryGetValue(fileName, out _))
                    HandleMultiFilePatches(inputFile, overridePath);

                // If this file is not patched directly then move on to the next.
                if (!patchExists)
                    continue;

                // Patch the file.
                string patchedFile = Path.Combine(Config.TempFolder, "patchedFiles", fileName);
                XDelta3.Execute(Operation.Apply, inputFile, xdelta3File, patchedFile, outputFile);
                _filesPatched++;
            }
            // There's stuff to do after patching. This function just gathers it all into one method.
            PostPatchingFunctions();
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        PATCH / LAUNCHER EXTRACTION FUNCTIONS : EXTRACTS PATCHES OR LAUNCHER BASED ON SELECTED PLATFORM AND CREATES THE PATCH FOLDERS.
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private static void ExtractPatches()
        {
            // Default to Windows Direct-X patches.
            string zipName = "patches_win_dx.zip";

            // Get the name of the zip file depending on the selected platform.
            switch (Config.SelectedPlatform)
            {
                case Platform.Android:     { zipName = "patches_android.zip";     break; }
                case Platform.Linux_x86:   { zipName = "patches_linux_x86.zip";   break; }
                case Platform.Linux_Arm64: { zipName = "patches_linux_arm64.zip"; break; }
                case Platform.MacOS_x86:   { zipName = "patches_macos_x86.zip";   break; }
                case Platform.MacOS_Arm64: { zipName = "patches_macos_arm64.zip"; break; }
                case Platform.Windows:     { if (Config.SelectedGraphics == GraphicsAPI.OpenGL) { zipName = "patches_win_gl.zip"; } break;  }
            };
            // Create the path to extract patches to and extract them.
            string patchesPath = Path.Combine(Config.TempFolder, "patches").CreatePath();
            Utilities.ExtractResourcesZip(zipName, patchesPath);

            // Create the path to where patched files will go.
            Path.Combine(Config.TempFolder, "patchedFiles").CreatePath(true);
        }

        public static void ExtractLauncher()
        {
            // Platform determines which launcher to extract.
            string zipName = "";
            bool isMacOS = false;

            switch (Config.SelectedPlatform)
            {
                // Just exit if it's Android. No launcher for it.
                case Platform.Android:      { return; }

                // Each has its own type of launcher.
                case Platform.Linux_x86:    { zipName = "launcher_linux_x86.zip"; break; }
                case Platform.Linux_Arm64:  { zipName = "launcher_linux_arm64.zip"; break; }
                case Platform.MacOS_x86:    { zipName = "launcher_macos_x86.zip"; isMacOS = true; break; }
                case Platform.MacOS_Arm64:  { zipName = "launcher_macos_arm64.zip"; isMacOS = true; break; }

                // Default to Windows.
                default:                    { zipName = "launcher_windows.zip"; break; }
            }
            // Remove the launcher if it exists.
            Config.Launcher.RemovePath();
            Config.WLauncher.RemovePath();

            // MacOS may have additional files included with the launcher.
            if (isMacOS)
            {
                Path.Combine(Config.BaseFolder + "\\libAvaloniaNative.dylib").RemovePath();
                Path.Combine(Config.BaseFolder + "\\libHarfBuzzSharp.dylib").RemovePath();
                Path.Combine(Config.BaseFolder + "\\libSkiaSharp.dylib").RemovePath();
            }
            // Write the zipfile, extract it, then delete it.
            Utilities.ExtractResourcesZip(zipName, Config.BaseFolder);
        }
/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        SETUP / VALIDATION CODE : SET UP WHETHER PATCHING FROM v1.0.0 OR PATCHING FROM BACKUP FILES AND VERIFY IF PATCHING SHOULD TAKE PLACE.
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private static void DisplayExecutableError()
        {
            string title = "Original Executable Not Found";
            string message = "Could not find the original \"Link's Awakening DX HD.exe\" to patch. It is suggested to start over with the original release of v1.0.0 and run it from there.";
            Forms.OkayDialog.Display(title, 250, 40, 27, 10, 15, message);
        }

        private static bool CompareHashes(string hash1, string hash2, string exePath)
        {
            // If the hashes match then we have what we are looking for.
            if (exePath.TestPath() && hash1 == hash2)
            {
                _executable = exePath;
                _patchFromBackup = false;
                return true;
            }
            // If they don't match blank out the executable path.
            _executable = "";
            return false;
        }

        private static bool SetSourceFile()
        {
            // Start with the hash of the file found in the main folder.
            string exeCheck = Config.ZeldaEXE;
            string goodHash = "F4ADFBA864B852908705EA6A18A48F18";

            // Checks the file path, hashes, and sets the variables.
            bool CheckFile(string path) { return CompareHashes(goodHash, path.CalculateHash("MD5"), path); }

            // First check the executable found in the main folder.
            if (CheckFile(exeCheck)) { return true; }

            // Try the same executable without the extension.
            exeCheck = exeCheck.Substring(0, exeCheck.Length - 4);
            if (CheckFile(exeCheck)) { return true; }

            // If it's already been patched try to find the executable in the backup path.
            exeCheck = Path.Combine(Config.BackupPath, "Link's Awakening DX HD.exe");
            if (CheckFile(exeCheck)) { return true; }

            // If it doesn't exist with the extension check without it.
            exeCheck = exeCheck.Substring(0, exeCheck.Length - 4);
            if (CheckFile(exeCheck)) { return true; }

            // If we still don't have the good hash then we're screwed.
            return false;
        }

        private static bool ValidateExist()
        {
            // If the executable was not resolved by the function above.
            if (!_executable.TestPath())
            {
                // Show an error message to the user.
                DisplayExecutableError();
                return false;
            }
            // We can continue with the patching.
            return true;
        }

        private static bool ValidateStart()
        {
            // The verification message changes if Android is selected to patch.
            string title = Config.SelectedPlatform == Platform.Android
                ? "Create v" + Config.Version + " APK"
                : "Patch to v" + Config.Version;
            string message = Config.SelectedPlatform == Platform.Android
                ? "Create an APK using game files patching to v" + Config.Version + "?"
                : "Are you sure you wish to patch the game to v" + Config.Version + "?";
            return Forms.YesNoDialog.Display(title, 280, 20, 24, 24, true, message);
        }

        private static void ReportFinished()
        {
            if (_silentMode)
            {
                Console.WriteLine("Patched " + _filesPatched + " files.");
            }
            else
            {
                if (Config.SelectedPlatform == Platform.Android)
                {
                    string title = "APK Created";
                    string message = _patchFromBackup
                        ? "Creating an APK from v1.0.0 backup files was successful. The game version is set to v"+ Config.Version + "." 
                        : "Creating an APK from original v1.0.0 files was successful. The game version is set to v"+ Config.Version + ".";
                    Forms.OkayDialog.Display(title, 260, 40, 34, 16, 10, message);
                }
                else
                {
                    string title = "Patching Complete";
                    string message = _patchFromBackup
                        ? "Patching the game from v1.0.0 backup files was successful. The game was updated to v"+ Config.Version + "." 
                        : "Patching Link's Awakening DX HD v1.0.0 was successful. The game was updated to v"+ Config.Version + ".";
                    Forms.OkayDialog.Display(title, 260, 40, 34, 16, 10, message);
                }
            }
        }

        private static void TryRemoveTempPath()
        {
            // Try to remove the temp path.
            try
            {
                Config.TempFolder.RemovePath();
            }
            // If it fails, show an error message.
            catch
            {
                ShowWarning("Patching Complete", "The game was patched successfully, but the patcher failed to delete the \"temp\" folder. Please report this as an issue on the Github repo!", 260, 40, 28, 10, 10);
            }
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        MAIN PATCHING FUNCTION: THIS IS WHERE IT ALL BEGINS WHETHER IT'S PATCHING OR GENERATING AN APK.
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        public static void StartPatching()
        {
            // Reset progress bar and set whether we are patching from v1.0.0 or backup files.
            ResetProgress();

            // Validate if patching should take place.
            if (!SetSourceFile()) return;
            if (!ValidateExist()) return;
            if (!ValidateStart()) return;

            // Disables dialog functionality. 
            Forms.MainDialog.ToggleDialog(false);

            // Remove temp path and recreate it.
            Config.TempFolder.RemovePath();
            Config.TempFolder.CreatePath(true);

            // Extract patches.
            ExtractPatches();

            // Create XDelta executable and patch files.
            XDelta3.Create();
            PatchGameFiles();
            XDelta3.Remove();

            //Extract the launcher.
            ExtractLauncher();

            // Linux / macOS finalization functions depend on both game and launcher being extracted.
            HostFinalizationFunctions();

            // Report finished, remove temp path, enable dialog.
            ReportFinished();
            TryRemoveTempPath();
            Forms.MainDialog.ToggleDialog(true);
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        SILENT PATCHING FUNCTION: WHEN THE PATCHER IS RAN FROM THE COMMAND LINE.
        - Returns exit code: 0 = success, 1 = exe not found, 2 = patching failed

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        public static int StartPatchingSilent()
        {
            Console.WriteLine("LADXHD Patcher v" + Config.Version + " - Silent Mode");
            Console.WriteLine("============================================");

            SetSourceFile();

            // Validate executable exists
            if (!_executable.TestPath())
            {
                Console.WriteLine("ERROR: Could not find \"Link's Awakening DX HD.exe\" to patch.");
                Console.WriteLine("Make sure this patcher is in the same folder as the game.");
                return 1;
            }

            Console.WriteLine("Found game executable. Starting patch process...");

            try
            {
                _silentMode = true;

                Config.TempFolder.CreatePath(true);
                Console.WriteLine("Extracting patches...");
                ExtractPatches();

                Console.WriteLine("Creating xdelta3...");
                XDelta3.Create();

                Console.WriteLine("Patching game files...");
                PatchGameFiles();

                Console.WriteLine("Extracting launcher...");
                ExtractLauncher();

                Console.WriteLine("Performing platform-specific finalization...");
                HostFinalizationFunctions();

                Console.WriteLine("Cleaning up...");
                XDelta3.Remove();
                Config.TempFolder.RemovePath();

                Console.WriteLine("============================================");
                Console.WriteLine("SUCCESS: Game patched to v" + Config.Version);
                Console.WriteLine("");
                Console.WriteLine("Files patched: " + _filesPatched);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Patching failed - " + ex.Message);
                
                // Cleanup on failure
                try
                {
                    XDelta3.Remove();
                    TryRemoveTempPath();
                }
                catch { }

                return 2;
            }
        }
    }
}
