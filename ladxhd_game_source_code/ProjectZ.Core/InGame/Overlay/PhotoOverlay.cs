using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZ.InGame.Controls;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.Overlay
{
    class PhotoOverlay
    {
        private DictAtlasEntry[] _spritePhotos;
        private DictAtlasEntry _spriteBook;
        private DictAtlasEntry _spriteCursor;
        private DictAtlasEntry _spriteNop;
        private DictAtlasEntry _spriteOk;
        private DictAtlasEntry _spriteButtonRed;
        private DictAtlasEntry _spriteButtonYellow;
        private DictAtlasEntry _spriteText_A, _spriteText_B, _spriteText_X, _spriteText_O;
        private DictAtlasEntry _spriteCancel;
        private DictAtlasEntry _spritePrint;

        private bool[] _unlockState = new bool[12];
        private int _cursorIndex;

        private float _transitionValue;
        private float _transitionCounter;
        private const float TransitionTimeOpen = 125;
        private const float TransitionTimeClose = 125;
        private bool _isShowingImage;

        private bool _hideButtons;

        private float _cursorState;
        private float _cursorCounter;
        private float _cursorTime = 200f;
        private bool _cursorPressed;

        private void LoadPhotoImages()
        {
            _spritePhotos = new DictAtlasEntry[12];
            for (var i = 0; i < 12; i++)
                _spritePhotos[i] = Resources.GetPhotoSprite("photo_" + (i + 1));

            _spriteNop = Resources.GetSprite("photo_no");
            _spriteOk = Resources.GetSprite("photo_ok");
            _spriteCancel = Resources.GetSprite("photo_cancel");
            _spritePrint = Resources.GetSprite("photo_print");
        }

        public void Load()
        {
            LoadPhotoImages();
            _spriteBook = Resources.GetSprite("photo_book");
            _spriteCursor = Resources.GetSprite("photo_cursor");
            _spriteButtonRed = Resources.GetSprite("photo_button_red");
            _spriteButtonYellow = Resources.GetSprite("photo_button_yellow");
            _spriteText_A = Resources.GetSprite("photo_text_a");
            _spriteText_B = Resources.GetSprite("photo_text_b");
            _spriteText_X = Resources.GetSprite("photo_text_x");
            _spriteText_O = Resources.GetSprite("photo_text_o");
        }

        public void Reload()
        {
            LoadPhotoImages();
        }

        public void OnOpen()
        {
            // check the state of the discovered photos
            _isShowingImage = false;
            _transitionCounter = 0;
            _transitionValue = 0;
            _cursorIndex = 0;
            _hideButtons = false;

            for (var i = 0; i < 12; i++)
                _unlockState[i] = !string.IsNullOrEmpty(Game1.GameManager.SaveManager.GetString("photo_" + (i + 1)));

            // set to alt image or not?
            var altPhoto = Game1.GameManager.SaveManager.GetString("photo_1_alt");
            var useAltPhoto = !string.IsNullOrEmpty(altPhoto);
            _spritePhotos[0] = Resources.GetPhotoSprite(useAltPhoto ? "photo_1_alt" : "photo_1");
        }

        public void Update()
        {
            // Convert the index into a 2D position.
            var cursorPoint = CursorPosition(_cursorIndex);

            if (!_isShowingImage)
            {
                // Show the image.
                if (ControlHandler.ButtonPressed(ControlHandler.ConfirmButton))
                {
                    _cursorPressed = true;

                    if (_cursorCounter > _cursorTime / 2)
                        _cursorCounter = _cursorTime - _cursorCounter;

                    if (_unlockState[_cursorIndex])
                    {
                        _isShowingImage = true;
                        Game1.GameManager.PlaySoundEffect("D360-19-13");
                    }
                    else
                    {
                        Game1.GameManager.PlaySoundEffect("D360-29-1D");
                    }
                }
                // Update the cursor position.
                else
                {
                    if (ControlHandler.ButtonPressed(CButtons.Left))
                        cursorPoint.X--;
                    if (ControlHandler.ButtonPressed(CButtons.Right))
                        cursorPoint.X++;
                    if (ControlHandler.ButtonPressed(CButtons.Up))
                        cursorPoint.Y--;
                    if (ControlHandler.ButtonPressed(CButtons.Down))
                        cursorPoint.Y++;

                    if (cursorPoint.X < 0)
                        cursorPoint.X += 4;
                    if (cursorPoint.X > 3)
                        cursorPoint.X -= 4;
                    if (cursorPoint.Y < 0)
                        cursorPoint.Y += 3;
                    if (cursorPoint.Y > 2)
                        cursorPoint.Y -= 3;
                }

                // Close the page.
                if (ControlHandler.ButtonPressed(ControlHandler.CancelButton))
                    Game1.GameManager.InGameOverlay.CloseOverlay();
            }
            else
            {
                // Show / Hide the buttons and labels.
                if (ControlHandler.ButtonPressed(ControlHandler.ConfirmButton))
                {
                    _hideButtons = !_hideButtons;
                }

                // Close the image.
                if (ControlHandler.ButtonPressed(ControlHandler.CancelButton))
                {
                    _isShowingImage = false;
                    _transitionCounter = TransitionTimeClose;
                    _hideButtons = false;
                    Game1.GameManager.PlaySoundEffect("D360-19-13");
                }
            }

            // Update photo transition in and out.
            if (_isShowingImage && _transitionCounter < TransitionTimeOpen)
            {
                _transitionCounter += Game1.DeltaTime;
                if (_transitionCounter > TransitionTimeOpen)
                    _transitionCounter = TransitionTimeOpen;

                _transitionValue = Math.Clamp(_transitionCounter / TransitionTimeOpen, 0, 1);
            }
            else if (!_isShowingImage && _transitionCounter > 0)
            {
                _transitionCounter -= Game1.DeltaTime;
                if (_transitionCounter < 0)
                    _transitionCounter = 0;

                _transitionValue = _transitionCounter / TransitionTimeClose;
                _cursorState = MathF.Sin(_transitionValue * MathF.PI * 0.5f);
            }

            // Show cursor animation on button press.
            if (_cursorPressed)
            {
                _cursorCounter += Game1.DeltaTime;
                if (_cursorCounter >= _cursorTime)
                {
                    _cursorCounter = 0;
                    _cursorPressed = false;
                }
                _cursorState = MathF.Sin(_cursorCounter / _cursorTime * MathF.PI);
            }

            // Update cursor index.
            var cursorIndexNew = CursorIndex(cursorPoint);
            if (_cursorIndex != cursorIndexNew)
            {
                Game1.GameManager.PlaySoundEffect("D360-10-0A");
                _cursorIndex = cursorIndexNew;
            }
        }

        private Point CursorPosition(int index)
        {
            return new Point(index % 2 + (index / 6) * 2, (index % 6) / 2);
        }

        private int CursorIndex(Point position)
        {
            return position.X % 2 + position.X / 2 * 6 + position.Y * 2;
        }

        public void Draw(SpriteBatch spriteBatch, float transparency)
        {
            var scale = Game1.UiScale + GameSettings.SeqScaleAmplify;

            // Set the photobook position.
            var bookPosition = new Vector2(
                Game1.WindowWidth / 2 - (_spriteBook.SourceRectangle.Width * scale) / 2,
                Game1.WindowHeight / 2 - (_spriteBook.SourceRectangle.Height * scale) / 2);

            // Push the photobook up so it doesn't overlap the textbox.
            var textBoxY = Game1.GameManager.InGameOverlay.TextboxOverlay.DialogBoxTextBox.Y;
            var bookBottom = bookPosition.Y + _spriteBook.SourceRectangle.Height * scale;
            if (bookBottom > textBoxY - 6 * scale)
                bookPosition.Y -= bookBottom - (textBoxY - 6 * scale);

            spriteBatch.Draw(_spriteBook.Texture, bookPosition, _spriteBook.SourceRectangle,
                Color.White * transparency, 0, Vector2.Zero, new Vector2(scale), SpriteEffects.None, 0);

            // Draw the images.
            for (var i = 0; i < 12; i++)
            {
                var imageSprite = _unlockState[i] ? _spriteOk : _spriteNop;
                var position = bookPosition +
                               new Vector2(27 + (i % 2) * 32 + (i / 6) * 88, 19 + ((i % 6) / 2) * 32) * scale -
                               new Vector2(imageSprite.SourceRectangle.Width / 2, 0) * scale;
                spriteBatch.Draw(imageSprite.Texture, position, imageSprite.SourceRectangle,
                    Color.White * transparency, 0, Vector2.Zero, new Vector2(scale), SpriteEffects.None, 0);
            }

            // Draw the cursor.
            var cursorPosition = bookPosition +
                           new Vector2(12 + (_cursorIndex % 2) * 32 + (_cursorIndex / 6) * 88, 8 + ((_cursorIndex % 6) / 2) * 32) * scale +
                           new Vector2(21, 21) * scale -
                           new Vector2(2, 2) * scale * _cursorState;
            spriteBatch.Draw(_spriteCursor.Texture, cursorPosition, _spriteCursor.SourceRectangle,
                Color.White * transparency, 0, Vector2.Zero, new Vector2(scale), SpriteEffects.None, 0);

            // Draw the selected image.
            if (_transitionValue != 0)
            {
                // Draw the photograph.
                var texture  = _spritePhotos[_cursorIndex].Texture;
                var startPos = bookPosition + new Vector2(27 + (_cursorIndex % 2) * 32 + (_cursorIndex / 6) * 88, 27 + ((_cursorIndex % 6) / 2) * 32) * scale;
                var position = Vector2.Lerp(startPos, new Vector2(Game1.WindowWidth / 2, Game1.WindowHeight / 2), _transitionValue);
                var source   = _spritePhotos[_cursorIndex].SourceRectangle;
                var color    = Color.White * transparency * _transitionValue;
                var origin   = new Vector2(_spritePhotos[_cursorIndex].SourceRectangle.Width / 2, _spritePhotos[_cursorIndex].SourceRectangle.Height / 2f);
                var vecscale = new Vector2(scale * (0.1f + _transitionValue * 0.9f));
                spriteBatch.Draw(texture, position, source, color, 0, origin, vecscale, SpriteEffects.None, 0);

                // Hide the buttons and text until transition is finished.
                if (_transitionCounter < 125)
                    return;

                // Don't draw the buttons if hidden.
                if (!_hideButtons)
                {
                    // Default to XBox style buttons.
                    var buttonSpriteTop = _spriteText_A;
                    var buttonSpriteBot = _spriteText_B;

                    // Set buttons to Nintendo style buttons.
                    if (GameSettings.Controller == "Nintendo")
                    {
                        buttonSpriteTop = _spriteText_B;
                        buttonSpriteBot = _spriteText_A;
                    }

                    // Set buttons to Playstation style buttons.
                    if (GameSettings.Controller == "Playstation")
                    {
                        buttonSpriteTop = _spriteText_X;
                        buttonSpriteBot = _spriteText_O;
                    }
                    // Draw the red button.
                    var buttonRedPosition = new Vector2(Game1.WindowWidth / 2 + 24 * scale, Game1.WindowHeight / 2 + 32 * scale);
                    spriteBatch.Draw(_spriteButtonRed.Texture, buttonRedPosition, _spriteButtonRed.SourceRectangle,
                        Color.White * transparency, 0, Vector2.Zero, new Vector2(scale), SpriteEffects.None, 0);

                    // Draw the red button text.
                    var buttonTextAPosition = new Vector2(Game1.WindowWidth / 2 + 29 * scale, Game1.WindowHeight / 2 + 35 * scale);
                    spriteBatch.Draw(buttonSpriteTop.Texture, buttonTextAPosition, buttonSpriteTop.SourceRectangle,
                        Color.White * transparency, 0, Vector2.Zero, new Vector2(scale), SpriteEffects.None, 0);

                    // Draw the "Print" text.
                    var printPosition = new Vector2(Game1.WindowWidth / 2 + 41 * scale, Game1.WindowHeight / 2 + 34 * scale);
                    spriteBatch.Draw(_spritePrint.Texture, printPosition, _spritePrint.SourceRectangle,
                        Color.White * transparency, 0, Vector2.Zero, new Vector2(scale), SpriteEffects.None, 0);

                    // Draw the yellow button.
                    var buttonYellowPosition = new Vector2(Game1.WindowWidth / 2 + 24 * scale, Game1.WindowHeight / 2 + 48 * scale);
                    spriteBatch.Draw(_spriteButtonYellow.Texture, buttonYellowPosition, _spriteButtonYellow.SourceRectangle,
                        Color.White * transparency, 0, Vector2.Zero, new Vector2(scale), SpriteEffects.None, 0);

                    // Draw the yellow button text.
                    var buttonTextBPosition = new Vector2(Game1.WindowWidth / 2 + 29 * scale, Game1.WindowHeight / 2 + 51 * scale);
                    spriteBatch.Draw(buttonSpriteBot.Texture, buttonTextBPosition, buttonSpriteBot.SourceRectangle,
                        Color.White * transparency, 0, Vector2.Zero, new Vector2(scale), SpriteEffects.None, 0);

                    // Draw the "Cancel" text.
                    var cancelPosition = new Vector2(Game1.WindowWidth / 2 + 41 * scale, Game1.WindowHeight / 2 + 50 * scale);
                    spriteBatch.Draw(_spriteCancel.Texture, cancelPosition, _spriteCancel.SourceRectangle,
                        Color.White * transparency, 0, Vector2.Zero, new Vector2(scale), SpriteEffects.None, 0);
                }
            }

            // After textbox overlay closes and a photo is not being shown.
            if (!Game1.GameManager.InGameOverlay.TextboxOverlay.IsOpen && !_isShowingImage)
            {
                // Draw the hint textbox background.
                var uiScale = Game1.UiScale;
                var textboxRef = Game1.GameManager.InGameOverlay.TextboxOverlay.DialogBoxTextBox;
                spriteBatch.Draw(Resources.SprWhite, textboxRef, Values.TextboxBackgroundColor * transparency);

                // Build the textbox string.
                var confirmString = Game1.LanguageManager.GetString("photo_book_select", "error");
                var cancelString = Game1.LanguageManager.GetString("photo_book_cancel", "error");
                var textBoxString =  confirmString + "\n" + cancelString;

                // Draw the hint text.
                GameFS.DrawString(spriteBatch, textBoxString,
                    new Vector2(textboxRef.X + 5 * uiScale, textboxRef.Y + 5 * uiScale),
                    Values.TextboxFontColor * transparency,
                    0, Vector2.Zero, uiScale, SpriteEffects.None, 0);
            }
        }
    }
}
