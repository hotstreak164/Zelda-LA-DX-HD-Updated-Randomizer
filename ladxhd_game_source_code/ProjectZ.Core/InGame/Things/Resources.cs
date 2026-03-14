using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Screens;
#if ANDROID
using Android.App;
using Android.Content.Res;
#endif

namespace ProjectZ.InGame.Things
{
    public class Resources
    {
        public class Texture
        {
            public string Name;
            public Texture2D SprTexture;

            public Texture(string name)
            {
                Name = name;
            }
        }
        // A list of all currently supported language codes.
        static string[] _languageList = { "chn", "deu", "esp", "fre", "ind", "ita", "por", "rus" };

        public static Effect RoundedCornerEffect;
        public static Effect BlurEffect;
        public static Effect RoundedCornerBlurEffect;
        public static Effect BlurEffectV;
        public static Effect BlurEffectH;
        public static Effect BBlurEffectV;
        public static Effect BBlurEffectH;
        public static Effect BBlurMapping;
        public static Effect FullShadowEffect;
        public static Effect SaturationEffect;
        public static Effect WobbleEffect;
        public static Effect CircleShader;
        public static Effect LightShader;
        public static Effect LightFadeShader;
        public static Effect ThanosShader;

        // some sprites need different parameters set
        // we try to use as little different sprites effects as possible
        public static SpriteShader DamageSpriteShader0;
        public static SpriteShader DamageSpriteShader1;
        public static SpriteShader CloudShader;
        public static SpriteShader ThanosSpriteShader0;
        public static SpriteShader ThanosSpriteShader1;
        public static SpriteShader WindFishShader;
        public static SpriteShader ColorShader;
        public static SpriteShader ShockShader0;
        public static SpriteShader ShockShader1;

    #if ANDROID
        // The textures that holds the sprites for the onscreen buttons for Android screens
        public static Texture2D SprButtons;
    #endif

        public static SpriteFont EditorFont, EditorFontMonoSpace, EditorFontSmallMonoSpace;
        public static SpriteFont GameHeaderFont;
        public static SpriteFont FontCredits, FontCreditsHeader;
        public static SpriteFont smallFont, smallFont_redux, smallFont_vwf, smallFont_vwf_redux;
        public static BitmapFont smallFont_chn, smallFont_chn_redux;

        public static BitmapFont ChinaFont => GameSettings.Uncensored ? smallFont_chn_redux : smallFont_chn;

        public static SpriteFont GameFont 
        {
            get
            {
                // Other languages use the "normal" fonts provided. Depending on certain settings there is variations.
                return (GameSettings.VarWidthFont, GameSettings.Uncensored) switch
                {
                    (true,  true)  => smallFont_vwf_redux,
                    (true,  false) => smallFont_vwf,
                    (false, true)  => smallFont_redux,
                    (false, false) => smallFont
                };
            }
        }
        public static Texture2D EditorEyeOpen, EditorEyeClosed, EditorIconDelete;
        public static Texture2D SprWhite, SprTiledBlock, SprObjectsAnimated, SprNpCs, SprNpCsRedux;
        public static Texture2D SprEnemies, SprMidBoss, SprNightmares;
        public static Texture2D SprShadow;
        public static Texture2D SprBlurTileset;
        public static Texture2D SprLink, SprLinkCloak;
        public static Texture2D SprGameSequences;
        public static Texture2D SprGameSequencesFinal;
        public static Texture2D SprFog;
        public static Texture2D SprLight;
        public static Texture2D SprLightRoomH;
        public static Texture2D SprLightRoomV;
        public static Texture2D NoiseTexture;
        public static Texture2D SprIconOptions, SprIconErase, SprIconCopy, EditorIconEdit, EditorIconSelect;
        public static Texture2D sgbBorder;

        public static List<Texture> TextureList = new();
        public static List<Texture> ReloadQueue = new();

        public static Dictionary<string, int> TilesetSizes = new();
        public static Dictionary<string, SoundEffect> SoundEffects = new();

        public static int GameFontHeight = 10;
        public static int EditorFontHeight;

        // Fields from this point on are affected by language and/or redux options.
        public static Texture2D SprPhotosEng, SprPhotosChn, SprPhotosDeu, SprPhotosEsp, SprPhotosFre, SprPhotosInd, SprPhotosIta, SprPhotosPor, SprPhotosRus;
        public static Texture2D SprPhotosEngRedux, SprPhotosChnRedux, SprPhotosDeuRedux, SprPhotosEspRedux, SprPhotosFreRedux, SprPhotosIndRedux, SprPhotosItaRedux, SprPhotosPorRedux, SprPhotosRusRedux;

        public static Texture2D SprObjectsEng, SprObjectsChn, SprObjectsDeu, SprObjectsEsp, SprObjectsFre, SprObjectsInd, SprObjectsIta, SprObjectsPor, SprObjectsRus;

        public static Texture2D SprObjects
        {
            get
            {
                return (Game1.LanguageManager.CurrentLanguageCode) switch
                {
                    "chn" => SprObjectsChn,
                    "deu" => SprObjectsDeu,
                    "esp" => SprObjectsEsp,
                    "fre" => SprObjectsFre,
                    "ind" => SprObjectsInd,
                    "ita" => SprObjectsIta,
                    "por" => SprObjectsPor,
                    "rus" => SprObjectsRus,
                    _     => SprObjectsEng
                };
            }
        }

        public static Texture2D SprMiniMapEng, SprMiniMapChn, SprMiniMapDeu, SprMiniMapEsp, SprMiniMapFre, SprMiniMapInd, SprMiniMapIta, SprMiniMapPor, SprMiniMapRus;
        public static Texture2D SprMiniMap
        {
            get
            {
                return (Game1.LanguageManager.CurrentLanguageCode) switch
                {
                    "chn" => SprMiniMapChn,
                    "deu" => SprMiniMapDeu,
                    "esp" => SprMiniMapEsp,
                    "fre" => SprMiniMapFre,
                    "ind" => SprMiniMapInd,
                    "ita" => SprMiniMapIta,
                    "por" => SprMiniMapPor,
                    "rus" => SprMiniMapRus,
                    _     => SprMiniMapEng
                };
            }
        }

        public static Texture2D SprItemEng, SprItemChn, SprItemDeu, SprItemEsp, SprItemFre, SprItemInd, SprItemIta, SprItemPor, SprItemRus;
        public static Texture2D SprItemEngRedux, SprItemChnRedux, SprItemDeuRedux, SprItemEspRedux, SprItemFreRedux, SprItemIndRedux, SprItemItaRedux, SprItemPorRedux, SprItemRusRedux;
        public static Texture2D SprItem
        {
            get
            {
                return (Game1.LanguageManager.CurrentLanguageCode, GameSettings.Uncensored) switch
                {
                    ("chn", false) => SprItemChn,
                    ("deu", false) => SprItemDeu,
                    ("esp", false) => SprItemEsp,
                    ("fre", false) => SprItemFre,
                    ("ind", false) => SprItemInd,
                    ("ita", false) => SprItemIta,
                    ("por", false) => SprItemPor,
                    ("rus", false) => SprItemRus,
                    (_, false)     => SprItemEng,

                    ("chn", true)  => SprItemChnRedux,
                    ("deu", true)  => SprItemDeuRedux,
                    ("esp", true)  => SprItemEspRedux,
                    ("fre", true)  => SprItemFreRedux,
                    ("ind", true)  => SprItemIndRedux,
                    ("ita", true)  => SprItemItaRedux,
                    ("por", true)  => SprItemPorRedux,
                    ("rus", true)  => SprItemRusRedux,
                    (_, true)      => SprItemEngRedux,
                };
            }
        }
        public static Dictionary<string, DictAtlasEntry> SpriteAtlas = new();
        public static Dictionary<string, DictAtlasEntry> SpriteAtlasChn = new();
        public static Dictionary<string, DictAtlasEntry> SpriteAtlasDeu = new();
        public static Dictionary<string, DictAtlasEntry> SpriteAtlasEsp = new();
        public static Dictionary<string, DictAtlasEntry> SpriteAtlasFre = new();
        public static Dictionary<string, DictAtlasEntry> SpriteAtlasInd = new();
        public static Dictionary<string, DictAtlasEntry> SpriteAtlasIta = new();
        public static Dictionary<string, DictAtlasEntry> SpriteAtlasPor = new();
        public static Dictionary<string, DictAtlasEntry> SpriteAtlasRus = new();

        public static Dictionary<string, DictAtlasEntry> SpriteAtlasRedux = new();
        public static Dictionary<string, DictAtlasEntry> SpriteAtlasChnRedux = new();
        public static Dictionary<string, DictAtlasEntry> SpriteAtlasDeuRedux = new();
        public static Dictionary<string, DictAtlasEntry> SpriteAtlasEspRedux = new();
        public static Dictionary<string, DictAtlasEntry> SpriteAtlasFreRedux = new();
        public static Dictionary<string, DictAtlasEntry> SpriteAtlasIndRedux = new();
        public static Dictionary<string, DictAtlasEntry> SpriteAtlasItaRedux = new();
        public static Dictionary<string, DictAtlasEntry> SpriteAtlasPorRedux = new();
        public static Dictionary<string, DictAtlasEntry> SpriteAtlasRusRedux = new();

        public enum AtlasLanguage { English, Chinese, German, Spanish, French, Indonesian, Italian, Portuguese, Russian }
        public enum AtlasVariant { Default, Redux }

        private static (AtlasLanguage, AtlasVariant) ParseAtlasTags(string filePath)
        {
            string[] parts = Path.GetFileNameWithoutExtension(filePath)
                .ToLowerInvariant()
                .Split('_');

            AtlasLanguage lang = AtlasLanguage.English;
            if (parts.Contains("chn")) lang = AtlasLanguage.Chinese;
            else if (parts.Contains("deu")) lang = AtlasLanguage.German;
            else if (parts.Contains("esp")) lang = AtlasLanguage.Spanish;
            else if (parts.Contains("fre")) lang = AtlasLanguage.French;
            else if (parts.Contains("ind")) lang = AtlasLanguage.Indonesian;
            else if (parts.Contains("ita")) lang = AtlasLanguage.Italian;
            else if (parts.Contains("por")) lang = AtlasLanguage.Portuguese;
            else if (parts.Contains("rus")) lang = AtlasLanguage.Russian;

            AtlasVariant variant = parts.Contains("redux") 
                ? AtlasVariant.Redux 
                : AtlasVariant.Default;

            return (lang, variant);
        }

        private static readonly Dictionary<(AtlasLanguage, AtlasVariant), Dictionary<string, DictAtlasEntry>> Atlases = new()
        {
            {(AtlasLanguage.English,    AtlasVariant.Default), SpriteAtlas},
            {(AtlasLanguage.English,    AtlasVariant.Redux),   SpriteAtlasRedux},
            {(AtlasLanguage.Chinese,    AtlasVariant.Default), SpriteAtlasChn},
            {(AtlasLanguage.Chinese,    AtlasVariant.Redux),   SpriteAtlasChnRedux},
            {(AtlasLanguage.German,     AtlasVariant.Default), SpriteAtlasDeu},
            {(AtlasLanguage.German,     AtlasVariant.Redux),   SpriteAtlasDeuRedux},
            {(AtlasLanguage.Spanish,    AtlasVariant.Default), SpriteAtlasEsp},
            {(AtlasLanguage.Spanish,    AtlasVariant.Redux),   SpriteAtlasEspRedux},
            {(AtlasLanguage.French,     AtlasVariant.Default), SpriteAtlasFre},
            {(AtlasLanguage.French,     AtlasVariant.Redux),   SpriteAtlasFreRedux},
            {(AtlasLanguage.Indonesian, AtlasVariant.Default), SpriteAtlasInd},
            {(AtlasLanguage.Indonesian, AtlasVariant.Redux),   SpriteAtlasIndRedux},
            {(AtlasLanguage.Italian,    AtlasVariant.Default), SpriteAtlasIta},
            {(AtlasLanguage.Italian,    AtlasVariant.Redux),   SpriteAtlasItaRedux},
            {(AtlasLanguage.Portuguese, AtlasVariant.Default), SpriteAtlasPor},
            {(AtlasLanguage.Portuguese, AtlasVariant.Redux),   SpriteAtlasPorRedux},
            {(AtlasLanguage.Russian,    AtlasVariant.Default), SpriteAtlasRus},
            {(AtlasLanguage.Russian,    AtlasVariant.Redux),   SpriteAtlasRusRedux},
        };

        public static void LoadIntro(GraphicsDevice graphics, ContentManager content)
        {
            SprWhite = new Texture2D(graphics, 1, 1);
            SprWhite.SetData(new[] { Color.White });

        #if ANDROID
            // Load the button textures for Android.
            LoadTexture(out SprButtons, Path.Combine(Values.PathContentFolder, "Buttons", "buttons.png"));
        #endif

            // base first
            var introPath = GameFS.NormalizePath(Path.Combine(Values.PathContentFolder, "Intro"));
            LoadTexturesFromFolder(introPath, false);

            // mods second
            var graphicsModsPath = GameFS.NormalizePath(Values.PathGraphicsMods);

            if (GameFS.IsDirectory(graphicsModsPath))
            {
                var introDirs = GameFS.EnumerateDirectories(graphicsModsPath, recursive: true, acceptDirectory: dir => string.Equals(dir, "Intro", StringComparison.OrdinalIgnoreCase));

                foreach (var introDir in introDirs)
                    LoadTexturesFromFolder(introDir, false);
            }
            AddSoundEffect(content, "D378-15-0F");
            AddSoundEffect(content, "D378-12-0C");
            AddSoundEffect(content, "D378-25-19");
        }

        public static void LoadBlurEffect(ContentManager content)
        {
            BlurEffect = content.Load<Effect>("Shader/EffectBlur");
            RoundedCornerBlurEffect = content.Load<Effect>("Shader/RoundedCornerEffectBlur");
        }

        private static void TryLoadTextures(ref Texture2D target, string inputPath)
        {
            inputPath = GameFS.NormalizePath(inputPath);
            if (GameFS.Exists(inputPath))
                LoadTexture(out target, inputPath);
        }

        public static void LoadTextures(GraphicsDevice graphics, ContentManager content)
        {
            LoadTilesetSizes();

            // Load game sequence textures.
            LoadTexture(out SprGameSequences, Path.Combine(Values.PathContentFolder, "Sequences", "game sequences.png"));
            LoadTexture(out SprGameSequencesFinal, Path.Combine(Values.PathContentFolder, "Sequences", "end sequence.png"));

            // Load the photo textures.
            LoadTexture(out SprPhotosEng, Path.Combine(Values.PathContentFolder, "Photo Mode", "photos.png"));
            TryLoadTextures(ref SprPhotosChn, Path.Combine(Values.PathContentFolder, "Photo Mode", "photos_chn.png"));
            TryLoadTextures(ref SprPhotosDeu, Path.Combine(Values.PathContentFolder, "Photo Mode", "photos_deu.png"));
            TryLoadTextures(ref SprPhotosEsp, Path.Combine(Values.PathContentFolder, "Photo Mode", "photos_esp.png"));
            TryLoadTextures(ref SprPhotosFre, Path.Combine(Values.PathContentFolder, "Photo Mode", "photos_fre.png"));
            TryLoadTextures(ref SprPhotosInd, Path.Combine(Values.PathContentFolder, "Photo Mode", "photos_ind.png"));
            TryLoadTextures(ref SprPhotosIta, Path.Combine(Values.PathContentFolder, "Photo Mode", "photos_ita.png"));
            TryLoadTextures(ref SprPhotosPor, Path.Combine(Values.PathContentFolder, "Photo Mode", "photos_por.png"));
            TryLoadTextures(ref SprPhotosRus, Path.Combine(Values.PathContentFolder, "Photo Mode", "photos_rus.png"));

            // Load the colored photo textures.
            LoadTexture(out SprPhotosEngRedux, Path.Combine(Values.PathContentFolder, "Photo Mode", "photos_redux.png"));
            TryLoadTextures(ref SprPhotosChnRedux, Path.Combine(Values.PathContentFolder, "Photo Mode", "photos_redux_chn.png"));
            TryLoadTextures(ref SprPhotosDeuRedux, Path.Combine(Values.PathContentFolder, "Photo Mode", "photos_redux_deu.png"));
            TryLoadTextures(ref SprPhotosEspRedux, Path.Combine(Values.PathContentFolder, "Photo Mode", "photos_redux_esp.png"));
            TryLoadTextures(ref SprPhotosFreRedux, Path.Combine(Values.PathContentFolder, "Photo Mode", "photos_redux_fre.png"));
            TryLoadTextures(ref SprPhotosIndRedux, Path.Combine(Values.PathContentFolder, "Photo Mode", "photos_redux_ind.png"));
            TryLoadTextures(ref SprPhotosItaRedux, Path.Combine(Values.PathContentFolder, "Photo Mode", "photos_redux_ita.png"));
            TryLoadTextures(ref SprPhotosPorRedux, Path.Combine(Values.PathContentFolder, "Photo Mode", "photos_redux_por.png"));
            TryLoadTextures(ref SprPhotosRusRedux, Path.Combine(Values.PathContentFolder, "Photo Mode", "photos_redux_rus.png"));

            // Load the editor icons.
            LoadTexture(out _, Path.Combine(Values.PathContentFolder, "Editor", "editorIcons4x.png"));

            // Load the UI textures.
            Texture2D _nullTex = null;
            LoadTexture(out _, Path.Combine(Values.PathContentFolder, "ui.png"));
            TryLoadTextures(ref _nullTex, Path.Combine(Values.PathContentFolder, "ui_chn.png"));
            TryLoadTextures(ref _nullTex, Path.Combine(Values.PathContentFolder, "ui_deu.png"));
            TryLoadTextures(ref _nullTex, Path.Combine(Values.PathContentFolder, "ui_esp.png"));
            TryLoadTextures(ref _nullTex, Path.Combine(Values.PathContentFolder, "ui_fre.png"));
            TryLoadTextures(ref _nullTex, Path.Combine(Values.PathContentFolder, "ui_ind.png"));
            TryLoadTextures(ref _nullTex, Path.Combine(Values.PathContentFolder, "ui_ita.png"));
            TryLoadTextures(ref _nullTex, Path.Combine(Values.PathContentFolder, "ui_por.png"));
            TryLoadTextures(ref _nullTex, Path.Combine(Values.PathContentFolder, "ui_rus.png"));

            // Load sequences and light graphics.
            LoadTexturesFromFolder(Path.Combine(Values.PathContentFolder, "Sequences"));
            LoadTexturesFromFolder(Path.Combine(Values.PathContentFolder, "Light"));

            // Load the tilesets and map objects.
            LoadTexturesFromFolder(Values.PathMapObjectFolder);
            LoadTexturesFromFolder(Values.PathTilesetFolder);

            // Load graphics mods last so they override base assets
            var graphicsModsPath = GameFS.NormalizePath(Values.PathGraphicsMods);

            if (GameFS.IsDirectory(graphicsModsPath))
                LoadTexturesFromFolder(graphicsModsPath, true);

            // Static resources that never change.
            SprLink = GetTexture("link0.png");
            SprLinkCloak = GetTexture("link_cloak.png");
            SprEnemies = GetTexture("enemies.png");
            SprObjectsAnimated = GetTexture("objects animated.png");
            SprMidBoss = GetTexture("midboss.png");
            SprNightmares = GetTexture("nightmares.png");
            SprBlurTileset = GetTexture("blur tileset.png");

            // Dynamic resources that change depending on options set.
            SprNpCs = GetTexture("npcs.png");
            SprNpCsRedux = GetTexture("npcs_redux.png");
            SprObjectsEng = GetTexture("objects.png");
            SprObjectsChn = GetTexture("objects_chn.png");
            SprObjectsDeu = GetTexture("objects_deu.png");
            SprObjectsEsp = GetTexture("objects_esp.png");
            SprObjectsFre = GetTexture("objects_fre.png");
            SprObjectsInd = GetTexture("objects_ind.png");
            SprObjectsIta = GetTexture("objects_ita.png");
            SprObjectsPor = GetTexture("objects_por.png");
            SprObjectsRus = GetTexture("objects_rus.png");
            SprMiniMapEng = GetTexture("minimap.png");
            SprMiniMapChn = GetTexture("minimap_chn.png");
            SprMiniMapDeu = GetTexture("minimap_deu.png");
            SprMiniMapEsp = GetTexture("minimap_esp.png");
            SprMiniMapFre = GetTexture("minimap_fre.png");
            SprMiniMapInd = GetTexture("minimap_ind.png");
            SprMiniMapIta = GetTexture("minimap_ita.png");
            SprMiniMapPor = GetTexture("minimap_por.png");
            SprMiniMapRus = GetTexture("minimap_rus.png");
            SprItemEng = GetTexture("items.png");
            SprItemChn = GetTexture("items_chn.png");
            SprItemDeu = GetTexture("items_deu.png");
            SprItemEsp = GetTexture("items_esp.png");
            SprItemFre = GetTexture("items_fre.png");
            SprItemInd = GetTexture("items_ind.png");
            SprItemIta = GetTexture("items_ita.png");
            SprItemPor = GetTexture("items_por.png");
            SprItemRus = GetTexture("items_rus.png");
            SprItemEngRedux = GetTexture("items_redux.png");
            SprItemChnRedux = GetTexture("items_redux_chn.png");
            SprItemDeuRedux = GetTexture("items_redux_deu.png");
            SprItemEspRedux = GetTexture("items_redux_esp.png");
            SprItemFreRedux = GetTexture("items_redux_fre.png");
            SprItemIndRedux = GetTexture("items_redux_ind.png");
            SprItemItaRedux = GetTexture("items_redux_ita.png");
            SprItemPorRedux = GetTexture("items_redux_por.png");
            SprItemRusRedux = GetTexture("items_redux_rus.png");

            // Load various SpriteFonts. 
            EditorFont = content.Load<SpriteFont>("Fonts/editor font");
            EditorFontHeight = (int)EditorFont.MeasureString("H").Y;
            EditorFontMonoSpace = content.Load<SpriteFont>("Fonts/editor mono font");
            EditorFontSmallMonoSpace = content.Load<SpriteFont>("Fonts/editor small mono font");
            GameHeaderFont = content.Load<SpriteFont>("Fonts/newHeaderFont");
            FontCredits = content.Load<SpriteFont>("Fonts/credits font");
            FontCreditsHeader = content.Load<SpriteFont>("Fonts/credits header font");
            smallFont = content.Load<SpriteFont>("Fonts/smallFont");
            smallFont_redux = content.Load<SpriteFont>("Fonts/smallFont_redux");
            smallFont_vwf = content.Load<SpriteFont>("Fonts/smallFont_vwf");
            smallFont_vwf_redux = content.Load<SpriteFont>("Fonts/smallFont_vwf_redux");
            smallFont_chn = content.Load<BitmapFont>("Fonts/smallFont_chn");
            smallFont_chn_redux = content.Load<BitmapFont>("Fonts/smallFont_chn_redux");

            // load textures
            SprTiledBlock = new Texture2D(graphics, 2, 2);
            SprTiledBlock.SetData(new[] { Color.White, Color.LightGray, Color.LightGray, Color.White });

            EditorEyeOpen = content.Load<Texture2D>("Editor/eye_open");
            EditorEyeClosed = content.Load<Texture2D>("Editor/eye_closed");
            EditorIconDelete = content.Load<Texture2D>("Editor/delete");
            EditorIconEdit = content.Load<Texture2D>("Editor/edit");
            EditorIconSelect = content.Load<Texture2D>("Editor/select");

            // Lighting and shadow textures
            SprLight = content.Load<Texture2D>("Light/light");
            SprLightRoomH = content.Load<Texture2D>("Light/ligth room");
            SprLightRoomV = content.Load<Texture2D>("Light/ligth room vertical");
            SprShadow = content.Load<Texture2D>("Light/shadow");
            LoadContentTextureWithAtlas(content, "Light/doorLight");

            // These are loaded in but appear to be unused
            SprIconOptions = content.Load<Texture2D>("Menu/gearIcon");
            SprIconErase = content.Load<Texture2D>("Menu/trashIcon");
            SprIconCopy = content.Load<Texture2D>("Menu/copyIcon");

            // Super Game Boy Border
            sgbBorder = content.Load<Texture2D>("Menu/sgb_border");

            // Fog texture
            SprFog = content.Load<Texture2D>("Objects/fog");

            // Load shader effects
            RoundedCornerEffect = content.Load<Effect>("Shader/RoundedCorner");
            BlurEffectH = content.Load<Effect>("Shader/BlurH");
            BlurEffectV = content.Load<Effect>("Shader/BlurV");
            BBlurEffectH = content.Load<Effect>("Shader/BBlurH");
            BBlurEffectV = content.Load<Effect>("Shader/BBlurV");
            FullShadowEffect = content.Load<Effect>("Shader/FullShadowEffect");

            // used in the inventory
            SaturationEffect = content.Load<Effect>("Shader/SaturationFilter");
            WobbleEffect = content.Load<Effect>("Shader/WobbleShader");
            CircleShader = content.Load<Effect>("Shader/CircleShader");
            LightShader = content.Load<Effect>("Shader/LightShader");
            LightFadeShader = content.Load<Effect>("Shader/LightFadeShader");

            var cloudShader = content.Load<Effect>("Shader/ColorCloud");
            CloudShader = new SpriteShader(cloudShader);
            CloudShader.FloatParameter.Add("scaleX", 1);
            CloudShader.FloatParameter.Add("scaleY", 1);

            NoiseTexture = GetTexture("thanos noise.png");
            ThanosShader = content.Load<Effect>("Shader/ThanosShader");
            ThanosShader.Parameters["NoiceTexture"].SetValue(NoiseTexture);

            // only works for sprites using the sequence sprite
            ThanosShader.Parameters["Scale"].SetValue(new Vector2(
                    (float)SprGameSequencesFinal.Width / NoiseTexture.Width,
                    (float)SprGameSequencesFinal.Height / NoiseTexture.Height));

            ThanosSpriteShader0 = new SpriteShader(ThanosShader);
            ThanosSpriteShader0.FloatParameter.Add("Percentage", 0);
            ThanosSpriteShader1 = new SpriteShader(ThanosShader);
            ThanosSpriteShader1.FloatParameter.Add("Percentage", 0);

            WindFishShader = new SpriteShader(content.Load<Effect>("Shader/WaleShader"));
            WindFishShader.FloatParameter.Add("Offset", 0);
            WindFishShader.FloatParameter.Add("Period", 0);

            ColorShader = new SpriteShader(content.Load<Effect>("Shader/ColorShader"));

            var damageShader = content.Load<Effect>("Shader/DamageShader");

            // crow needs mark1 to have a value bigger than 0.605333
            DamageSpriteShader0 = new SpriteShader(damageShader);
            DamageSpriteShader0.FloatParameter.Add("mark0", 0.1f);
            DamageSpriteShader0.FloatParameter.Add("mark1", 0.725f);
            DamageSpriteShader0.Vector4Parameter.Add("Color0", new Vector4(1.000f, 0.710f, 0.192f, 1.000f));
            DamageSpriteShader0.Vector4Parameter.Add("Color1", new Vector4(0.871f, 0.000f, 0.000f, 1.000f));
            DamageSpriteShader0.Vector4Parameter.Add("Color2", new Vector4(0.000f, 0.000f, 0.000f, 1.000f));

            // stone hinox needs mark1 to be below 0.553
            DamageSpriteShader1 = new SpriteShader(damageShader);
            DamageSpriteShader1.FloatParameter.Add("mark0", 0.1f);
            DamageSpriteShader1.FloatParameter.Add("mark1", 0.55f);
            DamageSpriteShader1.Vector4Parameter.Add("Color0", new Vector4(1.000f, 0.710f, 0.192f, 1.000f));
            DamageSpriteShader1.Vector4Parameter.Add("Color1", new Vector4(0.871f, 0.000f, 0.000f, 1.000f));
            DamageSpriteShader1.Vector4Parameter.Add("Color2", new Vector4(0.000f, 0.000f, 0.000f, 1.000f));

            var shockShader = content.Load<Effect>("Shader/ShockEffect");

            ShockShader0 = new SpriteShader(shockShader);
            ShockShader0.FloatParameter.Add("mark0", 0.0f);
            ShockShader0.FloatParameter.Add("mark1", 0.2675f);
            ShockShader0.FloatParameter.Add("mark2", 0.725f);

            ShockShader1 = new SpriteShader(shockShader);
            ShockShader1.FloatParameter.Add("mark0", 0.0f);
            ShockShader1.FloatParameter.Add("mark1", 0.35f);
            ShockShader1.FloatParameter.Add("mark2", 0.625f);
        }

        public static void AddSoundEffect(ContentManager content, string fileName)
        {
            var soundEffect = content.Load<SoundEffect>("SoundEffects/" + fileName);

            // try add is used because some files may already be loaded for the intro sequence
            SoundEffects.TryAdd(fileName, soundEffect);
        }

        public static void LoadSounds(ContentManager content)
        {
            // RootDirectory is usually "Content"
            var dir = GameFS.NormalizePath(Path.Combine(content.RootDirectory, "SoundEffects"));

            foreach (var file in GameFS.EnumerateFiles(dir, recursive: false, acceptFile: name => 
            name.EndsWith(".xnb", StringComparison.OrdinalIgnoreCase)))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                AddSoundEffect(content, name);
            }
        }

        public static void LoadTexturesFromFolder(string path, bool recurse = false)
        {
            foreach (var full in GameFS.EnumerateFiles(path, recurse, name => name.EndsWith(".png", StringComparison.OrdinalIgnoreCase), skipDirectory: dir => string.Equals(dir, "Intro", StringComparison.OrdinalIgnoreCase)))
            {
                string textureName = Path.GetFileName(full);
                var existing = TextureList.FirstOrDefault(t => t.Name.Equals(textureName, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    LoadTexture(out existing.SprTexture, full);
                }
                else
                {
                    var newTexture = new Texture(textureName);
                    LoadTexture(out newTexture.SprTexture, full);
                    TextureList.Add(newTexture);
                }
            }
        }

        public static Texture2D GetTexture(string name)
        {
            // Try exact match first.
            var match = TextureList.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (match != null)
                return match.SprTexture;

            // Remove language tags and try again.
            string[] parts = Path.GetFileNameWithoutExtension(name).Split('_');

            // Rebuild filename skipping language parts.
            string newName = string.Join("_", parts.Where(p => !_languageList.Contains(p))) + Path.GetExtension(name);
            match = TextureList.FirstOrDefault(t => t.Name.Equals(newName, StringComparison.OrdinalIgnoreCase));
            return match?.SprTexture;
        }

        public static string FindAtlasFile(string textureName)
        {
            textureName = GameFS.NormalizePath(textureName);

            string basePath = textureName.Replace(".png", "", StringComparison.OrdinalIgnoreCase);
            string fullAtlas = basePath + ".atlas";

            if (GameFS.Exists(fullAtlas))
                return fullAtlas;

            var parts = Path.GetFileNameWithoutExtension(textureName)
                .Split('_')
                .Where(name => name != "redux" && !_languageList.Contains(name));

            var fallbackName = string.Join("_", parts) + ".atlas";
            var fallbackPath = Path.Combine(Path.GetDirectoryName(textureName) ?? "", fallbackName);

            return GameFS.NormalizePath(fallbackPath);
        }

        public static void LoadContentTextureWithAtlas(ContentManager content, string filePath)
        {
            var texture = content.Load<Texture2D>(filePath);
            var atlasPath = FindAtlasFile(Path.Combine(Values.PathContentFolder, filePath));
            SpriteAtlasSerialization.LoadSourceDictionary(texture, atlasPath, SpriteAtlas);
        }

        public static void LoadTexture(out Texture2D texture, string assetPath)
        {
            assetPath = GameFS.NormalizePath(assetPath);

            using Stream stream = GameFS.OpenReadAny(assetPath);
            texture = Texture2D.FromStream(Game1.Graphics.GraphicsDevice, stream);

            string atlasFileName = FindAtlasFile(assetPath);
            var (lang, variant) = ParseAtlasTags(assetPath);

            if (!Atlases.TryGetValue((lang, variant), out var atlasDesignation))
                throw new InvalidOperationException($"No atlas found for {lang}/{variant}");

            SpriteAtlasSerialization.LoadSourceDictionary(texture, atlasFileName, atlasDesignation);
        }

        public static Rectangle SourceRectangle(string id)
        {
            // We don't need to get any special atlas since all versions share the same dimensions.
            return SpriteAtlas.TryGetValue(id, out var entry) ? entry.ScaledRectangle : Rectangle.Empty;
        }

        private static DictAtlasEntry GetSpriteInternal(string id, bool variation)
        {
            // All sprites use the current langage to search for variations, most sprites will also use "GameSettings.Uncensored" to determine if
            // there is a "redux" uncensored version, and the photograph sprites will use "GameSettings.PhotosColor" to get the colored versions.
            string lang = Game1.LanguageManager.CurrentLanguageCode;

            // All "search" chains in the switch below should end with "SpriteAtlas" as it will always contains an entry.
            var atlases = (lang, variation) switch
            {
                ("chn", false) => new[] { SpriteAtlasChn, SpriteAtlas },
                ("deu", false) => new[] { SpriteAtlasDeu, SpriteAtlas },
                ("esp", false) => new[] { SpriteAtlasEsp, SpriteAtlas },
                ("fre", false) => new[] { SpriteAtlasFre, SpriteAtlas },
                ("ind", false) => new[] { SpriteAtlasInd, SpriteAtlas },
                ("ita", false) => new[] { SpriteAtlasIta, SpriteAtlas },
                ("por", false) => new[] { SpriteAtlasPor, SpriteAtlas },
                ("rus", false) => new[] { SpriteAtlasRus, SpriteAtlas },
                (_, false)     => new[] { SpriteAtlas },

                ("chn", true) => new[] { SpriteAtlasChnRedux, SpriteAtlasChn, SpriteAtlas },
                ("deu", true) => new[] { SpriteAtlasDeuRedux, SpriteAtlasDeu, SpriteAtlas },
                ("esp", true) => new[] { SpriteAtlasEspRedux, SpriteAtlasEsp, SpriteAtlas },
                ("fre", true) => new[] { SpriteAtlasFreRedux, SpriteAtlasFre, SpriteAtlas },
                ("ind", true) => new[] { SpriteAtlasIndRedux, SpriteAtlasInd, SpriteAtlas },
                ("ita", true) => new[] { SpriteAtlasItaRedux, SpriteAtlasIta, SpriteAtlas },
                ("por", true) => new[] { SpriteAtlasPorRedux, SpriteAtlasPor, SpriteAtlas },
                ("rus", true) => new[] { SpriteAtlasRusRedux, SpriteAtlasRus, SpriteAtlas },
                (_, true)     => new[] { SpriteAtlasRedux, SpriteAtlas }
            };
            // Check each atlas and see if it contains the ID of the sprite we are trying to load. If not, go to the next in the chain.
            foreach (var atlas in atlases)
                if (atlas.TryGetValue(id, out var sprite))
                    return sprite;

            return SpriteAtlas.TryGetValue(id, out var fallback) ? fallback : null;
        }

        public static DictAtlasEntry GetSprite(string id) => GetSpriteInternal(id, GameSettings.Uncensored);

        public static DictAtlasEntry GetPhotoSprite(string id) => GetSpriteInternal(id, GameSettings.PhotosColor);

        public static void LoadTilesetSizes()
        {
            var fileName = Path.Combine(Values.PathTilesetFolder, "tileset size.txt");

            if (!GameFS.Exists(fileName))
                return;

            foreach (var line in GameFS.ReadAllLines(fileName))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                    continue;

                var split = line.Split(':', 2, StringSplitOptions.TrimEntries);
                if (split.Length == 2 && int.TryParse(split[1], out var value))
                    TilesetSizes[split[0]] = value;
            }
        }

        public static void RefreshMenuBorderTexture(ContentManager content, int index)
        {
            var texture = index switch
            {
                0 => content.Load<Texture2D>("Menu/menuBackground"),
                1 => content.Load<Texture2D>("Menu/menuBackgroundB"),
                2 => content.Load<Texture2D>("Menu/menuBackgroundC"),
                _ => content.Load<Texture2D>("Menu/menuBackground")
            };
            var menuScreen = (MenuScreen)Game1.ScreenManager.GetScreen(Values.ScreenNameMenu);
            menuScreen?.SetBackground(texture);
        }

        public static void RefreshDynamicResources()
        {
            // Reload photo album photos so proper photos are displayed.
            Game1.GameManager.InGameOverlay.RefreshPhotoOverlay();

            // Refresh title screen resources so proper logo is displayed.
            Game1.ScreenManager.Intro.RefreshIntroResources();

            // Reload the UI textures (hearts, rupee icon, small key icon, game over, etc). 
            ItemDrawHelper.RefreshImagesUI();
        }
    }
}