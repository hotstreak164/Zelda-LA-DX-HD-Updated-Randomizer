﻿using System;
using System.IO;
using System.Threading;
using GBSPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectZ.Base;
using ProjectZ.Base.UI;
using ProjectZ.InGame.Controls;
using ProjectZ.InGame.GameObjects;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.Pages;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Screens;
using ProjectZ.InGame.Things;

namespace ProjectZ
{
    public class Game1 : Game
    {
        public static Game1 Instance;
        public static GraphicsDeviceManager Graphics;
        public static SpriteBatch SpriteBatch;
        public static UiManager UiManager = new UiManager();
        public static ScreenManager ScreenManager = new ScreenManager();
        public static PageManager UiPageManager = new PageManager();
        public static Language LanguageManager = new Language();
        public static GameManager GameManager = new GameManager();
        public static GbsPlayer GbsPlayer = new GbsPlayer();
        public static Random RandomNumber = new Random();
        public static CameraField ClassicCamera = new CameraField();
        public static IEditorManager EditorManager;

        public static int WindowWidth;
        public static int WindowHeight;
        public static int WindowWidthEnd;
        public static int WindowHeightEnd;
        public static int RenderWidth;
        public static int RenderHeight;

        public static bool FullScreen;
        public static bool WasExclusive;

        private bool _firstFrameDrawn;
        private bool _fullscreenWasSet;

        private static bool _forceFullScreen = false;

        private static int _lastWindowWidth;
        private static int _lastWindowHeight;

    #if DIRECTX
        private static System.Windows.Forms.Form _windowForm;
        private static System.Drawing.Rectangle _lastWindowBounds;
        private static bool _isBorderless;
    #endif

        public static bool FpsSettingChanged;
        private readonly SimpleFps _fpsCounter = new SimpleFps();

        public static double FreezeTime;
        public static float TimeMultiplier;
        public static float DeltaTime;
        public static double TotalTime;
        public static double TotalGameTime;
        public static double TotalGameTimeLast;

        private static DoubleAverage _avgTotalMs = new DoubleAverage(30);
        private static DoubleAverage _avgTimeMult = new DoubleAverage(30);

        public static RenderTarget2D MainRenderTarget;
        private static RenderTarget2D _renderTarget1;
        private static RenderTarget2D _renderTarget2;
        private static bool _initRenderTargets;

        private const double _startDelayTime = 1.5;
        private double _startDelayElapsed;
        private bool _startDelayFinished;

        public static int MaxGameScale;
        public static int UiScale;
        public static bool ScaleChanged;

        public static bool WasActive;
        public static bool UpdateGame;
        public static bool ForceDialogUpdate;
        public static bool EditorMode;
        public static bool SaveAndExitGame;
        public static bool AutoLoadSave;
        public static int AutoLoadSlot;

    #if ANDROID
        private static int _androidSurfaceWidthHint;
        private static int _androidSurfaceHeightHint;
    #endif

        private static volatile bool _finishedLoading;
        private static volatile bool _isExiting;

        public static string DebugText;
        public static float DebugTimeScale = 1.0f;
        public static bool DebugStepper;
        public static int DebugLightMode;
        public static int DebugBoxMode;
        public static bool DebugMode;
        public static bool ShowDebugText;
        private Vector2 _debugTextSize;

        // Keyboard keys for debug functions.
        public static Keys DebugEditor        = Keys.Escape;
        public static Keys DebugToggleText    = Keys.F1;
        public static Keys DebugToggleMode    = Keys.F2;
        public static Keys DebugBox           = Keys.F3;
        public static Keys DebugReloadMap     = Keys.F4;
        public static Keys DebugSaveKey       = Keys.F5;
        public static Keys DebugLoadKey       = Keys.F6;
        public static Keys DebugShadowKey     = Keys.F9;
        public static Keys DebugStepStart     = Keys.Insert;
        public static Keys DebugStepNext      = Keys.Home;
        public static Keys DebugTimeScaleUp   = Keys.Add;
        public static Keys DebugTimeScaleDown = Keys.Subtract;

        // True when in-game after selecting save file. False at main menu and intro.
        public static bool InProgress;

        public static bool FinishedLoading => _finishedLoading;

    #if ANDROID
        public static void SetAndroidSurfaceSizeHint(int width, int height)
        {
            if (width > 0 && height > 0)
            {
                _androidSurfaceWidthHint = width;
                _androidSurfaceHeightHint = height;
            }
        }
    #endif

        public static Matrix GetMatrix
        {
            get
            {
                var gd = Instance?.GraphicsDevice;
                if (gd == null || WindowWidth <= 0 || WindowHeight <= 0)
                    return Matrix.Identity;

                // Viewport represents the actual usable physical pixels (e.g. safe area on devices with a "notch")
                var viewport = gd.Viewport;
                return Matrix.CreateScale(
                    (float)viewport.Width / WindowWidth,
                    (float)viewport.Height / WindowHeight, 1f);
            }
        }
        // lahdmod values
        private int  max_game_scale = 20;
        private bool editor_mode = false;

        public Game1(bool editorMode, bool loadSave, int loadSlot)
        {
            // Detect when the game is exiting.
            Exiting += OnGameExiting;

            // If a mod file exists load the values from it.
            string modFile = Path.Combine(Values.PathLAHDMods, "Game1.lahdmod");
            ModFile.Parse(modFile, this);

            // Use max game scale set by lahdmod file or default value set above.
            MaxGameScale = max_game_scale;
            GameSettings.GameScale = max_game_scale + 1;

            // Enable editor via lahdmod file or through the command line option.
            EditorMode = editorMode || editor_mode;

            // Create the graphics device and set the back buffer width/height.
            Graphics = new GraphicsDeviceManager(this);

        #if DEBUG
            EditorMode = true;
        #endif

            Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Graphics.PreferredBackBufferWidth = Values.MinWidth * 3;
            Graphics.PreferredBackBufferHeight = Values.MinHeight * 3;

            // Store the original window size to return to if started in fullscreen mode.
            _lastWindowWidth  = Graphics.PreferredBackBufferWidth;
            _lastWindowHeight = Graphics.PreferredBackBufferHeight;


        #if ANDROID
            if (_androidSurfaceWidthHint > 0 && _androidSurfaceHeightHint > 0)
            {
                Graphics.PreferredBackBufferWidth = _androidSurfaceWidthHint;
                Graphics.PreferredBackBufferHeight = _androidSurfaceHeightHint;
            }
            else
            {
                var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                if (displayMode.Width > 0 && displayMode.Height > 0)
                {
                    Graphics.PreferredBackBufferWidth = displayMode.Width;
                    Graphics.PreferredBackBufferHeight = displayMode.Height;
                }
            }
        #endif

            Graphics.ApplyChanges();

            // Allow the user to resize the window.
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += (_, __) => OnResize();
            Window.AllowAltF4 = true;

        #if !ANDROID
            Window.KeyDown += OnWindowKeyDown;
        #endif

            // Store any command line parameters if available.
            IsMouseVisible = EditorMode;
            AutoLoadSave = loadSave;
            AutoLoadSlot = loadSlot;

            // Set the content directory.
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // Direct-X now requires this workaround to set the icon.
        #if DIRECTX
            var path = Path.Combine("Data", "Icon", "Icon.ico");
            var icon = new System.Drawing.Icon(path);
            var form = System.Windows.Forms.Form.FromHandle(Window.Handle) as System.Windows.Forms.Form;
            if (form != null)
            {
                form.Icon = icon;
                form.ShowIcon = true;
                _windowForm = form;
                _lastWindowBounds = form.Bounds; // capture starting window size as the restore target
            }
        #endif

            // Store an instance so it can be referenced.
            Instance = this;
            base.Initialize();
        }

    #if !ANDROID
        private void OnWindowKeyDown(object sender, InputKeyEventArgs e)
        {
            // Check if the "Alt" key is held.
            var keyState = Keyboard.GetState();
            bool altDown = keyState.IsKeyDown(Keys.LeftAlt) || keyState.IsKeyDown(Keys.RightAlt);

            // No sense in doing more than we need to.
            if (!altDown)
                return;

            // Alt+F4: Close the game. Windows DirectX build doesn't do it reliably by default.
            if (e.Key == Keys.F4)
            {
                InputHandler.ResetInputState();
                Exit();
            }
            // Alt+Enter: Toggles fullscreen mode.
            else if (e.Key == Keys.Enter)
            {
                // Get the graphics settings page.
                if (Game1.UiPageManager.InsideElement.TryGetValue(typeof(VideoSettingsPage), out var videoPage))
                {
                    var GraphicsSettingsPage = (VideoSettingsPage)videoPage;

                    // Reverse the current screen mode.
                    if (GameSettings.ScreenMode > 0)
                    {
                        GameSettings.ScreenMode = 0;
                        GraphicsSettingsPage.SetFullscreenMode(0);
                    }
                    else
                    {
                        GameSettings.ScreenMode = 1;
                        GraphicsSettingsPage.SetFullscreenMode(1);
                    }
                }
                // Toggle the screen mode and save that it was toggled.
                ToggleFullscreen();
                SettingsSaveLoad.SaveSettings();
            }
        }
    #endif

        protected override void LoadContent()
        {
            // Hook device reset function & create a new SpriteBatch to draw textures.
            GraphicsDevice.DeviceReset += OnDeviceReset;
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            // Initialize controller and input handler.
            ControlHandler.Initialize();
            Components.Add(new InputHandler(this));

        #if ANDROID
            // Load the on-screen controller if on Android.
            VirtualController.Initialize(Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight, true);
        #endif

            // Load the users saved settings.
            SettingsSaveLoad.LoadSettings();

            // Load the Intro Screen and its resources.
            GameManager.UpdateSoundEffects();
            Resources.LoadIntro(Graphics.GraphicsDevice, Content);
            ScreenManager.LoadIntro(Content);

            // Start loading the resources that are needed after the intro.
            ThreadPool.QueueUserWorkItem(LoadContentThreaded);

            // Initialize the GBS Player and load in the Link's Awakening GBS file.
            GbsPlayer.LoadFile(Path.Combine(Values.PathDataFolder, "Music", "awakening.gbs"));
            GbsPlayer.StartThread();

            // set the fps settings of the game
            UpdateFpsSettings();

            // Initialize extra monster hit points set by the user.
            EnemyLives.Initialize();

            // Update the inventory menu transparency if disable blur is enabled.
            // There's probably a better place to do this, but I'm tired.
            Values.InventoryBackgroundColorTop = GameSettings.OpaqueHudBg == true 
                ? new Color(255, 255, 230) * 0.95f
                : new Color(255, 255, 230) * 0.85f;
            Values.InventoryBackgroundColor = GameSettings.OpaqueHudBg == true 
                ? new Color(255, 255, 230) * 0.95f
                : new Color(255, 255, 230) * 0.75f;
        }

        private void LoadContentThreaded(Object obj)
        {
            // Works around a strange bug that crashes when the game closes.
            if (_isExiting) return;

            // Load all of the game's resources.
            Resources.LoadBlurEffect(Content);
            Resources.LoadTextures(Graphics.GraphicsDevice, Content);
            Resources.LoadSounds(Content);
            GameManager.Load(Content);

            // Set up all of the GameObject templates.
            GameObjectTemplates.SetUpGameObjects();

            // Finish loading in resources.
            ScreenManager.Load(Content);
            LanguageManager.Load();
            UiPageManager.Load(Content);

            // Set up the editor if enabled.
            if (EditorMode)
                EditorManager?.SetUpEditorUi();

            // Flag that the thread has finished loading in content.
            _finishedLoading = true;

            // Now that everything has been loaded in, make sure the proper language textures are reloaded.
            Resources.RefreshDynamicResources();
        }

        protected override void Update(GameTime gameTime)
        {
            // Startup black screen delay.
            if (!_startDelayFinished)
            {
                _startDelayElapsed += gameTime.ElapsedGameTime.TotalSeconds;

            #if ANDROID
                if (GraphicsDevice != null)
                {
                    var pp = GraphicsDevice.PresentationParameters;
                    if (pp.BackBufferWidth > 0 && pp.BackBufferHeight > 0)
                    {
                        if (WindowWidth != pp.BackBufferWidth || WindowHeight != pp.BackBufferHeight)
                            OnResize();
                    }
                }
            #else
                if (GraphicsDevice != null)
                {
                    var pp = GraphicsDevice.PresentationParameters;
                    var ppW = pp.BackBufferWidth > 0 ? pp.BackBufferWidth : Window.ClientBounds.Width;
                    var ppH = pp.BackBufferHeight > 0 ? pp.BackBufferHeight : Window.ClientBounds.Height;
                    if (WindowWidth != ppW || WindowHeight != ppH)
                        OnResize();
                }
                else if (WindowWidth != Window.ClientBounds.Width || WindowHeight != Window.ClientBounds.Height)
                    OnResize();
            #endif

                if (_startDelayElapsed < _startDelayTime)
                    return;

                _startDelayFinished = true;
            }
            // If exclusive fullscreen mode is enabled.
            if (_firstFrameDrawn && !_fullscreenWasSet)
            {
                // We need to delay it until the graphics device has been fully set up.
                if (GameSettings.ScreenMode > 0)
                {
                    ToggleFullscreen();
                }
                _fullscreenWasSet = true;
            }

            // Prevent input when window is in background (do we even want this?).
            WasActive = IsActive;

            // Mute music and sound effects if user disabled on inactive window.
            GameManager.HandleInactiveWindow(IsActive);

            // Updates the FPS counter.
            _fpsCounter.Update(gameTime);

            // Initialize render targets if thread is finished loading resources and they have not been initialized yet. 
            if (_finishedLoading && !_initRenderTargets)
            {
                _initRenderTargets = true;
                WindowWidth = 0;
                WindowHeight = 0;
                WindowWidthEnd = 0;
                WindowHeightEnd = 0;
            }

        #if ANDROID
            // On Android use PP as the source of truth, not ClientBounds
            if (GraphicsDevice != null)
            {
                var pp = GraphicsDevice.PresentationParameters;
                if (pp.BackBufferWidth > 0 && pp.BackBufferHeight > 0)
                {
                    if (WindowWidth != pp.BackBufferWidth || WindowHeight != pp.BackBufferHeight)
                        OnResize();
                }
            }
        #else
            // If the window size has changed then trigger a resize event.
            if ((WindowWidth != Window.ClientBounds.Width) || (WindowHeight != Window.ClientBounds.Height))
                OnResize();
        #endif

            // Update the scale if it has been changed.
            if (ScaleChanged)
                UpdateScale();

            // If the FPS settings has changed then update them.
            if (FpsSettingChanged)
            {
                UpdateFpsSettings();
                FpsSettingChanged = false;
            }

        #if ANDROID
            // Update the on-screen controller if on Android.
            VirtualController.Update();
        #endif

            // Update input from any input devices.
            ControlHandler.Update();

            // Update all render targets.
            UpdateRenderTargets();

            // When the content thread is finished loading.
            if (_finishedLoading)
            {
                if (EditorMode && EditorManager != null)
                {
                    UiManager.Update();
                    EditorManager.EditorUpdate(gameTime);
                }
                // Update the UI.
                UiManager.CurrentScreen = "";
                UiPageManager.Update(gameTime);
            }
            // If editor is enabled and F1 key is pressed.
            if (EditorMode && InputHandler.KeyPressed(DebugToggleText))
                ShowDebugText = !ShowDebugText;

            // Debug Stepper (N Key) is not active.
            if (!DebugStepper)
            {
                TimeMultiplier = gameTime.ElapsedGameTime.Ticks / 166667f * DebugTimeScale;
                TotalGameTimeLast = TotalGameTime;

                // limit the game time so that it slows down if the steps are bigger than they would be for 30fps
                // if the timesteps get too big it would be hard (wast of time) to make the logic still function 100% correctly
                if (TimeMultiplier > 2.0f)
                {
                    TimeMultiplier = 2.0f;
                    DeltaTime = (TimeMultiplier * 1000.0f) / 60.0f;
                    TotalTime += (TimeMultiplier * 1000.0) / 60.0;
                    DebugText += "\nLow Framerate";

                    if (UpdateGame)
                        TotalGameTime += (TimeMultiplier * 1000.0) / 60.0;
                }
                else
                {
                    DeltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds * DebugTimeScale;
                    TotalTime += gameTime.ElapsedGameTime.TotalMilliseconds * DebugTimeScale;
                    if (UpdateGame)
                        TotalGameTime += gameTime.ElapsedGameTime.TotalMilliseconds * DebugTimeScale;
                }
            }
            // Update the screen manager.
            UpdateGame = true;

            if (!DebugStepper || InputHandler.KeyPressed(Keys.M))
                ScreenManager.Update(gameTime);

            // Pump GBS audio on the game thread
            GbsPlayer.Pump();

            if (_finishedLoading && ShowDebugText)
            {
                DebugText += _fpsCounter.Msg;

                _avgTotalMs.AddValue(gameTime.ElapsedGameTime.TotalMilliseconds);
                _avgTimeMult.AddValue(TimeMultiplier);
                DebugText += $"\ntotal ms:      {_avgTotalMs.Average,6:N3}" +
                             $"\ntime mult:     {_avgTimeMult.Average,6:N3}" +
                             $"\ntime scale:    {DebugTimeScale}" +
                             $"\ntime:          {TotalGameTime}";

                DebugText += "\nHistory Enabled: " + GameManager.SaveManager.HistoryEnabled;

                if (MapManager.ObjLink != null)
                {
                    var pos = MapManager.ObjLink.EntityPosition;

                    DebugText += "\nPosition X: " + pos.X;
                    DebugText += "\nPosition Y: " + pos.Y;
                    DebugText += "\nPosition Z: " + pos.Z;
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (!_startDelayFinished)
            {
                GraphicsDevice.Clear(Color.Black);
                return;
            }
            _firstFrameDrawn = true;

            _fpsCounter.CountDraw();

            ScreenManager.DrawRT(SpriteBatch);

            if (MainRenderTarget == null)
            {
                GraphicsDevice.Clear(Color.CadetBlue);
                ScreenManager.Draw(SpriteBatch);
                return;
            }
            Graphics.GraphicsDevice.SetRenderTarget(MainRenderTarget);
            GraphicsDevice.Clear(Color.CadetBlue);

            ScreenManager.Draw(SpriteBatch);

            if (!GameSettings.OpaqueHudBg)
                BlurImage();
            {
                Graphics.GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Color.Black);

            #if ANDROID
                var pp = GraphicsDevice.PresentationParameters;
                var targetWidth = pp.BackBufferWidth;
                var targetHeight = pp.BackBufferHeight;

                if (targetWidth <= 0 || targetHeight <= 0)
                {
                    var cb = Window.ClientBounds;
                    targetWidth = cb.Width;
                    targetHeight = cb.Height;
                }

                if (targetWidth <= 0 || targetHeight <= 0)
                {
                    var viewport = GraphicsDevice.Viewport;
                    targetWidth = viewport.Width;
                    targetHeight = viewport.Height;
                }
            #else
                var viewport = GraphicsDevice.Viewport;
            #endif

                SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            #if ANDROID
                SpriteBatch.Draw(MainRenderTarget, new Rectangle(0, 0, targetWidth, targetHeight), Color.White);
            #else
                SpriteBatch.Draw(MainRenderTarget, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.White);
            #endif
                SpriteBatch.End();
            }

            if (!GameSettings.OpaqueHudBg)
            {
                if (_renderTarget2 != null)
                {
                    Resources.BlurEffect.Parameters["sprBlur"].SetValue(_renderTarget2);
                    Resources.RoundedCornerBlurEffect.Parameters["sprBlur"].SetValue(_renderTarget2);
                }
                var vp = GraphicsDevice.Viewport;

                // These are the dimensions SV_Position is normalized against in the current pass
                Resources.BlurEffect.Parameters["width"].SetValue(vp.Width);
                Resources.BlurEffect.Parameters["height"].SetValue(vp.Height);

                Resources.RoundedCornerBlurEffect.Parameters["screenWidth"].SetValue(vp.Width);
                Resources.RoundedCornerBlurEffect.Parameters["screenHeight"].SetValue(vp.Height);

                // Also prevent texture-unit-1 wrap at runtime (belt-and-suspenders)
                GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

                SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.AnisotropicClamp, null, null, Resources.RoundedCornerBlurEffect, GetMatrix);

                // blurred ui parts
                if (_finishedLoading)
                    UiManager.DrawBlur(SpriteBatch);

                // blured stuff
                GameManager?.InGameOverlay?.InGameHud?.DrawBlur(SpriteBatch);

                // background for the debug text
                DebugTextBackground();

                SpriteBatch.End();
            }
            else
            {
                SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, GetMatrix);
                GameManager?.InGameOverlay?.InGameHud?.DrawSaveIcon(SpriteBatch);
                SpriteBatch.End();
            }

            {
                // draw the top part
                SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, null, null, null, GetMatrix);

                // draw the ui part
                if (_finishedLoading)
                    UiManager.Draw(SpriteBatch);

                // draw the game ui
                UiPageManager.Draw(SpriteBatch);

                // draw the screen tops
                ScreenManager.DrawTop(SpriteBatch);

            #if ANDROID
                VirtualController.Draw(SpriteBatch);
            #endif

                // draw the debug text
                DrawDebugText();
                DebugText = "";

            #if DEBUG
                if (GameManager.SaveManager.HistoryEnabled)
                    SpriteBatch.Draw(Resources.SprWhite, new Rectangle(0, WindowHeight - 6, WindowWidth, 6), Color.Red);
            #endif

                SpriteBatch.End();
            }
            base.Draw(gameTime);
        }

        private void BlurImage()
        {
            if (MainRenderTarget == null || _renderTarget1 == null || _renderTarget2 == null)
                return;

            var blurValue = 0.2f;

            if (Resources.BlurEffectH == null || Resources.BlurEffectV == null)
                return;

            Resources.BlurEffectH.Parameters["pixelX"].SetValue(1.0f / _renderTarget1.Width);
            Resources.BlurEffectV.Parameters["pixelY"].SetValue(1.0f / _renderTarget1.Height);

            var mult0 = blurValue;
            var mult1 = (1 - blurValue * 2) / 2;
            Resources.BlurEffectH.Parameters["mult0"].SetValue(mult0);
            Resources.BlurEffectH.Parameters["mult1"].SetValue(mult1);
            Resources.BlurEffectV.Parameters["mult0"].SetValue(mult0);
            Resources.BlurEffectV.Parameters["mult1"].SetValue(mult1);

            Graphics.GraphicsDevice.SetRenderTarget(_renderTarget2);
            SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.AnisotropicClamp, null, null, null, null);
            SpriteBatch.Draw(MainRenderTarget, new Rectangle(0, 0, _renderTarget2.Width, _renderTarget2.Height), Color.White);
            SpriteBatch.End();

            for (var i = 0; i < 2; i++)
            {
                // v blur
                Graphics.GraphicsDevice.SetRenderTarget(_renderTarget1);
                SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.AnisotropicClamp, null, null, Resources.BlurEffectV, null);
                SpriteBatch.Draw(_renderTarget2, Vector2.Zero, Color.White);
                SpriteBatch.End();

                // h blur
                Graphics.GraphicsDevice.SetRenderTarget(_renderTarget2);
                SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.AnisotropicClamp, null, null, Resources.BlurEffectH, null);
                SpriteBatch.Draw(_renderTarget1, Vector2.Zero, Color.White);
                SpriteBatch.End();
            }
        }

        public void TriggerFpsSettings()
        {
            if (!IsFixedTimeStep)
            {
                IsFixedTimeStep = true;
                Graphics.SynchronizeWithVerticalRetrace = false;
            }
            else
            {
                IsFixedTimeStep = false;
                Graphics.SynchronizeWithVerticalRetrace = true;
            }
            Graphics.ApplyChanges();
        }

        public static void ToggleFullscreen()
        {
            #if ANDROID
                _forceFullScreen = true;
            #endif

            if (_forceFullScreen || GameSettings.ScreenMode > 0)
            {
                FullScreen = GameSettings.ScreenMode > 0;

                if (GameSettings.ScreenMode == 1)
                {
                    // Borderless fullscreen
                #if DIRECTX
                    if (_windowForm != null)
                    {
                        // Only save bounds when coming from windowed, not from exclusive
                        if (!_isBorderless && !WasExclusive)
                        {
                            _lastWindowWidth  = Graphics.PreferredBackBufferWidth;
                            _lastWindowHeight = Graphics.PreferredBackBufferHeight;
                            _lastWindowBounds = _windowForm.Bounds;
                        }

                        // If coming from exclusive fullscreen, exit it first
                        if (WasExclusive)
                        {
                            Graphics.IsFullScreen = false;
                            Graphics.ApplyChanges();
                        }

                        var screenBounds = System.Windows.Forms.Screen.GetBounds(_windowForm);

                        _windowForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                        _windowForm.WindowState     = System.Windows.Forms.FormWindowState.Normal;
                        _windowForm.Bounds          = screenBounds;

                        Graphics.PreferredBackBufferWidth  = screenBounds.Width;
                        Graphics.PreferredBackBufferHeight = screenBounds.Height;
                        Graphics.ApplyChanges();

                        WindowWidth  = screenBounds.Width;
                        WindowHeight = screenBounds.Height;
                        ScaleChanged = true;

                        _isBorderless = true;
                        WasExclusive  = false;

                        GameManager?.UpdateRenderTargets();
                        return;
                    }
                #endif
                    // Non-DirectX fallback
                    if (!WasExclusive)
                    {
                        _lastWindowWidth  = Graphics.PreferredBackBufferWidth;
                        _lastWindowHeight = Graphics.PreferredBackBufferHeight;
                    }
                    Graphics.HardwareModeSwitch = false;
                    Graphics.IsFullScreen = true;
                    Graphics.ApplyChanges();
                    WasExclusive = false;
                }
                else if (GameSettings.ScreenMode == 2)
                {
                    // Exclusive fullscreen

                // Save bounds only when coming from windowed
                #if DIRECTX
                    if (!_isBorderless && !WasExclusive)
                #else
                    if (!WasExclusive)
                #endif
                    {
                        _lastWindowWidth  = Graphics.PreferredBackBufferWidth;
                        _lastWindowHeight = Graphics.PreferredBackBufferHeight;
                #if DIRECTX
                        if (_windowForm != null)
                            _lastWindowBounds = _windowForm.Bounds;
                #endif
                    }

                    // If coming from borderless, restore WinForms window first
                #if DIRECTX
                    if (_isBorderless && _windowForm != null)
                    {
                        _windowForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
                        _windowForm.Bounds = _lastWindowBounds.Width > 0
                            ? _lastWindowBounds
                            : new System.Drawing.Rectangle(100, 100, Values.MinWidth * 3, Values.MinHeight * 3);
                        _isBorderless = false;
                    }
                #endif

                    var dm = Game1.Graphics.GraphicsDevice.Adapter.CurrentDisplayMode;
                    Graphics.PreferredBackBufferWidth  = dm.Width;
                    Graphics.PreferredBackBufferHeight = dm.Height;
                    Graphics.HardwareModeSwitch = true;
                    Graphics.IsFullScreen = true;
                    Graphics.ApplyChanges();
                    WasExclusive = true;
                }
            }
            else
            {
                // Windowed mode
                GameSettings.ScreenMode = 0;
                FullScreen   = false;
                WasExclusive = false;

            #if DIRECTX
                if (_windowForm != null)
                {
                    var restoreBounds = (_lastWindowBounds.Width > 0 && _lastWindowBounds.Height > 0)
                        ? _lastWindowBounds
                        : new System.Drawing.Rectangle(100, 100, Values.MinWidth * 3, Values.MinHeight * 3);

                    _windowForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
                    _windowForm.Bounds          = restoreBounds;
                    _isBorderless = false;

                    // Use saved client size, not window bounds (which includes title bar and borders)
                    int restoreW = _lastWindowWidth  > 0 ? _lastWindowWidth  : Values.MinWidth  * 3;
                    int restoreH = _lastWindowHeight > 0 ? _lastWindowHeight : Values.MinHeight * 3;
                    Graphics.PreferredBackBufferWidth  = restoreW;
                    Graphics.PreferredBackBufferHeight = restoreH;
                    Graphics.IsFullScreen = false;
                    Graphics.ApplyChanges();

                    WindowWidth  = restoreW;
                    WindowHeight = restoreH;
                    ScaleChanged = true;

                    GameManager?.UpdateRenderTargets();
                    return;
                }
            #endif

                // Non-DirectX fallback
                int fallbackW = _lastWindowWidth  > 0 ? _lastWindowWidth  : Values.MinWidth  * 3;
                int fallbackH = _lastWindowHeight > 0 ? _lastWindowHeight : Values.MinHeight * 3;

                Graphics.PreferredBackBufferWidth  = fallbackW;
                Graphics.PreferredBackBufferHeight = fallbackH;
                Graphics.IsFullScreen = false;
                Graphics.HardwareModeSwitch = true;
                Graphics.ApplyChanges();
            }
            GameManager?.UpdateRenderTargets();
        }

        public void UpdateFpsSettings()
        {
            IsFixedTimeStep = false;
            Graphics.SynchronizeWithVerticalRetrace = GameSettings.VerticalSync;
            Graphics.ApplyChanges();
        }

        public void DebugTextBackground()
        {
            if (!ShowDebugText)
                return;

            _debugTextSize = GameFS.MeasureString(DebugText);

            SpriteBatch.Draw(_renderTarget2, new Rectangle(0, 0,
                (int)(_debugTextSize.X * 2) + 20, (int)(_debugTextSize.Y * 2) + 20), Color.White);
        }

        public void DrawDebugText()
        {
            if (!ShowDebugText)
                return;

            SpriteBatch.Draw(Resources.SprWhite, new Rectangle(0, 0, (int)(_debugTextSize.X * 2) + 20, (int)(_debugTextSize.Y * 2) + 20), Color.Black * 0.75f);

            GameFS.DrawString(SpriteBatch, DebugText, new Vector2(10), Color.White, 0, Vector2.Zero, 2f, SpriteEffects.None, 0);
        }

        private void OnDeviceReset(object sender, EventArgs e)
        {
            // Update render targets when device resets.
            GameManager?.UpdateRenderTargets();
            UpdateRenderTargetSizes(WindowWidth, WindowHeight);
        }

        private void OnResize()
        {
            int w = 0, h = 0;

            #if ANDROID
                if (GraphicsDevice == null) return;
            #endif

            if (GraphicsDevice != null)
            {
                var pp = GraphicsDevice.PresentationParameters;
                w = pp.BackBufferWidth;
                h = pp.BackBufferHeight;
            }

            if (w <= 0 || h <= 0)
            {
                w = Window.ClientBounds.Width;
                h = Window.ClientBounds.Height;
            }

            if (w <= 0 || h <= 0)
                return;

            #if !ANDROID
                if (GameSettings.ScreenMode == 0)
                {
                    int minW = Values.MinWidth;
                    int minH = Values.MinHeight;
                    if (w < minW || h < minH)
                    {
                        Graphics.PreferredBackBufferWidth  = Math.Max(w, minW);
                        Graphics.PreferredBackBufferHeight = Math.Max(h, minH);
                        Graphics.ApplyChanges();
                        var pp = GraphicsDevice.PresentationParameters;
                        w = pp.BackBufferWidth > 0 ? pp.BackBufferWidth : Window.ClientBounds.Width;
                        h = pp.BackBufferHeight > 0 ? pp.BackBufferHeight : Window.ClientBounds.Height;
                    }
                }
            #endif

            WindowWidth = w;
            WindowHeight = h;
            ScaleChanged = true;
        }

        public void ForceRecalculateScaling()
        {
            // Pull the current actual client size.
        #if ANDROID
            int w = 0, h = 0;
            if (GraphicsDevice != null)
            {
                var pp = GraphicsDevice.PresentationParameters;
                w = pp.BackBufferWidth;
                h = pp.BackBufferHeight;
            }
            if (w <= 0 || h <= 0) return;
        #else
            int w = Window.ClientBounds.Width;
            int h = Window.ClientBounds.Height;
            if (w <= 0 || h <= 0)
                return;
        #endif

            // Update the current window dimensions.
            WindowWidth = w;
            WindowHeight = h;

            // Force rescale to correct the size of render targets. 
            ScaleChanged = true;
            UpdateScale();

            // Force the render target resize as well.
            WindowWidthEnd = 0;
            WindowHeightEnd = 0;
            UpdateRenderTargets();
        }

        private void UpdateScale()
        {
            if (Camera.ClassicMode && GameSettings.ClassicScaling)
            {
                // Force integer scale or the field rect will be thrown off. The scaling value is calculated using the original dimensions of the
                // Game Boy version of Link's Awakening, minus the 16 pixels HUD ( 160x144 >> 160x128 ) so higher scaling values can be achieved.
                int gameScale = Math.Max(1, Math.Min(WindowWidth / 160, WindowHeight / 128));

                // Super Game Boy border is enabled. Calculate from the base resolution of the border instead.
                if (GameSettings.ClassicBorder == 2)
                    gameScale = Math.Max(1, Math.Min(WindowWidth / 256, WindowHeight / 224));

                // Send the game scale to the proper places it needs to go.
                MapManager.Camera.Scale = gameScale;
                GameManager.SetGameScale(gameScale);
            }
            else
            {
                // Get the maximum scale and add 1 for auto-scale.
                int maxScale = MaxGameScale + 1;

                // Calculate the game scale that is used for auto scaling.
                float gameScale = MathHelper.Clamp(Math.Min(WindowWidth / 160, WindowHeight / 128), 1, maxScale);
                float usedScale = gameScale;

                if (GameSettings.GameScale == maxScale)
                    usedScale = gameScale / 2;

                // If set to autoscale (Game1.MaxGameScale + 1) used the calculated value; otherwise use the value set by the user.
                MapManager.Camera.Scale = GameSettings.GameScale == maxScale
                    ? MathF.Ceiling(usedScale) 
                    : GameSettings.GameScale;

                // The camera scale uses a float value and can use a fractional scaling value when drawing the world.
                if (MapManager.Camera.Scale < 1)
                {
                    MapManager.Camera.Scale = 1 / (2 - MapManager.Camera.Scale);
                    GameManager.SetGameScale(1);
                }
                // If it's 1x or greater. We use "gameScale" directly here as a float as it allows fractional 
                // values while manually setting the scale only allows upscaling using integer values.
                else
                {
                    float newGameScale = GameSettings.GameScale == maxScale
                        ? MathF.Ceiling(usedScale)
                        : GameSettings.GameScale;
                    GameManager.SetGameScale(newGameScale);
                }
            }
            // Scale of the user interface.
            int interfaceScale = MathHelper.Clamp(Math.Min(WindowWidth / Values.MinWidth, WindowHeight / Values.MinHeight), 1, 11);

            if (GameSettings.UiScale > interfaceScale)
                UiScale = interfaceScale;
            else
                UiScale = GameSettings.UiScale == 0 
                    ? interfaceScale 
                    : MathHelper.Clamp(GameSettings.UiScale, 1, interfaceScale);

            // Call all of the "OnResize" methods to recalculate render targets.
            if (_finishedLoading)
                GameManager?.OnResize();
            UiManager?.OnResize();
            ScreenManager?.OnResize(WindowWidth, WindowHeight);
            UiPageManager?.OnResize(WindowWidth, WindowHeight);

        #if ANDROID
            // Update virtual button sizes.
            VirtualController.Initialize(Game1.WindowWidth, Game1.WindowHeight, null);
        #endif

            // This needs to go false or it will run every loop.
            ScaleChanged = false;
        }

        private void UpdateRenderTargets()
        {
            if (WindowWidthEnd == WindowWidth && WindowHeightEnd == WindowHeight)
                return;

            WindowWidthEnd = WindowWidth;
            WindowHeightEnd = WindowHeight;

            UpdateRenderTargetSizes(WindowWidth, WindowHeight);

            ScreenManager.OnResizeEnd(WindowWidth, WindowHeight);

            if (_finishedLoading)
                GameManager?.OnResizeEnd();
        }

        private void UpdateRenderTargetSizes(int width, int height)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);

            if (_finishedLoading)
            {
                if (Resources.BlurEffect != null)
                {
                    Resources.BlurEffect.Parameters["width"]?.SetValue(width);
                    Resources.BlurEffect.Parameters["height"]?.SetValue(height);
                }
                if (Resources.RoundedCornerBlurEffect != null)
                {
                    Resources.RoundedCornerBlurEffect.Parameters["textureWidth"]?.SetValue(width);
                    Resources.RoundedCornerBlurEffect.Parameters["textureHeight"]?.SetValue(height);
                }
            }
            var blurScale = MathHelper.Clamp(MapManager.Camera.Scale / 2, 1, 10);
            var blurRtWidth = Math.Max(1, (int)(width / blurScale));
            var blurRtHeight = Math.Max(1, (int)(height / blurScale));

            RenderTarget2D newMain = null;
            RenderTarget2D newRt1 = null;
            RenderTarget2D newRt2 = null;

            try
            {
                newMain = new RenderTarget2D(Graphics.GraphicsDevice, width, height);
                newRt1 = new RenderTarget2D(Graphics.GraphicsDevice, blurRtWidth, blurRtHeight);
                newRt2 = new RenderTarget2D(Graphics.GraphicsDevice, blurRtWidth, blurRtHeight);
            }
            catch (Exception)
            {
                newMain?.Dispose();
                newRt1?.Dispose();
                newRt2?.Dispose();
                return;
            }
            MainRenderTarget?.Dispose();
            _renderTarget1?.Dispose();
            _renderTarget2?.Dispose();

            MainRenderTarget = newMain;
            _renderTarget1 = newRt1;
            _renderTarget2 = newRt2;
        }

        private void DisposeRenderTargets()
        {
            // Dispose main render target.
            MainRenderTarget?.Dispose();
            MainRenderTarget = null;

            // Dispose render target 1.
            _renderTarget1?.Dispose();
            _renderTarget1 = null;

            // Dispose render target 2.
            _renderTarget2?.Dispose();
            _renderTarget2 = null;
        }

        private void OnGameExiting(object sender, EventArgs e)
        {
            // Stop the game loop so it doesn't do anything new.
            UpdateGame = false;
            _isExiting = true;

            // Shut down the GBS Player when closing.
            GbsPlayer.OnExit();

            // Try to prevent a crash with OpenGL disposing textures.
            try
            {
                // Dispose all render targets.
                DisposeRenderTargets();
                GameManager?.DisposeRenderTargets(true);

                // Destroy the sprite batch.
                SpriteBatch?.Dispose();
                SpriteBatch = null;

                // Unload all content.
                Content?.Unload();
            }
            catch {  }
        }
    }
}