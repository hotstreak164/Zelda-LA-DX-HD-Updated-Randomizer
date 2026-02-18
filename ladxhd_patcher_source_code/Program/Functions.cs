using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using static LADXHD_Patcher.XDelta3;

namespace LADXHD_Patcher
{
    internal class Functions
    {
        private static int    PatchProgress;
        private static int    FileCount;
        private static int    TotalCount;
        private static int    filesPatched;
        private static bool   silentMode;
        private static bool   patchFromBackup;
        private static string Executable;

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        FILE MAPPING CODE : NOT ALL FILES AND PATCHES ARE 1:1 FROM ORIGINAL GAME VERSION. NEW FILES NEED A "BASE" TO BE CREATED FROM USING A PATCH
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        // SOME RESOURCES ARE USED TO CREATE MULTIPLE FILES OR PATCHES. EACH ARRAY BELOW HOLDS ALL VERSIONS OF A FILE THAT IS
        // BASED OFF OF ANOTHER FILE. THE "MASTER" FILE THAT CREATES THESE VERSIONS IS LINKED TO THEM IN THE DICTIONARY BELOW

        private static string[] langFiles  = new[] { "chn.lng", "deu.lng", "esp.lng", "fre.lng", "ind.lng", "ita.lng", "por.lng", "rus.lng" };
        private static string[] langDialog = new[] { "dialog_chn.lng", "dialog_deu.lng", "dialog_esp.lng", "dialog_fre.lng", "dialog_ind.lng", "dialog_ita.lng", "dialog_por.lng", "dialog_rus.lng" };
        private static string[] smallFonts = new[] { "smallFont_redux.xnb", "smallFont_vwf.xnb", "smallFont_vwf_redux.xnb" };
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

        CHINESE FONT: BECAUSE THE FONT WE HAVE IS ALREADY COMPILED INTO XNB IT CAN'T BE COMPILED WITH THE GAME. SO WE INSTALL IT WITH THE PATCHER INSTEAD.
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private static void InstallChineseFont()
        {
            // I can't remember how to get a file without my resource helper so just use that.
            Dictionary<string, object> resources = ResourceHelper.GetAllResources();

            // Set the path to the Chinese font that will be created.
            string chinaFontXNB = Path.Combine(Config.gameFontsPath, "smallFont_chn.xnb");
            string chinaFontXNBRedux = Path.Combine(Config.gameFontsPath, "smallFont_chn_redux.xnb");

            // Write the chinese language file to the directory.
            File.WriteAllBytes(chinaFontXNB, (byte[])resources["smallFont_chn.xnb"]);
            File.WriteAllBytes(chinaFontXNBRedux, (byte[])resources["smallFont_chn_redux.xnb"]);
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        BAD BACKUPS: OLD PATCHER VERSIONS KEPT AROUND PATCHED FILES IN THE BACKUP FOLDER, WHICH MESSES UP THE PATCHER. BACKUP FOLDER IS FOR v1.0.0 FILES ONLY.
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private static void RemoveBadBackupFiles()
        {
            // Because old versions of the patchers saved "new" files, we need to remove them or they will cause problems.
            string[][] list = { langFiles, langDialog, smallFonts, backGround, linkImages, npcImages, itemImages, introImage, introAtlas, 
                                miniMapImg, objectsImg, photograph, uiImages, musicTile, dungeon3M, dungeon3D, bowwowanim, dungeonani };

            string[] remove = list.SelectMany(x => x).ToArray();

            // Loop through the files in the backup folder.
            foreach (string file in Config.backupPath.GetFiles("*", true))
            {
                // Get the file as a file item which gives us some cool properties to reference.
                FileItem fileItem = new FileItem(file);

                // If the current array file exists then remove it.
                if (remove.Contains(fileItem.Name))
                    fileItem.FullName.RemovePath();
            }
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        OBSOLETE FILES: SOME FILES IN THE GAME FOLDER HAVE BEEN MADE OBSOLETE AND MAY EVEN CAUSE PROBLEMS IF THEY REMAIN.
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private static string[] obsoleteFiles = new[] 
        {  
            "cave bird.map.data", "dungeon_end.map.data", "dungeon3_1.map", "dungeon3_1.map.data", "dungeon3_2.map", "dungeon3_2.map.data", "dungeon3_3.map", 
            "dungeon3_3.map.data", "dungeon3_4.map", "dungeon3_4.map.data", "dungeon 7_2d.map.data", "three_1.txt", "three_2.txt", "three_3.txt" 
        };

        private static void RemoveObsolete()
        {
            foreach (string file in Config.baseFolder.GetFiles("*", true))
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

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        PROGRESS CODE : TRACK AND UPDATE THE PROGRESS BAR BY KEEPING TRACK OF THE FILE COUNTS.
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private static void ResetProgress()
        {
            // Reset the variables.
            FileCount = 0;
            TotalCount = 0;
            PatchProgress = 0;

            // Update the bar to have zero progress.
            Forms.mainDialog.UpdateProgressBar(PatchProgress);
        }

        private static void UpdateProgress()
        {
            // Update the file count.
            FileCount++;

            // Get the percentage of progress.
            int progress = (int)(FileCount * 100.0 / TotalCount);

            // Update the progress bar with the value.
            if (FileCount % 10 == 0 || FileCount == TotalCount)
                Forms.mainDialog.UpdateProgressBar(progress);

            // Call do events to update the progress bar.
            Application.DoEvents();
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        PATCHING CODE : PATCH FILES USING XDELTA PATCHES FROM "Resources.resx" TO UPDATE TO THE LATEST VERSION.
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        private static void Dungeon3PatchFix()
        {
            // I fucked up. After the dungeon name change the file "dungeon3_1.map" no longer exists.
            string d3map = Path.Combine(Config.backupPath, "dungeon3_1.map");

            // Look for the backup dungeon 3 file as it should still exist.
            if (d3map.TestPath())
            {
                // Patch the file directly into the maps folder.
                string xdelta3File = Path.Combine(Config.tempFolder + "\\patches\\dungeon3.map.xdelta");
                string patchedFile = Path.Combine(Config.baseFolder + "\\Data\\Maps\\dungeon3.map");
                XDelta3.Execute(Operation.Apply, d3map, xdelta3File, patchedFile);
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
                string xdelta3File = Path.Combine(Config.tempFolder + "\\patches", newFile + ".xdelta");

                // Make sure a patch exists.
                if (!xdelta3File.TestPath())
                    continue;

                // If all has gone well, then patch the file to create a new file with a different name.
                string patchedFile = Path.Combine(Config.tempFolder + "\\patchedFiles", newFile);
                string newFilePath  = Path.Combine(fileItem.DirectoryName, newFile);
                XDelta3.Execute(Operation.Apply, fileItem.FullName, xdelta3File, patchedFile, newFilePath);
            }
        }

        private static void PatchGameFiles()
        {
            // Remove any garbage files that will just mess up the patcher.
            RemoveBadBackupFiles();
            filesPatched = 0;

            // Get all files found in the base folder recursively.
            var fileCollection = Config.baseFolder.GetFiles("*", true);

            // Get a count of how many files there are.
            TotalCount = fileCollection.Count;

            // Loop through all files in the collection.
            foreach (string file in fileCollection)
            {
                // We don't need a perfect representation of progress just a rough idea of the time left.
                UpdateProgress();

                // Get the file as a file item which gives us some cool properties to reference.
                FileItem fileItem = new FileItem(file);

                // Do not try to patch the patcher, the chinese fonts, modded files, or files directly in the backup folder.
                if (fileItem.Name == "xdelta3.exe" || fileItem.Name.StartsWith("smallFont_chn") || fileItem.IsInFolder("Mods") || fileItem.IsInFolder("Backup")  )
                    continue;

                // Get the backup path to test for existing backups and create new ones to it.
                string backupPath  = Path.Combine(Config.backupPath, fileItem.Name);
                string xdelta3File = Path.Combine(Config.tempFolder + "\\patches", fileItem.Name + ".xdelta");

                // Backup file if it has patch and a backup doesn't exist or restore from backup if one does exist.
                if (xdelta3File.TestPath())
                {
                    if (!backupPath.TestPath())
                        fileItem.FullName.CopyPath(backupPath, true);
                    else
                        backupPath.CopyPath(fileItem.FullName, true);
                }

                // If this file creates other files do so now.
                if (fileTargets.ContainsKey(fileItem.Name))
                    HandleMultiFilePatches(fileItem);

                // If this file is not patched directly then move on to the next.
                if (!xdelta3File.TestPath())
                    continue;

                // Patch the file.
                string patchedFile = Path.Combine(Config.tempFolder + "\\patchedFiles", fileItem.Name);
                XDelta3.Execute(Operation.Apply, fileItem.FullName, xdelta3File, patchedFile, fileItem.FullName);
                filesPatched++;
            }

            // Because of a mistake I made not keeping "dungeon_3_1.map" around, it now needs a special fix.
            Dungeon3PatchFix();

            // They will probably be there again so remove them one more time.
            RemoveBadBackupFiles();

            // Now is a good time to remove any files that the game no longer needs or may cause problems.
            RemoveObsolete();

            // Show the final message to the user.
            ReportFinished();
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        SETUP / VALIDATION CODE : SET UP WHETHER PATCHING FROM v1.0.0 OR PATCHING FROM BACKUP FILES AND VERIFY IF PATCHING SHOULD TAKE PLACE.
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private static void SetSourceFiles()
        {
            string backupExe = Path.Combine(Config.backupPath, "Link's Awakening DX HD.exe");
            patchFromBackup = backupExe.TestPath();
            Executable = patchFromBackup
                ? backupExe
                : Config.zeldaEXE;
        }

        private static bool ValidateExist()
        {
            if (!Executable.TestPath())
            {
                Forms.okayDialog.Display("Game Executable Not Found", 250, 40, 27, 10, 15, 
                    "Could not find \"Link's Awakening DX HD.exe\" to patch. Copy this patcher executable to the folder of the original release of v1.0.0 and run it from there.");
                return false;
            }
            return true;
        }

        private static bool ValidateStart()
        {
            return Forms.yesNoDialog.Display("Patch to " + Config.version, 280, 20, 20, 24, true, 
                "Are you sure you wish to patch the game to v" + Config.version + "?");
        }

        private static void ReportFinished()
        {
            if (silentMode)
            {
                Console.WriteLine("Patched " + filesPatched + " files.");
            }
            else
            {
                string message = patchFromBackup
                    ? "Patching the game from v1.0.0 backup files was successful. The game was updated to v"+ Config.version + "." 
                    : "Patching Link's Awakening DX HD v1.0.0 was successful. The game was updated to v"+ Config.version + ".";
                Forms.okayDialog.Display("Patching Complete", 260, 40, 34, 16, 10, message);
            }
        }

        private static void CreateModFolders()
        {
            // Create the new mods folders.
            Config.lahdmodModPath.CreatePath(true);
            Config.graphicsModPath.CreatePath(true);

            // Find the old "Mods" path for lahdmods and exit if it doesn't exist.
            if (!Directory.Exists(Config.previousModPath))
                return;

            // Move any lahdmods in the old "Mods" folder to the new location.
            foreach (string file in Directory.GetFiles(Config.previousModPath, "*", SearchOption.AllDirectories))
            {
                FileItem fileItem = new FileItem(file);
                string newModLoc = Path.Combine(Config.lahdmodModPath, fileItem.Name);
                fileItem.FullName.MovePath(newModLoc, true);
            }
        }

        private static void TryRemoveTempPath()
        {
            // Try to remove the temp path.
            try
            {
                Config.tempFolder.RemovePath();
            }
            // If it fails, show an error message.
            catch
            {
                string message = "The game was patched successfully, but the patcher failed to delete the \"temp\" folder. Please report this as an issue on the Github repo!";
                Forms.okayDialog.Display("Patching Complete", 260, 40, 28, 10, 10, message);
            }
        }

        public static void StartPatching()
        {
            SetSourceFiles();
            ResetProgress();

            if (!ValidateExist()) return;
            if (!ValidateStart()) return;

            Forms.mainDialog.ToggleDialog(false);
            Config.tempFolder.RemovePath();
            Config.tempFolder.CreatePath(true);
            ZipPatches.ExtractPatches();

            XDelta3.Create();
            PatchGameFiles();
            XDelta3.Remove();

            InstallChineseFont();
            CreateModFolders();

            TryRemoveTempPath();
            Forms.mainDialog.ToggleDialog(true);
        }

        /// <summary>
        /// Run patching in silent mode without GUI prompts.
        /// Returns exit code: 0 = success, 1 = exe not found, 2 = patching failed
        /// </summary>
        public static int StartPatchingSilent()
        {
            Console.WriteLine("LADXHD Patcher v" + Config.version + " - Silent Mode");
            Console.WriteLine("============================================");

            SetSourceFiles();

            // Validate executable exists
            if (!Executable.TestPath())
            {
                Console.WriteLine("ERROR: Could not find \"Link's Awakening DX HD.exe\" to patch.");
                Console.WriteLine("Make sure this patcher is in the same folder as the game.");
                return 1;
            }

            Console.WriteLine("Found game executable. Starting patch process...");

            try
            {
                silentMode = true;

                Config.tempFolder.CreatePath(true);
                Console.WriteLine("Extracting patches...");
                ZipPatches.ExtractPatches();

                Console.WriteLine("Creating xdelta3...");
                XDelta3.Create();

                Console.WriteLine("Patching game files...");
                PatchGameFiles();

                Console.WriteLine("Copying Chinese font file...");
                InstallChineseFont();

                Console.WriteLine("Creating mods folders...");
                CreateModFolders();

                Console.WriteLine("Cleaning up...");
                XDelta3.Remove();
                Config.tempFolder.RemovePath();

                Console.WriteLine("============================================");
                Console.WriteLine("SUCCESS: Game patched to v" + Config.version);
                Console.WriteLine("");
                Console.WriteLine("Files patched: " + filesPatched);
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
