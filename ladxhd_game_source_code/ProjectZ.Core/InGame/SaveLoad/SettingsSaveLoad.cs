using ProjectZ.InGame.Controls;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.SaveLoad
{
    class SettingsSaveLoad
    {
        private static string SettingsFilePath => SaveManager.GetSettingsFile();

        public static void LoadSettings()
        {
            if (new SaveManager() is not { } saveManager || !saveManager.LoadFile(SettingsFilePath))
                return;

            // Game Settings
            Game1.LanguageManager.CurrentLanguageIndex = saveManager.GetInt("CurrentLanguage", Game1.LanguageManager.CurrentLanguageIndex);
            Game1.LanguageManager.CurrentSubLanguageIndex = saveManager.GetInt("CurrentSubLanguage", Game1.LanguageManager.CurrentSubLanguageIndex);
            GameSettings.MenuBorder = saveManager.GetInt("MenuBorder", GameSettings.MenuBorder);
            GameSettings.ClassicSword = saveManager.GetBool("ClassicSword", GameSettings.ClassicSword);
            GameSettings.StoreSavePos = saveManager.GetBool("StoreSavePos", GameSettings.StoreSavePos);
            GameSettings.LastSavePos = saveManager.GetInt("LastSavePos", GameSettings.LastSavePos);
            GameSettings.Autosave = saveManager.GetBool("Autosave", GameSettings.Autosave);
            GameSettings.ItemsOnRight = saveManager.GetBool("ItemsOnRight", GameSettings.ItemsOnRight);
            GameSettings.EpilepsySafe = saveManager.GetBool("EpilepsySafe", GameSettings.EpilepsySafe);

            // Redux Settings
            GameSettings.VarWidthFont = saveManager.GetBool("VarWidthFont", GameSettings.VarWidthFont);
            GameSettings.NoHelperText = saveManager.GetBool("NoHelperText", GameSettings.NoHelperText);
            GameSettings.DialogSkip = saveManager.GetBool("DialogSkip", GameSettings.DialogSkip);
            GameSettings.Uncensored = saveManager.GetBool("Uncensored", GameSettings.Uncensored);
            GameSettings.Unmissables = saveManager.GetBool("Unmissables", GameSettings.Unmissables);
            GameSettings.PhotosColor = saveManager.GetBool("PhotosColor", GameSettings.PhotosColor);
            GameSettings.NoAnimalDamage = saveManager.GetBool("NoAnimalDamage", GameSettings.NoAnimalDamage);
            GameSettings.MapTeleport = saveManager.GetInt("MapTeleport", GameSettings.MapTeleport);

            // Camera Settings
            GameSettings.ClassicCamera = saveManager.GetBool("ClassicCamera", GameSettings.ClassicCamera);
            GameSettings.ModernOverworld = saveManager.GetBool("ModernOverworld", GameSettings.ModernOverworld);
            GameSettings.ClassicDungeon = saveManager.GetBool("ClassicDungeon", GameSettings.ClassicDungeon);
            GameSettings.ClassicBorder = saveManager.GetInt("ClassicBorder", GameSettings.ClassicBorder);
            GameSettings.ClassicAlpha = saveManager.GetFloat("ClassicAlpha", GameSettings.ClassicAlpha);
            GameSettings.ClassicScaling = saveManager.GetBool("ClassicScaling", GameSettings.ClassicScaling);
            GameSettings.CameraLock = saveManager.GetBool("CameraLock", GameSettings.CameraLock);
            GameSettings.SmoothCamera = saveManager.GetBool("SmoothCamera", GameSettings.SmoothCamera);
            GameSettings.ScreenShake = saveManager.GetBool("ScreenShake", GameSettings.ScreenShake);
            GameSettings.ExScreenShake = saveManager.GetBool("ExScreenShake", GameSettings.ExScreenShake);

            // Video Settings
            GameSettings.GameScale = saveManager.GetInt("GameScale", GameSettings.GameScale);
            GameSettings.UiScale = saveManager.GetInt("UIScale", GameSettings.UiScale);
            GameSettings.ScreenMode = saveManager.GetInt("ScreenMode", GameSettings.ScreenMode);
            GameSettings.VerticalSync = saveManager.GetBool("VerticalSync", GameSettings.VerticalSync);
            GameSettings.OpaqueHudBg = saveManager.GetBool("OpaqueHudBg", GameSettings.OpaqueHudBg);

            // Graphics Settings
            GameSettings.SeqScaleAmplify = saveManager.GetInt("SeqScaleAmplify", GameSettings.SeqScaleAmplify);
            GameSettings.EnableShadows = saveManager.GetBool("EnableShadows", GameSettings.EnableShadows);
            GameSettings.FogEffects = saveManager.GetBool("FogEffects", GameSettings.FogEffects);
            GameSettings.GlobalLights = saveManager.GetBool("GlobalLights", GameSettings.GlobalLights);
            GameSettings.ObjectLights = saveManager.GetBool("ObjectLights", GameSettings.ObjectLights);

            // Audio Settings
            GameSettings.MusicVolume = saveManager.GetInt("MusicVolume", GameSettings.MusicVolume);
            GameSettings.EffectVolume = saveManager.GetInt("EffectVolume", GameSettings.EffectVolume);
            GameSettings.ClassicMusic = saveManager.GetBool("ClassicMusic", GameSettings.ClassicMusic);
            GameSettings.MuteInactive = saveManager.GetBool("MuteInactive", GameSettings.MuteInactive);
            GameSettings.HeartBeep = saveManager.GetBool("HeartBeep", GameSettings.HeartBeep);
            GameSettings.MutePowerups = saveManager.GetBool("MutePowerups", GameSettings.MutePowerups);

            // Controls Settings
            GameSettings.DeadZone = saveManager.GetFloat("DeadZone", GameSettings.DeadZone);
            GameSettings.Controller = saveManager.GetString("Controller", GameSettings.Controller);
            GameSettings.TriggersScale = saveManager.GetBool("TriggersScale", GameSettings.TriggersScale);
            GameSettings.SixButtons = saveManager.GetBool("SixButtons", GameSettings.SixButtons);
            GameSettings.SwapButtons = saveManager.GetBool("SwapButtons", GameSettings.SwapButtons);
            GameSettings.OldMovement = saveManager.GetBool("OldMovement", GameSettings.OldMovement);
            GameSettings.DigitalAnalog = saveManager.GetBool("DigitalAnalog", GameSettings.DigitalAnalog);

            // On-Screen Control Settings
            GameSettings.TouchControls = saveManager.GetInt("TouchControls", GameSettings.TouchControls);
            GameSettings.TouchScaling = saveManager.GetInt("TouchScaling", GameSettings.TouchScaling);
            GameSettings.TouchOpacity = saveManager.GetInt("TouchOpacity", GameSettings.TouchOpacity);
            GameSettings.ShadowOpacity = saveManager.GetInt("ShadowOpacity", GameSettings.ShadowOpacity);
            GameSettings.TouchMovement = saveManager.GetInt("TouchMovement", GameSettings.TouchMovement);
            GameSettings.TouchTopMiddle = saveManager.GetBool("TouchTopMiddle", GameSettings.TouchTopMiddle);
            GameSettings.TouchSticks = saveManager.GetBool("TouchSticks", GameSettings.TouchSticks);

            // Modifiers Settings
            GameSettings.EnemyBonusHP = saveManager.GetInt("EnemyBonusHP", GameSettings.EnemyBonusHP);
            GameSettings.DamageFactor = saveManager.GetInt("DamageFactor", GameSettings.DamageFactor);
            GameSettings.DmgCooldown = saveManager.GetInt("DmgCooldown", GameSettings.DmgCooldown);
            GameSettings.MoveSpeedAdded = saveManager.GetFloat("MoveSpeedAdded", GameSettings.MoveSpeedAdded);
            GameSettings.NoHeartDrops = saveManager.GetBool("NoHeartDrops", GameSettings.NoHeartDrops);
            GameSettings.NoDamageLaunch = saveManager.GetBool("NoDamageLaunch", GameSettings.NoDamageLaunch);
            GameSettings.MirrorReflects = saveManager.GetBool("MirrorReflects", GameSettings.MirrorReflects);

            // Sword Interact Settings
            GameSettings.SwGrabNormal = saveManager.GetBool("SwGrabNormal", GameSettings.SwGrabNormal);
            GameSettings.SwGrabWorldItem = saveManager.GetBool("SwGrabWorldItem", GameSettings.SwGrabWorldItem);
            GameSettings.SwGrabFairy = saveManager.GetBool("SwGrabFairy", GameSettings.SwGrabFairy);
            GameSettings.SwGrabSmallKey = saveManager.GetBool("SwGrabSmallKey", GameSettings.SwGrabSmallKey);
            GameSettings.SwBoomerang = saveManager.GetBool("SwBoomerang", GameSettings.SwBoomerang);
            GameSettings.SwSmackBombs = saveManager.GetBool("SwSmackBombs", GameSettings.SwSmackBombs);
            GameSettings.SwMissileBlock = saveManager.GetBool("SwMissileBlock", GameSettings.SwMissileBlock);
            GameSettings.SwBreakPots = saveManager.GetBool("SwBreakPots", GameSettings.SwBreakPots);
            GameSettings.SwBeamShrubs = saveManager.GetBool("SwBeamShrubs", GameSettings.SwBeamShrubs);

            ControlHandler.LoadButtonMap(saveManager);
            ControlHandler.SetControllerIndex();
        }

        public static void SaveSettings()
        {
            var saveManager = new SaveManager();

            // Game Settings
            saveManager.SetInt("CurrentLanguage", Game1.LanguageManager.CurrentLanguageIndex);
            saveManager.SetInt("CurrentSubLanguage", Game1.LanguageManager.CurrentSubLanguageIndex);
            saveManager.SetInt("MenuBorder", GameSettings.MenuBorder);
            saveManager.SetBool("ClassicSword", GameSettings.ClassicSword);
            saveManager.SetBool("StoreSavePos", GameSettings.StoreSavePos);
            saveManager.SetInt("LastSavePos", GameSettings.LastSavePos);
            saveManager.SetBool("Autosave", GameSettings.Autosave);
            saveManager.SetBool("ItemsOnRight", GameSettings.ItemsOnRight);
            saveManager.SetBool("EpilepsySafe", GameSettings.EpilepsySafe);

            // Redux Settings
            saveManager.SetBool("VarWidthFont", GameSettings.VarWidthFont);
            saveManager.SetBool("NoHelperText", GameSettings.NoHelperText);
            saveManager.SetBool("DialogSkip", GameSettings.DialogSkip);
            saveManager.SetBool("Uncensored", GameSettings.Uncensored);
            saveManager.SetBool("Unmissables", GameSettings.Unmissables);
            saveManager.SetBool("PhotosColor", GameSettings.PhotosColor);
            saveManager.SetBool("NoAnimalDamage", GameSettings.NoAnimalDamage);
            saveManager.SetInt("MapTeleport", GameSettings.MapTeleport);

            // Camera Settings
            saveManager.SetBool("ClassicCamera", GameSettings.ClassicCamera);
            saveManager.SetBool("ModernOverworld", GameSettings.ModernOverworld);
            saveManager.SetBool("ClassicDungeon", GameSettings.ClassicDungeon);
            saveManager.SetInt("ClassicBorder", GameSettings.ClassicBorder);
            saveManager.SetFloat("ClassicAlpha", GameSettings.ClassicAlpha);
            saveManager.SetBool("ClassicScaling", GameSettings.ClassicScaling);
            saveManager.SetBool("CameraLock", GameSettings.CameraLock);
            saveManager.SetBool("SmoothCamera", GameSettings.SmoothCamera);
            saveManager.SetBool("ScreenShake", GameSettings.ScreenShake);
            saveManager.SetBool("ExScreenShake", GameSettings.ExScreenShake);

            // Video Settings
            saveManager.SetInt("GameScale", GameSettings.GameScale);
            saveManager.SetInt("UIScale", GameSettings.UiScale);
            saveManager.SetInt("ScreenMode", GameSettings.ScreenMode);
            saveManager.SetBool("VerticalSync", GameSettings.VerticalSync);
            saveManager.SetBool("OpaqueHudBg", GameSettings.OpaqueHudBg);

            // Graphics Settings
            saveManager.SetInt("SeqScaleAmplify", GameSettings.SeqScaleAmplify);
            saveManager.SetBool("EnableShadows", GameSettings.EnableShadows);
            saveManager.SetBool("FogEffects", GameSettings.FogEffects);
            saveManager.SetBool("GlobalLights", GameSettings.GlobalLights);
            saveManager.SetBool("ObjectLights", GameSettings.ObjectLights);

            // Audio Settings
            saveManager.SetInt("MusicVolume", GameSettings.MusicVolume);
            saveManager.SetInt("EffectVolume", GameSettings.EffectVolume);
            saveManager.SetBool("ClassicMusic", GameSettings.ClassicMusic);
            saveManager.SetBool("MuteInactive", GameSettings.MuteInactive);
            saveManager.SetBool("HeartBeep", GameSettings.HeartBeep);
            saveManager.SetBool("MutePowerups", GameSettings.MutePowerups);

            // Control Settings
            saveManager.SetFloat("DeadZone", GameSettings.DeadZone);
            saveManager.SetString("Controller", GameSettings.Controller);
            saveManager.SetBool("TriggersScale", GameSettings.TriggersScale);
            saveManager.SetBool("SixButtons", GameSettings.SixButtons);
            saveManager.SetBool("SwapButtons", GameSettings.SwapButtons);
            saveManager.SetBool("OldMovement", GameSettings.OldMovement);
            saveManager.SetBool("DigitalAnalog", GameSettings.DigitalAnalog);
            ControlHandler.SaveButtonMaps(saveManager);

            // On-Screen Control Settings
            saveManager.SetInt("TouchControls", GameSettings.TouchControls);
            saveManager.SetInt("TouchScaling", GameSettings.TouchScaling);
            saveManager.SetInt("TouchOpacity", GameSettings.TouchOpacity);
            saveManager.SetInt("ShadowOpacity", GameSettings.ShadowOpacity);
            saveManager.SetInt("TouchMovement", GameSettings.TouchMovement);
            saveManager.SetBool("TouchTopMiddle", GameSettings.TouchTopMiddle);
            saveManager.SetBool("TouchSticks", GameSettings.TouchSticks);

            // Modifiers Settings
            saveManager.SetInt("EnemyBonusHP", GameSettings.EnemyBonusHP);
            saveManager.SetInt("DamageFactor", GameSettings.DamageFactor);
            saveManager.SetInt("DmgCooldown", GameSettings.DmgCooldown);
            saveManager.SetFloat("MoveSpeedAdded", GameSettings.MoveSpeedAdded);
            saveManager.SetBool("NoHeartDrops", GameSettings.NoHeartDrops);
            saveManager.SetBool("NoDamageLaunch", GameSettings.NoDamageLaunch);
            saveManager.SetBool("MirrorReflects", GameSettings.MirrorReflects);

            // Sword Interact Settings
            saveManager.SetBool("SwGrabNormal", GameSettings.SwGrabNormal);
            saveManager.SetBool("SwGrabWorldItem", GameSettings.SwGrabWorldItem);
            saveManager.SetBool("SwGrabFairy", GameSettings.SwGrabFairy);
            saveManager.SetBool("SwGrabSmallKey", GameSettings.SwGrabSmallKey);
            saveManager.SetBool("SwBoomerang", GameSettings.SwBoomerang);
            saveManager.SetBool("SwSmackBombs", GameSettings.SwSmackBombs);
            saveManager.SetBool("SwMissileBlock", GameSettings.SwMissileBlock);
            saveManager.SetBool("SwBreakPots", GameSettings.SwBreakPots);
            saveManager.SetBool("SwBeamShrubs", GameSettings.SwBeamShrubs);

            // Write the save file.
            saveManager.Save(SettingsFilePath, Values.SaveRetries);
        }
    }
}