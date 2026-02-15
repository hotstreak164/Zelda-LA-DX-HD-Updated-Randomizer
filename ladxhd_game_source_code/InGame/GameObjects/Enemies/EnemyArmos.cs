using System;
using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components.AI;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Enemies
{
    internal class EnemyArmos : GameObject
    {
        private readonly Animator _animator;
        private readonly AnimationComponent _animationComponent;
        private readonly BodyComponent _body;
        private readonly AiComponent _aiComponent;
        private readonly DamageFieldComponent _damageField;
        private readonly BodyCollisionComponent _bodyCollision;
        private readonly AiDamageState _aiDamageState;
        private readonly AiStunnedState _sunnedState;
        private readonly AiTriggerRandomTime _walkCounter;

        private readonly string _animationPrefix;

        private float _moveSpeed = 0.5f;
        private float _counter;
        private int _direction;
        private bool _collided;
        private int _lives = EnemyLives.Armos;

        public EnemyArmos() : base("armos") { }

        public EnemyArmos(Map.Map map, int posX, int posY, bool darkArmos) : base(map)
        {
            Tags = Values.GameObjectTag.Enemy;

            EntityPosition = new CPosition(posX + 8, posY + 16, 0);
            ResetPosition  = new CPosition(posX + 8, posY + 16, 0);
            EntitySize = new Rectangle(-8, -16, 16, 17);
            CanReset = true;
            OnReset = Reset;

            _animator = AnimatorSaveLoad.LoadAnimator("Enemies/armos");
            _animationPrefix = darkArmos ? "_dark" : "";

            var sprite = new CSprite(EntityPosition);
            _animationComponent = new AnimationComponent(_animator, sprite, new Vector2(-8, -16));

            _body = new BodyComponent(EntityPosition, -7, -12, 14, 12, 8)
            {
                MoveCollision = OnCollision,
                CollisionTypes = Values.CollisionTypes.Normal |
                                 Values.CollisionTypes.Enemy |
                                 Values.CollisionTypes.Field,
                AvoidTypes =     Values.CollisionTypes.Hole | 
                                 Values.CollisionTypes.NPCWall,
                FieldRectangle = map.GetField(posX, posY),
                Bounciness = 0.25f,
                Drag = 0.8f
            };

            var stateIdle = new AiState { Init = InitIdle };
            var stateAwaking = new AiState(UpdateAwaking) { Init = InitAwaking };
            var stateWalking = new AiState { Init = InitWalking };
            stateWalking.Trigger.Add(_walkCounter = new AiTriggerRandomTime(ChangeDirection, 1000, 1500));

            _aiComponent = new AiComponent();
            _aiComponent.States.Add("idle", stateIdle);
            _aiComponent.States.Add("awaking", stateAwaking);
            _aiComponent.States.Add("walking", stateWalking);
            new AiFallState(_aiComponent, _body, null, null);
            new AiDeepWaterState(_body);
            _sunnedState = new AiStunnedState(_aiComponent, _animationComponent, 3300, 900) { ShakeOffset = 1, SilentStateChange = false, ReturnState = "walking" };
            _aiDamageState = new AiDamageState(this, _body, _aiComponent, sprite, _lives) { SpawnItem = "arrow_1" };

            _aiComponent.ChangeState("idle");

            var hittableBox = new CBox(EntityPosition, -7, -15, 14, 15, 8);
            var damageBox = new CBox(EntityPosition, -8, -13, 0, 16, 13, 4);

            AddComponent(DamageFieldComponent.Index, _damageField = new DamageFieldComponent(damageBox, HitType.Enemy, 8) { IsActive = false });
            AddComponent(HittableComponent.Index, new HittableComponent(hittableBox, OnHit));
            AddComponent(BodyComponent.Index, _body);
            AddComponent(AiComponent.Index, _aiComponent);
            AddComponent(CollisionComponent.Index, _bodyCollision = new BodyCollisionComponent(_body, Values.CollisionTypes.Enemy));
            AddComponent(PushableComponent.Index, new PushableComponent(_body.BodyBox, OnPush));
            AddComponent(BaseAnimationComponent.Index, _animationComponent);
            AddComponent(DrawComponent.Index, new BodyDrawComponent(_body, sprite, Values.LayerPlayer));
            AddComponent(DrawShadowComponent.Index, new DrawShadowCSpriteComponent(sprite));
        }

        private void Reset()
        {
            _damageField.IsActive = false;
            _bodyCollision.IsActive = true;
            _aiComponent.ChangeState("idle");
            _aiComponent.ChangeState("idle");
        }

        private Values.HitCollision OnHit(GameObject gameObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
        {
            // Because of the way the hit system works, this needs to be in any hit that doesn't default to "None" hit collision.
            if ((hitType & HitType.CrystalSmash) != 0 || (hitType & HitType.ClassicSword) != 0)
                return Values.HitCollision.None;

            if (_aiDamageState.IsInDamageState())
                return Values.HitCollision.None;

            if (_aiComponent.CurrentStateId == "idle" || _aiComponent.CurrentStateId == "awaking")
                return Values.HitCollision.RepellingParticle;

            if (hitType == HitType.MagicRod || hitType == HitType.MagicPowder)
                return Values.HitCollision.Blocking;

            if (hitType == HitType.Bomb || hitType == HitType.Bow)
                return _aiDamageState.OnHit(gameObject, direction, hitType, damage, pieceOfPower);

            if (hitType == HitType.Boomerang)
                return _aiDamageState.OnHit(gameObject, direction, hitType, 1, pieceOfPower);

            if (hitType == HitType.Hookshot)
            {
                _body.VelocityTarget = Vector2.Zero;
                _damageField.IsActive = false;
                _animator.Pause();
                _sunnedState.StartStun();
            }

            _aiDamageState.HitKnockBack(gameObject, direction, hitType, pieceOfPower, false);

            Game1.GameManager.PlaySoundEffect("D360-09-09");

            if (pieceOfPower)
                Game1.GameManager.PlaySoundEffect("D370-17-11");

            return Values.HitCollision.Blocking;
        }

        private void InitIdle()
        {
            _body.VelocityTarget = Vector2.Zero;
            _animator.Play("idle" + _animationPrefix);
        }

        private void InitAwaking()
        {
            _animator.Play("awaking" + _animationPrefix);
        }

        private void UpdateAwaking()
        {
            // wobble
            _counter += Game1.DeltaTime;
            _animationComponent.SpriteOffset.X = -8 + 1 * MathF.Sin(MathF.PI * ((_counter / 1000) * (60 / 4f)));

            if (!_animator.IsPlaying)
            {
                _animationComponent.SpriteOffset.X = -8;
                _aiComponent.ChangeState("walking");
            }

            _animationComponent.UpdateSprite();
        }

        private void InitWalking()
        {
            ChangeDirection();
            _animator.Play("walking" + _animationPrefix);
            _damageField.IsActive = true;
            _bodyCollision.IsActive = false;
            _collided = false;
        }

        private void ChangeDirection()
        {
            // random new direction
            _direction = Game1.RandomNumber.Next(0, 8);
            var radius = (float)Math.PI * (_direction / 4f);
            _body.VelocityTarget = new Vector2((float)Math.Sin(radius), (float)Math.Cos(radius)) * _moveSpeed;
        }

        private bool OnPush(Vector2 direction, PushableComponent.PushType type)
        {
            if (type == PushableComponent.PushType.Impact)
                _body.Velocity = new Vector3(direction.X * 2.5f, direction.Y * 2.5f, _body.Velocity.Z);
            else if (_aiComponent.CurrentStateId == "idle")
                _aiComponent.ChangeState("awaking");

            return true;
        }

        private void OnCollision(Values.BodyCollision direction)
        {
            // cut the time we walk into the wall
            if (!_collided)
            {
                _walkCounter.CurrentTime /= 2;
                _collided = true;
            }
        }
    }
}