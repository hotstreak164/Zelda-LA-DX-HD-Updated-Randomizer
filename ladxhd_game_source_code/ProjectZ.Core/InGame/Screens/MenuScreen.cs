using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectZ.InGame.Controls;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.Pages;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.Screens
{
    public class MenuScreen : Screen
    {
        private Matrix _animationMatrix => Game1.GetMatrix * Matrix.CreateScale(Game1.UiScale);
        private Animator _linkAnimation = new Animator();
        private Texture2D _sprBackground;
        private Rectangle _menuRectangle;

        private int _scale = 3;
        private int _menuWidth;
        private int _menuHeight;
        private int _backgroundWidth;

        private int _leftBar;
        private int _rightBar;
        private int _topBar;
        private int _bottomBar;
        private int _posX;

        public MenuScreen(string screenId) : base(screenId) { }

        public override void Load(ContentManager content)
        {
            string texture = (GameSettings.MenuBorder) switch
            {
                0 => "Menu/menuBackground",
                1 => "Menu/menuBackgroundB",
                2 => "Menu/menuBackgroundC"
            };
            _sprBackground = content.Load<Texture2D>(texture);

            _linkAnimation = AnimatorSaveLoad.LoadAnimator("menu_link");
            _linkAnimation.Play("idle");

            _menuWidth = Values.MinWidth - 32;
            _menuHeight = Values.MinHeight - 32;
        }

        public void SetBackground(Texture2D texture)
        {
            _sprBackground = texture;
        }

        public override void OnLoad()
        {
            Game1.UiPageManager.ClearStack();
            Game1.UiPageManager.ChangePage(typeof(MainMenuPage), null, PageManager.TransitionAnimation.TopToBottom, PageManager.TransitionAnimation.TopToBottom);

            Game1.GameManager.ResetMusic();
            Game1.GameManager.SetMusic(16, 0);

            Game1.GbsPlayer.SetVolumeMultiplier(1.0f);
            Game1.GbsPlayer.Play();
        }

        public override void Update(GameTime gameTime)
        {
            _scale = Game1.UiScale;

            if (_scale <= 0)
                _scale = 1;

            _backgroundWidth = (int)Math.Ceiling(Game1.WindowWidth / (double)(32 * _scale) + 1) * 32 * _scale;

            _menuRectangle = new Rectangle(
                Game1.WindowWidth / 2 - _menuWidth * _scale / 2,
                Game1.WindowHeight / 2 - _menuHeight * _scale / 2, _menuWidth * _scale, _menuHeight * _scale);

            _menuRectangle.X = _menuRectangle.X / _scale * _scale;
            _menuRectangle.Y = _menuRectangle.Y / _scale * _scale;

            _topBar = (int)Math.Ceiling((Game1.WindowHeight / 2 - _menuHeight * _scale / 2) / (float)_scale / _sprBackground.Height) * _sprBackground.Height;
            _bottomBar = (int)Math.Ceiling((Game1.WindowHeight / 2 - _menuHeight * _scale / 2) / (float)_scale / _sprBackground.Height) * _sprBackground.Height;

            _posX = (int)Math.Ceiling(_menuRectangle.X / (float)_scale / 32) * 32 - _menuRectangle.X / _scale;

            _leftBar = (int)Math.Ceiling((Game1.WindowWidth / 2 - _menuWidth * _scale / 2) / (float)_scale / _sprBackground.Width) * _sprBackground.Width;
            _rightBar = _leftBar;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw the black background rectangle
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, null, null, null, Game1.GetMatrix);
            spriteBatch.Draw(Resources.SprWhite, _menuRectangle, Color.Black);

            // Back Button Text
            {
                var backStr = "";
                if (ControlHandler.LastKeyboardDown && ControlHandler.ButtonDictionary[ControlHandler.CancelButton].Keys.Length > 0)
                    backStr = ControlHandler.ButtonDictionary[ControlHandler.CancelButton].Keys[0].ToString();
                if (!ControlHandler.LastKeyboardDown && ControlHandler.ButtonDictionary[ControlHandler.CancelButton].Buttons.Length > 0)
                    backStr = ControlHandler.GetButtonName(ControlHandler.ButtonDictionary[ControlHandler.CancelButton].Buttons[0]);

                var strType = Game1.LanguageManager.GetString("main_menu_back", "error");
                var backHelp = backStr + " " + strType;
                var backTextSize = GameFS.MeasureString(backHelp);
                var backPos = new Vector2(_menuRectangle.X + 2 * _scale, _menuRectangle.Bottom - backTextSize.Y * _scale);

                GameFS.DrawString(spriteBatch, backHelp, backPos, Color.White, 0, Vector2.Zero, _scale, SpriteEffects.None, 0);
            }

            // Select Button Text
            {
                var selectStr = "";
                if (ControlHandler.LastKeyboardDown && ControlHandler.ButtonDictionary[ControlHandler.ConfirmButton].Keys.Length > 0)
                    selectStr = ControlHandler.ButtonDictionary[ControlHandler.ConfirmButton].Keys[0].ToString();
                if (!ControlHandler.LastKeyboardDown && ControlHandler.ButtonDictionary[ControlHandler.ConfirmButton].Buttons.Length > 0)
                    selectStr = ControlHandler.GetButtonName(ControlHandler.ButtonDictionary[ControlHandler.ConfirmButton].Buttons[0]);

                var strType = Game1.LanguageManager.GetString("main_menu_select", "error");
                var inputHelp = selectStr + " " + strType;
                var selectTextSize = GameFS.MeasureString(inputHelp);
                var selectPos =  new Vector2(_menuRectangle.Right - (selectTextSize.X + 2) * _scale, _menuRectangle.Bottom - selectTextSize.Y * _scale);

                GameFS.DrawString(spriteBatch, inputHelp, selectPos, Color.White, 0, Vector2.Zero, _scale, SpriteEffects.None, 0);
            }

            // Tooltip Button Text
            if (Game1.UiPageManager.PageHasTooltips())
            {
                var tooltipStr = "";
                if (ControlHandler.LastKeyboardDown && ControlHandler.ButtonDictionary[CButtons.Y].Keys.Length > 0)
                    tooltipStr = ControlHandler.ButtonDictionary[CButtons.Y].Keys[0].ToString();
                if (!ControlHandler.LastKeyboardDown && ControlHandler.ButtonDictionary[CButtons.Y].Buttons.Length > 0)
                    tooltipStr = ControlHandler.GetButtonName(ControlHandler.ButtonDictionary[CButtons.Y].Buttons[0]);

                var tooltipType = Game1.LanguageManager.GetString("main_menu_tooltip", "error");
                var tooltipHelp = tooltipStr + " " + tooltipType;
                var tooltipTextSize = GameFS.MeasureString(tooltipHelp);
                var tooltipPos = new Vector2(_menuRectangle.X + (_menuRectangle.Width - tooltipTextSize.X * _scale) / 2f, _menuRectangle.Bottom - tooltipTextSize.Y * _scale);

                GameFS.DrawString(spriteBatch, tooltipHelp, tooltipPos, Color.White, 0, Vector2.Zero, _scale, SpriteEffects.None, 0);
            }
            spriteBatch.End();
        }

        public override void DrawTop(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, null, null, null, Game1.GetMatrix);

            // top
            spriteBatch.Draw(_sprBackground, new Rectangle(
                    -_posX * _scale, _menuRectangle.Y - _topBar * _scale, _backgroundWidth, _topBar * _scale),
                new Rectangle(0, 0, _backgroundWidth / _scale, _topBar), Color.White);
            // bottom
            spriteBatch.Draw(_sprBackground, new Rectangle(
                    -_posX * _scale, _menuRectangle.Bottom, _backgroundWidth, _bottomBar * _scale),
                new Rectangle(0, 0, _backgroundWidth / _scale, _bottomBar), Color.White);

            // left
            spriteBatch.Draw(_sprBackground, new Rectangle(
                    _menuRectangle.X - _leftBar * _scale, _menuRectangle.Y, _leftBar * _scale, _menuHeight * _scale),
                new Rectangle(0, 0, _leftBar, _menuHeight), Color.White);
            // right
            spriteBatch.Draw(_sprBackground, new Rectangle(
                    _menuRectangle.Right, _menuRectangle.Y, _rightBar * _scale, _menuHeight * _scale),
                new Rectangle(0, 0, _rightBar, _menuHeight), Color.White);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, null, null, null, _animationMatrix);

            spriteBatch.End();
            spriteBatch.Begin();
        }
    }
}