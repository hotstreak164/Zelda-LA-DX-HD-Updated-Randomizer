using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZ.InGame.Controls;
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
            if (ControlHandler.ButtonPressed(ControlHandler.CancelButton) &&
               !Game1.GameManager.InGameOverlay.TextboxOverlay.IsOpen)
                Game1.GameManager.InGameOverlay.CloseOverlay();
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
                Game1.WindowWidth / 2 - (int)(width * Game1.UiScale) / 2,
                Game1.WindowHeight / 2 - (int)(height * Game1.UiScale) / 2,
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

            spriteBatch.DrawString(Resources.GameFont, inputHelper,
                new Vector2(8 * Game1.UiScale, Game1.WindowHeight - 16 * Game1.UiScale),
                Color.White * transparency, 0, Vector2.Zero, Game1.UiScale, SpriteEffects.None, 0);
        }
    }
}
