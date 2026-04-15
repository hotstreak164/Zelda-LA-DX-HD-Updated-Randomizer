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

        private int _lastTrackId = -1;
        private bool _introMusic;

        public ObjMusicTile() : base("editor music") { }

        public ObjMusicTile(Map.Map map, int posX, int posY) : base(map)
        {
            AddComponent(UpdateComponent.Index, new UpdateComponent(Update));

            // Load the music tilemap data.
            UpdateMusicData();

            // We should only need to resolve this once on it's creation.
            _introMusic = Game1.GameManager.SaveManager.GetString("introMusic", "0") == "1";
        }

        private void Update()
        {
            // There is no valid music data, Link is showing the sword, or the map is currently transitioning.
            if (_musicData == null || MapManager.ObjLink.IsShowingSword() || MapManager.ObjLink.IsTransitioning)
                return;

            // If the intro is taking place (before the sword), force the intro music.
            if (_introMusic && _lastTrackId != 28)
            {
                _lastTrackId = 28;
                Game1.AudioManager.SetMusic(28, 0, true);
                return;
            }
            // If we're not forcing the intro music get the tile data.
            else if (!_introMusic)
            {
                // Offset the Y position by 4 pixels to match Link's body box center.
                var linkPosX = (int)(MapManager.ObjLink.PosX - Map.MapOffsetX * Values.TileSize) / 16;
                var linkPosY = (int)(MapManager.ObjLink.PosY - 4 - Map.MapOffsetY * Values.TileSize) / 16;
                var position = new Point(linkPosX, linkPosY);

                // Check if the current tile has music.
                var posCheck1 = (0 <= position.X && position.X < _musicData.GetLength(0));
                var posCheck2 = (0 <= position.Y && position.Y < _musicData.GetLength(1));

                // If the track has been resolved.
                var trackStr = _musicData[position.X, position.Y];

                // Try to parse as an integer.
                if (int.TryParse(trackStr, out var trackID))
                {
                    if (_lastTrackId != trackID)
                    {
                        _lastTrackId = trackID;
                        Game1.AudioManager.SetMusic(trackID, 0, true);
                    }
                }
            }
        }

        public void SwordCollected()
        {
            // Signal that the intro music is done.
            _lastTrackId = -1;
            _introMusic = false;
        }

        public void UpdateMusicData()
        {
            // Default to modern music tilemap data.
            string musicTileData = "musicOverworld.data";

            // Load classic music tilemap data.
            if (GameSettings.ClassicMusic)
                musicTileData = "musicOverworldClassic.data";

            // Reload the data into the game.
            _musicData = DataMapSerializer.LoadData(Path.Combine(Values.PathDataFolder, musicTileData));
        }

    }
}
