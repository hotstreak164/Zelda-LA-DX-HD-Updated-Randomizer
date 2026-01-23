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
    internal class EnemyIronMask : GameObject
    {
        private readonly Animator _animator;
        private readonly BodyComponent _body;
        private readonly AiComponent _aiComponent;
        private readonly AiDamageState _damageState;
        private readonly DamageFieldComponent _damageField;
        private readonly HittableComponent _hitComponent;
        private readonly PushableComponent _pushComponent;

        private float _moveSpeed = 0.5f;
        private float _moveSpeedUnprotected = 0.75f;
        private int _direction;
        private bool _isUnprotected;
        private int _lives = ObjLives.IronMask;

        public EnemyIronMask() : base("iron mask") { }

        public EnemyIronMask(Map.Map map, int posX, int posY) : base(map)
        {
            Tags = Values.GameObjectTag.Enemy;

            EntityPosition = new CPosition(posX + 8, posY + 16, 0);
            ResetPosition  = new CPosition(posX + 8, posY + 16, 0);
            EntitySize = new Rectangle(-8, -16, 16, 16);
            CanReset = true;
            OnReset = Reset;

            _animator = AnimatorSaveLoad.LoadAnimator("Enemies/iron mask");

            var sprite = new CSprite(EntityPosition);
            var animationComponent = new AnimationComponent(_animator, sprite, new Vector2(-8, -15));

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

            var stateIdle = new AiState { Init = InitIdle };
            stateIdle.Trigger.Add(new AiTriggerRandomTime(() => _aiComponent.ChangeState("walking"), 350, 750));
            var stateWalking = new AiState { Init = InitWalking };
            stateWalking.Trigger.Add(new AiTriggerRandomTime(() => _aiComponent.ChangeState("idle"), 750, 1000));
            var stateStunned = new AiState { Init = InitStunned };
            stateStunned.Trigger.Add(new AiTriggerRandomTime(() => _aiComponent.ChangeState("walking"), 1000, 1200));

            _aiComponent = new AiComponent();
            _aiComponent.States.Add("idle", stateIdle);
            _aiComponent.States.Add("walking", stateWalking);
            _aiComponent.States.Add("stunned", stateStunned);
            _damageState = new AiDamageState(this, _body, _aiComponent, sprite, _lives) { OnBurn = OnBurn };
            new AiFallState(_aiComponent, _body, OnHoleAbsorb);
            new AiDeepWaterState(_body);

            _aiComponent.ChangeState("idle");

            // stand in a random direction
            _direction = Game1.RandomNumber.Next(0, 4);
            _animator.Play("walk_" + _direction);
            _animator.IsPlaying = false;

            var damageBox = new CBox(EntityPosition, -8, -12, 0, 16, 12, 4);
            var hittableBox = new CBox(EntityPosition, -7, -14, 14, 14, 8);
            var pushableBox = new CBox(EntityPosition, -7, -12, 14, 12, 8);

            AddComponent(DamageFieldComponent.Index, _damageField = new DamageFieldComponent(damageBox, HitType.Enemy, 2));
            AddComponent(HittableComponent.Index, _hitComponent = new HittableComponent(hittableBox, OnHit));
            AddComponent(BodyComponent.Index, _body);
            AddComponent(AiComponent.Index, _aiComponent);
            AddComponent(PushableComponent.Index, _pushComponent = new PushableComponent(pushableBox, OnPush));
            AddComponent(BaseAnimationComponent.Index, animationComponent);
            AddComponent(DrawComponent.Index, new BodyDrawComponent(_body, sprite, Values.LayerPlayer));
            AddComponent(DrawShadowComponent.Index, new DrawShadowCSpriteComponent(sprite));

            new ObjSpriteShadow("sprshadowm", this, Values.LayerPlayer, map);
        }

        private void Reset()
        {
            _isUnprotected = false;
            _animator.Play("walk_" + _direction);
            _damageField.IsActive = true;
            _hitComponent.IsActive = true;
            _pushComponent.IsActive = true;
            _aiComponent.ChangeState("idle");
            _aiComponent.ChangeState("idle");
        }

        private void OnBurn()
        {
            _animator.Pause();
            _damageField.IsActive = false;
            _hitComponent.IsActive = false;
            _pushComponent.IsActive = false;
        }

        private void InitStunned()
        {
            if (!_isUnprotected)
                _animator.IsPlaying = false;
            _body.VelocityTarget = Vector2.Zero;
        }

        private void InitIdle()
        {
            if (!_isUnprotected)
                _animator.IsPlaying = false;
            _body.VelocityTarget = Vector2.Zero;
        }

        private void InitWalking()
        {
            ChangeDirection();
        }

        private void ChangeDirection()
        {
            // random new direction
            _direction = Game1.RandomNumber.Next(0, 4);

            if (!_isUnprotected)
                _animator.Play("walk_" + _direction);

            _body.VelocityTarget = AnimationHelper.DirectionOffset[_direction] *
                                   (_isUnprotected ? _moveSpeedUnprotected : _moveSpeed);
        }

        private bool OnPush(Vector2 direction, PushableComponent.PushType type)
        {
            if (type == PushableComponent.PushType.Impact)
                _body.Velocity = new Vector3(direction.X * 2.5f, direction.Y * 2.5f, _body.Velocity.Z);

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

            if (!_isUnprotected)
                _animator.Play("walk_" + _direction);
        }

        private Values.HitCollision OnHit(GameObject originObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
        {
            // Because of the way the hit system works, this needs to be in any hit that doesn't default to "None" hit collision.
            if ((hitType & HitType.CrystalSmash) != 0 || (hitType & HitType.ClassicSword) != 0)
                return Values.HitCollision.None;

            // can be hit if the damage source is coming from the back
            var dir = AnimationHelper.GetDirection(direction);

            if (!_isUnprotected && hitType == HitType.Hookshot && dir == (_direction + 2) % 4 &&
                _aiComponent.CurrentStateId != "stunned")
            {
                Game1.GameManager.PlaySoundEffect("D370-01-01");
                _isUnprotected = true;
                _animator.Play("unprotected");
                _damageState.SetDamageState(false);
                return Values.HitCollision.Repelling;
            }

            // throw objects will kill him directly
            if (hitType == HitType.ThrownObject || hitType == HitType.Bomb || hitType == HitType.MagicPowder || hitType == HitType.MagicRod)
                return _damageState.OnHit(originObject, direction, hitType, damage, pieceOfPower);

            // gets attacked from behind or is unprotected?
            if (dir == (_direction) % 4 || _isUnprotected || hitType == HitType.Hookshot)
                return _damageState.OnHit(originObject, direction, hitType, damage, pieceOfPower);

            // gets attacked from behind/sides by a bow
            if ((dir != (_direction + 2) % 4) && (hitType == HitType.Bow || hitType == HitType.Boomerang))
                return _damageState.OnHit(originObject, direction, hitType, damage, pieceOfPower);

            if (_aiComponent.CurrentStateId != "stunned")
            {
                _body.Velocity = new Vector3(direction, 0);
                _aiComponent.ChangeState("stunned");
            }
            if (_damageState.CurrentLives <= 0)
            {
                _damageField.IsActive = false;
                _hitComponent.IsActive = false;
                _pushComponent.IsActive = false;
            }
            return Values.HitCollision.RepellingParticle;
        }
    }
}