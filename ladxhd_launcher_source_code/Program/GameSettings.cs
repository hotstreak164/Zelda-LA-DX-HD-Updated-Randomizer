using System;
using System.Collections.Generic;
using System.IO;

namespace LADXHD_Launcher;

public static class GameSettings
{
    // Bool settings
    public static bool ClassicSword      = false;
    public static bool StoreSavePos      = false;
    public static bool Autosave          = true;
    public static bool ItemsOnRight      = false;
    public static bool EpilepsySafe      = false;
    public static bool VarWidthFont      = false;
    public static bool NoHelperText      = false;
    public static bool DialogSkip        = false;
    public static bool Uncensored        = false;
    public static bool Unmissables       = false;
    public static bool PhotosColor       = false;
    public static bool NoAnimalDamage    = false;
    public static bool ClassicCamera     = false;
    public static bool ModernOverworld   = false;
    public static bool ClassicDungeon    = false;
    public static bool ClassicScaling    = true;
    public static bool CameraLock        = true;
    public static bool SmoothCamera      = true;
    public static bool ScreenShake       = true;
    public static bool ExScreenShake     = false;
    public static bool VerticalSync      = true;
    public static bool OpaqueHudBg       = false;
    public static bool EnableShadows     = true;
    public static bool FogEffects        = true;
    public static bool GlobalLights      = true;
    public static bool ObjectLights      = true;
    public static bool ClassicMusic      = false;
    public static bool MuteInactive      = true;
    public static bool HeartBeep         = true;
    public static bool MutePowerups      = false;
    public static bool TriggersScale     = false;
    public static bool SixButtons        = false;
    public static bool SwapButtons       = false;
    public static bool OldMovement       = false;
    public static bool DigitalAnalog     = false;
    public static bool TouchTopMiddle    = false;
    public static bool TouchSticks       = false;
    public static bool NoHeartDrops      = false;
    public static bool NoDamageLaunch    = false;
    public static bool MirrorReflects    = false;
    public static bool SwGrabNormal      = true;
    public static bool SwGrabWorldItem   = false;
    public static bool SwGrabFairy       = false;
    public static bool SwGrabSmallKey    = false;
    public static bool SwBoomerang       = false;
    public static bool SwSmackBombs      = false;
    public static bool SwMissileBlock    = false;
    public static bool SwBreakPots       = false;
    public static bool SwBeamShrubs      = false;

    // Int settings
    public static int CurrentLanguage    = 0;
    public static int CurrentSubLanguage = 0;
    public static int MenuBorder         = 0;
    public static int LastSavePos        = 0;
    public static int MapTeleport        = 0;
    public static int ClassicBorder      = 1;
    public static int GameScale          = 6;
    public static int UiScale            = 11;
    public static int ScreenMode         = 0;
    public static int SeqScaleAmplify    = 0;
    public static int MusicVolume        = 100;
    public static int EffectVolume       = 100;
    public static int TouchControls      = 1;
    public static int TouchMovement      = 0;
    public static int TouchOpacity       = 30;
    public static int ShadowOpacity      = 15;
    public static int TouchScaling       = 10;
    public static int EnemyBonusHP       = 0;
    public static int DamageFactor       = 4;
    public static int DmgCooldown        = 16;

    // Float settings
    public static float ClassicAlpha     = 1.00f;
    public static float DeadZone         = 0.10f;
    public static float MoveSpeedAdded   = 0f;

    // String settings
    public static string Controller      = "XBox";

    private static string GetSettingsPath(string gameExeFolder)
    {
        string portable = Path.Combine(gameExeFolder, "portable.txt");
        if (File.Exists(portable))
            return Path.Combine(gameExeFolder, "settings");

        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, "Zelda_LA", "settings");
    }

    public static void Load(string gameDirectory)
    {
        string path = GetSettingsPath(gameDirectory);

        System.Diagnostics.Debug.WriteLine(gameDirectory);
        System.Diagnostics.Debug.WriteLine(path);

        if (!File.Exists(path))
            return;
        else
            System.Diagnostics.Debug.WriteLine("success");

        foreach (string line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            // split into 3 parts: type, key, value
            string[] parts = line.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) continue;

            string type  = parts[0];
            string key   = parts[1];
            string value = parts[2].Trim();

            try
            {
                switch (type)
                {
                    case "b": ApplyBool(key, value);   break;
                    case "i": ApplyInt(key, value);    break;
                    case "f": ApplyFloat(key, value);  break;
                    case "s": ApplyString(key, value); break;
                }
            }
            catch
            {
                // skip malformed lines
            }
        }
    }

    private static void ApplyBool(string key, string value)
    {
        bool v = value.Equals("True", StringComparison.OrdinalIgnoreCase);
        switch (key)
        {
            case "ClassicSword":    ClassicSword    = v; break;
            case "StoreSavePos":    StoreSavePos    = v; break;
            case "Autosave":        Autosave        = v; break;
            case "ItemsOnRight":    ItemsOnRight    = v; break;
            case "EpilepsySafe":    EpilepsySafe    = v; break;
            case "VarWidthFont":    VarWidthFont    = v; break;
            case "NoHelperText":    NoHelperText    = v; break;
            case "DialogSkip":      DialogSkip      = v; break;
            case "Uncensored":      Uncensored      = v; break;
            case "Unmissables":     Unmissables     = v; break;
            case "PhotosColor":     PhotosColor     = v; break;
            case "NoAnimalDamage":  NoAnimalDamage  = v; break;
            case "ClassicCamera":   ClassicCamera   = v; break;
            case "ModernOverworld": ModernOverworld = v; break;
            case "ClassicDungeon":  ClassicDungeon  = v; break;
            case "ClassicScaling":  ClassicScaling  = v; break;
            case "CameraLock":      CameraLock      = v; break;
            case "SmoothCamera":    SmoothCamera    = v; break;
            case "ScreenShake":     ScreenShake     = v; break;
            case "ExScreenShake":   ExScreenShake   = v; break;
            case "VerticalSync":    VerticalSync    = v; break;
            case "OpaqueHudBg":     OpaqueHudBg     = v; break;
            case "EnableShadows":   EnableShadows   = v; break;
            case "FogEffects":      FogEffects      = v; break;
            case "GlobalLights":    GlobalLights    = v; break;
            case "ObjectLights":    ObjectLights    = v; break;
            case "ClassicMusic":    ClassicMusic    = v; break;
            case "MuteInactive":    MuteInactive    = v; break;
            case "HeartBeep":       HeartBeep       = v; break;
            case "MutePowerups":    MutePowerups    = v; break;
            case "TriggersScale":   TriggersScale   = v; break;
            case "SixButtons":      SixButtons      = v; break;
            case "SwapButtons":     SwapButtons     = v; break;
            case "OldMovement":     OldMovement     = v; break;
            case "DigitalAnalog":   DigitalAnalog   = v; break;
            case "TouchTopMiddle":  TouchTopMiddle  = v; break;
            case "TouchSticks":     TouchSticks     = v; break;
            case "NoHeartDrops":    NoHeartDrops    = v; break;
            case "NoDamageLaunch":  NoDamageLaunch  = v; break;
            case "MirrorReflects":  MirrorReflects  = v; break;
            case "SwGrabNormal":    SwGrabNormal    = v; break;
            case "SwGrabWorldItem": SwGrabWorldItem = v; break;
            case "SwGrabFairy":     SwGrabFairy     = v; break;
            case "SwGrabSmallKey":  SwGrabSmallKey  = v; break;
            case "SwBoomerang":     SwBoomerang     = v; break;
            case "SwSmackBombs":    SwSmackBombs    = v; break;
            case "SwMissileBlock":  SwMissileBlock  = v; break;
            case "SwBreakPots":     SwBreakPots     = v; break;
            case "SwBeamShrubs":    SwBeamShrubs    = v; break;
        }
    }

    public static void Save(string gameDirectory)
    {
        string path = GetSettingsPath(gameDirectory);

        // Build a dictionary of values the launcher manages
        var managed = new Dictionary<string, string>
        {
            // Booleans
            { "ClassicSword",     $"b ClassicSword {ClassicSword}" },
            { "StoreSavePos",     $"b StoreSavePos {StoreSavePos}" },
            { "Autosave",         $"b Autosave {Autosave}" },
            { "ItemsOnRight",     $"b ItemsOnRight {ItemsOnRight}" },
            { "EpilepsySafe",     $"b EpilepsySafe {EpilepsySafe}" },
            { "VarWidthFont",     $"b VarWidthFont {VarWidthFont}" },
            { "NoHelperText",     $"b NoHelperText {NoHelperText}" },
            { "DialogSkip",       $"b DialogSkip {DialogSkip}" },
            { "Uncensored",       $"b Uncensored {Uncensored}" },
            { "Unmissables",      $"b Unmissables {Unmissables}" },
            { "PhotosColor",      $"b PhotosColor {PhotosColor}" },
            { "NoAnimalDamage",   $"b NoAnimalDamage {NoAnimalDamage}" },
            { "ClassicCamera",    $"b ClassicCamera {ClassicCamera}" },
            { "ModernOverworld",  $"b ModernOverworld {ModernOverworld}" },
            { "ClassicDungeon",   $"b ClassicDungeon {ClassicDungeon}" },
            { "ClassicScaling",   $"b ClassicScaling {ClassicScaling}" },
            { "CameraLock",       $"b CameraLock {CameraLock}" },
            { "SmoothCamera",     $"b SmoothCamera {SmoothCamera}" },
            { "ScreenShake",      $"b ScreenShake {ScreenShake}" },
            { "ExScreenShake",    $"b ExScreenShake {ExScreenShake}" },
            { "VerticalSync",     $"b VerticalSync {VerticalSync}" },
            { "OpaqueHudBg",      $"b OpaqueHudBg {OpaqueHudBg}" },
            { "EnableShadows",    $"b EnableShadows {EnableShadows}" },
            { "FogEffects",       $"b FogEffects {FogEffects}" },
            { "GlobalLights",     $"b GlobalLights {GlobalLights}" },
            { "ObjectLights",     $"b ObjectLights {ObjectLights}" },
            { "ClassicMusic",     $"b ClassicMusic {ClassicMusic}" },
            { "MuteInactive",     $"b MuteInactive {MuteInactive}" },
            { "HeartBeep",        $"b HeartBeep {HeartBeep}" },
            { "MutePowerups",     $"b MutePowerups {MutePowerups}" },
            { "TriggersScale",    $"b TriggersScale {TriggersScale}" },
            { "SixButtons",       $"b SixButtons {SixButtons}" },
            { "SwapButtons",      $"b SwapButtons {SwapButtons}" },
            { "OldMovement",      $"b OldMovement {OldMovement}" },
            { "DigitalAnalog",    $"b DigitalAnalog {DigitalAnalog}" },
            { "TouchTopMiddle",   $"b TouchTopMiddle {TouchTopMiddle}" },
            { "TouchSticks",      $"b TouchSticks {TouchSticks}" },
            { "NoHeartDrops",     $"b NoHeartDrops {NoHeartDrops}" },
            { "NoDamageLaunch",   $"b NoDamageLaunch {NoDamageLaunch}" },
            { "MirrorReflects",   $"b MirrorReflects {MirrorReflects}" },
            { "SwGrabNormal",     $"b SwGrabNormal {SwGrabNormal}" },
            { "SwGrabWorldItem",  $"b SwGrabWorldItem {SwGrabWorldItem}" },
            { "SwGrabFairy",      $"b SwGrabFairy {SwGrabFairy}" },
            { "SwGrabSmallKey",   $"b SwGrabSmallKey {SwGrabSmallKey}" },
            { "SwBoomerang",      $"b SwBoomerang {SwBoomerang}" },
            { "SwSmackBombs",     $"b SwSmackBombs {SwSmackBombs}" },
            { "SwMissileBlock",   $"b SwMissileBlock {SwMissileBlock}" },
            { "SwBreakPots",      $"b SwBreakPots {SwBreakPots}" },
            { "SwBeamShrubs",     $"b SwBeamShrubs {SwBeamShrubs}" },

            // Integers
            { "CurrentLanguage",    $"i CurrentLanguage {CurrentLanguage}" },
            { "CurrentSubLanguage", $"i CurrentSubLanguage {CurrentSubLanguage}" },
            { "MenuBorder",         $"i MenuBorder {MenuBorder}" },
            { "LastSavePos",        $"i LastSavePos {LastSavePos}" },
            { "MapTeleport",        $"i MapTeleport {MapTeleport}" },
            { "ClassicBorder",      $"i ClassicBorder {ClassicBorder}" },
            { "GameScale",          $"i GameScale {GameScale}" },
            { "UIScale",            $"i UIScale {UiScale}" },
            { "ScreenMode",         $"i ScreenMode {ScreenMode}" },
            { "SeqScaleAmplify",    $"i SeqScaleAmplify {SeqScaleAmplify}" },
            { "MusicVolume",        $"i MusicVolume {MusicVolume}" },
            { "EffectVolume",       $"i EffectVolume {EffectVolume}" },
            { "TouchControls",      $"i TouchControls {TouchControls}" },
            { "TouchMovement",      $"i TouchMovement {TouchMovement}" },
            { "TouchOpacity",       $"i TouchOpacity {TouchOpacity}" },
            { "ShadowOpacity",      $"i ShadowOpacity {ShadowOpacity}" },
            { "TouchScaling",       $"i TouchScaling {TouchScaling}" },
            { "EnemyBonusHP",       $"i EnemyBonusHP {EnemyBonusHP}" },
            { "DamageFactor",       $"i DamageFactor {DamageFactor}" },
            { "DmgCooldown",        $"i DmgCooldown {DmgCooldown}" },

            // Floats
            { "ClassicAlpha",    $"f ClassicAlpha {ClassicAlpha.ToString(System.Globalization.CultureInfo.InvariantCulture)}" },
            { "DeadZone",        $"f DeadZone {DeadZone.ToString(System.Globalization.CultureInfo.InvariantCulture)}" },
            { "MoveSpeedAdded",  $"f MoveSpeedAdded {MoveSpeedAdded.ToString(System.Globalization.CultureInfo.InvariantCulture)}" },

            // Strings
            { "Controller", $"s Controller {Controller}" },
        };

        // Read existing file, replace managed lines, pass through unknown lines.
        var output = new List<string>();

        if (File.Exists(path))
        {
            foreach (string line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    output.Add(line);
                    continue;
                }

                string[] parts = line.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 && managed.TryGetValue(parts[1], out string newLine))
                {
                    output.Add(newLine);
                    managed.Remove(parts[1]);
                }
                else
                {
                    output.Add(line);
                }
            }
        }

        // Append any managed values that weren't in the file yet
        foreach (var entry in managed.Values)
            output.Add(entry);

        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllLines(path, output);
    }

    private static void ApplyInt(string key, string value)
    {
        if (!int.TryParse(value, out int v)) return;
        switch (key)
        {
            case "CurrentLanguage":     CurrentLanguage      = v; break;
            case "CurrentSubLanguage":  CurrentSubLanguage   = v; break;
            case "MenuBorder":          MenuBorder           = v; break;
            case "LastSavePos":         LastSavePos          = v; break;
            case "MapTeleport":         MapTeleport          = v; break;
            case "ClassicBorder":       ClassicBorder        = v; break;
            case "GameScale":           GameScale            = v; break;
            case "UIScale":             UiScale              = v; break;
            case "ScreenMode":          ScreenMode           = v; break;
            case "SeqScaleAmplify":     SeqScaleAmplify      = v; break;
            case "MusicVolume":         MusicVolume          = v; break;
            case "EffectVolume":        EffectVolume         = v; break;
            case "TouchControls":       TouchControls        = v; break;
            case "TouchMovement":       TouchMovement        = v; break;
            case "TouchOpacity":        TouchOpacity         = v; break;
            case "ShadowOpacity":       ShadowOpacity        = v; break;
            case "TouchScaling":        TouchScaling         = v; break;
            case "EnemyBonusHP":        EnemyBonusHP         = v; break;
            case "DamageFactor":        DamageFactor         = v; break;
            case "DmgCooldown":         DmgCooldown          = v; break;
        }
    }

    private static void ApplyFloat(string key, string value)
    {
        if (!float.TryParse(value, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float v)) return;
        switch (key)
        {
            case "ClassicAlpha":    ClassicAlpha    = v; break;
            case "DeadZone":        DeadZone        = v; break;
            case "MoveSpeedAdded":  MoveSpeedAdded  = v; break;
        }
    }

    private static void ApplyString(string key, string value)
    {
        switch (key)
        {
            case "Controller": Controller = value; break;
        }
    }
}