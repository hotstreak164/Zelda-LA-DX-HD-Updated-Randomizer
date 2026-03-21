using Microsoft.Xna.Framework;
using ProjectZ.Base;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.GameObjects.Base.Components.AI;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Things;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Enemies
{
    internal class EnemyBombite : GameObject
    {
        private readonly BodyComponent _body;
        private readonly AiComponent _aiComponent;
        private readonly Animator _animator;
        private readonly AiDamageState _damageState;
        private readonly AiTriggerSwitch _damageCooldown;
        private readonly CBox _pongCollider;
        private readonly HittableComponent _hitComponent;
        private readonly PushableComponent _pushComponent;
        private readonly DamageFieldComponent _damageField;

        private const float WalkSpeed = 0.5f;
        private RectangleF _fieldRect;

        private int _direction;
        private int _lives = EnemyLives.Bombite;

        public EnemyBombite() : base("bombite") { }

        public EnemyBombite(Map.Map map, int posX, int posY) : base(map)
        {
            Tags = Values.GameObjectTag.Enemy;

            EntityPosition = new CPosition(posX + 8, posY + 16, 0);
            ResetPosition  = new CPosition(posX + 8, posY + 16, 0);
            EntitySize = new Rectangle(-8, -16, 16, 16);
            CanReset = true;
            OnReset = Reset;

            _animator = AnimatorSaveLoad.LoadAnimator("Enemies/bombite");
            _animator.Play("idle");

            var sprite = new CSprite(EntityPosition);
            var animationComponent = new AnimationComponent(_animator, sprite, new Vector2(-7, -16));

            _body = new BodyComponent(EntityPosition, -6, -12, 12, 11, 8)
            {
                MoveCollision = OnCollision,
                AbsorbPercentage = 0.9f,
                CollisionTypes = Values.CollisionTypes.Normal |
                                 Values.CollisionTypes.Field,
                AvoidTypes =     Values.CollisionTypes.Hole |
                                 Values.CollisionTypes.NPCWall,
                FieldRectangle = map.GetField(posX, posY),
                Bounciness = 0.25f,
                Drag = 0.85f,
            };
            _fieldRect = map.GetField(posX, posY);

            var stateIdle = new AiState();
            stateIdle.Trigger.Add(new AiTriggerRandomTime(ChangeDirection, 250, 500));
            var statePong = new AiState(UpdatePong);
            statePong.Trigger.Add(new AiTriggerCountdown(1100, null, Explode));
            statePong.Trigger.Add(new AiTriggerCountdown(750, null, StartBlinking));

            _aiComponent = new AiComponent();
            _aiComponent.States.Add("idle", stateIdle);
            _aiComponent.States.Add("pong", statePong);
            new AiFallState(_aiComponent, _body, OnHoleAbsorb, null);
            _damageState = new AiDamageState(this, _body, _aiComponent, sprite, _lives, false);

            _aiComponent.Trigger.Add(_damageCooldown = new AiTriggerSwitch(250));

            _aiComponent.ChangeState("idle");
            ChangeDirection();

            var damageBox = new CBox(EntityPosition, -3,  -8, 0,  6,  6, 4);
            _pongCollider = new CBox(EntityPosition, -6, -12, 0, 12, 11, 8);

            AddComponent(DamageFieldComponent.Index, _damageField = new DamageFieldComponent(damageBox, HitType.Enemy, 4));
            AddComponent(HittableComponent.Index, _hitComponent = new HittableComponent(_body.BodyBox, OnHit));
            AddComponent(BodyComponent.Index, _body);
            AddComponent(AiComponent.Index, _aiComponent);
            AddComponent(BaseAnimationComponent.Index, animationComponent);
            AddComponent(PushableComponent.Index, _pushComponent = new PushableComponent(_body.BodyBox, OnPush));
            AddComponent(DrawComponent.Index, new BodyDrawComponent(_body, sprite, Values.LayerPlayer));
            AddComponent(DrawShadowComponent.Index, new DrawShadowCSpriteComponent(sprite) { Height = 1.0f, Rotation = 0.1f });
        }

        public override void Reset()
        {
            _damageField.IsActive = true;
            _hitComponent.IsActive = true;
            _pushComponent.IsActive = true;
            _animator.Play("idle");
            _aiComponent.ChangeState("idle");
            _aiComponent.ChangeState("idle");
            _body.VelocityTarget = Vector2.Zero;
        }

        private void UpdatePong()
        {
            var hitReturn = Map.Objects.Hit(this, _pongCollider.Box.Center, _pongCollider.Box, HitType.Bomb, 2, false);
            if (hitReturn == Values.HitCollision.Enemy)
                Explode();
        }

        private void ChangeDirection()
        {
            _direction = Game1.RandomNumber.Next(0, 4);
            _body.VelocityTarget = AnimationHelper.DirectionOffset[_direction] * WalkSpeed;
        }

        private void StartBlinking()
        {
            _damageState.SetDamageState();
        }

        private void Explode()
        {
            // spawn explosion effect
            var objExplosion = new ObjBomb(Map, EntityPosition.X, EntityPosition.Y, false, false) { DamageEnemies = true };
            objExplosion.Explode();
            Map.Objects.SpawnObject(objExplosion);
            Map.Objects.SpawnObject(new EnemyBombiteRespawner(Map, (int)ResetPosition.X - 8, (int)ResetPosition.Y - 16, _fieldRect, false));
            Map.Objects.DeleteObjects.Add(this);
        }

        private bool OnPush(Vector2 direction, PushableComponent.PushType type)
        {
            if (type == PushableComponent.PushType.Impact)
                _body.Velocity = new Vector3(direction.X * 2.5f, direction.Y * 2.5f, _body.Velocity.Z);

            return true;
        }

        private void OnCollision(Values.BodyCollision direction)
        {
            if (_aiComponent.CurrentStateId == "pong")
            {
                Game1.GameManager.PlaySoundEffect("D360-09-09");

                if ((direction & Values.BodyCollision.Horizontal) != 0)
                    _body.VelocityTarget.X = -_body.VelocityTarget.X;
                else if ((direction & Values.BodyCollision.Vertical) != 0)
                    _body.VelocityTarget.Y = -_body.VelocityTarget.Y;
            }
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

            // Don't register sword hits at all and instead just start the pinball state.
            if ((hitType & HitType.Sword) != 0 || (hitType & HitType.SwordHold) != 0)
            {
                _body.VelocityTarget = direction * 3;
                _animator.Play("damage");
                _aiComponent.ChangeState("pong");
                return Values.HitCollision.None;
            }

            if (!_damageCooldown.State || gameObject == this)
                return Values.HitCollision.None;
            _damageCooldown.Reset();

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

            // hookshot/boomerang freeze
            _body.FieldRectangle = RectangleF.Empty;

            if (hitType != HitType.MagicPowder)
                _body.VelocityTarget = direction * 3;
            else
                _body.VelocityTarget = Vector2.Zero;

            _animator.Play("damage");
            _aiComponent.ChangeState("pong");

            return Values.HitCollision.Enemy;
        }
    }
}