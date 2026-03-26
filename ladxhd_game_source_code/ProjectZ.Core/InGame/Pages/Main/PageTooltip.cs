using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.Interface
{
    public static class PageTooltip
    {
        // Sprite font texture.
        private static SpriteFont Font => Resources.GameFont;

        // Text padding.
        static float paddingX = 10f * Game1.UiScale;
        static float paddingY = 10f * Game1.UiScale;

        // Textbox scale: relative to menu size.
        static float widthScale  = 0.85f;

        // Textbox background.
        static Color backgroundColor = Color.Black;
        static float backgroundAlpha = 0.95f;

        // Textbox border.
        static Color borderColor = Color.White;
        static float borderAlpha = 0.95f;
        static int borderThickness = Game1.UiScale == 1 ? 1 : 2 * Game1.UiScale;

        public static void Draw(SpriteBatch spriteBatch, string text)
        {
            // Update these as the scale may change.
            paddingX = 10f * Game1.UiScale;
            paddingY = 10f * Game1.UiScale;
            widthScale  = 0.85f;
            borderThickness = Game1.UiScale == 1 ? 1 : 2 * Game1.UiScale;

            // Menu size reference.
            float menuWidth = (Values.MinWidth - 32) * Game1.UiScale;
        #if ANDROID
            float menuHeight = (Values.MinHeight - 16) * Game1.UiScale;
        #else
            float menuHeight = (Values.MinHeight - 32) * Game1.UiScale;
        #endif
            // Menu top-left position.
            float menuX = (Game1.WindowWidth - menuWidth) / 2f;
            float menuY = (Game1.WindowHeight - menuHeight) / 2f;

            // Calculate the width of the tooltip.
            float boxWidth = menuWidth * widthScale;

            // Word-wrap text and apply padding.
            var wrappedLines = WrapText(text, boxWidth - paddingX * 2);
            float lineHeight = GameFS.LineSpacing * Game1.UiScale;

            // Different scales make padding look different.
            float extraPadding = Game1.UiScale == 1 ? paddingY : paddingY * 2;
            float reduceYPadding = 0;

            // If Chinese is selected, we need slightly more spacing between the lines.
            if (Game1.LanguageManager.CurrentLanguageCode == "chn")
            {
                lineHeight *= 1.25f;
                reduceYPadding = -(4 * Game1.UiScale);
            }
            // Set the height of the text block.
            float textBlockHeight = wrappedLines.Count * lineHeight;

            // Dynamically scale the height of the tooltip.
            float boxHeight = textBlockHeight + paddingY + borderThickness * 2;
            float maxBoxHeight = menuHeight * 0.75f;
            boxHeight = MathHelper.Min(boxHeight, maxBoxHeight);

            // Tooltip centered within the menu.
            float boxX = menuX + (menuWidth - boxWidth) / 2f;
            float boxY = menuY + (menuHeight - boxHeight) / 2f;

            // Rectangle for black textbox.
            var boxRect = new Rectangle((int)boxX, (int)boxY, (int)boxWidth, (int)boxHeight);

            // Draw the border around the textbox.
            spriteBatch.Draw(Resources.SprWhite, new Rectangle(boxRect.X - borderThickness, boxRect.Y - borderThickness, boxRect.Width + borderThickness * 2, borderThickness), borderColor * borderAlpha);
            spriteBatch.Draw(Resources.SprWhite, new Rectangle(boxRect.X - borderThickness, boxRect.Bottom, boxRect.Width + borderThickness * 2, borderThickness), borderColor * borderAlpha);
            spriteBatch.Draw(Resources.SprWhite, new Rectangle(boxRect.X - borderThickness, boxRect.Y, borderThickness, boxRect.Height), borderColor * borderAlpha);
            spriteBatch.Draw(Resources.SprWhite, new Rectangle(boxRect.Right, boxRect.Y, borderThickness, boxRect.Height), borderColor * borderAlpha);

            // Draw the black background.
            spriteBatch.Draw(Resources.SprWhite, boxRect, backgroundColor * backgroundAlpha);

            // Starting Y position vertically centered with top/bottom padding/
            float startY = boxRect.Y + paddingY + (boxRect.Height - paddingY * 2 - reduceYPadding - textBlockHeight) / 2f;

            foreach (var line in wrappedLines)
            {
                var lineSize = GameFS.MeasureString(line) * Game1.UiScale;
                float lineX = boxRect.X + paddingX + (boxRect.Width - paddingX * 2 - lineSize.X) / 2f; // center horizontally with padding
                GameFS.DrawString(spriteBatch, line, new Vector2(lineX, startY), Color.White, 0f, Vector2.Zero, Game1.UiScale, SpriteEffects.None, 0f);
                startY += lineHeight;
            }
        }

        private static void FindCrashChars(string text)
        {
            // Split the entire string into a character array.
            var chars = text.ToCharArray();

            // Loop through the character array.
            foreach (char c in chars)
            {
                // Write the current character.
                System.Diagnostics.Debug.WriteLine(c);

                // Try to measure it. If this crashes, the last character printed out is what crashed the game.
                float lineWidth = GameFS.MeasureString(c.ToString()).X * Game1.UiScale;
            }
        }

        private static List<string> WrapText(string text, float maxLineWidth)
        {
            // Debug function to find characters in the tooltip that crash.
            // FindCrashChars(font, text);

            // A list to hold the lines and a reference to the current line.
            List<string> lines = new List<string>();
            string currentLine = "";

            // Check if the current language is Chinese which splits on "characters".
            if (Game1.LanguageManager.CurrentLanguageCode == "chn")
            {
                // Wrap character by character for Chinese font.
                foreach (char c in text)
                {
                    // Test what the line would look like if added and measure it.
                    string testLine = currentLine + c;
                    float lineWidth = GameFS.MeasureString(testLine).X * Game1.UiScale;

                    // If the line with the added character is too wide for the textbox
                    // add the line to the list and start a new line.
                    if (lineWidth > maxLineWidth)
                    {
                        if (!string.IsNullOrEmpty(currentLine))
                            lines.Add(currentLine);

                        currentLine = c.ToString();
                    }
                    // If the character fits add it to the current line.
                    else
                    {
                        currentLine = testLine;
                    }
                }
            }
            // For all other languages we use the code below which splits on "spaces".
            else
            {
                // Split text into words.
                string[] words = text.Split(' ');

                // Loop through each word and decide if it fits in the current line.
                foreach (string word in words)
                {
                    // Test what the line would look like if added and measure it.
                    string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                    float lineWidth = GameFS.MeasureString(testLine).X * Game1.UiScale;

                    // If the line with the added word is too wide for the textbox
                    // add the line to the list and start a new line.
                    if (lineWidth > maxLineWidth)
                    {
                        if (!string.IsNullOrEmpty(currentLine))
                            lines.Add(currentLine);

                        currentLine = word;
                    }
                    // If the word fits add it to the current line.
                    else
                        currentLine = testLine;
                }
            }
            // Add remaining text to the final line.
            if (!string.IsNullOrEmpty(currentLine))
                lines.Add(currentLine);

            // Return the list of lines to display on the tooltip.
            return lines;
        }
    }
}
