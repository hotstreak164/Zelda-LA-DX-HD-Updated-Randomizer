using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace LADXHD_Launcher;

public partial class SettingsView : UserControl
{
    private MainWindow? _parent;

    public SettingsView() { InitializeComponent(); }

    public SettingsView(MainWindow parent)
    {
        InitializeComponent();
        _parent = parent;
    }

    private void ModernCamera_Changed(object sender, RoutedEventArgs e)
    {
        if (x_ModernCamera.IsChecked == true)
            x_ClassicCamera.IsChecked = false;
        else
            x_ClassicCamera.IsChecked = true;
    }

    private void ClassicCamera_Changed(object sender, RoutedEventArgs e)
    {
        if (x_ClassicCamera.IsChecked == true)
            x_ModernCamera.IsChecked = false;
        else
            x_ModernCamera.IsChecked = true;
    }

    private void DamageFactorChanged(object sender, NumericUpDownValueChangedEventArgs e)
    {
        // Translate the damage factor to what the game expects.
        if (n_DamageFactor.Value == null) return;
        decimal rounded = Math.Round(n_DamageFactor.Value.Value * 4) / 4;
        if (n_DamageFactor.Value != rounded)
            n_DamageFactor.Value = rounded;
    }

    public void LoadValues(int maxGameScale = 21)
    {
        // Suppress the sound effects so the checkbox sound doesn't fire a bunch of times.
        XnbAudio.SuppressSound = true;

        // Update the maximum game scale from the "advanced" file.
        n_GameScale.Maximum = (decimal)(maxGameScale + 1);

        // Video Settings
        n_GameScale.Value                   = (decimal)GameSettings.GameScale;
        n_UiScale.Value                     = (decimal)GameSettings.UiScale;
        x_VerticalSync.IsChecked            = GameSettings.VerticalSync;
        x_OpaqueHudBg.IsChecked             = GameSettings.OpaqueHudBg;
        c_ScreenMode.SelectedIndex          = GameSettings.ScreenMode;

        // Graphics Settings
        x_EnableShadows.IsChecked           = GameSettings.EnableShadows;
        x_FogEffects.IsChecked              = GameSettings.FogEffects;
        x_GlobalLights.IsChecked            = GameSettings.GlobalLights;
        x_ObjectLights.IsChecked            = GameSettings.ObjectLights;
        x_ScreenShake.IsChecked             = GameSettings.ScreenShake;
        x_ExScreenShake.IsChecked           = GameSettings.ExScreenShake;
        n_SeqScaleAmplify.Value             = (decimal)GameSettings.SeqScaleAmplify;

        // Audio Settings
        n_MusicVolume.Value                 = (decimal)GameSettings.MusicVolume;
        n_EffectVolume.Value                = (decimal)GameSettings.EffectVolume;
        x_ClassicMusic.IsChecked            = GameSettings.ClassicMusic;
        x_MuteInactive.IsChecked            = GameSettings.MuteInactive;
        x_HeartBeep.IsChecked               = GameSettings.HeartBeep;
        x_MutePowerups.IsChecked            = GameSettings.MutePowerups;

        // Control Settings
        n_DeadZone.Value                   = (decimal)GameSettings.DeadZone;
        c_Controller.SelectedIndex         = GameSettings.Controller switch
        {
            "Playstation" => 1,
            "Nintendo"    => 2,
            _             => 0
        };
        x_TriggersScale.IsChecked          = GameSettings.TriggersScale;
        x_SixButtons.IsChecked             = GameSettings.SixButtons;
        x_OldMovement.IsChecked            = GameSettings.OldMovement;
        x_DigitalAnalog.IsChecked          = GameSettings.DigitalAnalog;
        x_SwapButtons.IsChecked            = GameSettings.SwapButtons;

        // Game Settings
        c_CurrentLanguage.SelectedIndex    = GameSettings.CurrentLanguage;
        c_CurrentSubLanguage.SelectedIndex = GameSettings.CurrentSubLanguage;
        x_ClassicSword.IsChecked           = GameSettings.ClassicSword;
        x_StoreSavePos.IsChecked           = GameSettings.StoreSavePos;
        x_Autosave.IsChecked               = GameSettings.Autosave;
        x_ItemsOnRight.IsChecked           = GameSettings.ItemsOnRight;
        x_EpilepsySafe.IsChecked           = GameSettings.EpilepsySafe;

        // Redux Settings
        x_VarWidthFont.IsChecked           = GameSettings.VarWidthFont;
        x_NoHelperText.IsChecked           = GameSettings.NoHelperText;
        x_DialogSkip.IsChecked             = GameSettings.DialogSkip;
        x_Uncensored.IsChecked             = GameSettings.Uncensored;
        x_Unmissables.IsChecked            = GameSettings.Unmissables;
        x_PhotosColor.IsChecked            = GameSettings.PhotosColor;
        c_MapTeleport.SelectedIndex        = GameSettings.MapTeleport;
        x_NoAnimalDamage.IsChecked         = GameSettings.NoAnimalDamage;

        // Camera Settings
        x_ModernCamera.IsChecked           = !GameSettings.ClassicCamera;
        x_ModernOverworld.IsChecked        = GameSettings.ModernOverworld;
        x_ClassicCamera.IsChecked          = GameSettings.ClassicCamera;
        x_ClassicDungeon.IsChecked         = GameSettings.ClassicDungeon;
        c_ClassicBorder.SelectedIndex      = GameSettings.ClassicBorder;
        n_ClassicAlpha.Value               = (decimal)GameSettings.ClassicAlpha;
        x_ClassicScaling.IsChecked         = GameSettings.ClassicScaling;
        x_SmoothCamera.IsChecked           = GameSettings.SmoothCamera;

        // Modifier Settings
        n_EnemyBonusHP.Value               = (decimal)GameSettings.EnemyBonusHP;
        n_MoveSpeedAdded.Value             = (decimal)GameSettings.MoveSpeedAdded;
        n_DamageFactor.Value               = (decimal)GameSettings.DamageFactor / 4;
        n_DmgCooldown.Value                = (decimal)GameSettings.DmgCooldown;
        x_NoHeartDrops.IsChecked           = GameSettings.NoHeartDrops;
        x_NoDamageLaunch.IsChecked         = GameSettings.NoDamageLaunch;
        x_MirrorReflects.IsChecked         = GameSettings.MirrorReflects;

        // Sword Modifier Settings
        x_SwGrabNormal.IsChecked           = GameSettings.SwGrabNormal;
        x_SwGrabWorldItem.IsChecked        = GameSettings.SwGrabWorldItem;
        x_SwGrabFairy.IsChecked            = GameSettings.SwGrabFairy;
        x_SwGrabSmallKey.IsChecked         = GameSettings.SwGrabSmallKey;
        x_SwBoomerang.IsChecked            = GameSettings.SwBoomerang;
        x_SwSmackBombs.IsChecked           = GameSettings.SwSmackBombs;
        x_SwMissileBlock.IsChecked         = GameSettings.SwMissileBlock;
        x_SwBreakPots.IsChecked            = GameSettings.SwBreakPots;
        x_SwBeamShrubs.IsChecked           = GameSettings.SwBeamShrubs;

        // Ok it's fine now.
        XnbAudio.SuppressSound = false;
    }

    private void SaveValues()
    {
        // Video Settings
        GameSettings.GameScale          = (int)(n_GameScale.Value ?? 0);
        GameSettings.UiScale            = (int)(n_UiScale.Value ?? 0);
        GameSettings.VerticalSync       = x_VerticalSync.IsChecked == true;
        GameSettings.OpaqueHudBg        = x_OpaqueHudBg.IsChecked == true;
        GameSettings.ScreenMode         = c_ScreenMode.SelectedIndex;

        // Graphics Settings
        GameSettings.EnableShadows      = x_EnableShadows.IsChecked == true;
        GameSettings.FogEffects         = x_FogEffects.IsChecked == true;
        GameSettings.GlobalLights       = x_GlobalLights.IsChecked == true;
        GameSettings.ObjectLights       = x_ObjectLights.IsChecked == true;
        GameSettings.ScreenShake        = x_ScreenShake.IsChecked == true;
        GameSettings.ExScreenShake      = x_ExScreenShake.IsChecked == true;
        GameSettings.SeqScaleAmplify    = (int)(n_SeqScaleAmplify.Value ?? 0);

        // Audio Settings
        GameSettings.MusicVolume        = (int)(n_MusicVolume.Value ?? 0);
        GameSettings.EffectVolume       = (int)(n_EffectVolume.Value ?? 0);
        GameSettings.ClassicMusic       = x_ClassicMusic.IsChecked == true;
        GameSettings.MuteInactive       = x_MuteInactive.IsChecked == true;
        GameSettings.HeartBeep          = x_HeartBeep.IsChecked == true;
        GameSettings.MutePowerups       = x_MutePowerups.IsChecked == true;

        // Control Settings
        GameSettings.DeadZone           = (float)(n_DeadZone.Value ?? 0);
        GameSettings.Controller         = c_Controller.SelectedIndex switch
        {
            1 => "Playstation",
            2 => "Nintendo",
            _ => "XBox"
        };
        GameSettings.TriggersScale      = x_TriggersScale.IsChecked == true;
        GameSettings.SixButtons         = x_SixButtons.IsChecked == true;
        GameSettings.OldMovement        = x_OldMovement.IsChecked == true;
        GameSettings.DigitalAnalog      = x_DigitalAnalog.IsChecked == true;
        GameSettings.SwapButtons        = x_SwapButtons.IsChecked == true;

        // Game Settings
        GameSettings.CurrentLanguage    = c_CurrentLanguage.SelectedIndex;
        GameSettings.CurrentSubLanguage = c_CurrentSubLanguage.SelectedIndex;
        GameSettings.ClassicSword       = x_ClassicSword.IsChecked == true;
        GameSettings.StoreSavePos       = x_StoreSavePos.IsChecked == true;
        GameSettings.Autosave           = x_Autosave.IsChecked == true;
        GameSettings.ItemsOnRight       = x_ItemsOnRight.IsChecked == true;
        GameSettings.EpilepsySafe       = x_EpilepsySafe.IsChecked == true;

        // Redux Settings
        GameSettings.VarWidthFont       = x_VarWidthFont.IsChecked == true;
        GameSettings.NoHelperText       = x_NoHelperText.IsChecked == true;
        GameSettings.DialogSkip         = x_DialogSkip.IsChecked == true;
        GameSettings.Uncensored         = x_Uncensored.IsChecked == true;
        GameSettings.Unmissables        = x_Unmissables.IsChecked == true;
        GameSettings.PhotosColor        = x_PhotosColor.IsChecked == true;
        GameSettings.MapTeleport        = c_MapTeleport.SelectedIndex;
        GameSettings.NoAnimalDamage     = x_NoAnimalDamage.IsChecked == true;

        // Camera Settings
        GameSettings.ModernOverworld    = x_ModernOverworld.IsChecked == true;
        GameSettings.ClassicCamera      = x_ClassicCamera.IsChecked == true;
        GameSettings.ClassicDungeon     = x_ClassicDungeon.IsChecked == true;
        GameSettings.ClassicBorder      = c_ClassicBorder.SelectedIndex;
        GameSettings.ClassicAlpha       = (float)(n_ClassicAlpha.Value ?? 0);
        GameSettings.ClassicScaling     = x_ClassicScaling.IsChecked == true;
        GameSettings.CameraLock         = x_CameraLock.IsChecked == true;
        GameSettings.SmoothCamera       = x_SmoothCamera.IsChecked == true;

        // Modifier Settings
        GameSettings.EnemyBonusHP       = (int)(n_EnemyBonusHP.Value ?? 0);
        GameSettings.MoveSpeedAdded     = (float)(n_MoveSpeedAdded.Value ?? 0);
        GameSettings.DamageFactor       = (int)(n_DamageFactor.Value * 4 ?? 0);
        GameSettings.DmgCooldown        = (int)(n_DmgCooldown.Value ?? 0);
        GameSettings.NoHeartDrops       = x_NoHeartDrops.IsChecked == true;
        GameSettings.NoDamageLaunch     = x_NoDamageLaunch.IsChecked == true;
        GameSettings.MirrorReflects     = x_MirrorReflects.IsChecked == true;

        // Sword Modifier Settings
        GameSettings.SwGrabNormal       = x_SwGrabNormal.IsChecked == true;
        GameSettings.SwGrabWorldItem    = x_SwGrabWorldItem.IsChecked == true;
        GameSettings.SwGrabFairy        = x_SwGrabFairy.IsChecked == true;
        GameSettings.SwGrabSmallKey     = x_SwGrabSmallKey.IsChecked == true;
        GameSettings.SwBoomerang        = x_SwBoomerang.IsChecked == true;
        GameSettings.SwSmackBombs       = x_SwSmackBombs.IsChecked == true;
        GameSettings.SwMissileBlock     = x_SwMissileBlock.IsChecked == true;
        GameSettings.SwBreakPots        = x_SwBreakPots.IsChecked == true;
        GameSettings.SwBeamShrubs       = x_SwBeamShrubs.IsChecked == true;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveValues();
        GameSettings.Save(AppContext.BaseDirectory);
        _parent?.ShowSavedNotification();
        _parent?.NavigateTo(_parent.HomeView);
        XnbAudio.PlayXnbSound(XnbAudio.SoundSave);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        _parent?.NavigateTo(_parent.HomeView);
        XnbAudio.PlayXnbSound(XnbAudio.SoundClose);
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        _parent?.Close();
    }
}