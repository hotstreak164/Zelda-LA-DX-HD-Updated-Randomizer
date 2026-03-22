using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZ.InGame.Controls;
using ProjectZ.InGame.Interface;
using ProjectZ.InGame.Things;

namespace ProjectZ.Core.InGame.Pages.Settings
{
    internal class ControlOnScreenPage : InterfacePage
    {
        private readonly InterfaceListLayout _controlSettingsList;
        private readonly InterfaceListLayout _contentLayout;
        private readonly InterfaceListLayout _bottomBar;

        private readonly InterfaceSlider _sliderTouchControls;
        private readonly InterfaceSlider _sliderTouchScale;
        private readonly InterfaceSlider _sliderTouchOpacity;
        private readonly InterfaceSlider _sliderShadowOpacity;
        private readonly InterfaceListLayout _toggleTouchMiddleTop;

        List<string> _tooltips = new List<string>();
        private bool _showTooltip;

        public void SetOnScreenControlsSlider(int value) { ((InterfaceSlider)_sliderTouchControls).CurrentStep = value; }

        public ControlOnScreenPage(int width, int height)
        {
            EnableTooltips = true;
            var buttonWidth = 320;
            var buttonHeight = 15;
            var sliderHeight = 10;

            // On-Screen Button Settings Layout
            _controlSettingsList = new InterfaceListLayout { Size = new Point(width, height - 12), Selectable = true };
            _controlSettingsList.AddElement(new InterfaceLabel(Resources.GameHeaderFont, "settings_controls_header",
                new Point(buttonWidth, (int)(height * Values.MenuHeaderSize)), new Point(0, 0)));
            _contentLayout = new InterfaceListLayout { Size = new Point(width, (int)(height * Values.MenuContentSize) - 12), Selectable = true, ContentAlignment = InterfaceElement.Gravities.Top };

            // Slider: On-Screen Controls
            _sliderTouchControls = new InterfaceSlider("settings_controls_onscreenpad", 
                buttonWidth, sliderHeight, new Point(1, 2), 0, 2, 1, GameSettings.TouchControls,
                number => { GameSettings.TouchControls = number; })
                { SetString = number => SetOnScreenControlsVisibility(number) };
            _contentLayout.AddElement(_sliderTouchControls);
            _tooltips.Add("tooltip_controls_onscreenpad");

            // Slider: Fade Opacity
            _sliderTouchOpacity = new InterfaceSlider("settings_controls_fadeopacity", 
                buttonWidth, sliderHeight, new Point(1, 2), 0, 100, 1, GameSettings.TouchOpacity,
                number => { GameSettings.TouchOpacity = number; })
                { SetString = number => FadeOpacitySliderAdjustmentString(number) };
            _contentLayout.AddElement(_sliderTouchOpacity);
            _tooltips.Add("tooltip_controls_fadeopacity");

            // Slider: Shadow Opacity
            _sliderShadowOpacity = new InterfaceSlider("settings_controls_shadowopacity", 
                buttonWidth, sliderHeight, new Point(1, 2), 0, 100, 1, GameSettings.ShadowOpacity,
                number => { GameSettings.ShadowOpacity = number; })
                { SetString = number => FadeOpacitySliderAdjustmentString(number) };
            _contentLayout.AddElement(_sliderShadowOpacity);
            _tooltips.Add("tooltip_controls_shadowopacity");

            // Slider: Controls Scaling
            _sliderTouchScale = new InterfaceSlider("settings_controls_onscreenscale", 
                buttonWidth, sliderHeight, new Point(1, 2), 4, 20, 1, GameSettings.TouchScaling - 4,
                number => { GameSettings.TouchScaling = number; })
                { SetString = number => OnScreenScaleSliderAdjustmentString(number) };
            _contentLayout.AddElement(_sliderTouchScale);
            _tooltips.Add("tooltip_controls_onscreenscale");

            // Slider: Touch Momement Options
            _sliderTouchScale = new InterfaceSlider("settings_controls_touchmovement", 
                buttonWidth, sliderHeight, new Point(1, 2), 0, 2, 1, GameSettings.TouchMovement,
                number => { GameSettings.TouchMovement = number; })
                { SetString = number => TouchMovementSliderAdjustmentString(number) };
            _contentLayout.AddElement(_sliderTouchScale);
            _tooltips.Add("tooltip_controls_touchmovement");

            // Toggle: Select/Start On Top
            _toggleTouchMiddleTop = InterfaceToggle.GetToggleButton(
                new Point(buttonWidth, buttonHeight), new Point(5, 2),
                "settings_controls_onscreenmiddletop", GameSettings.TouchTopMiddle, 
                newState => { GameSettings.TouchTopMiddle = newState; VirtualController.Initialize(Game1.WindowWidth, Game1.WindowHeight); });
            _contentLayout.AddElement(_toggleTouchMiddleTop);
            _tooltips.Add("tooltip_controls_onscreenmiddletop");

            // Toggle: LS/RS Buttons
            _toggleTouchMiddleTop = InterfaceToggle.GetToggleButton(
                new Point(buttonWidth, buttonHeight), new Point(5, 2),
                "settings_controls_onscreenanalogbuttons", GameSettings.TouchSticks, 
                newState => { GameSettings.TouchSticks = newState; VirtualController.Initialize(Game1.WindowWidth, Game1.WindowHeight); });
            _contentLayout.AddElement(_toggleTouchMiddleTop);
            _tooltips.Add("tooltip_controls_onscreenanalogbuttons");

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

            // The back button was pressed.
            if (ControlHandler.ButtonPressed(ControlHandler.CancelButton))
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
        private string SetOnScreenControlsVisibility(int number)
        {
            return ": " + number switch
            {
                0 => Game1.LanguageManager.GetString("settings_controls_onscreenpad_01", "error"),
                1 => Game1.LanguageManager.GetString("settings_controls_onscreenpad_02", "error"),
                2 => Game1.LanguageManager.GetString("settings_controls_onscreenpad_03", "error"),
            };
        }

        private string FadeOpacitySliderAdjustmentString(int number)
        {
            // Apply the scaling settings to the controls.
            VirtualController.Initialize(Game1.WindowWidth, Game1.WindowHeight);

            // Display the updated value.
            return ": " + (number) + "%";
        }

        private string OnScreenScaleSliderAdjustmentString(int number)
        {
            // Apply the scaling settings to the controls.
            VirtualController.Initialize(Game1.WindowWidth, Game1.WindowHeight);

            // Display the updated value.
            return ": " + (number * 5) + "%";
        }

        private string TouchMovementSliderAdjustmentString(int number)
        {
            // Apply the scaling settings to the controls.
            VirtualController.Initialize(Game1.WindowWidth, Game1.WindowHeight);

            // Display the updated string.
            return ": " + number switch
            {
                0 => Game1.LanguageManager.GetString("settings_controls_touchmovement_01", "error"),
                1 => Game1.LanguageManager.GetString("settings_controls_touchmovement_02", "error"),
                2 => Game1.LanguageManager.GetString("settings_controls_touchmovement_03", "error"),
            };
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
                return Game1.LanguageManager.GetString("tooltip_default", "error");

            // Detect the chosen button by checking the content InterfaceListLayout.
            int index = _contentLayout.SelectionIndex;
            string tooltip = "Select an option to view its tooltip.";

            // Get the tooltip that matches the index.
            for (int i = 0; i < _tooltips.Count; i++)
            {
                if (i == index)
                    tooltip = Game1.LanguageManager.GetString(_tooltips[i], "error");
            }
            // Display the tooltip in the tooltip window.
            return tooltip;
        }
    }
}
