using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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
        private static List<InputCharacter> _alphabet;

        public static KeyboardState KeyboardState => _keyboardState;
        public static KeyboardState LastKeyboardState => _lastKeyboardState;
        public static MouseState MouseState => _mouseState;
        public static MouseState LastMousState => _lastMouseState;

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

        private const float TriggerPressThreshold = 0.5f;

        #region Constructor Region

        public InputHandler(Game game)
            : base(game)
        {
            // Rather than using a predefined alphabet which limits which characters the user is
            // allowed to type for a name, we capture the input and filter out invalid chars later.
        #if !ANDROID
            Game.Window.TextInput += OnTextInput;
        #endif

            // Create an array of the valid alphabet characters.
/*          _alphabet = new List<InputCharacter>();

             //Alphabet
            _alphabet.Add(new InputCharacter("A", "a", "ª", Keys.A));
            _alphabet.Add(new InputCharacter("B", "b", Keys.B));
            _alphabet.Add(new InputCharacter("C", "c", "¢", Keys.C));
            _alphabet.Add(new InputCharacter("D", "d", Keys.D));
            _alphabet.Add(new InputCharacter("E", "e", "€", Keys.E));
            _alphabet.Add(new InputCharacter("F", "f", Keys.F));
            _alphabet.Add(new InputCharacter("G", "g", Keys.G));
            _alphabet.Add(new InputCharacter("H", "h", Keys.H));
            _alphabet.Add(new InputCharacter("I", "i", Keys.I));
            _alphabet.Add(new InputCharacter("J", "j", Keys.J));
            _alphabet.Add(new InputCharacter("K", "k", Keys.K));
            _alphabet.Add(new InputCharacter("L", "l", "£", Keys.L));
            _alphabet.Add(new InputCharacter("M", "m", Keys.M));
            _alphabet.Add(new InputCharacter("N", "n", Keys.N));
            _alphabet.Add(new InputCharacter("O", "o", Keys.O));
            _alphabet.Add(new InputCharacter("P", "p", "¶", Keys.P));
            _alphabet.Add(new InputCharacter("Q", "q", Keys.Q));
            _alphabet.Add(new InputCharacter("R", "r", "®", Keys.R));
            _alphabet.Add(new InputCharacter("S", "s", Keys.S));
            _alphabet.Add(new InputCharacter("T", "t", Keys.T));
            _alphabet.Add(new InputCharacter("U", "u", "µ", Keys.U));
            _alphabet.Add(new InputCharacter("V", "v", Keys.V));
            _alphabet.Add(new InputCharacter("W", "w", Keys.W));
            _alphabet.Add(new InputCharacter("X", "x", Keys.X));
            _alphabet.Add(new InputCharacter("Y", "y", Keys.Y));
            _alphabet.Add(new InputCharacter("Z", "z", Keys.Z));

            // Decimal numbers.
            _alphabet.Add(new InputCharacter("~", "`", Keys.OemTilde));
            _alphabet.Add(new InputCharacter("!", "1", "¹", Keys.D1, Keys.NumPad1));
            _alphabet.Add(new InputCharacter("@", "2", "²", Keys.D2, Keys.NumPad2));
            _alphabet.Add(new InputCharacter("#", "3", "³", Keys.D3, Keys.NumPad3));
            _alphabet.Add(new InputCharacter("$", "4", Keys.D4, Keys.NumPad4));
            _alphabet.Add(new InputCharacter("%", "5", Keys.D5, Keys.NumPad5));
            _alphabet.Add(new InputCharacter("^", "6", Keys.D6, Keys.NumPad6));
            _alphabet.Add(new InputCharacter("&", "7", Keys.D7, Keys.NumPad7));
            _alphabet.Add(new InputCharacter("*", "8", Keys.D8, Keys.NumPad8));
            _alphabet.Add(new InputCharacter("(", "9", Keys.D9, Keys.NumPad9));
            _alphabet.Add(new InputCharacter(")", "0", "º", Keys.D0, Keys.NumPad0));

            // Numpad Specific
            _alphabet.Add(new InputCharacter("/", "/", Keys.Divide));
            _alphabet.Add(new InputCharacter("*", "*", Keys.Multiply));
            _alphabet.Add(new InputCharacter("-", "-", Keys.Subtract));
            _alphabet.Add(new InputCharacter("+", "+", Keys.Add));
            _alphabet.Add(new InputCharacter(".", ".", Keys.Decimal));

            // Punctuation.
            _alphabet.Add(new InputCharacter("_", "-", "°", Keys.OemMinus));
            _alphabet.Add(new InputCharacter("+", "=", "±", Keys.OemPlus));
            _alphabet.Add(new InputCharacter("{", "[", Keys.OemOpenBrackets));
            _alphabet.Add(new InputCharacter("}", "]", Keys.OemCloseBrackets));
            _alphabet.Add(new InputCharacter("|", "\\", "¦", Keys.OemPipe));
            _alphabet.Add(new InputCharacter(":", ";", Keys.OemSemicolon));
            _alphabet.Add(new InputCharacter("\"", "'", "¸", Keys.OemQuotes));
            _alphabet.Add(new InputCharacter("<", ",", "«", Keys.OemComma));
            _alphabet.Add(new InputCharacter(">", ".", "»", Keys.OemPeriod));
            _alphabet.Add(new InputCharacter("?", "/", "¿", Keys.OemQuestion));  */
        }

        #endregion

        public override void Update(GameTime gameTime)
        {
            _lastKeyboardState = _keyboardState;
            _keyboardState = Keyboard.GetState();

            _lastMouseState = _mouseState;
            _mouseState = Mouse.GetState();

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
            if (button == Buttons.LeftTrigger)  return _gamePadState.IsButtonDown(button) || TriggerDown(left: true);
            if (button == Buttons.RightTrigger) return _gamePadState.IsButtonDown(button) || TriggerDown(left: false);
            return _gamePadState.IsButtonDown(button);
        }

        public static bool LastGamePadDown(Buttons button)
        {
            if (button == Buttons.LeftTrigger)  return _lastGamePadState.IsButtonDown(button) || TriggerDownLast(left: true);
            if (button == Buttons.RightTrigger) return _lastGamePadState.IsButtonDown(button) || TriggerDownLast(left: false);
            return _lastGamePadState.IsButtonDown(button);
        }

        public static bool GamePadPressed(Buttons button)
        {
            if (button == Buttons.LeftTrigger)
                return (TriggerDown(true) && !TriggerDownLast(true)) || (_gamePadState.IsButtonDown(button) && _lastGamePadState.IsButtonUp(button));

            if (button == Buttons.RightTrigger)
                return (TriggerDown(false) && !TriggerDownLast(false)) || (_gamePadState.IsButtonDown(button) && _lastGamePadState.IsButtonUp(button));

            return _gamePadState.IsButtonDown(button) && _lastGamePadState.IsButtonUp(button);
        }

        public static bool GamePadReleased(Buttons button)
        {
            if (button == Buttons.LeftTrigger)
                return (!TriggerDown(true) && TriggerDownLast(true)) || (_gamePadState.IsButtonUp(button) && _lastGamePadState.IsButtonDown(button));

            if (button == Buttons.RightTrigger)
                return (!TriggerDown(false) && TriggerDownLast(false)) || (_gamePadState.IsButtonUp(button) && _lastGamePadState.IsButtonDown(button));

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
            return _mouseState.Position;
        }
        public static Point LastMousePosition()
        {
            return _lastMouseState.Position;
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
