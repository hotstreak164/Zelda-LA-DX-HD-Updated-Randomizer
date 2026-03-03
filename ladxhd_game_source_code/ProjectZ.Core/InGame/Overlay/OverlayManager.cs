using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectZ.Base;
using ProjectZ.Base.UI;
using ProjectZ.InGame.Overlay.Sequences;
using ProjectZ.InGame.Controls;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.Pages;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.Overlay
{
    public class OverlayManager
    {
        public float HudTransparency = 1;
        public bool DisableOverlayToggle;
        public bool DisableInventoryToggle;

        enum MenuState
        {
            None, Menu, Inventory, PhotoBook, GameSequence
        }

        enum GameScaleDirection: int
        {
            Decrease = -1, 
            Increase = 1
        }

        private MenuState _currentMenuState = MenuState.None;
        private MenuState _lastMenuState = MenuState.None;

        public bool InventoryState { get => _currentMenuState == MenuState.Inventory; }

        public TextboxOverlay TextboxOverlay;
        public HudOverlay InGameHud;

        private InventoryOverlay _inventoryOverlay;
        private MapOverlay _mapOverlay;
        private DungeonOverlay _dungeonOverlay;
        private PhotoOverlay _photoOverlay;

        private Dictionary<string, GameSequence> _gameSequences = new Dictionary<string, GameSequence>();
        private string _currentSequenceName;

        private RenderTarget2D _menuRenderTarget2D;
        private UiRectangle _blurRectangle;

        private Rectangle _recInventory;
        private Rectangle _recMap;
        private Rectangle _recMapCenter;
        private Rectangle _recDungeon;

        private Vector2 _menuPosition;

        private Point _inventorySize;
        private Point _mapSize;
        private Point _dungeonSize;
        private Point _overlaySize;

        private double _fadeCount;
        private float _fadeAnimationPercentage;

        private float _hudState = 1;
        private float _hudPercentage;
        private bool _hideHud;

        private readonly int _marginMap = 0;
        private readonly int _margin = 2;
        private readonly int _fadeTime = 200;
        private const int ChangeTime = 125;
        private int _fadeDir;
        private int _scale;
        private float _changeCount;

        private int _overlayWidth;
        private int _overlayHeight;

        private bool _fading;
        private bool _updateInventory = true;
        private bool _isChanging;
        private bool _mapOpened;

        private int _scaleButtonCount;
        private bool _scaleButtonDown;
        private float _scaleButtonTimer;
        private float _scaleButtonPeriod;

        public Dictionary<Point, (int Level, Vector2 Teleport)> TeleportMap = new()
        {
            // Tile Position    = Level, Link Teleport Position
            [new Point(5, 4)]   = (0, new Vector2(872, 606)),   // Manbo's Pond
            [new Point(3, 13)]  = (1, new Vector2(600, 1720)),  // Dungeon 1
            [new Point(4, 2)]   = (2, new Vector2(736, 340)),   // Dungeon 2
            [new Point(5, 11)]  = (3, new Vector2(920, 1472)),  // Dungeon 3
            [new Point(11, 2)]  = (4, new Vector2(1848, 320)),  // Dungeon 4
            [new Point(9, 13)]  = (5, new Vector2(1544, 1728)), // Dungeon 5
            [new Point(12, 8)]  = (6, new Vector2(1992, 1120)), // Dungeon 6
            [new Point(14, 0)]  = (7, new Vector2(2344, 96)),   // Dungeon 7
            [new Point(0, 1)]   = (8, new Vector2(104, 192))    // Dungeon 8
        };

        public OverlayManager()
        {
            _blurRectangle = (UiRectangle)Game1.UiManager.AddElement(new UiRectangle(Rectangle.Empty, "background", Values.ScreenNameGame, Color.Transparent, Color.Transparent, null), true);
        }

        public void Load(ContentManager content)
        {
            // Add all game sequences to the stack.
            _gameSequences["map"] = new MapOverlaySequence();
            _gameSequences["marinBeach"] = new MapOverlaySequence();
            _gameSequences["marinCliff"] = new MapOverlaySequence();
            _gameSequences["towerCollapse"] = new MapOverlaySequence();
            _gameSequences["shrine"] = new MapOverlaySequence();
            _gameSequences["picture"] = new MapOverlaySequence();
            _gameSequences["photo"] = new MapOverlaySequence();
            _gameSequences["bowWow"] = new MapOverlaySequence();
            _gameSequences["castle"] = new MapOverlaySequence();
            _gameSequences["gravestone"] = new MapOverlaySequence();
            _gameSequences["weatherBird"] = new MapOverlaySequence();
            _gameSequences["final"] = new MapOverlaySequence();
            _gameSequences["painting"] = new MapOverlaySequence();

            // Set the size of the UI elements.
            _mapSize = new Point(144 + 2 * _marginMap, 144 + 2 * _marginMap);
            _dungeonSize = new Point(80, 106);
            _inventorySize = new Point(268, 208);
            _overlaySize = new Point(_inventorySize.X + _margin + _dungeonSize.X, _inventorySize.Y);

            // Set up the overlays.
            TextboxOverlay = new TextboxOverlay();
            InGameHud = new HudOverlay();
            _mapOverlay = new MapOverlay(_mapSize.X, _mapSize.Y, _marginMap, false);
            _inventoryOverlay = new InventoryOverlay(_inventorySize.X, _inventorySize.Y);
            _dungeonOverlay = new DungeonOverlay(_dungeonSize.X, _dungeonSize.Y);
            _photoOverlay = new PhotoOverlay();

            // Load up all of the overlays.
            _mapOverlay.Load();
            _dungeonOverlay.Load();
            _photoOverlay.Load();
        }

        public void RefreshPhotoOverlay()
        {
            // Used to swap between sepia/colored photos.
            _photoOverlay.Reload();
        }

        public void OnLoad()
        {
            CloseOverlay();
            _hideHud = false;
            _fadeCount = 0;
            TextboxOverlay.Init();
        }

        public void Update()
        {
            // See if the inventory was disabled in "script.zScript".
            bool disableOptions = Game1.GameManager.SaveManager.GetString("disable_options", "0") == "1";
            bool disableInventory = Game1.GameManager.SaveManager.GetString("disable_inventory", "0") == "1";

            // Toggle Game Options Menu Overlay
            if ((_currentMenuState == MenuState.None || _currentMenuState == MenuState.Menu) && ControlHandler.ButtonPressed(CButtons.Select) && !disableOptions)
                ToggleState(MenuState.Menu);

            // Toggle the Inventory / Map Overlay
            if ((_currentMenuState == MenuState.None || _currentMenuState == MenuState.Inventory) && ControlHandler.ButtonPressed(CButtons.Start) && 
                !disableInventory && !DisableInventoryToggle && !_hideHud && !TextboxOverlay.IsOpen)
                ToggleState(MenuState.Inventory);
            
            // Use the inventory disable to identify moments to lock the free camera.
            if (!Camera.ClassicMode && (disableInventory || DisableInventoryToggle))
                MapManager.CameraOffset = Vector2.Zero;

            // Update the textbox and peform button scale change if a menu is currently not visible.
            if (_currentMenuState == MenuState.None)
            {
                // Update the textbox overlay.
                TextboxOverlay.Update();

                // Scale with bumper/trigger presses if enabled by the user.
                if (GameSettings.TriggersScale)
                    ButtonScaleChange();
            }
            // The menu is currently so pause updating the game.
            else if (_currentMenuState == MenuState.Menu)
            {
                Game1.UpdateGame = false;
            }
            // The inventory menu is open so pause updating the game.
            else if (_currentMenuState == MenuState.Inventory)
            {
                Game1.UpdateGame = false;

                // Detect if the map screen transition is taking place.
                if (_isChanging)
                {
                    // Transition between map and inventory screens.
                    _changeCount += (_updateInventory ? 1 : -1) * Game1.DeltaTime;
                    if (_changeCount >= ChangeTime || _changeCount < 0)
                    {
                        _isChanging = false;
                        _changeCount = _updateInventory ? ChangeTime : 0;
                        _updateInventory = !_updateInventory;
                    }
                }
                // A transition between inventory and map is not taking place.
                else
                {
                    // Check if the select button has been pressed. 
                    if (ControlHandler.ButtonPressed(CButtons.Select) && !TextboxOverlay.IsOpen)
                        ToggleInventoryMap();

                    // Update the inventory menu (cursor movement, item selections, etc).
                    if (_updateInventory)
                        _inventoryOverlay.UpdateMenu();

                    // Map overlay is currently in the forefront.
                    _mapOverlay.IsSelected = !_updateInventory;
                    _mapOverlay.Update();

                    // Update the textbox overlay (map area descriptions).
                    TextboxOverlay.Update();
                    _dungeonOverlay.Update();
                }
            }
            // Photo overlay is currently open.
            else if (_currentMenuState == MenuState.PhotoBook)
            {
                // Pause updating the game.
                Game1.UpdateGame = false;

                // Update the textbox overlay and photo overlay.
                TextboxOverlay.Update();
                _photoOverlay.Update();
            }
            // Sequence overlay is currently open (Marin on the beach, Mr.Write Christine picture, painting, etc.).
            else if (_currentMenuState == MenuState.GameSequence)
            {
                // Pause updating the game but force dialog updates.
                Game1.ForceDialogUpdate = true;
                Game1.UpdateGame = false;

                // Update the textbox overlay and game sequences.
                TextboxOverlay.Update();
                _gameSequences[_currentSequenceName].Update();
            }
            // Update the fade in/out effect.
            UpdateFade();

            // Update the HUD (hearts, rupees, keys, save icon).
            InGameHud.Update(_hudPercentage, (1 - _hudPercentage) * HudTransparency);

            // Allow overlays and inventory to be closed/opened again via button press.
            DisableOverlayToggle = false;
            DisableInventoryToggle = false;
        }
        private void ChangeGameScale(GameScaleDirection scaleDirection)
        {
            // Get the maximum scale and add 1 for auto-scale.
            int maxScale = Game1.MaxGameScale + 1;

            // Do not adjust the scale when classic camera is active.
            if (Camera.ClassicMode)
                return;

            // If both LT and RT are pressed together, set the scaling to auto-scaling.
            if ((GameSettings.SixButtons && ControlHandler.ButtonDown(CButtons.RT) && ControlHandler.ButtonDown(CButtons.LT)) || 
                (!GameSettings.SixButtons && ControlHandler.ButtonDown(CButtons.RB) && ControlHandler.ButtonDown(CButtons.LB)))
            {
                GameSettings.GameScale = maxScale;
            }
            // If either LT or RT were pressed scale up or down.
            else if ((GameSettings.SixButtons && ControlHandler.ButtonDown(CButtons.RT)) || (!GameSettings.SixButtons && ControlHandler.ButtonDown(CButtons.RB)) ||
                (GameSettings.SixButtons && ControlHandler.ButtonDown(CButtons.LT)) || (!GameSettings.SixButtons && ControlHandler.ButtonDown(CButtons.LB)))
            {
                // When autoscaling is set, match the scaling value so it can move up and down smoothly.
                if (GameSettings.GameScale == maxScale)
                {
                    float gameScale = MathHelper.Clamp(Math.Min(Game1.WindowWidth / 160, Game1.WindowHeight / 128), 1, maxScale);
                    gameScale = gameScale / 2;
                    GameSettings.GameScale = (int)MathF.Ceiling(gameScale);
                }
                // Set the new scale using the passed parameter.
                int newScale = GameSettings.GameScale + (int)scaleDirection;

                // Do not let the scale fall outside the slider range.
                if (newScale >= -3 && newScale < maxScale)
                    GameSettings.GameScale = newScale;
            }
            // Apply current scaling settings.
            Game1.ScaleChanged = true;
        }

        public void ButtonScaleChange()
        {
            // Increase the timer if one of the scaling buttons are held.
            if (_scaleButtonDown)
                _scaleButtonTimer += Game1.DeltaTime;

            // Increase/Decrease game scale. Start the timer so that there is a 500ms repeat delay.
            if ((GameSettings.SixButtons && ControlHandler.ButtonPressed(CButtons.LT)) || 
                (!GameSettings.SixButtons && ControlHandler.ButtonPressed(CButtons.LB)))
            {
                ChangeGameScale(GameScaleDirection.Decrease);
                _scaleButtonDown = true;
                _scaleButtonTimer = -425f;
                _scaleButtonPeriod = 75;
            }
            else if ((GameSettings.SixButtons && ControlHandler.ButtonPressed(CButtons.RT)) || 
                (!GameSettings.SixButtons && ControlHandler.ButtonPressed(CButtons.RB)))
            {
                ChangeGameScale(GameScaleDirection.Increase);
                _scaleButtonDown = true;
                _scaleButtonTimer = -425f;
                _scaleButtonPeriod = 75;
            }
            // Increase/Decrease game scale repeatedly while button is held every 75ms.
            if (_scaleButtonDown && _scaleButtonTimer > _scaleButtonPeriod && 
                ((GameSettings.SixButtons && ControlHandler.ButtonDown(CButtons.LT)) || 
                (!GameSettings.SixButtons && ControlHandler.ButtonDown(CButtons.LB))))
            {
                ChangeGameScale(GameScaleDirection.Decrease);
                _scaleButtonTimer = 0;
                _scaleButtonCount++;
            }
            if (_scaleButtonDown && _scaleButtonTimer > _scaleButtonPeriod && 
                ((GameSettings.SixButtons && ControlHandler.ButtonDown(CButtons.RT)) || 
                (!GameSettings.SixButtons && ControlHandler.ButtonDown(CButtons.RB))))
            {
                ChangeGameScale(GameScaleDirection.Increase);
                _scaleButtonTimer = 0;
                _scaleButtonCount++;
            }
            // The longer the button is held down, the faster the "zoom" will get. The left value in the switch represents
            // how many scaling iterations have passed, the right value represents how many milliseconds between iterations.
            _scaleButtonPeriod = _scaleButtonCount switch
            {
                <  5  => 75,
                <  8  => 60,
                <  12 => 55,
                <  15 => 40,
                <  18 => 25,
                <  21 => 10,
                >= 24 =>  5,
                _ => _scaleButtonPeriod
            };
            // When either button is released, reset the repeat variables.
            if ((GameSettings.SixButtons && ControlHandler.ButtonReleased(CButtons.LT)) || (!GameSettings.SixButtons && ControlHandler.ButtonReleased(CButtons.LB)) ||
                (GameSettings.SixButtons && ControlHandler.ButtonReleased(CButtons.RT)) || (!GameSettings.SixButtons && ControlHandler.ButtonReleased(CButtons.RB)))
            {
                _scaleButtonDown = false;
                _scaleButtonTimer = 0;
                _scaleButtonCount = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the game UI; fade out with overlay fade in.
            InGameHud.DrawTop(spriteBatch, _hudPercentage, (1 - _hudPercentage) * HudTransparency);

            // Draw the Textbox overlay.
            TextboxOverlay.DrawTop(spriteBatch);

            // Draw the Inventory / Map Screen / Photo Overlay / Game Sequence.
            if (_fadeAnimationPercentage > 0)
            {
                // If the current or last menu state is the inventory.
                if (_currentMenuState == MenuState.Inventory || _lastMenuState == MenuState.Inventory)
                {
                    // Draw the menu on the screen.
                    var menuY = 25 * _scale * (1 - _fadeAnimationPercentage);
                    var menuColor = Color.White * _fadeAnimationPercentage;

                    // Try to align the inventory while the dungeon panel is on the side of it.
                    // When the resololution is not wide enough move the inventory to the left.
                    int dungeonOffset;

                    if (!Game1.GameManager.MapManager.CurrentMap.DungeonMode)
                        dungeonOffset = (_margin + _dungeonSize.X) * _scale / 2;
                    else
                        dungeonOffset = Math.Clamp((_margin + _dungeonSize.X) * _scale / 2, -16, (Game1.WindowWidth - _overlayWidth) / 2 - 8);

                    spriteBatch.Draw(_menuRenderTarget2D, new Rectangle(
                        (int)_menuPosition.X + dungeonOffset, (int)(_menuPosition.Y - menuY), _overlayWidth, _overlayHeight), menuColor);
                }
                // Draw the Photobook overlay.
                else if (_currentMenuState == MenuState.PhotoBook || _lastMenuState == MenuState.PhotoBook)
                    _photoOverlay.Draw(spriteBatch, _fadeAnimationPercentage);

                // Draw the current game sequence.
                else if (_currentMenuState == MenuState.GameSequence || _lastMenuState == MenuState.GameSequence)
                    _gameSequences[_currentSequenceName].Draw(spriteBatch, _fadeAnimationPercentage);

                // Draw the inventory screen.
                if (_currentMenuState == MenuState.Inventory)
                {
                    // Draw the map toggle button and label.
                    var mapStart = "";
                    if (ControlHandler.LastKeyboardDown && ControlHandler.ButtonDictionary[CButtons.Select].Keys.Length > 0)
                        mapStart = ControlHandler.ButtonDictionary[CButtons.Select].Keys[0].ToString();
                    if (!ControlHandler.LastKeyboardDown && ControlHandler.ButtonDictionary[CButtons.Select].Buttons.Length > 0)
                        mapStart = ControlHandler.GetButtonName(ControlHandler.ButtonDictionary[CButtons.Select].Buttons[0]);

                    var mapString = mapStart + ": " + Game1.LanguageManager.GetString(_updateInventory ? "overlay_map" : "overlay_inventory", "error");
                    var mapDrawPos = new Vector2(8 * Game1.UiScale, Game1.WindowHeight - 16 * Game1.UiScale);

                    GameFS.DrawString(spriteBatch, mapString, mapDrawPos, Color.White * _fadeAnimationPercentage, 0, Vector2.Zero, Game1.UiScale, SpriteEffects.None, 0);

                    // When navigating the map, get the currently selected map position.
                    var nodeSelected = _mapOverlay.SelectionPosition;

                    // If we're in map mode and one of the dungeons are selected.
                    if (((GameSettings.MapTeleport == 1 || GameSettings.MapTeleport == 3) || (GameSettings.MapTeleport == 2 && MapManager.ObjLink.ManboTeleport)) && !_updateInventory && TeleportMap.ContainsKey(nodeSelected) && MapManager.ObjLink.Map.IsOverworld)
                    {
                        // Get the selected dungeon and check if the instrument has been collected.
                        int dungeonLevel = TeleportMap[nodeSelected].Level - 1;
                        var hasInstrument = Game1.GameManager.GetItem("instrument" + dungeonLevel);

                        // If instrument has not been collected don't draw the text.
                        if (hasInstrument == null || hasInstrument.Count < 1)
                            return;

                        // Get the correct button to display next to the text.
                        var teleStart = "";
                        if (ControlHandler.LastKeyboardDown && ControlHandler.ButtonDictionary[CButtons.X].Keys.Length > 0)
                            teleStart = ControlHandler.ButtonDictionary[CButtons.X].Keys[0].ToString();
                        if (!ControlHandler.LastKeyboardDown && ControlHandler.ButtonDictionary[CButtons.X].Buttons.Length > 0)
                            teleStart = ControlHandler.GetButtonName(ControlHandler.ButtonDictionary[CButtons.X].Buttons[0]);

                        // Set up the string to display.
                        var teleString = teleStart + ": " + Game1.LanguageManager.GetString("overlay_teleport", "error");
                        var teleTextSize = GameFS.MeasureString(teleString);
                        var teleDrawPos = new Vector2(Game1.WindowWidth - (teleTextSize.X + 6) * Game1.UiScale, Game1.WindowHeight - 16 * Game1.UiScale);

                        // Draw the teleport button and label.
                        GameFS.DrawString(spriteBatch, teleString, teleDrawPos, Color.White * _fadeAnimationPercentage, 0, Vector2.Zero, Game1.UiScale, SpriteEffects.None, 0);
                    }
                }
            }
        }

        private void EnsureMenuRenderTarget()
        {
            // Skip creation if width/height is invalid (can happen during resize or at startup).
            if (_overlayWidth <= 0 || _overlayHeight <= 0)
                return;

            if (_menuRenderTarget2D == null
                || _menuRenderTarget2D.IsDisposed
                || _menuRenderTarget2D.Width != _overlayWidth
                || _menuRenderTarget2D.Height != _overlayHeight)
            {
                try
                {
                    _menuRenderTarget2D?.Dispose();
                    _menuRenderTarget2D = new RenderTarget2D(Game1.Graphics.GraphicsDevice,
                        Math.Max(1, _overlayWidth),
                        Math.Max(1, _overlayHeight));
                }
                catch (Exception ex)
                {
                    // optional: log for debugging
                    System.Diagnostics.Debug.WriteLine($"MenuRenderTarget creation failed: {ex.Message}");
                    _menuRenderTarget2D = null;
                }
            }
        }

        public void DrawRenderTarget(SpriteBatch spriteBatch)
        {
            // If the fade percentage is above zero and a game sequence is currently visible, draw it with current render target.
            if (_fadeAnimationPercentage > 0 && (_currentMenuState == MenuState.GameSequence || _lastMenuState == MenuState.GameSequence))
                _gameSequences[_currentSequenceName].DrawRT(spriteBatch);

            // If the inventory is currently visible.
            if (_currentMenuState == MenuState.Inventory)
            {
                // Ensure the render target exists and has valid size
                EnsureMenuRenderTarget();

                // Don't try drawing if RT is currently null.
                if (_menuRenderTarget2D == null)
                    return;

                // Draw the various overlays.
                _mapOverlay.DrawRenderTarget(spriteBatch);
                _inventoryOverlay.DrawRT(spriteBatch);
                _dungeonOverlay.DrawOnRenderTarget(spriteBatch);

                Game1.Graphics.GraphicsDevice.SetRenderTarget(_menuRenderTarget2D);
                Game1.Graphics.GraphicsDevice.Clear(Color.Transparent);

                // Draw the inventory.
                DrawInventory(spriteBatch);
            }
        }

        public void UpdateRenderTarget()
        {
            // Update all render targets.
            if (_menuRenderTarget2D == null || _menuRenderTarget2D.Width != _overlayWidth || _menuRenderTarget2D.Height != _overlayHeight)
                _menuRenderTarget2D = new RenderTarget2D(Game1.Graphics.GraphicsDevice, _overlayWidth, _overlayHeight);

            _inventoryOverlay.UpdateRenderTarget();
            _mapOverlay.UpdateRenderTarget();
            _dungeonOverlay.UpdateRenderTarget();

            UpdateOverlayDimensions();
        }

        public void DisposeRenderTargets()
        {
            try
            {
                _inventoryOverlay?.DisposeRenderTargets();
                _mapOverlay?.DisposeRenderTargets();
                _dungeonOverlay?.DisposeRenderTargets();

                _menuRenderTarget2D?.Dispose(); 
                _menuRenderTarget2D = null;
            }
            catch { }
        }

        private void UpdateOverlayDimensions()
        {
            // Update the scale to match the UI scale.
            _scale = Game1.UiScale;

            // Recalculate the size of the inventory.
            _recInventory = new Rectangle(0, 0, _inventorySize.X * _scale, _inventorySize.Y * _scale);

            // Recalculate the size of the map stuff.
            _recMap = new Rectangle(
                _recInventory.Right - 6 * _scale - _mapSize.X * _scale,
                _recInventory.Bottom - 6 * _scale - _mapSize.Y * _scale, _mapSize.X * _scale, _mapSize.Y * _scale);
            _recMapCenter = new Rectangle(
                _recInventory.Width / 2 - _mapSize.X / 2 * _scale,
                _recInventory.Height / 2 - _mapSize.Y / 2 * _scale, _mapSize.X * _scale, _mapSize.Y * _scale);
            _recDungeon = new Rectangle(
                _recInventory.Right + _margin * _scale,
                _recInventory.Bottom - _dungeonSize.Y * _scale,
                _dungeonSize.X * _scale, _dungeonSize.Y * _scale);
        }

        private void DrawInventory(SpriteBatch spriteBatch)
        {
            // Recalculate the dimensions in case the UI scale has changed.
            UpdateOverlayDimensions();

            spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, null, null);

            var percentage = MathF.Sin(-MathF.PI / 2 + (_changeCount / ChangeTime) * MathF.PI) * 0.5f + 0.5f;

            // Draw the inventory overlay.
            _inventoryOverlay.Draw(spriteBatch, _recInventory, Color.White * (1 - percentage));

            spriteBatch.End();

            // Draw the map onto the inventory screen.
            var mapRectangle = new Rectangle(
                (int)MathHelper.Lerp(_recMap.X, _recMapCenter.X, percentage),
                (int)MathHelper.Lerp(_recMap.Y, _recMapCenter.Y, percentage), _recMap.Width, _recMap.Height);
            _mapOverlay.Draw(spriteBatch, mapRectangle, Color.White);

            spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp);

            // Draw the dungeon map.
            _dungeonOverlay.Draw(spriteBatch, _recDungeon, Color.White * (1 - percentage));

            spriteBatch.End();
        }

        public void UpdateInventoryButtons(bool sixButtons)
        {
            // Updates the number of equippable buttons between 4 and 6 buttons.
            _inventoryOverlay.UpdateButtonLayout(sixButtons);
        }

        public void ResolutionChanged()
        {
            TextboxOverlay?.ResolutionChange();
            InGameHud?.ResolutionChange();
            _inventoryOverlay?.ResolutionChanged();
            _dungeonOverlay?.ResolutionChanged();

            _blurRectangle.Rectangle.Width = Game1.WindowWidth;
            _blurRectangle.Rectangle.Height = Game1.WindowHeight;

            _scale = Game1.UiScale;

            // Render at actual screen resolution (not native resolution)
            _overlayWidth = _overlaySize.X * _scale;
            _overlayHeight = _overlaySize.Y * _scale;

            _menuPosition = new Vector2(
                Game1.WindowWidth / 2 - _overlayWidth / 2, 
                Game1.WindowHeight / 2 - _overlayHeight / 2);

            EnsureMenuRenderTarget();
        }

        public void OpenPhotoOverlay()
        {
            // Open photo overlay and set the menu state.
            _photoOverlay.OnOpen();
            SetState(MenuState.PhotoBook);
        }

        public void StartSequence(string name)
        {
            // Exit if the sequence doesn't exist in the dictionary.
            if (!_gameSequences.ContainsKey(name))
                return;

            // Start the sequence and set the menu state.
            _currentSequenceName = name;
            _gameSequences[_currentSequenceName].OnStart();
            SetState(MenuState.GameSequence);
        }

        public GameSequence GetCurrentGameSequence()
        {
            // Get the current game sequence.
            if (_currentSequenceName != null && _gameSequences.ContainsKey(_currentSequenceName))
                return _gameSequences[_currentSequenceName];

            return null;
        }

        public void ToggleInventoryMap()
        {
            _isChanging = true;

            if (!_mapOpened)
                Game1.GameManager.PlaySoundEffect("D360-19-13");

            _mapOpened = !_mapOpened;
        }

        public bool UpdateCameraAndAnimation()
        {
            return (_currentMenuState != MenuState.Inventory && TextboxOverlay.IsOpen) || _currentMenuState == MenuState.GameSequence;
        }

        public void HideHud(bool hidden)
        {
            _hideHud = hidden;
        }

        private void UpdateFade()
        {
            // Update the fade in/out effect.
            if (_fading)
            {
                _fadeCount += Game1.DeltaTime * _fadeDir;

                // Fade in/out has finished.
                if (_fadeCount <= 0 || _fadeCount >= _fadeTime)
                {
                    _fading = false;
                    _fadeCount = MathHelper.Clamp((float)_fadeCount, 0, _fadeTime);
                }
            }
            // Update the fade percentage.
            var fadePercentage = (float)_fadeCount / _fadeTime;
            _fadeAnimationPercentage = (float)Math.Sin(Math.PI / 2 * fadePercentage);
            _blurRectangle.BackgroundColor = Color.Black * 0.5f * _fadeAnimationPercentage;
            _blurRectangle.BlurColor = Values.GameMenuBackgroundColor * _fadeAnimationPercentage;

            if (_fadeAnimationPercentage <= 0 && _currentSequenceName != null && _currentMenuState == MenuState.None)
                _currentSequenceName = null;

            // Hide the HUD.
            if (TextboxOverlay.IsOpen || _currentMenuState != MenuState.None || _hideHud)
            {
                _hudState = AnimationHelper.MoveToTarget(_hudState, 1, 0.1f * Game1.TimeMultiplier);
            }
            else if (!Game1.GameManager.DialogIsRunning() && (Game1.UpdateGame || Game1.ForceDialogUpdate))
            {
                _hudState = AnimationHelper.MoveToTarget(_hudState, 0, 0.1f * Game1.TimeMultiplier);
            }
            // Update the HUD percentage.
            _hudPercentage = (float)Math.Sin(Math.PI / 2 * _hudState);
        }

        private void ToggleState(MenuState newState)
        {
            if (_currentMenuState == MenuState.None)
                SetState(newState);
            else
            {
                _mapOpened = false;
                CloseOverlay();
            }
        }

        private void SetState(MenuState newState)
        {
            // Don't change the state if a textbox is open.
            if (TextboxOverlay.IsOpen || DisableOverlayToggle)
                return;

            // Pause the currently playing soundeffects.
            if (newState == MenuState.Inventory || newState == MenuState.Menu)
                Game1.GameManager.PauseSoundEffects();

            // Play the menu opening sound when opening the options menu or inventory menu.
            if (newState == MenuState.Inventory || newState == MenuState.Menu)
                Game1.GameManager.PlaySoundEffect("D360-17-11");

            // The inventory open was opened.
            if (newState == MenuState.Inventory)
            {
                _isChanging = false;
                _changeCount = 0;
                _updateInventory = true;

                _mapOverlay.OnFocus();
                _dungeonOverlay.OnFocus();
            }
            // The options menu was opened.
            else if (newState == MenuState.Menu)
            {
                // Don't allow opening the menu again until it has closed.
                Game1.UiPageManager.ChangePage(typeof(GameMenuPage), null, PageManager.TransitionAnimation.TopToBottom, PageManager.TransitionAnimation.TopToBottom);
            }
            _fading = true;
            _fadeDir = 1;
            _lastMenuState = _currentMenuState;
            _currentMenuState = newState;
        }

        public void CloseOverlay()
        {
            // Play the menu closing sound when closing the options menu or inventory menu.
            if (_currentMenuState == MenuState.Inventory || _currentMenuState == MenuState.Menu)
                Game1.GameManager.PlaySoundEffect("D360-18-12");

            _fading = true;
            _fadeDir = -1;
            _lastMenuState = _currentMenuState;
            _currentMenuState = MenuState.None;

            // Store last input state and close all open pages.
            InputHandler.ResetInputState();
            Game1.UiPageManager.PopAllPages(PageManager.TransitionAnimation.TopToBottom, PageManager.TransitionAnimation.TopToBottom);

            // Resume the sound effects.
            Game1.GameManager.ContinueSoundEffects();
        }

        public bool MenuIsOpen()
        {
            // Store the menu state when opening the menu.
            return _currentMenuState == MenuState.Menu;
        }
    }
}
