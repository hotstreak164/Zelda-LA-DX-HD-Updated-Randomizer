using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ProjectZ;

#if ANDROID
using Microsoft.Xna.Framework.Input.Touch;
#endif

namespace ProjectZ.Base
{
    #region InputCharacter

    internal class InputCharacter
    {
        private readonly string _upper;
        private readonly string _lower;
        private readonly string _alt;

        private readonly Keys[] _code;

        public InputCharacter(string upper, string lower, string alt, params Keys[] code)
        {
            _upper = upper;
            _lower = lower;
            _alt = alt;
            _code = code;
        }

        public InputCharacter(string upper, string lower, params Keys[] code)
            : this(upper, lower, lower, code)
        { }

        public string ReturnCharacter(bool shiftDown, bool altDown)
        {
            return altDown ? _alt : shiftDown ? _upper : _lower;
        }

        public Keys[] ReturnKeys()
        {
            return _code;
        }
    }
    #endregion

    public class InputHandler : GameComponent
    {
        public static KeyboardState KeyboardState => _keyboardState;
        public static KeyboardState LastKeyboardState => _lastKeyboardState;
        public static MouseState MouseState => _mouseState;
        public static MouseState LastMouseState => _lastMouseState;

        private static KeyboardState _keyboardState;
        private static KeyboardState _lastKeyboardState;

        private static MouseState _mouseState;
        private static MouseState _lastMouseState;

        private static GamePadState _gamePadState;
        private static GamePadState _lastGamePadState;

        private static float _gamePadAccuracy = 0.2f;

        private static string _textInputBuffer = "";
        private static bool _textInputEnabled;

        private static int _gamePadIndex = 0;
        public static int GamePadIndex => _gamePadIndex;

        public static GamePadState GamePadState => _gamePadState;
        public static GamePadState LastGamePadState => _lastGamePadState;

    #if ANDROID
        private static TouchCollection _touchState;
        private static TouchCollection _lastTouchState;

        public static TouchCollection TouchState => _touchState;
        public static TouchCollection LastTouchState => _lastTouchState;

        private static readonly bool _useAnalogTriggerFallback = true;
    #else
        private static readonly bool _useAnalogTriggerFallback = true;
    #endif

        private const float TriggerPressThreshold = 0.5f;

        public InputHandler(Game game)
            : base(game)
        {
            // Rather than using a predefined alphabet which limits which characters the user is
            // allowed to type for a name, we capture the input and filter out invalid chars later.
        #if !ANDROID
            Game.Window.TextInput += OnTextInput;
        #endif

        #if ANDROID
            TouchPanel.EnabledGestures = GestureType.None;
        #endif
        }

        public override void Update(GameTime gameTime)
        {
            _lastKeyboardState = _keyboardState;
            _keyboardState = Keyboard.GetState();

            _lastMouseState = _mouseState;
            _mouseState = Mouse.GetState();

        #if ANDROID
            PlatformInput.BeginFrame();
            _lastTouchState = _touchState;
            _touchState = TouchPanel.GetState();
        #endif

            DetectGamePad();

            _lastGamePadState = _gamePadState;
            _gamePadState = GamePad.GetState(_gamePadIndex);

            // Prevents input when Window is in the background (do we really want this?).
            if (!Game1.WasActive)
                ResetInputState();
        }

        public static void DetectGamePad()
        {
            // Try 0..3 and pick the first connected pad.
            for (int i = 0; i < 4; i++)
            {
                var st = GamePad.GetState(i);
                if (st.IsConnected)
                {
                    _gamePadIndex = i;
                    return;
                }
            }
            _gamePadIndex = 0;
        }

        /// <summary>
        /// set the last input state to the current state
        /// </summary>
        public static void ResetInputState()
        {
            _lastKeyboardState = _keyboardState;
            _lastMouseState = _mouseState;
            _lastGamePadState = _gamePadState;

        #if ANDROID
            _lastTouchState = _touchState;
        #endif
        }

        public static bool LastKeyDown(Keys key)
        {
            return _lastKeyboardState.IsKeyDown(key);
        }

        public static bool KeyDown(Keys key)
        {
            return _keyboardState.IsKeyDown(key);
        }

        public static bool KeyPressed(Keys key)
        {
            return _keyboardState.IsKeyDown(key) &&
                _lastKeyboardState.IsKeyUp(key);
        }

        public static bool KeyReleased(Keys key)
        {
            return _keyboardState.IsKeyUp(key) &&
                _lastKeyboardState.IsKeyDown(key);
        }

        public static List<Keys> GetPressedKeys()
        {
            var pressedKeys = new List<Keys>();
            var downKeys = _keyboardState.GetPressedKeys();

            for (var i = 0; i < downKeys.Length; i++)
            {
                if (KeyPressed(downKeys[i]))
                    pressedKeys.Add(downKeys[i]);
            }
            return pressedKeys;
        }

        public static List<Buttons> GetPressedButtons()
        {
            var pressedKeys = new List<Buttons>();

            foreach (Buttons button in Enum.GetValues(typeof(Buttons)))
            {
                if (GamePadPressed(button))
                    pressedKeys.Add(button);
            }
            return pressedKeys;
        }

        public static bool PlatformSelectPressed()
        {
        #if ANDROID
            return ProjectZ.Base.PlatformInput.ConsumeSelectPressed();
        #else
            return false;
        #endif
        }

        private static bool TriggerDown(bool left)
        {
            var v = left ? _gamePadState.Triggers.Left : _gamePadState.Triggers.Right;
            return v > TriggerPressThreshold;
        }

        private static bool TriggerDownLast(bool left)
        {
            var v = left ? _lastGamePadState.Triggers.Left : _lastGamePadState.Triggers.Right;
            return v > TriggerPressThreshold;
        }

        public static bool GamePadDown(Buttons button)
        {
            if (_useAnalogTriggerFallback)
            {
                if (button == Buttons.LeftTrigger)  return TriggerDown(left: true);
                if (button == Buttons.RightTrigger) return TriggerDown(left: false);
            }
            return _gamePadState.IsButtonDown(button);
        }

        public static bool LastGamePadDown(Buttons button)
        {
            if (_useAnalogTriggerFallback)
            {
                if (button == Buttons.LeftTrigger)  return TriggerDownLast(left: true);
                if (button == Buttons.RightTrigger) return TriggerDownLast(left: false);
            }
            return _lastGamePadState.IsButtonDown(button);
        }

        public static bool GamePadPressed(Buttons button)
        {
            if (_useAnalogTriggerFallback)
            {
                if (button == Buttons.LeftTrigger)  return TriggerDown(true)  && !TriggerDownLast(true);
                if (button == Buttons.RightTrigger) return TriggerDown(false) && !TriggerDownLast(false);
            }
            return _gamePadState.IsButtonDown(button) && _lastGamePadState.IsButtonUp(button);
        }

        public static bool GamePadReleased(Buttons button)
        {
            if (_useAnalogTriggerFallback)
            {
                if (button == Buttons.LeftTrigger)  return !TriggerDown(true)  && TriggerDownLast(true);
                if (button == Buttons.RightTrigger) return !TriggerDown(false) && TriggerDownLast(false);
            }
            return _gamePadState.IsButtonUp(button) && _lastGamePadState.IsButtonDown(button);
        }

        public static bool GamePadLeftStick(Vector2 dir)
        {
            return ((dir.X < 0 && _gamePadState.ThumbSticks.Left.X < -_gamePadAccuracy) || (dir.X > 0 && _gamePadState.ThumbSticks.Left.X > _gamePadAccuracy) ||
                (dir.Y < 0 && _gamePadState.ThumbSticks.Left.Y < -_gamePadAccuracy) || (dir.Y > 0 && _gamePadState.ThumbSticks.Left.Y > _gamePadAccuracy));
        }
        public static bool LastGamePadLeftStick(Vector2 dir)
        {
            return ((dir.X < 0 && _lastGamePadState.ThumbSticks.Left.X < -_gamePadAccuracy) || (dir.X > 0 && _lastGamePadState.ThumbSticks.Left.X > _gamePadAccuracy) ||
                (dir.Y < 0 && _lastGamePadState.ThumbSticks.Left.Y < -_gamePadAccuracy) || (dir.Y > 0 && _lastGamePadState.ThumbSticks.Left.Y > _gamePadAccuracy));
        }

        public static bool GamePadRightStick(Vector2 dir)
        {
            return ((dir.X < 0 && _gamePadState.ThumbSticks.Right.X < -_gamePadAccuracy) || (dir.X > 0 && _gamePadState.ThumbSticks.Right.X > _gamePadAccuracy) ||
                    (dir.Y < 0 && _gamePadState.ThumbSticks.Right.Y < -_gamePadAccuracy) || (dir.Y > 0 && _gamePadState.ThumbSticks.Right.Y > _gamePadAccuracy));
        }

        #region Mouse Region

        //scroll
        public static bool MouseWheelUp()
        {
            return _mouseState.ScrollWheelValue > _lastMouseState.ScrollWheelValue;
        }
        public static bool MouseWheelDown()
        {
            return _mouseState.ScrollWheelValue < _lastMouseState.ScrollWheelValue;
        }

        //down
        public static bool MouseLeftDown()
        {
            return _mouseState.LeftButton == ButtonState.Pressed;
        }
        public static bool MouseLeftDown(Rectangle rectangle)
        {
            return MouseIntersect(rectangle) && MouseLeftDown();
        }
        public static bool MouseRightDown()
        {
            return _mouseState.RightButton == ButtonState.Pressed;
        }
        public static bool MouseMiddleDown()
        {
            return _mouseState.MiddleButton == ButtonState.Pressed;
        }

        //start
        public static bool MouseLeftStart()
        {
            return _mouseState.LeftButton == ButtonState.Pressed && _lastMouseState.LeftButton == ButtonState.Released;
        }
        public static bool MouseRightStart()
        {
            return _mouseState.RightButton == ButtonState.Pressed && _lastMouseState.RightButton == ButtonState.Released;
        }
        public static bool MouseMiddleStart()
        {
            return _mouseState.MiddleButton == ButtonState.Pressed && _lastMouseState.MiddleButton == ButtonState.Released;
        }

        //released
        public static bool MouseLeftReleased()
        {
            return _mouseState.LeftButton == ButtonState.Released && _lastMouseState.LeftButton == ButtonState.Pressed;
        }
        public static bool MouseRightReleased()
        {
            return _mouseState.RightButton == ButtonState.Released && _lastMouseState.RightButton == ButtonState.Pressed;
        }

        //pressed
        public static bool MouseLeftPressed()
        {
            return _mouseState.LeftButton == ButtonState.Pressed && _lastMouseState.LeftButton == ButtonState.Released;
        }
        public static bool MouseLeftPressed(Rectangle rectangle)
        {
            return rectangle.Contains(MousePosition()) && MouseLeftPressed();
        }

        public static bool MouseRightPressed()
        {
            return _mouseState.RightButton == ButtonState.Pressed && _lastMouseState.RightButton == ButtonState.Released;
        }
        public static bool MouseRightPressed(Rectangle rectangle)
        {
            return MouseIntersect(rectangle) && MouseRightPressed();
        }

        public static bool MouseIntersect(Rectangle rectangle)
        {
            return rectangle.Contains(MousePosition());
        }

        public static Point MousePosition()
        {
            // Correct for the OS safe-area offset in fullscreen (e.g. macOS notch).
            // Use the adapter's actual display height rather than PreferredBackBufferHeight,
            // which can be stale after ToggleFullscreen and would produce a wrong offset.
            // On displays with no safe area (Windows, Linux, non-notch Macs) this is 0.
            int offY = Game1.FullScreen ? Game1.Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height - Game1.WindowHeight : 0;
            return new Point(_mouseState.X, _mouseState.Y - offY);
        }

        public static Point LastMousePosition()
        {
            // Correct for the OS safe-area offset in fullscreen (e.g. macOS notch).
            // Use the adapter's actual display height rather than PreferredBackBufferHeight,
            // which can be stale after ToggleFullscreen and would produce a wrong offset.
            // On displays with no safe area (Windows, Linux, non-notch Macs) this is 0.
            int offY = Game1.FullScreen ? Game1.Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height - Game1.WindowHeight : 0;
            return new Point(_lastMouseState.X, _lastMouseState.Y - offY);
        }

        #endregion

        #region return text + return number

        /// <summary>
        /// returns the pressed keys if they are in the InputHandler.alphabet
        /// only returns one key at a time
        /// </summary>
        /// <returns></returns>
        /// 
        public static void EnableTextInput()
        {
            _textInputEnabled = true;
            _textInputBuffer = "";
        }

        public static void DisableTextInput()
        {
            _textInputEnabled = false;
            _textInputBuffer = "";
        }

        private static void OnTextInput(object sender, TextInputEventArgs e)
        {
            if (!_textInputEnabled)
                return;

            if (!char.IsControl(e.Character))
                _textInputBuffer += e.Character;
        }

        public static string ReturnCharacter()
        {
            var result = _textInputBuffer;
            _textInputBuffer = "";
            return result;
        }

        /// <summary>
        /// returns pressed number from d0-d9 and numpad0-numpad9
        /// </summary>
        /// <returns></returns>
        public static int ReturnNumber()
        {
            for (var i = 0; i < 10; i++)
                if (KeyPressed(Keys.D0 + i) || KeyPressed(Keys.NumPad0 + i))
                    return i;

            return -1;
        }

        #endregion
    }
}
