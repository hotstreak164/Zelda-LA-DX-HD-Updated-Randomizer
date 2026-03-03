using System.IO;
using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Things
{
    class ObjMusicTile : GameObject
    {
        private string[,] _musicData;
        private string _lastTrack;

        private bool _currentEnabled;
        private int _lastTrackId = -1;

        // @TODO: fade in/out
        public ObjMusicTile() : base("editor music") { }

        public ObjMusicTile(Map.Map map, int posX, int posY) : base(map)
        {
            AddComponent(UpdateComponent.Index, new UpdateComponent(Update));

            // Store current option to detect change later.
            _currentEnabled = GameSettings.ClassicMusic;

            // Load the music tilemap data.
            UpdateMusicData();
        }

        private void Update()
        {
            if (_musicData == null)
                return;

            // Offset the Y position by 4 pixels to match Link's body box center.
            var position = new Point(
                (int)(MapManager.ObjLink.PosX - Map.MapOffsetX * Values.TileSize) / 16,
                (int)(MapManager.ObjLink.PosY - 4 - Map.MapOffsetY * Values.TileSize) / 16);

            if (0 <= position.X && position.X < _musicData.GetLength(0) &&
                0 <= position.Y && position.Y < _musicData.GetLength(1))
            {
                var trackStr = _musicData[position.X, position.Y];
                if (int.TryParse(trackStr, out var trackID))
                {
                    if (_lastTrackId != trackID)
                    {
                        _lastTrackId = trackID;
                        Game1.GameManager.SetMusic(trackID, 0, false);
                    }
                }
            }
            // Detect if the user changed the classic music cues option and reload music tilemap data.
            if (_currentEnabled != GameSettings.ClassicMusic)
                UpdateMusicData();
        }

        private void UpdateMusicData()
        {
            // Default to modern music tilemap data.
            string musicTileData = "musicOverworld.data";

            // Load classic music tilemap data.
            if (GameSettings.ClassicMusic)
                musicTileData = "musicOverworldClassic.data";

            // Reload the data into the game.
            _musicData = DataMapSerializer.LoadData(Path.Combine(Values.PathContentFolder, musicTileData));

            // Update the currently enabled boolean to detect a future change.
            _currentEnabled = GameSettings.ClassicMusic;
        }
    }
}
