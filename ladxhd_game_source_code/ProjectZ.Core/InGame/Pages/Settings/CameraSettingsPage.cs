using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZ.InGame.Controls;
using ProjectZ.InGame.Interface;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.Pages
{
    class CameraSettingsPage : InterfacePage
    {
        private readonly InterfaceListLayout _cameraOptionsList;
        private readonly InterfaceListLayout _contentLayout;
        private readonly InterfaceListLayout _bottomBar;

        private readonly InterfaceButton     _buttonCameraType;
        private readonly InterfaceListLayout _toggleModernOverworld;
        private readonly InterfaceListLayout _toggleClassicDungeon;
        private readonly InterfaceSlider     _sliderCameraBorder;
        private readonly InterfaceSlider     _sliderBorderOpacity;
        private readonly InterfaceListLayout _toggleClassicScaling;
        private readonly InterfaceListLayout _toggleCameraLock;
        private readonly InterfaceListLayout _toggleCameraSmooth;

        List<string> _tooltips = new List<string>();
        private bool _showTooltip;

        public void SetCameraMode(bool state) => ToggleCameraModes(state);
        public void SetModernOverworld(bool state) => ((InterfaceToggle)_toggleModernOverworld.Elements[1]).ToggleState = state;
        public void SetClassicDungeon(bool state) => ((InterfaceToggle)_toggleClassicDungeon.Elements[1]).ToggleState = state;
        public void SetClassicCamBorder(int value) { ((InterfaceSlider)_sliderCameraBorder).CurrentStep = value; }
        public void SetClassicBorderAlpha(int value) { ((InterfaceSlider)_sliderBorderOpacity).CurrentStep = value; }
        public void SetClassicScaleLock(bool state) => ((InterfaceToggle)_toggleClassicScaling.Elements[1]).ToggleState = state;
        public void SetCameraLock(bool state) => ((InterfaceToggle)_toggleCameraLock.Elements[1]).ToggleState = state; 
        public void SetCameraSmoothCam(bool state) => ((InterfaceToggle)_toggleCameraSmooth.Elements[1]).ToggleState = state;

        public void UpdateCameraOverrideText()
        {
            // Get the translated camera name for modern/classic.
            string cameraName = GameSettings.ClassicCamera 
                ? Game1.LanguageManager.GetString("settings_camera_camera_classic", "error")
                : Game1.LanguageManager.GetString("settings_camera_camera_modern", "error");

            // The "OverrideText" is stored so if the language is changed then the text also needs to be updated.
            string UpdateText = Game1.LanguageManager.GetString("settings_camera_cameratype", "error") + ": " + cameraName;

            // Update the label with the properly translated textu.
            _buttonCameraType.InsideLabel.OverrideText = UpdateText;
        }

        public CameraSettingsPage(int width, int height)
        {
            EnableTooltips = true;
            var buttonWidth = 320;
            var buttonHeight = 16;
            var sliderHeight = 12;

            // Camera Settings Layout
            _cameraOptionsList = new InterfaceListLayout { Size = new Point(width, height - 12), Selectable = true };
            _cameraOptionsList.AddElement(new InterfaceLabel(Resources.GameHeaderFont, "settings_camera_header",
                new Point(buttonWidth, (int)(height * Values.MenuHeaderSize)), new Point(0, 0)));
            _contentLayout = new InterfaceListLayout { Size = new Point(width, (int)(height * Values.MenuContentSize) - 12), Selectable = true, ContentAlignment = InterfaceElement.Gravities.Top };

            // Button: Modern/Classic Camera
            _contentLayout.AddElement(_buttonCameraType = new InterfaceButton(new Point(buttonWidth, buttonHeight), new Point(0, 2), "", PressButtonCameraChange));
            UpdateCameraOverrideText();

            // Toggle: Modern: Overworld Only
            _toggleModernOverworld = InterfaceToggle.GetToggleButton(new Point(buttonWidth, buttonHeight), new Point(5, 2),
                "settings_camera_modernoverworld", GameSettings.ModernOverworld, 
                newState => { GameSettings.ModernOverworld = newState; Game1.ScaleChanged = true; Camera.SnapCameraTimer = 10f; });

            // Toggle: Classic: Dungeons Only
            _toggleClassicDungeon = InterfaceToggle.GetToggleButton(new Point(buttonWidth, buttonHeight), new Point(5, 2),
                "settings_camera_classicdungeon", GameSettings.ClassicDungeon, 
                newState => { GameSettings.ClassicDungeon = newState; Game1.ScaleChanged = true; Camera.SnapCameraTimer = 10f; });
            
            // Depending on which of the above two options is added depends on camera state.
            if (GameSettings.ClassicCamera)
                _contentLayout.AddElement(_toggleClassicDungeon);
            else
                _contentLayout.AddElement(_toggleModernOverworld);

            // Slider: Classic Camera Border
            _sliderCameraBorder = new InterfaceSlider("settings_camera_camborder",
                buttonWidth, sliderHeight, new Point(1, 2), 0, 2, 1, GameSettings.ClassicBorder, 
                number => { GameSettings.ClassicBorder = number; Game1.ScaleChanged = true; Camera.SnapCameraTimer = 10f; }) 
                { SetString = number => ClassicBorderAdjustment(number) };
            _contentLayout.AddElement(_sliderCameraBorder);

            // Slider: Classic Blackout Amount
            _sliderBorderOpacity = new InterfaceSlider("settings_camera_blackpercent",
                buttonWidth, sliderHeight, new Point(1, 2), 0, 100, 5, (int)(GameSettings.ClassicAlpha * 100),
                number => { GameSettings.ClassicAlpha = (float)(number * 0.01); })
                { SetString = number => SetClassicBorderOpacity(number) };
            _contentLayout.AddElement(_sliderBorderOpacity);

            // Toggle: Classic Scale Lock
            _toggleClassicScaling = InterfaceToggle.GetToggleButton(
                new Point(buttonWidth, buttonHeight), new Point(5, 2),
                "settings_camera_classicscaling", GameSettings.ClassicScaling, 
                newState => { GameSettings.ClassicScaling = newState; Game1.ScaleChanged = true; Camera.SnapCameraTimer = 10f; });
            _contentLayout.AddElement(_toggleClassicScaling);

            // Toggle: Camera Lock
            _toggleCameraLock = InterfaceToggle.GetToggleButton(
                new Point(buttonWidth, buttonHeight), new Point(5, 2),
                "settings_camera_cameralock", GameSettings.CameraLock, 
                newState => { GameSettings.CameraLock = newState; ReloadVirtualController(); });
            _contentLayout.AddElement(_toggleCameraLock);

            // Toggle: Smooth Camera
            _toggleCameraSmooth = InterfaceToggle.GetToggleButton(
                new Point(buttonWidth, buttonHeight), new Point(5, 2),
                "settings_camera_smoothcamera", GameSettings.SmoothCamera, 
                newState => { GameSettings.SmoothCamera = newState; });
            _contentLayout.AddElement(_toggleCameraSmooth);

            // Bottom Bar / Back Button:
            _bottomBar = new InterfaceListLayout() { Size = new Point(width, (int)(height * Values.MenuFooterSize)), Selectable = true, HorizontalMode = true };
            _bottomBar.AddElement(new InterfaceButton(new Point(100, 18), new Point(2, 4), "settings_menu_back", element => { Game1.UiPageManager.PopPage(); }));
            _cameraOptionsList.AddElement(_contentLayout);
            _cameraOptionsList.AddElement(_bottomBar);
            PageLayout = _cameraOptionsList;

            // Update button colors.
            UpdateInterfaceColors();
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

        private void PressButtonCameraChange(InterfaceElement element)
        {
            ToggleCameraModes();
        }

        private void ToggleCameraModes(bool? classicState = null)
        {
            // Force the parameter if defined and if not, invert the current Classic Camera selection.
            GameSettings.ClassicCamera = classicState ?? !GameSettings.ClassicCamera;

            // Override the button text with this fancy hack.
            UpdateCameraOverrideText();

            // The camera has changed so the game scale must also be upated.
            Game1.ScaleChanged = true;

            // Toggling classic camera "grays out" some options depending on its state.
            UpdateInterfaceColors();

            // Replace the opposing "support" option with the one that matches the current type.
            if (GameSettings.ClassicCamera)
                _contentLayout.ReplaceElement(_toggleModernOverworld, _toggleClassicDungeon);
            else
                _contentLayout.ReplaceElement(_toggleClassicDungeon, _toggleModernOverworld);
        }

        public void ReloadVirtualController()
        {
        #if ANDROID
            VirtualController.Initialize(Game1.WindowWidth, Game1.WindowHeight);
        #endif
        }

        public void UpdateInterfaceColors()
        {
            _toggleClassicDungeon.ToggleElementColors(GameSettings.ClassicCamera);
            _sliderCameraBorder.ToggleSliderColors(GameSettings.ClassicCamera);
            _sliderBorderOpacity.ToggleSliderColors(GameSettings.ClassicCamera);
            _toggleCameraLock.ToggleElementColors(!GameSettings.ClassicCamera);
        }

        private string ClassicBorderAdjustment(int number)
        {
            return ": " + number switch
            {
                0 => Game1.LanguageManager.GetString("tooltip_camera_camborderA", "error"),
                1 => Game1.LanguageManager.GetString("tooltip_camera_camborderB", "error"),
                2 => Game1.LanguageManager.GetString("tooltip_camera_camborderC", "error"),
                _ => Game1.LanguageManager.GetString("tooltip_camera_camborderA", "error")
            };
        }

        private string SetClassicBorderOpacity(int number)
        {
            return ": " + number + "%";
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
            if (_cameraOptionsList.SelectionIndex == 2)
                return  Game1.LanguageManager.GetString("tooltip_default", "error");

            // Detect the chosen button by checking the content InterfaceListLayout.
            int index = _contentLayout.SelectionIndex;
            string tooltip = "Select an option to view its tooltip.";

            // Use the selected index to determine which tooltip to show.
            switch (index) 
            {
                case 0:  { tooltip = Game1.LanguageManager.GetString(GameSettings.ClassicCamera ? "tooltip_camera_classiccam" : "tooltip_camera_moderncam", "error"); break; }
                case 1:  { tooltip = Game1.LanguageManager.GetString(GameSettings.ClassicCamera ? "tooltip_camera_classicdungeon" : "tooltip_camera_modernoverworld", "error"); break; }
                case 2:  { tooltip = Game1.LanguageManager.GetString("tooltip_camera_camborder", "error"); break; }
                case 3:  { tooltip = Game1.LanguageManager.GetString("tooltip_camera_blackpercent", "error"); break; }
                case 4:  { tooltip = Game1.LanguageManager.GetString("tooltip_camera_classicscaling", "error"); break; }
                case 5:  { tooltip = Game1.LanguageManager.GetString("tooltip_camera_cameralock", "error"); break; }
                case 6:  { tooltip = Game1.LanguageManager.GetString("tooltip_camera_smoothcamera", "error"); break; }
            }
            // Display the tooltip in the tooltip window.
            return tooltip;
        }
    }
}