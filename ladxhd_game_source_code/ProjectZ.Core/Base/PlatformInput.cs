using System.Threading;
using ProjectZ.InGame.Controls;
using Microsoft.Xna.Framework;

namespace ProjectZ.Base
{
    public static class PlatformInput
    {
        // Legacy latch kept for backward compatibility with ConsumeSelectPressed().
        public static volatile bool SelectPressed;

        private static volatile int _keyEventDown;
        private static volatile int _keyEventLast;
        private static volatile float _keyEventRightStickX;
        private static volatile float _keyEventRightStickY;

        public static Vector2 KeyEventRightStick => new Vector2(_keyEventRightStickX, _keyEventRightStickY);
        public static bool KeyEventButtonDown(CButtons button) => (_keyEventDown & (int)button) != 0;
        public static bool KeyEventButtonPressed(CButtons button) => (_keyEventDown & (int)button) != 0 && (_keyEventLast & (int)button) == 0;
        public static bool KeyEventButtonReleased(CButtons button) => (_keyEventDown & (int)button) == 0 && (_keyEventLast & (int)button) != 0;

        public static bool ConsumeSelectPressed()
        {
            if (!SelectPressed) return false;
            SelectPressed = false;
            return true;
        }

        public static void BeginFrame()
        {
            _keyEventLast = _keyEventDown;
        }

        public static void SetKeyEventButton(CButtons button, bool down)
        {
            if (down)
                Interlocked.Or(ref _keyEventDown, (int)button);
            else
                Interlocked.And(ref _keyEventDown, ~(int)button);
        }

        public static void SetKeyEventRightStick(float x, float y)
        {
            _keyEventRightStickX = x;
            _keyEventRightStickY = y;
        }
    }
}