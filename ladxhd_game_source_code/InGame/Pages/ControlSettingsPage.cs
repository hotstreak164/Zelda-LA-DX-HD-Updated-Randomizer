using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectZ.InGame.Controls;
using ProjectZ.InGame.Interface;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.Pages
{
    class ControlSettingsPage : InterfacePage
    {
        private readonly InterfaceListLayout _controlSettingsList;
        private readonly InterfaceListLayout _contentLayout;
        private readonly InterfaceListLayout _bottomBar;
        private readonly InterfaceButton _controllerType;
        private float _controlCooldown = 0f;
        private bool _showTooltip;

        public ControlSettingsPage(int width, int height)
        {
            EnableTooltips = true;

            // Game Settings Layout
            _controlSettingsList = new InterfaceListLayout { Size = new Point(width, height - 12), Selectable = true };

            var buttonWidth = 320;
            var buttonHeight = 16;

            _controlSettingsList.AddElement(new InterfaceLabel(Resources.GameHeaderFont, "settings_controls_header",
                new Point(buttonWidth, (int)(height * Values.MenuHeaderSize)), new Point(0, 0)));
            _contentLayout = new InterfaceListLayout { Size = new Point(width, (int)(height * Values.MenuContentSize) - 12), Selectable = true, ContentAlignment = InterfaceElement.Gravities.Top };

            // Slider: Deadzone
            var sliderDeadzone = new InterfaceSlider(Resources.GameFont, "settings_controls_deadzone", 
                buttonWidth, 11, new Point(1, 2), 0, 100, 1, (int)(GameSettings.DeadZone * 100),
                number => { GameSettings.DeadZone = (float)(number * 0.01); })
                { SetString = number => ": " + number + "%" };
            _contentLayout.AddElement(sliderDeadzone);

            // Button: Controller Type
            _contentLayout.AddElement(_controllerType = new InterfaceButton(new Point(buttonWidth, buttonHeight), new Point(0, 2), "", PressButtonSetController));
            _controllerType.InsideLabel.OverrideText = Game1.LanguageManager.GetString("settings_controls_gamepad", "error") + ": " + GameSettings.Controller;

            // Button: Remap Settings
            _contentLayout.AddElement(new InterfaceButton(new Point(buttonWidth, buttonHeight), new Point(1, 2), 
                "settings_controls_remap", element => { Game1.UiPageManager.ChangePage(typeof(ControlMappingPage)); }));

            // Button: Toggle Six Buttons
            var toggleSixButtons = InterfaceToggle.GetToggleButton(new Point(buttonWidth, buttonHeight), new Point(5, 2),
                "settings_controls_sixbuttons", GameSettings.SixButtons, 
                newState => { UpdateSixButtonToggle(newState); });
            _contentLayout.AddElement(toggleSixButtons);

            // Button: Swap Confirm & Cancel
            var toggleSwapButtons = InterfaceToggle.GetToggleButton(new Point(buttonWidth, buttonHeight), new Point(5, 2),
                "settings_controls_swapbuttons", GameSettings.SwapButtons, 
                newState => { _controlCooldown = 500f; GameSettings.SwapButtons = newState; ControlHandler.SetConfirmCancelButtons(); });
            _contentLayout.AddElement(toggleSwapButtons);

            // Button: Classic Movement
            var toggleOldMovement = InterfaceToggle.GetToggleButton(new Point(buttonWidth, buttonHeight), new Point(5, 2),
                "settings_controls_classicmove", GameSettings.OldMovement, 
                newState => { GameSettings.OldMovement = newState; });
            _contentLayout.AddElement(toggleOldMovement);

            // Button: Digital Analog
            var toggleEightWay = InterfaceToggle.GetToggleButton(new Point(buttonWidth, buttonHeight), new Point(5, 2),
                "settings_controls_digitalanalog", GameSettings.DigitalAnalog, 
                newState => { GameSettings.DigitalAnalog = newState; });
            _contentLayout.AddElement(toggleEightWay);

            // Bottom Bar / Back Button:
            _bottomBar = new InterfaceListLayout() { Size = new Point(width, (int)(height * Values.MenuFooterSize)), Selectable = true, HorizontalMode = true };
            _bottomBar.AddElement(new InterfaceButton(new Point(100, 18), new Point(2, 4), "settings_menu_back", element => { Game1.UiPageManager.PopPage(); }));
            _controlSettingsList.AddElement(_contentLayout);
            _controlSettingsList.AddElement(_bottomBar);
            PageLayout = _controlSettingsList;
        }

        public override void Update(CButtons pressedButtons, GameTime gameTime)
        {
            base.Update(pressedButtons, gameTime);

            if (_controlCooldown > 0f)
                _controlCooldown -= Game1.DeltaTime;

            // The back button was pressed.
            if (_controlCooldown <= 0f && ControlHandler.ButtonPressed(ControlHandler.CancelButton))
                Game1.UiPageManager.PopPage();

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
            // the left button is always the first one selected
            _bottomBar.Deselect(false);
            _bottomBar.Select(InterfaceElement.Directions.Left, false);
            _bottomBar.Deselect(false);

            PageLayout.Deselect(false);
            PageLayout.Select(InterfaceElement.Directions.Top, false);
        }

        public void PressButtonSetController(InterfaceElement element)
        {
            // Push forward the index +1 and loop back around.
            int index = Array.IndexOf(ControlHandler.ControllerNames, GameSettings.Controller);
            index = (index + 1) % ControlHandler.ControllerNames.Length;
            GameSettings.Controller = ControlHandler.ControllerNames[index];
            ControlHandler.SetControllerIndex();

            // Override the button text with this fancy hack.
            _controllerType.InsideLabel.OverrideText = Game1.LanguageManager.GetString("settings_controls_gamepad", "error") + ": " + GameSettings.Controller;

            // Update the buttons on the controller page.
            ControlMappingPage.UpdateLabels();
        }

        public void UpdateSixButtonToggle(bool newState)
        {
            // Enable or disable the six inventory button state.
            GameSettings.SixButtons = newState;

            // The number of inventory slots needs to be upated now so the game knows to enable/disable the top front buttons immediately.
            Values.HandItemSlots = newState ? 6 : 4; 
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
            if (_controlSettingsList.SelectionIndex == 2)
                return  Game1.LanguageManager.GetString("tooltip_default", "error");

            // Detect the chosen button by checking the content InterfaceListLayout.
            int index = _contentLayout.SelectionIndex;
            string tooltip = "Select an option to view its tooltip.";

            // Use the selected index to determine which tooltip to show.
            switch (index) 
            {
                case 0:  { tooltip = Game1.LanguageManager.GetString("tooltip_controls_deadzone", "error"); break; }
                case 1:  { tooltip = Game1.LanguageManager.GetString("tooltip_controls_gamepad", "error"); break; }
                case 2:  { tooltip = Game1.LanguageManager.GetString("tooltip_controls_remap", "error"); break; }
                case 3:  { tooltip = Game1.LanguageManager.GetString("tooltip_controls_sixbuttons", "error"); break; }
                case 4:  { tooltip = Game1.LanguageManager.GetString("tooltip_controls_swapconfirm", "error"); break; }
                case 5:  { tooltip = Game1.LanguageManager.GetString("tooltip_controls_classicmove", "error"); break; }
                case 6:  { tooltip = Game1.LanguageManager.GetString("tooltip_controls_digitalanalog", "error"); break; }
            }
            // Display the tooltip in the tooltip window.
            return tooltip;
        }
    }
}
