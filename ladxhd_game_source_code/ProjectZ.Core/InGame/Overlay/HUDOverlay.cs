using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZ.Base.UI;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.Overlay
{
    public class HudOverlay
    {
        private readonly ItemSlotOverlay _itemSlotOverlay = new ItemSlotOverlay();

        private readonly UiRectangle _heartBackground;
        private readonly UiRectangle _rupeeBackground;
        private readonly UiRectangle _keyBackground;

        private readonly DictAtlasEntry _saveIcon;

        private Rectangle _gameUiWindow;

        private Point _heartPosition;
        private Point _rubeePosition;
        private Vector2 _saveIconPosition;
        private Point _keyPosition;

        private const int FadeOffsetBackground = 10;
        private const int FadeOffset = 13;

        private const int SaveIconTime = 1000;
        private float _saveIconTransparency;
        private float _saveIconCounter;

        private int _itemsScale;
        private int _heartScale;
        private int _rupeeScale;
        private int _keysScale;
        private int _siconScale;

        // lahdmod values
        public  bool custom_items_show     = true;
        private int  custom_items_scale    = 0;
        public  int  custom_items_offsetx  = 0;
        public  int  custom_items_offsety  = 0;

        public  bool custom_heart_show     = true;
        private int  custom_heart_scale    = 0;
        private int  custom_heart_offsetx  = 0;
        private int  custom_heart_offsety  = 0;

        public  bool custom_rupee_show     = true;
        private int  custom_rupee_scale    = 0;
        private int  custom_rupee_offsetx  = 0;
        private int  custom_rupee_offsety  = 0;

        public  bool custom_keys_show      = true;
        private int  custom_keys_scale     = 0;
        private int  custom_keys_offsetx   = 0;
        private int  custom_keys_offsety   = 0;

        public  bool custom_sicon_show     = true;
        private int  custom_sicon_scale    = 0;

        public HudOverlay()
        {
            // If a mod file exists load the values from it.
            string modFile = Path.Combine(Values.PathLAHDMods, "HUDOverlay.lahdmod");

            if (File.Exists(modFile))
                ModFile.Parse(modFile, this);

            if (custom_heart_show)
            {
                _heartBackground = new UiRectangle(Rectangle.Empty, "heart", Values.ScreenNameGame, Values.OverlayBackgroundColor, Values.OverlayBackgroundBlurColor, null) { Radius = Values.UiBackgroundRadius };
                Game1.UiManager.AddElement(_heartBackground);
            }
            if (custom_rupee_show)
            {
                _rupeeBackground = new UiRectangle(Rectangle.Empty, "rupee", Values.ScreenNameGame, Values.OverlayBackgroundColor, Values.OverlayBackgroundBlurColor, null) { Radius = Values.UiBackgroundRadius };
                Game1.UiManager.AddElement(_rupeeBackground);
            }
            if (custom_keys_show)
            {
                _keyBackground = new UiRectangle(Rectangle.Empty, "keys", Values.ScreenNameGame, Values.OverlayBackgroundColor, Values.OverlayBackgroundBlurColor, null) { Radius = Values.UiBackgroundRadius };
                Game1.UiManager.AddElement(_keyBackground);
            }
            if (custom_sicon_show)
            {
                _saveIcon = Resources.GetSprite("save_icon");
            }
        }

        public void ResolutionChange()
        {
            _itemsScale = custom_items_scale == 0 ? Game1.UiScale : custom_items_scale;
            _heartScale = custom_heart_scale == 0 ? Game1.UiScale : custom_heart_scale;
            _rupeeScale = custom_rupee_scale == 0 ? Game1.UiScale : custom_rupee_scale;
            _keysScale  = custom_keys_scale  == 0 ? Game1.UiScale : custom_keys_scale;
            _siconScale = custom_sicon_scale == 0 ? Game1.UiScale : custom_sicon_scale;
        }

        public void Update(float fadePercentage, float transparency)
        {
            _saveIconCounter -= Game1.DeltaTime;
            if (_saveIconCounter < 0)
                _saveIconCounter = 0;
            _saveIconTransparency = Math.Min(Math.Clamp(_saveIconCounter / 100, 0, 1), Math.Clamp((SaveIconTime - _saveIconCounter) / 100, 0, 1));

            var scale = Math.Min(Game1.WindowWidth / (float)Values.MinWidth, Game1.WindowHeight / (float)Values.MinHeight);

            _gameUiWindow.Width = (int)(Values.MinWidth * scale);
            _gameUiWindow.Height = (int)(Values.MinHeight * scale);

            var ar = MathHelper.Clamp(Game1.WindowWidth / (float)Game1.WindowHeight, 1, 2);

            _gameUiWindow.Width = MathHelper.Clamp((int)(Game1.WindowHeight * ar), 0, Game1.WindowWidth);
            _gameUiWindow.Height = MathHelper.Clamp((int)(Game1.WindowWidth / ar), 0, Game1.WindowHeight);
            _gameUiWindow.X = Game1.WindowWidth / 2 - _gameUiWindow.Width / 2;
            _gameUiWindow.Y = Game1.WindowHeight / 2 - _gameUiWindow.Height / 2;

            // top left
            if (custom_rupee_show)
            {
                _heartPosition = new Point(_gameUiWindow.X + 16 * Game1.UiScale + custom_heart_offsetx, _gameUiWindow.Y + 16 * Game1.UiScale + custom_heart_offsety);
                _heartBackground.Rectangle = ItemDrawHelper.GetHeartRectangle(_heartPosition, _heartScale);
                _heartBackground.Rectangle.X -= (int)(fadePercentage * FadeOffsetBackground * _heartScale);
                _heartBackground.BackgroundColor = Values.OverlayBackgroundColor * transparency;
                _heartBackground.BlurColor = Values.OverlayBackgroundBlurColor * transparency;
            }
            // top right, rupees
            if (custom_heart_show)
            {
                _rubeePosition = new Point(_gameUiWindow.X + _gameUiWindow.Width - ItemDrawHelper.RubeeSize.X * _rupeeScale - 16 * Game1.UiScale + custom_rupee_offsetx, _gameUiWindow.Y + 16 * Game1.UiScale + custom_rupee_offsety);
                _rupeeBackground.Rectangle = ItemDrawHelper.GetRubeeRectangle(new Point(_rubeePosition.X, _rubeePosition.Y), _rupeeScale);
                _rupeeBackground.Rectangle.X += (int)(fadePercentage * FadeOffsetBackground * _rupeeScale);
                _rupeeBackground.BackgroundColor = Values.OverlayBackgroundColor * transparency;
                _rupeeBackground.BlurColor = Values.OverlayBackgroundBlurColor * transparency;
            }
            // top right, keys
            if (custom_keys_show)
            {
                _keyPosition = new Point(_gameUiWindow.X + _gameUiWindow.Width - ItemDrawHelper.KeySize.X * _keysScale - 16 * Game1.UiScale + custom_keys_offsetx, _gameUiWindow.Y + 16 * 2 * _rupeeScale + custom_keys_offsety);
                _keyBackground.Rectangle = ItemDrawHelper.GetKeyRectangle(new Point(_keyPosition.X, _keyPosition.Y), _keysScale);
                _keyBackground.Rectangle.X += (int)(fadePercentage * FadeOffsetBackground * _keysScale);
                if (Game1.GameManager.GetItem("smallkey") is null)
                {
                    _keyBackground.BackgroundColor = Values.OverlayBackgroundColor * 0.0f;
                    _keyBackground.BlurColor = Values.OverlayBackgroundBlurColor * 0.0f;
                }
                else
                {
                    _keyBackground.BackgroundColor = Values.OverlayBackgroundColor * transparency;
                    _keyBackground.BlurColor = Values.OverlayBackgroundBlurColor * transparency;
                }
            }
            // Update overlay position
            int direction = GameSettings.ItemsOnRight ? 1 : -1;
            _itemSlotOverlay.UpdatePositions(_gameUiWindow, new Point(direction * (int)(fadePercentage * FadeOffsetBackground * Game1.UiScale), 0), _itemsScale);

            // Save icon position
            if (custom_sicon_show)
            {
                _saveIconPosition = new Vector2(GameSettings.ItemsOnRight 
                    ? _gameUiWindow.X + _saveIcon.SourceRectangle.Width * Game1.UiScale
                    : _gameUiWindow.X + _gameUiWindow.Width - _saveIcon.SourceRectangle.Width * Game1.UiScale - 16 * _siconScale,
                    _gameUiWindow.Y + _gameUiWindow.Height - _saveIcon.SourceRectangle.Height * Game1.UiScale - 16 * _siconScale);
            }
            _itemSlotOverlay.SetTransparency(transparency);
        }

        public void DrawTop(SpriteBatch spriteBatch, float fadePercentage, float transparency)
        {
            if (UiManager.HideOverlay) { return; }

            // draw the item slots
            if (custom_items_show)
                ItemSlotOverlay.Draw(spriteBatch, _itemSlotOverlay.ItemSlotPosition + new Point(GameSettings.ItemsOnRight ? 1 : -1 * (int)(fadePercentage * FadeOffset * _itemsScale), 0), _itemsScale, transparency);

            // draw dungeon keys
            if (custom_keys_show)
                ItemDrawHelper.DrawSmallKeys(spriteBatch, _keyPosition + new Point((int)(fadePercentage * FadeOffset * _keysScale), 0), _keysScale, Color.White * transparency);

            // draw the rubees
            if (custom_rupee_show)
                ItemDrawHelper.DrawRubee(spriteBatch, _rubeePosition + new Point((int)(fadePercentage * FadeOffset * _rupeeScale), 0), _rupeeScale, Color.Black * transparency);

            // draw the heart position
            if (custom_heart_show)
                ItemDrawHelper.DrawHearts(spriteBatch, _heartPosition - new Point((int)(fadePercentage * FadeOffset * _heartScale), 0), _heartScale, Color.White * transparency);
        }

        public void DrawBlur(SpriteBatch spriteBatch)
        {
            // draw the save icon
            if (custom_sicon_show)
            {
                Resources.RoundedCornerBlurEffect.Parameters["blurColor"].SetValue((Values.OverlayBackgroundBlurColor * _saveIconTransparency).ToVector4());
                DrawHelper.DrawNormalized(spriteBatch, _saveIcon.Texture, _saveIconPosition, _saveIcon.ScaledRectangle, Values.OverlayBackgroundColor * _saveIconTransparency, _siconScale);
            }
        }

        public void ShowSaveIcon()
        {
            _saveIconCounter = SaveIconTime;
        }
    }
}
