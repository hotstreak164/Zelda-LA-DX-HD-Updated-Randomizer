using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.IO;
using static LADXHD_Migrater.XDelta3;

namespace LADXHD_Migrater
{
    internal class Functions
    {
        private static Dictionary<string, object> resources = ResourceHelper.GetAllResources();

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        FILE MAPPING CODE : NOT ALL FILES AND PATCHES ARE 1:1 FROM ORIGINAL GAME VERSION. NEW FILES NEED A "BASE" TO BE CREATED FROM USING A PATCH
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        // SOME RESOURCES ARE USED TO CREATE MULTIPLE FILES OR PATCHES. EACH ARRAY BELOW HOLDS ALL VERSIONS OF A FILE THAT IS
        // BASED OFF OF ANOTHER FILE. THE "MASTER" FILE THAT CREATES THESE VERSIONS IS LINKED TO THEM IN THE DICTIONARY BELOW

        private static string[] langFiles  = new[] { "chn.lng", "deu.lng", "esp.lng", "fre.lng", "ind.lng", "ita.lng", "por.lng", "rus.lng" };
        private static string[] langDialog = new[] { "dialog_chn.lng", "dialog_deu.lng", "dialog_esp.lng", "dialog_fre.lng", "dialog_ind.lng", "dialog_ita.lng", "dialog_por.lng", "dialog_rus.lng" };
        private static string[] smallFonts = new[] { "smallFont_redux.png", "smallFont_vwf.png", "smallFont_vwf_redux.png", "smallFont_chn_0.png", "smallFont_chn_redux_0.png" };
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

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        MIGRATION CODE : COPY NEW FILES THAT CAN NOT BE CREATED FROM PATCHES. THIS INCLUDES FONTS AND A BITMAP ICON FOR OPENGL PORTS.
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private static void CopyNewFiles()
        {
            // Set up the path to the two ".fnt" files for the Chinese fonts.
            string smallFont_chn_fileA = Path.Combine(Config.Update_Content, "Fonts", "smallFont_chn.fnt");
            string smallFont_chn_fileB = Path.Combine(Config.Update_Content, "Fonts", "smallFont_chn_redux.fnt");

            // Write the files to the "Content\Fonts" folder.
            File.WriteAllBytes(smallFont_chn_fileA, (byte[])resources["smallFont_chn.fnt"]);
            File.WriteAllBytes(smallFont_chn_fileB, (byte[])resources["smallFont_chn_redux.fnt"]);

            // Set up the path to the replacement fonts used for multi-platform support.
            string editorFontA = Path.Combine(Config.Update_Content, "Fonts", "Courier-Prime.ttf");
            string editorFontB = Path.Combine(Config.Update_Content, "Fonts", "NotoSans-Regular.ttf");

            // Write the files to the "Content\Fonts" folder.
            File.WriteAllBytes(editorFontA, (byte[])resources["Courier-Prime.ttf"]);
            File.WriteAllBytes(editorFontB, (byte[])resources["NotoSans-Regular.ttf"]);

            // Set up the path to the bitmap Icon.
            string iconPath = Path.Combine(Config.Update_Data, "Icon").CreatePath();
            string iconFile = Path.Combine(iconPath, "Icon.bmp");

            // Write the bitmap icon to the "Data\Icon" folder.
            using (var ms = new MemoryStream())
            {
                ((Bitmap)resources["Icon.bmp"]).Save(ms, ImageFormat.Bmp);
                File.WriteAllBytes(iconFile, ms.ToArray());
            }
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        MIGRATION CODE : COPY OR PATCH V1.0.0 ASSETS IN "assets_original" USING PATCHES IN "assets_patches" TO CONTENT/DATA OF "ladxhd_game_source_code"
       
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private static void HandleMultiFilePatches(FileItem fileItem, string origPath, string updatePath)
        {
            // Check if the original file has derivatives.
            if (!fileTargets.TryGetValue(fileItem.Name, out var target))
                return;

            // If original file has derivative modified files based off of it.
            foreach (string newFile in target)
            {
                // Create all derivative files based on the original file.
                string xdelta3File = Path.Combine(Config.Patches, newFile + ".xdelta");
                string patchedFile = Path.Combine(updatePath + fileItem.DirectoryName.Replace(origPath, ""), newFile);
                XDelta3.Execute(Operation.Apply, fileItem.FullName, xdelta3File, patchedFile);
            }
        }

        public static void MigrateCopyLoop(string origPath, string updatePath)
        {
            // Remove the target path before copying files to it.
            updatePath.RemovePath();

            // Loop through the original files.
            foreach (string file in origPath.GetFiles("*", true))
            {
                // Get the current "original" file as a file item.
                FileItem fileItem = new FileItem(file);

                // Make sure it's not in "bin" or "obj" folders.
                if (fileItem.IsInFolder("bin") || fileItem.IsInFolder("obj"))
                    continue;

                // Set up path to output xdelta file and create the output folder while also getting path to updated file.
                string xdelta3File = Path.Combine(Config.Patches, fileItem.Name + ".xdelta");
                string patchedFile = Path.Combine((updatePath + fileItem.DirectoryName.Replace(origPath, "")).CreatePath(), fileItem.Name);

                // If a patch exists for the current file, patch it. If it doesn't then copy it.
                if (xdelta3File.TestPath())
                    XDelta3.Execute(Operation.Apply, fileItem.FullName, xdelta3File, patchedFile);
                else
                    File.Copy(fileItem.FullName, patchedFile, true);

                // Handle modified files that are derivatives of original files.
                HandleMultiFilePatches(fileItem, origPath, updatePath);
            }
            // Finally, copy the files to the destination.
            CopyNewFiles();
        }

        public static void MigrateFiles()
        {
            XDelta3.Create();
            MigrateCopyLoop(Config.Orig_Content, Config.Update_Content);
            MigrateCopyLoop(Config.Orig_Data, Config.Update_Data);
            XDelta3.Remove();
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        PATCHES CODE : CREATE PATCHES FROM CONTENT/DATA IN "ladxhd_game_source_code" VS. FILES IN "assets_original" TO FOLDER "assets_patches"

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private static void CreatePatchLoop(string origPath, string updatePath)
        {
            // Create the "assets_patches" folder if it doesn't exist.
            Config.Patches.CreatePath(true);

            // Loop through the new Content and Data folders.
            foreach (string file in updatePath.GetFiles("*", true))
            {
                // Get the current "new" file as a file item.
                FileItem fileItem = new FileItem(file);
                string oldFile = "";

                // Make sure it's not in "bin" or "obj" folders.
                if (fileItem.IsInFolder("bin") || fileItem.IsInFolder("obj")) continue;

                // Get the path to the original file using the relative folder.
                oldFile = Path.Combine(origPath + fileItem.DirectoryName.Replace(updatePath, ""), fileItem.Name);
                
                // If the file doesn't exist it might be a derivative ("eng.lng" -> "deu.lng" for example).
                if (!oldFile.TestPath() && reverseFileTargets.TryGetValue(fileItem.Name, out string shortName))
                    oldFile = Path.Combine(origPath + fileItem.DirectoryName.Replace(updatePath, ""), shortName);

                // If the original file doesn't exist or it's not a derivative skip it.
                if (oldFile == "" || !oldFile.TestPath()) continue;

                // Now that we have both files, calculate hashes from them.
                string oldHash = oldFile.CalculateHash("MD5");
                string newHash = fileItem.FullName.CalculateHash("MD5");

                // If the hashes differ, the file has been updated.
                if (oldHash != newHash)
                {
                    // Create a patch from the old file vs. the new file.
                    string patchName = Path.Combine(Config.Patches, fileItem.Name + ".xdelta");
                    XDelta3.Execute(Operation.Create, oldFile, fileItem.FullName, patchName);
                }
            }
        }

        public static void CreatePatches()
        {
            XDelta3.Create();
            Config.Patches.ClearPath();
            CreatePatchLoop(Config.Orig_Content, Config.Update_Content);
            CreatePatchLoop(Config.Orig_Data, Config.Update_Data);
            XDelta3.Remove();
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        CLEAN BUILD FILES CODE : REMOVE ALL "bin" / "obj" FOLDERS AND REMOVE PREVIOUS BUILD FOLDERS "publish" / "zelda_ladxhd_build"

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        public static void CleanBuildFiles()
        {
            (Config.Game_Source + "\\ProjectZ.Android\\bin").RemovePath();
            (Config.Game_Source + "\\ProjectZ.Android\\obj").RemovePath();

            (Config.Game_Source + "\\ProjectZ.Core\\bin").RemovePath();
            (Config.Game_Source + "\\ProjectZ.Core\\obj").RemovePath();
            (Config.Game_Source + "\\ProjectZ.Core\\Content\\bin").RemovePath();
            (Config.Game_Source + "\\ProjectZ.Core\\Content\\obj").RemovePath();
            (Config.Game_Source + "\\ProjectZ.Core\\Publish").RemovePath();

            (Config.Game_Source + "\\ProjectZ.Desktop\\bin").RemovePath();
            (Config.Game_Source + "\\ProjectZ.Desktop\\obj").RemovePath();

            (Config.Migrate_Source + "\\bin").RemovePath();
            (Config.Migrate_Source + "\\obj").RemovePath();

            (Config.Patcher_Source + "\\bin").RemovePath();
            (Config.Patcher_Source + "\\obj").RemovePath();

            (Config.ModMaker_Source + "\\bin").RemovePath();
            (Config.ModMaker_Source + "\\obj").RemovePath();

            (Config.BaseFolder + "\\zelda_ladxhd_build").RemovePath();
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        CREATE NEW BUILD CODE: BUILD A NEW VERSION USING THE CURRENT ASSETS AND MOVE TO THE FOLDER "zelda_ladxhd_build"

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        public static void CreateBuild()
        {
            // Try to build the game.
            if (DotNet.BuildGame())
            {
                // If it succeeded, move the folder to the main folder.
                string MoveDestination = Config.BaseFolder + "\\zelda_ladxhd_build";
                Config.Publish_Path.MovePath(MoveDestination, true);
            }
        }
    }
}
