using Microsoft.Xna.Framework;
using ProjectZ.Base;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.GameObjects.Base.Components.AI;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Things;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Enemies
{
    internal class EnemyBombiteGreen : GameObject
    {
        private readonly BodyComponent _body;
        private readonly AiComponent _aiComponent;
        private readonly Animator _animator;
        private readonly AiDamageState _damageState;
        private readonly AiTriggerSwitch _damageCooldown;
        private readonly AiStunnedState _aiStunnedState;
        private readonly CSprite _sprite;
        private readonly HittableComponent _hitComponent;
        private readonly PushableComponent _pushComponent;
        private readonly DamageFieldComponent _damageField;

        private const float WalkSpeed = 0.5f;
        private RectangleF _fieldRect;

        private int _direction;
        private bool _startedAnimation;
        private bool _follow;
        private int _lives = EnemyLives.BombiteGreen;
        private bool _wasStunned;

        public EnemyBombiteGreen() : base("bombiteGreen") { }

        public EnemyBombiteGreen(Map.Map map, int posX, int posY) : base(map)
        {
            Tags = Values.GameObjectTag.Enemy;

            EntityPosition = new CPosition(posX + 8, posY + 16, 0);
            ResetPosition  = new CPosition(posX + 8, posY + 16, 0);
            EntitySize = new Rectangle(-8, -16, 16, 16);
            CanReset = true;
            OnReset = Reset;

            _animator = AnimatorSaveLoad.LoadAnimator("Enemies/bombiteGreen");

            _sprite = new CSprite(EntityPosition);
            var animationComponent = new AnimationComponent(_animator, _sprite, new Vector2(-7, -16));

            _body = new BodyComponent(EntityPosition, -6, -12, 12, 11, 8)
            {
                AbsorbPercentage = 0.9f,
                CollisionTypes = Values.CollisionTypes.Normal |
                                 Values.CollisionTypes.Field,
                AvoidTypes =     Values.CollisionTypes.Hole |
                                 Values.CollisionTypes.NPCWall,
                Bounciness = 0.25f,
                Drag = 0.85f,
            };
            _fieldRect = map.GetField(posX, posY);

            var stateIdle = new AiState(UpdateIdle) { Init = InitIdle };
            stateIdle.Trigger.Add(new AiTriggerRandomTime(ChangeDirection, 250, 500));
            var stateFollow = new AiState(UpdateFollow);

            _aiComponent = new AiComponent();
            _aiComponent.States.Add("idle", stateIdle);
            _aiComponent.States.Add("follow", stateFollow);
            new AiFallState(_aiComponent, _body, OnHoleAbsorb, null);
            _damageState = new AiDamageState(this, _body, _aiComponent, _sprite, _lives, false);
            _aiStunnedState = new AiStunnedState(_aiComponent, animationComponent, 3300, 900) { SilentStateChange = false };

            _aiComponent.Trigger.Add(_damageCooldown = new AiTriggerSwitch(250));

            _aiComponent.ChangeState("idle");
            ChangeDirection();

            var damageCollider = new CBox(EntityPosition, -6, -13, 0, 12, 12, 4);

            AddComponent(DamageFieldComponent.Index, _damageField = new DamageFieldComponent(damageCollider, HitType.Enemy, 2));
            AddComponent(HittableComponent.Index, _hitComponent = new HittableComponent(_body.BodyBox, OnHit));
            AddComponent(BodyComponent.Index, _body);
            AddComponent(AiComponent.Index, _aiComponent);
            AddComponent(BaseAnimationComponent.Index, animationComponent);
            AddComponent(PushableComponent.Index, _pushComponent = new PushableComponent(_body.BodyBox, OnPush));
            AddComponent(DrawComponent.Index, new BodyDrawComponent(_body, _sprite, Values.LayerPlayer));
            AddComponent(DrawShadowComponent.Index, new DrawShadowCSpriteComponent(_sprite) { Height = 1.0f, Rotation = 0.1f });
        }

        private void Reset()
        {
            _damageField.IsActive = true;
            _hitComponent.IsActive = true;
            _pushComponent.IsActive = true;
            _aiComponent.ChangeState("idle");
            _aiComponent.ChangeState("idle");
        }

        private void TryReleaseStun()
        {
            if (!_aiStunnedState.Active && _wasStunned)
            {
                _damageField.IsActive = true;
                _wasStunned = false;
            }
        }

        private void InitIdle()
        {
            _animator.Play("idle");
        }

        private void UpdateIdle()
        {
            if (_follow && !_damageState.IsInDamageState())
            {
                _aiComponent.ChangeState("follow");
                _damageField.IsActive = false;
            }
            TryReleaseStun();
        }

        private void UpdateFollow()
        {
            // start animation when slowed down enough
            if (!_startedAnimation && _body.Velocity.Length() < 0.1f)
            {
                _startedAnimation = true;
                _animator.Play("timer");
            }

            if (_startedAnimation)
            {
                if (!_animator.IsPlaying)
                    Explode();
                else if (_animator.CurrentFrameIndex > 2)
                {
                    // blink
                    _sprite.SpriteShader = Game1.TotalGameTime % (AiDamageState.BlinkTime * 2) < AiDamageState.BlinkTime ? Resources.DamageSpriteShader0 : null;
                }

                // move towards the player
                var direction = MapManager.ObjLink.Position - EntityPosition.Position;
                var distance = direction.Length();
                if (direction != Vector2.Zero)
                    direction.Normalize();

                if (distance > 20)
                    _body.VelocityTarget = direction;
                else
                    _body.VelocityTarget = Vector2.Zero;
            }
            TryReleaseStun();
        }

        private void ChangeDirection()
        {
            _direction = Game1.RandomNumber.Next(0, 4);
            _body.VelocityTarget = AnimationHelper.DirectionOffset[_direction] * WalkSpeed;
        }

        private void Explode()
        {
            // spawn explosion effect
            var objExplosion = new ObjBomb(Map, EntityPosition.X, EntityPosition.Y, false, false);
            objExplosion.Explode();
            Map.Objects.SpawnObject(objExplosion);
            Map.Objects.SpawnObject(new EnemyBombiteRespawner(Map, (int)ResetPosition.X - 8, (int)ResetPosition.Y - 16, _fieldRect, true));
            Map.Objects.DeleteObjects.Add(this);
        }

        private bool OnPush(Vector2 direction, PushableComponent.PushType type)
        {
            if (type == PushableComponent.PushType.Impact)
                _body.Velocity = new Vector3(direction.X * 2.5f, direction.Y * 2.5f, _body.Velocity.Z);

            return true;
        }

        private void OnHoleAbsorb()
        {
            _animator.SpeedMultiplier = 3f;
            _animator.Play("idle");
        }

        private Values.HitCollision OnHit(GameObject gameObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
        {
            // Because of the way the hit system works, this needs to be in any hit that doesn't default to "None" hit collision.
            if ((hitType & HitType.CrystalSmash) != 0 || (hitType & HitType.ClassicSword) != 0)
                return Values.HitCollision.None;

            if (_damageState.IsInDamageState())
                return Values.HitCollision.None;

            if (hitType == HitType.Bomb && !(gameObject is EnemyBombite))
            {
                if (_damageState.CurrentLives <= 0)
                {
                    _damageField.IsActive = false;
                    _hitComponent.IsActive = false;
                    _pushComponent.IsActive = false;
                }
                // spawn a bomb
                _damageState.SpawnItem = "bomb_1";
                return _damageState.OnHit(gameObject, direction, hitType, damage, pieceOfPower);
            }

            // stun state
            if (hitType == HitType.Hookshot || hitType == HitType.Boomerang)
            {
                _body.VelocityTarget = Vector2.Zero;
                _body.Velocity.X += direction.X * 4.0f;
                _body.Velocity.Y += direction.Y * 4.0f;

                _damageField.IsActive = false;
                _aiStunnedState.StartStun();
                _animator.Pause();
                _wasStunned = true;

                return Values.HitCollision.Enemy;
            }

            if (hitType != HitType.MagicPowder)
            {
                if (pieceOfPower)
                    Game1.GameManager.PlaySoundEffect("D370-17-11");

                Game1.GameManager.PlaySoundEffect("D360-03-03");

                if (pieceOfPower)
                    _damageState.HitKnockBack(gameObject, direction, hitType, pieceOfPower, false);
                else
                {
                    _body.Velocity.X += direction.X * 5.0f;
                    _body.Velocity.Y += direction.Y * 5.0f;
                    _damageState.SetDamageState(false);
                }
            }
            else
            {
                _body.Velocity.X += direction.X * 1.0f;
                _body.Velocity.Y += direction.Y * 1.0f;
                _damageState.SetDamageState(false);
            }
            if (!_aiStunnedState.IsStunned())
                _follow = true;

            return Values.HitCollision.Enemy;
        }
    }
}