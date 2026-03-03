using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.GameObjects.Base.Components.AI;
using ProjectZ.InGame.GameObjects.Things;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Enemies
{
    internal class EnemyDarknut : GameObject
    {
        private readonly EnemyDarknutSword _sword;
        private readonly Animator _animator;
        private readonly AiComponent _aiComponent;
        private readonly AiDamageState _damageState;
        private readonly CSprite _sprite;
        private readonly DamageFieldComponent _damageField;
        private readonly HittableComponent _hitComponent;
        private readonly PushableComponent _pushComponent;

        private const string _leafSaveKey = "ow_goldLeafNut";
        private const float MoveSpeed = 0.5f;
        private const float AttackSpeed = 0.55f;
        private const int AttackRange = 50;
        private const int FollowRange = 65;

        private Rectangle _fieldRectangle;
        private int _direction;
        private int _lives = EnemyLives.Darknut;
        private bool _isActive = true;

        public bool FinishedSpawning = true;
        public bool SpawnGoldLeaf;

        public BodyComponent Body;
        public int Direction => _direction;
        public string AiState { get => _aiComponent.CurrentStateId; }
        public CBox HittableBox
        {
            get => _hitComponent.HittableBox;
            set => _hitComponent.HittableBox = value;
        }
        public override bool IsActive
        {
            set
            {
                _isActive = value;
                _sword.IsActive = value;
            }
            get => _isActive;
        }

        public EnemyDarknut() : base("darknut") { }

        public EnemyDarknut(Map.Map map, int posX, int posY) : base(map)
        {
            Tags = Values.GameObjectTag.Enemy;

            EntityPosition = new CPosition(posX + 8, posY + 16, 0);
            ResetPosition  = new CPosition(posX + 8, posY + 16, 0);
            EntitySize = new Rectangle(-8, -16, 16, 16);
            CanReset = true;
            OnReset = Reset;

            _animator = AnimatorSaveLoad.LoadAnimator("Enemies/darknut");
            _animator.Play("walk_1");

            _sprite = new CSprite(EntityPosition);
            var animationComponent = new AnimationComponent(_animator, _sprite, new Vector2(-8, -16));

            _fieldRectangle = map.GetField(posX, posY);

            Body = new BodyComponent(EntityPosition, -6, -12, 12, 12, 8)
            {
                MoveCollision = OnCollision,
                CollisionTypes = Values.CollisionTypes.Normal |
                                 Values.CollisionTypes.Field |
                                 Values.CollisionTypes.Enemy,
                AvoidTypes =     Values.CollisionTypes.Hole |
                                 Values.CollisionTypes.NPCWall,
                FieldRectangle = _fieldRectangle,
                Bounciness = 0.25f,
                AbsorbPercentage = 0.75f,
                Drag = 0.85f
            };

            var stateSpawned = new AiState();
            stateSpawned.Trigger.Add(new AiTriggerCountdown(550, null, EndWallSpawn));
            var stateIdle = new AiState { Init = InitIdle };
            stateIdle.Trigger.Add(new AiTriggerRandomTime(EndIdle, 300, 500));
            var stateWalk = new AiState { Init = InitWalking };
            stateWalk.Trigger.Add(new AiTriggerRandomTime(() => _aiComponent.ChangeState("idle"), 550, 850));
            var stateAttack = new AiState(StateAttack);

            _aiComponent = new AiComponent();
            _aiComponent.Trigger.Add(new AiTriggerUpdate(UpdateDamageTick));
            _aiComponent.States.Add("spawned", stateSpawned);
            _aiComponent.States.Add("idle", stateIdle);
            _aiComponent.States.Add("walking", stateWalk);
            _aiComponent.States.Add("attack", stateAttack);

            new AiFallState(_aiComponent, Body, OnHoleAbsorb, OnAbsorbDeath);
            _damageState = new AiDamageState(this, Body, _aiComponent, _sprite, _lives)
            {
                OnDeath = OnDeath,
                OnBurn = OnBurn
            };

            var damageBox = new CBox(EntityPosition, -8, -12, 0, 16, 12, 4);
            var hittableBox = new CBox(EntityPosition, -4, -14, 8, 12, 8);
            var pushableBox = new CBox(EntityPosition, -7, -11, 0, 14, 11, 4);

            AddComponent(DamageFieldComponent.Index, _damageField = new DamageFieldComponent(damageBox, HitType.Enemy, 2));
            AddComponent(HittableComponent.Index, _hitComponent = new HittableComponent(hittableBox, OnHit));
            AddComponent(BodyComponent.Index, Body);
            AddComponent(AiComponent.Index, _aiComponent);
            AddComponent(PushableComponent.Index, _pushComponent = new PushableComponent(pushableBox, OnPush));
            AddComponent(BaseAnimationComponent.Index, animationComponent);
            AddComponent(DrawComponent.Index, new DrawCSpriteComponent(_sprite, Values.LayerPlayer));
            AddComponent(DrawShadowComponent.Index, new DrawShadowCSpriteComponent(_sprite));

            _sword = new EnemyDarknutSword(Map, this);
        }

        private void Reset()
        {
            _sword.Animator.Continue();
            _sword._damageField.IsActive = true;
            _sword._hitComponent.IsActive = true;
            _sword._pushComponent.IsActive = true;

            InitIdle();
            _animator.Continue();
            _damageField.IsActive = true;
            _hitComponent.IsActive = true;
            _pushComponent.IsActive = true;
            _damageState.CurrentLives = EnemyLives.Darknut;
            _aiComponent.ChangeState("idle");
            _aiComponent.ChangeState("idle");
        }

        private void OnBurn()
        {
            _animator.Pause();
            _sword.Animator.Pause();
            _sword._damageField.IsActive = false;
            _sword._hitComponent.IsActive = false;
            _sword._pushComponent.IsActive = false;
            _damageField.IsActive = false;
            _hitComponent.IsActive = false;
            _pushComponent.IsActive = false;
        }

        public override void Init()
        {
            // add the sword to the map
            Map.Objects.SpawnObject(_sword);

            // start randomly idle or walking facing a random direction
            _direction = Game1.RandomNumber.Next(0, 4);
            _aiComponent.ChangeState(Game1.RandomNumber.Next(0, 2) == 0 ? "walking" : "idle");
        }

        private void UpdateDamageTick()
        {
            _sword.Sprite.SpriteShader = _sprite.SpriteShader;
        }

        public void WallSpawn()
        {
            FinishedSpawning = false;
            IsActive = true;
            _sword.IsActive = true;
            _damageState.IsActive = false;
            _aiComponent.ChangeState("spawned");

            // look down while spawning
            _direction = 3;
            _animator.Play("walk_" + _direction);
            _sword.Animator.Play("walk_" + _direction);
            Body.VelocityTarget = Vector2.Zero;
        }

        private void EndWallSpawn()
        {
            FinishedSpawning = true;
            _damageState.IsActive = true;
            _aiComponent.ChangeState("attack");
        }

        private void InitIdle()
        {
            _animator.Play("stand_" + _direction);
            _sword.Animator.Play("stand_" + _direction);
            Body.VelocityTarget = Vector2.Zero;
        }

        private void EndIdle()
        {
            var distance = EntityPosition.Position - MapManager.ObjLink.Position;

            if (_fieldRectangle.Contains(MapManager.ObjLink.Position) && distance.Length() < AttackRange)
                _aiComponent.ChangeState("attack");
            else
                _aiComponent.ChangeState("walking");
        }

        private void InitWalking()
        {
            ChangeDirection();
        }

        private void StateAttack()
        {
            var direction = (MapManager.ObjLink.Position + AnimationHelper.DirectionOffset[_direction] * 3) - EntityPosition.Position;

            if (!_fieldRectangle.Contains(MapManager.ObjLink.Position) || direction.Length() > FollowRange)
            {
                _aiComponent.ChangeState("idle");
                return;
            }

            if (direction != Vector2.Zero)
                direction.Normalize();

            Body.VelocityTarget = direction * AttackSpeed;

            _direction = AnimationHelper.GetDirection(direction);
            _animator.Play("walk_" + _direction);
            _sword.Animator.Play("walk_" + _direction);

            _animator.SpeedMultiplier = 2f;
            _sword.Animator.SpeedMultiplier = 2f;

        }

        private void ChangeDirection()
        {
            // random new direction
            _direction = Game1.RandomNumber.Next(0, 4);
            _animator.Play("walk_" + _direction);
            _sword.Animator.Play("walk_" + _direction);
            Body.VelocityTarget = AnimationHelper.DirectionOffset[_direction] * MoveSpeed;
        }

        private void OnDeath(bool pieceOfPower)
        {
            _damageState.BaseOnDeath(pieceOfPower);

            Map.Objects.DeleteObjects.Add(_sword);

            if (!SpawnGoldLeaf)
                return;

            var playerDirection = MapManager.ObjLink.Position - EntityPosition.Position;
            if (playerDirection != Vector2.Zero)
                playerDirection.Normalize();
            playerDirection *= 1.75f;

            // spawn the golden leaf jumping towards the player
            var objLeaf = new ObjItem(Map, 0, 0, null, _leafSaveKey, "goldLeaf", null);
            if (!objLeaf.IsDead)
            {
                objLeaf.EntityPosition.Set(new Vector3(EntityPosition.X, EntityPosition.Y, EntityPosition.Z));
                objLeaf.SetVelocity(new Vector3(playerDirection.X, playerDirection.Y, 1.0f));
                objLeaf.Collectable = false;
                Map.Objects.SpawnObject(objLeaf);
            }
        }

        private bool OnPush(Vector2 direction, PushableComponent.PushType type)
        {
            if (type == PushableComponent.PushType.Impact)
                Body.Velocity = new Vector3(direction.X * 2.5f, direction.Y * 2.5f, Body.Velocity.Z);

            return true;
        }

        private void OnCollision(Values.BodyCollision direction)
        {
            if (_aiComponent.CurrentStateId != "walking")
                return;

            // stop walking
            _aiComponent.ChangeState("idle");
        }

        private void OnHoleAbsorb()
        {
            _animator.SpeedMultiplier = 3f;
            _animator.Play("walk_" + _direction);
            _sword.Animator.SpeedMultiplier = 3f;
            _sword.Animator.Play("walk_" + _direction);
        }

        private void OnAbsorbDeath()
        {
            Map.Objects.DeleteObjects.Add(_sword);
        }

        private Values.HitCollision OnHit(GameObject gameObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
        {
            // Because of the way the hit system works, this needs to be in any hit that doesn't default to "None" hit collision.
            if ((hitType & HitType.CrystalSmash) != 0 || (hitType & HitType.ClassicSword) != 0)
                return Values.HitCollision.None;

            if (_damageState.CurrentLives <= 0)
            {
                _damageField.IsActive = false;
                _hitComponent.IsActive = false;
                _pushComponent.IsActive = false;
            }
            return _damageState.OnHit(gameObject, direction, hitType, damage, pieceOfPower);
        }
    }
}