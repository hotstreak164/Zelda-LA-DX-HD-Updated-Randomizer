using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectZ.InGame.Controls;
using ProjectZ.InGame.Interface;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.Pages
{
    class SwordInteractPage : InterfacePage
    {
        private readonly InterfaceListLayout _swordSettingsList;
        private readonly InterfaceListLayout _contentLayout;
        private readonly InterfaceListLayout _bottomBar;
        private bool _showTooltip;

        public SwordInteractPage(int width, int height)
        {
            EnableTooltips = true;

            // Game Settings Layout
            _swordSettingsList = new InterfaceListLayout { Size = new Point(width, height - 12), Selectable = true };

            var buttonWidth = 320;
            var buttonHeight = 15;

            _swordSettingsList.AddElement(new InterfaceLabel(Resources.GameHeaderFont, "settings_sword_header",
                new Point(buttonWidth, (int)(height * Values.MenuHeaderSize)), new Point(0, 0)));
            _contentLayout = new InterfaceListLayout { Size = new Point(width, (int)(height * Values.MenuContentSize) - 12), Selectable = true, ContentAlignment = InterfaceElement.Gravities.Top };

            // Button: Collect Items
            var toggleSwordGrabNormal = InterfaceToggle.GetToggleButton(new Point(buttonWidth, buttonHeight), new Point(5, 2),
                "settings_sword_grabnormal", GameSettings.SwGrabNormal, 
                newState => { GameSettings.SwGrabNormal = newState; });
            _contentLayout.AddElement(toggleSwordGrabNormal);

            // Button: Collect Static Items
            var toggleSwordGrabStatic = InterfaceToggle.GetToggleButton(new Point(buttonWidth, buttonHeight), new Point(5, 2),
                "settings_sword_grabworlditem", GameSettings.SwGrabWorldItem, 
                newState => { GameSettings.SwGrabWorldItem = newState; });
            _contentLayout.AddElement(toggleSwordGrabStatic);

            // Button: Collect Fairies
            var toggleSwordGrabFairy = InterfaceToggle.GetToggleButton(new Point(buttonWidth, buttonHeight), new Point(5, 2),
                "settings_sword_grabfairy", GameSettings.SwGrabFairy, 
                newState => { GameSettings.SwGrabFairy = newState; });
            _contentLayout.AddElement(toggleSwordGrabFairy);

            // Button: Collect Small Keys
            var toggleSwordGrabKeys = InterfaceToggle.GetToggleButton(new Point(buttonWidth, buttonHeight), new Point(5, 2),
                "settings_sword_grabsmallkeys", GameSettings.SwGrabSmallKey, 
                newState => { GameSettings.SwGrabSmallKey = newState; });
            _contentLayout.AddElement(toggleSwordGrabKeys);

            // Button: Ricochet Items
            var toggleRicochetItems = InterfaceToggle.GetToggleButton(new Point(buttonWidth, buttonHeight), new Point(5, 2),
                "settings_sword_itemsmack", GameSettings.SwItemSmack, 
                newState => { GameSettings.SwItemSmack = newState; });
            _contentLayout.AddElement(toggleRicochetItems);

            // Button: Sword Block Projectiles
            var toggleSwordBlock = InterfaceToggle.GetToggleButton(new Point(buttonWidth, buttonHeight), new Point(5, 2),
                "settings_sword_swordblock", GameSettings.SwMissileBlock, 
                newState => { GameSettings.SwMissileBlock = newState; });
            _contentLayout.AddElement(toggleSwordBlock);

            // Button: Smash Pots & Skulls
            var toggleSmashPots = InterfaceToggle.GetToggleButton(new Point(buttonWidth, buttonHeight), new Point(5, 2),
                "settings_sword_breakpots", GameSettings.SwBreakPots, 
                newState => { GameSettings.SwBreakPots = newState; });
            _contentLayout.AddElement(toggleSmashPots);

            // Button: Beam Cuts Grass & Bushes
            var toggleBeamShrubs = InterfaceToggle.GetToggleButton(new Point(buttonWidth, buttonHeight), new Point(5, 2),
                "settings_sword_beamshrubs", GameSettings.SwBeamShrubs, 
                newState => { GameSettings.SwBeamShrubs = newState; });
            _contentLayout.AddElement(toggleBeamShrubs);

            // Bottom Bar / Back Button:
            _bottomBar = new InterfaceListLayout() { Size = new Point(width, (int)(height * Values.MenuFooterSize)), Selectable = true, HorizontalMode = true };
            _bottomBar.AddElement(new InterfaceButton(new Point(100, 18), new Point(2, 4), "settings_menu_back", element => { SwordPageBackButton(); }));
            _swordSettingsList.AddElement(_contentLayout);
            _swordSettingsList.AddElement(_bottomBar);
            PageLayout = _swordSettingsList;
        }

        public override void Update(CButtons pressedButtons, GameTime gameTime)
        {
            base.Update(pressedButtons, gameTime);

            // The back button was pressed.
            if (ControlHandler.ButtonPressed(ControlHandler.CancelButton))
                SwordPageBackButton();
            
            // The tooltip button was pressed.
            if (ControlHandler.ButtonPressed(CButtons.Y))
            {
                _showTooltip = !_showTooltip;
                if (_showTooltip)
                    Game1.GameManager.PlaySoundEffect("D360-21-15");
            }
            // Hide the tooltip when pressing anything.
            else if (ControlHandler.AnyButtonPressed())
                _showTooltip = false;
        }

        private void SwordPageBackButton()
        {
            Game1.GameManager.ItemManager.Load(); 
            Game1.UiPageManager.PopPage();
        }

        public override void OnLoad(Dictionary<string, object> intent)
        {
            // The left button is always the first one selected.
            _bottomBar.Deselect(false);
            _bottomBar.Select(InterfaceElement.Directions.Left, false);
            _bottomBar.Deselect(false);

            PageLayout.Deselect(false);
            PageLayout.Select(InterfaceElement.Directions.Top, false);
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 position, int height, float alpha)
        {
            // Always draw the menu even when not showing tooltips.
            base.Draw(spriteBatch, position, height, alpha);

            // If the user pressed the top most face button, show the tooltip window.
            if (_showTooltip)
            {
                string tooltipText = GetOptionToolip();
                PageTooltip.Draw(spriteBatch, tooltipText);
            }
        }

        private string GetOptionToolip()
        {
            // Detect back button press by checking the index of the main InterfaceListLayout.
            if (_swordSettingsList.SelectionIndex == 2)
                return  Game1.LanguageManager.GetString("tooltip_default", "error");

            // Detect the chosen button by checking the content InterfaceListLayout.
            int index = _contentLayout.SelectionIndex;
            string tooltip = "Select an option to view its tooltip.";

            // Use the selected index to determine which tooltip to show.
            switch (index) 
            {
                case 0:  { tooltip = Game1.LanguageManager.GetString("tooltip_sword_grabnormal", "error"); break; }
                case 1:  { tooltip = Game1.LanguageManager.GetString("tooltip_sword_grabworlditem", "error"); break; }
                case 2:  { tooltip = Game1.LanguageManager.GetString("tooltip_sword_grabfairy", "error"); break; }
                case 3:  { tooltip = Game1.LanguageManager.GetString("tooltip_sword_grabsmallkeys", "error"); break; }
                case 4:  { tooltip = Game1.LanguageManager.GetString("tooltip_sword_itemsmack", "error"); break; }
                case 5:  { tooltip = Game1.LanguageManager.GetString("tooltip_sword_swordblock", "error"); break; }
                case 6:  { tooltip = Game1.LanguageManager.GetString("tooltip_sword_breakpots", "error"); break; }
                case 7:  { tooltip = Game1.LanguageManager.GetString("tooltip_sword_beamshrubs", "error"); break; }
            }
            // Display the tooltip in the tooltip window.
            return tooltip;
        }
    }
}
