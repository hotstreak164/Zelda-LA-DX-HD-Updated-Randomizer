namespace ProjectZ.Base
{
    public static class PlatformInput
    {
        // Set by platform layer (Android). Read by InputHandler.
        public static volatile bool SelectPressed;

        public static bool ConsumeSelectPressed()
        {
            if (!SelectPressed) return false;
            SelectPressed = false;
            return true;
        }
    }
}