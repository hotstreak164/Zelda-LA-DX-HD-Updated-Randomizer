namespace ProjectZ.InGame.Things
{
    class GameSettings
    {
        // Game Settings
        public static int     MenuBorder      =  0;
        public static bool    ClassicSword    =  false;
        public static bool    StoreSavePos    =  false;
        public static int     LastSavePos     =  0;
        public static bool    Autosave        =  true;
        public static bool    ItemsOnRight    =  false;
        public static bool    EpilepsySafe    =  false;

        // Redux Settings
        public static bool    VarWidthFont    =  false;
        public static bool    NoHelperText    =  false;
        public static bool    DialogSkip      =  false;
        public static bool    Uncensored      =  false;
        public static bool    Unmissables     =  false;
        public static bool    PhotosColor     =  false;
        public static bool    NoAnimalDamage  =  false;
        public static bool    DungeonTeleport =  false;

        // Camera Settings
        public static bool    ClassicCamera   =  false;
        public static bool    ModernOverworld =  false;
        public static bool    ClassicDungeon  =  false;
        public static int     ClassicBorders  =  0;
        public static float   ClassicAlpha    =  1.00f;
        public static bool    CameraLock      =  true;
        public static bool    SmoothCamera    =  true;
        public static bool    ScreenShake     =  true;
        public static bool    ExScreenShake   =  false;

        // Video Settings
        public static int     GameScale       =  Game1.MaxGameScale + 1;
        public static int     UiScale         =  11;
        public static bool    IsFullscreen    =  false;
        public static bool    ExFullscreen    =  false;
        public static bool    GlobalLights    =  true;
        public static bool    ObjectLights    =  true;
        public static bool    EnableShadows   =  true;
        public static bool    VerticalSync    =  true;

        // Audio Settings
        private static int    _musicVolume    =  100;
        private static int    _effectVolume   =  100;
        public static bool    ClassicMusic    =  false;
        public static bool    MuteInactive    =  true;
        public static bool    HeartBeep       =  true;
        public static bool    MutePowerups    =  false;

        // Control Settings
        public static float   DeadZone        =  0.10f;
        public static string  Controller      =  "XBox";
        public static bool    TriggersScale   =  false;
        public static bool    SixButtons      =  false;
        public static bool    SwapButtons     =  false;
        public static bool    OldMovement     =  false;
        public static bool    DigitalAnalog   =  false;

        // Modifiers Settings
        public static int     EnemyBonusHP    =  0;
        public static int     DamageFactor    =  4;
        public static int     DmgCooldown     =  16;
        public static float   MoveSpeedAdded  =  0;
        public static bool    NoHeartDrops    =  false;
        public static bool    NoDamageLaunch  =  false;

        // Sword Collection
        public static bool    SwGrabNormal    =  true;
        public static bool    SwGrabWorldItem =  false;
        public static bool    SwGrabFairy     =  false;
        public static bool    SwGrabSmallKey  =  false;
        public static bool    SwBoomerang     =  false;
        public static bool    SwSmackBombs    =  false;
        public static bool    SwMissileBlock  =  false;
        public static bool    SwBreakPots     =  false;
        public static bool    SwBeamShrubs    =  false;

        public static int MusicVolume
        {
            get => _musicVolume;
            set { _musicVolume = value; Game1.GbsPlayer.SetVolume(value / 100.0f); }
        }

        public static int EffectVolume
        {
            get => _effectVolume;
            set { _effectVolume = value; }
        }

        public static void RestoreDefaults()
        {
            // Game Settings
            MenuBorder      =  0;
            ClassicSword    =  false;
            StoreSavePos    =  false;
            LastSavePos     =  0;
            Autosave        =  true;
            ItemsOnRight    =  false;
            EpilepsySafe    =  false;

            // Redux Settings
            VarWidthFont    =  false;
            NoHelperText    =  false;
            DialogSkip      =  false;
            Uncensored      =  false;
            Unmissables     =  false;
            PhotosColor     =  false;
            NoAnimalDamage  =  false;
            DungeonTeleport =  false;

            // Camera Settings
            ClassicCamera   =  false;
            ModernOverworld =  false;
            ClassicDungeon  =  false;
            ClassicBorders  =  0;
            ClassicAlpha    =  1.00f;
            CameraLock      =  true;
            SmoothCamera    =  true;
            ScreenShake     =  true;
            ExScreenShake   =  false;

            // Video Settings
            GameScale       =  Game1.MaxGameScale + 1;
            UiScale         =  11;
            IsFullscreen    =  false;
            ExFullscreen    =  false;
            GlobalLights    =  true;
            ObjectLights    =  true;
            EnableShadows   =  true;
            VerticalSync    =  true;

            // Audio Settings
            ClassicMusic    =  false;
            MuteInactive    =  true;
            HeartBeep       =  true;
            MutePowerups    =  false;

            // Control Settings
            DeadZone        =  0.10f;
            Controller      =  "XBox";
            TriggersScale   =  false;
            SixButtons      =  false;
            SwapButtons     =  false;
            OldMovement     =  false;
            DigitalAnalog   =  false;

            // Modifiers Settings
            EnemyBonusHP    =  0;
            DamageFactor    =  4;
            DmgCooldown     =  16;
            MoveSpeedAdded  =  0;
            NoHeartDrops    =  false;
            NoDamageLaunch  =  false;

            // Sword Collection
            SwGrabNormal    =  true;
            SwGrabWorldItem =  false;
            SwGrabFairy     =  false;
            SwGrabSmallKey  =  false;
            SwBoomerang     =  false;
            SwSmackBombs    =  false;
            SwMissileBlock  =  false;
            SwBreakPots     =  false;
            SwBeamShrubs    =  false;
        }
    }
}