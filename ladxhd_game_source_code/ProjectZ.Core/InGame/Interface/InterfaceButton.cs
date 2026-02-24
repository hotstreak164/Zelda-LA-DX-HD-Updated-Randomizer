using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZ.InGame.Controls;
using ProjectZ.InGame.Pages;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.Interface
{
    public class InterfaceButton : InterfaceElement
    {
        public InterfaceElement InsideElement;
        public InterfaceLabel InsideLabel;

        public delegate void BFunction(InterfaceElement element);
        public BFunction ClickFunction;

        public InterfaceButton()
        {
            Selectable = true;
            Color = Values.MenuButtonColor;
            SelectionColor = Values.MenuButtonColorSelected;
        }

        public InterfaceButton(Point size, Point margin, InterfaceElement insideElement, BFunction clickFunction) : this()
        {
            Size = size;
            Margin = margin;
            InsideElement = insideElement;
            ClickFunction = clickFunction;
        }

        public InterfaceButton(Point size, Point margin, string text, BFunction clickFunction) : this()
        {
            Size = size;
            Margin = margin;

            // By setting this to a public field we can easily access it's internals directly.
            InsideLabel = new InterfaceLabel(text, size, Point.Zero);

            InsideElement = InsideLabel;
            ClickFunction = clickFunction;
        }

        public override void Select(Directions direction, bool animate)
        {
            InsideElement?.Select(direction, animate);

            base.Select(direction, animate);
        }

        public override void Deselect(bool animate)
        {
            InsideElement?.Deselect(animate);

            base.Deselect(animate);
        }

        public override InputEventReturn PressedButton(CButtons pressedButton)
        {
            // HACK: Allow pressing "Start" to select a save file and only save files.
            var currentPage = Game1.UiPageManager.GetCurrentPage();
            var buttonText = InsideLabel?.Text;

            // The hack should only be applied to the main menu page so check for that.
            if (currentPage != null && currentPage.GetType() == typeof(MainMenuPage))
            {
                // Only allow Start to select the save file and not the other buttons.
                if (pressedButton == CButtons.Start && (buttonText == "Settings" || buttonText == "Quit"))
                    return InputEventReturn.Nothing;

                // All that remains is the save file selection.
                if (pressedButton != CButtons.Start && pressedButton != ControlHandler.ConfirmButton)
                    return InputEventReturn.Nothing;
            }
            // In every other case only allow the confirm button to click a button.
            else if (pressedButton != ControlHandler.ConfirmButton)
                return InputEventReturn.Nothing;

            // A function has been assigned to the button.
            if (ClickFunction != null)
            {
                // Play a sound effect in most cases, except when it's these buttons.
                if (buttonText != "Back" && buttonText != "Return to Game")
                    Game1.GameManager.PlaySoundEffect("D360-19-13");

                // Run the click function and return input did something.
                ClickFunction(this);
                return InputEventReturn.Something;
            }
            // Since this is constantly checked in a loop, return nothing most of the time.
            return InputEventReturn.Nothing;
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 drawPosition, float scale, float transparency)
        {
            base.Draw(spriteBatch, drawPosition, scale, transparency);

            // draw the embedded element
            InsideElement?.Draw(spriteBatch, drawPosition, scale, transparency);
        }
    }
}