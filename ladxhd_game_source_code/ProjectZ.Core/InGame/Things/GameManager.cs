﻿using System;
using System.Collections.Generic;
﻿using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectZ.InGame.GameObjects;
using ProjectZ.InGame.GameSystems;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.Overlay;
using ProjectZ.InGame.Pages;
using ProjectZ.InGame.SaveLoad;

namespace ProjectZ.InGame.Things
{
    public class GameManager
    {
        public struct MiniMapTile
        {
            public int TileIndex;
            public int HintTileIndex;
            public bool DiscoveryState;
            public string HintKey;
        }

        public struct MiniMapOverrides
        {
            public string SaveKey;
            public int PosX;
            public int PosY;
            public int TileIndex;
        }

        public class MiniMap
        {
            public int OffsetX;
            public int OffsetY;
            public MiniMapTile[,] Tiles;
            public MiniMapOverrides[] Overrides;
        }

        private class PlayingSoundEffect
        {
            public bool LowerMusicVolume;
            public float Volume;
            public double EndTime;
            public SoundEffectInstance Instance;
        }

        public Matrix GetMatrix
        {
            get
            {
                if (_activeRenderTarget == null)
                    return Matrix.Identity;

                var denomX = (int)(Game1.WindowWidth * _scaleMultiplier);
                var denomY = (int)(Game1.WindowHeight * _scaleMultiplier);

                denomX = Math.Max(1, denomX);
                denomY = Math.Max(1, denomY);

                float scaleX = (float)_activeRenderTarget.Width / denomX;
                float scaleY = (float)_activeRenderTarget.Height / denomY;

                if (float.IsNaN(scaleX) || float.IsNaN(scaleY) || float.IsInfinity(scaleX) || float.IsInfinity(scaleY))
                    return Matrix.Identity;

                return Matrix.CreateScale(scaleX, scaleY, 1f);
            }
        }
        public int CurrentRenderWidth;
        public int CurrentRenderHeight;
        public float CurrentRenderScale;

        public int BlurRenderTargetWidth => (int)(Game1.RenderWidth / MapManager.Camera.Scale / 2) + 8;
        public int BlurRenderTargetHeight => (int)(Game1.RenderHeight / MapManager.Camera.Scale / 2) + 8;

        public int SideBlurRenderTargetWidth => BlurRenderTargetWidth * 2;
        public int SideBlurRenderTargetHeight => BlurRenderTargetHeight * 2;

        public MapManager MapManager = new MapManager();
        public OverlayManager InGameOverlay = new OverlayManager();
        public SaveManager SaveManager = new SaveManager();
        public ItemManager ItemManager = new ItemManager();

        public float ForestColorState;
        public bool UseShockEffect;

        public const int EquipmentSlots = 12;
        public GameItemCollected[] Equipment = new GameItemCollected[EquipmentSlots];
        public List<GameItemCollected> CollectedItems = new List<GameItemCollected>();

        // sound effects that are currently playing
        private Dictionary<string, PlayingSoundEffect> CurrentSoundEffects = new Dictionary<string, PlayingSoundEffect>();

        // dungeon maps
        public Dictionary<string, MiniMap> DungeonMaps = new Dictionary<string, MiniMap>();
        public Dictionary<Type, GameSystem> GameSystems = new Dictionary<Type, GameSystem>();

        public Point PlayerDungeonPosition;
        public Point? PlayerMapPosition;

        public bool[,] MapVisibility;
        public bool ThiefState = false;
        public string RealSaveName = "Link";

        public string SaveName 
        {
            get { return ThiefState ? Game1.LanguageManager.GetString("savename_thief", "error") : RealSaveName; }
            set { RealSaveName = value; } 
        } 
        public int GameType = 0;

        // playtime tracking
        public float TotalPlaytime = 0.0f; // total playtime across all sessions in minutes
        public float CurrentSessionPlaytime = 0.0f; // current session playtime in minutes
        public float DrawPlayerOnTopPercentage;

        // save game data
        public string LoadedMap;
        public int SavePositionX;
        public int SavePositionY;
        public int SaveDirection;
        public int SaveSlot;
        public string SaveFileVersion = "4";

        private float _shakeCountX;
        private float _shakeCountY;
        private float _shakeSpeedX;
        private float _shakeSpeedY;
        private float _maxOffsetX;
        private float _maxOffsetY;

        public static int CloakGreen = 0;
        public static int CloakBlue = 1;
        public static int CloakRed = 2;

        public Color CloakColor => ItemDrawHelper.CloakColors[CloakType];

        public int CloakType;
        public int DeathCount;
        public int KillCount;
        public int MaxHearts = 3;
        public int CurrentHealth = 4 * 3;
        public int SwordLevel;
        public int ShieldLevel;
        public int StoneGrabberLevel;
        public bool HasMagnifyingLens;
        public bool DebugMode;
        public bool GameCleared;

        // 0: Marin, 1: Manbo, 2: Mamu
        public int[] OcarinaSongs = new int[3];
        public int SelectedOcarinaSong = 0;

        public bool GuardianAcornIsActive;
        public int GuardianAcornCount;
        public int GuardianAcornDamageCount;

        public bool PieceOfPowerIsActive;
        public int PieceOfPowerCount;
        public int PieceOfPowerDamageCount;

        private readonly Dictionary<string, List<DialogPath>> _dialogPaths = new Dictionary<string, List<DialogPath>>();
        private DialogPath _currentDialogPath;
        private readonly Queue<string> _dialogPathQueue = new Queue<string>();

        private RenderTarget2D _activeRenderTarget;
        private RenderTarget2D _inactiveRenderTarget1;
        private RenderTarget2D _inactiveRenderTarget2;
        private RenderTarget2D _shadowRenderTarget;
        private RenderTarget2D _shadowRenderTargetBlur;
        private RenderTarget2D _lightRenderTarget;

        // used for the blured tile layer; use usage of the render targets should probably be optimized
        public RenderTarget2D TempRT0;
        public RenderTarget2D TempRT1;
        public RenderTarget2D TempRT2;

        public float _scaleMultiplier;
        private int _currentDialogPathState;

        // 0: Map Music, 1: PowerUp Music, 2: Marin Singing
        private const int MusicChannels = 3;
        private int[] _musicArray = new int[MusicChannels];

        // Counters used to stop music.
        private float[] _musicCounter = new float[MusicChannels];

        // Muting the sound requires overwriting effect volume so store user setting.
        private int _curEffectVolume = GameSettings.EffectVolume;
        private bool _lastStateSet;
        private bool _muteInactive;

        // Quick reference to "ObjLink" in MapManager.
        private ObjLink Link => MapManager.ObjLink;

        public string CurrentDialogKey { get; private set; }

        public GameManager()
        {
            ResetMusic();
            GameSystems.Add(typeof(MapTransitionSystem), new MapTransitionSystem(MapManager));
            GameSystems.Add(typeof(GameOverSystem), new GameOverSystem());
            GameSystems.Add(typeof(EndingSystem), new EndingSystem());
            GameSystems.Add(typeof(MapShowSystem), new MapShowSystem());
        }

        public void Load(ContentManager content)
        {
            ItemDrawHelper.Load();

            InGameOverlay.Load(content);
            MapManager.Load();
            ItemManager.Load();

            DialogPathLoader.LoadScripts(Path.Combine(Values.PathContentFolder, "scripts.zScript"), _dialogPaths);
        }

        public void OnLoad()
        {
            InGameOverlay.OnLoad();
            _currentDialogPath = null;
            _dialogPathQueue.Clear();

            ResetMusic();

            foreach (var gameSystem in GameSystems)
                gameSystem.Value.OnLoad();

            // Force recalculating the render targets and force a rescal event just before the game loads. This ensures that the game
            // field is rendered at the correct scale. This fixes a scaling issue when starting in one screen mode and setting to another.
            Game1.GameManager?.UpdateRenderTargets();
            Game1.Instance?.ForceRecalculateScaling();

            // Ensure render targets are available now that we're entering gameplay
            UpdateRenderTargets();
        }

        public void UpdateGame()
        {
            // Update the overlay. Includes the HUD and inventory.
            InGameOverlay.Update();

            // Update the sound effects and music.
            UpdateSoundEffects();
            UpdateMusic();

            ItemDrawHelper.Update();

            // Update the dialogs. "ForceDialogUpdate" is used in sequences where the dialog should be updated but the rest of the
            // game remains frozen. Needs to come after the InGameOverlay update because "Game1.UpdateGame" can be set to false by it.
            if (Game1.UpdateGame || Game1.ForceDialogUpdate)
            {
                UpdateDialog();
                Game1.ForceDialogUpdate = false;
            }
            // Update the game if enabled and the player is not taking damage (freeze time is used when damage is taken).
            if (Game1.UpdateGame && Game1.TotalGameTime > Game1.FreezeTime)
            {
                // Track playtime during active gameplay (exclude pause/menu time).
                CurrentSessionPlaytime += Game1.DeltaTime / 1000.0f / 60.0f;

                // Update the game-systems.
                foreach (var gameSystem in GameSystems)
                    gameSystem.Value.Update();

                // Update the current map and animate all objects.
                if (Game1.UpdateGame)
                    MapManager.Update(false);
            }
            // Update the current map but freeze all objects. 
            else if (InGameOverlay.UpdateCameraAndAnimation())
            {
                MapManager.Update(true);
                MapManager.UpdateAnimation();
            }

            // update screen shake
            if (Game1.UpdateGame)
                UpdateShake();
        }

        public void DrawGame(SpriteBatch spriteBatch)
        {
            if (GameSettings.EnableShadows && MapManager.CurrentMap.UseShadows && !UseShockEffect)
            {
                /// RT:CRASH BYPASS
                if (_shadowRenderTarget == null) return;

                // render the shadows
                RenderShadows(spriteBatch);

                Resources.BlurEffectH.Parameters["pixelX"].SetValue(1.0f / _shadowRenderTarget.Width);
                Resources.BlurEffectV.Parameters["pixelY"].SetValue(1.0f / _shadowRenderTarget.Height);
                Resources.BlurEffectH.Parameters["mult0"].SetValue(0.35f);
                Resources.BlurEffectH.Parameters["mult1"].SetValue(0.15f);
                Resources.BlurEffectV.Parameters["mult0"].SetValue(0.35f);
                Resources.BlurEffectV.Parameters["mult1"].SetValue(0.15f);

                // v blur
                Game1.Graphics.GraphicsDevice.SetRenderTarget(_shadowRenderTargetBlur);
                Game1.Graphics.GraphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.AnisotropicClamp, null, null, Resources.BlurEffectV, null);
                spriteBatch.Draw(_shadowRenderTarget, Vector2.Zero, Color.White);
                spriteBatch.End();

                // h blur
                Game1.Graphics.GraphicsDevice.SetRenderTarget(_shadowRenderTarget);
                Game1.Graphics.GraphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.AnisotropicClamp, null, null, Resources.BlurEffectH, null);
                spriteBatch.Draw(_shadowRenderTargetBlur, Vector2.Zero, Color.White);
                spriteBatch.End();

                MapManager.CurrentMap.Objects.ShadowTexture = _shadowRenderTarget;
            }

            ChangeRenderTarget();
            Game1.Graphics.GraphicsDevice.Clear(Color.Black);

            // draw the map
            MapManager.Draw(spriteBatch);

            if (UseShockEffect)
            {
                // Greatly reduces the flashing lights when shopkeeper gets revenge or the Bat grants more item bag space.
                if (!GameSettings.EpilepsySafe)
                {
                    ChangeRenderTarget();

                    var usedShader = MapManager.CurrentMap.UseLight ? Resources.ShockShader1 : Resources.ShockShader0;
                    ObjectManager.SetSpriteShader(usedShader);

                    spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, null, null, usedShader.Effect);
                    spriteBatch.Draw(_inactiveRenderTarget1, Vector2.Zero, Color.White);
                    spriteBatch.End();
                }
            }

            // @Move into the World class?
            if (MapManager.CurrentMap.UseLight && !UseShockEffect)
            {
                // draw the lights
                ChangeRenderTarget();
                MapManager.DrawLight(spriteBatch);

                // combine the light with the game
                ChangeRenderTarget();
                _lightRenderTarget = _inactiveRenderTarget1;

                Game1.Graphics.GraphicsDevice.Clear(Color.Black);
                Resources.LightShader.Parameters["sprLight"].SetValue(_lightRenderTarget);
                Resources.LightShader.Parameters["lightState"].SetValue(MapManager.CurrentMap.LightState);
                Resources.LightShader.Parameters["mode"].SetValue(0);
                Resources.LightShader.Parameters["width"].SetValue(_lightRenderTarget.Width);
                Resources.LightShader.Parameters["height"].SetValue(_lightRenderTarget.Height);

                spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.AnisotropicClamp, null, null, Resources.LightShader);
                spriteBatch.Draw(_inactiveRenderTarget2, Vector2.Zero, Color.White);
                spriteBatch.End();
            }

            // update the game-systems
            foreach (var gameSystem in GameSystems)
                gameSystem.Value.Draw(spriteBatch);

            if (MapManager.CurrentMap.UseLight && !UseShockEffect && DrawPlayerOnTopPercentage > 0 && _lightRenderTarget != null)
            {
                Resources.LightShader.Parameters["sprLight"].SetValue(_lightRenderTarget);
                Resources.LightShader.Parameters["lightState"].SetValue(DrawPlayerOnTopPercentage);
                Resources.LightShader.Parameters["mode"].SetValue(1);
                Resources.LightShader.Parameters["width"].SetValue(_lightRenderTarget.Width);
                Resources.LightShader.Parameters["height"].SetValue(_lightRenderTarget.Height);

                spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, null, null, Resources.LightShader, MapManager.Camera.TransformMatrix);
                Link.DrawTransition(spriteBatch);
                spriteBatch.End();
            }
            else if (DrawPlayerOnTopPercentage > 0)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, null, null, null, MapManager.Camera.TransformMatrix);
                Link.DrawTransition(spriteBatch);
                spriteBatch.End();
            }

            // draw the output of the light and the dark shader passes
            ChangeRenderTarget();
            Game1.Graphics.GraphicsDevice.SetRenderTarget(Game1.MainRenderTarget);
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.AnisotropicWrap);

            /// RT:CRASH BYPASS
            if (_inactiveRenderTarget1 != null)
                spriteBatch.Draw(_inactiveRenderTarget1, new Rectangle(0, 0, Game1.MainRenderTarget.Width, Game1.MainRenderTarget.Height), Color.White);

            // debug stuff
            MapManager.Camera.Draw(spriteBatch);

            spriteBatch.End();
        }

        public void StartDialogPath(string dialogKey)
        {
            CurrentDialogKey = dialogKey;
            _dialogPathQueue.Enqueue(dialogKey);
        }

        public void AddFirstDialogPath(string dialogKey)
        {
            // @HACK
            var items = _dialogPathQueue.ToArray();
            _dialogPathQueue.Clear();

            _dialogPathQueue.Enqueue(dialogKey);
            foreach (var item in items)
                _dialogPathQueue.Enqueue(item);
        }

        // @TODO: this should probably be removed and replaced with StartDialogPath
        public void StartDialog(string dialogKey)
        {
            InGameOverlay.TextboxOverlay.StartDialog(Game1.LanguageManager.GetString(dialogKey, "error"));
        }

        /// <summary>
        /// @HACK: used by the map overlay to completly run a dialog path; this does only work for single dialogs
        /// the problem is that the current game dialog should be unaffected by the dialogs run by the map overlay
        /// </summary>
        /// <param name="dialogKey"></param>
        public void RunDialog(string dialogKey)
        {
            // look if a dialog path exists for the key
            DialogPath dialogPath = null;
            if (dialogKey != null && _dialogPaths.ContainsKey(dialogKey))
            {
                var paths = _dialogPaths[dialogKey];
                for (var i = 0; i < paths.Count; i++)
                {
                    if (SaveManager.GetString(paths[i].VariableKey) == null && paths[i].Condition == "0" ||
                        SaveManager.GetString(paths[i].VariableKey) == paths[i].Condition)
                    {
                        dialogPath = paths[i];
                        break;
                    }
                }
            }

            // try to start a new dialog box
            if (dialogPath == null && !InGameOverlay.TextboxOverlay.IsOpen)
            {
                // directly start a dialog
                string stateString = null;
                if (dialogKey != null)
                    stateString = SaveManager.GetString(dialogKey);

                InGameOverlay.TextboxOverlay.StartDialog(
                    Game1.LanguageManager.GetString(dialogKey + (stateString != null ? "_" + stateString : ""), "error"));
            }

            while (dialogPath != null)
            {
                var breakLoop = true;
                var dialogPathState = 0;

                // execute the current dialog path
                if (dialogPath != null)
                {
                    while (dialogPath.Action.Count > dialogPathState &&
                           dialogPath.Action[dialogPathState].Execute())
                    {
                        dialogPathState++;

                        // init the next dialog action
                        if (dialogPath.Action.Count > dialogPathState)
                            dialogPath.Action[dialogPathState].Init();
                    }

                    // do not stop executing at a empty dialog path
                    if (dialogPath.Action.Count == 0)
                        breakLoop = false;

                    // finished current path?
                    if (dialogPath.Action.Count <= dialogPathState)
                    {
                        breakLoop = false;
                        dialogPath = null;
                    }
                }

                // exit the loop if there is nothing to do
                if (breakLoop)
                    break;
            }
        }

        public bool DialogIsRunning()
        {
            return _currentDialogPath != null || _dialogPathQueue.Count > 0;
        }

        public bool FinalDialogAction()
        {
            return false;
        }

        private void UpdateDialog()
        {
            while (_currentDialogPath != null || _dialogPathQueue != null)
            {
                var breakLoop = true;

                // start a new dialog path?
                if (_dialogPathQueue.Count > 0 && _currentDialogPath == null)
                {
                    _currentDialogPath = DequeueDialogPath();
                    _currentDialogPathState = 0;

                    if (_currentDialogPath != null && _currentDialogPath.Action.Count > _currentDialogPathState)
                        _currentDialogPath.Action[_currentDialogPathState].Init();
                }

                // execute the current dialog path
                if (_currentDialogPath != null)
                {
                    while (_currentDialogPath.Action.Count > _currentDialogPathState &&
                           _currentDialogPath.Action[_currentDialogPathState].Execute())
                    {
                        _currentDialogPathState++;

                        // init the next dialog action
                        if (_currentDialogPath.Action.Count > _currentDialogPathState)
                            _currentDialogPath.Action[_currentDialogPathState].Init();
                    }

                    // do not stop executing at a empty dialog path
                    if (_currentDialogPath.Action.Count == 0)
                        breakLoop = false;

                    // finished current path?
                    if (_currentDialogPath.Action.Count <= _currentDialogPathState)
                    {
                        breakLoop = false;
                        _currentDialogPath = null;
                    }
                }

                // exit the loop if there is nothing to do
                if (breakLoop)
                    break;
            }
        }

        private DialogPath DequeueDialogPath()
        {
            InGameOverlay.TextboxOverlay.UpdateObjects = false;

            var dialogKey = _dialogPathQueue.Peek();

            // look if a dialog path exists for the key
            if (dialogKey != null && _dialogPaths.ContainsKey(dialogKey))
            {
                var paths = _dialogPaths[dialogKey];
                for (var i = 0; i < paths.Count; i++)
                {
                    if (SaveManager.GetString(paths[i].VariableKey) == null && paths[i].Condition == "0" ||
                        SaveManager.GetString(paths[i].VariableKey) == paths[i].Condition)
                    {
                        _dialogPathQueue.Dequeue();
                        return paths[i];
                    }
                }
            }

            // try to start a new dialog box
            if (!InGameOverlay.TextboxOverlay.IsOpen)
            {
                _dialogPathQueue.Dequeue();

                // directly start a dialog
                string stateString = null;
                if (dialogKey != null)
                    stateString = SaveManager.GetString(dialogKey);

                InGameOverlay.TextboxOverlay.StartDialog(
                    Game1.LanguageManager.GetString(dialogKey + (stateString != null ? "_" + stateString : ""), "error"));
            }

            return null;
        }

        public void RenderShadows(SpriteBatch spriteBatch)
        {
            if (_shadowRenderTarget == null || _shadowRenderTargetBlur == null)
                return;

            try
            {
                Game1.Graphics.GraphicsDevice.SetRenderTarget(_shadowRenderTarget);
                Game1.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Game1.Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                Game1.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
                Game1.Graphics.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
                Game1.Graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

                // Make sure SpriteBatch is in a valid state for drawing (caller should handle begin/end)
                MapManager.CurrentMap.Objects.DrawShadow(spriteBatch);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("RenderShadows failed: " + ex);
                try { Game1.Graphics.GraphicsDevice.SetRenderTarget(null); } catch { }
            }
        }

        public void ChangeRenderTarget()
        {
            // If RTs not created, try to create them.
            if (_activeRenderTarget == null || _inactiveRenderTarget1 == null || _inactiveRenderTarget2 == null)
            {
                UpdateRenderTargets();
                if (_activeRenderTarget == null || _inactiveRenderTarget1 == null || _inactiveRenderTarget2 == null)
                {
                    // fallback: leave render target as backbuffer
                    try { Game1.Graphics.GraphicsDevice.SetRenderTarget(null); } catch { }
                    return;
                }
            }
            // Swap round-robin safely
            var tempActiveRt = _activeRenderTarget;
            _activeRenderTarget = _inactiveRenderTarget2;
            _inactiveRenderTarget2 = _inactiveRenderTarget1;
            _inactiveRenderTarget1 = tempActiveRt;

            // Ensure we don't attempt to set a null RT
            SetActiveRenderTarget();
        }

        public void SetActiveRenderTarget()
        {
            // Ensure RTs are ready
            if (_activeRenderTarget == null)
            {
                UpdateRenderTargets();
                if (_activeRenderTarget == null)
                {
                    // can't set a null RT; fallback to backbuffer (null)
                    try { Game1.Graphics.GraphicsDevice.SetRenderTarget(null); } catch { }
                    return;
                }
            }
            try
            {
                Game1.Graphics.GraphicsDevice.SetRenderTarget(_activeRenderTarget);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SetActiveRenderTarget failed: " + ex);
                try { Game1.Graphics.GraphicsDevice.SetRenderTarget(null); } catch { }
            }
        }

        public void DisposeRenderTargets(bool disposeOverlay)
        {
            try
            {
                // Also dispose overlay render targets.
                if (disposeOverlay)
                    InGameOverlay?.DisposeRenderTargets();

                _activeRenderTarget?.Dispose(); _activeRenderTarget = null;
                _inactiveRenderTarget1?.Dispose(); _inactiveRenderTarget1 = null;
                _inactiveRenderTarget2?.Dispose(); _inactiveRenderTarget2 = null;

                _lightRenderTarget?.Dispose(); _lightRenderTarget = null;
                _shadowRenderTarget?.Dispose(); _shadowRenderTarget = null;
                _shadowRenderTargetBlur?.Dispose(); _shadowRenderTargetBlur = null;

                TempRT0?.Dispose(); TempRT0 = null;
                TempRT1?.Dispose(); TempRT1 = null;
                TempRT2?.Dispose(); TempRT2 = null;
            }
            catch { }
        }

        public RenderTarget2D GetLastRenderTarget()
        {
            return _inactiveRenderTarget1;
        }

        public void DrawTop(SpriteBatch spriteBatch)
        {
            // draw the inventory
            InGameOverlay.Draw(spriteBatch);
        }

        public void DrawRenderTarget(SpriteBatch spriteBatch)
        {
            // draw the rt stuff of the game ui
            InGameOverlay.DrawRenderTarget(spriteBatch);
        }

        public void SetGameScale(float scale)
        {
            _scaleMultiplier = MathF.Ceiling(scale) / scale;

            UpdateRenderTargets();
        }

        public void OnResize()
        {
            InGameOverlay.ResolutionChanged();

            Game1.RenderWidth = (int)(Game1.WindowWidth  * _scaleMultiplier);
            Game1.RenderHeight = (int)(Game1.WindowHeight * _scaleMultiplier);

            UpdateRenderTargets();

            // Use the active render target size to determine bounds, and fall back to
            // viewport size if it's currently null. Fixes scaling issues on Android.
            if (_activeRenderTarget != null)
                MapManager.Camera.SetBounds(_activeRenderTarget.Width, _activeRenderTarget.Height);
            else
            {
                var viewport = Game1.Graphics.GraphicsDevice.Viewport;
                MapManager.Camera.SetBounds(viewport.Width, viewport.Height);
            }
            MapManager.Camera.ForceUpdate(MapManager.GetCameraTarget());
        }

        public void OnResizeEnd()
        {
            InGameOverlay.UpdateRenderTarget();
            UpdateRenderTargets();
        }

        public void UpdateRenderTargets()
        {
            // If sizes didn't change or sizes invalid, skip
            if ((CurrentRenderWidth == Game1.RenderWidth &&
                 CurrentRenderHeight == Game1.RenderHeight &&
                 CurrentRenderScale == MapManager.Camera.Scale) ||
                 Game1.RenderWidth <= 0 || Game1.RenderHeight <= 0)
                return;

            CurrentRenderWidth = Math.Max(1, Game1.RenderWidth);
            CurrentRenderHeight = Math.Max(1, Game1.RenderHeight);
            CurrentRenderScale = MapManager.Camera.Scale;

            // compute shadow/temp sizes (clamped)
            var shadowScale = MathHelper.Clamp(MapManager.Camera.Scale / 2, 1, 10);
            var shadowRtWidth = Math.Max(1, (int)(CurrentRenderWidth / shadowScale));
            var shadowRtHeight = Math.Max(1, (int)(CurrentRenderHeight / shadowScale));

            var blurRtWidth = Math.Max(1, BlurRenderTargetWidth);
            var blurRtHeight = Math.Max(1, BlurRenderTargetHeight);
            var sideBlurRtWidth = Math.Max(1, SideBlurRenderTargetWidth);
            var sideBlurRtHeight = Math.Max(1, SideBlurRenderTargetHeight);

            // create new RTs first
            RenderTarget2D newActive = null;
            RenderTarget2D newInactive1 = null;
            RenderTarget2D newInactive2 = null;
            RenderTarget2D newShadow = null;
            RenderTarget2D newShadowBlur = null;
            RenderTarget2D newTemp0 = null;
            RenderTarget2D newTemp1 = null;
            RenderTarget2D newTemp2 = null;

            try
            {
                var usage = RenderTargetUsage.PreserveContents;
                newActive = new RenderTarget2D(Game1.Graphics.GraphicsDevice, CurrentRenderWidth, CurrentRenderHeight,
                    false, SurfaceFormat.Color, DepthFormat.None, 0, usage);
                newInactive1 = new RenderTarget2D(Game1.Graphics.GraphicsDevice, CurrentRenderWidth, CurrentRenderHeight,
                    false, SurfaceFormat.Color, DepthFormat.None, 0, usage);
                newInactive2 = new RenderTarget2D(Game1.Graphics.GraphicsDevice, CurrentRenderWidth, CurrentRenderHeight,
                    false, SurfaceFormat.Color, DepthFormat.None, 0, usage);

                newShadow = new RenderTarget2D(Game1.Graphics.GraphicsDevice, shadowRtWidth, shadowRtHeight,
                    false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
                newShadowBlur = new RenderTarget2D(Game1.Graphics.GraphicsDevice, shadowRtWidth, shadowRtHeight,
                    false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

                newTemp0 = new RenderTarget2D(Game1.Graphics.GraphicsDevice, blurRtWidth, blurRtHeight,
                    false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
                newTemp1 = new RenderTarget2D(Game1.Graphics.GraphicsDevice, blurRtWidth, blurRtHeight,
                    false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
                newTemp2 = new RenderTarget2D(Game1.Graphics.GraphicsDevice, sideBlurRtWidth, sideBlurRtHeight,
                    false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateRenderTargets: failed creating render targets: " + ex);
                newActive?.Dispose();
                newInactive1?.Dispose();
                newInactive2?.Dispose();
                newShadow?.Dispose();
                newShadowBlur?.Dispose();
                newTemp0?.Dispose();
                newTemp1?.Dispose();
                newTemp2?.Dispose();
                return;
            }

            // All new RTs created successfully: swap them in and dispose old ones
            DisposeRenderTargets(false);

            _activeRenderTarget = newActive;
            _inactiveRenderTarget1 = newInactive1;
            _inactiveRenderTarget2 = newInactive2;
            _shadowRenderTarget = newShadow;
            _shadowRenderTargetBlur = newShadowBlur;
            TempRT0 = newTemp0;
            TempRT1 = newTemp1;
            TempRT2 = newTemp2;
        }

        public void HealPlayer(int hearts)
        {
            CurrentHealth += hearts;
            if (CurrentHealth > MaxHearts * 4)
                CurrentHealth = MaxHearts * 4;
        }

        public void InflictDamage(int damage)
        {
            if (DebugMode)
                return;

            if (CloakType == CloakBlue)
                damage = (int)MathF.Ceiling(damage / 2f);
            if (GuardianAcornIsActive)
                damage = (int)MathF.Ceiling(damage / 2f);

            damage = (int)MathF.Ceiling(damage * (GameSettings.DamageFactor * 0.25f));

            CurrentHealth -= damage;

            if (CurrentHealth < 0)
                CurrentHealth = 0;

            // reset count for the guardian acorn
            GuardianAcornCount = 0;

            if (GuardianAcornIsActive)
            {
                GuardianAcornDamageCount++;
                if (GuardianAcornDamageCount >= 3)
                {
                    StopGuardianAcorn();
                    GuardianAcornCount = 0;
                }
            }

            // piece of power
            if (PieceOfPowerIsActive)
            {
                PieceOfPowerDamageCount++;
                if (PieceOfPowerDamageCount >= 3)
                {
                    StopPieceOfPower();
                    PieceOfPowerCount = 0;
                }
            }
        }

        public void InitGuardianAcorn()
        {
            if (PieceOfPowerIsActive)
                StopPieceOfPower();

            GuardianAcornIsActive = true;
            GuardianAcornDamageCount = 0;

            if (!GameSettings.MutePowerups)
                StartPieceOfPowerMusic(0);
            else
                PlaySoundEffect("D360-23-17");
        }

        public void StopGuardianAcorn()
        {
            GuardianAcornIsActive = false;

            if (!GameSettings.MutePowerups)
                SetMusic(-1, 1, false);
        }

        public void InitPieceOfPower()
        {
            if (GuardianAcornIsActive)
                StopGuardianAcorn();

            PieceOfPowerIsActive = true;
            PieceOfPowerDamageCount = 0;

            if (!GameSettings.MutePowerups)
                StartPieceOfPowerMusic(0);
            else
                PlaySoundEffect("D360-23-17");
        }
        public void StartPieceOfPowerMusic(int Variation)
        {
            // 0: Delayed with sound effect
            // 1: Music starts instantly.
            int trackId = Variation == 0 ? 38 : 72;
            SetMusic(trackId, 1);

            // @HACK: When music is restarted for any reason: map/area transition, healing
            // from a great fairy, etc. we want the version without the starting sound effect.
            if (Variation == 0)
            {
                Game1.GbsPlayer.CurrentTrack = 72;
                _musicArray[1] = 72;
            }
        }
        public void StopPieceOfPower()
        {
            PieceOfPowerIsActive = false;

            if (!GameSettings.MutePowerups)
                SetMusic(-1, 1, false);
        }

        public void ResetMusic()
        {
            for (var i = 0; i < MusicChannels; i++)
            {
                _musicArray[i] = -1;
                _musicCounter[i] = 0;
            }
        }

        public void UpdateMusic()
        {
            for (var i = 0; i < MusicChannels; i++)
            {
                if (_musicCounter[i] == 0)
                    continue;

                _musicCounter[i] -= Game1.DeltaTime;

                // finished playing the music?
                if (_musicCounter[i] <= 0)
                {
                    _musicArray[i] = -1;
                    _musicCounter[i] = 0;
                    PlayMusic();
                }
            }
        }

        public void StopMusic(bool reset = false)
        {
            if (reset)
                ResetMusic();

            Game1.GbsPlayer.Stop();
        }

        public void StopMusic(int time, int priority)
        {
            _musicCounter[priority] = time;
        }

        public void PlayMusic(bool startPlaying = true)
        {
            for (var i = MusicChannels - 1; i >= 0; i--)
            {
                if (_musicArray[i] >= 0)
                {
                    var songNumber = (byte)_musicArray[i];
                    Game1.GbsPlayer.RequestTrack(songNumber);

                    if (startPlaying)
                        Game1.GbsPlayer.Play();

                    return;
                }
            }
            // no music is playing?
            Game1.GbsPlayer.Stop();
        }

        public bool CheckSetMusicConditions(int trackID, int priority)
        {
            // Don't restart the overworld track if the version with the intro was already started. But if it's the part
            // of the game where Marin joins the player and the beach photo is taken, we need to allow song 4 to replace 48.
            if (trackID == 4 && _musicArray[priority] == 48 && SaveManager.GetString("maria_state") != "3")
                return false;

            // Make sure to not restart the music while showing the overworld in the final sequence. 
            if (priority != 2 && _musicArray[2] == 62)
                return false;

            // When entering a village (3: Mabe Village, 10: Animal Village) with (72: Piece of Power Music)
            // backup the piece of power music and force the new song onto the piece of power slot.
            if ((trackID == 3 || trackID == 10) && _musicArray[1] == 72 && priority == 0)
            {
                _musicArray[0] = 72;
                _musicArray[1] = trackID;
            }
            // When leaving the village, restore piece of power music and write new track to it's proper slot.
            else if ((trackID != 3 && trackID != 10) && _musicArray[0] == 72 && priority == 0)
            {
                _musicArray[0] = trackID;
                _musicArray[1] = 72;
            }
            // In any other cases, just handle music normally.
            else
            {
                _musicArray[priority] = trackID;
            }
            // Play the music.
            return true;
        }

        public void SetMusic(int trackID, int priority, bool startPlaying = true)
        {
            // See if we should play music and if there is any nuances to take care of before playing the music.
            if (CheckSetMusicConditions(trackID, priority))
                PlayMusic(startPlaying);
        }

        public int GetCurrentMusic()
        {
            for (var i = _musicArray.Length - 1; i >= 0; i--)
                if (_musicArray[i] >= 0)
                    return _musicArray[i];

            return -1;
        }

        public void HandleInactiveWindow(bool IsActive)
        {
            // We don't need this to run every single game tick.
            if (IsActive != _lastStateSet)
            {
                if (!IsActive & GameSettings.MuteInactive)
                {
                    Game1.GbsPlayer.SetVolume(0f);
                    _muteInactive = true;
                }
                else
                {
                    Game1.GbsPlayer.SetVolume(GameSettings.MusicVolume / 100.0f);
                    _muteInactive = false;
                }
            }
            _lastStateSet = IsActive;
        }

        public void UpdateSoundEffects()
        {
            var lowerVolume = false;

            // Set the volume to 0 if window is inactive otherwise use the volume set by the player.
            _curEffectVolume = _muteInactive ? 0 : GameSettings.EffectVolume;

            // we use ToList to be able to remove entries in the foreach loop
            foreach (var soundEffect in CurrentSoundEffects.ToList())
            {
                if (CurrentSoundEffects[soundEffect.Key].LowerMusicVolume)
                    lowerVolume = true;

                // update the volume of the sound effects to match the current settings
                soundEffect.Value.Instance.Volume = CurrentSoundEffects[soundEffect.Key].Volume * _curEffectVolume / 100 * Values.SoundEffectVolumeMult;
                soundEffect.Value.Instance.IsLooped = false;

                if (soundEffect.Value.EndTime != 0 &&
                    soundEffect.Value.EndTime < Game1.TotalGameTime)
                    soundEffect.Value.Instance.Stop();

                // finished playing?
                if (soundEffect.Value.Instance.State == SoundState.Stopped)
                    CurrentSoundEffects.Remove(soundEffect.Key);
            }

            if (lowerVolume)
                Game1.GbsPlayer.SetVolumeMultiplier(0.35f);
        }

        public void PauseSoundEffects()
        {
            foreach (var soundEffect in CurrentSoundEffects)
                if (soundEffect.Value.Instance.State == SoundState.Playing)
                    soundEffect.Value.Instance.Pause();
        }

        public void ContinueSoundEffects()
        {
            foreach (var soundEffect in CurrentSoundEffects)
                if (soundEffect.Value.Instance.State == SoundState.Paused)
                    soundEffect.Value.Instance.Resume();
        }

        public void PlaySoundEffect(string name, bool restart, Vector2 position, float range = 256)
        {
            var playerDistance = Link.EntityPosition.Position - position;
            var volume = 1 - playerDistance.Length() / range;

            if (volume > 0)
                PlaySoundEffect(name, restart, volume);
        }

        public void PlaySoundEffect(string name, bool restart = true, float volume = 1, float pitch = 0, bool lowerMusicVolume = false, float playtime = 0)
        {
            CurrentSoundEffects.TryGetValue(name, out var entry);

            // if the same sound is playing it will be stopped and replaced with the new instance
            if (restart && entry!= null && entry.Instance != null)
            {
                entry.Instance.Stop();
                CurrentSoundEffects.Remove(name);
            }
            if (!restart && entry != null && entry.Instance != null)
            {
                entry.Volume = volume;
                if (playtime != 0)
                    entry.EndTime = Game1.TotalGameTime + playtime;

                entry.Instance.Volume = volume * _curEffectVolume / 100f * Values.SoundEffectVolumeMult;
                entry.Instance.Pitch = pitch;
                
                return;
            }

            entry = new PlayingSoundEffect() { Volume = volume, LowerMusicVolume = lowerMusicVolume };
            entry.Instance = Resources.SoundEffects[name].CreateInstance();

            // the volume of the sound effects is higher than the music; so scale effect volume a little down
            entry.Instance.Volume = volume * _curEffectVolume / 100f * Values.SoundEffectVolumeMult;
            entry.Instance.Pitch = pitch;

            if (playtime != 0)
            {
                entry.Instance.IsLooped = true;
                entry.EndTime = Game1.TotalGameTime + playtime;
            }

            entry.Instance.Play();

            CurrentSoundEffects.Add(name, entry);
        }

        public void StopSoundEffect(string name)
        {
            if (CurrentSoundEffects.TryGetValue(name, out var entry))
                entry.Instance.Stop();
        }

        public bool IsPlaying(string name)
        {
            if (CurrentSoundEffects.TryGetValue(name, out var entry))
                return entry.Instance.State == SoundState.Playing;

            return false;
        }

        public void ShakeScreenContinue(int time, int maxX, int maxY, float shakeSpeedX, float shakeSpeedY)
        {
            var periodsX = (_shakeCountX / 100f * _shakeSpeedX) % (MathF.PI * 2);
            _shakeCountX = time;
            if (_shakeSpeedX > 0)
                _shakeCountX += periodsX / _shakeSpeedX * 100f;

            _shakeCountY = time;
            _maxOffsetX = maxX;
            _maxOffsetY = maxY;
            _shakeSpeedX = shakeSpeedX;
            _shakeSpeedY = shakeSpeedY;
        }

        public void ShakeScreen(int time, float maxX, float maxY, float shakeSpeedX, float shakeSpeedY, int startDirX = 1, int startDirY = 1)
        {
            _shakeCountX = time;
            _shakeCountY = time;
            _maxOffsetX = maxX;
            _maxOffsetY = maxY;
            _shakeSpeedX = shakeSpeedX;
            _shakeSpeedY = shakeSpeedY;

            if (_shakeSpeedX > 0)
            {
                var periodsX = MathF.Round((time / 100f * _shakeSpeedX) / MathF.PI);
                if ((startDirX == -1 && periodsX % 2 == 0) ||
                    (startDirX == 1 && periodsX % 2 == 1))
                    periodsX += 1;
                _shakeCountX = (periodsX * MathF.PI) / _shakeSpeedX * 100f;
            }

            if (_shakeSpeedY > 0)
            {
                var periodsY = MathF.Round((time / 100f * _shakeSpeedY) / MathF.PI);
                if ((startDirY == 1 && periodsY % 2 == 0) ||
                    (startDirY == -1 && periodsY % 2 == 1))
                    periodsY += 1;
                _shakeCountY = (periodsY * MathF.PI) / _shakeSpeedY * 100f;
            }
        }

        public void UpdateShake()
        {
            bool shakingX = _shakeCountX > 0;
            bool shakingY = _shakeCountY > 0;

            if (shakingX)
            {
                _shakeCountX -= Game1.DeltaTime;
                MapManager.Camera.ShakeOffsetX = (float)Math.Sin(_shakeCountX / 100f * _shakeSpeedX) * _maxOffsetX;
            }
            else
            {
                // Ensure camera offset snaps cleanly back to 0
                MapManager.Camera.ShakeOffsetX = 0;
                _shakeCountX = 0;
            }

            if (shakingY)
            {
                _shakeCountY -= Game1.DeltaTime;
                MapManager.Camera.ShakeOffsetY = (float)Math.Sin(_shakeCountY / 100f * _shakeSpeedY) * _maxOffsetY;
            }
            else
            {
                // Ensure camera offset snaps cleanly back to 0
                MapManager.Camera.ShakeOffsetY = 0;
                _shakeCountY = 0;
            }
            // Round offsets to whole pixels for extra safety
            MapManager.Camera.ShakeOffsetX = (float)Math.Round(MapManager.Camera.ShakeOffsetX);
            MapManager.Camera.ShakeOffsetY = (float)Math.Round(MapManager.Camera.ShakeOffsetY);
        }

        public bool LoadMiniMap(string mapName)
        {
            // already loaded the levels?
            if (DungeonMaps.ContainsKey(mapName))
                return true;

            // load the mini map
            var fileName = Path.Combine(Values.PathMinimapFolder, mapName) + ".txt";
            var dungeonMap = SaveLoadMap.LoadMiniMap(fileName);

            if (dungeonMap == null)
                return false;

            DungeonMaps.Add(mapName, dungeonMap);
            return true;
        }

        public void SetDungeon(string dungeonName, int dungeonLevel)
        {
            var level = 0;
            while (true)
            {
                if (!LoadMiniMap(dungeonName + "_" + level))
                    break;

                LoadMiniMap(dungeonName + "_" + level + "_alt");

                level++;
            }
            MapManager.NextMap.DungeonMode = true;
            MapManager.NextMap.DungeonMapless = false;
            MapManager.NextMap.DungeonCastle = false;
            MapManager.NextMap.LocationName = dungeonName;
            MapManager.NextMap.LocationFullName = dungeonName + "_" + dungeonLevel;
        }

        public void SetDungeonMapless(string dungeonName)
        {
            MapManager.NextMap.DungeonMode = false;
            MapManager.NextMap.DungeonMapless = true;
            MapManager.NextMap.DungeonCastle = false;
            MapManager.NextMap.LocationName = dungeonName;
            MapManager.NextMap.LocationFullName = dungeonName;
        }

        public void SetCastle(string dungeonName)
        {
            MapManager.NextMap.DungeonMode = false;
            MapManager.NextMap.DungeonMapless = false;
            MapManager.NextMap.DungeonCastle = true;
            MapManager.NextMap.LocationName = dungeonName;
            MapManager.NextMap.LocationFullName = dungeonName;
            System.Diagnostics.Debug.WriteLine(MapManager.NextMap.DungeonCastle);
        }

        public void SetNoFollowersMap()
        {
            MapManager.NextMap.NoFollowers = true;
        }

        public void SetFinalMap()
        {
            MapManager.NextMap.IsFinalMap = true;
        }

        public void DungeonUpdatePlayerPosition(Point position)
        {
            // updated map discovery state
            if (MapManager.CurrentMap.LocationFullName != null &&
                DungeonMaps.ContainsKey(MapManager.CurrentMap.LocationFullName) &&
                position.X >= 0 && position.Y >= 0 &&
                position.X < DungeonMaps[MapManager.CurrentMap.LocationFullName].Tiles.GetLength(0) &&
                position.Y < DungeonMaps[MapManager.CurrentMap.LocationFullName].Tiles.GetLength(1))
                DungeonMaps[MapManager.CurrentMap.LocationFullName].Tiles[position.X, position.Y].DiscoveryState = true;

            PlayerDungeonPosition = position;
        }

        public void SetMapPosition(Point position)
        {
            if (MapVisibility == null ||
                0 > position.X || position.X >= MapVisibility.GetLength(0) ||
                0 > position.Y || position.Y >= MapVisibility.GetLength(1))
                return;

            MapVisibility[position.X, position.Y] = true;
            PlayerMapPosition = position;
        }

        public GameItemCollected GetItem(string itemId)
        {
            for (var i = 0; i < Equipment.Length; i++)
            {
                if (Equipment[i] != null && Equipment[i].Name == itemId &&
                    (string.IsNullOrEmpty(Equipment[i].LocationBounding) ||
                     Equipment[i].LocationBounding == MapManager.CurrentMap.LocationName))
                    return Equipment[i];
            }

            for (var i = 0; i < CollectedItems.Count; i++)
            {
                // player has item
                if (CollectedItems[i].Name == itemId &&
                    (string.IsNullOrEmpty(CollectedItems[i].LocationBounding) ||
                     CollectedItems[i].LocationBounding == MapManager.CurrentMap.LocationName))
                    return CollectedItems[i];
            }

            return null;
        }

        public void CollectItem(GameItemCollected itemCollected, int equipmentSlot = -1)
        {
            if (itemCollected.LocationBounding == "")
                itemCollected.LocationBounding = null;

            var item = ItemManager[itemCollected.Name];
            // the base item has the max count information
            var baseItem = ItemManager[item.Name];

            // make sure to replace then name
            // this is used for items that have different variations like the normal powder or powderPD with dialog
            itemCollected.Name = item.Name;

            // add the arrow count to the bow and remove the arrows
            if (itemCollected.Name == "bow")
            {
                var arrow = GetItem("arrow");
                if (arrow != null)
                {
                    itemCollected.Count += arrow.Count;
                    RemoveItem("arrow", arrow.Count);
                }
            }
            // if we have the bow collected change the type to bow
            if (itemCollected.Name == "arrow")
            {
                var bow = GetItem("bow");
                if (bow != null)
                {
                    itemCollected.Name = "bow";
                    item = ItemManager[itemCollected.Name];
                    baseItem = ItemManager[item.Name];
                }
            }
            if (itemCollected.Name == "cloakBlue")
                CloakType = CloakBlue;
            else if (itemCollected.Name == "cloakRed")
                CloakType = CloakRed;

            // Unlock the ocarina songs.
            if (itemCollected.Name == "ocarina_maria")
            {
                OcarinaSongs[0] = 1;

                if (SelectedOcarinaSong == -1)
                    SelectedOcarinaSong = 0;
            }
            if (itemCollected.Name == "ocarina_manbo")
            {
                OcarinaSongs[1] = 1;

                if (SelectedOcarinaSong == -1)
                    SelectedOcarinaSong = 1;
            }
            if (itemCollected.Name == "ocarina_frog")
            {
                OcarinaSongs[2] = 1;

                if (SelectedOcarinaSong == -1)
                    SelectedOcarinaSong = 2;
            }

            // Magnifying lens collected.
            if (itemCollected.Name == "trade13")
                HasMagnifyingLens = true;

            if (baseItem.Equipable)
            {
                var maxCount = baseItem.MaxCount;

                if (itemCollected.Name == "sword1")
                    SwordLevel = 1;
                else if (itemCollected.Name == "sword2")
                    SwordLevel = 2;

                if (itemCollected.Name == "shield" ||
                    itemCollected.Name == "mirrorShield")
                    ShieldLevel = item.Level;
                if (itemCollected.Name == "stonelifter" || itemCollected.Name == "stonelifter2")
                    StoneGrabberLevel = item.Level;

                // powder, bomb or arrow?
                // check if the inventory was upgraded or not
                if (item.Name == "powder" && SaveManager.GetString("upgradePowder") == "1")
                    maxCount += 20;
                if (item.Name == "bomb" && SaveManager.GetString("upgradeBomb") == "1")
                    maxCount += 30;
                if (item.Name == "bow" && SaveManager.GetString("upgradeBow") == "1")
                    maxCount += 30;

                // search if the player already owns the equipment
                for (var i = 0; i < Equipment.Length; i++)
                {
                    if (Equipment[i] != null && Equipment[i].Name == item.Name)
                    {
                        Equipment[i].Count += itemCollected.Count;

                        if (maxCount > 0 && Equipment[i].Count > maxCount)
                            Equipment[i].Count = maxCount;

                        return;
                    }
                }

                if (maxCount > 0 && itemCollected.Count > maxCount)
                    itemCollected.Count = maxCount;

                // requested equipment slot is empty?
                if (0 <= equipmentSlot && equipmentSlot < Equipment.Length && Equipment[equipmentSlot] == null)
                {
                    SetEquipment(equipmentSlot, itemCollected);
                    return;
                }

                // add item to the collected item list
                var start = equipmentSlot < 0 ? 4 : 0;
                for (var i = start; i < Equipment.Length; i++)
                {
                    if (Equipment[i] != null)
                        continue;

                    SetEquipment(i, itemCollected);
                    return;
                }
            }
            // The item picked up is not an equippable item.
            else
            {
                // Search if the player already owns the item.
                var found = false;
                for (var i = 0; i < CollectedItems.Count; i++)
                {
                    if ((CollectedItems[i].Name == item.Name) && (CollectedItems[i].LocationBounding == itemCollected.LocationBounding))
                    {
                        CollectedItems[i].Count += itemCollected.Count;

                        if (baseItem.MaxCount > 0 && CollectedItems[i].Count > baseItem.MaxCount)
                            CollectedItems[i].Count = baseItem.MaxCount;

                        found = true;
                        break;
                    }
                }
                if (!found)
                    CollectedItems.Add(itemCollected);

                // A Piece of Heart or a Heart Container was picked up.
                if (item.Name == "heartMeter" || item.Name == "heartMeterFull")
                {
                    var heart = GetItem(item.Name);
                    while (heart?.Count >= 4)
                    {
                        heart.Count -= 4;
                        MaxHearts++;
                        HealPlayer(99);
                        ItemDrawHelper.EnableHeartAnimationSound();
                    }
                }

                // The flippers were picked up.
                else if (item.Name == "flippers")
                    Link.HasFlippers = true;
            }
        }

        public int GetEquipmentSlot(string itemName)
        {
            for (var i = 0; i < Equipment.Length; i++)
            {
                if (Equipment[i] != null && Equipment[i].Name == itemName)
                    return i;
            }

            return 0;
        }

        public bool RemoveItem(string itemName, int count)
        {
            // equipment
            for (var i = 0; i < Equipment.Length; i++)
            {
                if (Equipment[i] == null || Equipment[i].Name != itemName ||
                    ItemManager[Equipment[i].Name].Level == 0 && Equipment[i].Count < count)
                    continue;

                Equipment[i].Count -= count;

                // remove the item from the inventory if the player can only have 1 of it
                // bombs, powder, etc will stay in the inventory
                if (Equipment[i].Count <= 0 && ItemManager[Equipment[i].Name].MaxCount == 1)
                    Equipment[i] = null;

                // remove the item?
                // not sure what to do here
                // in the original powder gets removed but bombs not; what happens to arrows I do not know
                if (itemName == "powder" && Equipment[i] != null && Equipment[i].Count == 0)
                    Equipment[i] = null;

                return true;
            }

            // items
            for (var i = 0; i < CollectedItems.Count; i++)
            {
                if (CollectedItems[i] == null || CollectedItems[i].Name != itemName ||
                    ItemManager[CollectedItems[i].Name].Level == 0 && CollectedItems[i].Count < count ||
                    !string.IsNullOrEmpty(CollectedItems[i].LocationBounding) && CollectedItems[i].LocationBounding != MapManager.CurrentMap.LocationName)
                    continue;

                CollectedItems[i].Count -= count;

                // remove the item?
                if (ItemManager[CollectedItems[i].Name].Level != 0 || CollectedItems[i].Count == 0)
                {
                    CollectedItems.RemoveAt(i);

                    if (itemName == "flippers")
                        Link.HasFlippers = false;
                }

                return true;
            }

            return false;
        }

        public void ChangeItem(int oldSlot, int newSlot)
        {
            var tempAcc = Equipment[oldSlot];

            SetEquipment(oldSlot, Equipment[newSlot]);
            SetEquipment(newSlot, tempAcc);
        }

        public void SetEquipment(int index, GameItemCollected item)
        {
            Equipment[index] = item;
            UpdateEquipment();
        }

        public void UpdateEquipment()
        {
            Link.CarrySword = false;
            Link.CarryShield = false;

            for (var i = 0; i < Values.HandItemSlots; i++)
            {
                if (Equipment[i]?.Name == "sword1" || Equipment[i]?.Name == "sword2")
                    Link.CarrySword = true;
                else if (Equipment[i]?.Name == "shield" || Equipment[i]?.Name == "mirrorShield")
                    Link.CarryShield = true;
            }
        }

        public void SetGameTypeSettings()
        {
            // Game Type is set to "Modern".
            if (GameType == 1)
            {
                GameSettings.ClassicMusic = false;
                GameSettings.ClassicCamera = false;
                GameSettings.ClassicDungeon = false;
                GameSettings.ClassicBorders = 0;
            }
            // Game Type is set to "Classic".
            else if (GameType == 2)
            {
                GameSettings.ClassicMusic = true;
                GameSettings.ClassicCamera = true;
                GameSettings.ClassicDungeon = false;
                GameSettings.ClassicBorders = 1;
            }
            // Game type is set to "Hybrid".
            else if (GameType == 3)
            {
                GameSettings.ClassicMusic = true;
                GameSettings.ClassicCamera = true;
                GameSettings.ClassicDungeon = true;
                GameSettings.ClassicBorders = 1;
            }
            // Apply the settings to the settings menus.
            if (Game1.UiPageManager.InsideElement.TryGetValue(typeof(AudioSettingsPage), out var audPage))
            {
                var audioPage = (AudioSettingsPage)audPage;
                audioPage.SetClassicAudio(GameSettings.ClassicMusic);
            }
            if (Game1.UiPageManager.InsideElement.TryGetValue(typeof(CameraSettingsPage), out var camPage))
            {
                var cameraPage = (CameraSettingsPage)camPage;
                cameraPage.SetCameraMode(GameSettings.ClassicCamera);
                cameraPage.SetClassicDungeon(GameSettings.ClassicDungeon);
                cameraPage.SetClassicCamBorder(GameSettings.ClassicBorders);
                cameraPage.SetCameraLock(GameSettings.CameraLock);
                cameraPage.SetCameraSmoothCam(GameSettings.SmoothCamera);
            }
            // Force a scale change as switching from either camera to the other will require it.
            Game1.ScaleChanged = true;
        }

        public void StartNewGame(int slot, string slotName)
        {
            ResetStuff();

            SaveSlot = slot;
            SaveName = slotName;
            TotalPlaytime = 0.0f;
            CurrentSessionPlaytime = 0.0f;

            Equipment = new GameItemCollected[EquipmentSlots];

            UpdateEquipment();

            SaveManager.Reset();

            // Save file versions:
            // 0: The "save_version" key doesn't exist.
            // 1: Seashell Mansion & "Nothing is Missable" enabled.
            // 2: World teleporter indexes fixed.
            // 3: Dungeon 3 has a new map.
            // 4: New games may have "cleared" state when they shouldn't.
            SaveManager.SetString("save_version", SaveFileVersion);

            // set up values
            // debug fill the inventory
            if (RealSaveName == "DebugMode")
            {
                DebugMode = true;

                CollectItem(new GameItemCollected("sword1") { Count = 1 }, 0);
                CollectItem(new GameItemCollected("shield") { Count = 1 }, 0);
                CollectItem(new GameItemCollected("feather") { Count = 1 }, 0);
                CollectItem(new GameItemCollected("stonelifter") { Count = 1 }, 0);
                CollectItem(new GameItemCollected("pegasusBoots") { Count = 1 }, 0);
                CollectItem(new GameItemCollected("shovel") { Count = 1 }, 0);
                CollectItem(new GameItemCollected("flippers") { Count = 1 }, 0);
                CollectItem(new GameItemCollected("magicRod") { Count = 1 }, 0);
                CollectItem(new GameItemCollected("hookshot") { Count = 1 }, 0);
                CollectItem(new GameItemCollected("boomerang") { Count = 1 }, 0);
                CollectItem(new GameItemCollected("powder") { Count = 999 }, 0);
                CollectItem(new GameItemCollected("bomb") { Count = 999 }, 0);
                CollectItem(new GameItemCollected("bow") { Count = 999 }, 0);
                CollectItem(new GameItemCollected("ocarina") { Count = 1 }, 0);
            }
            CollectedItems.Clear();
            DungeonMaps.Clear();
            ItemDrawHelper.Init();

            CloakType = CloakGreen;
            ThiefState = false;
            GameCleared = false;
            HasMagnifyingLens = false;
            GuardianAcornIsActive = false;
            PieceOfPowerIsActive = false;

            MaxHearts = 3;
            CurrentHealth = 12;
            KillCount = 0;
            DeathCount = 0;
            SwordLevel = 0;
            ShieldLevel = 0;
            StoneGrabberLevel = 0;
            GuardianAcornCount = 0;
            GuardianAcornDamageCount = 0;
            PieceOfPowerCount = 0;
            PieceOfPowerDamageCount = 0;
            SelectedOcarinaSong = 0;
            OcarinaSongs[0] = 0;
            OcarinaSongs[1] = 0;
            OcarinaSongs[2] = 0;

            PlayerMapPosition = null;
            MapVisibility = new bool[16, 16];

            // randomize the directions of the egg
            SaveManager.SetString("eggDirections", Game1.RandomNumber.Next(0, 4).ToString());

            // create empty map
            MapManager.CurrentMap = Map.Map.CreateEmptyMap();

            Link.Map = MapManager.CurrentMap;
            Link.MapTransitionStart = Link.EntityPosition.Position;
            Link.MapTransitionEnd = Link.EntityPosition.Position;
            Link.EntityPosition.Z = 0;
            Link.TransitionOutWalking = false;
            Link.TransitionInWalking = false;
            Link.BlackScreenOverride = true;
            Link.InitGame();
            MapManager.Camera.ForceUpdate(MapManager.GetCameraTargetLink());

            // load the map
            Link.SetNextMapPosition(new Vector2(Link.PosX, Link.PosY));
            ((MapTransitionSystem)GameSystems[typeof(MapTransitionSystem)]).LoadMapFromFile("house1.map", true, true, Values.MapFirstTransitionColor, false);
            ((MapTransitionSystem)GameSystems[typeof(MapTransitionSystem)]).AdditionalBlackScreenDelay = Values.GameSaveBlackScreen;
        }

        public void LoadSaveFile(int slot)
        {
            ResetStuff();

            Link.InitGame();

            // Load the values from "save#" and "saveGame#". 
            SaveGameSaveLoad.LoadSaveFile(this, slot);

            // Fixes changes to save files that are now invalid.
            SaveFileFix_v0();
            SaveFileFix_v1();
            SaveFileFix_v2();
            SaveFileFix_v3();

            // Item and equipment preparations.
            ItemDrawHelper.Init();
            UpdateEquipment();

            // Create a new empty map file to load objects into and put Link on it.
            MapManager.CurrentMap = Map.Map.CreateEmptyMap();
            MapManager.CurrentMap.Objects.SpawnObject(Link);

            // These are set from the "SaveGameSaveLoad.LoadSaveFile()" call from above.
            Link.Map = MapManager.CurrentMap;
            Link.SaveMap = LoadedMap;
            Link.SavePosition.X = SavePositionX;
            Link.SavePosition.Y = SavePositionY;
            Link.SaveDirection = SaveDirection;
            Link.Direction = SaveDirection;
            Link.DirectionEntry = SaveDirection;
            Link.SetWalkingDirection(SaveDirection);

            // Set up the camera.
            MapManager.CameraOffset = Vector2.Zero;
            MapManager.Camera.ForceUpdate(MapManager.GetCameraTargetLink());

            // Set up the map transition stuff.
            Link.MapTransitionStart = Link.EntityPosition.Position;
            Link.MapTransitionEnd = Link.EntityPosition.Position;
            Link.TransitionOutWalking = false;
            Link.TransitionInWalking = false;
            Link.BlackScreenOverride = true;

            // Default Z-Position to zero.
            Link.EntityPosition.Z = 0;

            // This value is an override for the low health beep. When true it does not force the beep to play
            // but if it's false, it does force it to not play. This is used for the ending sequence.
            Link.ToggleLowHealthBeep(true);

            // load the map
            var transitionSystem = ((MapTransitionSystem)GameSystems[typeof(MapTransitionSystem)]);
            Link.SetNextMapPosition(new Vector2(SavePositionX, SavePositionY));
            transitionSystem.LoadMapFromFile(LoadedMap, true, true, Values.MapFirstTransitionColor, false);
            transitionSystem.AdditionalBlackScreenDelay = Values.GameSaveBlackScreen;

            // If the game was saved frozen or the inventory disabled, unfreeze and enable the inventory.
            Link.FreezeAnimations(false);
            Link.DisableInventory(false);
        }

        private void SaveFileFix_v0()
        {
            // If second_chance doesn't exist and the Level 7/8 dungeons have been completed.
            var secondchance = GetItem("second_chance") == null;
            var instrument07 = GetItem("instrument6") != null;
            var instrument08 = GetItem("instrument7") != null;

            // Add the second chance key and set it to 1.
            if (secondchance && instrument07 && instrument08)
                SaveManager.SetString("second_chance", "1");
        }

        private void SaveFileFix_v1()
        {
            // Fixes teleporter IDs on version 1 save files.
            string saveVersionStr = SaveManager.GetString("save_version", "0");
            int.TryParse(saveVersionStr, out int saveVersion);

            // This only affects v0 and v1 save files.
            if (saveVersion <= 1)
            {
                // Get the unlocked state of the teleporters that have reversed indexes.
                string teleporter1 = SaveManager.GetString("unlocked_teleporter_1", "none");
                string teleporter2 = SaveManager.GetString("unlocked_teleporter_2", "none");

                // Only modify if one them is unlocked and the other isn't.
                if (teleporter1 != teleporter2)
                {
                    // Reverse the unlocked teleporters.
                    if (teleporter1 == "1")
                    {
                        SaveManager.SetString("unlocked_teleporter_2", "1");
                        SaveManager.RemoveString("unlocked_teleporter_1");
                    }
                    if (teleporter2 == "1") 
                    {
                        SaveManager.SetString("unlocked_teleporter_1", "1");
                        SaveManager.RemoveString("unlocked_teleporter_2");
                    }
                }
                // Increment the save version to 2 so the next fix picks it up.
                SaveManager.SetString("save_version", "2");
            }
        }

        private void SaveFileFix_v2()
        {
            // Fixes Dungeon 3 map name on versions 1 and 2 save files.
            string saveVersionStr = SaveManager.GetString("save_version", "0");
            int.TryParse(saveVersionStr, out int saveVersion);

            // Check the save file name. Just to be safe, also make sure the dungeon name is correct.
            if (saveVersion < 3 || LoadedMap == "dungeon3_1.map")
            {
                // In v1.4.8 Dungeon 3 map file has changed so we need to convert to new name and coordinates.
                if (LoadedMap == "dungeon3_1.map")
                {
                    // Fix the GameManager values.
                    LoadedMap = "dungeon3.map";
                    SavePositionX = 256;
                    SavePositionY = 1032;

                    // May as well fix the save file values as well.
                    SaveManager.SetString("currentMap", LoadedMap);
                    SaveManager.SetInt("posX", SavePositionX);
                    SaveManager.SetInt("posY", SavePositionY);
                }
                // Increment the save version.
                SaveManager.SetString("save_version", "3");
            }
        }

        private void SaveFileFix_v3()
        {
            // Fixes "game cleared" on save files where it wasn't actually cleared.
            string saveVersionStr = SaveManager.GetString("save_version", "0");
            int.TryParse(saveVersionStr, out int saveVersion);

            // Check if the save file is below version 4.
            if (saveVersion < 4)
            {
                // Check to see if the game is really cleared.
                bool notInEgg = SaveManager.GetString("egg_enter_end") == null;

                // It's not perfect, but it's the closest value we have to the end.
                if (notInEgg)
                    GameCleared = false;
            }
            // Increment the save version.
            SaveManager.SetString("save_version", SaveFileVersion);
        }

        public void RespawnPlayer()
        {
            if (SaveManager.HistoryEnabled)
            {
                SaveManager.RevertHistory();
                SaveManager.DisableHistory();
            }
            ResetStuff();

            // create empty map
            MapManager.CurrentMap = Map.Map.CreateEmptyMap();
            MapManager.CurrentMap.Objects.SpawnObject(Link);

            Link.Map = MapManager.CurrentMap;
            MapManager.Camera.ForceUpdate(MapManager.GetCameraTargetLink());

            // respawn the player
            Link.Respawn();
            ItemDrawHelper.Init();

            Link.MapTransitionStart = Link.EntityPosition.Position;
            Link.MapTransitionEnd = Link.EntityPosition.Position;
            Link.TransitionOutWalking = false;
            Link.TransitionInWalking = false;
            Link.BlackScreenOverride = true;

            // respawn looking down
            Link.DirectionEntry = 3;
            Link.SetWalkingDirection(3);
            Link.SetNextMapPosition(Link.SavePosition);

            // load the map
            var transitionSystem = ((MapTransitionSystem)GameSystems[typeof(MapTransitionSystem)]);
            transitionSystem.LoadMapFromFile(Link.SaveMap, true, true, Values.MapFirstTransitionColor, false);
            transitionSystem.AdditionalBlackScreenDelay = Values.GameRespawnBlackScreen;
        }

        private void ResetStuff()
        {
            SaveGameSaveLoad.ClearSaveState();
            SaveManager.DisableHistory();

            // This was done to support DialogActionCooldown working after loading a new save.
            Game1.TotalGameTime = 0;
            Game1.TotalGameTimeLast = 0;
            Game1.FreezeTime = 0;

            _shakeCountX = 0;
            _shakeCountY = 0;
        }
    }
}