using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.GameObjects.Base.Components.AI;
using ProjectZ.InGame.GameObjects.Things;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Enemies
{
    internal class EnemyGibdo : GameObject
    {
        private readonly Animator _animator;
        private readonly BodyComponent _body;
        private readonly AiComponent _aiComponent;
        private readonly AiDamageState _damageState;
        private readonly AiStunnedState _aiStunnedState;
        private readonly DamageFieldComponent _damageField;
        private readonly HittableComponent _hitComponent;
        private readonly PushableComponent _pushComponent;

        private readonly EnemyStalfosOrange _stalfos;

        private const float MoveSpeed = 0.5f;

        private int _direction;
        private int _lives = ObjLives.Gibdo;

        public EnemyGibdo() : base("gibdo") { }

        public EnemyGibdo(Map.Map map, int posX, int posY) : base(map)
        {
            Tags = Values.GameObjectTag.Enemy;

            EntityPosition = new CPosition(posX + 8, posY + 16, 0);
            ResetPosition  = new CPosition(posX + 8, posY + 16, 0);
            EntitySize = new Rectangle(-8, -16, 16, 16);
            CanReset = true;
            OnReset = Reset;

            _animator = AnimatorSaveLoad.LoadAnimator("Enemies/gibdo");
            _animator.Play("idle");

            var sprite = new CSprite(EntityPosition);
            var animationComponent = new AnimationComponent(_animator, sprite, new Vector2(-8, -16));

            _body = new BodyComponent(EntityPosition, -6, -10, 12, 10, 8)
            {
                MoveCollision = OnCollision,
                CollisionTypes = Values.CollisionTypes.Normal |
                                 Values.CollisionTypes.Field |
                                 Values.CollisionTypes.Enemy,
                AvoidTypes =     Values.CollisionTypes.Hole | 
                                 Values.CollisionTypes.NPCWall,
                FieldRectangle = map.GetField(posX, posY),
                Bounciness = 0.25f,
                Drag = 0.85f
            };

            var stateWalking = new AiState { Init = InitWalking };
            stateWalking.Trigger.Add(new AiTriggerRandomTime(() => _aiComponent.ChangeState("walk"), 550, 850));
            _aiComponent = new AiComponent();
            _aiComponent.States.Add("walk", stateWalking);

            _damageState = new AiDamageState(this, _body, _aiComponent, sprite, _lives, false)
            {
                HitMultiplierX = 1.0f,
                HitMultiplierY = 1.0f,
                OnDeath = OnDeath,
                OnBurn = OnBurn
            };
            _aiStunnedState = new AiStunnedState(_aiComponent, animationComponent, 3300, 900) { ShakeOffset = 1, SilentStateChange = false, ReturnState = "walk" };
            new AiFallState(_aiComponent, _body, OnHoleAbsorb);

            var damageBox = new CBox(EntityPosition, -7, -14, 0, 14, 14, 4);
            var pushableBox = new CBox(EntityPosition, -6, -13, 0, 12, 13, 4);
            var hittableBox = new CBox(EntityPosition, -7, -15, 14, 15, 8);

            _stalfos = new EnemyStalfosOrange(Map, posX, posY, true) { IsActive = false, WasSpawned = true };
            Map.Objects.SpawnObject(_stalfos);

            AddComponent(DamageFieldComponent.Index, _damageField = new DamageFieldComponent(damageBox, HitType.Enemy, 4));
            AddComponent(HittableComponent.Index, _hitComponent = new HittableComponent(hittableBox, OnHit));
            AddComponent(BodyComponent.Index, _body);
            AddComponent(AiComponent.Index, _aiComponent);
            AddComponent(PushableComponent.Index, _pushComponent = new PushableComponent(pushableBox, OnPush));
            AddComponent(BaseAnimationComponent.Index, animationComponent);
            AddComponent(DrawComponent.Index, new BodyDrawComponent(_body, sprite, Values.LayerPlayer));
            AddComponent(DrawShadowComponent.Index, new DrawShadowCSpriteComponent(sprite));

            _aiComponent.ChangeState("walk");

            new ObjSpriteShadow("sprshadowm", this, Values.LayerPlayer, map);
        }

        private void Reset()
        {
            _animator.Continue();
            _damageField.IsActive = true;
            _hitComponent.IsActive = true;
            _pushComponent.IsActive = true;
            _aiComponent.ChangeState("walk");
            _aiComponent.ChangeState("walk");
        }

        private void OnBurn()
        {
            _animator.Pause();
            _damageField.IsActive = false;
            _hitComponent.IsActive = false;
            _pushComponent.IsActive = false;
        }

        private void OnDeath(bool pieceOfPower)
        {
            if (Map == null)
                return;

            if (_aiComponent.CurrentStateId == "burning")
            {
                Map.Objects.DeleteObjects.Add(this);

                // Spawn the Stalfos.
                _stalfos.EntityPosition.Set(new Vector2(EntityPosition.X, (int)EntityPosition.Y));
                _stalfos.IsActive = true;
                return;
            }
            _damageState.BaseOnDeath(pieceOfPower);
            Map.Objects.DeleteObjects.Add(_stalfos);
        }

        private void InitWalking()
        {
            _animator.Play("idle");
            _damageField.IsActive = true;
            // walk into a random direction
            _direction = Game1.RandomNumber.Next(0, 4);
            _body.VelocityTarget = AnimationHelper.DirectionOffset[_direction] * MoveSpeed;
        }

        private bool OnPush(Vector2 direction, PushableComponent.PushType type)
        {
            if (type == PushableComponent.PushType.Impact)
            {
                _body.Velocity.X = direction.X * 2.5f;
                _body.Velocity.Y = direction.Y * 2.5f;
            }

            return true;
        }

        private void OnCollision(Values.BodyCollision direction)
        {
            if ((direction & Values.BodyCollision.Horizontal) != 0)
                _body.VelocityTarget.X = -_body.VelocityTarget.X;
            if ((direction & Values.BodyCollision.Vertical) != 0)
                _body.VelocityTarget.Y = -_body.VelocityTarget.Y;
        }

        private void OnHoleAbsorb()
        {
            _animator.SpeedMultiplier = 3f;
        }

        private Values.HitCollision OnHit(GameObject gameObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
        {
            // Because of the way the hit system works, this needs to be in any hit that doesn't default to "None" hit collision.
            if ((hitType & HitType.CrystalSmash) != 0 || (hitType & HitType.ClassicSword) != 0)
                return Values.HitCollision.None;

            if (hitType == HitType.Bomb)
                damage = 3;
            if (hitType == HitType.Boomerang)
                damage = 2;
            if (hitType == HitType.Bow)
                damage = 1;
            if (hitType == HitType.Hookshot)
            {
                _body.VelocityTarget = Vector2.Zero;
                _body.Velocity.X += direction.X * 0.75f;
                _body.Velocity.Y += direction.Y * 0.75f;
                _damageField.IsActive = false;
                _aiStunnedState.StartStun();
                _animator.Pause();
                return Values.HitCollision.Enemy;
            }
            if (_damageState.CurrentLives <= 0)
            {
                _damageField.IsActive = false;
                _hitComponent.IsActive = false;
                _pushComponent.IsActive = false;
            }
            return _damageState.OnHit(gameObject, direction, hitType, damage, pieceOfPower);
        }

        public void AddToEnemyTriggerGroup(ObjEnemyTrigger etrigger)
        {
            // If respawned in a room with an enemy trigger, this is a means 
            // to adding the Stalfos spawned with the Gibdo to the trigger list.
            etrigger.EnemyTriggerList.Add(_stalfos);
        }
    }
}