using System;
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
    internal class EnemyHardhatBeetle : GameObject
    {
        private readonly AiComponent _aiComponent;
        private readonly BodyComponent _body;
        private readonly AiStunnedState _stunnedState;
        private readonly Animator _animator;
        private readonly AiDamageState _damageState;
        private readonly DamageFieldComponent _damageField;

        private Vector2 _vecDirection;

        private float _maxSpeed;

        private bool _isFollowing;
        private bool _wasFollowing;
        private int _lives = ObjLives.HardhatBeetle;

        private float speedChange;

        public EnemyHardhatBeetle() : base("hardHatBeetle") { }

        public EnemyHardhatBeetle(Map.Map map, int posX, int posY) : base(map)
        {
            Tags = Values.GameObjectTag.Enemy;

            EntityPosition = new CPosition(posX + 8, posY + 16, 0);
            ResetPosition  = new CPosition(posX + 8, posY + 16, 0);
            EntitySize = new Rectangle(-8, -16, 16, 16);
            CanReset = true;
            OnReset = Reset;

            _animator = AnimatorSaveLoad.LoadAnimator("Enemies/hardhat beetle");
            _animator.Play("walk");

            var sprite = new CSprite(EntityPosition);
            var animationComponent = new AnimationComponent(_animator, sprite, Vector2.Zero);

            var fieldRectangle = map.GetField(posX, posY);

            _body = new BodyComponent(EntityPosition, -6, -10, 12, 9, 8)
            {
                MoveCollision = OnCollision,
                Drag = 0.875f,
                CollisionTypes = Values.CollisionTypes.Normal |
                                 Values.CollisionTypes.Field,
                AvoidTypes =     Values.CollisionTypes.Hole |
                                 Values.CollisionTypes.NPCWall |
                                 Values.CollisionTypes.DeepWater,
                FieldRectangle = fieldRectangle
            };

            _aiComponent = new AiComponent();

            var stateWaiting = new AiState { Init = InitWaiting };
            stateWaiting.Trigger.Add(new AiTriggerRandomTime(UpdateWaiting, 75, 100));
            var stateMoving = new AiState(UpdateMoving);

            _aiComponent.States.Add("waiting", stateWaiting);
            _aiComponent.States.Add("moving", stateMoving);
            _stunnedState = new AiStunnedState(_aiComponent, animationComponent, 3300, 900) { SilentStateChange = false, ReturnState = "waiting" };
            _damageState = new AiDamageState(this, _body, _aiComponent, sprite, _lives);
            new AiDeepWaterState(_body);
            new AiFallState(_aiComponent, _body, OnHoleAbsorb, OnHoleDeath);

            _aiComponent.ChangeState("waiting");
            _maxSpeed = GameMath.GetRandomFloat(0.25f, 0.55f);

            var damageCollider = new CBox(EntityPosition, -7, -11, 0, 14, 11, 4);
            var hittableRectangle = new CBox(EntityPosition, -8, -14, 16, 14, 8);

            AddComponent(DamageFieldComponent.Index, _damageField = new DamageFieldComponent(damageCollider, HitType.Enemy, 4) { PushMultiplier = 2.00f });
            AddComponent(HittableComponent.Index, new HittableComponent(hittableRectangle, OnHit));
            AddComponent(BodyComponent.Index, _body);
            AddComponent(PushableComponent.Index, new PushableComponent(_body.BodyBox, OnPush) { RepelMultiplier = 4.25f });
            AddComponent(BaseAnimationComponent.Index, animationComponent);
            AddComponent(AiComponent.Index, _aiComponent);
            AddComponent(DrawComponent.Index, new BodyDrawComponent(_body, sprite, Values.LayerPlayer));
            AddComponent(DrawShadowComponent.Index, new DrawShadowCSpriteComponent(sprite));
        }

        private void Reset()
        {
            _isFollowing = false;
            _wasFollowing = false;
            _aiComponent.ChangeState("waiting");
        }

        private void InitWaiting()
        {
            _animator.Play("walk");
            _animator.SpeedMultiplier = 1.0f;
        }

        private void UpdateWaiting()
        {
            if (_body.FieldRectangle.Intersects(MapManager.ObjLink.BodyRectangle))
                _aiComponent.ChangeState("moving");
        }

        private void UpdateMoving()
        {
            // Give them a random speed that fluctuates every 3/4 second to prevent them from stacking on
            // top of each other. This also more closely matches their behavior from the original games.
            if ((speedChange += Game1.DeltaTime) > 750)
            {
                _maxSpeed = GameMath.GetRandomFloat(0.25f, 0.55f);
                speedChange = 0;
            }
            if (_vecDirection != Vector2.Zero)
            {
                var oldPercentage = (float)Math.Pow(0.9f, Game1.TimeMultiplier);
                var newDirection = _body.VelocityTarget * oldPercentage +
                                   _vecDirection * (1 - oldPercentage);
                newDirection.Normalize();

                _body.VelocityTarget = newDirection * _maxSpeed;
            }
            else
                _body.VelocityTarget = Vector2.Zero;

            _isFollowing = MapManager.ObjLink.BodyRectangle.Intersects(_body.FieldRectangle);

            if (_isFollowing)
                _vecDirection = new Vector2(MapManager.ObjLink.EntityPosition.X - EntityPosition.X, MapManager.ObjLink.EntityPosition.Y - EntityPosition.Y);
            else
                _vecDirection = new Vector2(ResetPosition.X - EntityPosition.X, ResetPosition.Y - EntityPosition.Y);

            if (!_isFollowing && (int)EntityPosition.X == (int)ResetPosition.X && (int)EntityPosition.Y == (int)ResetPosition.Y)
            {
                _body.VelocityTarget = Vector2.Zero;
                _aiComponent.ChangeState("waiting");
            }

            if (_vecDirection != Vector2.Zero)
                _vecDirection.Normalize();

            _damageField.IsActive = true;
            _wasFollowing = _isFollowing | !Camera.ClassicMode;
            _isFollowing = false;
        }

        private bool OnPush(Vector2 direction, PushableComponent.PushType type)
        {
            if (type == PushableComponent.PushType.Impact)
                _body.Velocity = new Vector3(direction.X * 1.5f, direction.Y * 1.5f, _body.Velocity.Z);

            return true;
        }

        private void OnCollision(Values.BodyCollision direction)
        {
            // this is used so that the speed is not lost while sliding on a wall
            // not sure if this could be done better
            if (_wasFollowing)
            {
                if ((direction & Values.BodyCollision.Horizontal) != 0)
                {
                    var ratio = Math.Abs(_vecDirection.X) / Math.Abs(_vecDirection.Y);
                    if (1 < ratio && ratio < 25)
                    {
                        _vecDirection.X = 0;
                        _vecDirection.Y *= ratio;
                    }
                }
                else if ((direction & Values.BodyCollision.Vertical) != 0)
                {
                    var ratio = Math.Abs(_vecDirection.Y) / Math.Abs(_vecDirection.X);
                    if (1 < ratio && ratio < 25)
                    {
                        _vecDirection.X *= ratio;
                        _vecDirection.Y = 0;
                    }
                }
                return;
            }
            _body.VelocityTarget = Vector2.Zero;

            // collide with a wall
            if ((direction & Values.BodyCollision.Horizontal) != 0)
                _vecDirection.X = -_vecDirection.X;
            else if ((direction & Values.BodyCollision.Vertical) != 0)
                _vecDirection.Y = -_vecDirection.Y;
        }

        private void OnHoleAbsorb()
        {
            _animator.SpeedMultiplier = 2.0f;
            _animator.Play("walk");
        }

        private void OnHoleDeath()
        {
            Map.Objects.SpawnObject(new EnemyHardhatBeetleRespawner(Map, (int)ResetPosition.X - 8, (int)ResetPosition.Y - 16, _body.FieldRectangle));
        }

        private Values.HitCollision OnHit(GameObject gameObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
        {
            // Because of the way the hit system works, this needs to be in any hit that doesn't default to "None" hit collision.
            if ((hitType & HitType.CrystalSmash) != 0 || (hitType & HitType.ClassicSword) != 0)
                return Values.HitCollision.None;

            if (_damageState.IsInDamageState())
                return Values.HitCollision.None;

            if (hitType == HitType.Bomb || hitType == HitType.BowWow)
            {
                _damageState.SpawnItem = "bomb_1";
                return _damageState.OnHit(gameObject, direction, hitType, damage, pieceOfPower);
            }

            if (hitType == HitType.Boomerang || hitType == HitType.Hookshot)
            {
                _body.VelocityTarget = Vector2.Zero;
                _animator.Play("stunned");
                _stunnedState.StartStun();
                _damageField.IsActive = false;
            }
            // Allows knockback effect from piece of power or red tunic.
            if (pieceOfPower)
                return _damageState.OnHit(gameObject, direction, hitType, 0, pieceOfPower);

            _damageState.SetDamageState(false);
            _body.Velocity.X = direction.X * 3.0f;
            _body.Velocity.Y = direction.Y * 3.0f;
            Game1.GameManager.PlaySoundEffect("D360-09-09");
            return Values.HitCollision.Enemy;
        }
    }
}