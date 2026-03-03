using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZ.InGame.Controls;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.Overlay.Sequences
{
    class MapOverlaySequence : GameSequence
    {
        private MapOverlay _mapOverlay;

        public MapOverlaySequence()
        {
            _sequenceWidth = 144;
            _sequenceHeight = 144;

            _mapOverlay = new MapOverlay(_sequenceWidth, _sequenceHeight, 0, true);
            _mapOverlay.Load();
            _mapOverlay.IsSelected = true;
        }

        public override void OnStart()
        {
            base.OnStart();

            _mapOverlay.OnFocus();
        }

        public override void Update()
        {
            base.Update();

            _mapOverlay.UpdateRenderTarget();
            _mapOverlay.Update();

            // Overlay can be closed if dialog box is not visible.
            if (ControlHandler.ButtonPressed(ControlHandler.CancelButton) && !Game1.GameManager.InGameOverlay.TextboxOverlay.IsOpen)
            {
                Game1.GameManager.InGameOverlay.CloseOverlay();
                MapManager.ObjLink.ManboTeleport = false;
            }
        }

        public override void DrawRT(SpriteBatch spriteBatch)
        {
            _mapOverlay.DrawRenderTarget(spriteBatch);
            Game1.Graphics.GraphicsDevice.SetRenderTarget(null);
        }

        public override void Draw(SpriteBatch spriteBatch, float transparency)
        {
            spriteBatch.End();

            // Use unscaled logical dimensions - the overlay itself handles UiScale.
            var width = _sequenceWidth;
            var height = _sequenceHeight;

            _mapOverlay.Draw(spriteBatch, new Rectangle(
                Game1.WindowWidth / 2 - width * Game1.UiScale / 2,
                Game1.WindowHeight / 2 - height * Game1.UiScale / 2,
                width, height),
                Color.White * transparency, Game1.GetMatrix);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);

            // Draw the close + button text.
            var selectStr = "";
            if (ControlHandler.LastKeyboardDown && ControlHandler.ButtonDictionary[ControlHandler.CancelButton].Keys.Length > 0)
                selectStr = ControlHandler.ButtonDictionary[ControlHandler.CancelButton].Keys[0].ToString();

            if (!ControlHandler.LastKeyboardDown && ControlHandler.ButtonDictionary[ControlHandler.CancelButton].Buttons.Length > 0)
                selectStr = ControlHandler.GetButtonName(ControlHandler.ButtonDictionary[ControlHandler.CancelButton].Buttons[0]);

            var inputHelper = selectStr + ": " + Game1.LanguageManager.GetString("map_overlay_close", "error");

            GameFS.DrawString(spriteBatch, inputHelper, new Vector2(8 * Game1.UiScale, Game1.WindowHeight - 16 * Game1.UiScale), Color.White * transparency, 0, Vector2.Zero, Game1.UiScale, SpriteEffects.None, 0);

            // When navigating the map, get the currently selected map position.
            var nodeSelected = _mapOverlay.SelectionPosition;

            // If we're in map mode and one of the dungeons are selected.
            if ((GameSettings.MapTeleport == 1 || GameSettings.MapTeleport == 3 || GameSettings.MapTeleport == 2 && MapManager.ObjLink.ManboTeleport) && Game1.GameManager.InGameOverlay.TeleportMap.ContainsKey(nodeSelected) && MapManager.ObjLink.Map.IsOverworld)
            {
                // Get the selected dungeon and check if the instrument has been collected.
                int dungeonLevel = Game1.GameManager.InGameOverlay.TeleportMap[nodeSelected].Level - 1;
                var instrument = Game1.GameManager.GetItem("instrument" + dungeonLevel);
                var hasInstrument = instrument != null && instrument.Count > 0;
                var isManboPond = dungeonLevel < 0 && MapManager.ObjLink.ManboTeleport;

                // If instrument has not been collected don't draw the text.
                if (!hasInstrument && !isManboPond)
                    return;

                // Get the correct button to display next to the text.
                var teleStart = "";
                if (ControlHandler.LastKeyboardDown && ControlHandler.ButtonDictionary[CButtons.X].Keys.Length > 0)
                    teleStart = ControlHandler.ButtonDictionary[CButtons.X].Keys[0].ToString();
                if (!ControlHandler.LastKeyboardDown && ControlHandler.ButtonDictionary[CButtons.X].Buttons.Length > 0)
                    teleStart = ControlHandler.GetButtonName(ControlHandler.ButtonDictionary[CButtons.X].Buttons[0]);

                // Set up the string to display.
                var teleString = teleStart + ": " + Game1.LanguageManager.GetString("overlay_teleport", "error");
                var teleTextSize = GameFS.MeasureString(teleString);
                var teleDrawPos = new Vector2(Game1.WindowWidth - (teleTextSize.X + 6) * Game1.UiScale, Game1.WindowHeight - 16 * Game1.UiScale);

                // Draw the teleport button and label.
                GameFS.DrawString(spriteBatch, teleString, teleDrawPos, Color.White, 0, Vector2.Zero, Game1.UiScale, SpriteEffects.None, 0);
            }
        }
    }
}
