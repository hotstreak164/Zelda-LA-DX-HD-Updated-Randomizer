using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
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
            { "eng.lng",             langFiles },
            { "dialog_eng.lng",     langDialog },
            { "smallFont.xnb",      smallFonts },
            { "menuBackground.xnb", backGround },
            { "link0.png",          linkImages },
            { "npcs.png",            npcImages },
            { "items.png",          itemImages },
            { "intro.png",          introImage },
            { "intro.atlas",        introAtlas },
            { "minimap.png",        miniMapImg },
            { "objects.png",        objectsImg },
            { "photos.png",         photograph },
            { "ui.png",               uiImages },
            { "musicOverworld.data", musicTile },
            { "dungeon3_1.map",      dungeon3M },
            { "dungeon3_1.map.data", dungeon3D },
            { "BowWow.ani",         bowwowanim },
            { "mapPlayer.ani",      dungeonani }
        };

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        PROGRESS CODE : TRACK AND UPDATE THE PROGRESS BAR BY KEEPING TRACK OF THE FILE COUNTS.
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

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

            // Update the progress bar with the value.
            if (_fileCount % 10 == 0 || _fileCount == _totalCount)
                Forms.MainDialog.UpdateProgressBar(progress);

            // Call do events to update the progress bar.
            Application.DoEvents();
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        POST PATCHING CODE : STUFF THAT IS DONE AFTER PATCHING HAS FINISHED.
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private static void Dungeon3PatchFix()
        {
            // I fucked up. After the dungeon name change the file "dungeon3_1.map" no longer exists.
            string d3map = Path.Combine(Config.BackupPath, "dungeon3_1.map");

            // Look for the backup dungeon 3 file as it should still exist.
            if (d3map.TestPath())
            {
                // Patch the file directly into the maps folder.
                string xdelta3File = Path.Combine(Config.TempFolder + "\\patches\\dungeon3.map.xdelta");
                string patchedFile = Path.Combine(Config.BaseFolder + "\\Data\\Maps\\dungeon3.map");
                XDelta3.Execute(Operation.Apply, d3map, xdelta3File, patchedFile);
            }
        }

        private static void CopyNewFiles()
        {
            // Set up the path to the Icon.
            string iconPath = Path.Combine(Config.DataPath, "Icon").CreatePath();
            string iconFile = Path.Combine(iconPath, "Icon.ico");

            // Write the files to the "Content\Fonts" folder.
            File.WriteAllBytes(iconFile, (byte[])resources["Icon.ico"]);

            // Set up the path to the bitmap Icon.
            string iconBmpPath = Path.Combine(Config.DataPath, "Icon").CreatePath();
            string iconBmpFile = Path.Combine(iconBmpPath, "Icon.bmp");

            // Write the bitmap icon to the "Data\Icon" folder.
            using (var ms = new MemoryStream())
            {
                ((Bitmap)resources["Icon.bmp"]).Save(ms, ImageFormat.Bmp);
                File.WriteAllBytes(iconBmpFile, ms.ToArray());
            }
            // If it's the OpenGL build then it needs SDL2.dll.
            if (Config.SelectedGraphics == GraphicsAPI.OpenGL)
            {
                string SdlPath = Path.Combine(Config.BaseFolder, "SDL2.dll");
                File.WriteAllBytes(SdlPath, (byte[])resources["SDL2.dll"]);
            }
        }

        private static void RemoveBadBackupFiles()
        {
            // Because old versions of the patchers saved "new" files, we need to remove them or they will cause problems.
            string[][] list = { langFiles, langDialog, smallFonts, backGround, linkImages, npcImages, itemImages, introImage, introAtlas, 
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

        private static void RemoveObsolete()
        {
            string[] obsoleteFiles = new[] {  
                "cave bird.map.data", "dungeon_end.map.data", "dungeon3_1.map", "dungeon3_1.map.data", "dungeon3_2.map", "dungeon3_2.map.data", "dungeon3_3.map", 
                "dungeon3_3.map.data", "dungeon3_4.map", "dungeon3_4.map.data", "dungeon 7_2d.map.data", "three_1.txt", "three_2.txt", "three_3.txt" 
            };

            foreach (string file in Config.BaseFolder.GetFiles("*", true))
            {
                // Get the file as a file item which gives us some cool properties to reference.
                FileItem fileItem = new FileItem(file);

                // Skip backup files for safety.
                if (fileItem.IsInFolder("Backup"))
                    continue;

                // If the obsolete file exists then delete it.
                if (obsoleteFiles.Contains(fileItem.Name))
                    fileItem.FullName.RemovePath();
            }
        }

        private static void CreateModFolders()
        {
            // Create the new mods folders.
            Config.LAHDModPath.CreatePath(true);
            Config.GraphicsModPath.CreatePath(true);

            // Find the old "Mods" path for lahdmods and exit if it doesn't exist.
            if (!Directory.Exists(Config.PreviousModPath))
                return;

            // Move any lahdmods in the old "Mods" folder to the new location.
            foreach (string file in Directory.GetFiles(Config.PreviousModPath, "*", SearchOption.AllDirectories))
            {
                FileItem fileItem = new FileItem(file);
                string newModLoc = Path.Combine(Config.LAHDModPath, fileItem.Name);
                fileItem.FullName.MovePath(newModLoc, true);
            }
        }

        private static void PostPatchingFunctions()
        {
            // Because of a mistake I made not keeping "dungeon_3_1.map" around, it now needs a special fix.
            Dungeon3PatchFix();

            // We need the new FNT files for Chinese font, the new editor fonts, and a bitmap icon for OpenGL.
            CopyNewFiles();

            // They will probably be there again so remove them one more time.
            RemoveBadBackupFiles();

            // Now is a good time to remove any files that the game no longer needs or may cause problems.
            RemoveObsolete();

            // Create the "Mod" folders.
            CreateModFolders();
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        PATCHING CODE : PATCH FILES USING XDELTA PATCHES FROM "Resources.resx" TO UPDATE TO THE LATEST VERSION.
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private static Dictionary<string, string> _gameFileLookup;

        private static void BuildGameFileLookup()
        {
            _gameFileLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string file in Directory.EnumerateFiles(Config.BaseFolder, "*", SearchOption.AllDirectories))
            {
                var fileItem = new FileItem(file);

                if (fileItem.IsInFolder("Backup"))
                    continue;

                string name = Path.GetFileName(file);

                if (!_gameFileLookup.ContainsKey(name))
                    _gameFileLookup[name] = file;
            }
        }

        private static void HandleMultiFilePatches(FileItem fileItem)
        {
            // Use the file name to get the files that it creates.
            if (!fileTargets.TryGetValue(fileItem.Name, out var targets))
                return;

            // Loop through the target file names.
            foreach (string newFile in targets)
            {
                // Set up the path to the patch.
                string xdelta3File = Path.Combine(Config.TempFolder + "\\patches", newFile + ".xdelta");

                // Make sure a patch exists.
                if (!xdelta3File.TestPath())
                    continue;

                // If all has gone well, then patch the file to create a new file with a different name.
                string patchedFile = Path.Combine(Config.TempFolder + "\\patchedFiles", newFile);
                string newFilePath  = Path.Combine(fileItem.DirectoryName, newFile);
                XDelta3.Execute(Operation.Apply, fileItem.FullName, xdelta3File, patchedFile, newFilePath);
            }
        }

        private static void PatchGameFiles()
        {
            // Remove any garbage files that will just mess up the patcher.
            RemoveBadBackupFiles();
            _filesPatched = 0;

            // Build fast lookup of game files (name -> full path)
            BuildGameFileLookup();

            // Get a count of how many files there are.
            _totalCount = _gameFileLookup.Count;

            // Loop through all files in the collection.
            foreach (var kvp in _gameFileLookup)
            {
                // We don't need a perfect representation of progress just a rough idea of the time left.
                UpdateProgress();

                string fileName = kvp.Key;
                string fullPath = kvp.Value;

                // Get the file as a file item which gives us some cool properties to reference.
                FileItem fileItem = new FileItem(fullPath);

                // Do not try to patch the patcher, the chinese fonts, modded files, or files directly in the backup folder.
                if (fileItem.Name == "xdelta3.exe" || fileItem.Name.StartsWith("smallFont_chn") || fileItem.IsInFolder("Mods"))
                    continue;

                // Get the backup path to test for existing backups and create new ones to it.
                string backupPath  = Path.Combine(Config.BackupPath, fileName);
                string xdelta3File = Path.Combine(Config.TempFolder, "patches", fileName + ".xdelta");

                // Backup file if it has patch and a backup doesn't exist or restore from backup if one does exist.
                bool patchExists = xdelta3File.TestPath();
                if (patchExists)
                {
                    if (!backupPath.TestPath())
                        fullPath.CopyPath(backupPath, true);
                    else
                        backupPath.CopyPath(fullPath, true);
                }
                // If this file creates other files do so now.
                if (fileTargets.TryGetValue(fileName, out _))
                    HandleMultiFilePatches(fileItem);
                
                // If this file is not patched directly then move on to the next.
                if (!patchExists)
                    continue;

                // Patch the file.
                string patchedFile = Path.Combine(Config.TempFolder, "patchedFiles", fileName);
                XDelta3.Execute(Operation.Apply, fullPath, xdelta3File, patchedFile, fullPath);
                _filesPatched++;
            }
            // There's stuff to do after patching. This function just gathers it all into one method.
            PostPatchingFunctions();
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        SETUP / VALIDATION CODE : SET UP WHETHER PATCHING FROM v1.0.0 OR PATCHING FROM BACKUP FILES AND VERIFY IF PATCHING SHOULD TAKE PLACE.
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private static void SetSourceFiles()
        {
            string backupExe = Path.Combine(Config.BackupPath, "Link's Awakening DX HD.exe");
            _patchFromBackup = backupExe.TestPath();
            _executable = _patchFromBackup
                ? backupExe
                : Config.ZeldaEXE;
        }

        private static bool ValidateExist()
        {
            if (!_executable.TestPath())
            {
                Forms.OkayDialog.Display("Game Executable Not Found", 250, 40, 27, 10, 15, 
                    "Could not find \"Link's Awakening DX HD.exe\" to patch. Copy this patcher executable to the folder of the original release of v1.0.0 and run it from there.");
                return false;
            }
            return true;
        }

        private static bool ValidateStart()
        {
            return Forms.YesNoDialog.Display("Patch to " + Config.Version, 280, 20, 20, 24, true, 
                "Are you sure you wish to patch the game to v" + Config.Version + "?");
        }

        private static void ReportFinished()
        {
            if (_silentMode)
            {
                Console.WriteLine("Patched " + _filesPatched + " files.");
            }
            else
            {
                string message = _patchFromBackup
                    ? "Patching the game from v1.0.0 backup files was successful. The game was updated to v"+ Config.Version + "." 
                    : "Patching Link's Awakening DX HD v1.0.0 was successful. The game was updated to v"+ Config.Version + ".";
                Forms.OkayDialog.Display("Patching Complete", 260, 40, 34, 16, 10, message);
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
                string message = "The game was patched successfully, but the patcher failed to delete the \"temp\" folder. Please report this as an issue on the Github repo!";
                Forms.OkayDialog.Display("Patching Complete", 260, 40, 28, 10, 10, message);
            }
        }

        public static void StartPatching()
        {
            SetSourceFiles();
            ResetProgress();

            if (!ValidateExist()) return;
            if (!ValidateStart()) return;

            Forms.MainDialog.ToggleDialog(false);
            Config.TempFolder.RemovePath();
            Config.TempFolder.CreatePath(true);
            ZipPatches.ExtractPatches();

            XDelta3.Create();
            PatchGameFiles();
            XDelta3.Remove();

            ReportFinished();
            TryRemoveTempPath();
            Forms.MainDialog.ToggleDialog(true);
        }

        /// <summary>
        /// Run patching in silent mode without GUI prompts.
        /// Returns exit code: 0 = success, 1 = exe not found, 2 = patching failed
        /// </summary>
        public static int StartPatchingSilent()
        {
            Console.WriteLine("LADXHD Patcher v" + Config.Version + " - Silent Mode");
            Console.WriteLine("============================================");

            SetSourceFiles();

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
                ZipPatches.ExtractPatches();

                Console.WriteLine("Creating xdelta3...");
                XDelta3.Create();

                Console.WriteLine("Patching game files...");
                PatchGameFiles();

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
