using System.Collections.Generic;
using ProjectZ.InGame.GameObjects.Base;

namespace ProjectZ.InGame.GameObjects.Things
{
    class ObjMusic : GameObject
    {
        private string _title;

        public ObjMusic() : base("editor music") { }

        public ObjMusic(Map.Map map, int posX, int posY, string title) : base(map)
        {
            _title = title;

            if (int.TryParse(_title, out var songNr))
                Map.MapMusic[0] = GetProperMusicTrack(songNr);

            IsDead = true;
        }

        private static readonly Dictionary<int, int> SongToDungeon = new()
        {
            // This matches the music track with the level { track, level }
            { 19, 1 }, { 20, 2 }, { 21, 3 }, { 22, 4 }, 
            { 74, 5 }, { 87, 6 }, { 90, 7 }, { 89, 8 }
        };

        private int GetProperMusicTrack(int songNr)
        {
            int dungeonClear = 23;

            if (SongToDungeon.TryGetValue(songNr, out int dungeonIndex))
            {
                string heartKey = $"d{dungeonIndex}_nHeart";
                string instrumentKey = $"instrument{dungeonIndex - 1}";

                if (Game1.GameManager.SaveManager.GetString(heartKey) == "1" &&
                    (Game1.GameManager.GetItem(instrumentKey)?.Count ?? 0) < 1)
                {
                    return dungeonClear;
                }
            }
            return songNr;
        }
    }
}
