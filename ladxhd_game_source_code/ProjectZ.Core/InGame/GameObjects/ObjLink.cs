using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectZ.Base;
using ProjectZ.Base.UI;
using ProjectZ.InGame.Controls;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.GameObjects.Base.Systems;
using ProjectZ.InGame.GameObjects.Bosses;
using ProjectZ.InGame.GameObjects.Dungeon;
using ProjectZ.InGame.GameObjects.Effects;
using ProjectZ.InGame.GameObjects.Enemies;
using ProjectZ.InGame.GameObjects.MidBoss;
using ProjectZ.InGame.GameObjects.NPCs;
using ProjectZ.InGame.GameObjects.Things;
using ProjectZ.InGame.GameSystems;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects
{
    public partial class ObjLink : GameObject
    {
        public enum State
        {
            Idle, Pushing, Grabbing, Pulling, PreCarrying, Carrying, Throwing, CarryingItem, PickingUp, Falling,
            Attacking, Blocking, AttackBlocking, Charging, ChargeBlocking, Jumping, AttackJumping, ChargeJumping,
            Ocarina, OcarinaTeleport, Rafting, Pushed,
            FallRotateEntry,
            Drowning, Drowned, Swimming, AttackSwimming, ChargeSwimming,
            Teleporting, MagicRod, Hookshot, Bombing, Powdering, Digging, BootKnockback,
            TeleporterUpWait, TeleporterUp, TeleportFallWait, TeleportFall,
            Dying, InitStunned, Stunned, Knockout,
            SwordShow0, SwordShow1, SwordShow2, SwordShowLv2,
            ShowInstrumentPart0, ShowInstrumentPart1, ShowInstrumentPart2, ShowInstrumentPart3,
            ShowToadstool,
            CloakShow0, CloakShow1,
            Intro, BedTransition,
            Sequence, FinalInstruments,
            Frozen, FinalStand
        }
        public State CurrentState;

        // Link Animator
        public readonly Animator Animation;
        private int _animationOffsetX = -7;
        private int _animationOffsetY = -16;

        // Weapon Animator
        private Animator AnimatorWeapons;

        // Link Sprite
        private CSprite _sprite;
        public float SpriteTransparency;
        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                _sprite.IsVisible = value;
            }
        }
        // Link Position
        public Vector2 Position => EntityPosition.Position;
        public CPosition CenterPosition => new CPosition(EntityPosition.X, EntityPosition.Y - 4, EntityPosition.Z);
        public float PosX => EntityPosition.X;
        public float PosY => EntityPosition.Y;
        public float PosZ => EntityPosition.Z;

        // Link Movement
        public bool CanWalk;
        private bool _isWalking;
        private bool _forceWalking;
        private float WalkSpeed = 1.0f;
        private float WalkSpeedPoP = 1.25f;
        private float BootsRunningSpeed = 2.0f;
        private float SwimSpeed = 0.5f;
        private float SwimSpeedA = 1.0f;
        private float _currentWalkSpeed;
        private float _waterSoundCounter;

        public Vector2 LastMoveVector;
        private Vector2 _moveVelocity;
        private Vector2 _lastMoveVelocity;
        private Vector2 _lastBaseMoveVelocity;

        // Link Direction
        public int Direction;
        private readonly Vector2[] _walkDirection = 
        { 
            new Vector2(-1, 0), 
            new Vector2(0, -1), 
            new Vector2(1, 0), 
            new Vector2(0, 1) 
        };
        public Vector2 ForwardVector { get => _walkDirection[Direction]; }

        // Link Body
        public BodyComponent _body;
        public RectangleF BodyRectangle => _body.BodyBox.Box.Rectangle();
        public RectangleF PlayerRectangle => new RectangleF(PosX - 4, PosY - 12 - PosZ, 8, 12);
        private BodyDrawComponent _drawBody;
        private BodyDrawShadowComponent _shadowComponent;
        private DrawComponent.DrawTemplate _bodyDrawFunction;
        public Point CollisionBoxSize;

        // Holes
        private Point _lastTilePosition;
        private Vector3 _holeResetPosition;
        private Vector3 _altHoleResetPosition;
        private float _holeFallCounter;
        private bool _isFallingIntoHole;
        private double _holeTeleportCounter;
        public int HoleTeleporterId;
        public bool WasHoleReset;
        public bool HoleFalling;
        public string HoleResetRoom;
        public string HoleResetEntryId;

        // Transitions
        public Vector2? MapTransitionStart;
        public Vector2? MapTransitionEnd;
        public Vector2? NextMapPositionStart;
        public Vector2? NextMapPositionEnd;
        public string NextMapPositionId;
        public int DirectionEntry;
        public bool IsTransitioning;
        public bool NextMapFallStart;
        public bool NextMapFallRotateStart;
        public bool TransitionOutWalking;
        public bool TransitionInWalking;
        public bool BlackScreenOverride;
        public bool OcarinaDungeonTeleport;
        private double _fallEntryCounter;
        private bool _wasTransitioning;
        private bool _startBedTransition;

        // Rail Jump (jumping from cliffside)
        private bool _railJump;
        private bool _startedJumping;
        private bool _hasStartedJumping;
        private float _railJumpPositionZ;
        private float _railJumpPercentage;
        private float _railJumpHeight;
        private Vector2 _railJumpStartPosition;
        private Vector2 _railJumpTargetPosition;

        // Followers
        public GameObjectFollower ObjFollower;
        private ObjCock _objRooster;
        private ObjMarin _objMaria;
        private ObjBowWow _objBowWow;

        private const string _spawnGhostKey = "spawn_ghost";
        private ObjGhost _objGhost;
        private bool _spawnGhost;
        private float _flyStartZPos;

        // Egg Follower Turnaround
        bool _eggPreventStart;
        float _eggPreventTimer;

        // Trapped State
        private int _trapInteractionCount;
        private bool _isTrapped;
        private bool _trappedDisableItems;

        // Sword 
        public bool IsPoking;
        public Box SwordDamageBox;
        private bool _pickingUpSword;
        private float _swordPokeTime = 100;
        private float _swordPokeCounter;
        private bool _pokeStart;
        private int _beamDirection;

        private bool _pokeRepelFix;
        private float _pokeRepelTimer;

        private Vector2[] _shootSwordOffset;
        private bool _shotSword;

        public CBox DamageCollider;
        private Vector2 _hitVelocity;
        private Vector2 _repelVelocity;
        private Vector2 _shieldVelocity;
        private Vector2 _knockBackVelocity;

        private float _baseRepelStrength = 2.45f;
        private float _swimRepelStrength = 2.25f;

        public static int BlinkTime = 66;
        public static int CooldownTime = BlinkTime * GameSettings.DmgCooldown;

        private double _hitCount;
        private double _hitRepelTime;
        private double _hitParticleTime;

        public bool InDamageState => _hitCount > 0;

        private float _swordChargeCounter = 100;
        private bool _swordPoked;
        private bool _stopCharging;

        private Point[] _pokeAnimationOffset;
        private bool _isHoldingSword;
        private bool _isSwordSpinning;
        private bool _isSwordSpinAttack;
        public bool CarrySword;

        // Sword Level 2
        private float _showSwordLv2Counter;
        private float _showSwordL2ParticleCounter;
        private bool _shownSwordLv2Dialog;

        // Prevents sword from colliding with NPCs.
        private Vector2 _avoidanceStartPos;
        private int _avoidanceDirection;
        private bool _npcSwordCross;
        private bool _npcCrossSword;

        // Items: Show
        public GameItem ShowItem;
        private Vector2 _showItemOffset;
        private GameItemCollected _collectedShowItem;

        // Items: Pickup Delay
        private const float PreCarryTime = 200;
        private float _preCarryCounter;

        // Items: Pickup
        private string _pickupDialogOverride;
        private string _additionalPickupDialog;
        private double _itemShowCounter;
        private bool _showItem;
        private bool _savedPreItemPickup;
        public bool SavePreItemPickup => _savedPreItemPickup; 

        // Items: Disable
        public bool DisableItems;
        public float DisableItemCounter;

        // Items: Store Item
        public GameItem StoreItem;
        private int _storeItemWidth;
        private int _storeItemHeight;
        private Vector2 _storePickupPosition;
        private bool _showStealMessage;

        // Magic Powder
        private Vector2[] _powderOffset;

        // Bombs
        public List<ObjBomb> BombList = new List<ObjBomb>();
        private List<GameObject> _destroyableWallList = new List<GameObject>();
        private Vector2[] _bombOffset;

        // Flippers
        public bool HasFlippers;
        private int _lastSwimDirection;
        private Vector2 _swimVelocity;
        private float _swimBoostCount;
        private float _diveCounter;

        // No Flippers: Drowning
        private MapStates.FieldStates _lastFieldState;
        private Vector2 _drownResetPosition;
        private float _drownResetCounter;
        private bool _drownedInLava;

        // Pegasus Boots
        private bool _bootsHolding;
        private bool _bootsRunning;
        private bool _bootsWasRunning;
        private bool _bootsStop;
        private float _bootsCounter;
        private float _bootsParticleTime = 120f;
        private float _bootsMaxSpeed = 2.0f;
        private int _bootsLastDirection;
        private bool _bootsRunJump;
        private Box _crystalSmashBox;

        // Arrows
        private Vector2[] _arrowOffset;

        // Shield
        public Box ShieldBlockBox;
        public bool CarryShield;
        private bool _wasBlocking;
        private bool _blockButton;

        // Hookshot
        public ObjHookshot Hookshot = new ObjHookshot();
        private Vector2[] _hookshotOffset;
        private bool _hookshotPull;
        private bool _hookshotActive;
        private float _hookshotCounter;
        private float _hookshotCooldown;

        // Boomerang
        public ObjBoomerang Boomerang = new ObjBoomerang();
        public Vector2[] _boomerangOffset;

        // Magic Rod
        private Vector2[] _magicRodOffset;

        // Shovel
        private Vector2[] _shovelOffset;
        private Point _digPosition;
        private bool _hasDug;
        private bool _canDig;

        // Ocarina
        private List<GameObject> _ocarinaList = new List<GameObject>();
        private float _ocarinaCounter;
        private int _ocarinaNoteIndex;
        private int _ocarinaSong;
        public bool ManboTeleport;

        // Power Bracelet
        private RectangleF GrabRectangle;
        private const float PullTime = 100;
        private const float PullMaxTime = 400;
        private const float PullResetTime = -133;
        private float _pullCounter;
        private bool _isPulling;
        private bool _wasPulling;
        private GameObject _instantPickupObject;
        private bool _instantPickup;
        private bool _swimRoosterPickup;

        // Power Bracelet: Carry Object
        private GameObject _carriedGameObject;
        private DrawComponent _carriedObjDrawComp;
        private CarriableComponent _carriedComponent;
        private Vector3 _carryStartPosition;

        // Roc's Feather: Jumping
        private bool _canJump = true;
        private float _railJumpSpeed;
        private float _jumpEndTimer;
        public float _jumpStartZPos;

        // Roc's Feather: 2D Jumping
        private bool _jump2DHold;
        private bool _jump2DHeld;

        // Tunic Color Transition (Color Dungeon Reward)
        private int CloakTransitionTime = 2200;
        private float _cloakTransitionCounter;
        private float _cloakPercentage;
        private int CloakTransitionOutTime = 2500;
        private float _cloakTransitionOutCounter;

        // Teleporting
        private ObjDungeonTeleporter _teleporter;
        private string _teleportMap;
        private string _teleporterId;
        private float _teleportCounter;
        private float _teleportCounterFull;
        private int _teleportState;

        // Instruments
        private bool[] _noteInit = { false, false };
        private int[] _noteSpriteIndex = { 0, 0 };
        private double _instrumentPickupTime;
        private float _instrumentCounter;
        private int _instrumentIndex;
        private int _instrumentCycleTime = 1000;
        private bool _drawInstrumentEffect;
        private bool _pickingUpInstrument;
        private const int dist0 = 30;
        private const int dist1 = 15;
        private readonly Vector2[] _showInstrumentOffset = 
        {
            new Vector2(-dist1, -dist0), 
            new Vector2(dist1, -dist0), 
            new Vector2(dist0, dist1), 
            new Vector2(dist0, -dist1),
            new Vector2(dist1, dist0),
            new Vector2(-dist1, dist0),
            new Vector2(-dist0, -dist1),
            new Vector2(-dist0, dist1) 
        };
        private Rectangle[] _noteSourceRectangles = 
        {
            new Rectangle(145, 97, 10, 12),
            new Rectangle(156, 97, 6, 12) 
        };
        private readonly int[] _instrumentMusicIndex = { 31, 39, 40, 41, 42, 43, 44, 45 };

        // Raft
        private ObjRaft _objRaft;
        private bool _isRafting;

        // Pushing
        private Vector2 _pushStart;
        private Vector2 _pushEnd;
        private float _pushCounter;
        private int _pushTime;

        // Vacuum Enemy
        private float _rotationCounter;
        private bool _isRotating;
        private bool _wasRotating;
        public int _rotateDirection;

        // Stunned 
        private float _stunnedCounter;
        private bool _stunnedParticles;

        // Final Sequence
        private int _finalIndex;
        private double _finalSeqCounter;

        // Save Position
        public string SaveMap;
        public Vector2 SavePosition;
        public int SaveDirection;

        // Low Heart Alarm
        private float _lowHealthBeepCounter;
        private bool _enableHealthBeep;

        // Sprite Shadows
        private ObjSpriteShadow _spriteShadow;

        // Field Properties
        public Rectangle CurrentField = Rectangle.Empty;
        public Rectangle PreviousField = Rectangle.Empty;
        public Rectangle ContrastField = Rectangle.Empty;
        public ObjFieldBarrier[] FieldBarrier;
        public bool FieldChange;

        // Prevents Enemy Position Reset
        public bool PreventReset;
        public float PreventResetTimer;

        // Prevent Damage Hits (No Collision)
        private float PreventDamageTimer;

        // Miscellaneous
        private DictAtlasEntry _stunnedParticleSprite;

        public bool UpdatePlayer;
        public bool DisableInput;
        private bool _isLocked;
        private bool _isGrabbed;
        private bool _isFlying;
        private bool _wasFlying;
        private Map.Map _previousMap;
        private bool _pickingUpAnglerKey;

        public bool FreezeWorldAroundPlayer;
        public bool FreezeWorldForEvents;

        public bool WasPushing;

        //====================
        // Mod File Values
        //====================
        bool swordbeam_level1 = false;
        bool swordbeam_always = false;

        float sword_charge_time = 670;
        float boots_charge_time = 533;

        float feather_velocity = 2.35f;

        bool light_source = false;
        int light_red = 255;
        int light_grn = 255;
        int light_blu = 255;
        float light_bright = 1.0f;
        int light_size = 120;

        float dmg_shader_color1_red = 255;
        float dmg_shader_color1_grn = 181;
        float dmg_shader_color1_blu =  49;

        float dmg_shader_color2_red = 222;
        float dmg_shader_color2_grn = 0;
        float dmg_shader_color2_blu = 0;

        float dmg_shader_color3_red = 0;
        float dmg_shader_color3_grn = 0;
        float dmg_shader_color3_blu = 0;

        public ObjLink() : base((Map.Map)null)
        {
            // If a mod file exists load the values from it.
            string modFile = Path.Combine(Values.PathLAHDMods, "ObjLink.lahdmod");
            ModFile.Parse(modFile, this);

            Resources.DamageSpriteShader0["Color0"] = new Vector4(dmg_shader_color1_red/255f, dmg_shader_color1_grn/255f, dmg_shader_color1_blu/255f, 1.0f);
            Resources.DamageSpriteShader0["Color1"] = new Vector4(dmg_shader_color2_red/255f, dmg_shader_color2_grn/255f, dmg_shader_color2_blu/255f, 1.0f);
            Resources.DamageSpriteShader0["Color2"] = new Vector4(dmg_shader_color3_red/255f, dmg_shader_color3_grn/255f, dmg_shader_color3_blu/255f, 1.0f);

            EntityPosition = new CPosition(0, 0, 0);
            EntitySize = new Rectangle(-8, -16, 16, 16);

            // load the player + sword animations
            Animation = AnimatorSaveLoad.LoadAnimator("link0");
            AnimatorWeapons = AnimatorSaveLoad.LoadAnimator("Objects/sword");

            _stunnedParticleSprite = Resources.GetSprite("stunned particle");

            CollisionBoxSize = new Point(8, 8);

            _body = new BodyComponent(EntityPosition, -4, -10, 8, 10, 8)
            {
                IsPusher = true,
                IsSlider = true,
                MaxJumpHeight = 3,
                Drag = 0.72f,
                DragAir = 0.72f,
                Gravity = -0.15f,
                Gravity2D = 0.1f,
                AbsorbStop = 0.35f,
                AbsorbPercentage = 1f,
                HoleOnPull = OnHolePull,
                HoleAbsorb = OnHoleAbsorb,
                MoveCollision = OnMoveCollision,
                CollisionTypes = Values.CollisionTypes.Normal |
                                 Values.CollisionTypes.Enemy |
                                 Values.CollisionTypes.PlayerItem |
                                 Values.CollisionTypes.LadderTop,
            };

            DamageCollider = new CBox(EntityPosition, -5, -10, 10, 10, 8);

            _powderOffset = new[]
            {
                new Vector2(-12, 0),
                new Vector2(-2, -CollisionBoxSize.Y -5),
                new Vector2(12, 0),
                new Vector2(2, 10)
            };

            _boomerangOffset = new[]
            {
                new Vector2(-10, -3),
                new Vector2(-2, -CollisionBoxSize.Y -1),
                new Vector2(10, -3),
                new Vector2(2, 6)
            };

            _arrowOffset = new[]
            {
                new Vector2(-6, -2),
                new Vector2(-2, -CollisionBoxSize.Y -1),
                new Vector2(6, -2),
                new Vector2(2, 2)
            };

            _magicRodOffset = new[]
            {
                new Vector2(-10, -4),
                new Vector2(-4, -CollisionBoxSize.Y - 4),
                new Vector2(10, -4),
                new Vector2(3, 2)
            };

            _shootSwordOffset = new[]
            {
                new Vector2(-6, -4),
                new Vector2(-5, -CollisionBoxSize.Y - 3),
                new Vector2(6, -4),
                new Vector2(4, 2)
            };

            _hookshotOffset = new[]
            {
                new Vector2(-5, -4),
                new Vector2(-3, -CollisionBoxSize.Y - 2),
                new Vector2(5, -4),
                new Vector2(3, 0)
            };

            _shovelOffset = new[]
            {
                new Vector2(-9, -1),
                new Vector2(0, -14),
                new Vector2(9, -1),
                new Vector2(0, 1)
            };

            _bombOffset = new[]
            {
                new Vector2(-10, 0),
                new Vector2(0, -CollisionBoxSize.Y - 2),
                new Vector2(10, 0),
                new Vector2(0, 8)
            };

            _pokeAnimationOffset = new[]
            {
                new Point(-16, -4),
                new Point(-4, -CollisionBoxSize.Y - 16),
                new Point(16, -4),
                new Point(5, 12)
            };

            _sprite = new CSprite(EntityPosition);
            _drawBody = new BodyDrawComponent(_body, DrawLink, Values.LayerPlayer);
            _bodyDrawFunction = _drawBody.Draw;
            _drawBody.Draw = Draw;

            AddComponent(KeyChangeListenerComponent.Index, new KeyChangeListenerComponent(OnKeyChange));
            AddComponent(BodyComponent.Index, _body);
            AddComponent(BaseAnimationComponent.Index, new AnimationComponent(Animation, _sprite, new Vector2(_animationOffsetX, _animationOffsetY)));
            AddComponent(CollisionComponent.Index, new BodyCollisionComponent(_body, Values.CollisionTypes.Player));
            AddComponent(UpdateComponent.Index, new UpdateComponent(Update));
            AddComponent(DrawComponent.Index, _drawBody);
            AddComponent(DrawShadowComponent.Index, _shadowComponent = new BodyDrawShadowComponent(_body, _sprite));
            AddComponent(LightDrawComponent.Index, new LightDrawComponent(DrawLight));

            EntityPosition.AddPositionListener(typeof(CarriableComponent), UpdatePositionCarriedObject);

            // Set the move speed value the user chose.
            AlterMoveSpeed(GameSettings.MoveSpeedAdded);

            // If attacking in a jumping state, return to jumping state after attack.
            AnimatorWeapons.OnAnimationFinished = () =>
            {
                if (!_body.IsGrounded && CurrentState == State.AttackJumping)
                {
                    if (_isHoldingSword)
                    {
                        string shieldString = CarryShield
                            ? Game1.GameManager.ShieldLevel == 2 ? "ms_" : "s_"
                            : "_";

                        CurrentState = State.ChargeJumping;
                        AnimatorWeapons.Play("stand_" + Direction);
                        Animation.Play("cjump" + shieldString + Direction);
                        _swordPokeCounter = _swordPokeTime;
                    }
                    else
                    {
                        CurrentState = State.Jumping;
                        Animation.Play("jump_" + Direction);
                    }
                }
            };
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  UPDATE CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public void Update()
        {
            // Update the current field and make a field barrier if Classic Camera is enabled.
            UpdateCurrentField();

            // Variable that prevents "HitPlayer" method from firing.
            if (PreventDamageTimer > 0)
            {
                // Timer must be active for it to remain true.
                PreventDamageTimer -= Game1.DeltaTime;
                if (PreventDamageTimer <= 0)
                    PreventDamageTimer = 0;
            }
            // Update falling into a map transition (I think).
            if (CurrentState == State.FallRotateEntry)
            {
                _fallEntryCounter += Game1.DeltaTime;
                Direction = (int)(DirectionEntry + (_fallEntryCounter + 96) / 48) % 4;

                if (_body.IsGrounded)
                    CurrentState = State.Idle;

                UpdateAnimation();
            }
            // @HACK
            // this is only needed because the player should not be able to step into the door 1 frame
            // after finishing the transition this would cause the door transition to not start
            if (IsTransitioning || _wasTransitioning)
            {
                _wasTransitioning = IsTransitioning;
                return;
            }

            // Photo Mouse when rejecting having a picture taken.
            if (CurrentState == State.Pushed)
            {
                _pushCounter += Game1.DeltaTime;

                // push towards the target position
                if (_pushCounter > _pushTime)
                {
                    EntityPosition.Set(_pushEnd);
                    CurrentState = State.Idle;
                }
                else
                {
                    var percentage = MathF.Sin((_pushCounter / _pushTime) * MathF.PI * 0.5f);
                    var newPosition = Vector2.Lerp(_pushStart, _pushEnd, percentage);
                    EntityPosition.Set(newPosition);
                }
            }

            // need to update the bomb to make sure it does not explode while the player is not getting updated
            if (_carriedComponent != null && _carriedComponent.IsPickedUp)
            {
                // used to updated the position to match the animation
                // gets called twice when moving
                // not sure how this could be done better
                UpdatePositionCarriedObject(EntityPosition);
            }
            // If Link is currently locked. Usually set when a dialog is open.
            if (!UpdatePlayer)
            {
                // If holding a toadstool then disable inventory.
                if (CurrentState == State.ShowToadstool)
                    Game1.GameManager.InGameOverlay.DisableInventoryToggle = true;

                UpdatePlayer = true;

                // Only update Link's animation.
                if (!Is2DMode)
                    UpdateAnimation();
                else
                    Update2DFrozen();

                UpdateDive();
                UpdateOcarinaAnimation();
                UpdateDrawComponents();
                return;
            }
            // Low health beep.
            UpdateHeartWarningSound();

            if (CurrentState == State.FinalInstruments)
            {
                _finalSeqCounter -= Game1.DeltaTime;
                if (_finalIndex == 0)
                {
                    if (_finalSeqCounter <= 0)
                    {
                        _finalIndex = 1;
                        _finalSeqCounter += 2250;
                        Animation.Play("show1");
                        Game1.GameManager.PlaySoundEffect("D360-52-34");
                    }
                }
                else if (_finalIndex == 1)
                {
                    if (_finalSeqCounter <= 0)
                        ((MapShowSystem)Game1.GameManager.GameSystems[typeof(MapShowSystem)]).StartEnding();
                }
                return;
            }
            else if (CurrentState == State.CloakShow0)
            {
                _cloakTransitionCounter += Game1.DeltaTime;
                _cloakPercentage = _cloakTransitionCounter / CloakTransitionTime;

                if (_cloakTransitionCounter > CloakTransitionTime)
                {
                    _cloakPercentage = 1;

                    if (ShowItem == null)
                    {
                        Game1.GameManager.StartDialog("cloak_green");
                        Game1.GameManager.CloakType = GameManager.CloakGreen;
                    }
                    else if (ShowItem.Name == "cloakBlue")
                        Game1.GameManager.StartDialog("cloak_blue");
                    else if (ShowItem.Name == "cloakRed")
                        Game1.GameManager.StartDialog("cloak_red");

                    CurrentState = State.CloakShow1;

                    // add the item to the inventory
                    if (_collectedShowItem != null)
                    {
                        Game1.GameManager.CollectItem(_collectedShowItem, 0);
                        _collectedShowItem = null;
                    }

                    ShowItem = null;
                }
            }
            else if (CurrentState == State.CloakShow1)
            {
                _cloakTransitionOutCounter += Game1.DeltaTime;

                var transitionSystem = (MapTransitionSystem)Game1.GameManager.GameSystems[typeof(MapTransitionSystem)];
                transitionSystem.SetColorMode(Color.White, MathHelper.Clamp(_cloakTransitionOutCounter / 1000f, 0, 1));

                if (_cloakTransitionOutCounter > CloakTransitionOutTime)
                {
                    Game1.GameManager.StartDialogPath("color_fairy_4");

                    Direction = 3;
                    MapTransitionStart = EntityPosition.Position;
                    MapTransitionEnd = MapTransitionStart;
                    TransitionOutWalking = false;

                    // append a map change
                    ((MapTransitionSystem)Game1.GameManager.GameSystems[typeof(MapTransitionSystem)]).AppendMapChange("overworld.map", "cloakOut", false, true, Color.White, true);
                }
            }
            else if (CurrentState == State.ShowToadstool)
            {
                CurrentState = State.Idle;
            }
            else if (CurrentState == State.SwordShowLv2)
            {
                _showSwordL2ParticleCounter += Game1.DeltaTime;
                if (_showSwordL2ParticleCounter > 4800 && !_shownSwordLv2Dialog)
                {
                    _shownSwordLv2Dialog = true;
                    _showSwordL2ParticleCounter = 0;
                    Game1.GameManager.SetMusic(-1, 2);
                    Game1.GameManager.StartDialogPath("sword2Collected");
                }
                // make sure to show the sword while the dialog box is open
                else if (_shownSwordLv2Dialog)
                {
                    ShowItem = null;
                    CurrentState = State.Idle;
                }
            }
            else if (CurrentState == State.PickingUp && !_pickingUpInstrument && !_pickingUpSword && !_pickingUpAnglerKey)
            {
                Game1.GameManager.InGameOverlay.DisableInventoryToggle = true;
                FreezeWorldAroundPlayer = true;
            }
            else if (CurrentState == State.TeleporterUpWait)
            {
                _holeTeleportCounter += Game1.DeltaTime;
                if (_holeTeleportCounter > 1000)
                {
                    CurrentState = State.TeleporterUp;

                    _holeTeleportCounter -= 1000;
                    _shadowComponent.Transparency = 0;

                    Game1.GameManager.PlaySoundEffect("D360-37-25");
                }
            }
            else if (CurrentState == State.TeleporterUp)
            {
                _holeTeleportCounter += Game1.DeltaTime;
                var time = 400;

                EntityPosition.Z = (float)(_holeTeleportCounter / time) * 128;
                Direction = (int)(_holeTeleportCounter / 64) % 4;

                // fade in
                var percentage = MathHelper.Clamp(1 - ((float)_holeTeleportCounter - (time - 100)) / 100, 0, 1);
                SpriteTransparency = percentage;
                _shadowComponent.Transparency = percentage;

                if (_holeTeleportCounter > time)
                {
                    _holeTeleportCounter -= time;

                    if (ObjOverworldTeleporter.TeleporterDictionary.TryGetValue(HoleTeleporterId, out var teleporter))
                        teleporter.SetNextTeleporterPosition();
                    else
                        CurrentState = State.Idle;
                }
            }
            else if (CurrentState == State.TeleportFallWait)
            {
                _holeTeleportCounter += Game1.DeltaTime;
                var time = 350;

                if (_holeTeleportCounter > time)
                {
                    _holeTeleportCounter -= time - 50;
                    _body.Velocity = new Vector3(0, 0, 0);
                    CurrentState = State.TeleportFall;
                }
            }
            else if (CurrentState == State.TeleportFall)
            {
                _holeTeleportCounter += Game1.DeltaTime;
                Direction = (int)(_holeTeleportCounter / 64) % 4;

                // fade in
                var percentage = MathHelper.Clamp((float)_holeTeleportCounter / 100, 0, 1);

                if (_body.IsGrounded)
                {
                    percentage = 1;
                    CurrentState = State.Idle;

                    UpdateSaveLocation();

                    // save settings?
                    if (GameSettings.Autosave)
                    {
                        SettingsSaveLoad.SaveSettings();
                        SaveGameSaveLoad.SaveGame(Game1.GameManager, true);
                    }
                    Camera.SnapCamera = false;
                }
                SpriteTransparency = percentage;
                _shadowComponent.Transparency = percentage;
            }

            if (CurrentState == State.Knockout)
                return;

            // Stunned
            if (CurrentState == State.InitStunned && _hitVelocity.Length() < 0.25f)
            {
                Animation.Play("stunned");
                CurrentState = State.Stunned;
            }

            if (CurrentState == State.Stunned)
            {
                if (_stunnedCounter > 0)
                {
                    _body.DragAir = 0.95f;
                    _stunnedCounter -= Game1.DeltaTime;
                }
                if (_stunnedCounter <= 0)
                {
                    _body.DragAir = 0.9f;
                    CurrentState = State.Idle;
                }
            }
            AnimatorWeapons.Update();

            // update all the item stuff
            // this need to be before the update method to correctly start jumping?
            UpdateItem();

            if (Is2DMode)
                Update2D();
            else
                Update3D();

            UpdateOcarina();

            UpdateDamageShader();
            _hitCount -= Game1.DeltaTime;

            if (_savedPreItemPickup && (CurrentState == State.Idle || CurrentState == State.Swimming))
                EndPickup();

            // die?
            if (Game1.GameManager.CurrentHealth <= 0 && !Game1.GameManager.UseShockEffect)
                OnDeath();

            UpdateDrawComponents();

            if (DisableItemCounter > 0)
                DisableItemCounter -= Game1.DeltaTime;

            if (DisableItemCounter <= 0)
                DisableItems = false;

            HoleResetRoom = null;
            CanWalk = true;
            _canJump = true;
            _isLocked = false;

            _hasStartedJumping = _startedJumping;
            _startedJumping = false;

            _currentWalkSpeed = Game1.GameManager.PieceOfPowerIsActive ? WalkSpeedPoP : WalkSpeed;

            // Press the toggle HUD key (InGame/GameObjects/Things/Values.cs) to hide the UI.
            if (InputHandler.KeyPressed(Keys.OemTilde) || InputHandler.KeyPressed(Keys.Delete))
                UiManager.HideOverlay = !UiManager.HideOverlay;

            // Capture the current field now so it can be compared on the next frame to see if
            // the field has changed. We only want to update the FieldBarrier on field changes.
            ContrastField = CurrentField;

            // If input was disabled, enable it now.
            DisableInput = false;

            // Clear the sword damage box if none of the below is true.
            if (!IsAttackingState() && !IsChargingState() && !AnimatorWeapons.IsPlaying && !_bootsRunning)
                SwordDamageBox = Box.Empty;

            // Clear the shield box if it's not being utilized.
            if (!IsBlockingState() && !_bootsRunning)
                ShieldBlockBox = Box.Empty;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  UPDATE 3D CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void Update3D()
        {
            UpdateNPCAvoidance();
            UpdateIntro();
            UpdateBedTransition();
            UpdateRafting();
            UpdateFlying();
            UpdateTeleporting();
            UpdateSwordSequence();
            UpdateInstrumentSequence();
            UpdateSwimmingPartOne();
            UpdateIgnoresZ();
            UpdateDrownResetPosition();
            UpdateWalking();
            UpdateSwimmingPartTwo();
            UpdateMovementPhysics();
            UpdateJump();
            UpdateSavePosition();
            UpdateFallingIntoHole();
            UpdateAnimation();
            UpdateGhostSpawn();
            UpdateSpriteShadow();

            // Stop pushing animation but store it for use in other places.
            WasPushing = false;
            if (CurrentState == State.Pushing)
            {
                // WasPushing can be used outside of ObjLink to know if he was pushing or not.
                WasPushing = true;
                CurrentState = State.Idle;
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  DRAWING CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateDrawComponents()
        {
            if (_drawInstrumentEffect)
                _drawBody.Layer = Values.LayerTop;
            else
                _drawBody.Layer = (CurrentState == State.Swimming && _diveCounter > 0) ? Values.LayerBottom : Values.LayerPlayer;

            if ((CurrentState == State.Swimming && _diveCounter > 0) ||
                CurrentState == State.Drowning ||
                CurrentState == State.Drowned ||
                CurrentState == State.BedTransition || _isTrapped)
                _shadowComponent.IsActive = false;
            else
                _shadowComponent.IsActive = true;
        }

        private void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible)
                return;

            // draw the player sprite behind the sword
            if (Direction != 1 && !_isTrapped && CurrentState != State.ChargeSwimming)
                _bodyDrawFunction(spriteBatch);

            // draw the sword/magic rod
            if (IsAttackingState() ||
                IsChargingState() ||
                CurrentState == State.SwordShow1 ||
                CurrentState == State.MagicRod ||
                (_bootsRunning && CarrySword))
            {
                var changeColor = _swordChargeCounter <= 0 &&
                            Game1.TotalGameTime % (8 / 0.06) >= 4 / 0.06 &&
                            ObjectManager.CurrentEffect != Resources.DamageSpriteShader0.Effect;

                // Change the draw shader
                if (changeColor)
                {
                    spriteBatch.End();
                    ObjectManager.SpriteBatchBegin(spriteBatch, Resources.DamageSpriteShader0);
                }

                // Draw the sword. Use offset of 6 instead of 7 when 2D Link is swimming and charging.
                var swordXOffset = (Is2DMode && CurrentState == State.ChargeSwimming) ? 6 : 7;

                AnimatorWeapons.Draw(spriteBatch, new Vector2(EntityPosition.X - swordXOffset, EntityPosition.Y - 16 - EntityPosition.Z), Color.White);

                // Change the draw shader
                if (changeColor)
                {
                    spriteBatch.End();
                    ObjectManager.SpriteBatchBegin(spriteBatch, null);
                }
            }

            // draw the sword after the first pickup
            if (CurrentState == State.SwordShow2)
            {
                var itemSword = Game1.GameManager.ItemManager["sword1"];
                var position = new Vector2(
                    BodyRectangle.X - itemSword.SourceRectangle.Value.Width / 2f,
                    (EntityPosition.Y - EntityPosition.Z - 15) - itemSword.SourceRectangle.Value.Height);

                ItemDrawHelper.DrawItem(spriteBatch, itemSword, position, Color.White, 1, true);
            }

            // draw the toadstool
            if (CurrentState == State.ShowToadstool)
            {
                var itemToadstool = Game1.GameManager.ItemManager["toadstool"];
                var position = new Vector2(
                    BodyRectangle.X - itemToadstool.SourceRectangle.Value.Width / 2f,
                    (EntityPosition.Y - EntityPosition.Z - 15) - itemToadstool.SourceRectangle.Value.Height);

                ItemDrawHelper.DrawItem(spriteBatch, itemToadstool, position, Color.White, 1);
            }

            // draw the player sprite in front of the sword
            if (Direction == 1 && !_isTrapped || CurrentState == State.ChargeSwimming)
                _bodyDrawFunction(spriteBatch);

            if (_drawInstrumentEffect)
                DrawInstrumentEffect(spriteBatch);

            // draw the picked up store item
            if (StoreItem != null)
                ItemDrawHelper.DrawItem(spriteBatch, StoreItem, _storePickupPosition, Color.White, 1, true);

            // draw the shown item
            if (ShowItem != null)
            {
                var itemPosition = EntityPosition.Position + _showItemOffset;
                itemPosition.Y -= EntityPosition.Z;

                if (CurrentState == State.CloakShow0)
                {
                    ItemDrawHelper.DrawItem(spriteBatch, ShowItem, itemPosition, Color.White * (1 - _cloakPercentage), 1, true);
                }
                else if (ShowItem.Name == "sword2")
                {
                    var swordImage = Resources.GetSprite("sword2Show");
                    DrawHelper.DrawNormalized(spriteBatch, swordImage.Texture, itemPosition, swordImage.ScaledRectangle, Color.White, swordImage.Scale);
                }
                else
                    ItemDrawHelper.DrawItem(spriteBatch, ShowItem, itemPosition, Color.White, 1, true);
            }

            // draw the object the player is carrying
            if (_carriedObjDrawComp != null)
            {
                _carriedObjDrawComp.IsActive = true;
                _carriedObjDrawComp.Draw(spriteBatch);
                _carriedObjDrawComp.IsActive = false;
            }

            // draw the dots over the head in the stunned state
            if (CurrentState == State.Stunned && _stunnedParticles)
            {
                var rotation = (float)(Game1.TotalGameTime / 1200) * MathF.PI * 2;
                var offset0 = new Vector2(MathF.Cos(rotation) * 8 - 2, MathF.Sin(rotation) * 3 - 2);
                DrawHelper.DrawNormalized(spriteBatch, _stunnedParticleSprite,
                    offset0 + new Vector2(EntityPosition.X, EntityPosition.Y - EntityPosition.Z - 18), Color.White);

                var offset1 = new Vector2(MathF.Cos(rotation + MathF.PI) * 8 - 2, MathF.Sin(rotation + MathF.PI) * 3 - 2);
                DrawHelper.DrawNormalized(spriteBatch, _stunnedParticleSprite,
                    offset1 + new Vector2(EntityPosition.X, EntityPosition.Y - EntityPosition.Z - 18), Color.White);
            }

            if (CurrentState == State.SwordShowLv2)
                DrawSwordL2Particles(spriteBatch);

            // draw the notes while showing an instrument
            {
                var leftNotePosition = new Vector2(EntityPosition.X - 8, EntityPosition.Y - 24);
                DrawNote(spriteBatch, leftNotePosition, new Vector2(-0.4f, -1.0f), 0);

                var rightNotePosition = new Vector2(EntityPosition.X + 8, EntityPosition.Y - 24);
                DrawNote(spriteBatch, rightNotePosition, new Vector2(0.4f, -1.0f), 1);
            }

            if (CurrentState == State.FinalInstruments)
                DrawFinalInstruments(spriteBatch);

            // Draw boxes when pressing F2 and Debug/Editor is enabled.
            if (Game1.DebugMode)
            {
                // Draw the save hole position.
                spriteBatch.Draw(Resources.SprWhite,
                    new Vector2(_holeResetPosition.X - 5, _holeResetPosition.Y - 5), new Rectangle(0, 0,
                       10, 10), Color.HotPink * 0.65f);

                // Draw weapon damage rectangle.
                var swordRectangle = SwordDamageBox.Rectangle();
                spriteBatch.Draw(Resources.SprWhite,
                    new Vector2(swordRectangle.X, swordRectangle.Y), new Rectangle(0, 0,
                        (int)swordRectangle.Width, (int)swordRectangle.Height), Color.Blue * 0.75f);

                // Draw shield rectangle.
                var shieldRectangle = ShieldBlockBox.Rectangle();
                spriteBatch.Draw(Resources.SprWhite,
                    new Vector2(shieldRectangle.X, shieldRectangle.Y), new Rectangle(0, 0,
                        (int)shieldRectangle.Width, (int)shieldRectangle.Height), Color.Green * 0.75f);

                // Draw dash smash rectangle.
                var dashRectangle = _crystalSmashBox.Rectangle();
                spriteBatch.Draw(Resources.SprWhite,
                    new Vector2(dashRectangle.X, dashRectangle.Y), new Rectangle(0, 0,
                        (int)dashRectangle.Width, (int)dashRectangle.Height), Color.Red * 0.75f);

                // Draw grab rectangle.
                spriteBatch.Draw(Resources.SprWhite,
                    new Vector2(GrabRectangle.X, GrabRectangle.Y), new Rectangle(0, 0,
                        (int)GrabRectangle.Width, (int)GrabRectangle.Height), Color.Yellow * 0.75f);

                // Draw the field barrier.
                if (FieldBarrier != null)
                {
                    foreach (var barrier in FieldBarrier)
                    {
                        spriteBatch.Draw(Resources.SprWhite,
                            new Vector2(barrier.CollisionBox.X, barrier.CollisionBox.Y), new Rectangle(0, 0,
                            (int)barrier.CollisionBox.Width, (int)barrier.CollisionBox.Height), Color.Blue * 0.75f);
                    }
                }
            }
        }

        private void DrawSwordL2Particles(SpriteBatch spriteBatch)
        {
            DrawSwordParticle(spriteBatch, new Vector2(EntityPosition.X - 4, EntityPosition.Y - 22), new Vector2(-32, -16), -125, 300, 200, 0);
            DrawSwordParticle(spriteBatch, new Vector2(EntityPosition.X - 4, EntityPosition.Y - 22), new Vector2(-32, -16), -125 - 250, 300, 200, 0);

            DrawSwordParticle(spriteBatch, new Vector2(EntityPosition.X - 4, EntityPosition.Y - 22), new Vector2(-32, -32), 0, 300, 200, 1);
            DrawSwordParticle(spriteBatch, new Vector2(EntityPosition.X - 4, EntityPosition.Y - 22), new Vector2(-32, -32), -250, 300, 200, 1);

            DrawSwordParticle(spriteBatch, new Vector2(EntityPosition.X - 4, EntityPosition.Y - 22), new Vector2(-24, -52), -50, 450, 50, 2);
            DrawSwordParticle(spriteBatch, new Vector2(EntityPosition.X - 4, EntityPosition.Y - 22), new Vector2(-24, -52), -50 - 250, 450, 50, 2);

            DrawSwordParticle(spriteBatch, new Vector2(EntityPosition.X - 4, EntityPosition.Y - 22), new Vector2(0, -64), -75, 450, 50, 3);
            DrawSwordParticle(spriteBatch, new Vector2(EntityPosition.X - 4, EntityPosition.Y - 22), new Vector2(0, -64), -75 - 250, 450, 50, 3);

            DrawSwordParticle(spriteBatch, new Vector2(EntityPosition.X - 4, EntityPosition.Y - 22), new Vector2(24, -52), -50, 450, 50, 4);
            DrawSwordParticle(spriteBatch, new Vector2(EntityPosition.X - 4, EntityPosition.Y - 22), new Vector2(24, -52), -50 - 250, 450, 50, 4);

            DrawSwordParticle(spriteBatch, new Vector2(EntityPosition.X - 4, EntityPosition.Y - 22), new Vector2(32, -32), 0, 300, 200, 5);
            DrawSwordParticle(spriteBatch, new Vector2(EntityPosition.X - 4, EntityPosition.Y - 22), new Vector2(32, -32), -250, 300, 200, 5);

            DrawSwordParticle(spriteBatch, new Vector2(EntityPosition.X - 4, EntityPosition.Y - 22), new Vector2(32, -16), -125, 300, 200, 6);
            DrawSwordParticle(spriteBatch, new Vector2(EntityPosition.X - 4, EntityPosition.Y - 22), new Vector2(32, -16), -125 - 250, 300, 200, 6);
        }

        private void DrawInstrumentEffect(SpriteBatch spriteBatch)
        {
            var fadeTime = 100;
            var speed = 500;
            var center = new Vector2(EntityPosition.X, EntityPosition.Y - 20);

            {
                var time = (float)(Game1.TotalGameTime % speed);
                var state = MathF.Sin((time / speed) * MathF.PI * 0.475f);
                var distance = 32 - 20 * state;
                var transparency = MathHelper.Clamp(time / fadeTime, 0, 1) *
                                   MathHelper.Clamp((speed - time) / fadeTime, 0, 1);
                var sourceRectangle = time < (speed / 1.65f) ? new Rectangle(194, 114, 12, 12) : new Rectangle(194, 98, 12, 12);

                for (var y = 0; y < 2; y++)
                    for (var x = 0; x < 2; x++)
                    {
                        var position = new Vector2(
                            center.X - 6 + (x * 2 - 1) * distance,
                            center.Y - 6 + (y * 2 - 1) * distance);
                        spriteBatch.Draw(Resources.SprItem, position, sourceRectangle,
                            Color.White * transparency, 0, Vector2.Zero, Vector2.One,
                            (x == 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None) |
                            (y == 0 ? SpriteEffects.FlipVertically : SpriteEffects.None), 0);
                    }
            }

            {
                var time = (float)((Game1.TotalGameTime + speed / 2) % speed);
                var state = MathF.Sin((time / speed) * MathF.PI * 0.475f);
                var distance = 40 - 34 * state;
                var transparency = MathHelper.Clamp(time / fadeTime, 0, 1) *
                                   MathHelper.Clamp((speed - time) / fadeTime, 0, 1);
                var sourceRectangle = time < (speed / 1.65f) ? new Rectangle(176, 116, 16, 8) : new Rectangle(176, 100, 16, 8);

                for (var y = 0; y < 2; y++)
                    for (var x = 0; x < 2; x++)
                    {
                        var rotation = (float)((x * 2 + y) * Math.PI / 2);

                        var position = new Vector2(
                            center.X + (y == 0 ? (x * 2 - 1) * distance : 0),
                            center.Y + (y == 0 ? 0 : (x * 2 - 1) * distance));

                        spriteBatch.Draw(Resources.SprItem, position, sourceRectangle,
                            Color.White * transparency, rotation, new Vector2(16, 4), Vector2.One, SpriteEffects.None, 0);
                    }
            }
        }

        private void DrawFinalInstruments(SpriteBatch spriteBatch)
        {
            if (_finalIndex != 1)
                return;

            var percentage = 0.25f + Math.Clamp((float)(2500 - _finalSeqCounter) / 2000, 0, 1) * 0.75f;

            // draw the instruments
            for (var i = 0; i < 8; i++)
            {
                var itemInstrument = Game1.GameManager.ItemManager["instrument" + i];
                var position = new Vector2(EntityPosition.X - 8, EntityPosition.Y - 60) + _showInstrumentOffset[i] * percentage;
                ItemDrawHelper.DrawItem(spriteBatch, itemInstrument, position, Color.White, 1, true);
            }
        }

        private void DrawLink(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch);

            // draw the colored cloak
            var texture = _sprite.SprTexture;

            var cloakColor = Game1.GameManager.CloakColor;
            if (CurrentState == State.CloakShow0 && ShowItem == null)
                cloakColor = Color.Lerp(cloakColor, ItemDrawHelper.CloakColors[0], _cloakPercentage);
            else if (CurrentState == State.CloakShow0 && ShowItem != null && ShowItem.Name == "cloakBlue")
                cloakColor = Color.Lerp(cloakColor, ItemDrawHelper.CloakColors[1], _cloakPercentage);
            else if (CurrentState == State.CloakShow0 && ShowItem != null && ShowItem.Name == "cloakRed")
                cloakColor = Color.Lerp(cloakColor, ItemDrawHelper.CloakColors[2], _cloakPercentage);

            _sprite.Color = cloakColor * SpriteTransparency;
            _sprite.SprTexture = Resources.SprLinkCloak;
            _sprite.Draw(spriteBatch);

            _sprite.Color = Color.White * SpriteTransparency;
            _sprite.SprTexture = texture;
        }

        private void DrawNote(SpriteBatch spriteBatch, Vector2 position, Vector2 direction, int noteIndex)
        {
            var timeOffset = noteIndex * _instrumentCycleTime / 2;

            if (_instrumentCounter < timeOffset ||
                (CurrentState != State.ShowInstrumentPart1 || _drawInstrumentEffect) &&
                ((_instrumentCounter - timeOffset) / _instrumentCycleTime + 1) * _instrumentCycleTime + timeOffset > 0)
                return;

            var time = (_instrumentCounter + timeOffset) % _instrumentCycleTime;

            var transparency = 1.0f;
            // fade out
            if (time > _instrumentCycleTime - 100)
            {
                _noteInit[noteIndex] = false;
                transparency = (_instrumentCycleTime - time) / 100f;
            }
            // fade in
            else if (time < 100)
            {
                if (!_noteInit[noteIndex])
                {
                    _noteInit[noteIndex] = true;
                    _noteSpriteIndex[noteIndex] = Game1.RandomNumber.Next(0, 2);

                }
                transparency = time / 100;
            }
            position += direction * time * 0.02f + new Vector2(-direction.X, direction.Y) * (float)Math.Sin(time * 0.015) * 0.75f;
            position += new Vector2(
                -_noteSourceRectangles[_noteSpriteIndex[noteIndex]].Width / 2f,
                -_noteSourceRectangles[_noteSpriteIndex[noteIndex]].Height);

            spriteBatch.Draw(Resources.SprItem, position,
                _noteSourceRectangles[_noteSpriteIndex[noteIndex]], Color.White * transparency);
        }

        private void DrawSwordParticle(SpriteBatch spriteBatch, Vector2 position, Vector2 direction, int timeOffset, int fullTime, int timeDelay, int index)
        {
            var fadeTime = 50;
            var particleTime = (_showSwordL2ParticleCounter + timeOffset) % (fullTime + timeDelay);
            var percentage = particleTime / fullTime;
            var colorTransparency = Math.Min((fullTime - particleTime) / fadeTime, particleTime / fadeTime);
            var particlePosition = position + percentage * direction;
            var spriteParticle = Resources.GetSprite("sword_particle_" + index);

            if (0 < particleTime && particleTime < fullTime)
                DrawHelper.DrawNormalized(spriteBatch, spriteParticle.Texture,
                    particlePosition - spriteParticle.Origin, spriteParticle.ScaledRectangle, Color.White * colorTransparency, spriteParticle.Scale);
        }

        private void DrawLight(SpriteBatch spriteBatch)
        {
            if (light_source && GameSettings.ObjectLights)
            {
                var _lightColor = new Color(light_red, light_grn, light_blu);
                var _lightRectangle = new Rectangle((int)_body.Position.X - light_size / 2, (int)_body.Position.Y - (int)_body.Position.Z - light_size / 2 - 6, light_size, light_size);
                spriteBatch.Draw(Resources.SprLight, _lightRectangle, _lightColor * light_bright);
            }
        }

        public void DrawTransition(SpriteBatch spriteBatch)
        {
            if (!IsVisible)
                return;

            _bodyDrawFunction(spriteBatch);

            if (_drawInstrumentEffect)
                DrawInstrumentEffect(spriteBatch);

            // draw the shown item
            if (ShowItem != null)
            {
                var itemPosition = EntityPosition.Position + _showItemOffset;
                ItemDrawHelper.DrawItem(spriteBatch, ShowItem, itemPosition, Color.White, 1, true);
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  KEY LISTENER CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnKeyChange()
        {
            // Freeze the game if the value is set.
            var strFreeze = "freezeGame";
            var FreezeGame = Game1.GameManager.SaveManager.GetString(strFreeze, "0");
            if (FreezeGame == "1")
                FreezeWorldForEvents = true;
            else if (FreezeGame == "0")
                FreezeWorldForEvents = false;

            // Change the color of the tunic in the color dungeon.
            var strCloak = "cloak_transition";
            var cloakTransition = Game1.GameManager.SaveManager.GetString(strCloak);
            if (cloakTransition == "1")
            {
                _cloakTransitionCounter = 0;
                _cloakPercentage = 0;
                _cloakTransitionOutCounter = 0;
                Game1.GameManager.SaveManager.RemoveString(strCloak);
                Game1.GameManager.SaveManager.SetString(strCloak, "0");
                CurrentState = State.CloakShow0;
            }
            if (cloakTransition == "2")
            {
                _cloakTransitionCounter = 0;
                _cloakPercentage = 0;
                _cloakTransitionOutCounter = 0;
                Game1.GameManager.SaveManager.RemoveString(strCloak);
                Game1.GameManager.SaveManager.SetString(strCloak, "0");
                CurrentState = State.CloakShow1;
            }

            // Movement was forced through "script.zScript".
            var moveValue = Game1.GameManager.SaveManager.GetString("link_move");
            if (!string.IsNullOrEmpty(moveValue))
            {
                CurrentState = State.Idle;

                // If the player was dashing reset anything related.
                _bootsHolding = false;
                _bootsRunning = false;
                _bootsWasRunning = false;
                Animation.SpeedMultiplier = 1.0f;

                var split = moveValue.Split(',');
                var directionX = float.Parse(split[0], CultureInfo.InvariantCulture);
                var directionY = float.Parse(split[1], CultureInfo.InvariantCulture);

                var velocity = new Vector2(directionX, directionY);
                _body.VelocityTarget = velocity;
                Direction = AnimationHelper.GetDirection(velocity);
                _forceWalking = true;

                Game1.GameManager.SaveManager.SetString("link_move_collision", "0");
                Game1.GameManager.SaveManager.RemoveString("link_move");
            }

            // Idle state was forced through "script.zScript".
            var idleValue = Game1.GameManager.SaveManager.GetString("link_idle");
            if (!string.IsNullOrEmpty(idleValue))
            {
                CurrentState = State.Idle;
                Game1.GameManager.SaveManager.RemoveString("link_idle");
            }

            // Facing was forced through "script.zScript".
            var strAnimation = "link_direction";
            var newDirection = Game1.GameManager.SaveManager.GetString(strAnimation);
            if (!string.IsNullOrEmpty(newDirection))
            {
                Direction = int.Parse(newDirection);
                UpdateAnimation();
                Game1.GameManager.SaveManager.SetString(strAnimation, null);
            }

            // Animation was forced through "script.zScript".
            var animationValue = Game1.GameManager.SaveManager.GetString("link_animation");
            if (!string.IsNullOrEmpty(animationValue))
            {
                Animation.Play(animationValue);
                CurrentState = State.Sequence;
                Game1.GameManager.SaveManager.RemoveString("link_animation");
            }

            // Diving was forced through "script.zScript".
            var diveValue = Game1.GameManager.SaveManager.GetString("link_dive");
            if (!string.IsNullOrEmpty(diveValue))
            {
                _diveCounter = int.Parse(diveValue);
                CurrentState = State.Swimming;
                Game1.GameManager.SaveManager.RemoveString("link_dive");
            }

            // Hide the HUD was forced through "script.zScript".
            var hideHudValue = Game1.GameManager.SaveManager.GetString("hide_hud");
            if (!string.IsNullOrEmpty(hideHudValue))
            {
                Game1.GameManager.InGameOverlay.HideHud(true);
                Game1.GameManager.SaveManager.RemoveString("hide_hud");
            }

            // Photo Mouse pushes Link back (used in photo sequences in "script.zScript").
            var pushValue = Game1.GameManager.SaveManager.GetString("link_push");
            if (!string.IsNullOrEmpty(pushValue))
            {
                var split = pushValue.Split(',');

                if (split.Length == 1)
                {
                    _pushStart = EntityPosition.Position;
                    _pushEnd = new Vector2(80, 94);
                    _pushTime = int.Parse(split[0]);
                }
                else
                {
                    var offsetX = float.Parse(split[0], CultureInfo.InvariantCulture);
                    var offsetY = float.Parse(split[1], CultureInfo.InvariantCulture);
                    _pushStart = EntityPosition.Position;
                    _pushEnd = _pushStart + new Vector2(offsetX, offsetY);
                    _pushTime = int.Parse(split[2]);
                }
                _pushCounter = 0;
                CurrentState = State.Pushed;
                Game1.GameManager.SaveManager.RemoveString("link_push");
            }

            // Used during the ending sequence to stop Link from walking.
            var linkFinalStairStand = Game1.GameManager.SaveManager.GetString("finalstairstand");
            if (!string.IsNullOrEmpty(linkFinalStairStand))
            {
                _forceWalking = false;
                CurrentState = State.FinalStand;
                Game1.GameManager.SaveManager.RemoveString("finalstairstand");
            }

            // Used during the ghost house sequence to stop Link from walking.
            var ghostAutoWalk = Game1.GameManager.SaveManager.GetString("ghost_autowalk");
            if (!string.IsNullOrEmpty(ghostAutoWalk) && ghostAutoWalk == "1")
            {
                _forceWalking = false;
                Game1.GameManager.SaveManager.RemoveString("ghost_autowalk");
            }

            // Used during the ending sequence when talking to the Wind Fish and showing the 8 instruments.
            var linkFinal = Game1.GameManager.SaveManager.GetString("link_final");
            if (!string.IsNullOrEmpty(linkFinal))
            {
                _finalIndex = 0;
                _finalSeqCounter = 1500;
                Animation.Play("final_stand_down");
                CurrentState = State.FinalInstruments;
                Game1.GameManager.SetMusic(62, 2);
                Game1.GameManager.SaveManager.RemoveString("link_final");
            }

            // Mountain photo sequence: Drop the rooster if flying when it starts.
            var mntPhoto = Game1.GameManager.SaveManager.GetString("photo_12", "0") == "1";
            var hasRooster = Game1.GameManager.SaveManager.GetString("has_rooster", "0") == "1";
            var dropRooster = Game1.GameManager.SaveManager.GetString("drop_rooster", "0") == "1";

            if (mntPhoto && hasRooster && dropRooster)
            {
                ReleaseCarriedObject();
                ReturnToIdle();
                Game1.GameManager.SaveManager.RemoveString("drop_rooster");
            }
            // Deletes a death from the death counter.
            var removeDeath = Game1.GameManager.SaveManager.GetString("remove_death", "0") == "1";
            if (removeDeath)
            {
                Game1.GameManager.DeathCount--;
                Game1.GameManager.SaveManager.RemoveString("remove_death");
            }
            // Dodongo snakes use an invisible button to reset their music.
            var dSnakeMusic = Game1.GameManager.SaveManager.GetString("dodongo_snake_music");
            if (!string.IsNullOrEmpty(dSnakeMusic))
            {
                Game1.GameManager.SetMusic(-1, 2);
                Game1.GameManager.SaveManager.RemoveString("dodongo_snake_music");
            }

            // Boomerang Trade: Hidden Goriya
            // Can be exchanged for: Shovel, Feather
            var boomerangValue = Game1.GameManager.SaveManager.GetString("boomerang_trade");
            if (!string.IsNullOrEmpty(boomerangValue))
            {
                Game1.GameManager.SaveManager.RemoveString("boomerang_trade");
                int index = GameSettings.SwapButtons ? 0 : 1;

                if (Game1.GameManager.Equipment[index] != null &&
                    (Game1.GameManager.Equipment[index].Name == "shovel" ||
                     Game1.GameManager.Equipment[index].Name == "feather" ||
                     Game1.GameManager.Equipment[index].Name == "magicRod" ||
                     Game1.GameManager.Equipment[index].Name == "hookshot"))
                {
                    Game1.GameManager.SaveManager.SetString("tradded_item", Game1.GameManager.Equipment[index].Name);
                    Game1.GameManager.Equipment[index] = null;
                    Game1.GameManager.StartDialogPath("npc_hidden_boomerang");
                }
                else
                {
                    Game1.GameManager.StartDialogPath("npc_hidden_reject");
                }
            }

            // Boomerang Return: Hidden Goriya
            var boomerangReturnValue = Game1.GameManager.SaveManager.GetString("boomerang_trade_return");
            if (!string.IsNullOrEmpty(boomerangReturnValue))
            {
                Game1.GameManager.SaveManager.RemoveString("boomerang_trade_return");

                // Remove the boomerang.
                Game1.GameManager.RemoveItem("boomerang", 1);

                // Return the traded item.
                var tradedItem = Game1.GameManager.SaveManager.GetString("tradded_item");
                var item = new GameItemCollected(tradedItem);
                MapManager.ObjLink.PickUpItem(item, true);
                _pickupDialogOverride = "npc_hidden_4";
            }

            // Spawn the Ghost who wants to go to the house by the bay.
            var spawnGhostValue = Game1.GameManager.SaveManager.GetString(_spawnGhostKey);
            if (!string.IsNullOrEmpty(spawnGhostValue))
            {
                _spawnGhost = true;
            }

            // We don't need this sticking around so remove it.
            var mountainPhoto = Game1.GameManager.SaveManager.GetString("start_mountain_photo");
            if (!string.IsNullOrEmpty(mountainPhoto))
            {
                Game1.GameManager.SaveManager.RemoveString("start_mountain_photo");
            }

            // Borrow the rooster from the hen house (after dungeon 8 is finished).
            var borrowRooster = Game1.GameManager.SaveManager.GetString("borrow_rooster");
            if (borrowRooster == "0")
            {
                Game1.GameManager.SaveManager.RemoveString("borrow_rooster");
                Map.Objects.RemoveObject(_objRooster);
                ObjFollower = _objRooster = null;
            }
            else if (borrowRooster == "1")
            {
                Game1.GameManager.SaveManager.RemoveString("borrow_rooster");
                var itemRooster = new GameItemCollected("rooster") { Count = 1 };
                PickUpItem(itemRooster, false, false, true);
                ObjFollower = _objRooster = new ObjCock(Map,
                    (int)(EntityPosition.X + AnimationHelper.DirectionOffset[Direction].X),
                    (int)(EntityPosition.Y + AnimationHelper.DirectionOffset[Direction].X),
                    "borrow_rooster");
                Map.Objects.SpawnObject(_objRooster);
                Map.Objects.RegisterAlwaysAnimateObject(ObjFollower);
                _objRooster.BorrowRooster();
            }

            // Take a walk with Marin (after dungeon 8 is finished).
            var borrowMarin = Game1.GameManager.SaveManager.GetString("borrow_marin");
            if (borrowMarin == "0")
            {
                Game1.GameManager.SaveManager.RemoveString("borrow_marin");
                ObjFollower = _objMaria = null;
            }
            else if (borrowMarin == "1")
            {
                Game1.GameManager.SaveManager.RemoveString("borrow_marin");
                var itemMarin = new GameItemCollected("marin") { Count = 1 };
                PickUpItem(itemMarin, false, false, true);
                SpawnMarin();
            }

            // Prevent entry to Egg with a follower during second chance.
            var egg_turn_around = Game1.GameManager.SaveManager.GetString("egg_turn_around");

            // Stop walking, reset timer, remove strings from SaveManager.
            if (egg_turn_around == "0")
            {
                _eggPreventStart = false;
                _forceWalking = false;
                _eggPreventTimer = 0;
                Game1.GameManager.SaveManager.RemoveString("link_move");
                Game1.GameManager.SaveManager.RemoveString("egg_turn_around");
            }
            // Drop any objects (like rooster), walk in reverse, start timer to disable.
            else if (egg_turn_around == "1")
            {
                ReleaseCarriedObject();
                Game1.GameManager.SaveManager.SetString("link_move", "0,1");
                MapManager.ObjLink.SeqLockPlayer();
                _eggPreventStart = true;
            }
            // Progress the timer to stop the follower turnaround sequence.
            if (_eggPreventStart)
            {
                _eggPreventTimer += Game1.DeltaTime;
                if (_eggPreventTimer > 2000)
                    Game1.GameManager.SaveManager.SetString("egg_turn_around", "0");
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  HIT PLAYER CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private bool WasBlocked(Box box, RectangleF boxRect, Vector2 boxCenter, Vector2 bodyCenter, int direction)
        {
            // Get the difference between the centers.
            Vector2 delta = bodyCenter - boxCenter;

            float halfW = boxRect.Width / 2f;
            float halfH = boxRect.Height / 2f;

            // A check to see if the two boxes are colliding.
            bool inside = Math.Abs(delta.X) <= halfW && Math.Abs(delta.Y) <= halfH;

            // Get the opposite direction.
            bool facingDir = Direction == ReverseDirection(direction);

            // If everything passes, it's a block.
            return (!inside || box.Intersects(ShieldBlockBox)) && facingDir;
        }

        public bool HitPlayer(Box box, HitType type, int damage, float pushMultiplier = 1.75f, int missileDir = -1)
        {
            // Prevent hits when playing the ocarina.
            if (PreventDamageTimer > 0)
                return false;

            // Get the box as a floats rectangle.
            RectangleF boxRect = box.Rectangle();

            // Get the centers of the rectangles.
            Vector2 boxCenter = new Vector2(boxRect.X + boxRect.Width / 2f, boxRect.Y + boxRect.Height / 2f); ;
            Vector2 bodyCenter = BodyRectangle.Center;
            Vector2 boxDir = bodyCenter - boxCenter;
            Vector2 vecDirection;
            int intDirection;

            // Get the intersecting rectangle.
            RectangleF intersection = BodyRectangle.GetIntersection(box.Rectangle());

            // If the rectangle isn't empty then use the box to calculate the direction.
            if (intersection.Width <= 0 || intersection.Height <= 0)
                vecDirection = boxDir;
            else
            {
                Vector2 interCenter = new Vector2(intersection.X + intersection.Width / 2f, intersection.Y + intersection.Height / 2f);
                vecDirection = bodyCenter - interCenter;
            }
            // Normalize the direction vector.
            if (vecDirection.LengthSquared() > 0.000001f)
                vecDirection.Normalize();

            // If the direction was passed use that. Otherwise calculate it. This
            // is used solely to determine if a block succeeds based on direction.
            if (missileDir >= 0)
                intDirection = missileDir;
            else
                intDirection = ToDirection(vecDirection);

            // Check if it's a projectile that was successfully blocked.
            bool blocked = WasBlocked(box, boxRect, boxCenter, bodyCenter, intDirection);

            // Try to damage the player.
            return HitPlayer(vecDirection * pushMultiplier, type, damage, blocked);
        }

        public bool HitPlayer(Vector2 direction, HitType type, int damage, bool blocked, int damageCooldown = 0)
        {
            // Check conditions where the player wouldn't take damage.
            if (_hitCount > 0 || CurrentState == State.Dying || CurrentState == State.PickingUp ||
                CurrentState == State.Drowning || CurrentState == State.Drowned || CurrentState == State.Knockout ||
                IsDiving() || Game1.GameManager.UseShockEffect || !UpdatePlayer || Hookshot.IsMoving)
            {
                return false;
            }
            // Check if the block conditions passed + Link is in blocking state or running with the shield.
            if (blocked && (IsBlockingState() || _bootsRunning && CarryShield))
            {
                if (type == HitType.Projectile)
                    Game1.GameManager.PlaySoundEffect("D360-22-16");

                return false;
            }
            // jump a little if we get hit by a spike
            if ((type & HitType.Spikes) != 0)
            {
                _body.Velocity.Z = 1.0f;
            }
            // redirect the down force to the sides
            if (Map.Is2dMap && _body.IsGrounded && direction.Y > 0)
            {
                direction.X += Math.Sign(direction.X) * Math.Abs(direction.Y) * 0.5f;
                direction.Y = 0;
            }
            // fall down on damage taken while climbing
            if (Map.Is2dMap && _isClimbing)
                _isClimbing = false;

            // Hit velocity is responsible for knockback.
            if (!_isRafting && !_isTrapped)
                _hitVelocity += direction;
            else
                _hitVelocity = Vector2.Zero;

            if (_hitCount > 0)
                return false;

            Game1.GameManager.PlaySoundEffect("D370-03-03");

            // Use the calculated cooldown if not set by an external call.
            if (damageCooldown != 0)
                _hitCount = damageCooldown;
            else
                _hitCount = CooldownTime;

            Game1.GameManager.InflictDamage(damage);

            // Shake the screen on damage if the user has it enabled.
            var freezeTime = 67;
            var shakeMult = (100.0f / freezeTime) * MathF.PI;
            Game1.FreezeTime = Game1.TotalGameTime + freezeTime;
            if (GameSettings.ExScreenShake)
                Game1.GameManager.ShakeScreen(freezeTime, (int)(direction.X * 2), (int)(direction.Y * 2), shakeMult, shakeMult);
            UpdateDamageShader();

            return true;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  DEATH CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnDeath()
        {
            // We only need to run this once.
            if (CurrentState == State.Dying)
                return;

            // Check to see if this is death by the shop keeper.
            bool shopPunish = Game1.GameManager.SaveManager.GetString("stoleItem", "0") == "1";
            bool shopFinish = Game1.GameManager.SaveManager.GetString("punishActive", "0") == "1";

            // Has potion?
            var potion = Game1.GameManager.GetItem("potion");

            // Use the potion if available but not if the death is by the shopkeeper.
            if (potion != null && potion.Count >= 1 && !shopPunish)
            {
                Game1.GameManager.RemoveItem("potion", 1);
                Game1.GameManager.HealPlayer(99);
                ItemDrawHelper.EnableHeartAnimationSound();
                return;
            }
            // Prevent future shopkeeper visits from slaughtering the player.
            if (shopPunish)
                Game1.GameManager.SaveManager.SetString("stoleItem", "0");

            // If carrying the rooster.
            if (IsFlying())
                ReleaseCarriedObject();

            // Set the dying state which prevents this from running again.
            CurrentState = State.Dying;
            Animation.Play("dying");

            // Stop the music and play the death sound effect.
            Game1.GameManager.StopMusic(true);
            Game1.GameManager.PlaySoundEffect("D370-08-08");

            // Set the correct start frame depending on the direction the player is facing
            int[] dirToFrame = { 0, 2, 1, 3 };
            Animation.SetFrame(dirToFrame[Direction]);

            // Begin the game over sequence.
            if (!shopFinish)
                ((GameOverSystem)Game1.GameManager.GameSystems[typeof(GameOverSystem)]).StartDeath();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  MOVEMENT CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateIgnoresZ()
        {
            if (CurrentState == State.Swimming ||
                CurrentState == State.Hookshot ||
                CurrentState == State.TeleporterUp ||
                CurrentState == State.TeleportFallWait || _isFlying || _isGrabbed || _isClimbing)
                _body.IgnoresZ = true;
            else
                _body.IgnoresZ = false;
        }

        private void UpdateWalking()
        {
            if (DisableInput) return;

            if (CurrentState != State.Idle &&
                !IsAttackingState() &&
                !IsChargingState() &&
                !IsBlockingState() &&
                !IsSwimmingState() &&
                CurrentState != State.CarryingItem &&
                CurrentState != State.Pushing &&
                CurrentState != State.Powdering &&
                CurrentState != State.Bombing &&
                CurrentState != State.MagicRod &&
                CurrentState != State.Throwing &&
                (CurrentState != State.Carrying || _isFlying) &&
                (!IsJumpingState() || _railJump) || !CanWalk || _isRafting)
                return;

            var walkVelocity = Vector2.Zero;

            if (!_isLocked && (!IsAttackingState() || !_body.IsGrounded))
                walkVelocity = ControlHandler.GetMoveVector2();

            var walkVelLength = walkVelocity.Length();
            if (walkVelLength > 1)
                walkVelocity.Normalize();

            var vectorDirection = ToDirection(walkVelocity);

            if (_bootsRunning && (walkVelLength < 0 || vectorDirection != ReverseDirection(Direction)))
            {
                if (_bootsLastDirection != Direction)
                    _bootsStop = true;

                if (!_bootsStop)
                {
                    _moveVelocity = AnimationHelper.DirectionOffset[Direction] * BootsRunningSpeed;

                    // can move up or down while running
                    if (Direction % 2 != 0)
                        _moveVelocity.X += walkVelocity.X;
                    else if (Direction % 2 == 0)
                        _moveVelocity.Y += walkVelocity.Y;
                }
                if (_isTrapped)
                {
                    _bootsStop = true;
                    _moveVelocity = Vector2.Zero;
                }
            }
            else if (walkVelLength > 0)
            {
                // slow down in the grass
                if (_body.CurrentFieldState.HasFlag(MapStates.FieldStates.Grass) && _body.IsGrounded)
                    _currentWalkSpeed *= 0.8f;

                // slow down in the water
                if (_body.CurrentFieldState.HasFlag(MapStates.FieldStates.Water) && _body.IsGrounded)
                {
                    _currentWalkSpeed *= 0.8f;

                    _waterSoundCounter += Game1.DeltaTime;
                    if (_waterSoundCounter > 250)
                    {
                        _waterSoundCounter -= 250;
                        Game1.GameManager.PlaySoundEffect("D360-14-0E", false);
                    }
                }

                // do not walk when trapped
                if (!_isTrapped)
                {
                    _isWalking = true;

                    if (_body.IsGrounded)
                    {
                        // after hitting the ground we still have _lastMoveVelocity
                        if (!_body.WasGrounded)
                            _moveVelocity = Vector2.Zero;

                        _moveVelocity += walkVelocity * _currentWalkSpeed;
                    }
                }
                // Update the direction the player is walking towards.
                if (!IsAttackingState() && !IsChargingState())
                {
                    Direction = ToDirection(walkVelocity);
                }
            }
            // Allow changing direction when attacking while standing still.
            else
            {
                Vector2 vecMoved = ControlHandler.GetMoveVector2();
                if ((CurrentState == State.Attacking || CurrentState == State.AttackBlocking) &&
                    !_isHoldingSword && vecMoved != Vector2.Zero && _body.IsGrounded)
                    Direction = ToDirection(vecMoved);
            }
            _lastBaseMoveVelocity = _moveVelocity;

            // Set the move vector for air movement while jumping off of a cliff.
            if (!_startedJumping && !_hasStartedJumping && _body.WasGrounded && !_body.IsGrounded)
                _lastMoveVelocity = _moveVelocity;

            // Standing on the ground, always reset the running jump variable.
            if (_body.IsGrounded && _body.Velocity.Z <= 0)
            {
                if (CurrentState == State.AttackJumping)
                    CurrentState = State.Attacking;
                _bootsRunJump = false;
            }
            else
            {
                // Detect first-frame running jump
                if (_bootsWasRunning)
                    _bootsRunJump = true;

                // Calculate target and difference
                Vector2 targetVelocity = walkVelocity * _currentWalkSpeed;
                float velocityDiff = (_lastMoveVelocity - targetVelocity).Length();
                float lerpAmount = Math.Clamp((0.05f / velocityDiff) * Game1.TimeMultiplier, 0, 1);

                if (velocityDiff > 0 && walkVelocity != Vector2.Zero)
                {
                    bool lockX = Math.Abs(_lastMoveVelocity.X) >= Math.Abs(_lastMoveVelocity.Y);

                    // Compute perpendicular Lerp as usual
                    Vector2 newMoveVelocity = Vector2.Lerp(_lastMoveVelocity, targetVelocity, lerpAmount);

                    if (_bootsRunJump)
                    {
                        // Running jump: determine locked axis and apply smooth slowdown if opposite input.
                        float lockedAxis = lockX ? _lastMoveVelocity.X : _lastMoveVelocity.Y;
                        float inputAxis = lockX ? walkVelocity.X : walkVelocity.Y;

                        lockedAxis = (Math.Sign(inputAxis) != Math.Sign(lockedAxis) && inputAxis != 0)
                            ? MathHelper.Lerp(lockedAxis, inputAxis * _currentWalkSpeed, lerpAmount)
                            : Math.Sign(lockedAxis) * _bootsMaxSpeed;

                        // Recombine axes
                        _lastMoveVelocity = lockX
                            ? new Vector2(lockedAxis, newMoveVelocity.Y)
                            : new Vector2(newMoveVelocity.X, lockedAxis);
                    }
                    else
                    {
                        // Normal jump: just use Lerp on both axes
                        _lastMoveVelocity = newMoveVelocity;
                    }
                }
                _moveVelocity = _lastMoveVelocity;
            }
        }

        private void OnMoveCollision(Values.BodyCollision collision)
        {
            // Detect hitting crystals made by the smash box created when dashing with Pegasus Boots.
            var dashSmashHit = Map.Objects.Hit(this, _crystalSmashBox.Center, _crystalSmashBox, HitType.CrystalSmash, 0, false, false);

            if (dashSmashHit == Values.HitCollision.Blocking)
                return;

            // Detect colliding with a solid object and perform knockback.
            if (CurrentState == State.Idle && _bootsWasRunning)
            {
                var knockBack = false;
                var pushUpward = 2.65f;
                var pushBackward = 1.85f;
                _knockBackVelocity = Vector2.Zero;

                if ((collision & Values.BodyCollision.Horizontal) != 0 && Direction % 2 == 0)
                {
                    var dirX = (collision & Values.BodyCollision.Left) != 0 ? -1 : 1;
                    _body.Velocity.X = -dirX;
                    _knockBackVelocity.X = -dirX * pushBackward;
                    knockBack = true;

                    if (GameSettings.ScreenShake)
                        Game1.GameManager.ShakeScreen(600, 1.00f, 0.50f, 11.0f, 5.00f, dirX, 1);
                }
                if ((collision & Values.BodyCollision.Vertical) != 0 && Direction % 2 != 0)
                {
                    var dirY = (collision & Values.BodyCollision.Top) != 0 ? -1 : 1;
                    _body.Velocity.Y = -dirY;
                    _knockBackVelocity.Y = -dirY * pushBackward;
                    knockBack = true;

                    if (GameSettings.ScreenShake)
                        Game1.GameManager.ShakeScreen(600, 0.50f, 1.00f, 5.00f, 11.0f, 1, dirY);
                }
                if (knockBack)
                {
                    _bootsRunning = false;
                    _bootsCounter = 0;
                    _body.Velocity.Z = pushUpward;
                    CurrentState = State.BootKnockback;

                    var damageOrigin = BodyRectangle.Center;
                    var damageBox = _body.BodyBox.Box;
                    damageBox.X += AnimationHelper.DirectionOffset[Direction].X;
                    damageBox.Y += AnimationHelper.DirectionOffset[Direction].Y;

                    Game1.GameManager.PlaySoundEffect("D360-11-0B");

                    Map.Objects.Hit(this, damageOrigin, damageBox, HitType.PegasusBootsPush, 0, false);
                }
            }

            // what is this?
            if ((collision & Values.BodyCollision.Floor) != 0)
            {
                _moveVelocity = _lastMoveVelocity * 0.5f;
                _lastBaseMoveVelocity = _moveVelocity;
            }

            if (CurrentState == State.BootKnockback &&
                (collision & Values.BodyCollision.Floor) != 0)
            {
                CurrentState = State.Idle;
                _body.Velocity.Z = 0;
            }

            if (Is2DMode)
                OnMoveCollision2D(collision);
            else
            {
                if (_isRotating)
                    return;

                // colliding horizontally or vertically? -> start pushing
                if (CurrentState == State.Idle &&
                    _body.IsGrounded && (_body.Velocity != Vector3.Zero || _body.VelocityTarget != Vector2.Zero) &&
                    ((collision & Values.BodyCollision.Horizontal) != 0 && (Direction == 0 || Direction == 2) ||
                    (collision & Values.BodyCollision.Vertical) != 0 && (Direction == 1 || Direction == 3)))
                {
                    var box = _body.BodyBox.Box;

                    // offset by one in the walk direction
                    box.X += AnimationHelper.DirectionOffset[Direction].X;
                    box.Y += AnimationHelper.DirectionOffset[Direction].Y;
                    var cBox = Box.Empty;
                    var outBox = Box.Empty;

                    // check if the object we are walking into is actually an object where the push animation should be played
                    if (ControlHandler.GetMoveVector2() != Vector2.Zero)
                        if (Map.Objects.Collision(box, cBox, _body.CollisionTypes, Values.CollisionTypes.PushIgnore, Direction, _body.Level, ref outBox))
                            CurrentState = State.Pushing;
                }

                if (CurrentState == State.Swimming)
                {
                    if ((collision & Values.BodyCollision.Horizontal) != 0)
                        _moveVelocity.X = 0;
                    if ((collision & Values.BodyCollision.Vertical) != 0)
                        _moveVelocity.Y = 0;
                }

                // used for scripting (final stript stop at the top of the stairs)
                Game1.GameManager.SaveManager.SetString("link_move_collision", "1");

                // stop the hit velocity if the are colliding with a wall
                // this was done because the player pushes into the hitVelocity direction
                if ((collision & Values.BodyCollision.Horizontal) != 0 && _body.VelocityTarget.X == 0)
                    _hitVelocity.X = 0;
                if ((collision & Values.BodyCollision.Vertical) != 0 && _body.VelocityTarget.Y == 0)
                    _hitVelocity.Y = 0;

                if (IsChargingState() &&
                    ((collision & Values.BodyCollision.Left) != 0 && Direction == 0 ||
                    (collision & Values.BodyCollision.Top) != 0 && Direction == 1 ||
                    (collision & Values.BodyCollision.Right) != 0 && Direction == 2 ||
                    (collision & Values.BodyCollision.Bottom) != 0 && Direction == 3))
                {
                    if (_swordPokeCounter <= 0)
                    {
                        IsPoking = true;
                        _pokeStart = true;

                        Animation.Play("poke_" + Direction);
                        AnimatorWeapons.Play("poke_" + Direction);
                        _swordChargeCounter = sword_charge_time;

                        // If in an accompanying state -> switch to a merged state.
                        if (IsSwimmingState())
                            CurrentState = State.AttackSwimming;
                        else if (IsJumpingState())
                            CurrentState = State.AttackJumping;
                        else if (IsBlockingState())
                            CurrentState = State.AttackBlocking;
                        else
                            CurrentState = State.Attacking;
                    }
                    _swordPokeCounter -= Game1.DeltaTime;
                }
                else
                {
                    IsPoking = false;
                    _swordPokeCounter = _swordPokeTime;
                }
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  MOVEMENT PHYSICS CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private static int BarrierToFacingDirection(int fieldBarrierIndex)
        {
            // The field barrier vs. the direction Link needs to be facing
            // to be knocked back into the nearby field. 
            return fieldBarrierIndex switch
            {
                0 => 3,  // - Top field, facing Down.
                1 => 1,  // - Bottom field, facing Up.
                2 => 2,  // - Left field, facing Right.
                3 => 0,  // - Right field, facing Left.
                _ => 0
            };
        }

        private void CancelBootsVelocity(int Index)
        {
            // If knockback direction would cross into another field, cancel it
            if (Direction == BarrierToFacingDirection(Index))
            {
                _knockBackVelocity = Vector2.Zero;
                _body.Velocity = Vector3.Zero;
                _body.VelocityTarget = Vector2.Zero;
                CurrentState = State.Idle;
            }
        }

        private void CancelRepelVelocities(bool cancelBodyVelocity)
        {
            // Eliminate all knockback velocities.
            _hitVelocity = Vector2.Zero;
            _repelVelocity = Vector2.Zero;
            _shieldVelocity = Vector2.Zero;
            _body.VelocityTarget = Vector2.Zero;

            // Also fully eliminate any body velocity.
            if (cancelBodyVelocity)
            {
                _body.Velocity.X = 0;
                _body.Velocity.Y = 0;
            }
        }

        private void PreventFieldKnockback()
        {
            // If it's null then Classic Camera is disabled.
            if (FieldBarrier == null)
                return;

            // Loop through the field barriers.
            for (int i = 0; i < FieldBarrier.Length; i++)
            {
                // Create a new box that is slightly larger than the field barrier box.
                int buffer = 2;
                Box fieldBox = new Box(FieldBarrier[i].Position.X - buffer, FieldBarrier[i].Position.Y - buffer, 0, FieldBarrier[i].Width + buffer * 2, FieldBarrier[i].Height + buffer * 2, 16);

                // If knocked into it then stop all velocities.
                if (_body.BodyBox.Box.Intersects(fieldBox))
                {
                    // Cancel repel velocities. Boots knockback uses a different function so it's possible
                    // to still be knocked back while inside a field barrier if parallel to the field.
                    if (CurrentState != State.BootKnockback)
                        CancelRepelVelocities(true);
                    else
                        CancelBootsVelocity(i);
                    return;  
                }
            }
        }

        private void UpdateMovementPhysics()
        {
            // Slows down walking movement when the player is hit.
            var moveMultiplier = MathHelper.Clamp(1f - _hitVelocity.Length(), 0, 1);

            // Calculate the movement velocity before being repelled.
            Vector2 finalMove = _moveVelocity * moveMultiplier + _hitVelocity;

            // If shield repelled, movement velocity is cancelled out.
            if (_shieldVelocity.Length() > 0.01f)
                finalMove = _shieldVelocity;

            // If sword repelled, movement velocity is cancelled out.
            if (_repelVelocity.Length() > 0.01f)
                finalMove = _repelVelocity;

            // Move the player by calculating the target velocity.
            if (CurrentState != State.Hookshot)
            {
                if (CurrentState == State.BootKnockback)
                    _body.VelocityTarget = _knockBackVelocity;
                else if (!Map.Is2dMap)
                    _body.VelocityTarget = finalMove;
                else
                    _body.VelocityTarget = _moveVector2D * moveMultiplier + _hitVelocity + _repelVelocity + _shieldVelocity;
            }
            // Store the current movement velocity and reset it.
            LastMoveVector = _moveVelocity;
            _moveVelocity = Vector2.Zero;

            //-----------------------------------------------------------------------------------------------------
            // Sword Repel Knockback: Overrides all other velocities.
            //-----------------------------------------------------------------------------------------------------
            // If the player is sword repelled then knock them back.
            if (_repelVelocity.Length() > 0.01f)
            {
                // Normalize the sword repel velocity.
                var repelNormal = _repelVelocity;
                repelNormal.Normalize();

                // Reduce velocity gradually while on the ground or while swimming.
                if (_body.IsGrounded || IsSwimmingState())
                {
                    float slowDownAmount = 0.12f + (_repelVelocity.Length() * 0.015f);
                    _repelVelocity -= repelNormal * slowDownAmount * Game1.TimeMultiplier;
                }
                // Also reduce velocity while in the air but only up to a certain point.
                else
                {
                    if (_repelVelocity.Length() > 1.20)
                    {
                        float slowDownAmount = 0.05f + (_repelVelocity.Length() * 0.015f);
                        _repelVelocity -= repelNormal * slowDownAmount * Game1.TimeMultiplier;
                    }
                }
                // Snap to zero when velocity reaches the threshold.
                if (_repelVelocity.Length() < 0.25f)
                {
                    _repelVelocity = Vector2.Zero;
                }
                // If the repel crosses into the field barrier then cancel the velocity.
                PreventFieldKnockback();
            }
            // Zero out sword repel velocity when it doesn't meet the thresholds.
            else
                _repelVelocity = Vector2.Zero;

            //-----------------------------------------------------------------------------------------------------
            // Shield Repel Knockback: Knockback from bumping enemy with shield. Overrides movement.
            //-----------------------------------------------------------------------------------------------------
            // If the player is shield repelled then knock them back.
            if (_shieldVelocity.Length() > 0.01f)
            {
                // Normalize the shield repel velocity.
                var shieldRepelNormal = _shieldVelocity;
                shieldRepelNormal.Normalize();

                // Reduce velocity gradually while on the ground or while swimming.
                if (_body.IsGrounded || IsSwimmingState())
                {
                    float slowDownAmount = 0.12f + (_shieldVelocity.Length() * 0.015f);
                    _shieldVelocity -= shieldRepelNormal * slowDownAmount * Game1.TimeMultiplier;
                }
                // Also reduce velocity while in the air but only up to a certain point.
                else
                {
                    if (_shieldVelocity.Length() > 1.20)
                    {
                        float slowDownAmount = 0.12f + (_shieldVelocity.Length() * 0.015f);
                        _shieldVelocity -= shieldRepelNormal * slowDownAmount * Game1.TimeMultiplier;
                    }
                }
                // Snap to zero when velocity reaches the threshold.
                if (_shieldVelocity.Length() < 0.25f)
                {
                    _shieldVelocity = Vector2.Zero;
                }
                // If the repel crosses into the field barrier then cancel the velocity.
                PreventFieldKnockback();
            }
            // Zero out shield repel velocity when it doesn't meet the thresholds.
            else
                _shieldVelocity = Vector2.Zero;

            //-----------------------------------------------------------------------------------------------------
            // Damage Hit Knockback: Knockback from taking damage. Movement can somewhat counter it.
            //-----------------------------------------------------------------------------------------------------
            // If the player is hit perform a knockback.
            if (_hitCount > 0 && _hitVelocity.Length() > 0.05f * Game1.TimeMultiplier)
            {
                // Normalize the hit velocity.
                var hitNormal = _hitVelocity;
                hitNormal.Normalize();

                // Reduce velocity gradually while on the ground or while swimming.
                if (_body.IsGrounded || IsSwimmingState())
                {
                    var slowDownAmount = 0.05f + MathHelper.Clamp(_hitVelocity.Length() / 25f, 0, 0.05f);
                    _hitVelocity -= hitNormal * slowDownAmount * Game1.TimeMultiplier;
                }
                // Also reduce velocity while in the air but only up to a certain point.
                else
                {
                    if (_hitVelocity.Length() > 1.20)
                    {
                        float slowDownAmount = 0.05f + (_hitVelocity.Length() * 0.015f);
                        _hitVelocity -= hitNormal * slowDownAmount * Game1.TimeMultiplier;
                    }
                }
                // Snap to zero when velocity reaches the threshold.
                if (_hitVelocity.Length() < 0.25f)
                {
                    _hitVelocity = Vector2.Zero;
                }
                // If the hit crosses into the field barrier then cancel the velocity.
                PreventFieldKnockback();
            }
            // Zero out hit velocity when it doesn't meet the thresholds.
            else
                _hitVelocity = Vector2.Zero;

            //-----------------------------------------------------------------------------------------------------
            // Pegasus Boots Knockback: Knockback when smashing into the wall with pegasus Boots.
            //-----------------------------------------------------------------------------------------------------
            if (CurrentState == State.BootKnockback && _knockBackVelocity.Length() > 0.01f)
            {
                // Decay Z velocity over time to create an arc.
                float decayZ = 0.15f;
                _body.Velocity.Z -= decayZ * Game1.TimeMultiplier;

                // Normalize the boots knockback velocity.
                var knockbackNormal = _knockBackVelocity;
                knockbackNormal.Normalize();

                // Apply slowdown over time.
                float slowDownAmount = 0.08f;
                _knockBackVelocity -= knockbackNormal * slowDownAmount * Game1.TimeMultiplier;

                // Snap to zero when velocity reaches the threshold.
                if (_knockBackVelocity.Length() < 0.2f)
                    _knockBackVelocity = Vector2.Zero;

                // Check if player has hit the ground and remove all velocities.
                if (_body.Position.Z <= 0 && _body.Velocity.Z <= 0)
                {
                    _body.Position.Z = 0;
                    _body.Velocity.Z = 0;
                    _knockBackVelocity = Vector2.Zero;
                    CurrentState = State.Idle;
                }
                // This will probably never occur, but prevent a knockback into another field.
                PreventFieldKnockback();
            }
            else if (CurrentState == State.BootKnockback)
            {
                _knockBackVelocity = Vector2.Zero;
                CurrentState = State.Idle;
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  RAIL JUMP CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public Vector2 RailJumpTarget() => _railJumpTargetPosition;
        public float RailJumpSpeed() => _railJumpSpeed;
        public float RailJumpHeight() => _railJumpHeight;
        public float RailJumpAmount() => _railJump ? _railJumpPercentage : 0f;

        public void StartRailJump(Vector2 goalPosition, float jumpHeightMultiply, float jumpSpeedMultiply, float goalPositionZ = 0)
        {
            if (_isRafting)
                return;

            if (CurrentState == State.Swimming)
                CurrentState = State.Idle;

            if (!Jump(false, false))
                return;

            Game1.GameManager.PlaySoundEffect("D360-08-08");

            _railJump = true;

            _railJumpStartPosition = EntityPosition.Position;
            _railJumpTargetPosition = goalPosition;

            // values for distance of 16
            _railJumpSpeed = 0.045f * jumpSpeedMultiply;
            _railJumpHeight = 12 * jumpHeightMultiply;
            _railJumpPositionZ = goalPositionZ;

            _railJumpPercentage = 0;

            _body.IgnoreHeight = true;
            _body.IgnoresZ = true;
            _body.Velocity.Z = 0;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  SWIMMING CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateSwimmingPartOne()
        {
            // Used only to lift the Flying Rooster out of the water.
            if (_swimRoosterPickup)
            {
                // Keep "_swimPickup" true until "Pulling" state is finished then set to false.
                _swimRoosterPickup = CurrentState == State.Pulling;
                return;
            }
            // we cant use the field state of the body because the raft updates the state while exiting
            var fieldState = SystemBody.GetFieldState(_body);

            // start/stop swimming or drowning
            if (!_isRafting && !_isFlying && fieldState.HasFlag(MapStates.FieldStates.DeepWater) && CurrentState != State.Dying)
            {
                if (!IsJumpingState() && CurrentState != State.PickingUp && _body.IsGrounded)
                {
                    ReleaseCarriedObject();
                    var inLava = fieldState.HasFlag(MapStates.FieldStates.Lava);

                    if (HasFlippers && !inLava && CurrentState != State.Swimming)
                    {
                        if (Map.Is2dMap && (CurrentState == State.Attacking || CurrentState == State.AttackSwimming))
                            CurrentState = State.AttackSwimming;
                        else if (Map.Is2dMap && (CurrentState == State.Charging || CurrentState == State.ChargeSwimming))
                            CurrentState = State.ChargeSwimming;
                        else
                            CurrentState = State.Swimming;

                        // Reset the "was flying" state when swimming. Swimming doesn't matter if player was flying.
                        _wasFlying = false;

                        // Only push the player if he walks into the water and does not jump. Jumping is handled in another location.
                        if (!_lastFieldState.HasFlag(fieldState))
                            _body.Velocity = new Vector3(_body.VelocityTarget.X, _body.VelocityTarget.Y, 0) * 0.35f;

                        // splash effect
                        var splashAnimator = new ObjAnimator(Map, 0, 0, 0, 3, Values.LayerPlayer, "Particles/splash", "idle", true);
                        splashAnimator.EntityPosition.Set(new Vector2(
                            _body.Position.X + _body.OffsetX + _body.Width / 2f,
                            _body.Position.Y + _body.OffsetY + _body.Height - _body.Position.Z - 6));
                        Map.Objects.SpawnObject(splashAnimator);

                        Game1.GameManager.PlaySoundEffect("D360-14-0E");

                        _diveCounter = 0;
                        _swimBoostCount = 0;
                        _swimVelocity = Vector2.Zero;
                    }
                    else if (!HasFlippers || inLava)
                    {
                        if (CurrentState != State.Drowning && CurrentState != State.Drowned)
                        {
                            // Only push Link if he walks into the water.
                            if (!_lastFieldState.HasFlag(fieldState))
                            {
                                // Use the controller move vector to determine the offset.
                                Vector2 move = ControlHandler.GetMoveVector2();
                                if (move != Vector2.Zero)
                                {
                                    move.Normalize();
                                    Vector2 offset = move * 5.5f;

                                    // The Y axis needs a lesser nudge when going down and a huge nudge going up.
                                    if (offset.Y < -5f) { offset = new Vector2(offset.X, -2f); }
                                    if (offset.Y > 5f) { offset = new Vector2(offset.X, 9f); }

                                    // Move Link to the offset position.
                                    EntityPosition.Set(EntityPosition.Position + offset);
                                }
                            }
                            // Spawn in the splash effect.
                            var splashAnimator = new ObjAnimator(Map, 0, 0, 0, 3, Values.LayerPlayer, "Particles/splash", "idle", true);
                            splashAnimator.EntityPosition.Set(new Vector2(
                                _body.Position.X + _body.OffsetX + _body.Width / 2f,
                                _body.Position.Y + _body.OffsetY + _body.Height - _body.Position.Z - 6));
                            Map.Objects.SpawnObject(splashAnimator);

                            Game1.GameManager.PlaySoundEffect("D370-03-03");

                            CurrentState = State.Drowning;
                            _drownedInLava = inLava;

                            // Deal damage when in lava.
                            _hitCount = inLava ? CooldownTime : 0;
                        }
                    }
                }
            }
            else if (CurrentState == State.Swimming && (!IsTransitioning || !Map.Is2dMap))
                CurrentState = State.Idle;

            if (CurrentState == State.Swimming)
            {
                EntityPosition.Z = 0;
                _body.IsGrounded = true;
            }
            _lastFieldState = _body.CurrentFieldState;
        }

        private void UpdateSwimmingPartTwo()
        {
            // Update drowning.
            if (CurrentState == State.Drowning)
            {
                if (Animation.CurrentFrameIndex < 2)
                {
                    _body.Velocity = Vector3.Zero;
                    EntityPosition.Set(new Vector2(
                        MathF.Round(EntityPosition.X), MathF.Round(EntityPosition.Y)));
                }
                if (Animation.CurrentFrameIndex == 2)
                {
                    IsVisible = false;
                    CurrentState = State.Drowned;
                    _drownResetCounter = 500;
                }
            }
            // Update drowned.
            else if (CurrentState == State.Drowned)
            {
                _drownResetCounter -= Game1.DeltaTime;
                if (_drownResetCounter <= 0)
                {
                    CurrentState = State.Idle;
                    CanWalk = true;
                    IsVisible = true;

                    _hitCount = CooldownTime;

                    if (_drownedInLava)
                    {
                        Game1.GameManager.CurrentHealth -= (int)MathF.Ceiling(2 * (GameSettings.DamageFactor * 0.25f));
                        _drownedInLava = false;
                    }
                    _body.CurrentFieldState = MapStates.FieldStates.None;
                    EntityPosition.Set(_drownResetPosition);
                }
            }
            // Update swimming.
            if (CurrentState == State.Swimming)
            {
                if (_diveCounter > -100)
                {
                    _diveCounter -= Game1.DeltaTime;

                    // Stop diving.
                    if (ControlHandler.ButtonPressed(ControlHandler.CancelButton))
                        _diveCounter = 0;
                }

                // Start diving.
                else if (ControlHandler.ButtonPressed(ControlHandler.CancelButton))
                    StartDiving(1500);

                if (_swimBoostCount > -300)
                    _swimBoostCount -= Game1.DeltaTime;

                else if (ControlHandler.ButtonPressed(ControlHandler.ConfirmButton))
                {
                    _swimBoostCount = 300;
                    Game1.GameManager.PlaySoundEffect("D360-15-0F");
                }

                if (_swimBoostCount > 0)
                    _moveVelocity *= SwimSpeedA;
                else
                    _moveVelocity *= SwimSpeed;

                var distance = _moveVelocity - _swimVelocity;
                var length = distance.Length();
                if (distance != Vector2.Zero)
                    distance.Normalize();

                if (length < 0.045f)
                    _swimVelocity = _moveVelocity;
                else
                    _swimVelocity += distance * (_swimBoostCount > 0 ? 0.06f : 0.045f) * Game1.TimeMultiplier;

                _moveVelocity = _swimVelocity;
            }
            else
            {
                _diveCounter = 0;
            }
        }

        private void StartDiving(int diveTime)
        {
            // splash effect
            var splashAnimator = new ObjAnimator(Map, 0, 0, 0, 0, Values.LayerTop, "Particles/splash", "idle", true);
            splashAnimator.EntityPosition.Set(new Vector2(
                _body.Position.X + _body.OffsetX + _body.Width / 2f,
                _body.Position.Y + _body.OffsetY + _body.Height - _body.Position.Z - 3));
            Map.Objects.SpawnObject(splashAnimator);

            Game1.GameManager.PlaySoundEffect("D360-14-0E");

            _diveCounter = diveTime;
        }

        private void UpdateDive()
        {
            _diveCounter -= Game1.DeltaTime;
        }

        private void UpdateDrownResetPosition()
        {
            // save the last position the player is grounded to use for the reset position if the player drowns
            if (!IsJumpingState() &&
                CurrentState != State.Drowning &&
                CurrentState != State.Drowned && _body.IsGrounded)
            {
                // center the position
                // can lead to the position being inside something
                var bodyCenter = new Vector2(EntityPosition.X, EntityPosition.Y - _body.Height / 2f);
                bodyCenter.X = (int)(bodyCenter.X / 16) * 16 + 8;
                bodyCenter.Y = (int)(bodyCenter.Y / 16) * 16 + 8 + _body.Height / 2f;

                // found new reset position?
                if (!Map.GetFieldState(bodyCenter).HasFlag(MapStates.FieldStates.DeepWater))
                {
                    var bodyBox = new Box(
                        bodyCenter.X + _body.OffsetX,
                        bodyCenter.Y + _body.OffsetY, 0, _body.Width, _body.Height, _body.Depth);
                    var cBox = Box.Empty;

                    // check it the player is not standing inside something
                    if (!Map.Objects.Collision(bodyBox, Box.Empty, _body.CollisionTypes | Values.CollisionTypes.DrownExclude, 0, 0, ref cBox))
                        _drownResetPosition = bodyCenter;
                }
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  ANIMATION / GRAPHICS CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void PlayWalkAndBlockAnimations(string shieldString, int animDirection, bool isBlocking)
        {
            // Default to standard walking animation.
            string targetAnimation = "walk" + shieldString + animDirection;

            // Check if the player is blocking.
            if (isBlocking)
            {
                // Set the blocking animation for either walking or standing.
                targetAnimation = _isWalking
                    ? "walkb" + shieldString + animDirection
                    : "standb" + shieldString + animDirection;
            }
            // If already playing the correct animation we can exit now.
            if (Animation.AnimationID == targetAnimation)
                return;

            // Retrieve the previous frame information.
            int prevFrame = Animation.CurrentFrameIndex;
            double prevTime = Animation.FrameCounter % Animation.CurrentFrame.FrameTime;

            // Play the new animation and pass on the current frame and duration.
            Animation.Play(targetAnimation, prevFrame, prevTime);
        }

        private void UpdateAnimation()
        {
            // If under the effects of a vacuum, use the rotational direction.
            var animDirection = _isRotating
                ? _rotateDirection
                : Direction;

            if (Game1.GameManager.UseShockEffect)
                return;

            // Include the shield in the animation string if available ("s_" for shield, "ms_" for mirror shield).
            string shieldString = CarryShield
                ? Game1.GameManager.ShieldLevel == 2 ? "ms_" : "s_"
                : "_";

            // Pegasus boots running animation.
            if (!IsTransitioning && (_bootsHolding || _bootsRunning || _forceWalking))
            {
                _swordChargeCounter = sword_charge_time;

                // Running in place charging, or run with the shield in front of the player.
                if (!_bootsRunning)
                    Animation.Play("walk" + shieldString + animDirection);
                else
                    Animation.Play((CarryShield ? "walkb" : "walk") + shieldString + animDirection);

                // Movement speed is doubled.
                if (!_forceWalking)
                    Animation.SpeedMultiplier = 2.0f;

                return;
            }
            // A backup path for when the above fails. If walking is forced, walking should happen.
            if (_forceWalking)
            {
                Animation.Play((CarryShield ? "walkb" : "walk") + shieldString + animDirection);
                return;
            }
            // When the rotation from a vacuum ends, the body and weapon animators need to be resynced.
            if ((IsChargingState() || _bootsRunning) && _wasRotating)
            {
                Direction = _rotateDirection;
                Animation.Play("stand" + shieldString + Direction);
                AnimatorWeapons.Play("stand_" + Direction);
            }
            _wasRotating = false;

            // Restore normal animation speed.
            Animation.SpeedMultiplier = 1.0f;

            // Play animation based on Link's current state and other factors.
            if (CurrentState == State.FinalStand)
                Animation.Play("final_stand");
            else if ((CurrentState == State.Idle && !_isWalking && _body.IsGrounded) ||
                (CurrentState == State.Charging && !_isWalking) ||
                (CurrentState == State.Rafting && !_isWalking) ||
                CurrentState == State.Teleporting ||
                CurrentState == State.ShowInstrumentPart3 ||
                CurrentState == State.TeleportFall ||
                CurrentState == State.TeleporterUp ||
                CurrentState == State.FallRotateEntry)
            {
                Animation.Play("stand" + shieldString + animDirection);
            }
            // The "jump-land" hack plays the "stand" animation briefly.
            else if (_jumpEndTimer > 0 &&
                !IsAttackingState() &&
                CurrentState != State.Dying &&
                CurrentState != State.ShowToadstool &&
                CurrentState != State.PickingUp &&
                CurrentState != State.Powdering &&
                CurrentState != State.Digging &&
                CurrentState != State.Bombing &&
                CurrentState != State.Hookshot &&
                CurrentState != State.Ocarina &&
                CurrentState != State.MagicRod &&
                CurrentState != State.BedTransition &&
                !(!IsChargingState() && (Direction == 1 || Direction == 3)) &&
                !IsTransitioning && !HoleFalling && !IsPoking)
            {
                Animation.Play("stand" + shieldString + animDirection);
            }
            else if (CurrentState == State.ChargeJumping)
                Animation.Play("cjump" + shieldString + animDirection);
            else if ((CurrentState == State.Idle || CurrentState == State.Charging || CurrentState == State.Rafting) && _isWalking)
                PlayWalkAndBlockAnimations(shieldString, animDirection, false);
            else if (CurrentState == State.Blocking || CurrentState == State.ChargeBlocking)
                PlayWalkAndBlockAnimations(shieldString, animDirection, true);
            else if ((CurrentState == State.Carrying || CurrentState == State.CarryingItem) && !_isFlying)
                Animation.Play((!_isWalking ? "standc_" : "walkc_") + animDirection);
            else if (IsFlying())
                Animation.Play("flying_" + animDirection);
            else if (CurrentState == State.Pushing)
                Animation.Play("push_" + animDirection);
            else if (CurrentState == State.Grabbing)
                Animation.Play("grab_" + animDirection);
            else if (CurrentState == State.Pulling)
                Animation.Play("pull_" + animDirection);
            else if (CurrentState == State.Swimming)
            {
                Animation.Play(_diveCounter > 0 ? "dive" : "swim_" + animDirection);
                if (_swimVelocity.Length() < 0.1 && !IsTransitioning)
                    Animation.IsPlaying = false;
            }
            else if (CurrentState == State.Drowning)
                Animation.Play("drown");

            // If anything forced walking, disable it now that the animation has played.
            if (!IsTransitioning)
                _isWalking = false;
        }

        private void UpdateDamageShader()
        {
            if (_hitCount > 0)
                _sprite.SpriteShader = (CooldownTime - _hitCount) % (BlinkTime * 2) < BlinkTime ? Resources.DamageSpriteShader0 : null;
            else
                _sprite.SpriteShader = null;
        }

        private void UpdateSpriteShadow()
        {
            // If shadows is disabled then draw a sprite shadow.
            if (!GameSettings.EnableShadows)
            {
                if (_spriteShadow == null)
                {
                    _spriteShadow = new ObjSpriteShadow(Map, this, Values.LayerPlayer, "sprshadowm");
                    Map.Objects.RegisterAlwaysAnimateObject(_spriteShadow);
                }
            }
            // Remove the sprite shadow if shadows was enabled.
            else if (_spriteShadow != null)
            {
                Map.Objects.RemoveObject(_spriteShadow);
                _spriteShadow = null;
            }
            // If the shadow is spawned but the map has changed.
            if (_spriteShadow != null && _spriteShadow.Map != Map)
            {
                // Remove the old sprite shadow.
                if (_spriteShadow.Map != null)
                    _spriteShadow.Map.Objects.RemoveObject(_spriteShadow);

                // Repawn the shadow.
                Map.Objects.SpawnObject(_spriteShadow);
                Map.Objects.RegisterAlwaysAnimateObject(_spriteShadow);
                _spriteShadow.Map = Map;
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  LOW HEARTS ALARM CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateHeartWarningSound()
        {
            // Don't play the beep if the user disabled it.
            if (!GameSettings.HeartBeep || !_enableHealthBeep) return;

            // Calculate the pecentage of heart's remaining.
            double currentHP = Game1.GameManager.CurrentHealth;
            double maximumHP = Game1.GameManager.MaxHearts * 4;

            // Play the beep if health is below 20 percent.
            if (currentHP / maximumHP < 0.20)
            {
                _lowHealthBeepCounter += Game1.DeltaTime;

                if (_lowHealthBeepCounter > 825)
                {
                    _lowHealthBeepCounter = 0;
                    Game1.GameManager.PlaySoundEffect("D370-04-04");
                }
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  FIELD / FIELD BARRIER CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void CreateFieldBarrier()
        {
            // Create the field barrier colliders.
            FieldBarrier = new ObjFieldBarrier[4];
            FieldBarrier[0] = new ObjFieldBarrier(Map, CurrentField.X - 16, CurrentField.Y - 16, Values.CollisionTypes.Field, new Rectangle(0, 0, 192, 16));
            FieldBarrier[1] = new ObjFieldBarrier(Map, CurrentField.X - 16, CurrentField.Y + 128, Values.CollisionTypes.Field, new Rectangle(0, 0, 192, 16));
            FieldBarrier[2] = new ObjFieldBarrier(Map, CurrentField.X - 16, CurrentField.Y, Values.CollisionTypes.Field, new Rectangle(0, 0, 16, 128));
            FieldBarrier[3] = new ObjFieldBarrier(Map, CurrentField.X + 160, CurrentField.Y, Values.CollisionTypes.Field, new Rectangle(0, 0, 16, 128));

            // Spawn in the field barrier colliders.
            Map.Objects.SpawnObject(FieldBarrier[0]);
            Map.Objects.SpawnObject(FieldBarrier[1]);
            Map.Objects.SpawnObject(FieldBarrier[2]);
            Map.Objects.SpawnObject(FieldBarrier[3]);
        }

        private void UpdateFieldBarrier()
        {
            // Don't update unless the field has changed.
            if (CurrentField == ContrastField) return;

            // Spawn in the field barrier rectangles.
            FieldBarrier[0].SetPosition(CurrentField.X - 16, CurrentField.Y - 16);
            FieldBarrier[1].SetPosition(CurrentField.X - 16, CurrentField.Y + 128);
            FieldBarrier[2].SetPosition(CurrentField.X - 16, CurrentField.Y);
            FieldBarrier[3].SetPosition(CurrentField.X + 160, CurrentField.Y);
        }

        private void DestroyFieldBarrier()
        {
            // Nobody likes crashes so verify it's null.
            if (FieldBarrier == null) return;

            // Destroy the current field barrier and nullify it.
            foreach (var fBarrier in FieldBarrier)
                Map.Objects.RemoveObject(fBarrier);

            PreventReset = false;
            FieldBarrier = null;
        }

        private void UpdateCurrentField()
        {
            // Set the current field that Link is on.
            CurrentField = Map.GetField(CenterPosition.Position);

            // We only use the field barrier when "Classic Camera" is active.
            if (Camera.ClassicMode)
            {
                // Detect when the field has changed.
                FieldChange = CurrentField != ContrastField;

                // Store the previous field that was just left.
                if (FieldChange)
                    PreviousField = ContrastField;

                // Check to see if the current field has not yet been set. When a game is started,
                // the first few frames will return (0,0) for the current field position.
                if (new Vector2(CurrentField.X, CurrentField.Y) != Vector2.Zero)
                {
                    // Create the barrier if null or update if it exists.
                    if (FieldBarrier == null)
                        CreateFieldBarrier();
                    else
                        UpdateFieldBarrier();
                }
                // Prevent resetting enemies shortly after map transitions.
                if (PreventResetTimer > 0)
                {
                    PreventResetTimer -= Game1.DeltaTime;
                    if (PreventResetTimer < 0)
                        PreventReset = false;
                }
            }
            // Destroy the barrier if "Classic Camera" is not active.
            else
                DestroyFieldBarrier();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  NPC AVOIDANCE CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateNPCAvoidance()
        {
            // Check if sword hitbox is within NPC hitbox.
            _npcSwordCross = CheckNPCAvoidance();

            // Tracks how far the player traveled when triggering avoidance.
            float travelDistance = 0;

            // Update the distance if avoidance took place.
            if (_npcCrossSword)
                travelDistance = Vector2.Distance(_avoidanceStartPos, EntityPosition.Position);

            // When sword is no longer within NPC hitbox and Link is holding sword, restore charging state.
            if (!_npcSwordCross && _isHoldingSword && _npcCrossSword && (_avoidanceDirection != Direction || travelDistance > 22))
            {
                _npcCrossSword = false;
                Animation.Play("stand" + Direction);
                AnimatorWeapons.Play("stand_" + Direction);
                CurrentState = State.Charging;
                _swordChargeCounter = sword_charge_time;
                _isHoldingSword = false;
            }
            // This probably isn't the best place for this but it's where it logically needs to happen.
            WasHoleReset = false;
        }

        private bool CheckNPCAvoidance()
        {
            // Get a list of NPCs to check if sword crosses their hitbox.
            List<GameObject> npcList = new List<GameObject>();

            Map.Objects.GetComponentList(npcList,
                (int)SwordDamageBox.X, (int)SwordDamageBox.Y,
                (int)SwordDamageBox.Width, (int)SwordDamageBox.Height,
                CollisionComponent.Mask);

            // Loop through the NPCs checking for collision.
            foreach (var npc in npcList)
            {
                if (npc.IsActive)
                {
                    var collisionObject = npc.Components[CollisionComponent.Index] as CollisionComponent;
                    var collisionBody = npc.Components[CollisionComponent.Index] as BodyCollisionComponent;
                    if (collisionObject != null && collisionBody != null && collisionBody.IsActive &&
                        (collisionObject.CollisionType & Values.CollisionTypes.NPC) != 0)
                    {
                        // If the sword box and body box intersect return true.
                        var bodyObject = npc.Components[BodyComponent.Index] as BodyComponent;
                        if (bodyObject != null && SwordDamageBox.Intersects(bodyObject.BodyBox.Box))
                            return true;
                    }
                }
            }
            return false;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  DREAM SHRINE CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public void StartBedTransition() => _startBedTransition = true;

        private void UpdateBedTransition()
        {
            if (_startBedTransition && CurrentState == State.Idle)
            {
                CurrentState = State.BedTransition;
                _startBedTransition = false;
                Animation.Play("bed");
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  INSTRUMENTS CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateInstrumentSequence()
        {
            // We need to prevent overlays from being opened because they
            // do not stop the music and it would run out of sync.
            if ((ShowItem != null && ShowItem.Name.StartsWith("instrument")) || IsShowingInstrument())
                Game1.GameManager.InGameOverlay.DisableInventoryToggle = true;

            if (CurrentState == State.ShowInstrumentPart0)
            {
                // is the sound effect still playing?
                if (_instrumentPickupTime + 7500 < Game1.TotalGameTime)
                {
                    Game1.GameManager.SetMusic(_instrumentMusicIndex[_instrumentIndex], 2);
                    Game1.GbsPlayer.Play();
                    Game1.GbsPlayer.SoundGenerator.SetStopTime(8);
                    CurrentState = State.ShowInstrumentPart1;
                }
            }
            else if (CurrentState == State.ShowInstrumentPart1)
            {
                _instrumentCounter += Game1.DeltaTime;

                if (_instrumentCounter > 3500)
                {
                    _drawInstrumentEffect = true;
                    Game1.GameManager.PlaySoundEffect("D360-43-2B", false);
                }
                if (_instrumentCounter > 8000)
                {
                    Game1.GameManager.SetMusic(-1, 0);
                    Game1.GameManager.SetMusic(-1, 2);
                    Game1.GameManager.PlaySoundEffect("D378-44-2C");

                    _instrumentCounter = 0;
                    CurrentState = State.ShowInstrumentPart2;
                }
            }
            else if (CurrentState == State.ShowInstrumentPart2)
            {
                // Some update caused music to continue playing after instrument screen goes white so don't let this happen. 
                Game1.GameManager.StopMusic(true);

                _instrumentCounter += Game1.DeltaTime;
                var transitionSystem = (MapTransitionSystem)Game1.GameManager.GameSystems[typeof(MapTransitionSystem)];
                transitionSystem.ResetTransition();
                transitionSystem.SetColorMode(Color.White, MathHelper.Clamp(_instrumentCounter / 500f, 0, 1));

                if (_instrumentCounter > 2500)
                {
                    Direction = 3;
                    UpdateAnimation();

                    CurrentState = State.ShowInstrumentPart3;
                    ShowItem = null;
                    _drawInstrumentEffect = false;

                    Game1.GameManager.StartDialogPath($"instrument{_instrumentIndex}Collected");
                }
            }
            else if (CurrentState == State.ShowInstrumentPart3)
            {
                MapTransitionStart = EntityPosition.Position;
                MapTransitionEnd = MapTransitionStart;
                TransitionOutWalking = false;

                EndPickup();

                ((MapTransitionSystem)Game1.GameManager.GameSystems[typeof(MapTransitionSystem)]).AppendMapChange("overworld.map", $"d{_instrumentIndex+1}Finished", false, true, Color.White, true);

                // Set the key-value pair to open the instrument door.
                var openDoor = $"d{_instrumentIndex+1}_cleared";
                Game1.GameManager.SaveManager.SetString(openDoor, "1");
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  HOLE CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateSavePosition()
        {
            Vector2 bodyCenter = _body.BodyBox.Box.Center;
            Vector3 newResetPosition = _holeResetPosition;
            Point currentTilePosition = new Point(((int)bodyCenter.X - Map.MapOffsetX * 16) / 160, ((int)bodyCenter.Y - Map.MapOffsetY * 16) / 128);
            Point tileDiff = currentTilePosition - _lastTilePosition;
            _lastTilePosition = currentTilePosition;

            // update position?
            if (tileDiff != Point.Zero)
            {
                var tileSize = 16;

                // Zero out the alternative reset position.
                _altHoleResetPosition = Vector3.Zero;

                // For X and Y check if the room has changed since last check.
                newResetPosition.X = (tileDiff.X == 0)
                    ? EntityPosition.X
                    : (int)(bodyCenter.X / tileSize + (tileDiff.X > 0 ? 0 : 1)) * tileSize;

                newResetPosition.Y = (tileDiff.Y == 0)
                    ? EntityPosition.Y
                    : (int)(bodyCenter.Y / tileSize + (tileDiff.Y > 0 ? 0 : 1)) * tileSize;

                // Add buffer to push player inward into the field. The direction determines the size of the pixel buffer
                // due to the fact that Link's body box is not perfectly centered on his sprite and has a downward bias.
                if (tileDiff.X > 0) newResetPosition.X += 8;  // Came from left → push right
                if (tileDiff.X < 0) newResetPosition.X -= 8;  // Came from right → push left
                if (tileDiff.Y > 0) newResetPosition.Y += 16; // Came from top → push down
                if (tileDiff.Y < 0) newResetPosition.Y -= 2;  // Came from bottom → push up

                // For Z check if jumping. If on ground set Z to current Z but if in air set Z to what it was before jump.
                newResetPosition.Z = _body.IsGrounded
                    ? EntityPosition.Z
                    : _jumpStartZPos;

                newResetPosition.Z = _isFlying
                    ? _flyStartZPos
                    : newResetPosition.Z;

                // Check if there is no hole at the new position.
                var bodyBox = new Box(newResetPosition.X + _body.BodyBox.OffsetX, newResetPosition.Y + _body.BodyBox.OffsetY, 0, _body.Width, _body.Height, 8);
                var outBox = Box.Empty;

                if (!Map.Objects.Collision(bodyBox, Box.Empty, Values.CollisionTypes.Hole, 0, 0, ref outBox))
                {
                    _holeResetPosition = newResetPosition;
                }
            }
        }

        private void UpdateFallingIntoHole()
        {
            // change the room?
            if (_isFallingIntoHole)
            {
                _holeFallCounter -= Game1.DeltaTime;

                if (_holeFallCounter <= 0)
                {
                    _isFallingIntoHole = false;

                    if (HoleResetRoom != null)
                    {
                        // append a map change
                        ((MapTransitionSystem)Game1.GameManager.GameSystems[
                            typeof(MapTransitionSystem)]).AppendMapChange(HoleResetRoom, HoleResetEntryId);
                    }
                    // teleport on hole fall?
                    else if (HoleTeleporterId >= 0)
                    {
                        _holeTeleportCounter = 0;
                        CurrentState = State.TeleporterUpWait;
                    }
                }
            }
            HoleTeleporterId = -1;

            // finished falling down the hole?
            if (CurrentState == State.Falling && !Animation.IsPlaying)
                OnHoleReset();
        }

        private void SetHoleResetPosition(Vector3 position)
        {
            // Sets hole reset position on map initialization.
            _holeResetPosition = position;

            var offset = Map != null ? new Point(Map.MapOffsetX, Map.MapOffsetY) : Point.Zero;
            _lastTilePosition = new Point(((int)position.X - offset.X * 16) / 160, ((int)position.Y - offset.Y * 16) / 128);
        }

        public void SetHoleResetPosition(Vector3 position, int direction)
        {
            // If Link jumped when setting the hole reset point then use the Z value before the jump started.
            float positionZ = _body.IsGrounded ? position.Z : _jumpStartZPos;

            // Sets an "alternate" reset point when walking over a "ObjHoleResetPoint".
            _altHoleResetPosition = direction switch
            {
                0 => new Vector3(position.X + MathF.Ceiling(_body.Width / 2f), position.Y + 8 + MathF.Ceiling(_body.Height / 2f), positionZ),
                1 => new Vector3(position.X + 8, position.Y + _body.Height + 1, positionZ),
                2 => new Vector3(position.X + 16 - MathF.Ceiling(_body.Width / 2f), position.Y + 8 + MathF.Ceiling(_body.Height / 2f), positionZ),
                3 => new Vector3(position.X + 8, position.Y + 16, positionZ),
                _ => Vector3.Zero
            };
            // Also used for the drown reset point. Instead of opening up the can of worms of converting the drown 
            // reset point to a Vector3 just use the X and Y coordinates from the _altHoleResetPosition.
            _drownResetPosition = new Vector2(_altHoleResetPosition.X, _altHoleResetPosition.Y);
        }

        private void OnHolePull(Vector2 direction, float percentage)
        {
            if (percentage >= 0.55f)
                _canJump = false;
        }

        private void OnHoleAbsorb()
        {
            if (CurrentState == State.Falling ||
                CurrentState == State.TeleporterUpWait ||
                CurrentState == State.TeleporterUp ||
                CurrentState == State.PickingUp ||
                CurrentState == State.Dying)
                return;

            CurrentState = State.Falling;
            HoleFalling = true;

            FreeTrappedPlayer();
            ReleaseCarriedObject();

            _railJump = false;
            _isFallingIntoHole = true;
            _holeFallCounter = 350;

            Animation.Play("fall");
            Game1.GameManager.PlaySoundEffect("D370-12-0C");
        }

        private void OnHoleReset()
        {
            // change the room?
            if (HoleResetRoom != null)
                return;

            _isFallingIntoHole = false;

            CurrentState = State.Idle;
            CanWalk = true;

            _hitCount = CooldownTime;
            Game1.GameManager.InflictDamage(2);

            MoveToHoleResetPosition();
        }

        private void MoveToHoleResetPosition()
        {
            // Create the respawn point and move Link to it.
            Vector3 resetPosition = _holeResetPosition;
            WasHoleReset = true;
            EntityPosition.Set(resetPosition);

            // Alternative reset point.
            var cBox = Box.Empty;
            if (_altHoleResetPosition != Vector3.Zero &&
                Map.Objects.Collision(_body.BodyBox.Box, Box.Empty, _body.CollisionTypes, 0, 0, ref cBox))
            {
                resetPosition = _altHoleResetPosition;
                EntityPosition.Set(resetPosition);
            }
            HoleFalling = false;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  RAFT CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public void StartRaftRiding(ObjRaft objRaft)
        {
            // Throw the rooster if using it.
            if (MapManager.ObjLink.IsFlying())
                MapManager.ObjLink.ReleaseCarriedObject();

            if (!IsJumpingState())
                CurrentState = State.Rafting;

            _isRafting = true;
            _objRaft = objRaft;
            _body.VelocityTarget = Vector2.Zero;
            _body.IgnoreHeight = true;
        }

        private void UpdateRafting()
        {
            if (_isRafting && (CurrentState == State.Rafting || CurrentState == State.Charging || CurrentState == State.ChargeBlocking))
            {
                var moveVelocity = ControlHandler.GetMoveVector2();

                var moveVelocityLength = moveVelocity.Length();
                if (moveVelocityLength > 1)
                    moveVelocity.Normalize();

                if (moveVelocityLength > 0)
                {
                    _isWalking = true;
                    _objRaft.TargetVelocity(moveVelocity * 0.5f);

                    if (CurrentState != State.Charging && CurrentState != State.ChargeBlocking)
                    {
                        var vectorDirection = ToDirection(moveVelocity);
                        Direction = vectorDirection;
                    }
                }
            }
        }

        public void RaftJump(Vector2 targetPosition)
        {
            if (!_isRafting)
                return;

            if (IsJumpingState())
                return;

            CurrentState = State.Jumping;

            Game1.GameManager.PlaySoundEffect("D360-13-0D");

            Direction = 3;
            Animation.Play("jump_" + Direction);

            if (_objRaft != null)
                _objRaft.Jump(targetPosition, 100);
        }

        private void StopRaft()
        {
            if (_isRafting)
            {
                _objRaft.Body.VelocityTarget = Vector2.Zero;
                _objRaft.Body.AdditionalMovementVT = Vector2.Zero;
                _objRaft.Body.LastAdditionalMovementVT = Vector2.Zero;
            }
        }

        public void ExitRaft()
        {
            CurrentState = State.Idle;

            _isRafting = false;
            _objRaft = null;

            EntityPosition.Set(new Vector2(EntityPosition.X, EntityPosition.Y - 1));
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  ITEM CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateItem()
        {
            // Track when the sword is spinning or not spinning.
            var swordSpinning = AnimatorWeapons.AnimationID?.StartsWith("swing") == true;
            var spinIsPlaying = AnimatorWeapons.IsPlaying;
            _isSwordSpinning = swordSpinning && spinIsPlaying;

            // When no longer blocking return to idle state.
            if (CurrentState == State.Blocking)
                ReturnToIdle();
            else
                _wasBlocking = false;

            // When grabbing or pulling return to idle state.
            if (CurrentState == State.Grabbing || CurrentState == State.Pulling)
                ReturnToIdle();

            _isPulling = false;
            _isHoldingSword = false;
            _bootsHolding = false;

            if (!_isLocked)
            {
                // interact with object
                if ((CurrentState == State.Idle || CurrentState == State.Pushing || CurrentState == State.Swimming || CurrentState == State.CarryingItem) &&
                    ControlHandler.ButtonPressed(ControlHandler.ConfirmButton) && InteractWithObject())
                {
                    InputHandler.ResetInputState();
                    return;
                }
                if (_isTrapped && !_trappedDisableItems &&
                    (ControlHandler.ButtonPressed(CButtons.A) ||
                     ControlHandler.ButtonPressed(CButtons.B) ||
                     ControlHandler.ButtonPressed(CButtons.X) ||
                     ControlHandler.ButtonPressed(CButtons.Y)))
                {
                    _trapInteractionCount--;
                    if (_trapInteractionCount <= 0)
                        FreeTrappedPlayer();
                }

                // use/hold/release item
                if (!DisableItems && (!_isTrapped || !_trappedDisableItems))
                {
                    // HACK FIX: This fixes a crash if an item is used immediately after loading a save file.
                    if (Direction < 0) { Direction = 0; }

                    for (var i = 0; i < Values.HandItemSlots; i++)
                    {
                        if (Game1.GameManager.Equipment[i] != null &&
                            ControlHandler.ButtonPressed((CButtons)((int)CButtons.A * Math.Pow(2, i))))
                            UseItem(Game1.GameManager.Equipment[i]);

                        if (Game1.GameManager.Equipment[i] != null &&
                            ControlHandler.ButtonDown((CButtons)((int)CButtons.A * Math.Pow(2, i))))
                            HoldItem(Game1.GameManager.Equipment[i]);

                        if (Game1.GameManager.Equipment[i] != null &&
                            ControlHandler.ButtonReleased((CButtons)((int)CButtons.A * Math.Pow(2, i))))
                            ReleaseItem(Game1.GameManager.Equipment[i]);
                    }
                }
                // If an "instant pickup" object was grabbed, force power bracelent pick up until the loop ends.
                if (_instantPickup) { HoldBracelet(); }

            }
            UpdatePegasusBoots();

            // shield pushing
            if (IsBlockingState() || _bootsRunning && CarryShield)
                UpdateShieldPush();

            // pick up animation
            if (CurrentState == State.PreCarrying)
            {
                _preCarryCounter += Game1.DeltaTime;

                // change the animation of the player depending on where the picked up object is
                if (_preCarryCounter > 100)
                    Animation.Play("standc_" + Direction);

                UpdatePositionCarriedObject(EntityPosition);
            }

            // stop attacking
            if (IsAttackingState() && !Animation.IsPlaying)
            {
                _isSwordSpinAttack = false;

                if (_isSwordSpinning)
                {
                    Vector2 vecMoved = ControlHandler.GetMoveVector2();
                    Direction = ToDirection(vecMoved);
                }

                if (!_isHoldingSword || _swordPoked || _stopCharging)
                    ReturnToIdle();
                else
                {
                    // If in another state when charge begins set a dual charging state.
                    CurrentState = CurrentState switch
                    {
                        State.Blocking => State.ChargeBlocking,
                        State.AttackBlocking => State.ChargeBlocking,
                        State.Jumping => State.ChargeJumping,
                        State.AttackJumping => State.ChargeJumping,
                        State.AttackSwimming => State.ChargeSwimming,
                        _ => State.Charging
                    };
                    // Play animation and add to charge counter.
                    AnimatorWeapons.Play("stand_" + Direction);
                    _swordPokeCounter = _swordPokeTime;
                }
            }
            if (IsChargingState())
                UpdateCharging();

            if (IsAttackingState() || CurrentState == State.SwordShow1 || _bootsRunning && CarrySword)
                UpdateAttacking();

            if (!IsShowingInstrument() && !IsShowingCloak() && CurrentState != State.SwordShowLv2)
                UpdatePickup();

            if (!Animation.IsPlaying && (CurrentState == State.Powdering || CurrentState == State.Bombing || CurrentState == State.MagicRod || CurrentState == State.Throwing))
                ReturnToIdle();

            UpdateHookshot();

            if (CurrentState == State.Digging)
                UpdateDigging();

            _wasPulling = _isPulling;
        }

        private bool InteractWithObject()
        {
            var boxSize = 6;
            var interactionBox = new Box(
                EntityPosition.X + _walkDirection[Direction].X * (BodyRectangle.Width / 2 + boxSize / 2) - boxSize / 2,
                BodyRectangle.Center.Y + _walkDirection[Direction].Y * (BodyRectangle.Height / 2 + boxSize / 2) - boxSize / 2, 0,
                boxSize, boxSize, 16);

            return Map.Objects.InteractWithObject(interactionBox);
        }

        private void UseItem(GameItemCollected item)
        {
            Action useItem = item.Name switch
            {
                "sword1"        => UseSword,
                "sword2"        => UseSword,
                "feather"       => UseFeather,
                "toadstool"     => UseToadstool,
                "powder"        => UsePowder,
                "bomb"          => UseBomb,
                "bow"           => UseArrow,
                "shovel"        => UseShovel,
                "stonelifter"   => UseBracelet,
                "stonelifter2"  => UseBracelet,
                "hookshot"      => UseHookshot,
                "boomerang"     => UseBoomerang,
                "magicRod"      => UseMagicRod,
                "ocarina"       => UseOcarina,
                "pegasusBoots"  => UsePegasusBoots,
                _               => null
            };
            useItem?.Invoke();
        }

        private void HoldItem(GameItemCollected item)
        {
            Action holdItem = item.Name switch
            {
                "sword1"        => HoldSword,
                "sword2"        => HoldSword,
                "feather"       => HoldFeather,
                "shield"        => HoldShield,
                "mirrorShield"  => HoldShield,
                "stonelifter"   => HoldBracelet,
                "stonelifter2"  => HoldBracelet,
                "pegasusBoots"  => HoldPegasusBoots,
                _               => null
            };
            holdItem?.Invoke();
        }

        private void ReleaseItem(GameItemCollected item)
        {
            Action releaseItem = item.Name switch
            {
                "shield"        => ReleaseShield,
                "mirrorShield"  => ReleaseShield,
                "feather"       => ReleaseFeather,
                _               => null
            };
            releaseItem?.Invoke();
        }

        public void PickUpItem(GameItemCollected itemCollected, bool showItem, bool showDialog = true, bool playSound = true)
        {
            if (itemCollected == null)
                return;

            var item = Game1.GameManager.ItemManager[itemCollected.Name];

            // the base item has the max count and other information
            var baseItem = Game1.GameManager.ItemManager[item.Name];

            // Save the game before entering the show animation to support exiting the game while the item is shown.
            _savedPreItemPickup = true;
            if (item.PickUpDialog != null && !Game1.GameManager.SaveManager.HistoryEnabled)
            {
                SaveGameSaveLoad.FillSaveState(Game1.GameManager);
                Game1.GameManager.SaveManager.EnableHistory();
            }
            _showItem = false;
            _pickingUpInstrument = false;
            _pickingUpSword = false;
            _pickingUpAnglerKey = false;

            // Used to replace sword, shield, or power bracelet with a higher level.
            var equipmentPosition = 0;

            // Picking up the Sword off the beach.
            if (item.Name == "sword1")
            {
                // The variable below freezes the world around Link and disables the inventory.
                _pickingUpSword = true;
                Game1.GameManager.SetMusic(14, 2);

                // Freeze the game. The "sword1Collected:0" event in "scripts.zScript" will unfreeze after a time.
                FreezeAnimations(true);
            }

            // Level 2 Sword was collected from Seashell Mansion.
            else if (item.Name == "sword2")
            {
                equipmentPosition = Game1.GameManager.GetEquipmentSlot("sword1");
                Game1.GameManager.RemoveItem("sword1", 99);
                Game1.GameManager.CollectItem(itemCollected, equipmentPosition);
                Game1.GameManager.SetMusic(14, 2);
            }

            // The Angler Key (level 3 dungeon) was collected.
            else if (item.Name == "dkey3")
            {
                // Don't temporarily freeze the desert quicksand when picking up the key.
                _pickingUpAnglerKey = true;
            }
            
            // A Shield was collected.
            else if (baseItem.Name == "shield")
            {
                // I'm honestly not sure what is supposed to be happening here.
                var mirrorShield = Game1.GameManager.GetItem("mirrorShield");
                if (mirrorShield != null)
                {
                    Game1.GameManager.PlaySoundEffect(item.SoundEffectName, true, 1, 0, item.TurnDownMusic);
                    return;
                }
            }

            // The Mirror Shield was collected.
            else if (item.Name == "mirrorShield")
            {
                // Replace the shield with the mirror shield.
                equipmentPosition = Game1.GameManager.GetEquipmentSlot("shield");
                Game1.GameManager.RemoveItem("shield", 99);
                Game1.GameManager.CollectItem(itemCollected, equipmentPosition);
            }

            // The level 2 Power Bracelet was collected.
            else if (itemCollected.Name == "stonelifter2")
            {
                equipmentPosition = Game1.GameManager.GetEquipmentSlot("stonelifter");
                Game1.GameManager.RemoveItem("stonelifter", 99);
                Game1.GameManager.CollectItem(itemCollected, equipmentPosition);
            }

            // A Piece of Heart was collected.
            else if (itemCollected.Name == "heartMeter")
            {
                // Check if a full heart container is finished and start "heartMeterFilled" path in "script.zScript". 
                var heart = Game1.GameManager.GetItem("heartMeter");
                if (heart?.Count == 3 && !GameSettings.NoHelperText)
                    _additionalPickupDialog = "heartMeterFilled";
            }

            // A full Heart Container was collected.
            else if (itemCollected.Name == "heartMeterFull")
            {
                // Interestingly, this starts track 36 which is the heart container pickup sound + the "dungeon cleared" music that follows. But the only part of this
                // that is played is the heart pickup sound. The music is started more quickly in "script.zScript" where it plays track 23 which is the dungeon cleared
                // music without the sound effect. This allows the music to play more quickly after the heart pickup, as there is a delay after the sound in track 36.
                Game1.GameManager.SetMusic(36, 2);
            }

            // A Seashell present at the mansion was collected.
            else if (itemCollected.Name == "shellPresent")
            {
                // Get the number of shell presents the player has collected.
                var currentShellPresents = Game1.GameManager.SaveManager.GetString("shell_presents", "0");

                // Add to the total number of shell presents that have been collected so far. These are tracked for "Nothing is Missable" options so that the player can
                // collect the shell presents even if the number of shells exceeds the number required at Seashell Mansion (for example: 7 shells spawns 5 shell present).
                if (int.TryParse(currentShellPresents, out int shellPresents))
                {
                    shellPresents++;
                    Game1.GameManager.SaveManager.SetString("shell_presents", shellPresents.ToString());
                }
            }

            // A Heart was collected.
            if (item.Name == "heart")
            {
                // Play the healing sound effect if HP is lower than current max.
                if (Game1.GameManager.CurrentHealth < Game1.GameManager.MaxHearts * 4)
                    Game1.GameManager.PlaySoundEffect("D370-06-06");

                // Add 4 hit points to current health.
                Game1.GameManager.CurrentHealth += itemCollected.Count * 4;

                // If the amount of healing exceeds max health then correct it to max.
                if (Game1.GameManager.CurrentHealth > Game1.GameManager.MaxHearts * 4)
                    Game1.GameManager.CurrentHealth = Game1.GameManager.MaxHearts * 4;
            }

            // The item picked up is an accessory.
            else if ((item.ShowAnimation == 1 || item.ShowAnimation == 2) && showItem)
            {
                // Reset the block button sound effect.
                _blockButton = false;

                // Stop all player movement.
                _moveVelocity = Vector2.Zero;
                _hitVelocity = Vector2.Zero;
                _repelVelocity = Vector2.Zero;
                _shieldVelocity = Vector2.Zero;
                _knockBackVelocity = Vector2.Zero;
                _body.Velocity = Vector3.Zero;
                _body.VelocityTarget = Vector2.Zero;

                // Sets the item Link holds over his head.
                ShowItem = item;

                // Hold the item over the head with one or two hands (to the left side or the middle).
                _showItemOffset.X = item.ShowAnimation == 1 ? 0 : -4;
                _showItemOffset.Y = -15;

                // Despawn boomerang or hookshot if it collected an item that is shown.
                if (ShowItem.Name != null)
                {
                    if (Hookshot != null)
                        Hookshot.Despawn();
                    if (Boomerang != null)
                        Boomerang.Despawn();
                }
                // Initialize the powerup state if it's a powerup item.
                if (ShowItem.Name == "guardianAcorn")
                    Game1.GameManager.InitGuardianAcorn();
                else if (ShowItem.Name == "pieceOfPower")
                    Game1.GameManager.InitPieceOfPower();

                // Show Link holding the sword when picking up a Piece of Power.
                if (ShowItem.Name == "pieceOfPower")
                {
                    var swordItem = Game1.GameManager.GetItem("sword1");
                    if (swordItem != null && swordItem.Count > 0)
                        ShowItem = Game1.GameManager.ItemManager["sword1PoP"];
                    else
                        ShowItem = Game1.GameManager.ItemManager["sword2PoP"];
                }

                // make sure to use the right source rectangle if the shown item does not have one
                var sourceRectangle = ShowItem.SourceRectangle ?? baseItem.SourceRectangle.Value;
                if (ShowItem.MapSprite != null)
                    sourceRectangle = ShowItem.MapSprite.SourceRectangle;
                else if (baseItem.MapSprite != null)
                    sourceRectangle = baseItem.MapSprite.SourceRectangle;

                // spawn pickup animation
                if (item.ShowEffect)
                    Map.Objects.SpawnObject(new ObjPickupAnimation(Map,
                        EntityPosition.X + _showItemOffset.X, EntityPosition.Y - EntityPosition.Z + _showItemOffset.Y - sourceRectangle.Height / 2));

                _showItemOffset -= new Vector2(sourceRectangle.Width / 2f, sourceRectangle.Height);

                CurrentState = State.PickingUp;
                Game1.GameManager.SaveManager.SetString("player_shows_item", "1");
                Animation.Play("show" + item.ShowAnimation);
                _itemShowCounter = item.ShowTime;
                _showItem = true;

                // make sure to collect the item the player is currently showing
                if (_collectedShowItem != null)
                    Game1.GameManager.CollectItem(_collectedShowItem, 0);

                _collectedShowItem = itemCollected;

                if (ShowItem.Name == "sword2")
                {
                    _jumpEndTimer = 0;
                    _shownSwordLv2Dialog = false;
                    _showSwordL2ParticleCounter = 0;
                    CurrentState = State.SwordShowLv2;
                }
                // not sure if this is what should happen here
                ReleaseCarriedObject();
            }
            else
            {
                Game1.GameManager.CollectItem(itemCollected, equipmentPosition);
            }

            if (item.Name.StartsWith("instrument"))
            {
                // stop playing music
                Game1.GameManager.SetMusic(26, 2);

                _instrumentPickupTime = Game1.TotalGameTime;

                _instrumentIndex = int.Parse(item.Name.Replace("instrument", ""));
                _pickingUpInstrument = true;
            }

            if (item.PickUpDialog != null && !_showItem && showDialog)
            {
                Game1.GameManager.StartDialogPath(item.PickUpDialog);
            }

            // play sound
            if (playSound && item.SoundEffectName != null)
                Game1.GameManager.PlaySoundEffect(item.SoundEffectName, true, 1, 0, item.TurnDownMusic);
            if (item.MusicName >= 0)
                Game1.GameManager.SetMusic(item.MusicName, 1);
        }

        private void UpdatePickup()
        {
            // If the item is null then do nothing here.
            if (ShowItem == null)
                return;

            // Disable the inventory while showing the item.
            Game1.GameManager.InGameOverlay.DisableInventoryToggle = true;

            // Decrement the show counter.
            _itemShowCounter -= Game1.DeltaTime;

            // When the counter has finished ticking.
            if (_itemShowCounter > 0)
                return;

            // If the item is to be held over Link's head.
            if (_showItem)
            {
                // We only want this section to run once.
                _showItem = false;

                // Show the pickup dialog if the item has one.
                if (ShowItem.PickUpDialog != null)
                {
                    // Check for override text before the normal text.
                    if (string.IsNullOrEmpty(_pickupDialogOverride))
                        Game1.GameManager.StartDialogPath(ShowItem.PickUpDialog);
                    else
                    {
                        Game1.GameManager.StartDialogPath(_pickupDialogOverride);
                        _pickupDialogOverride = null;
                    }
                    // Check for a second message to show after the first.
                    if (!string.IsNullOrEmpty(_additionalPickupDialog))
                    {
                        Game1.GameManager.StartDialogPath(_additionalPickupDialog);
                        _additionalPickupDialog = null;
                    }
                }
                // Additional time after the dialog.
                if (ShowItem.Name == "sword1")
                {
                    _itemShowCounter = 5650;
                    CurrentState = State.SwordShow0;
                }
                else if (ShowItem.Name.StartsWith("instrument"))
                    _itemShowCounter = 1000;
                else
                    _itemShowCounter = 250;
            }
            else
            {
                // Used in "script.zScript" when learning ocarina songs or obtaining the pineapple.
                Game1.GameManager.SaveManager.SetString("player_shows_item", "0");

                // Add the item to the player's inventory.
                if (_collectedShowItem != null)
                {
                    Game1.GameManager.CollectItem(_collectedShowItem, 0);
                    _collectedShowItem = null;
                }
                // If item was a follower then spawn them into the world.
                UpdateFollower(false);

                // Spin the sword after picking it up off the beach.
                if (ShowItem.Name == "sword1")
                {
                    Game1.GameManager.PlaySoundEffect("D378-03-03");
                    Animation.Play("swing_3");
                    AnimatorWeapons.Play("swing_3");
                    CurrentState = State.SwordShow1;
                    _swordChargeCounter = 1;
                    ShowItem = null;
                }
                // If it's an instrument stop powerup music and set vars for instrument sequence.
                else if (ShowItem.Name.StartsWith("instrument"))
                {
                    Game1.GameManager.StopPieceOfPower();
                    Game1.GameManager.StopGuardianAcorn();

                    _itemShowCounter = 0;
                    _instrumentCounter = 0;
                    CurrentState = State.ShowInstrumentPart0;
                }
                // After everything is done clear the item and return to idle state.
                else
                {
                    ShowItem = null;
                    if (CurrentState == State.PickingUp)
                        ReturnToIdle();
                }
            }
        }

        private void EndPickup()
        {
            _savedPreItemPickup = false;
            SaveGameSaveLoad.ClearSaveState();
            Game1.GameManager.SaveManager.DisableHistory();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  SWORD CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateSwordSequence()
        {
            if (CurrentState == State.SwordShow1)
            {
                if (!Animation.IsPlaying)
                {
                    Animation.Play("show2");
                    _showSwordLv2Counter = 500;
                    CurrentState = State.SwordShow2;
                    Game1.GameManager.PlaySoundEffect("D360-07-07");
                    var animation = new ObjSparkingEffect(Map, 0, 0, 0, 0);
                    animation.EntityPosition.Set(new Vector2(BodyRectangle.X, EntityPosition.Y - EntityPosition.Z - 30));
                    Map.Objects.SpawnObject(animation);

                    // "GetObjectsOfType" is not something that should be used often, but in this case it's fine.
                    List<GameObject> objects = Map.Objects.GetObjectsOfType(typeof(ObjMusicTile));

                    // Update that the sword has been collected on the music tile.
                    foreach (var musicTile in objects.OfType<ObjMusicTile>())
                        musicTile.SwordCollected();
                }
                else
                    return;
            }
            else if (CurrentState == State.SwordShow2)
            {
                _showSwordLv2Counter -= Game1.DeltaTime;
                if (_showSwordLv2Counter < 0)
                    CurrentState = State.Idle;
            }
        }

        private void UseSword()
        {
            // Prevent using the sword while it is spinning.
            if (_isSwordSpinning)
                return;

            // Workaround when charging sword and walking into an NPC.
            if (_npcCrossSword)
                _npcCrossSword = false;

            // Workaround for keeping direction left or right when attacking in 2D maps.
            if (Map.Is2dMap)
            {
                Vector2 input = ControlHandler.GetMoveVector2();
                if (input.X != 0 && input.Y != 0)
                    Direction = input.X > 0 ? 2 : 0;
            }
            // Skip sword if any of the below is happening.
            if (!IsAttackingState() &&
                !IsBlockingState() &&
                CurrentState != State.Idle &&
                CurrentState != State.Pushing &&
                CurrentState != State.Rafting &&
                (CurrentState != State.Jumping || _railJump) &&
                (CurrentState != State.Swimming || !Map.Is2dMap))
                return;

            // Play a random sword slash sound effect.
            var slashSounds = new[] { "D378-02-02", "D378-20-14", "D378-21-15", "D378-24-18" };
            Game1.GameManager.PlaySoundEffect(slashSounds[Game1.RandomNumber.Next(0, 4)]);

            // Play the attack and weapon animation.
            Animation.Stop();
            AnimatorWeapons.Stop();
            Animation.Play("attack_" + Direction);
            AnimatorWeapons.Play("attack_" + Direction);

            // Set up some states.
            IsPoking = false;
            _pokeStart = false;
            _stopCharging = false;
            _swordPoked = false;
            _shotSword = false;
            _isSwordSpinAttack = false;

            _swordChargeCounter = sword_charge_time;
            _beamDirection = Direction;

            // If the raft is moving stop it during attacks.
            StopRaft();

            // If in an accompanying state -> switch to a merged state.
            CurrentState = CurrentState switch
            {
                State.Blocking => State.AttackBlocking,
                State.Swimming => State.AttackSwimming,
                State.Jumping => State.AttackJumping,
                _ => State.Attacking
            };
            // Reset the jump hack timer.
            _jumpEndTimer = 0;
        }

        private void HoldSword()
        {
            // Since there is no state to know when the sword is held this
            // variable can be referenced to perform that check.
            _isHoldingSword = true;
        }

        private Box GetSwordDamageBox(RectangleF collisionRectangle) => 
            new Box(
                collisionRectangle.X + EntityPosition.X + _animationOffsetX,
                collisionRectangle.Y + EntityPosition.Y - EntityPosition.Z + _animationOffsetY, -8,
                collisionRectangle.Width,
                collisionRectangle.Height, 16);

        private void UpdateCharging()
        {
            //  Keep the charging state until rail jump has finished.
            if (_railJump && CurrentState == State.ChargeJumping)
                _isHoldingSword = true;

            // stop charging
            if (_isHoldingSword)
            {
                // A workaround when repelling against certain objects that forces
                // poke animation and needs to return to the "stand" animation.
                if (_pokeRepelFix)
                {
                    if (_pokeRepelTimer > 0)
                        _pokeRepelTimer -= Game1.DeltaTime;

                    if (_pokeRepelTimer <= 0)
                    {
                        _pokeRepelFix = false;
                        Animation.Play("stand_" + Direction);
                        AnimatorWeapons.Play("stand_" + Direction);
                    }
                }
                // poke objects that walk into the sowrd
                RectangleF collisionRectangle = AnimatorWeapons.CollisionRectangle;
                var damageOrigin = BodyRectangle.Center;
                SwordDamageBox = GetSwordDamageBox(collisionRectangle);

                var hitType = Game1.GameManager.SwordLevel == 1 ? HitType.Sword1 : HitType.Sword2;
                var damage = Game1.GameManager.SwordLevel == 1 ? 1 : 2;

                // Red cloak doubles damage.
                if (Game1.GameManager.CloakType == GameManager.CloakRed)
                    damage *= 2;

                // Piece of power doubles the damage.
                if (Game1.GameManager.PieceOfPowerIsActive)
                    damage *= 2;

                var pieceOfPower = Game1.GameManager.PieceOfPowerIsActive || Game1.GameManager.CloakType == GameManager.CloakRed;
                var hitCollision = Map.Objects.Hit(this, damageOrigin, SwordDamageBox, hitType | HitType.SwordHold, damage, pieceOfPower, out var direction, true);

                // If the sword is pointed at an NPC.
                if (_npcSwordCross)
                {
                    // Force Link into idle state and put the sword away.
                    _npcCrossSword = true;
                    CurrentState = State.Idle;
                    _isHoldingSword = false;
                    _avoidanceDirection = Direction;
                    _avoidanceStartPos = new Vector2(EntityPosition.X, EntityPosition.Y);
                    return;
                }
                // Start poking?
                if (hitCollision != Values.HitCollision.None && hitCollision != Values.HitCollision.NoneBlocking)
                {
                    var knockback = IsSwimmingState() ? _swimRepelStrength : _baseRepelStrength;

                    // If it's repelling and the player is charging, don't interrupt the charge.
                    if ((hitCollision & Values.HitCollision.RepellingParticle) != 0 && IsChargingState())
                    {
                        // Spawn a hit particle.
                        if (_hitParticleTime + 225 < Game1.TotalGameTime)
                        {
                            // Use the direction to determine the offset.
                            Point offset = Direction switch
                            {
                                0 => new Point(-8,0),
                                1 => new Point(2,-11),
                                2 => new Point(10,0),
                                3 => new Point(0,5),
                                _ => new Point(0,0),
                            };
                            _hitParticleTime = Game1.TotalGameTime;
                            SpawnRepelParticle(collisionRectangle, offset.X, offset.Y);
                        }
                        RepelPlayer(hitCollision, direction, knockback);

                        // When repelling against sworded enemies.
                        if ((hitCollision & Values.HitCollision.Repelling2) != 0)
                        {
                            CurrentState = State.Attacking;

                            // Play the poking animation when making contact.
                            Animation.Play("poke_" + Direction);
                            AnimatorWeapons.Play("poke_" + Direction);

                            // Reset the sword charge counter.
                            _swordChargeCounter = sword_charge_time;

                            // A workaround when repelling against certain objects that forces
                            // poke animation and needs to return to the "stand" animation.
                            _pokeRepelFix = true;
                            _pokeRepelTimer = 128;
                        }
                    }
                    // If it's a standard sword attack or <other>?
                    else
                    {
                        // Set the sword was poked and play the poke animation.
                        _swordPoked = true;
                        Animation.Play("poke_" + Direction);
                        AnimatorWeapons.Play("poke_" + Direction);

                        // If in an accompanying state -> switch to a merged state.
                        if (CurrentState == State.Blocking)
                            CurrentState = State.AttackBlocking;
                        else
                            CurrentState = State.Attacking;

                        RepelPlayer(hitCollision, direction, knockback);
                    }
                }
                else if (_swordChargeCounter > 0)
                {
                    _swordChargeCounter -= Game1.DeltaTime;

                    // Finished charging?
                    if (_swordChargeCounter <= 0)
                        Game1.GameManager.PlaySoundEffect("D360-04-04");
                }
            }
            else
            {
                // Start the sword spin attack.
                if (_swordChargeCounter <= 0)
                    StartSwordSpin();
                else
                {
                    // If cancelling a charge in the air, resume jumping animation. This
                    // method of charge cancelling works for both 2D and 3D maps. 
                    if (!_railJump && !_body.IsGrounded)
                    {
                        CurrentState = State.Jumping;
                        Animation.Play("jump_" + Direction);
                    }
                    // Otherwise return to idle state.
                    else
                        ReturnToIdle();
                }
            }
            // Probably a hacky way of updating the sword position while swimming in 2D mode.
            var moveVector = ControlHandler.GetMoveVector2();
            var moveDirX = moveVector.X switch
            {
                < 0 => _lastSwimDirection = 0,
                > 0 => _lastSwimDirection = 2,
                _   => _lastSwimDirection
            };
            if (CurrentState == State.ChargeSwimming && moveDirX % 2 == 0)
                AnimatorWeapons.Play("stand_" + moveDirX);
        }

        private void StartSwordSpin()
        { 
            // If in an accompanying state -> switch to a merged state.
            if (IsSwimmingState())
                CurrentState = State.AttackSwimming;
            else if (IsJumpingState())
                CurrentState = State.AttackJumping;
            else if (IsBlockingState())
                CurrentState = State.AttackBlocking;
            else
                CurrentState = State.Attacking;

            Animation.Play("swing_" + Direction);
            AnimatorWeapons.Play("swing_" + Direction);

            Game1.GameManager.PlaySoundEffect("D378-03-03");

            _swordChargeCounter = sword_charge_time;
            _isSwordSpinAttack = true;
        }

        public bool ClassicSword { get => GameSettings.ClassicSword && !_isSwordSpinning; }

        private static Box GetSwordClassicTile(Box box)
        {
            const int TileSize = 16;

            // Use center point of the box.
            float centerX = box.X + box.Width  * 0.5f;
            float centerY = box.Y + box.Height * 0.5f;

            // Offset the center when facing up or down.
            if (MapManager.ObjLink.Direction == 1)
                centerX += 3;
            else if (MapManager.ObjLink.Direction == 3)
                centerX -= 3;

            // Get the tile position.
            int tileX = (int)Math.Floor(centerX / TileSize);
            int tiley = (int)Math.Floor(centerY / TileSize);

            return new Box(tileX * TileSize, tiley * TileSize, box.Z, TileSize, TileSize, box.Depth);
        }

        private void UpdateAttacking()
        {
            // If the player is dashing, hold the sword out front.
            if (_bootsRunning && CarrySword)
                if (_isRotating)
                    AnimatorWeapons.Play("stand_" + _rotateDirection);
                else
                    AnimatorWeapons.Play("stand_" + Direction);

            // If the sword is not out just exit.
            if (AnimatorWeapons.CollisionRectangle.IsEmpty)
                return;

            // Get the damage origin point.
            var damageOrigin = BodyRectangle.Center;
            if (Map.Is2dMap)
                damageOrigin.Y -= 4;

            // Get the base damage type of hit to try to hit enemies with.
            var hitType = _bootsRunning 
                ? HitType.PegasusBootsSword 
                : Game1.GameManager.SwordLevel == 1 
                    ? HitType.Sword1 
                    : HitType.Sword2;

            // Get the base damage depending on the sword's level.
            var damage = Game1.GameManager.SwordLevel == 1 ? 1 : 2;

            // If it's a sword spin, double the damage and add "SwordSpin" damage type.
            if (_isSwordSpinAttack)
            {
                damage *= 2;
                hitType |= HitType.SwordSpin;
            }
            // If the player is dashing with boots, double the damage again.
            if (_bootsRunning)
                damage *= 2;

            // If the player has a piece of power, double the damage again.
            if (Game1.GameManager.PieceOfPowerIsActive)
                damage *= 2;

            // If the player has the red tunic, double the damage yet again.
            if (Game1.GameManager.CloakType == GameManager.CloakRed)
                damage *= 2;

            // Track if a "Piece of Power" is active or if the red tunic is equipped. This is used for the "damage launch" effect.
            var pieceOfPower = Game1.GameManager.PieceOfPowerIsActive || Game1.GameManager.CloakType == GameManager.CloakRed;

            // Get the sword's damage box using the sprite's animation rectangle.
            RectangleF collisionRectangle = AnimatorWeapons.CollisionRectangle;
            SwordDamageBox = GetSwordDamageBox(collisionRectangle);
            var ClassicBox = Box.Empty;

            // If "Classic Sword" is enabled get the tile the sword overlaps with the most.
            if (ClassicSword)
            {
                // Only the final frame can hit.
                if (AnimatorWeapons.CurrentFrameIndex == 2)
                {
                    // Reduce it to the single dominant tile.
                    ClassicBox = GetSwordClassicTile(SwordDamageBox);
                }
            }
            // For the "normal" hit lerp the collision box between the three frames of the attack.
            if (AnimatorWeapons.CurrentAnimation.Frames.Length > AnimatorWeapons.CurrentFrameIndex + 1)
            {
                var frameState = (float)(AnimatorWeapons.FrameCounter / AnimatorWeapons.CurrentFrame.FrameTime);
                var collisionRectangleNextFrame = AnimatorWeapons.GetCollisionBox(AnimatorWeapons.CurrentAnimation.Frames[AnimatorWeapons.CurrentFrameIndex + 1]);

                collisionRectangle = new RectangleF(
                    MathHelper.Lerp(collisionRectangle.X, collisionRectangleNextFrame.X, frameState),
                    MathHelper.Lerp(collisionRectangle.Y, collisionRectangleNextFrame.Y, frameState),
                    MathHelper.Lerp(collisionRectangle.Width, collisionRectangleNextFrame.Width, frameState),
                    MathHelper.Lerp(collisionRectangle.Height, collisionRectangleNextFrame.Height, frameState));
            }
            // Try to hit enemies with the "normal" hit. This fires whether "Classic Sword" is enabled or not.
            var hitCollision = Map.Objects.Hit(this, damageOrigin, SwordDamageBox, hitType, damage, pieceOfPower, out var direction, true);

            // If "Classic Sword" is enabled, also hit with "ClassicSword" damage type. This will only try to
            // hit bushes, grass, and crystals. Bombs will also not react to this or the sword hit type if enabled.
            if (ClassicSword && !ClassicBox.IsEmpty)
                Map.Objects.Hit(this, damageOrigin, ClassicBox, HitType.ClassicSword, damage, pieceOfPower, out var directionB, true);

            // If the player is poking with the sword.
            if (_pokeStart)
            {
                _pokeStart = false;

                if (hitCollision != Values.HitCollision.NoneBlocking)
                {
                    var swordRectangle = AnimatorWeapons.CollisionRectangle;
                    var swordBox = new Box(
                        swordRectangle.X + EntityPosition.X + _animationOffsetX,
                        swordRectangle.Y + EntityPosition.Y - EntityPosition.Z + _animationOffsetY, 0,
                        swordRectangle.Width, swordRectangle.Height, 4);
                    var destroyableWall = DestroyableWall(swordBox);
                    Game1.GameManager.PlaySoundEffect("D360-07-07");

                    if (destroyableWall)
                        Game1.GameManager.PlaySoundEffect("D378-23-17");

                    var pokeParticle = new ObjSparkingEffect(Map, 0, 0, 0, 0);
                    pokeParticle.EntityPosition.X = EntityPosition.X + _pokeAnimationOffset[Direction].X;
                    pokeParticle.EntityPosition.Y = EntityPosition.Y + _pokeAnimationOffset[Direction].Y;
                    Map.Objects.SpawnObject(pokeParticle);
                }
            }
            if (hitCollision != Values.HitCollision.None && hitCollision != Values.HitCollision.NoneBlocking)
                _stopCharging = true;

            // Use the direction that was set when the attack took place.
            var beamDirection = _beamDirection;

            // Shoot the sword if the player has the Level 2 sword and full health.
            if (!_shotSword && (Game1.GameManager.SwordLevel == 2 || swordbeam_level1) && (Game1.GameManager.CurrentHealth >= Game1.GameManager.MaxHearts * 4 || swordbeam_always) && AnimatorWeapons.CurrentFrameIndex == 2)
            {
                _shotSword = true;
                var objSwordShot = new ObjSwordShot(Map, EntityPosition, _shootSwordOffset[beamDirection], Game1.GameManager.SwordLevel, beamDirection);
                Map.Objects.SpawnObject(objSwordShot);
                Map.Objects.RegisterAlwaysAnimateObject(objSwordShot);
            }

            // Spawn hit particle?
            if ((hitCollision & Values.HitCollision.Particle) != 0 && _hitParticleTime + 225 < Game1.TotalGameTime)
            {
                _hitParticleTime = Game1.TotalGameTime;
                SpawnRepelParticle(collisionRectangle);
            }
            var knockback = IsSwimmingState() ? _swimRepelStrength : _baseRepelStrength;

            RepelPlayer(hitCollision, direction, knockback);
        }

        private void RepelPlayer(Values.HitCollision collisionType, Vector2 direction, float customMultiplier = 0f)
        {
            // Repel the player.
            if ((collisionType & Values.HitCollision.Repelling) != 0 && _hitRepelTime + 225 < Game1.TotalGameTime)
            {
                _hitRepelTime = Game1.TotalGameTime;

                var multiplier = Map.Is2dMap ? 1.5f : (_bootsRunning ? 1.5f : 1.0f);

                if ((collisionType & Values.HitCollision.Repelling0) != 0)
                    multiplier = 2.50f;
                else if ((collisionType & Values.HitCollision.Repelling1) != 0)
                    multiplier = 2.00f;
                else if ((collisionType & Values.HitCollision.Repelling2) != 0)
                    multiplier = 1.50f;
                else if (customMultiplier > 0f)
                    multiplier = customMultiplier;

                if (_bootsRunning)
                    _bootsStop = true;

                _repelVelocity = new Vector2(-direction.X, -direction.Y) * multiplier;
            }
            PreventFieldKnockback();
        }

        private void SpawnRepelParticle(RectangleF collisionRectangle, int OffsetX = 0, int OffsetY = 0)
        {
            Game1.GameManager.PlaySoundEffect("D360-07-07");

            // Spawn the poke particle.
            var posX = (int)(EntityPosition.X - 8 + collisionRectangle.X + collisionRectangle.Width / 2 + OffsetX);
            var posY = (int)(EntityPosition.Y - 15 + collisionRectangle.Y + collisionRectangle.Height / 2 + OffsetY);
            var pokeParticle = new ObjSparkingEffect(Map, posX, posY, 0, 0);
            Map.Objects.SpawnObject(pokeParticle);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  SHIELD CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void HoldShield()
        {
            if ((CurrentState != State.Idle &&
                CurrentState != State.Pushing &&
                CurrentState != State.Attacking &&
                CurrentState != State.Rafting &&
                CurrentState != State.Charging) ||
                _bootsRunning)
                return;

            if (!_wasBlocking & !_blockButton)
                Game1.GameManager.PlaySoundEffect("D378-22-16");

            _wasBlocking = _blockButton = true;

            if (CurrentState == State.Attacking)
                CurrentState = State.AttackBlocking;
            else if (CurrentState == State.Charging)
                CurrentState = State.ChargeBlocking;
            else
                CurrentState = State.Blocking;
        }

        private void ReleaseShield()
        {
            _blockButton = false;

            if (!IsBlockingState())
                return;

            if (CurrentState == State.AttackBlocking)
                CurrentState = State.Attacking;
            if (CurrentState == State.ChargeBlocking)
                CurrentState = State.Charging;
        }

        private Box GetShieldBox()
        {
            // The mirror shield requires a slightly different offset than the normal shield
            // when facing south. I'm guessing that it's actually one pixel larger facing down.
            var mirrorShield = Game1.GameManager.GetItem("mirrorShield");
            var hasMirrorShield = mirrorShield?.Count >= 1;
            var rect = Animation.CollisionRectangle;
            var key = (Direction, hasMirrorShield);

            var offsets = key switch
            {
                (1, _)     => ( -9, -18, +4, +2), // Up
                (2, _)     => (-11, -16, +4, +2), // Right
                (3, true)  => ( -8, -18, +4, +3), // Down (Mirror Shield)
                (3, false) => ( -9, -18, +4, +3), // Down
                (_, _)     => ( -7, -16, +4, +2), // Left
            };
            // Assign the results of the switch.
            var (xOff, yOff, wOff, hOff) = offsets;

            // Return the proper shield box based on direction.
            return new Box(
                EntityPosition.X + rect.X + xOff,
                EntityPosition.Y + rect.Y + yOff, 0,
                rect.Width + wOff,
                rect.Height + hOff, 12);
        }

        private void UpdateShieldPush()
        {
            // Check if the collision rectangle is empty or if the player is trapped (Like-Like / Anti-Kirby).
            if (Animation.CollisionRectangle.IsEmpty || _isTrapped)
                return;

            // Get the shield rectangle.
            ShieldBlockBox = GetShieldBox();
            var pushedRectangle = Map.Objects.PushObject(ShieldBlockBox, _walkDirection[Direction] + _body.VelocityTarget * 0.5f, PushableComponent.PushType.Impact);

            // Push the object and get repelled from the pushed object.
            if (pushedRectangle != null)
            {
                _bootsRunning = false;
                _bootsCounter = 0;

                // Only apply velocity if it's already zero so it can properly decay over time.
                if (_shieldVelocity == Vector2.Zero || Vector2.Dot(_shieldVelocity, -_walkDirection[Direction]) < 0f)
                    _shieldVelocity = -_walkDirection[Direction] * pushedRectangle.RepelMultiplier;

                // Spawn the "poke" particle.
                if (pushedRectangle.RepelParticle)
                {
                    var posX = (int)(pushedRectangle.PushableBox.Box.X + pushedRectangle.PushableBox.Box.Width / 2);
                    var posY = (int)(pushedRectangle.PushableBox.Box.Y + pushedRectangle.PushableBox.Box.Height / 2);
                    Map.Objects.SpawnObject(new ObjSparkingEffect(Map, posX, posY, 0, 0));
                    Game1.GameManager.PlaySoundEffect("D360-07-07");
                }
                // Play the "bumping" sound effect.
                else
                    Game1.GameManager.PlaySoundEffect("D360-09-09");
            }
            PreventFieldKnockback();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  ROCS FEATHER CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UseFeather()
        {
            if (Is2DMode)
                Jump2D();
            else
                Jump();
        }

        private void HoldFeather()
        {
            // Set when holding in jump button. This is used to track when the
            // button was released. Holding button longer = higher jumping.
            _jump2DHold = Map.Is2dMap;
        }

        private void ReleaseFeather()
        {
            // Track when the jump button is released.
            _jump2DHold = false;
        }

        private bool Jump(bool force = false, bool playSoundEffect = true)
        {
            if ((!force && (
                CurrentState != State.Idle &&
                CurrentState != State.Idle &&
                CurrentState != State.Attacking &&
                CurrentState != State.AttackBlocking &&
                CurrentState != State.Charging &&
                CurrentState != State.ChargeBlocking &&
                CurrentState != State.Pushing &&
                CurrentState != State.Blocking &&
                CurrentState != State.Rafting)) ||
                _isTrapped || !_canJump)
            {
                if (_isTrapped && playSoundEffect)
                    Game1.GameManager.PlaySoundEffect("D360-13-0D");

                return false;
            }

            if (!_body.IsGrounded)
                return false;

            // release the carried object if the player is carrying something
            ReleaseCarriedObject();

            if (playSoundEffect)
                Game1.GameManager.PlaySoundEffect("D360-13-0D");

            if (_isRafting)
            {
                // do not move while jumping
                _moveVelocity = Vector2.Zero;
                _lastMoveVelocity = Vector2.Zero;
                StopRaft();
            }
            else
            {
                // base move velocity does not contain the velocity added in the air
                // so when we hit the floor and directly jump afterwards we do not get the velocity of the previouse jump
                _lastMoveVelocity = _lastBaseMoveVelocity;
            }

            _startedJumping = true;
            _body.Velocity.Z = feather_velocity;
            _jumpStartZPos = _body.Position.Z;
            _jumpEndTimer = 0;

            // while attacking the player can still jump but without the animation
            if (CurrentState == State.Attacking)
                CurrentState = State.AttackJumping;
            else if (CurrentState == State.Charging || CurrentState == State.ChargeBlocking)
                CurrentState = State.ChargeJumping;
            else
            {
                if (!_bootsRunning)
                {
                    _bootsWasRunning = true;
                }
                // start the jump animation
                Animation.Play("jump_" + Direction);
                CurrentState = State.Jumping;
            }
            return true;
        }

        private void UpdateJump()
        {
            // Update the jump hack timer.
            if (_jumpEndTimer > 0)
                _jumpEndTimer -= Game1.DeltaTime;

            // Catch when an attack ends just before a jump which fails to set the jumping state.
            if ((CurrentState == State.Idle) && !_body.IsGrounded && _body.Velocity.Z > 1.85f)
            {
                CurrentState = State.Jumping;
                Animation.Play("jump_" + Direction);
            }

            // If not in a jumping state return early.
            if (CurrentState != State.Jumping &&
                CurrentState != State.AttackJumping &&
                CurrentState != State.ChargeJumping)
                return;

            // Handle when rail jumping.
            if (_railJump)
            {
                _railJumpPercentage += Game1.TimeMultiplier * _railJumpSpeed;
                var amount = MathF.Sin(_railJumpPercentage * (MathF.PI * 0.3f)) / MathF.Sin(MathF.PI * 0.3f);
                var newPosition = Vector2.Lerp(_railJumpStartPosition, _railJumpTargetPosition, amount);
                EntityPosition.Set(newPosition);

                EntityPosition.Z = MathF.Sin(_railJumpPercentage * MathF.PI) * _railJumpHeight + _railJumpPercentage * _railJumpPositionZ;

                if (_railJumpPercentage >= 1)
                {
                    _railJump = false;
                    _body.IgnoreHeight = false;
                    _body.IgnoresZ = false;
                    _body.Velocity.Z = -1f;
                    _body.JumpStartHeight = _railJumpPositionZ;
                    EntityPosition.Set(_railJumpTargetPosition);
                    EntityPosition.Z = _railJumpPositionZ;
                    _lastMoveVelocity = Vector2.Zero;
                }
            }
            // Touched the ground.
            if (_body.IsGrounded && _body.Velocity.Z <= 0)
            {
                // Only push the player if he jumps into the water and does not walk. Walking is handled in another location.
                if (SystemBody.GetFieldState(_body).HasFlag(MapStates.FieldStates.DeepWater))
                {
                    _body.Velocity = new Vector3(_body.VelocityTarget.X, _body.VelocityTarget.Y, 0) * 0.5f;
                }
                // HACK: Jumping then just before landing plays the same frame of animation as the first
                // frame in walking. This timer forces "stand" animation for a few frames.
                else if (!_railJump)
                    _jumpEndTimer = 75;

                // Reset the jump starting Z position to 0.
                _jumpStartZPos = 0;

                if (!_railJump)
                {
                    // Keep the charging state if it was held during a jump.
                    if (CurrentState == State.ChargeJumping)
                        CurrentState = State.Charging;
                    else
                        ReturnToIdle();
                }
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  MAGIC POWDER CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UseToadstool()
        {
            var findWitch = new List<GameObject>();
            var boxSize = 14;
            var interactionBox = new Box(
                EntityPosition.X + _walkDirection[Direction].X * (BodyRectangle.Width / 2 + boxSize / 2) - boxSize / 2,
                BodyRectangle.Center.Y + _walkDirection[Direction].Y * (BodyRectangle.Height / 2 + boxSize / 2) - boxSize / 2, 0,
                boxSize, boxSize, 16);

            Map.Objects.GetComponentList(findWitch, (int)interactionBox.X, (int)interactionBox.Y,
                (int)interactionBox.Width, (int)interactionBox.Height, InteractComponent.Mask);

            foreach (var obj in findWitch)
            {
                if (obj is ObjPerson theWitch && theWitch.PersonID == "witch")
                {
                    var dotWitch = theWitch.EntityPosition.Position - EntityPosition.Position;
                    if (Vector2.Dot(dotWitch, _walkDirection[Direction]) < 0.7f)
                        continue;

                    theWitch.ForceInteract();
                    return;
                }
            }

            if (CurrentState != State.PickingUp)
            {
                CurrentState = State.ShowToadstool;
                Animation.Play("show2");
                Game1.GameManager.StartDialogPath("toadstool_hold");
            }
        }

        private void UsePowder()
        {
            if (CurrentState != State.Idle &&
                CurrentState != State.Jumping &&
                CurrentState != State.Rafting &&
                CurrentState != State.Pushing &&
                (CurrentState != State.Swimming || !Map.Is2dMap))
                return;

            // Remove one powder from the inventory.
            if (!Game1.GameManager.RemoveItem("powder", 1))
                return;

            var spawnPosition = new Vector2(EntityPosition.X, EntityPosition.Y) + _powderOffset[Direction];
            Map.Objects.SpawnObject(new ObjPowder(Map, spawnPosition.X, spawnPosition.Y, EntityPosition.Z, true));

            if (CurrentState != State.Jumping)
            {
                StopRaft();

                CurrentState = State.Powdering;
                Animation.Play("powder_" + Direction);
            }
            // Try to spawn a new toadstool if out of powder.
            Game1.GameManager.StartDialogPath("toadstool_check");
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  BOMBS CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UseBomb()
        {
            // If Link is carrying an object throw it.
            if (_carriedGameObject != null)
            {
                ThrowCarriedObject();
                return;
            }
            // Return if not in any of the current states.
            if (CurrentState != State.Idle &&
                CurrentState != State.Rafting &&
                CurrentState != State.Pushing &&
                CurrentState != State.Jumping &&
                (CurrentState != State.Swimming || !Map.Is2dMap))
                return;

            // Pick up the bomb if there is one infront of the player.
            var _bombGrabRectangle = new RectangleF(
                EntityPosition.X + _walkDirection[Direction].X * (_body.Width / 2) - 4,
                EntityPosition.Y - _body.Height / 2 + _walkDirection[Direction].Y * (_body.Height / 2) - 4, 8, 8);

            // Pick up the first bomb found.
            foreach (var objBomb in BombList)
            {
                var carriableComponent = objBomb.Components[CarriableComponent.Index] as CarriableComponent;
                if (!carriableComponent.IsActive ||
                    !carriableComponent.Rectangle.Rectangle.Intersects(_bombGrabRectangle))
                    continue;

                carriableComponent?.StartGrabbing?.Invoke();
                StartPickup(carriableComponent);

                Animation.Play("pull_" + Direction);

                return;
            }

            // Remove 1 bomb from the inventory.
            if (!Game1.GameManager.RemoveItem("bomb", 1))
                return;

            var spawnPosition = new Vector2(EntityPosition.X, EntityPosition.Y) + _bombOffset[Direction];
            Map.Objects.SpawnObject(new ObjBomb(Map, spawnPosition.X, spawnPosition.Y, true, false, 2000));
            
            CurrentState = State.Bombing;

            // play animation
            Animation.Play("powder_" + Direction);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  BOW & ARROWS CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void UseArrow()
        {
            if (CurrentState != State.Idle &&
                CurrentState != State.Jumping &&
                CurrentState != State.Rafting &&
                CurrentState != State.Bombing &&
                CurrentState != State.Pushing &&
                (CurrentState != State.Swimming || !Map.Is2dMap))
                return;

            // Remove one powder from the inventory,
            if (!Game1.GameManager.RemoveItem("bow", 1))
                return;

            var arrow = new ObjArrow(Map, EntityPosition, _arrowOffset[Direction], Direction);
            Map.Objects.SpawnObject(arrow);
            Map.Objects.RegisterAlwaysAnimateObject(arrow);

            if (CurrentState != State.Jumping)
            {
                StopRaft();

                CurrentState = State.Powdering;
                Animation.Play("powder_" + Direction);
            }

            Game1.GameManager.PlaySoundEffect("D378-10-0A");
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  SHOVEL CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UseShovel()
        {
            if (CurrentState != State.Idle || _isClimbing)
                return;

            CurrentState = State.Digging;
            _hasDug = false;

            // play animation
            Animation.Play("dig_" + Direction);

            _digPosition = new Point(
                (int)((EntityPosition.X + _shovelOffset[Direction].X) / Values.TileSize),
                (int)((EntityPosition.Y + _shovelOffset[Direction].Y) / Values.TileSize));

            _canDig = Map.CanDig(_digPosition);

            if (_canDig)
                Game1.GameManager.PlaySoundEffect("D378-14-0E");
            else
                Game1.GameManager.PlaySoundEffect("D360-07-07");
        }

        private void UpdateDigging()
        {
            if (Animation.CurrentFrameIndex > 0 && !_hasDug)
            {
                _hasDug = true;
                if (_canDig)
                    Map.Dig(_digPosition, EntityPosition.Position, Direction);
            }
            if (!Animation.IsPlaying)
                CurrentState = State.Idle;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  POWER BRACELET CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UseBracelet()
        {
            if (_carriedComponent == null || CurrentState != State.Carrying)
                return;

            ThrowCarriedObject();
        }

        private RectangleF GetGrabRectangle(int dir, float size)
        {
            float bias = 0.16f;
            float recX = EntityPosition.X + _walkDirection[dir].X * (_body.Width / 2) - 1 - (bias / 2);
            float recY = EntityPosition.Y - _body.Height / 2 + _walkDirection[dir].Y * (_body.Height / 2) - 1 - (bias / 2);
            return new RectangleF(recX, recY, size + bias, size + bias);
        }

        private void HoldBracelet()
        {
            // Part One: Grabbing the object. State must be idle, pushing, or swimming (for Flying Rooster) to continue.
            if (CurrentState != State.Idle && CurrentState != State.Pushing && CurrentState != State.Swimming)
                return;

            // Stores the grabbed object.
            GameObject grabbedObject = null;

            // Try to grab an object in range and not yet carrying an object.
            if (_carriedComponent == null && _instantPickupObject == null)
            {
                // Tiny rectangle that finds objects in front of Link that can be grabbed.
                GrabRectangle = GetGrabRectangle(Direction, 2.00f);

                // Find's any possible objects within the rectangle.
                grabbedObject = Map.Objects.GetCarryableObjects(GrabRectangle);

                // A grabble object has been found.
                if (grabbedObject != null)
                {
                    // Get the carry component of the grabbable object.
                    var carriableComponent = grabbedObject.Components[CarriableComponent.Index] as CarriableComponent;

                    // Don't grab the rooster if it's too high in the air.
                    if (grabbedObject is ObjCock cock)
                    {
                        if (cock.EntityPosition.Z > 8)
                            return;
                    }
                    // If the component is active then grab the object.
                    if (carriableComponent.IsActive)
                    {
                        // Allow picking up the rooster in the water. Otherwise don't try to lift.
                        if (CurrentState == State.Swimming)
                        {
                            if (grabbedObject is not ObjCock) return;
                            _swimRoosterPickup = true;
                        }
                        // Grabbing state is used to determine part two.
                        CurrentState = State.Grabbing;

                        if (!carriableComponent.IsHeavy || Game1.GameManager.StoneGrabberLevel > 1)
                            carriableComponent?.StartGrabbing?.Invoke();
                    }
                }
            }
            // If the previous run of this method started a pull increment the counter or otherwise reset it.
            if (_wasPulling)
                _pullCounter += Game1.DeltaTime;
            else
                _pullCounter = 0;

            // If an instant pickup object was grabbed, restore it from the previous loop.
            if (_instantPickupObject != null)
                grabbedObject = _instantPickupObject;

            // Part Two: An object was found above and the state was set to grabbing.
            if (CurrentState == State.Grabbing || _instantPickup)
            {
                // If not in the middle of an "instant pickup" loop, try to see if the current object is an instant pickup type.
                if (!_instantPickup)
                {
                    // Object is an "instant pickup" type.
                    Type[] instantPickupTypes = { typeof(ObjCock), typeof(MBossSmasherBall), typeof(BossGenieBottle), typeof(EnemyKarakoro), typeof(ObjDungeonHorseHead), typeof(ObjBall), typeof(ObjBird) };
                    bool isSpinyBeetle = grabbedObject is ObjBush bush && bush.NoRespawn || grabbedObject is ObjStone stone && stone.NoRespawn;
                    bool isPlayerBomb = grabbedObject is ObjBomb bomb && BombList.Contains(bomb);

                    // If it's an instant pickup type, remember it and store the object type.
                    if (isSpinyBeetle || isPlayerBomb || ObjectManager.IsGameObjectType(grabbedObject, instantPickupTypes))
                    {
                        _instantPickupObject = grabbedObject;
                        _instantPickup = true;
                    }
                }
                // Gets the carriable component from the object found above.
                var carriableComponent = grabbedObject.Components[CarriableComponent.Index] as CarriableComponent;

                // Get the direction of the analog stick.
                var moveVec = ControlHandler.GetMoveVector2();

                // Get if the object is being pulled and it's not null.
                if (carriableComponent?.Pull != null)
                {
                    // If being pulled get the vector. If not then reset it.
                    Vector2 pullVector = _pullCounter > 0
                        ? moveVec
                        : Vector2.Zero;

                    // If the pull has failed and the pull counter is below zero, reset the pull counter.
                    // PullResetTime is (-133). During this time the animation is not played.
                    if (!carriableComponent.Pull(pullVector) && _pullCounter < 0)
                        _pullCounter = PullResetTime;
                }
                // The pull vector must be over half the range of the analog stick.
                if (moveVec.Length() > 0.5 || _instantPickup)
                {
                    // Get the direction of the pull vector.
                    var moveDir = AnimationHelper.GetDirection(moveVec);

                    // The player must be pulling in the opposite direction.
                    if (ReverseDirection(moveDir) == Direction || _instantPickup)
                    {
                        // Do not show the pull animation while resetting.
                        if (_pullCounter >= 0)
                            CurrentState = State.Pulling;

                        // Used to determine if pulling was done on the next frame. This sets
                        // "_wasPulling" which counts up on the pull timer.
                        _isPulling = true;

                        // It's not a heavy object or the Power Bracelet is greater than level 1.
                        if (!carriableComponent.IsHeavy || Game1.GameManager.StoneGrabberLevel > 1)
                        {
                            // The pull counter exceeds the PullTime (100) and the object is not null.
                            if (_pullCounter >= PullTime && grabbedObject != null)
                            {
                                // Pick up the object and reset instant pickup variables.
                                StartPickup(carriableComponent);
                                _instantPickupObject = null;
                                _instantPickup = false;
                            }
                            // Reset the pull counter if it exceeds the maximum (400) and the carriable component has "IsStruggle"
                            // set on it. This is to reset the animation if pulling on the lever in level 4 and 7 dungeons.
                            if (_pullCounter > PullMaxTime && carriableComponent.IsStruggle)
                                _pullCounter = PullResetTime;
                        }
                    }
                }
            }
        }

        private void UpdatePositionCarriedObject(CPosition newPosition)
        {
            if (_carriedComponent == null)
                return;

            var targetPosition = new Vector3(EntityPosition.X, EntityPosition.Y, EntityPosition.Z + _carriedComponent.CarryHeight);

            if (CurrentState == State.PreCarrying)
            {
                // finished pickup animation?
                if (_preCarryCounter >= PreCarryTime)
                {
                    _preCarryCounter = PreCarryTime;
                    CurrentState = State.Carrying;
                }
                var pickupTime = 1 - MathF.Cos((_preCarryCounter / PreCarryTime) * (MathF.PI / 2));

                var carryPositionXY = Vector2.Lerp(
                    new Vector2(_carryStartPosition.X, _carryStartPosition.Y),
                    new Vector2(targetPosition.X, targetPosition.Y),
                    1 - MathF.Cos(pickupTime * (MathF.PI / 2)));
                var carryPositionZ = MathHelper.Lerp(_carryStartPosition.Z, targetPosition.Z,
                    MathF.Sin(pickupTime * (MathF.PI / 2)));

                if (!_carriedComponent.UpdatePosition(new Vector3(carryPositionXY.X, carryPositionXY.Y, carryPositionZ)))
                {
                    CurrentState = State.Idle;
                    ReleaseCarriedObject();
                }
            }
            else if (!_isFlying)
            {
                // move the carried object up/down with the walk animation
                if (Direction % 2 == 0)
                    targetPosition.Z += _isWalking ? Animation.CurrentFrameIndex : 1;
                else if (Map.Is2dMap)
                    targetPosition.Z += 1;

                if (!_carriedComponent.UpdatePosition(targetPosition))
                {
                    CurrentState = State.Idle;
                    ReleaseCarriedObject();
                }
            }
        }

        private void ThrowCarriedObject()
        {
            Game1.GameManager.PlaySoundEffect("D360-08-08");

            // play a little throw animation
            Animation.Play("throw_" + Direction);
            CurrentState = State.Throwing;

            _carriedComponent.Throw(_walkDirection[Direction] * 3f);
            RemoveCarriedObject();
        }

        private void StartPickup(CarriableComponent carriableComponent)
        {
            if (carriableComponent?.Init == null)
                return;

            _carriedComponent = carriableComponent;

            Game1.GameManager.PlaySoundEffect("D370-02-02");

            _carryStartPosition = _carriedComponent.Init();
            _carriedComponent.IsPickedUp = true;
            CurrentState = State.PreCarrying;
            _preCarryCounter = 0;

            _carriedGameObject = carriableComponent.Owner;
            _carriedObjDrawComp = carriableComponent.Owner.Components[DrawComponent.Index] as DrawComponent;
            if (_carriedObjDrawComp != null)
                _carriedObjDrawComp.IsActive = false;
        }

        public void ReleaseCarriedObject()
        {
            // let the carried item fall down
            if (_carriedComponent == null)
                return;

            _carriedComponent.Throw(new Vector2(0, 0));
            RemoveCarriedObject();
        }

        private void RemoveCarriedObject()
        {
            _carriedComponent.IsPickedUp = false;
            _carriedComponent = null;
            _carriedGameObject = null;

            if (_carriedObjDrawComp != null)
            {
                _carriedObjDrawComp.IsActive = true;
                _carriedObjDrawComp = null;
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  HOOKSHOT CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UseHookshot()
        {
            // If the cooldown is active then force exit.
            if (_hookshotCooldown > 0)
                return;

            // Only run if idle or using hookshot for the forced return.
            if (CurrentState != State.Idle &&
                CurrentState != State.Rafting &&
                CurrentState != State.Pushing &&
                CurrentState != State.Hookshot &&
                (!Map.Is2dMap || CurrentState != State.Swimming))
                return;

            // If hookshot is in progress force a comeback.
            if (_hookshotActive)
            {
                Hookshot.ForceComeback();
                return;
            }
            // Fire the shot, make active, and reset activation counter.
            _hookshotActive = true;
            _hookshotCounter = 0;

            // Get the current direction to fire the hookshot.
            var hookshotDirection = CurrentState == State.Swimming ? _swimDirection : Direction;

            // Spawn in the hookshot object and track it via "Hookshot" variable.
            var spawnPosition = new Vector3(
                EntityPosition.X + _hookshotOffset[hookshotDirection].X,
                EntityPosition.Y + _hookshotOffset[hookshotDirection].Y, EntityPosition.Z);
            Hookshot.Start(Map, spawnPosition, AnimationHelper.DirectionOffset[hookshotDirection]);
            Map.Objects.SpawnObject(Hookshot);
            Map.Objects.RegisterAlwaysAnimateObject(Hookshot);

            // Set the current state and reset values.
            CurrentState = State.Hookshot;
            _body.VelocityTarget = Vector2.Zero;
            _body.HoleAbsorption = Vector2.Zero;
            _body.IgnoreHoles = true;
            StopRaft();

            // Play Link's animation ("powder" is used for several items).
            Animation.Play("powder_" + hookshotDirection);
        }

        private void UpdateHookshot()
        {
            // If cooldown is active reduce it.
            if (_hookshotCooldown > 0)
                _hookshotCooldown -= Game1.DeltaTime;

            // Increase the reactivation counter.
            _hookshotCounter += Game1.DeltaTime;

            // After a period, force the hookshot to be inactive.
            if (_hookshotCounter > 1350)
            {
                _hookshotCounter = 0;

                // This is a workaround to the hookshot sometimes getting "stuck"
                // after usage caused by various interruptions to it finishing.
                if (_hookshotActive)
                {
                    Hookshot.Despawn();
                    CurrentState = State.Idle;
                    _hookshotActive = false;
                    return;
                }
            }
            // Hookshot is in progress.
            if (CurrentState == State.Hookshot)
            {
                // If currently moving continue moving.
                if (Hookshot.IsMoving)
                    return;

                // If it's returned, reset values and add brief cooldown.
                _hookshotActive = false;
                _hookshotCounter = 0;
                _hookshotCooldown = 75;
                _body.IgnoreHoles = false;
                ReturnToIdle();
            }
        }

        public void StartHookshotPull()
        {
            _hookshotPull = true;
            if (Map.Is2dMap)
            {
                _body.Velocity.Y = 0;
                _body.LastVelocityCollision = Values.BodyCollision.None;
            }

            // if the player is on the upper level he will not get pulled through water and we can move through colliders
            if ((_body.CurrentFieldState & MapStates.FieldStates.UpperLevel) != 0)
            {
                _body.IsGrounded = false;
                _body.Level = MapStates.GetLevel(_body.CurrentFieldState);
            }
        }

        public bool UpdateHookshotPull()
        {
            var distance = _body.BodyBox.Box.Center - Hookshot.HookshotPosition.Position;
            var pullVector = AnimationHelper.DirectionOffset[Direction];

            // Reached the end of the hook or collided with an object before.
            var check01 = distance.Length() < (distance + pullVector).Length();
            var check02 = _body.LastVelocityCollision != Values.BodyCollision.None && (_body.SlideOffset == Vector2.Zero || _body.BodyBox.Box.Contains(Hookshot.HookshotPosition.Position));
            var check03 = IsDying();

            // If any of the checks pass, stop the hookshot pull.
            if (check01 || check02 || check03)
            {
                _hookshotPull = false;
                _body.IgnoresZ = false;
                _body.IgnoreHoles = false;
                _body.Level = 0;
                return false;
            }
            _body.VelocityTarget = pullVector * 3;

            return true;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  BOOMERANG CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UseBoomerang()
        {
            if ((CurrentState != State.Idle &&
                CurrentState != State.Jumping &&
                CurrentState != State.Pushing &&
                CurrentState != State.Rafting &&
                (CurrentState != State.Swimming || !Map.Is2dMap)) || !Boomerang.IsReady)
                return;

            var spawnPosition = new Vector3(EntityPosition.X + _boomerangOffset[Direction].X, EntityPosition.Y + _boomerangOffset[Direction].Y, EntityPosition.Z);

            // can throw into multiple directions
            var boomerangVector = ControlHandler.GetMoveVector2();
            if (boomerangVector != Vector2.Zero)
                boomerangVector.Normalize();
            else
                boomerangVector = _lastBaseMoveVelocity;
            if (boomerangVector != Vector2.Zero)
                boomerangVector.Normalize();
            else
                boomerangVector = _walkDirection[Direction];

            Boomerang.Start(Map, spawnPosition, boomerangVector);
            Map.Objects.SpawnObject(Boomerang);
            Map.Objects.RegisterAlwaysAnimateObject(Boomerang);

            if (CurrentState != State.Jumping &&
                CurrentState != State.ChargeJumping)
            {
                CurrentState = State.Powdering;
                Animation.Play("powder_" + Direction);
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  MAGIC ROD CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void UseMagicRod()
        {
            if (CurrentState != State.Idle &&
                CurrentState != State.Rafting &&
                CurrentState != State.Pushing &&
                (CurrentState != State.Swimming || !Map.Is2dMap) &&
                (CurrentState != State.Jumping || _railJump))
                return;

            var magicShot = new ObjMagicRodShot(Map, EntityPosition, _magicRodOffset[Direction], Direction);
            Map.Objects.SpawnObject(magicShot);
            Map.Objects.RegisterAlwaysAnimateObject(magicShot);

            CurrentState = State.MagicRod;
            _swordChargeCounter = sword_charge_time;

            Game1.GameManager.PlaySoundEffect("D378-13-0D");
            StopRaft();

            // play animation
            Animation.Play("rod_" + Direction);
            AnimatorWeapons.Play("rod_" + Direction);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  OCARINA CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UseOcarina()
        {
            if ((CurrentState != State.Idle && CurrentState != State.Pushing && CurrentState != State.Ocarina) || _isClimbing)
                return;

            // Cancel playing the ocarina if pressed again.
            if (CurrentState == State.Ocarina)
            {
                StopOcarina();
                return;
            }
            // Used when drawing the notes.
            _ocarinaNoteIndex = 0;
            _ocarinaCounter = 0;

            // Pause whatever music is playing.
            Game1.GbsPlayer.Pause();

            // Set the selected ocarina song integer.
            _ocarinaSong = Game1.GameManager.SelectedOcarinaSong;

            // Get the song that has been selected.
            string ocarinaSong = _ocarinaSong switch
            {
                0 => "D370-09-09",  // Ballad of the Windfish
                1 => "D370-11-0B",  // Manbo's Mambo
                2 => "D370-10-0A",  // Frog's Song of Soul
                _ => "D370-21-15"   // Bad Playing
            };
            // Play the selected song.
            Game1.GameManager.PlaySoundEffect(ocarinaSong);

            // Set the state, face Link forward, and show the animation.
            CurrentState = State.Ocarina;
            Direction = 3;
            Animation.Play("ocarina");

            // Prevent Link from taking hits during this time.
            PreventDamageTimer = 8000;

            // Freeze the game world while the song is played.
            FreezeAnimations(true);
        }

        private void StopOcarina()
        {
            // Try to cancel the ocarina songs.
            Game1.GameManager.StopSoundEffect("D370-09-09");
            Game1.GameManager.StopSoundEffect("D370-11-0B");
            Game1.GameManager.StopSoundEffect("D370-10-0A");
            Game1.GameManager.StopSoundEffect("D370-21-15");

            // Resume the background music.
            Game1.GbsPlayer.Play();

            // This will not become some kind of exploit.
            PreventDamageTimer = 0;

            // Return to idle state.
            CurrentState = State.Idle;

            // Unfreeze the game world.
            FreezeAnimations(false);
        }

        private void UpdateOcarina()
        {
            // Ocarina is still being played.
            if (CurrentState == State.Ocarina)
            {
                // Disable the inventory while the ocarina plays.
                Game1.GameManager.InGameOverlay.DisableInventoryToggle = true;

                // Finished playing the ocarina.
                if (!Animation.IsPlaying)
                {
                    FinishedOcarinaSong();
                    return;
                }
                // Update animation.
                UpdateOcarinaAnimation();
            }
            // Manbo's Mambo teleport is currently in progress.
            else if (CurrentState == State.OcarinaTeleport)
            {
                // Show the animation while teleporting.
                CurrentState = State.Idle;
            }
        }

        public void StartOcarinaDuo()
        {
            CurrentState = State.Ocarina;

            _ocarinaNoteIndex = 0;
            _ocarinaCounter = 0;

            Animation.Play("ocarina_duo");
        }

        public void StopOcarinaDuo()
        {
            CurrentState = State.Idle;
        }

        private void UpdateOcarinaAnimation()
        {
            if (CurrentState != State.Ocarina)
                return;

            _ocarinaCounter += Game1.DeltaTime;
            if (_ocarinaCounter > 100 + _ocarinaNoteIndex * 910)
            {
                _ocarinaNoteIndex++;

                var dir = _ocarinaNoteIndex % 2 == 1 ? -1 : 1;
                var objNote = new ObjNote(Map, new Vector2(EntityPosition.X + dir * 7, EntityPosition.Y), dir);
                Map.Objects.SpawnObject(objNote);
            }
        }

        private void FinishedOcarinaSong()
        {
            // Set the timer to make damage happen again.
            PreventDamageTimer = 200;

            // Unfreeze the game world when the song is finished.
            FreezeAnimations(false);

            // Continue playing the music.
            if (_ocarinaSong != 1)
                Game1.GbsPlayer.Play();

            // Bad ocarina song was played.
            if (_ocarinaSong == -1)
            {
                ReturnToIdle();
                Game1.GameManager.StartDialogPath("ocarina_bad");
                return;
            }
            // Manbo's Mambo was played.
            if (_ocarinaSong == 1)
            {
                // The value for MapTeleport must be 2 or 3 to use the map to warp with Manbo's song.
                if (GameSettings.MapTeleport >= 2 && !Map.DungeonMode && !Map.DungeonEgg)
                {
                    // Open a new instance of the map overlay and set the flag 'ManboTeleport' that signifies it was an ocarina warp.
                    ManboTeleport = true;
                    Game1.GameManager.InGameOverlay.StartSequence("map");
                    Game1.GbsPlayer.Play();
                    ReturnToIdle();
                    return;
                }
                // Freeze the game during the transition.
                FreezeAnimations(true);

                CurrentState = State.OcarinaTeleport;
                MapTransitionStart = EntityPosition.Position;
                MapTransitionEnd = EntityPosition.Position;
                TransitionOutWalking = false;

                Game1.GameManager.PlaySoundEffect("D360-44-2C");

                // load the map
                var transitionSystem = (MapTransitionSystem)Game1.GameManager.GameSystems[typeof(MapTransitionSystem)];
                transitionSystem.ResetTransition();

                if (Map.DungeonMode || Map.DungeonEgg)
                {
                    // HACK: If the player used the warp above level 8 and entered the dungeon, the save position is set to the warp
                    // rather than the dungeon 8 entrance. So if the last position is the warp, overwrite it with dungeon 8 entrance.
                    if (SavePosition == new Vector2(280, 102) && SaveMap == "overworld.map")
                    {
                        SavePosition = new Vector2(576, 1028);
                        SaveMap = "dungeon8.map";
                    }
                    // Respawn at the dungeon entrance.
                    SetNextMapPosition(SavePosition);
                    transitionSystem.AppendMapChange(SaveMap, null, false, false, Color.White, true);
                    OcarinaDungeonTeleport = true;
                }
                else
                {
                    // Append a map change.
                    transitionSystem.AppendMapChange("overworld.map", "ocarina_entry", false, false, Color.White, true);
                }
                transitionSystem.StartTeleportTransition = true;
                return;
            }
            ReturnToIdle();

            // Update Ocarina Listeners.
            var recDetection = new RectangleF(EntityPosition.X - 100, EntityPosition.Y - 100, 200, 200);

            // Get objects around Link to see if they have ocarina listeners.
            _ocarinaList.Clear();
            Map.Objects.GetComponentList(_ocarinaList, (int)recDetection.X, (int)recDetection.Y, (int)recDetection.Width, (int)recDetection.Height, OcarinaListenerComponent.Mask);

            // Loop through all objects found.
            foreach (var objOcarinaListener in _ocarinaList)
            {
                var ocarinaComponent = (OcarinaListenerComponent)objOcarinaListener.Components[OcarinaListenerComponent.Index];

                // Compute the world-space rectangle for this listener's interaction zone.
                var recInteraction = new RectangleF(
                    objOcarinaListener.EntityPosition.X + ocarinaComponent.InteractRect.X,
                    objOcarinaListener.EntityPosition.Y + ocarinaComponent.InteractRect.Y,
                    ocarinaComponent.InteractRect.Width,
                    ocarinaComponent.InteractRect.Height);

                // Check if player is inside the interaction rectangle.
                if (recInteraction.Contains(EntityPosition.Position))
                {
                    ocarinaComponent.OcarinaPlayedFunction?.Invoke(Game1.GameManager.SelectedOcarinaSong);
                }
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  PEGASUS BOOTS CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UsePegasusBoots()
        {
            if (!_bootsHolding & _bootsRunning)
                _bootsStop = true;
        }

        private void HoldPegasusBoots()
        {
            if (CurrentState == State.BootKnockback || _isTrapped)
                return;

            _bootsHolding = true;
        }

        private Box GetCrystalSmashBox()
        {
            // The crystal smash box is used to smash crystals. Use
            // the current direction to determine the box offsets.
            var key = Direction;
            var offsets = key switch
            {
                1 => (  -7, -16, +14,  +5),
                2 => (  +5, -12,  +5, +14),
                3 => (  -7,  +1, +14,  +5),
                _ => ( -10, -12,  +5, +14)
            };
            // Assign the results of the switch.
            var (xOff, yOff, wOff, hOff) = offsets;

            // Return the box used to smash crystals with.
            return new Box(
                EntityPosition.X + xOff,
                EntityPosition.Y + yOff, 4,
                wOff, hOff, 4);
        }

        private void UpdatePegasusBoots()
        {
            _bootsWasRunning = _bootsRunning;
            if ((CurrentState != State.Blocking && CurrentState != State.Idle) || _isClimbing || Map.Is2dMap && Direction % 2 != 0)
            {
                _bootsHolding = false;
                _bootsRunning = false;
                _bootsCounter = 0;
                return;
            }
            // stop running but start charging with a time boost
            if (_bootsStop && _body.Velocity.Length() < 0.25f)
            {
                _bootsStop = false;
                _bootsRunning = false;

                // Over/equals 500 = subtract 300. Above zero = halve it. At 0 = use value.
                _bootsCounter = boots_charge_time >= 500
                    ? boots_charge_time - 300
                    : boots_charge_time > 0
                        ? boots_charge_time / 2
                        : boots_charge_time;
            }
            if (_bootsHolding || _bootsRunning)
            {
                var lastCounter = _bootsCounter;
                _bootsCounter += Game1.DeltaTime;

                // Spawn particles: dust or water particles.
                if (_bootsCounter % _bootsParticleTime < lastCounter % _bootsParticleTime)
                {
                    // Water splash particles.
                    if (_body.CurrentFieldState.HasFlag(MapStates.FieldStates.Water))
                    {
                        Game1.GameManager.PlaySoundEffect("D360-14-0E");

                        var splashAnimator = new ObjAnimator(_body.Owner.Map, 0, 0, 0, 3, 1, "Particles/splash", "idle", true);
                        splashAnimator.EntityPosition.Set(new Vector2(
                            _body.Position.X + _body.OffsetX + _body.Width / 2f,
                            _body.Position.Y + _body.OffsetY + _body.Height - _body.Position.Z - 3));
                        Map.Objects.SpawnObject(splashAnimator);
                    }
                    // Ground dust particles.
                    else
                    {
                        Game1.GameManager.PlaySoundEffect("D378-07-07");

                        var animator = new ObjAnimator(Map, (int)EntityPosition.X, (int)(EntityPosition.Y + 1),
                            0, -1 - (int)EntityPosition.Z, Values.LayerPlayer, "Particles/run", "spawn", true);
                        Map.Objects.SpawnObject(animator);
                    }
                }
                // Start running when the counter exceeds the charge time.
                if (!_bootsRunning && _bootsCounter > boots_charge_time)
                {
                    _bootsLastDirection = Direction;
                    _bootsRunning = true;
                    _bootsWasRunning = true;
                    _bootsStop = false;
                }
                // Spawn the smash box while running.
                if (_bootsRunning)
                    _crystalSmashBox = GetCrystalSmashBox();
            }
            else
            {
                _crystalSmashBox = Box.Empty;
                _bootsCounter = 0;
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  ITEM SHOP CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateStoreItemPosition(CPosition position)
        {
            _storePickupPosition.X = position.X - _storeItemWidth / 2f;
            _storePickupPosition.Y = position.Y - EntityPosition.Z - 14 - _storeItemHeight;
        }

        public void StartHoldingItem(GameItem item)
        {
            CurrentState = State.CarryingItem;

            StoreItem = item;

            _storeItemWidth = item.SourceRectangle.Value.Width;
            _storeItemHeight = item.SourceRectangle.Value.Height;

            EntityPosition.AddPositionListener(typeof(ObjLink), UpdateStoreItemPosition);
            UpdateStoreItemPosition(EntityPosition);

            Game1.GameManager.SaveManager.SetString("holdItem", "1");
        }

        public void StopHoldingItem()
        {
            CurrentState = State.Idle;

            StoreItem = null;

            // this removes all listeners with the ObjLink as a key
            EntityPosition.PositionChangedDict.Remove(typeof(ObjLink));

            Game1.GameManager.SaveManager.SetString("holdItem", "0");
        }

        private void StealItem()
        {
            StopHoldingItem();

            // used in ObjStoreItem to not return the item to the shelf
            Game1.GameManager.SaveManager.SetString("result", "0");

            // Rename the player to "Thief".
            Game1.GameManager.ThiefState = true;

            // add the item to the inventory
            var strItem = Game1.GameManager.SaveManager.GetString("itemShopItem");
            var strCount = Game1.GameManager.SaveManager.GetString("itemShopCount");

            var item = new GameItemCollected(strItem)
            {
                Count = int.Parse(strCount)
            };
            // gets picked up
            PickUpItem(item, false, false);

            Game1.GameManager.SaveManager.SetString("stoleItem", "1");
            _showStealMessage = true;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  FOLLOWER CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateFollower(bool mapInit)
        {
            var hasFollower = false;

            // Check if marin is following the player.
            var itemMarin = Game1.GameManager.GetItem("marin");
            if (itemMarin != null && itemMarin.Count > 0)
            {
                // For some reason, the "ObjFollower" is always null after a map transition. Most of the time, we can just safely respawn Marin
                // so this isn't a problem. This code is also what works around the beach issue where she doesn't spawn. But, it doesn't work if
                // leaving a dungeon and she's currently waiting for Link to come out. So in that case, don't run this code, use the backup path.
                if (ObjFollower == null && !_objMaria._dungeonLeaveSequence)
                    SpawnMarin();
                else
                    ObjFollower = _objMaria;

                hasFollower = true;
            }

            // Check if the rooster is following the player.
            var itemRooster = Game1.GameManager.GetItem("rooster");
            if (itemRooster != null && itemRooster.Count > 0)
            {
                ObjFollower = _objRooster;
                hasFollower = true;
            }

            // Check if the ghost is following the player.
            var itemGhost = Game1.GameManager.GetItem("ghost");
            if (itemGhost != null && itemGhost.Count > 0)
            {
                ObjFollower = _objGhost;
                hasFollower = true;
            }
            // Check if there is a follower and it is already spawned.
            if (hasFollower)
            {
                if (ObjFollower.Map != Map)
                {
                    if (mapInit && NextMapPositionStart.HasValue)
                        ObjFollower.EntityPosition.Set(NextMapPositionStart.Value);
                    else
                        ObjFollower.EntityPosition.Set(EntityPosition.Position);

                    ObjFollower.Map = Map;
                    Map.Objects.SpawnObject(ObjFollower);
                }
                Map.Objects.RegisterAlwaysAnimateObject(ObjFollower);
            }
            // Remove the current follower from the map.
            else if (ObjFollower != null)
            {
                Map.Objects.DeleteObjects.Add(ObjFollower);
                ObjFollower = null;
            }
        }

        private static Vector2 GetMarinSpawnOffset(int direction, float distance)
        {
            return direction switch
            {
                0 => new Vector2(distance, 0),
                1 => new Vector2(0, distance),
                2 => new Vector2(-distance, 0),
                3 => new Vector2(0, -distance),
                _ => Vector2.Zero
            };
        }

        private void SpawnMarin()
        {
            Vector2 offset = GetMarinSpawnOffset(Direction, 13f);
            Vector2 marinSpawnPos = new Vector2(_body.Position.X, _body.Position.Y) + offset;
            _objMaria = new ObjMarin(Map, (int)EntityPosition.X, (int)EntityPosition.Y);
            ObjFollower = _objMaria;
            Map.Objects.SpawnObject(_objMaria);
            Map.Objects.RegisterAlwaysAnimateObject(_objMaria);
            _objMaria.SetPosition(marinSpawnPos);
            _objMaria.SetFacingDirection(Direction);
        }

        private void UpdateGhostSpawn()
        {
            if (!_spawnGhost || !Map.IsOverworld)
                return;

            var dungeonEntryPosition = new Vector2(1840, 272);
            var distance = Position - dungeonEntryPosition;
            if (MathF.Abs(distance.X) > 512 || MathF.Abs(distance.Y) > 256)
            {
                _spawnGhost = false;
                Game1.GameManager.SaveManager.RemoveString(_spawnGhostKey);
                Game1.GameManager.CollectItem(new GameItemCollected("ghost") { Count = 1 }, 0);
                UpdateFollower(false);
                _objGhost.StartFollowing();
            }
        }

        public void StartFlying(ObjCock objCock)
        {
            _isFlying = true;
            _wasFlying = false;
            _objRooster = objCock;
            _flyStartZPos = MathF.Truncate(EntityPosition.Z);
        }

        public void StopFlying(Vector2 velocity)
        {
            _isFlying = false;
            _wasFlying = true;

            _body.IgnoresZ = false;
            _body.IsGrounded = false;
            _body.JumpStartHeight = 0;

            _flyStartZPos = 0;
            _lastMoveVelocity = Vector2.Zero;

            if (_objRooster != null)
                _objRooster.StopFlying();

            _body.Velocity = new Vector3(velocity.X * 4, velocity.Y * 4, 0);
        }

        private void UpdateFlying()
        {
            // Player is currently carrying the rooster around.
            if (IsFlying())
            {
                // The hit velocity is added to the movement (*3) for the flame trap knockback on the way 
                // to level 8 as the normal value sent back is not strong enough to knock it back.
                var moveVelocity = ControlHandler.GetMoveVector2() + (_hitVelocity * 3);

                var moveVelocityLength = moveVelocity.Length();
                if (moveVelocityLength > 1)
                    moveVelocity.Normalize();

                if (moveVelocityLength > 0)
                {
                    _objRooster.TargetVelocity(moveVelocity, 0.5f, Direction);
                    var vectorDirection = ToDirection(moveVelocity);
                    Direction = vectorDirection;
                }
            }

            // The player hit the ground or water after throwing the rooster.
            if (CurrentState == State.Idle && _body.IsGrounded && _wasFlying)
            {
                // Check if the player is over water.
                var fieldState = SystemBody.GetFieldState(_body);

                // Reduce velocity by 50% hitting water. Reduce it to zero when hitting land.
                if (fieldState.HasFlag(MapStates.FieldStates.DeepWater))
                    _body.Velocity = _body.Velocity * 0.5f;
                else
                    _body.Velocity = Vector3.Zero;

                _wasFlying = false;
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  MAP CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public void UpdateSaveLocation()
        {
            MapManager.ObjLink.SaveMap = Map.MapName;
            MapManager.ObjLink.SavePosition = EntityPosition.Position;
            MapManager.ObjLink.SaveDirection = Direction;
        }

        public void StartIntro()
        {
            // set the music
            Game1.GameManager.SetMusic(27, 2);

            CurrentState = State.Intro;

            Animation.Play("intro");

            NextMapPositionStart = null;
            NextMapPositionEnd = null;
            SetPosition(new Vector2(56, 51));
            MapManager.Camera.ForceUpdate(Game1.GameManager.MapManager.GetCameraTarget());

            MapManager.ObjLink.SaveMap = Map.MapName;
            MapManager.ObjLink.SavePosition = new Vector2(70, 70);
            MapManager.ObjLink.SaveDirection = 3;
        }

        private void UpdateIntro()
        {
            if (CurrentState == State.Intro)
            {
                var walkVelocity = ControlHandler.GetMoveVector2();

                Game1.GameManager.InGameOverlay.DisableInventoryToggle = true;

                if (Animation.CurrentAnimation.Id == "intro_sit" &&
                    !Game1.GameManager.InGameOverlay.TextboxOverlay.IsOpen && walkVelocity.Length() > 0)
                {
                    CurrentState = State.Idle;
                    Direction = 2;
                    StartRailJump(EntityPosition.Position + new Vector2(12, 4), 1, 1);
                    Animation.Play("intro_jump");

                    Game1.GameManager.SaveManager.SetString("played_intro", "1");
                }
                return;
            }
        }

        public void InitGame()
        {
            Animation.Play((CarryShield ? "stands_" : "stand_") + Direction);
            SpriteTransparency = 1;

            NextMapFallStart = false;
            NextMapFallRotateStart = false;

            Game1.GameManager.SwordLevel = 0;
            Game1.GameManager.ShieldLevel = 0;
            Game1.GameManager.StoneGrabberLevel = 0;
            Game1.GameManager.SelectedOcarinaSong = -1;
            Game1.GameManager.OcarinaSongs[0] = 0;
            Game1.GameManager.OcarinaSongs[1] = 0;
            Game1.GameManager.OcarinaSongs[2] = 0;
            Game1.GameManager.HasMagnifyingLens = false;

            _spawnGhost = false;
            HasFlippers = false;
            StoreItem = null;

            _body.IsActive = true;
            _forceWalking = false;

            _objMaria = new ObjMarin(Map, 0, 0);
            _objRooster = new ObjCock(Map, 0, 0, null);
            _objGhost = new ObjGhost(Map, 0, 0);

            MapInit();

            CurrentState = State.Idle;

            Game1.InProgress = true;
        }

        public void MapInit()
        {
            if (!IsSwimmingState() &&
                CurrentState != State.OcarinaTeleport)
                CurrentState = State.Idle;

            Boomerang.Reset();
            Hookshot.Reset();

            _hookshotPull = false;

            _railJump = false;
            IsVisible = true;

            _isRafting = false;
            _isFlying = false;
            _wasFlying = false;

            _isClimbing = false;

            _isTrapped = false;
            _shadowComponent.IsActive = true;

            _isGrabbed = false;

            ShowItem = null;
            _collectedShowItem = null;
            ObjFollower = null;

            _hitRepelTime = 0;
            _hitParticleTime = 0;

            _hitCount = 0;
            _sprite.SpriteShader = null;

            _moveVelocity = Vector2.Zero;
            _lastMoveVelocity = Vector2.Zero;
            _hitVelocity = Vector2.Zero;
            _body.Velocity = Vector3.Zero;

            _body.IgnoreHeight = false;
            _body.IgnoreHoles = false;
            _body.DeepWaterOffset = -3;
            _body.Level = 0;
            _body.IsGrounded = true;

            _bootsHolding = false;
            _bootsRunning = false;
            _bootsCounter = 0;

            _carriedGameObject = null;
            _carriedComponent = null;
            _carriedObjDrawComp = null;

            _drawInstrumentEffect = false;

            _diveCounter = 0;
            _swimVelocity = Vector2.Zero;

            PreventDamageTimer = 0;

            if (NextMapFallStart)
            {
                EntityPosition.Z = 64;

                _body.Velocity.Z = -3.75f;
                _body.IgnoresZ = false;
                _body.JumpStartHeight = EntityPosition.Z;

                NextMapFallStart = false;
            }

            if (NextMapFallRotateStart)
            {
                EntityPosition.Z = 160;

                _body.Velocity.Z = -3.75f;
                _body.IgnoresZ = false;
                _body.IsGrounded = false;
                _body.JumpStartHeight = EntityPosition.Z;

                _fallEntryCounter = 0;
                CurrentState = State.FallRotateEntry;

                NextMapFallRotateStart = false;
            }

            if (NextMapPositionEnd.HasValue)
                SetHoleResetPosition(new Vector3(NextMapPositionEnd.Value.X, NextMapPositionEnd.Value.Y, 0));

            if (Is2DMode)
                MapInit2D();

            // Stop Guardian Acorn and Piece of Power during certain transitions.
            if (Map != null && _previousMap != null)
            {
                bool isOverworld = Map.IsOverworld || _previousMap.IsOverworld;
                bool mapIsCave = Map.IsCave || _previousMap.IsCave;
                bool mapIsDungeon = Map.DungeonMode || Map.DungeonCastle || Map.DungeonEgg;

                if (isOverworld || !mapIsCave && !mapIsDungeon)
                {
                    Game1.GameManager.StopGuardianAcorn();
                    Game1.GameManager.StopPieceOfPower();
                }
            }
            // The BowWow object is designed to automatically set to "_objBowWow" so it needs to be
            // terminated when it is not supposed to be in use or we get an invisible BowWow following.
            if ((Map != null && Map.DungeonMode) || Game1.GameManager.SaveManager.GetString("has_bowWow", "0") != "1")
                _objBowWow = null;

            Game1.GameManager.UseShockEffect = false;
        }

        public void FinishLoadingMap(Map.Map map)
        {
            Map = map;
            Is2DMode = map.Is2dMap;
            _body.EnableStepUp = Is2DMode;

            if (NextMapPositionStart.HasValue)
                SetPosition(NextMapPositionStart.Value);

            MapInit();

            UpdateFollower(true);

            if (ObjFollower != null)
                ObjFollower.EntityPosition.Set(NextMapPositionStart.Value);

            if (_spriteShadow != null)
                _spriteShadow.EntityPosition.Set(NextMapPositionStart.Value);
        }
        
        public void Respawn()
        {
            Animation.Play((CarryShield ? "stands_" : "stand_") + Direction);

            StoreItem = null;
            _body.IsActive = true;

            var hearts = 3;
            if (Game1.GameManager.MaxHearts >= 14)
                hearts = 10;
            else if (Game1.GameManager.MaxHearts >= 10)
                hearts = 7;
            else if (Game1.GameManager.MaxHearts >= 6)
                hearts = 5;

            Game1.GameManager.CurrentHealth = hearts * 4;
            Game1.GameManager.DeathCount++;

            MapInit();
        }

        public void SetPosition(Vector2 newPosition)
        {
            _body.VelocityTarget = Vector2.Zero;
            EntityPosition.Set(new Vector2(newPosition.X, newPosition.Y));
        }

        public void FreezePlayer()
        {
            UpdatePlayer = false;
            _isWalking = false;
            _bootsRunning = false;
            _bootsHolding = false;

            // stop movement
            // on the boat the player should still move up/down while playing the sequence
            if (Map != null && !Map.Is2dMap)
            {
                // make sure to fall down when jumping into a game sequence
                _body.Velocity.X = 0;
                _body.Velocity.Y = 0;
                if (IsJumpingState() ||
                    CurrentState == State.Powdering)
                    CurrentState = State.Idle;
            }
            _body.VelocityTarget = Vector2.Zero;
            _moveVelocity = Vector2.Zero;
            _hitVelocity = Vector2.Zero;
            _swimVelocity = Vector2.Zero;

            // stop push animation
            if (CurrentState == State.Pushing)
                CurrentState = State.Idle;

            if (Map != null && Map.Is2dMap)
                UpdateAnimation2D();
            else
                UpdateAnimation();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  TELEPORT TRANSITION CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateTeleporting()
        {
            if (CurrentState == State.Teleporting)
            {
                if (_teleportCounterFull < 1250 || Direction <= 2)
                    _teleportCounter += Game1.DeltaTime;

                _teleportCounterFull += Game1.DeltaTime;
                var rotationSpeed = 150 - (float)Math.Sin((_teleportCounterFull / 2000f) * Math.PI) * 50;
                if (_teleportCounter > rotationSpeed)
                {
                    _teleportCounter -= rotationSpeed;
                    Direction = (Direction + 1) % 4;
                    UpdateAnimation();
                }
                var transitionSystem = (MapTransitionSystem)Game1.GameManager.GameSystems[typeof(MapTransitionSystem)];
                transitionSystem.ResetTransition();

                if (_teleportState == 0 && _teleportCounterFull >= 1250)
                {
                    if (_teleporter != null)
                    {
                        _teleportState = 1;

                        EntityPosition.Set(_teleporter.TeleportPosition);
                        _teleporter.Lock();

                        var goalPosition = Game1.GameManager.MapManager.GetCameraTarget();
                        MapManager.Camera.SoftUpdate(goalPosition);
                    }
                    else if (Direction == 3 && _teleportCounterFull >= 1450)
                    {
                        MapTransitionStart = EntityPosition.Position;
                        MapTransitionEnd = EntityPosition.Position;
                        TransitionOutWalking = false;

                        transitionSystem.AppendMapChange(_teleportMap, _teleporterId, false, true, Color.White, true);
                    }
                    transitionSystem.SetColorMode(Color.White, 1);
                }
                var fadeOutTime = 250.0f;
                var fadeoutStart = 1750;
                var fadeoutEnd = 1750 + fadeOutTime;

                // Teleport fade in.
                if (_teleportCounterFull >= 750 && _teleportCounterFull < 1250)
                {
                    transitionSystem.SetColorMode(Color.White, (_teleportCounterFull - 750) / 500f);
                }
                // Teleport fade out.
                else if (_teleportState == 1 && _teleportCounterFull >= fadeoutStart && _teleportCounterFull < fadeoutEnd)
                {
                    transitionSystem.SetColorMode(Color.White, 1 - (_teleportCounterFull - fadeoutStart) / fadeOutTime);
                }
                // Teleport has finished.
                else if (_teleportState == 1 && _teleportCounterFull >= fadeoutEnd)
                {
                    _drawBody.Layer = Values.LayerPlayer;
                    transitionSystem.SetColorMode(Color.White, 0);
                    CurrentState = State.Idle;
                    Camera.SnapCamera = false;
                }
            }
        }

        public void StartTeleportation(ObjDungeonTeleporter teleporter)
        {
            _teleporter = teleporter;

            CurrentState = State.Teleporting;
            _drawBody.Layer = Values.LayerTop;

            _teleportState = 0;
            _teleportCounter = 0;
            _teleportCounterFull = 0;

            ReleaseCarriedObject();

            if (Camera.ClassicMode)
                Camera.SnapCamera = true;
        }

        public void StartTeleportation(string teleportMap, string teleporterId)
        {
            _teleporter = null;

            CurrentState = State.Teleporting;
            _drawBody.Layer = Values.LayerTop;

            _teleportMap = teleportMap;
            _teleporterId = teleporterId;
            _teleportState = 0;
            _teleportCounter = 0;
            _teleportCounterFull = 0;

            ReleaseCarriedObject();

            if (Camera.ClassicMode)
                Camera.SnapCamera = true;
        }

        public void StartWorldTelportation(Vector2 newPosition)
        {
            CurrentState = State.TeleportFallWait;

            var positionDistance = EntityPosition.Position - newPosition;
            var fallPositionV2 = new Vector2(newPosition.X, newPosition.Y);
            var fallPositionV3 = new Vector3(newPosition.X, newPosition.Y, 128);
            EntityPosition.Set(fallPositionV3);

            HoleFalling = false;

            if (ObjFollower != null)
            {
                var itemGhost = Game1.GameManager.GetItem("ghost");
                if (itemGhost != null && itemGhost.Count >= 0)
                    ObjFollower.EntityPosition.Set(fallPositionV2);
                else
                    ObjFollower.EntityPosition.Set(fallPositionV3);
            }
            if (_objBowWow != null)
                _objBowWow.EntityPosition.Set(fallPositionV3);

            if (_spriteShadow != null)
                _spriteShadow.EntityPosition.Set(fallPositionV2);

            // Only jump to the new position if it is a different teleporter at a different location.
            if (!Camera.ClassicMode && positionDistance.Length() > 64)
                MapManager.Camera.ForceUpdate(Game1.GameManager.MapManager.GetCameraTarget());
            else
                Camera.SnapCamera = true;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  MAP TRANSITION CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public void SetNextMapPosition(Vector2 playerPosition)
        {
            // this will be used to set the position of the player after loading the map
            // one of them should always be null
            // the playerPosition is used after loading a savestate
            NextMapPositionStart = playerPosition;
            NextMapPositionEnd = playerPosition;
            NextMapPositionId = null;
        }

        public void SetNextMapPosition(string nextMapPositionId)
        {
            // this will be used to set the position of the player after loading the map
            // one of them should always be null
            // the nextMapPositionId is used after going though a door
            NextMapPositionId = nextMapPositionId;
            NextMapPositionStart = null;
            NextMapPositionEnd = null;
        }

        public void OnAppendMapChange()
        {
            if (_objMaria != null)
                _objMaria.OnAppendMapChange();
        }

        public void StartTransitioning()
        {
            // Store that a transition is taking place.
            IsTransitioning = true;

            // Interesting bug: when repelled into a staircase, it's possible to get stuck
            // in a loop of entering/exiting until the velocity fully decays.
            CancelRepelVelocities(false);

            // Set the previous map and draw Link over everything.
            _previousMap = Map;
            _drawBody.Layer = Values.LayerTop;

            // Force walking and stop boots running.
            _isWalking = true;
            _bootsRunning = false;

            // The player shouldn't take damage while transitioning.
            PreventDamageTimer = 3000;

            // If the player stole an item.
            if (StoreItem != null)
                StealItem();

            // Drop any items if being carried.
            ReleaseCarriedObject();

            // If Link is flying with the rooster then stop flying.
            if (MapManager.ObjLink.IsFlying())
                MapManager.ObjLink.StopFlying(Vector2.Zero);

            // Restore current state to "Idle".
            if (MapTransitionStart.HasValue && MapTransitionEnd.HasValue && !IsSwimmingState() && 
                CurrentState != State.BedTransition && CurrentState != State.Knockout && CurrentState != State.OcarinaTeleport) 
            {   
                CurrentState = State.Idle;
            }
            // Remove the player's velocity.
            _body.VelocityTarget = Vector2.Zero;

            // If it's a 2D map set up additional parameters.
            if (Map.Is2dMap)
            {
                if (_ladderCollision)
                {
                    _isClimbing = true;
                    Direction = 1;
                }
                _body.IgnoresZ = true;
                _body.Velocity.Y = 0.0f;
            }
            else
            {
                _body.Velocity = Vector3.Zero;
            }
        }

        public void UpdateMapTransitionOut(float state)
        {
            // Move Link to the new position on the map.
            if (MapTransitionStart.HasValue && MapTransitionEnd.HasValue)
            {
                var newPosition = Vector2.Lerp(MapTransitionStart.Value, MapTransitionEnd.Value, state);
                SetPosition(newPosition);
            }
            // Recalculate scale when certain camera options are enabled.
            bool classicDungeon = GameSettings.ClassicCamera && GameSettings.ClassicDungeon;
            bool modernOverworld = !GameSettings.ClassicCamera && GameSettings.ModernOverworld;

            // Apply the scaling change to the camera.
            if (classicDungeon || modernOverworld)
                Game1.ScaleChanged = true;

            // Lock the camera while transitioning.
            if (!Map.Is2dMap || Direction == 1)
                Game1.GameManager.MapManager.UpdateCameraY = MapTransitionStart == MapTransitionEnd;

            // Jump fix hack can mess up walking animation so cancel it out.
            _jumpEndTimer = 0;
            _isWalking = TransitionOutWalking;

            // Force north facing when climbing down a ladder into a
            // door. No clue what the proper way to fix this would be.
            if (Map.Is2dMap && _isClimbing)
                Direction = 1;

            // Disable hole falling logic.
            HoleFalling = false;

            // Force animation updates since Link is being moved incrementally.
            if (Is2DMode)
                UpdateAnimation2D();
            else
                UpdateAnimation();
        }

        public void SetFollowerMapState()
        {
            // Disable followers on maps that contain the "NoFollowers" map object.
            if (Map.NoFollowers || Map.Is2dMap)
                ObjFollower.IsActive = false;

            // Marin has her own method of respawning. Not doing it this way breaks her dungeon transition.
            else
                if (ObjFollower != _objMaria)
                    ObjFollower.IsActive = true;
        }

        public void UpdateMapTransitionIn(float state)
        {
            // Check if the transition state is "state 0".
            if (state == 0)
            {
                // Make sure to not start falling while transitioning into a 2d map with a ladder.
                if (Map.Is2dMap)
                    _body.IgnoresZ = true;
            }
            if (DirectionEntry >= 0)
                Direction = DirectionEntry;

            // Make sure the transition has both a start and end position.
            if (NextMapPositionStart.HasValue && NextMapPositionEnd.HasValue)
            {
                var newPosition = Vector2.Lerp(NextMapPositionStart.Value, NextMapPositionEnd.Value, state);
                SetPosition(newPosition);

                // Transition the follower out of the map. I'm not sure why this check existed, but the commented off code used to be part of the check.
                if (ObjFollower != null) // && NextMapPositionStart.Value != NextMapPositionEnd.Value)
                {
                    var followerPosition = Vector2.Lerp(NextMapPositionStart.Value, NextMapPositionEnd.Value, state * 0.5f);
                    ObjFollower.SetPosition(followerPosition);
                    SetFollowerMapState();
                }
            }
            // Lock the camera while transitioning.
            if (!Map.Is2dMap || Direction == 1)
                Game1.GameManager.MapManager.UpdateCameraY = NextMapPositionStart == NextMapPositionEnd;

            // Keep Link walking during the transition in.
            if (!Map.Is2dMap)
                _isWalking = TransitionOutWalking;

            // Set the hole and water reset position to be at the transition entrance.
            _holeResetPosition = EntityPosition.ToVector3();
            _drownResetPosition = EntityPosition.Position;

            UpdateSwimmingPartOne();

            UpdateIgnoresZ();

            if (Is2DMode)
                UpdateAnimation2D();
            else
                UpdateAnimation();
        }

        public void EndTransitioning()
        {
            // Transition is over.
            IsTransitioning = false;

            // Force block button released.
            _blockButton = false;

            // I'm not sure why this is set to zero but here it is.
            _body.HoleAbsorption = Vector2.Zero;

            // The player can take damage again.
            PreventDamageTimer = 0;

            // If using Manbo's song in a dungeon, force the player to face north.
            if (OcarinaDungeonTeleport)
            {
                Direction = 1;
                Animation.Play("stand_" + Direction);
                OcarinaDungeonTeleport = false;
            }
            if (!Map.Is2dMap)
            {
                _body.Velocity.X = 0;
                _body.Velocity.Y = 0;
            }
            // This is because the water is deeper than 0.
            if ((SystemBody.GetFieldState(_body) & MapStates.FieldStates.DeepWater) == 0 && CurrentState != State.Swimming && !_isClimbing)
                _body.IgnoresZ = false;

            // Restore the player's draw layer.
            _drawBody.Layer = Values.LayerPlayer;

            // Restore the camera following the player.
            MapManager.Camera.CameraFollowMultiplier = 1.0f;

            // Used solely to show the message after the player steals from the shop.
            if (_showStealMessage)
            {
                _showStealMessage = false;
                Game1.GameManager.StartDialogPath("shopkeeper_steal");
            }
            // Restart the music.
            if (!GameSettings.MutePowerups && (Game1.GameManager.PieceOfPowerIsActive || Game1.GameManager.GuardianAcornIsActive))
                Game1.GameManager.StartPowerupMusic(1);

            // Destroy the field barrier after a transition so it can be recreated.
            DestroyFieldBarrier();

            // Manbo's song transition can freeze the game so unfreeze it now.
            FreezeAnimations(false);

            // When classic camera is enabled don't reset objects immediately after transition. 
            if (Camera.ClassicMode)
            {
                PreventReset = true;
                PreventResetTimer = 200f;
            }
            // Disable black screen override for modern camera.
            else
                BlackScreenOverride = false;
        }
    }
}
