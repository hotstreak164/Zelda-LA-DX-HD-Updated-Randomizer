using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZ.InGame.Controls;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.Interface;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.Pages
{
    class MainMenuPage : InterfacePage
    {
        enum State { Select, Delete, Copy }

        public InterfaceListLayout[] SaveEntries = new InterfaceListLayout[SaveStateManager.SaveCount];

        private float[] _playerSelectionState = new float[SaveStateManager.SaveCount];
        private InterfaceButton[] _saveButtons = new InterfaceButton[SaveStateManager.SaveCount];
        private Dictionary<string, object> _newGameIntent = new Dictionary<string, object>();

        private Animator _playerAnimation = new Animator();
        private Animator _swordAnimation = new Animator();

        private InterfacePlayerImage[] _playerImage = new InterfacePlayerImage[SaveStateManager.SaveCount];

        private DictAtlasEntry _heartSprite;

        private InterfaceElement[][] _heartImage = new InterfaceElement[4][];

        private InterfaceGravityLayout[] _saveButtonLayouts = new InterfaceGravityLayout[SaveStateManager.SaveCount];

        private InterfaceLabel[] _saveNames = new InterfaceLabel[SaveStateManager.SaveCount];
        private InterfaceLabel[] _saveRupees = new InterfaceLabel[SaveStateManager.SaveCount];
        private InterfaceLabel[] _savePlaytime = new InterfaceLabel[SaveStateManager.SaveCount];

        private InterfaceListLayout[] _deleteCopyLayouts = new InterfaceListLayout[SaveStateManager.SaveCount];

        private InterfaceListLayout _mainLayout;
        private InterfaceListLayout _newGameButtonLayout;
        private InterfaceListLayout _menuBottomBar;
        private InterfaceListLayout _saveFileList;

        private string[] cloakColors = new string[SaveStateManager.SaveCount];

        private int _selectedSaveIndex;
        private bool _selectStoredSave;

        public MainMenuPage(int width, int height)
        {
            var smallButtonWidth = 100;
            var smallButtonMargin = 2;
            var saveButtonRec = new Point(204, 32);
            var sideSize = 70;

            _heartSprite = Resources.GetSprite("heart menu");

            _playerAnimation = AnimatorSaveLoad.LoadAnimator("menu_link");
            _playerAnimation.Play("green");

            _swordAnimation = AnimatorSaveLoad.LoadAnimator("menu_link");
            _swordAnimation.Play("sword");

            _newGameButtonLayout = new InterfaceListLayout { Size = saveButtonRec };
            _newGameButtonLayout.AddElement(new InterfaceLabel("main_menu_new_game"));

            // Save File List
            _saveFileList = new InterfaceListLayout() { Size = new Point(width, (int)(height * Values.MenuContentSize) - 12), Selectable = true };
            for (var i = 0; i < SaveStateManager.SaveCount; i++)
            {
                _saveButtonLayouts[i] = new InterfaceGravityLayout { Size = new Point(saveButtonRec.X, saveButtonRec.Y) };

                var numberWidth = 17;
                var saveSlotNumber = new InterfaceLabel(null, new Point(numberWidth, 28), Point.Zero)
                { Gravity = InterfaceElement.Gravities.Left };
                saveSlotNumber.SetText((i + 1).ToString());

                _saveButtonLayouts[i].AddElement(saveSlotNumber);

                var saveInfoLayout = new InterfaceListLayout { HorizontalMode = true, Size = new Point(saveButtonRec.X - numberWidth, saveButtonRec.Y), Gravity = InterfaceElement.Gravities.Right };

                // Current Heart Count
                {
                    var heartsWidth = saveButtonRec.X / 2 - numberWidth - 20;
                    var hearts = new InterfaceListLayout { Size = new Point(heartsWidth, 30) };

                    var rowOne = new InterfaceListLayout { Size = new Point(heartsWidth - 4, 7), Margin = new Point(2, 1), HorizontalMode = true, ContentAlignment = InterfaceElement.Gravities.Left };
                    var rowTwo = new InterfaceListLayout { Size = new Point(heartsWidth - 4, 7), Margin = new Point(2, 1), HorizontalMode = true, ContentAlignment = InterfaceElement.Gravities.Left };

                    _heartImage[i] = new InterfaceElement[14];
                    for (var j = 0; j < 7; j++)
                    {
                        int k = j + 7;
                        _heartImage[i][j] = rowOne.AddElement(new InterfaceImage(Resources.SprItem, _heartSprite.ScaledRectangle, Point.Zero, new Point(1, 1)) { Gravity = InterfaceElement.Gravities.Left });
                        _heartImage[i][k] = rowTwo.AddElement(new InterfaceImage(Resources.SprItem, _heartSprite.ScaledRectangle, Point.Zero, new Point(1, 1)) { Gravity = InterfaceElement.Gravities.Left });
                    }
                    hearts.AddElement(rowOne);
                    hearts.AddElement(rowTwo);

                    saveInfoLayout.AddElement(hearts);
                }
                // Name / Rupees / Playtime
                {
                    var rightWidth = saveButtonRec.X / 2 + 8;
                    var middle = new InterfaceListLayout { Gravity = InterfaceElement.Gravities.Left, Margin = new Point(2, 0), Size = new Point(rightWidth, 30) };

                    middle.AddElement(_saveNames[i] = new InterfaceLabel(null, new Point(rightWidth - 18, 10), Point.Zero) { Margin = new Point(1, 0), TextAlignment = InterfaceElement.Gravities.Left | InterfaceElement.Gravities.Bottom });
                    middle.AddElement(_saveRupees[i] = new InterfaceLabel(null, new Point(rightWidth - 17, 10), Point.Zero) { Margin = new Point(0, 0), TextAlignment = InterfaceElement.Gravities.Left });
                    middle.AddElement(_savePlaytime[i] = new InterfaceLabel(null, new Point(rightWidth - 17, 10), Point.Zero) { Margin = new Point(0, 0), TextAlignment = InterfaceElement.Gravities.Left });

                    saveInfoLayout.AddElement(middle);
                }
                var i1 = i;
                _saveButtonLayouts[i].AddElement(saveInfoLayout);

                _saveButtons[i] = new InterfaceButton
                {
                    InsideElement = _saveButtonLayouts[i],
                    Size = new Point(saveButtonRec.X, saveButtonRec.Y),
                    Margin = new Point(0, 2),
                    ClickFunction = e => OnClickSave(i1)
                };

                SaveEntries[i] = new InterfaceListLayout { HorizontalMode = true, Gravity = InterfaceElement.Gravities.Right, AutoSize = true, Selectable = true };
                SaveEntries[i].AddElement(new InterfaceListLayout { Size = new Point(sideSize - 20, 20) });
                SaveEntries[i].AddElement(_playerImage[i] = new InterfacePlayerImage(_playerAnimation, _swordAnimation, _playerAnimation.SprTexture, _playerAnimation.CurrentFrame.SourceRectangle, new Point(24, 16), new Point(0, 0)));
                SaveEntries[i].AddElement(_saveButtons[i]);

                // Copy / Delete Options
                var currentSlot = i;
                _deleteCopyLayouts[i] = new InterfaceListLayout
                {
                    Gravity = InterfaceElement.Gravities.Right,
                    Size = new Point(sideSize, saveButtonRec.Y),
                    PreventSelection = true,
                    Selectable = true,
                    Visible = false
                };

                var insideCopy = new InterfaceListLayout() { Size = new Point(sideSize - 4, 13) };
                insideCopy.AddElement(new InterfaceLabel("main_menu_copy") { Size = new Point(40, 12), TextAlignment = InterfaceElement.Gravities.Bottom });
                _deleteCopyLayouts[i].AddElement(new InterfaceButton(new Point(sideSize - 4, 13), new Point(0, 1), insideCopy, element => OnClickCopy(currentSlot)));

                var insideDelete = new InterfaceListLayout() { Size = new Point(sideSize - 4, 13) };
                insideDelete.AddElement(new InterfaceLabel("main_menu_erase") { Size = new Point(40, 12), TextAlignment = InterfaceElement.Gravities.Bottom });
                _deleteCopyLayouts[i].AddElement(new InterfaceButton(new Point(sideSize - 4, 13), new Point(0, 1), insideDelete, element => OnClickDelete(currentSlot)));

                SaveEntries[i].AddElement(_deleteCopyLayouts[i]);
                _saveFileList.AddElement(SaveEntries[i]);
            }
            var buttonHeight = 18;

            // Bottom Bar
            _menuBottomBar = new InterfaceListLayout
            {
                Size = new Point(saveButtonRec.X, (int)(height * Values.MenuFooterSize)),
                HorizontalMode = true,
                Selectable = true
            };
            _menuBottomBar.AddElement(new InterfaceButton(new Point(smallButtonWidth, buttonHeight), new Point(smallButtonMargin, 0), "main_menu_settings", element => Game1.UiPageManager.ChangePage(typeof(SettingsPage))));
            _menuBottomBar.AddElement(new InterfaceButton(new Point(smallButtonWidth, buttonHeight), new Point(smallButtonMargin, 0), "main_menu_quit", element => Game1.UiPageManager.ChangePage(typeof(ExitGamePage))));

           /// NOTE: For some reason, all of this code was used to create the bottom two buttons. But the two lines above do practically the same thing. I have no idea
           /// why the below code was used, as it's not only overly convoluted but also more limited in what can be accessed about the buttons. For example, there is a
           /// hack on "InterfaceButton" that checks the text of the buttons. The code below does not put text on the buttons, but rather overlays it on top of them. I
           /// decided to keep it here for now just in case the new code has issues I don't yet see. If the above works out, this should be deleted before v1.5.0.
/*
            var smallButtonLayout = new InterfaceGravityLayout { Size = new Point(smallButtonWidth, buttonHeight) };
            smallButtonLayout.AddElement(new InterfaceLabel("main_menu_settings") { Gravity = InterfaceElement.Gravities.Center });
            _menuBottomBar.AddElement(new InterfaceButton
            {
                Size = new Point(smallButtonWidth, buttonHeight),
                InsideElement = smallButtonLayout,
                Margin = new Point(smallButtonMargin, 0),
                ClickFunction = element =>
                {
                    Game1.UiPageManager.ChangePage(typeof(SettingsPage));
                }
            });

            var smallButtonLayout2 = new InterfaceGravityLayout { Size = new Point(smallButtonWidth, buttonHeight) };
            smallButtonLayout2.AddElement(new InterfaceLabel("main_menu_quit") { Gravity = InterfaceElement.Gravities.Center });
            _menuBottomBar.AddElement(new InterfaceButton
            {
                Size = new Point(smallButtonWidth, buttonHeight),
                InsideElement = smallButtonLayout2,
                Margin = new Point(smallButtonMargin, 0),
                ClickFunction = element =>
                {
                    Game1.UiPageManager.ChangePage(typeof(ExitGamePage));
                }
            });
*/
            _mainLayout = new InterfaceListLayout { Size = new Point(width, height - 12), Gravity = InterfaceElement.Gravities.Left, Selectable = true };
            _mainLayout.AddElement(new InterfaceLabel(Resources.GameHeaderFont, "main_menu_select_header", new Point(width, (int)(height * Values.MenuHeaderSize)), new Point(0, 0)));
            _mainLayout.AddElement(_saveFileList);
            _mainLayout.AddElement(_menuBottomBar);

            PageLayout = _mainLayout;
            PageLayout.Select(InterfaceElement.Directions.Top, false);
        }

        public override void OnLoad(Dictionary<string, object> intent)
        {
            SaveStateManager.LoadSaveData();

            UpdateUi();

            if (_selectedSaveIndex != -1)
            {
                _saveFileList.Elements[_selectedSaveIndex].Deselect(false);
                _saveFileList.Elements[_selectedSaveIndex].Select(InterfaceElement.Directions.Left, false);
            }
            for (var i = 0; i < _deleteCopyLayouts.Length; i++)
                _deleteCopyLayouts[i].Visible = i == 0 && SaveStateManager.SaveStates[i] != null;

            PageLayout = _mainLayout;
            PageLayout.Deselect(false);
            PageLayout.Select(InterfaceElement.Directions.Top, false);

            UpdatePlayerAnimation(25);
        }

        public override void OnReturn(Dictionary<string, object> intent)
        {
            UpdateUi();

            base.OnReturn(intent);

            if (intent != null && intent.TryGetValue("deleteReturn", out var deleteReturn) && (bool)deleteReturn)
            {
                // select the savestate
                _saveFileList.Elements[_selectedSaveIndex].Deselect(false);
                _saveFileList.Elements[_selectedSaveIndex].Select(InterfaceElement.Directions.Left, false);
            }

            // delete the save state?
            if (intent != null && intent.TryGetValue("deleteSavestate", out var deleteSaveState) && (bool)deleteSaveState)
            {
                SaveGameSaveLoad.DeleteSaveFile(_selectedSaveIndex);
                ReloadSaves();
            }

            // copy save state
            if (intent != null && intent.TryGetValue("copyTargetSlot", out var targetSlot))
            {
                SaveGameSaveLoad.CopySaveFile(_selectedSaveIndex, (int)targetSlot);
                ReloadSaves();

                // select the savestate
                _saveFileList.Elements[_selectedSaveIndex].Deselect(false);
                _saveFileList.Elements[_selectedSaveIndex].Select(InterfaceElement.Directions.Left, false);
                _saveFileList.Elements[_selectedSaveIndex].Deselect(false);

                // select the target slot
                _saveFileList.Select((int)targetSlot, false);
            }
        }

        public override void Update(CButtons pressedButtons, GameTime gameTime)
        {
            // If the player wants to automatically select the last save file accessed.
            if (GameSettings.StoreSavePos && !_selectStoredSave)
            {
                _saveFileList.SimulatePadPresses("down", GameSettings.LastSavePos, false);
            }
            // To prevent bad selection on settings page pop, always disable after menu loads.
            _selectStoredSave = true;

            // If the player used the command line to automatically load a save slot.
            if (Game1.FinishedLoading && Game1.AutoLoadSave)
            {
                // Make sure the slot is within range.
                int LoadSlot = Game1.AutoLoadSlot is >= 0 and <= 3 ? Game1.AutoLoadSlot : 0;

                // Set the autoload to false.
                Game1.AutoLoadSave = false;

                // Load the save file slot.
                LoadSave(LoadSlot);
            }

            base.Update(pressedButtons, gameTime);

            // Update the Link animation on the menu.
            UpdatePlayerAnimation();

            // Only show the copy/delete buttons for the saveslot that is currently selected.
            var selectedSaveIndex = -1;
            for (var i = 0; i < _deleteCopyLayouts.Length; i++)
            {
                _deleteCopyLayouts[i].Visible = _saveFileList.Elements[i].Selected && SaveStateManager.SaveStates[i] != null;
                if (_saveFileList.Elements[i].Selected)
                    selectedSaveIndex = i;
            }

            // Exit back to the video/title screen if pressing the cancel button.
            if (ControlHandler.ButtonPressed(ControlHandler.CancelButton))
            {
                _selectedSaveIndex = selectedSaveIndex;
                _selectStoredSave = false;

                // Close the menu page and change to the intro screen.
                Game1.ScreenManager.ChangeScreen(Values.ScreenNameIntro);
                Game1.UiPageManager.PopPage(null, PageManager.TransitionAnimation.TopToBottom, PageManager.TransitionAnimation.TopToBottom, true);
            }
            // Get the selected index to get the cloak color.
            var index = _saveFileList.SelectionIndex;

            // Update the cloak color.
            if (index is >= 0 and <= 4)
            {
                if (!string.IsNullOrEmpty(cloakColors[index]))
                    _playerAnimation.Play(cloakColors[index]);
                else
                    _playerAnimation.Play("green");
            }
        }

        private void UpdatePlayerAnimation(float transitionSpeed = 0.25f)
        {
            // update the animation
            _playerAnimation.Update();

            for (var i = 0; i < SaveStateManager.SaveCount; i++)
            {
                _playerSelectionState[i] = AnimationHelper.MoveToTarget(_playerSelectionState[i], _saveButtons[i].Selected ? 1 : 0, transitionSpeed * Game1.TimeMultiplier);

                _playerImage[i].ImageColor = Color.Lerp(Color.Transparent, Color.White, _playerSelectionState[i]);
                _playerImage[i].SourceRectangle = _playerAnimation.CurrentFrame.SourceRectangle;
                _playerImage[i].Offset = new Vector2(
                    _playerAnimation.CurrentAnimation.Offset.X + _playerAnimation.CurrentFrame.Offset.X,
                    _playerAnimation.CurrentAnimation.Offset.Y + _playerAnimation.CurrentFrame.Offset.Y);
                _playerImage[i].Effects =
                    (_playerAnimation.CurrentFrame.MirroredV ? SpriteEffects.FlipVertically : SpriteEffects.None) |
                    (_playerAnimation.CurrentFrame.MirroredH ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            }
        }

        private void OnClickCopy(int number)
        {
            _selectedSaveIndex = number;

            var intent = new Dictionary<string, object>();
            intent.Add("selectedSlot", number);

            Game1.UiPageManager.ChangePage(typeof(CopyPage), intent, PageManager.TransitionAnimation.Fade, PageManager.TransitionAnimation.Fade);
        }

        private void OnClickDelete(int number)
        {
            _selectedSaveIndex = number;

            Game1.UiPageManager.ChangePage(typeof(DeleteSaveSlotPage), null, PageManager.TransitionAnimation.Fade, PageManager.TransitionAnimation.Fade);
        }

        private void OnClickSave(int number)
        {
            _selectedSaveIndex = number;

            // load the save file
            LoadSave(number);
        }

        private void LoadSave(int saveIndex)
        {
            // load game or create new save
            if (SaveStateManager.SaveStates[saveIndex] != null)
            {
                // change to the game screen
                Game1.ScreenManager.ChangeScreen(Values.ScreenNameGame);
                // load the save
                Game1.GameManager.LoadSaveFile(saveIndex);
                // close the menu page
                Game1.UiPageManager.PopPage(null, PageManager.TransitionAnimation.TopToBottom, PageManager.TransitionAnimation.TopToBottom, true);
                // Store the last game save that was selected.
                GameSettings.LastSavePos = saveIndex;
            }
            else
            {
                // change to the NewGamePage
                _newGameIntent["SelectedSaveSlot"] = saveIndex;
                Game1.UiPageManager.ChangePage(typeof(NewGamePage), _newGameIntent);
                // Store the last game save that was selected.
                GameSettings.LastSavePos = saveIndex;
            }
            _selectStoredSave = false;
        }

        private void ReloadSaves()
        {
            // load the savestates
            SaveStateManager.LoadSaveData();

            // update the UI
            UpdateUi();
        }

        private void UpdateUi()
        {
            for (var i = 0; i < SaveStateManager.SaveCount; i++)
            {
                if (SaveStateManager.SaveStates[i] == null)
                {
                    _saveButtons[i].InsideElement = _newGameButtonLayout;
                    continue;
                }
                else
                {
                    _saveButtons[i].InsideElement = _saveButtonLayouts[i];
                }
                // If the player has stolen an item, replace it with the "Thief" name.
                _saveNames[i].SetText(SaveStateManager.SaveStates[i].Thief 
                    ? Game1.LanguageManager.GetString("savename_thief", "error") 
                    : SaveStateManager.SaveStates[i].Name);

                // Load the players rupee count.
                _saveRupees[i].SetText(SaveStateManager.SaveStates[i].CurrentRupees.ToString());

                // Does the player have: level 2 sword, mirror shield, colored tunic.
                var sword = SaveStateManager.SaveStates[i].SwordLevel2;
                var cloak = SaveStateManager.SaveStates[i].CloakType;
                var shield = SaveStateManager.SaveStates[i].MirrorShield;

                // The "animation" ID to play based on tunic color.
                string baseColor = cloak switch
                {
                    1 => "blue",
                    2 => "red",
                    _ => "green"
                };
                // Player has the Level 2 sword so show it on Link's sprite.
                _playerImage[i].ShowSword = sword;

                // Player has the mirror shield so show it on Link's sprite.
                cloakColors[i] = shield ? baseColor + "s" : baseColor;

                // Playtime format displays as: HH:MM
                var totalMinutes = SaveStateManager.SaveStates[i].TotalPlaytime;
                var hours = (int)(totalMinutes / 60);
                var minutes = (int)(totalMinutes % 60);
                var playtimeText = $"{hours:D2}:{minutes:D2}";
                _savePlaytime[i].SetText(playtimeText);

                for (var j = 0; j < 14; j++)
                {
                    // only draw the hearts the player has
                    _heartImage[i][j].Hidden = SaveStateManager.SaveStates[i].MaxHearts <= j;

                    var state = 4 - MathHelper.Clamp(SaveStateManager.SaveStates[i].CurrentHealth - (j * 4), 0, 4);

                    ((InterfaceImage)_heartImage[i][j]).SourceRectangle = new Rectangle(
                        _heartSprite.ScaledRectangle.X + (_heartSprite.ScaledRectangle.Width + _heartSprite.TextureScale) * state,
                        _heartSprite.ScaledRectangle.Y,
                        _heartSprite.ScaledRectangle.Width, _heartSprite.ScaledRectangle.Height);
                }
            }
        }
    }
}
