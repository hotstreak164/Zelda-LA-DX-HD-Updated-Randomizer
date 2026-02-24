using System.Collections.Generic;
using System.Security.AccessControl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZ.InGame.Controls;
using ProjectZ.InGame.Interface;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.Pages
{
    class PresetOptionsPage : InterfacePage
    {
        private readonly InterfaceListLayout _presetSettingsLayout;
        private readonly InterfaceListLayout _contentLayout;
        private readonly InterfaceListLayout _bottomBar;

        private readonly InterfaceListLayout _toggleDefaultSettings;

        private bool _showTooltip;

        public PresetOptionsPage(int width, int height)
        {
            EnableTooltips = true;

            // Audio Settings Layout
            _presetSettingsLayout = new InterfaceListLayout { Size = new Point(width, height - 12), Selectable = true };

            var buttonWidth = 320;
            var buttonHeight = 16;
            var buttonSize = new Point(150, 16);

            _presetSettingsLayout.AddElement(new InterfaceLabel(Resources.GameHeaderFont, "settings_preset_header",
                new Point(buttonWidth, (int)(height * Values.MenuHeaderSize)), new Point(0, 0)));
            _contentLayout = new InterfaceListLayout { Size = new Point(width, (int)(height * Values.MenuContentSize - 12)), Selectable = true, ContentAlignment = InterfaceElement.Gravities.Top };

            // Button: Set Default Option Values
            _contentLayout.AddElement(new InterfaceButton(buttonSize, new Point(1, 2), "settings_preset_setdefault", element => { RestoreDefaults(); }));

            // Button: Set Modern Values
            _contentLayout.AddElement(new InterfaceButton(buttonSize, new Point(1, 2), "settings_preset_setmodern", element => { SetModernValues(); }));

            // Button: Set Classic Values
            _contentLayout.AddElement(new InterfaceButton(buttonSize, new Point(1, 2), "settings_preset_setclassic", element => { SetClassicValues(); }));

            // Button: Set Hybrid Values
            _contentLayout.AddElement(new InterfaceButton(buttonSize, new Point(1, 2), "settings_preset_sethybrid", element => { SetHybridValues(); }));

            // Button: Set Purist Values
            _contentLayout.AddElement(new InterfaceButton(buttonSize, new Point(1, 2), "settings_preset_purist", element => { SetPuristValues(); }));

            // Bottom Bar / Back Button:
            _bottomBar = new InterfaceListLayout() { Size = new Point(width, (int)(height * Values.MenuFooterSize)), Selectable = true, HorizontalMode = true };
            _bottomBar.AddElement(new InterfaceButton(new Point(100, 18), new Point(2, 4), "settings_menu_back", element => { Game1.UiPageManager.PopPage(); }));
            _presetSettingsLayout.AddElement(_contentLayout);
            _presetSettingsLayout.AddElement(_bottomBar);
            PageLayout = _presetSettingsLayout;
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

        private void RestoreDefaults()
        {
            GameSettings.RestoreDefaults();
            UpdateSettingsGUI();
        }

        private static void SetModernValues()
        {
            GameSettings.ClassicSword = false;
            GameSettings.VarWidthFont = true;
            GameSettings.Unmissables = true;
            GameSettings.PhotosColor = true;
            GameSettings.MapTeleport = 1;
            GameSettings.ClassicCamera = false;
            GameSettings.ModernOverworld = false;
            GameSettings.ClassicDungeon = false;
            GameSettings.ClassicBorders = 0;
            GameSettings.CameraLock = false;
            GameSettings.ScreenShake = true;
            GameSettings.ExScreenShake = true;
            GameSettings.GlobalLights = true;
            GameSettings.ObjectLights = true;
            GameSettings.EnableShadows = true;
            GameSettings.ClassicMusic = false;
            GameSettings.OldMovement = false;
            GameSettings.DigitalAnalog = false;
            GameSettings.EnemyBonusHP = 0;
            GameSettings.DamageFactor = 4;
            GameSettings.DmgCooldown = 16;
            GameSettings.MoveSpeedAdded = 0;
            GameSettings.NoHeartDrops = false;
            GameSettings.NoDamageLaunch = false;
            GameSettings.MirrorReflects = true;
            GameSettings.SwGrabNormal = true;
            GameSettings.SwGrabWorldItem = false;
            GameSettings.SwGrabFairy = true;
            GameSettings.SwGrabSmallKey = false;
            GameSettings.SwBoomerang = true;
            GameSettings.SwSmackBombs = true;
            GameSettings.SwMissileBlock = false;
            GameSettings.SwBreakPots = true;
            GameSettings.SwBeamShrubs = false;
            UpdateSettingsGUI();
        }

        private static void SetClassicValues()
        {
            GameSettings.ClassicSword = false;
            GameSettings.Unmissables = true;
            GameSettings.PhotosColor = false;
            GameSettings.MapTeleport = 0;
            GameSettings.ClassicCamera = true;
            GameSettings.ClassicDungeon = false;
            GameSettings.ClassicBorders = 1;
            GameSettings.CameraLock = true;
            GameSettings.ScreenShake = true;
            GameSettings.ExScreenShake = true;
            GameSettings.GlobalLights = true;
            GameSettings.ObjectLights = false;
            GameSettings.EnableShadows = true;
            GameSettings.ClassicMusic = true;
            GameSettings.OldMovement = false;
            GameSettings.DigitalAnalog = false;
            GameSettings.EnemyBonusHP = 0;
            GameSettings.DamageFactor = 4;
            GameSettings.DmgCooldown = 16;
            GameSettings.MoveSpeedAdded = 0;
            GameSettings.NoHeartDrops = false;
            GameSettings.NoDamageLaunch = false;
            GameSettings.MirrorReflects = false;
            GameSettings.SwGrabNormal = true;
            GameSettings.SwGrabWorldItem = false;
            GameSettings.SwGrabFairy = false;
            GameSettings.SwGrabSmallKey = false;
            GameSettings.SwBoomerang = false;
            GameSettings.SwSmackBombs = false;
            GameSettings.SwMissileBlock = false;
            GameSettings.SwBreakPots = false;
            GameSettings.SwBeamShrubs = false;
            UpdateSettingsGUI();
        }

        private static void SetHybridValues()
        {
            GameSettings.ClassicSword = false;
            GameSettings.Unmissables = true;
            GameSettings.PhotosColor = true;
            GameSettings.MapTeleport = 1;
            GameSettings.ClassicCamera = true;
            GameSettings.ModernOverworld = true;
            GameSettings.ClassicDungeon = true;
            GameSettings.ClassicBorders = 1;
            GameSettings.CameraLock = false;
            GameSettings.ScreenShake = true;
            GameSettings.ExScreenShake = true;
            GameSettings.GlobalLights = true;
            GameSettings.ObjectLights = true;
            GameSettings.EnableShadows = true;
            GameSettings.ClassicMusic = false;
            GameSettings.HeartBeep = true;
            GameSettings.OldMovement = false;
            GameSettings.DigitalAnalog = false;
            GameSettings.EnemyBonusHP = 0;
            GameSettings.DamageFactor = 4;
            GameSettings.DmgCooldown = 16;
            GameSettings.MoveSpeedAdded = 0;
            GameSettings.NoHeartDrops = false;
            GameSettings.NoDamageLaunch = false;
            GameSettings.MirrorReflects = true;
            GameSettings.SwGrabNormal = true;
            GameSettings.SwGrabWorldItem = false;
            GameSettings.SwGrabFairy = true;
            GameSettings.SwGrabSmallKey = false;
            GameSettings.SwBoomerang = false;
            GameSettings.SwSmackBombs = false;
            GameSettings.SwMissileBlock = false;
            GameSettings.SwBreakPots = false;
            GameSettings.SwBeamShrubs = false;
            UpdateSettingsGUI();
        }

        private static void SetPuristValues()
        {
            GameSettings.ClassicSword = true;
            GameSettings.VarWidthFont = false;
            GameSettings.NoHelperText = false;
            GameSettings.DialogSkip = false;
            GameSettings.Unmissables = false;
            GameSettings.PhotosColor = false;
            GameSettings.MapTeleport = 0;
            GameSettings.ClassicCamera = true;
            GameSettings.ClassicDungeon = false;
            GameSettings.ClassicBorders = 1;
            GameSettings.CameraLock = true;
            GameSettings.ScreenShake = true;
            GameSettings.ExScreenShake = false;
            GameSettings.GlobalLights = false;
            GameSettings.ObjectLights = false;
            GameSettings.EnableShadows = false;
            GameSettings.ClassicMusic = true;
            GameSettings.HeartBeep = true;
            GameSettings.OldMovement = true;
            GameSettings.DigitalAnalog = true;
            GameSettings.EnemyBonusHP = 0;
            GameSettings.DamageFactor = 4;
            GameSettings.DmgCooldown = 16;
            GameSettings.MoveSpeedAdded = 0;
            GameSettings.NoHeartDrops = false;
            GameSettings.NoDamageLaunch = false;
            GameSettings.MirrorReflects = false;
            GameSettings.SwGrabNormal = true;
            GameSettings.SwGrabWorldItem = false;
            GameSettings.SwGrabFairy = false;
            GameSettings.SwGrabSmallKey = false;
            GameSettings.SwBoomerang = false;
            GameSettings.SwSmackBombs = false;
            GameSettings.SwMissileBlock = false;
            GameSettings.SwBreakPots = false;
            GameSettings.SwBeamShrubs = false;
            UpdateSettingsGUI();
        }

        public static void UpdateSettingsGUI()
        {
            if (Game1.UiPageManager.InsideElement.TryGetValue(typeof(GameSettingsPage), out var gamePage))
            {
                var GameSettingsPage = (GameSettingsPage)gamePage;
                GameSettingsPage.SetMenuBricks(GameSettings.MenuBorder);
                GameSettingsPage.SetClassicSword(GameSettings.ClassicSword);
                GameSettingsPage.SetSavePosition(GameSettings.StoreSavePos);
                GameSettingsPage.SetAutoSave(GameSettings.Autosave);
                GameSettingsPage.SetItemSlotRight(GameSettings.ItemsOnRight);
                GameSettingsPage.SetEpilepsySafe(GameSettings.EpilepsySafe);
            }
            if (Game1.UiPageManager.InsideElement.TryGetValue(typeof(ReduxOptionsPage), out var reduxPage))
            {
                var ReduxSettingsPage = (ReduxOptionsPage)reduxPage;
                ReduxSettingsPage.SetVariableWidthFont(GameSettings.VarWidthFont);
                ReduxSettingsPage.SetDisableHelperText(GameSettings.NoHelperText);
                ReduxSettingsPage.SetEnableDialogSkip(GameSettings.DialogSkip);
                ReduxSettingsPage.SetDisableCensorship(GameSettings.Uncensored);
                ReduxSettingsPage.SetEnableUnmissables(GameSettings.Unmissables);
                ReduxSettingsPage.SetColoredPhotographs(GameSettings.PhotosColor);
                ReduxSettingsPage.SetNoAnimalDamage(GameSettings.NoAnimalDamage);
                ReduxSettingsPage.SetMapTeleportValue(GameSettings.MapTeleport);
            }
            if (Game1.UiPageManager.InsideElement.TryGetValue(typeof(CameraSettingsPage), out var camPage))
            {
                var CameraSettingsPage = (CameraSettingsPage)camPage;
                CameraSettingsPage.SetCameraMode(GameSettings.ClassicCamera);
                CameraSettingsPage.SetModernOverworld(GameSettings.ModernOverworld);
                CameraSettingsPage.SetClassicDungeon(GameSettings.ClassicDungeon);
                CameraSettingsPage.SetClassicCamBorder(GameSettings.ClassicBorders);
                CameraSettingsPage.SetClassicBorderAlpha((int)(GameSettings.ClassicAlpha * 100));
                CameraSettingsPage.SetCameraLock(GameSettings.CameraLock);
                CameraSettingsPage.SetCameraSmoothCam(GameSettings.SmoothCamera);
                CameraSettingsPage.SetCameraScreenShake(GameSettings.ScreenShake);
                CameraSettingsPage.SetCameraExScreenShake(GameSettings.ExScreenShake);
            }
            if (Game1.UiPageManager.InsideElement.TryGetValue(typeof(GraphicSettingsPage), out var videoPage))
            {
                var GraphicsSettingsPage = (GraphicSettingsPage)videoPage;
                GraphicsSettingsPage.SetGameScaleValue(GameSettings.GameScale);
                GraphicsSettingsPage.SetUserInterfaceScale(GameSettings.UiScale);
                GraphicsSettingsPage.SetGlobalLighting(GameSettings.GlobalLights);
                GraphicsSettingsPage.SetObjectLighting(GameSettings.ObjectLights);
                GraphicsSettingsPage.SetDynamicShadows(GameSettings.EnableShadows);
                GraphicsSettingsPage.SetVerticalSync(GameSettings.VerticalSync);
            }
            if (Game1.UiPageManager.InsideElement.TryGetValue(typeof(AudioSettingsPage), out var audioPage))
            {
                var AudioSettingsPage = (AudioSettingsPage)audioPage;
                AudioSettingsPage.SetMusicVolume(GameSettings.MusicVolume);
                AudioSettingsPage.SetSoundVolume(GameSettings.EffectVolume);
                AudioSettingsPage.SetClassicAudio(GameSettings.ClassicMusic);
                AudioSettingsPage.SetMuteInactive(GameSettings.MuteInactive);
                AudioSettingsPage.SetHealthAlarm(GameSettings.HeartBeep);
                AudioSettingsPage.SetPowerupMusic(GameSettings.MutePowerups);
            }
            if (Game1.UiPageManager.InsideElement.TryGetValue(typeof(ControlSettingsPage), out var controlPage))
            {
                var ControlSettingsPage = (ControlSettingsPage)controlPage;
                ControlSettingsPage.SetDeadZoneValue((int)(GameSettings.DeadZone * 100));
                ControlSettingsPage.SetTriggerScale(GameSettings.TriggersScale);
                ControlSettingsPage.SetSixButtons(GameSettings.SixButtons);
                ControlSettingsPage.SetSwapButtons(GameSettings.SwapButtons);
                ControlSettingsPage.SetClassicMove(GameSettings.OldMovement);
                ControlSettingsPage.SetDigitalAnalog(GameSettings.DigitalAnalog);
            }
            if (Game1.UiPageManager.InsideElement.TryGetValue(typeof(ModifiersPage), out var modPage))
            {
                var ModifiersPage = (ModifiersPage)modPage;
                ModifiersPage.SetEnemyHitPoints(GameSettings.EnemyBonusHP);
                ModifiersPage.SetDamageTaken(GameSettings.DamageFactor);
                ModifiersPage.SetDamageCooldown(GameSettings.DmgCooldown);
                ModifiersPage.SetMovementSpeed((int)(GameSettings.MoveSpeedAdded * 10));
                ModifiersPage.SetNoHeartDrops(GameSettings.NoHeartDrops);
                ModifiersPage.SetNoDamageLaunch(GameSettings.NoDamageLaunch);
                ModifiersPage.SetMirrorReflects(GameSettings.MirrorReflects);
            }
            if (Game1.UiPageManager.InsideElement.TryGetValue(typeof(SwordInteractPage), out var swordPage))
            {
                var SwordInteractPage = (SwordInteractPage)swordPage;
                SwordInteractPage.SetSwordCollectNormal(GameSettings.SwGrabNormal);
                SwordInteractPage.SetSwordCollectStatic(GameSettings.SwGrabWorldItem);
                SwordInteractPage.SetSwordCollectFairy(GameSettings.SwGrabFairy);
                SwordInteractPage.SetSwordCollectKeys(GameSettings.SwGrabSmallKey);
                SwordInteractPage.SetSwordBounceBoomerang(GameSettings.SwBoomerang);
                SwordInteractPage.SetSwordBounceBombs(GameSettings.SwSmackBombs);
                SwordInteractPage.SetSwordBlockProjectile(GameSettings.SwMissileBlock);
                SwordInteractPage.SetSwordSmashesPots(GameSettings.SwBreakPots);
                SwordInteractPage.SetSwordBeamCutsShrubs(GameSettings.SwBeamShrubs);
            }
            Game1.ScaleChanged = true;
        }

        private string GetOptionToolip()
        {
            // Detect back button press by checking the index of the main InterfaceListLayout.
            if (_presetSettingsLayout.SelectionIndex == 2)
                return  Game1.LanguageManager.GetString("tooltip_default", "error");

            // Detect the chosen button by checking the content InterfaceListLayout.
            int index = _contentLayout.SelectionIndex;
            string tooltip = "Select an option to view its tooltip.";

            // Use the selected index to determine which tooltip to show.
            switch (index) 
            {
                case 0:  { tooltip = Game1.LanguageManager.GetString("tooltip_preset_setdefault", "error"); break; }
                case 1:  { tooltip = Game1.LanguageManager.GetString("tooltip_preset_setmodern", "error"); break; }
                case 2:  { tooltip = Game1.LanguageManager.GetString("tooltip_preset_setclassic", "error"); break; }
                case 3:  { tooltip = Game1.LanguageManager.GetString("tooltip_preset_sethybrid", "error"); break; }
                case 4:  { tooltip = Game1.LanguageManager.GetString("tooltip_preset_purist", "error"); break; }
            }
            // Display the tooltip in the tooltip window.
            return tooltip;
        }
    }
}
