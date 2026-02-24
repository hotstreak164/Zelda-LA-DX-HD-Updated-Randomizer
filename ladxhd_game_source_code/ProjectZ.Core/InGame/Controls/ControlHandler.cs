using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ProjectZ.Base;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.Controls
{
    public class ControlHandler
    {
        public static Dictionary<CButtons, ButtonMapper> ButtonDictionary = new Dictionary<CButtons, ButtonMapper>();

        public static CButtons DebugButtons;
        public static CButtons ConfirmButton = CButtons.A;
        public static CButtons CancelButton  = CButtons.B;

        public static bool LastKeyboardDown;
        public static bool LastDirectionDPad;

        private const int ScrollStartTime = 500;
        private const int ScrollTime = 125;

        private static float _scrollCounter;
        private static bool _initDirection;

        public static string[] ControllerNames;
        public static string[,] ControllerLabels;
        public static int ControllerIndex = 0;

        public static void Initialize()
        {
            ResetControls();

            // A simple array to reference the names by index.
            ControllerNames = new string[]{ "XBox", "Nintendo", "Playstation" };

            // A rectangular 2D array is used to easily reference the button labels.
            ControllerLabels = new string[,] 
            { 
                { "A", "B", "X", "Y", "LB", "RB", "LT", "RT",   "Back",   "Start" },  // XBox
                { "B", "A", "Y", "X",  "L",  "R", "ZL", "ZR", "Select",   "Start" },  // Nintendo
                { "Χ", "Ο", "Γ", "Δ", "L1", "R1", "L2", "R2",  "Share", "Options" }   // Playstation
            }; 
        }

        public static void SetControllerIndex()
        {
            // Controller index is set when loading a save file or when switching a controller.
            ControllerIndex = Array.IndexOf(ControllerNames, GameSettings.Controller);
        }

        private static readonly Dictionary<string, int> ButtonIndexMap = new()
        {
            // This serves as a lookup table to translate the MonoGame button name to the 
            // selected controller. The value references the button position in the 2D array.
            ["A"]             = 0,
            ["B"]             = 1,
            ["X"]             = 2,
            ["Y"]             = 3,
            ["LeftShoulder"]  = 4,
            ["RightShoulder"] = 5,
            ["LeftTrigger"]   = 6,
            ["RightTrigger"]  = 7,
            ["Back"]          = 8,
            ["Start"]         = 9
        };
        
        public static string GetButtonName(Buttons button)
        {
            // This method should be used anywhere a button name is displayed in-game.
            string buttonName = button.ToString();

            return ButtonIndexMap.TryGetValue(buttonName, out int index)
                ? ControllerLabels[ControllerIndex, index]
                : buttonName;
        }

        public static void ResetControls()
        {
            ButtonDictionary.Clear();
            ButtonDictionary.Add(CButtons.Up, new ButtonMapper(new[] { Keys.Up }, new[] { Buttons.DPadUp }));
            ButtonDictionary.Add(CButtons.Down, new ButtonMapper(new[] { Keys.Down }, new[] { Buttons.DPadDown }));
            ButtonDictionary.Add(CButtons.Left, new ButtonMapper(new[] { Keys.Left }, new[] { Buttons.DPadLeft }));
            ButtonDictionary.Add(CButtons.Right, new ButtonMapper(new[] { Keys.Right }, new[] { Buttons.DPadRight }));
            ButtonDictionary.Add(CButtons.A, new ButtonMapper(new[] { Keys.S }, new[] { Buttons.A }));
            ButtonDictionary.Add(CButtons.B, new ButtonMapper(new[] { Keys.D }, new[] { Buttons.B }));
            ButtonDictionary.Add(CButtons.X, new ButtonMapper(new[] { Keys.A }, new[] { Buttons.X }));
            ButtonDictionary.Add(CButtons.Y, new ButtonMapper(new[] { Keys.W }, new[] { Buttons.Y }));
            ButtonDictionary.Add(CButtons.LB, new ButtonMapper(new[] { Keys.OemMinus }, new[] { Buttons.LeftShoulder }));
            ButtonDictionary.Add(CButtons.RB, new ButtonMapper(new[] { Keys.OemPlus }, new[] { Buttons.RightShoulder }));
            ButtonDictionary.Add(CButtons.LT, new ButtonMapper(new[] { Keys.OemOpenBrackets }, new[] { Buttons.LeftTrigger }));
            ButtonDictionary.Add(CButtons.RT, new ButtonMapper(new[] { Keys.OemCloseBrackets }, new[] { Buttons.RightTrigger }));
            ButtonDictionary.Add(CButtons.Select, new ButtonMapper(new[] { Keys.Space }, new[] { Buttons.Back }));
            ButtonDictionary.Add(CButtons.Start, new ButtonMapper(new[] { Keys.Enter }, new[] { Buttons.Start }));
            ButtonDictionary.Add(CButtons.LS, new ButtonMapper(new[] { Keys.Q }, new[] { Buttons.LeftStick }));
            ButtonDictionary.Add(CButtons.RS, new ButtonMapper(new[] { Keys.W }, new[] { Buttons.RightStick }));
            SetConfirmCancelButtons();
        }

        public static void SaveButtonMaps(SaveManager saveManager)
        {
            // load the input settings
            foreach (var buttonMap in ButtonDictionary)
            {
                // save the keyboard buttons
                for (var i = 0; i < buttonMap.Value.Keys.Length; i++)
                    saveManager.SetInt("control" + buttonMap.Key + "key" + i, (int)buttonMap.Value.Keys[i]);
                // save the gamepad buttons
                for (var i = 0; i < buttonMap.Value.Buttons.Length; i++)
                    saveManager.SetInt("control" + buttonMap.Key + "button" + i, (int)buttonMap.Value.Buttons[i]);
            }
        }

        public static void LoadButtonMap(SaveManager saveManager)
        {
            // load the input settings
            foreach (var buttonMap in ButtonDictionary)
            {
                // load the keyboard button
                var index = 0;
                int key;
                var keys = new List<Keys>();
                while ((key = saveManager.GetInt("control" + buttonMap.Key + "key" + index, -1)) >= 0)
                {
                    keys.Add((Keys)key);
                    index++;
                }

                // set the loaded keys
                if (keys.Count > 0)
                    buttonMap.Value.Keys = keys.ToArray();

                // load the gamepad button
                index = 0;
                int button;
                var gamepadButtons = new List<Buttons>();
                while ((button = saveManager.GetInt("control" + buttonMap.Key + "button" + index, -1)) >= 0)
                {
                    gamepadButtons.Add((Buttons)button);
                    index++;
                }

                // set the loaded buttons
                if (gamepadButtons.Count > 0)
                    buttonMap.Value.Buttons = gamepadButtons.ToArray();
            }
            SetConfirmCancelButtons();
        }

        public static void SetConfirmCancelButtons()
        {
            if (GameSettings.SwapButtons)
            {
                ConfirmButton = CButtons.B;
                CancelButton  = CButtons.A;
            }
            else
            {
                ConfirmButton = CButtons.A;
                CancelButton  = CButtons.B;
            }
        }

        public static Vector2 GetCamVector2()
        {
            var gamepadState = GamePad.GetState(PlayerIndex.One);
            var vec = new Vector2(gamepadState.ThumbSticks.Right.X, -gamepadState.ThumbSticks.Right.Y);
            return vec;
        }

        public static void Update()
        {
            if (_scrollCounter < 0)
                _scrollCounter += ScrollTime;

            _initDirection = _scrollCounter == ScrollStartTime;

            var direction = GetMoveVector2();

            if (direction.Length() != 0)
                _scrollCounter -= Game1.DeltaTime;
            else
                _scrollCounter = ScrollStartTime;

            foreach (var button in ButtonDictionary)
                for (var i = 0; i < button.Value.Keys.Length; i++)
                    if (InputHandler.LastKeyDown(button.Value.Keys[i]))
                        LastKeyboardDown = true;

            foreach (var button in ButtonDictionary)
                for (var i = 0; i < button.Value.Buttons.Length; i++)
                    if (InputHandler.LastGamePadDown(button.Value.Buttons[i]))
                        LastKeyboardDown = false;

            DebugButtons = CButtons.None;
        }

        private static Vector2 Digitalize(Vector2 vec)
        {
            float ax = Math.Abs(vec.X);
            float ay = Math.Abs(vec.Y);

            float threshold = 0.35f;

            if (ax > ay)
            {
                if (ay / ax >= threshold)
                    return new Vector2(Math.Sign(vec.X), Math.Sign(vec.Y));
                return new Vector2(Math.Sign(vec.X), 0);
            }
            else
            {
                if (ax / ay >= threshold)
                    return new Vector2(Math.Sign(vec.X), Math.Sign(vec.Y));
                return new Vector2(0, Math.Sign(vec.Y));
            }
        }

        public static Vector2 GetMoveVector2()
        {
            var gamepadState = GamePad.GetState(PlayerIndex.One);

            Vector2 vec = new Vector2(gamepadState.ThumbSticks.Left.X, -gamepadState.ThumbSticks.Left.Y);

            if (Math.Abs(vec.X) <= GameSettings.DeadZone && Math.Abs(vec.Y) <= GameSettings.DeadZone)
                vec = Vector2.Zero;
            else if (GameSettings.DigitalAnalog)
                vec = Digitalize(vec);

            if (vec == Vector2.Zero)
            {
                if (ButtonDown(CButtons.Left))
                    vec.X = -1;
                else if (ButtonDown(CButtons.Right))
                    vec.X = 1;

                if (ButtonDown(CButtons.Up))
                    vec.Y = -1;
                else if (ButtonDown(CButtons.Down))
                    vec.Y = 1;
            }
            return vec;
        }

        public static Vector2 GetAnalogDirection()
        {
            var gamepadState = GamePad.GetState(PlayerIndex.One);

            var vec = new Vector2(gamepadState.ThumbSticks.Left.X, -gamepadState.ThumbSticks.Left.Y);

            if (Math.Abs(vec.X) <= GameSettings.DeadZone && Math.Abs(vec.Y) <= GameSettings.DeadZone)
                vec = Vector2.Zero;

            return vec;
        }

        public static bool LastButtonDown(CButtons button)
        {
            // check the keyboard buttons
            for (var i = 0; i < ButtonDictionary[button].Keys.Length; i++)
                if (InputHandler.LastKeyDown(ButtonDictionary[button].Keys[i]))
                    return true;

            // check the gamepad buttons
            for (var i = 0; i < ButtonDictionary[button].Buttons.Length; i++)
                if (InputHandler.LastGamePadDown(ButtonDictionary[button].Buttons[i]))
                    return true;

            return false;
        }

        public static bool ButtonDown(CButtons button)
        {
            var direction = GetAnalogDirection();

            if (_initDirection && direction != Vector2.Zero)
            {
                var dir = AnimationHelper.GetDirection(direction);
                if ((dir == 0 && button == CButtons.Left) || (dir == 1 && button == CButtons.Up) ||
                    (dir == 2 && button == CButtons.Right) || (dir == 3 && button == CButtons.Down))
                    return true;
            }

            // check the keyboard buttons
            for (var i = 0; i < ButtonDictionary[button].Keys.Length; i++)
                if (InputHandler.KeyDown(ButtonDictionary[button].Keys[i]))
                    return true;

            // check the gamepad buttons
            for (var i = 0; i < ButtonDictionary[button].Buttons.Length; i++)
                if (InputHandler.GamePadDown(ButtonDictionary[button].Buttons[i]))
                    return true;

            return false;
        }

        public static bool ButtonPressed(CButtons button, bool controllerOnly = false)
        {
            var direction = GetAnalogDirection();

            if (_initDirection && direction != Vector2.Zero)
            {
                var dir = AnimationHelper.GetDirection(direction);
                if ((dir == 0 && button == CButtons.Left) || (dir == 1 && button == CButtons.Up) ||
                    (dir == 2 && button == CButtons.Right) || (dir == 3 && button == CButtons.Down))
                    return true;
            }

            // check the keyboard buttons
            if (!controllerOnly)
                for (var i = 0; i < ButtonDictionary[button].Keys.Length; i++)
                    if (InputHandler.KeyPressed(ButtonDictionary[button].Keys[i]))
                        return true;

            // check the gamepad buttons
            for (var i = 0; i < ButtonDictionary[button].Buttons.Length; i++)
                if (InputHandler.GamePadPressed(ButtonDictionary[button].Buttons[i]))
                    return true;

            // button presses used by tests
            if ((DebugButtons & button) != 0)
                return true;

            return false;
        }

        public static bool ButtonReleased(CButtons button)
        {
            var direction = GetAnalogDirection();
            if  (direction != Vector2.Zero)
            {
                var dir = AnimationHelper.GetDirection(direction);
                if ((dir == 0 && button == CButtons.Left) || (dir == 1 && button == CButtons.Up) ||
                    (dir == 2 && button == CButtons.Right) || (dir == 3 && button == CButtons.Down))
                    return true;
            }

            // check the keyboard buttons
            for (var i = 0; i < ButtonDictionary[button].Keys.Length; i++)
                if (InputHandler.KeyReleased(ButtonDictionary[button].Keys[i]))
                    return true;

            // check the gamepad buttons
            for (var i = 0; i < ButtonDictionary[button].Buttons.Length; i++)
                if (InputHandler.GamePadReleased(ButtonDictionary[button].Buttons[i]))
                    return true;

            return false;
        }

        public static bool MenuButtonDown(CButtons button)
        {
            var direction = GetAnalogDirection();

            if (direction != Vector2.Zero)
            {
                var dir = AnimationHelper.GetDirection(direction);
                if (((dir == 0 && button == CButtons.Left) || 
                    (dir == 1 && button == CButtons.Up) ||
                    (dir == 2 && button == CButtons.Right) || 
                    (dir == 3 && button == CButtons.Down)) && 
                    (_scrollCounter < 0 || _initDirection))
                    return true;
            }

            return ButtonPressed(button) || (ButtonDown(button) && _scrollCounter < 0);
        }

        public static bool TrendyButtonDown(CButtons button)
        {
            // Check keyboard bindings.
            foreach (var key in ButtonDictionary[button].Keys)
                if (InputHandler.KeyDown(key))
                    return true;

            // Check gamepad bindings.
            if (button == CancelButton)
                return InputHandler.GamePadDown(GameSettings.SwapButtons ? Buttons.A : Buttons.B);

            if (button == ConfirmButton)
                return InputHandler.GamePadDown(GameSettings.SwapButtons ? Buttons.B : Buttons.A);

            return false;
        }

        public static CButtons GetPressedButtons()
        {
            CButtons pressedButtons = 0;

            foreach (var bEntry in ButtonDictionary)
            {
                for (var i = 0; i < bEntry.Value.Keys.Length; i++)
                    if (InputHandler.KeyPressed(bEntry.Value.Keys[i]))
                        pressedButtons |= bEntry.Key;

                // check the gamepad buttons
                for (var i = 0; i < bEntry.Value.Buttons.Length; i++)
                    if (InputHandler.GamePadPressed(bEntry.Value.Buttons[i]))
                        pressedButtons |= bEntry.Key;
            }
            return pressedButtons;
        }

        public static bool AnyButtonPressed()
        {
            // Detect any digital button press.
            foreach (var bEntry in ButtonDictionary)
            {
                for (var i = 0; i < bEntry.Value.Keys.Length; i++)
                    if (InputHandler.KeyPressed(bEntry.Value.Keys[i]))
                        return true;

                for (var i = 0; i < bEntry.Value.Buttons.Length; i++)
                    if (InputHandler.GamePadPressed(bEntry.Value.Buttons[i]))
                        return true;
            }
            // Detect any movement from the analog.
            if (GetMoveVector2() != Vector2.Zero)
                return true;

            // Otherwise nothing was pressed.
            return false;
        }
    }
}