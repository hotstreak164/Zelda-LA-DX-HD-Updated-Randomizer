using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZ.Core.InGame.Pages;
using ProjectZ.InGame.Controls;
using ProjectZ.InGame.Interface;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.Pages
{
    class SettingsPage : InterfacePage
    {
        private readonly InterfaceListLayout _settingsLayout;
        private readonly InterfaceListLayout _contentLayout;
        private readonly InterfaceListLayout _bottomBar;
        private readonly InterfaceLabel _versionLabel;
        private bool _showTooltip;

        public SettingsPage(int width, int height)
        {
            EnableTooltips = true;

            // Settings Page Layout
            _settingsLayout = new InterfaceListLayout { Size = new Point(width, height - 12), Selectable = true };
            var headerLayout = new InterfaceListLayout { Size = new Point(width, (int)(height * Values.MenuHeaderSize)), ContentAlignment = InterfaceElement.Gravities.Left, HorizontalMode = true };
            {
                _versionLabel = new InterfaceLabel("", new Point((width - 150) / 2 - 2, headerLayout.Size.Y-22), new Point(5, 0)) { Translate = false, TextAlignment = InterfaceElement.Gravities.Left | InterfaceElement.Gravities.Top };
                _versionLabel.SetText(Values.VersionString);
                headerLayout.AddElement(_versionLabel);
                headerLayout.AddElement(new InterfaceLabel(Resources.GameHeaderFont, "settings_menu_header", new Point(150, (int)(height * Values.MenuHeaderSize)), new Point(-8, 0)));
            }
            _settingsLayout.AddElement(headerLayout);
            _contentLayout = new InterfaceListLayout { Size = new Point(width, (int)(height * Values.MenuContentSize) - 12), Selectable = true };
            var buttonSize = new Point(150, 14);

            // Button: Game Settings
            _contentLayout.AddElement(new InterfaceButton(buttonSize, new Point(1, 2), "settings_menu_game", element => { Game1.UiPageManager.ChangePage(typeof(GameSettingsPage)); }));

            // Button: Redux Settings
            _contentLayout.AddElement(new InterfaceButton(buttonSize, new Point(1, 2), "settings_menu_redux", element => { Game1.UiPageManager.ChangePage(typeof(ReduxOptionsPage)); }));

            // Button: Camera Settings
            _contentLayout.AddElement(new InterfaceButton(buttonSize, new Point(1, 2), "settings_menu_camera", element => { Game1.UiPageManager.ChangePage(typeof(CameraSettingsPage)); }));

            // Button: Video Settings
            _contentLayout.AddElement(new InterfaceButton(buttonSize, new Point(1, 2), "settings_menu_video", element => { Game1.UiPageManager.ChangePage(typeof(VideoSettingsPage)); }));

            // Button: Graphics Settings
            _contentLayout.AddElement(new InterfaceButton(buttonSize, new Point(1, 2), "settings_menu_graphics", element => { Game1.UiPageManager.ChangePage(typeof(GraphicsSettingsPage)); }));

            // Button: Audio Settings
            _contentLayout.AddElement(new InterfaceButton(buttonSize, new Point(1, 2), "settings_menu_audio", element => { Game1.UiPageManager.ChangePage(typeof(AudioSettingsPage)); }));

            // Button: Control Settings
            _contentLayout.AddElement(new InterfaceButton(buttonSize, new Point(1, 2), "settings_menu_controls", element => { Game1.UiPageManager.ChangePage(typeof(ControlSettingsPage)); }));

            // Button: Modifier Settings
            _contentLayout.AddElement(new InterfaceButton(buttonSize, new Point(1, 2), "settings_menu_mods", element => { Game1.UiPageManager.ChangePage(typeof(ModifiersPage)); }));

            // Button: Modifier Settings
            _contentLayout.AddElement(new InterfaceButton(buttonSize, new Point(1, 2), "settings_menu_presets", element => { Game1.UiPageManager.ChangePage(typeof(PresetOptionsPage)); }));

            // Bottom Bar / Exit Button:
            _bottomBar = new InterfaceListLayout { Size = new Point(width, (int)(height * Values.MenuFooterSize)), Selectable = true };
            _bottomBar.AddElement(new InterfaceButton(new Point(100, 18), new Point(2, 4), "settings_menu_back", element => { ExitPage(); }));
            _settingsLayout.AddElement(_contentLayout);
            _settingsLayout.AddElement(_bottomBar);
            PageLayout = _settingsLayout;
        }

        public override void Update(CButtons pressedButtons, GameTime gameTime)
        {
            base.Update(pressedButtons, gameTime);

            // The back button was pressed.
            if (ControlHandler.ButtonPressed(ControlHandler.CancelButton))
                ExitPage();

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

        public override void OnLoad(Dictionary<string, object> intent)
        {
            PageLayout.Deselect(false);
            PageLayout.Select(InterfaceElement.Directions.Top, false);

            // only show the version in the main menu
            if(Game1.ScreenManager.CurrentScreenId == Values.ScreenNameGame)
                _versionLabel.TextColor = Color.Transparent;
            else
                _versionLabel.TextColor = Color.White;
        }

        private void ExitPage()
        {
            // save the new settings
            SettingsSaveLoad.SaveSettings();

            Game1.UiPageManager.PopPage();
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
            if (_settingsLayout.SelectionIndex == 2)
                return  Game1.LanguageManager.GetString("tooltip_default", "error");

            // Detect the chosen button by checking the content InterfaceListLayout.
            int index = _contentLayout.SelectionIndex;
            string tooltip = "Select an option to view its tooltip.";

            // Use the selected index to determine which tooltip to show.
            switch (index) 
            {
                case 0:  { tooltip = Game1.LanguageManager.GetString("tooltip_menu_game", "error"); break; }
                case 1:  { tooltip = Game1.LanguageManager.GetString("tooltip_menu_redux", "error"); break; }
                case 2:  { tooltip = Game1.LanguageManager.GetString("tooltip_menu_camera", "error"); break; }
                case 3:  { tooltip = Game1.LanguageManager.GetString("tooltip_menu_video", "error"); break; }
                case 4:  { tooltip = Game1.LanguageManager.GetString("tooltip_menu_audio", "error"); break; }
                case 5:  { tooltip = Game1.LanguageManager.GetString("tooltip_menu_controls", "error"); break; }
                case 6:  { tooltip = Game1.LanguageManager.GetString("tooltip_menu_mods", "error"); break; }
                case 7:  { tooltip = Game1.LanguageManager.GetString("tooltip_menu_presets", "error"); break; }
            }
            // Display the tooltip in the tooltip window.
            return tooltip;
        }

    }
}
