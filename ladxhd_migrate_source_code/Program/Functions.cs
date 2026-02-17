using System;
using System.Collections.Generic;
using System.IO;
using static LADXHD_Migrater.XDelta3;

namespace LADXHD_Migrater
{
    internal class Functions
    {
/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        FILE MAPPING CODE : NOT ALL FILES AND PATCHES ARE 1:1 FROM ORIGINAL GAME VERSION. NEW FILES NEED A "BASE" TO BE CREATED FROM USING A PATCH
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        // SOME RESOURCES ARE USED TO CREATE MULTIPLE FILES OR PATCHES. EACH ARRAY BELOW HOLDS ALL VERSIONS OF A FILE THAT IS
        // BASED OFF OF ANOTHER FILE. THE "MASTER" FILE THAT CREATES THESE VERSIONS IS LINKED TO THEM IN THE DICTIONARY BELOW

        private static string[] langFiles  = new[] { "chn.lng", "deu.lng", "esp.lng", "fre.lng", "ind.lng", "ita.lng", "por.lng", "rus.lng" };
        private static string[] langDialog = new[] { "dialog_chn.lng", "dialog_deu.lng", "dialog_esp.lng", "dialog_fre.lng", "dialog_ind.lng", "dialog_ita.lng", "dialog_por.lng", "dialog_rus.lng" };
        private static string[] smallFonts = new[] { "smallFont_redux.png", "smallFont_vwf.png", "smallFont_vwf_redux.png" };
        private static string[] backGround = new[] { "menuBackgroundB.png", "menuBackgroundC.png", "sgb_border.png" };
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
            { "smallFont.png",      smallFonts },
            { "menuBackground.png", backGround },
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

        // CREATE A REVERSE MAP OF THE DICTIONARY SO IT CAN EASILY BE SEARCHED IN EITHER DIRECTION

        private static readonly Dictionary<string, string> reverseFileTargets = BuildReverseMap();
        private static Dictionary<string, string> BuildReverseMap()
        {
            var reverse = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in fileTargets)
            {
                string shortName = kvp.Key;
                string[] longNames = kvp.Value;
                foreach (string longName in longNames)
                    reverse[longName] = shortName;
            }
            return reverse;
        }

        public static bool InJunkFolder(FileItem fileItem)
        {
            return (fileItem.DirectoryName.IndexOf("content\\bin\\", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    fileItem.DirectoryName.IndexOf("content\\obj\\", StringComparison.OrdinalIgnoreCase) >= 0);
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        MIGRATION CODE : COPY OR PATCH V1.0.0 ASSETS IN "assets_original" USING PATCHES IN "assets_patches" TO CONTENT/DATA OF "ladxhd_game_source_code"
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        public static bool VerifyMigrate()
        {
            if (!Config.orig_Content.TestPath() || !Config.orig_Data.TestPath())
            {
                Forms.okayDialog.Display("Error: Assets Missing", 250, 40, 26, 16, 15,
                    "Either the original \"Content\" folder, \"Data\" folder, or both are missing from the \"assets_original\" folder.");
                return false;
            }
            bool verify = Forms.yesNoDialog.Display("Confirm Migration", 250, 40, 31, 16, true, 
                "Are you sure you wish to migrate assets? This will apply current patches and overwrite your assets!");
            return verify;
        }

        private static void HandleMultiFilePatches(FileItem fileItem, string origPath, string updatePath)
        {
            if (!fileTargets.TryGetValue(fileItem.Name, out var target))
                return;

            foreach (string newFile in target)
            {
                string xdelta3File = Path.Combine(Config.patches, newFile + ".xdelta");
                string patchedFile = Path.Combine(updatePath + fileItem.DirectoryName.Replace(origPath, ""), newFile);
                XDelta3.Execute(Operation.Apply, fileItem.FullName, xdelta3File, patchedFile);
            }
        }

        public static void MigrateCopyLoop(string origPath, string updatePath)
        {
            updatePath.RemovePath();

            foreach (string file in origPath.GetFiles("*", true))
            {
                FileItem fileItem = new FileItem(file);

                if (InJunkFolder(fileItem)) continue;

                string xdelta3File = Path.Combine(Config.patches, fileItem.Name + ".xdelta");
                string patchedFile = Path.Combine((updatePath + fileItem.DirectoryName.Replace(origPath, "")).CreatePath(), fileItem.Name);

                if (xdelta3File.TestPath())
                    XDelta3.Execute(Operation.Apply, fileItem.FullName, xdelta3File, patchedFile);
                else
                    File.Copy(fileItem.FullName, patchedFile, true);

                HandleMultiFilePatches(fileItem, origPath, updatePath);
            }
        }

        public static void MigrateFiles()
        {
            if (!VerifyMigrate()) return;
            Forms.mainDialog.ToggleDialog(false);

            XDelta3.Create();
            MigrateCopyLoop(Config.orig_Content, Config.update_Content);
            MigrateCopyLoop(Config.orig_Data, Config.update_Data);
            XDelta3.Remove();

            Forms.okayDialog.Display("Finished Migration", 280, 40, 45, 26, 15, 
                "Updated Content/Data files to latest versions.");
            Forms.mainDialog.ToggleDialog(true);
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        PATCHES CODE : CREATE PATCHES FROM CONTENT/DATA IN "ladxhd_game_source_code" VS. FILES IN "assets_original" TO FOLDER "assets_patches"

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        public static bool VerifyCreatePatch()
        {
            if (!Config.orig_Content.TestPath() || !Config.orig_Data.TestPath())
            {
                Forms.okayDialog.Display("Error: Assets Missing", 250, 40, 26, 16, 15,
                    "Either the original \"Content\" folder, \"Data\" folder, or both are missing from the \"assets_original\" folder.");
                return false;
            }
            if (!Config.update_Content.TestPath() || !Config.update_Data.TestPath())
            {
                Forms.okayDialog.Display("Assets Missing", 250, 40, 34, 16, 15,
                    "Either the \"Content\" folder, \"Data\" folder, or both are missing from \"ladxhd_game_source_code\".");
                return false;
            }
            bool verify = Forms.yesNoDialog.Display("Confirm Create Patches", 250, 40, 31, 16, true, 
                "Are you sure you wish to create patches? This will overwrite all current patches with recent changes!");
            return verify;
        }

        public static void CreatePatchLoop(string origPath, string updatePath)
        {
            Config.patches.CreatePath(true);

            foreach (string file in updatePath.GetFiles("*", true))
            {
                FileItem fileItem = new FileItem(file);
                string oldFile = "";

                if (InJunkFolder(fileItem)) continue;

                oldFile = Path.Combine(origPath + fileItem.DirectoryName.Replace(updatePath, ""), fileItem.Name);

                if (!oldFile.TestPath() && reverseFileTargets.TryGetValue(fileItem.Name, out string shortName))
                    oldFile = Path.Combine(origPath + fileItem.DirectoryName.Replace(updatePath, ""), shortName);

                if (oldFile == "") continue;

                string oldHash = oldFile.CalculateHash("MD5");
                string newHash = fileItem.FullName.CalculateHash("MD5");

                if (oldHash != newHash)
                {
                    string patchName = Path.Combine(Config.patches, fileItem.Name + ".xdelta");
                    XDelta3.Execute(Operation.Create, oldFile, fileItem.FullName, patchName);
                }
            }
        }

        public static void CreatePatches()
        {
            if (!VerifyCreatePatch()) return;
            Forms.mainDialog.ToggleDialog(false);

            XDelta3.Create();
            CreatePatchLoop(Config.orig_Content, Config.update_Content);
            CreatePatchLoop(Config.orig_Data, Config.update_Data);
            XDelta3.Remove();

            Forms.okayDialog.Display("Patches Created", 250, 40, 27, 9, 15,
                "Finished creating xdelta patches from modified files. If any files were intentionally modifed, these can be shared as a new PR for the GitHub repository.");
            Forms.mainDialog.ToggleDialog(true);
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        CLEAN BUILD FILES CODE : REMOVE ALL "bin" / "obj" FOLDERS AND REMOVE PREVIOUS BUILD FOLDERS "publish" / "zelda_ladxhd_build"

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        public static bool VerifyCleanFiles()
        {
            bool verify = Forms.yesNoDialog.Display("Clean Build Files", 250, 40, 29, 9, true, 
                "Are you sure you wish to clean build files? This will remove all instances of \'obj\', \'bin\', \'Publish\', and \'zelda_ladxhd_build\' folders if they currently exist.");
            return verify;
        }

        public static void CleanBuildFiles()
        {
            if (!VerifyCleanFiles()) return;
            Forms.mainDialog.ToggleDialog(false);

            (Config.game_source + "\\bin").RemovePath();
            (Config.game_source + "\\obj").RemovePath();
            (Config.game_source + "\\Content\\bin").RemovePath();
            (Config.game_source + "\\Content\\obj").RemovePath();
            (Config.game_source + "\\Publish").RemovePath();
            (Config.migrate_source + "\\bin").RemovePath();
            (Config.migrate_source + "\\obj").RemovePath();
            (Config.patcher_source + "\\bin").RemovePath();
            (Config.patcher_source + "\\obj").RemovePath();
            (Config.baseFolder + "\\zelda_ladxhd_build").RemovePath();

            Forms.okayDialog.Display("Finished", 260, 40, 26, 26, 15,
                "Finished cleaning build files (obj/bin/Publish folders).");
            Forms.mainDialog.ToggleDialog(true);
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        CREATE NEW BUILD CODE: BUILD A NEW VERSION USING THE CURRENT ASSETS AND MOVE TO THE FOLDER "zelda_ladxhd_build"

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        public static void CreateBuild()
        {
            Forms.mainDialog.ToggleDialog(false);

            if (DotNet.BuildGame())
            {
                string MoveDestination = Config.baseFolder + "\\zelda_ladxhd_build";
                Config.publish_Path.MovePath(MoveDestination, true);

                Forms.okayDialog.Display("Finished", 250, 40, 28, 16, 15,
                    "Finished build process. If the build was successful, it can be found in the \"zelda_ladxhd_build\" folder.");
            }
            Forms.mainDialog.ToggleDialog(true);
        }
    }
}
