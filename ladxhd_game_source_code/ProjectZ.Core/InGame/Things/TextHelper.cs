using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace ProjectZ.InGame.Things
{
    internal class TextHelper
    {
        public static int LineSpacing => Game1.LanguageManager.CurrentLanguageCode == "chn" 
            ? Resources.ChinaFont.LineHeight 
            : Resources.GameFont.LineSpacing;

        public static void DrawString(SpriteBatch spriteBatch, string text, Vector2 position, Color color)
        {
            if (Game1.LanguageManager.CurrentLanguageCode == "chn")
                spriteBatch.DrawString(Resources.ChinaFont, text, position, color);
            else
                spriteBatch.DrawString(Resources.GameFont, text, position, color);
        }

        public static void DrawString(SpriteBatch spriteBatch, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            if (Game1.LanguageManager.CurrentLanguageCode == "chn")
                spriteBatch.DrawString(Resources.ChinaFont, text, position, color, rotation, origin, new Vector2(scale, scale), effects, layerDepth);
            else
                spriteBatch.DrawString(Resources.GameFont, text, position, color, rotation, origin, scale, effects, layerDepth);
        }

        public static Vector2 MeasureString(string text)
        {
            if (Game1.LanguageManager.CurrentLanguageCode == "chn")
                return Resources.ChinaFont.MeasureString(text);
            return Resources.GameFont.MeasureString(text);
        }
    }
}