using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.SaveLoad
{
    public class SaveGameSaveLoad
    {
        private static SaveManager playerSaveState;

        public static string SaveFileName = "save";
        public static string SaveFileNameGame = "saveGame";

        public static bool SaveExists(int slot)
        {
            return SaveManager.FileExists(Path.Combine(Values.PathSaveFolder, SaveFileName + slot)) &&
                   SaveManager.FileExists(Path.Combine(Values.PathSaveFolder, SaveFileNameGame + slot));
        }

        public static bool CopySaveFile(int from, int to)
        {
            return CopySaveFile(Path.Combine(Values.PathSaveFolder, SaveFileName + from), Path.Combine(Values.PathSaveFolder, SaveFileName + to)) &&
                   CopySaveFile(Path.Combine(Values.PathSaveFolder, SaveFileNameGame + from), Path.Combine(Values.PathSaveFolder, SaveFileNameGame + to));
        }

        public static bool CopySaveFile(string fromFile, string toFile)
        {
            if (!File.Exists(fromFile))
                return false;

            var fromFull = Path.GetFullPath(fromFile);
            var toFull = Path.GetFullPath(toFile);
            if (string.Equals(fromFull, toFull, StringComparison.OrdinalIgnoreCase))
                return false;

            var dir = Path.GetDirectoryName(toFull);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var temp = toFull + ".tmp";

            File.Copy(fromFull, temp, overwrite: true);

            if (File.Exists(toFull))
                File.Delete(toFull);

            File.Move(temp, toFull);

            return true;
        }

        public static bool DeleteSaveFile(int slot)
        {
            return DeleteSaveFile(Path.Combine(Values.PathSaveFolder, SaveFileName + slot)) &&
                   DeleteSaveFile(Path.Combine(Values.PathSaveFolder, SaveFileNameGame + slot));
        }

        private static bool DeleteSaveFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void SaveGame(GameManager gameManager, bool showIcon)
        {
            Directory.CreateDirectory(Values.PathSaveFolder);

            var saveFilePath = Path.Combine(Values.PathSaveFolder, SaveFileNameGame + gameManager.SaveSlot);

            gameManager.SaveManager.Save(saveFilePath, Values.SaveRetries);

            if (playerSaveState == null)
                FillSaveState(ref playerSaveState, gameManager);

            playerSaveState?.Save(Path.Combine(Values.PathSaveFolder, SaveFileName + gameManager.SaveSlot), Values.SaveRetries);
            playerSaveState = null;

            if (showIcon)
                Game1.GameManager.InGameOverlay.InGameHud.ShowSaveIcon();
        }

        public static void FillSaveState(GameManager gameManager)
        {
            FillSaveState(ref playerSaveState, gameManager);
        }

        public static void ClearSaveState()
        {
            playerSaveState = null;
        }

        private static void FillSaveState(ref SaveManager saveManager, GameManager gameManager)
        {
            saveManager = new SaveManager();

            saveManager.SetString("savename", gameManager.RealSaveName);
            saveManager.SetBool("ThiefState", gameManager.ThiefState);
            saveManager.SetInt("maxHearts", gameManager.MaxHearts);
            saveManager.SetInt("deathCount", gameManager.DeathCount);
            saveManager.SetInt("killCount", gameManager.KillCount);
            saveManager.SetInt("currentHealth", gameManager.CurrentHealth);
            saveManager.SetInt("cloak", gameManager.CloakType);
            saveManager.SetInt("ocarinaSong", gameManager.SelectedOcarinaSong);
            saveManager.SetInt("guardianAcornCount", gameManager.GuardianAcornCount);
            saveManager.SetInt("pieceOfPowerCount", gameManager.PieceOfPowerCount);
            saveManager.SetFloat("totalPlaytime", gameManager.TotalPlaytime + gameManager.CurrentSessionPlaytime);
            saveManager.SetBool("cleared", gameManager.GameCleared);
            saveManager.SetBool("debugMode", gameManager.DebugMode);

            // this is only used in the main menu
            var rubyObject = Game1.GameManager.GetItem("ruby");
            if (rubyObject != null)
                saveManager.SetInt("rubyCount", rubyObject.Count);

            saveManager.SetString("currentMap", MapManager.ObjLink.SaveMap);
            saveManager.SetInt("posX", (int)MapManager.ObjLink.SavePosition.X);
            saveManager.SetInt("posY", (int)MapManager.ObjLink.SavePosition.Y);

            saveManager.SetInt("dir", MapManager.ObjLink.SaveDirection);

            if (gameManager.PlayerMapPosition != null)
            {
                saveManager.SetInt("mapPosX", gameManager.PlayerMapPosition.Value.X);
                saveManager.SetInt("mapPosY", gameManager.PlayerMapPosition.Value.Y);
            }

            var dsKeys = "";
            foreach (var strKey in gameManager.DungeonMaps.Keys)
                dsKeys += strKey + ",";
            saveManager.SetString("dungeonKeyNames", dsKeys);

            foreach (var miniMap in gameManager.DungeonMaps)
            {
                for (var y = 0; y < miniMap.Value.Tiles.GetLength(1); y++)
                {
                    var strLine = "";

                    for (var x = 0; x < miniMap.Value.Tiles.GetLength(0); x++)
                        strLine += (miniMap.Value.Tiles[x, y].DiscoveryState ? "1" : "0") + ",";

                    saveManager.SetString(miniMap.Key + "line" + y, strLine);
                }
            }

            // save equipped items
            for (var i = 0; i < gameManager.Equipment.Length; i++)
            {
                var strItem = "";
                if (gameManager.Equipment[i] != null)
                    strItem += gameManager.Equipment[i].Name + ":" + gameManager.Equipment[i].Count;

                saveManager.SetString("equipment" + i, strItem);
            }

            // save all the collected objects (keys, relicts,...)
            for (var i = 0; i < gameManager.CollectedItems.Count; i++)
            {
                var strItem = "";
                strItem += gameManager.CollectedItems[i].Name + ":" + gameManager.CollectedItems[i].Count;

                if (gameManager.CollectedItems[i].LocationBounding != null)
                    strItem += ":" + gameManager.CollectedItems[i].LocationBounding;

                saveManager.SetString("object" + i, strItem);
            }

            // save the discovered map areas
            var values = new int[8];
            if (gameManager.MapVisibility != null)
                for (var y = 0; y < 16; y++)
                {
                    var index = y / 2;
                    for (var x = 0; x < 16; x++)
                        values[index] = values[index] << 1 | (gameManager.MapVisibility[x, y] ? 0x1 : 0x0);
                }

            for (var i = 0; i < values.Length; i++)
                saveManager.SetInt("map" + i, values[i]);
        }

        public static void LoadSaveFile(GameManager gameManager, int slot)
        {
            if (!gameManager.SaveManager.LoadFile(Path.Combine(Values.PathSaveFolder, SaveFileNameGame + slot)))
                return;

            var saveManager = new SaveManager();

            gameManager.SaveSlot = slot;
            gameManager.Equipment = new GameItemCollected[GameManager.EquipmentSlots];
            gameManager.CollectedItems.Clear();
            gameManager.DungeonMaps = new Dictionary<string, GameManager.MiniMap>();

            if (!saveManager.LoadFile(Path.Combine(Values.PathSaveFolder, SaveFileName + slot)))
                return;

            gameManager.SaveName = saveManager.GetString("savename");
            gameManager.ThiefState = saveManager.GetBool("ThiefState", false);
            gameManager.MaxHearts = saveManager.GetInt("maxHearts");
            gameManager.CurrentHealth = saveManager.GetInt("currentHealth");
            gameManager.CloakType = saveManager.GetInt("cloak", 0);
            gameManager.SelectedOcarinaSong = saveManager.GetInt("ocarinaSong", -1);
            gameManager.GuardianAcornCount = saveManager.GetInt("guardianAcornCount", 0);
            gameManager.PieceOfPowerCount = saveManager.GetInt("pieceOfPowerCount", 0);
            gameManager.DeathCount = saveManager.GetInt("deathCount", 0);
            gameManager.KillCount = saveManager.GetInt("killCount", 0);
            gameManager.TotalPlaytime = saveManager.GetFloat("totalPlaytime", 0.0f);
            gameManager.GameCleared = saveManager.GetBool("cleared", false);
            gameManager.CurrentSessionPlaytime = 0.0f;

            gameManager.DebugMode = saveManager.GetBool("debugMode", false);

            // so the map positions is still shown right even if the game was saved outside of the overworld
            if (saveManager.ContainsValue("mapPosX"))
                gameManager.PlayerMapPosition = new Point(
                    saveManager.GetInt("mapPosX"),
                    saveManager.GetInt("mapPosY"));
            else
                gameManager.PlayerMapPosition = null;

            // load the dungeon discovery state
            var strDungeonKeys = saveManager.GetString("dungeonKeyNames");
            if (!string.IsNullOrEmpty(strDungeonKeys))
            {
                var keys = strDungeonKeys.Split(',');

                for (var i = 0; i < keys.Length - 1; i++)
                {
                    // make sure the mini map is loaded
                    gameManager.LoadMiniMap(keys[i]);

                    // should never happen
                    if (!gameManager.DungeonMaps.TryGetValue(keys[i], out var map))
                        continue;

                    var width = map.Tiles.GetLength(0);
                    var height = map.Tiles.GetLength(1);

                    for (var y = 0; y < height; y++)
                    {
                        var line = saveManager.GetString(keys[i] + "line" + y);

                        // should never happen
                        if (line == null)
                            continue;

                        var splitLine = line.Split(',');

                        // should never happen
                        if (splitLine.Length - 1 != width)
                            continue;

                        for (var x = 0; x < width; x++)
                            map.Tiles[x, y].DiscoveryState = splitLine[x] == "1";
                    }
                }
            }

            // load equipped items
            for (var i = 0; i < gameManager.Equipment.Length; i++)
            {
                var strItem = saveManager.GetString("equipment" + i);

                if (!string.IsNullOrEmpty(strItem))
                {
                    // load the collected item
                    gameManager.CollectItem(GetGameItem(strItem), i);
                }
                else
                {
                    gameManager.Equipment[i] = null;
                }
            }

            // load all the collected items
            string strObject;
            var counter = 0;
            while ((strObject = saveManager.GetString("object" + counter)) != null)
            {
                // add the collected object
                gameManager.CollectItem(GetGameItem(strObject));
                counter++;
            }

            // load the discovered map data map
            gameManager.MapVisibility = new bool[16, 16];
            var values = new int[8];

            for (var i = 0; i < values.Length; i++)
                values[i] = saveManager.GetInt("map" + i);

            for (var y = 0; y < 16; y++)
            {
                var index = y / 2;

                for (var x = 0; x < 16; x++)
                {
                    // check the first bit of the 32bit value
                    gameManager.MapVisibility[x, y] = (values[index] & 0x80000000) != 0;
                    values[index] = values[index] << 1;
                }
            }
            // Get the current map and position of Link.
            gameManager.LoadedMap = saveManager.GetString("currentMap");
            gameManager.SavePositionX = saveManager.GetInt("posX");
            gameManager.SavePositionY = saveManager.GetInt("posY");
            gameManager.SaveDirection = saveManager.GetInt("dir");
        }

        public static GameItemCollected GetGameItem(string strItem)
        {
            var strSplit = strItem.Split(':');
            if (strSplit.Length < 2)
                return new GameItemCollected(strSplit.Length > 0 ? strSplit[0] : "");

            var item = new GameItemCollected(strSplit[0]);

            if (short.TryParse(strSplit[1], out var count))
                item.Count = count;

            if (strSplit.Length > 2)
                item.LocationBounding = strSplit[2];

            return item;
        }
    }
}
