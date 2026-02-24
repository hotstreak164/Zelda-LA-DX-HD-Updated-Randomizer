using System;
using Microsoft.Xna.Framework;
using ProjectZ.Base;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.GameObjects.Base.Components.AI;
using ProjectZ.InGame.GameObjects.Dungeon;
using ProjectZ.InGame.GameObjects.Things;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.MidBoss
{
    internal class MBossSmasher : GameObject
    {
        private MBossSmasherBall _ball;

        private readonly BodyComponent _body;
        private readonly AiComponent _aiComponent;
        private readonly AiDamageState _damageState;
        private readonly Animator _animator;
        private readonly CubicBezier _pickupCurveX;
        private readonly CubicBezier _pickupCurveY;
        private readonly DamageFieldComponent _damageField;
        private readonly PushableComponent _pushComponent;
        private readonly HittableComponent _hitComponent;

        private readonly RectangleF _triggerRectangle;
        private Vector2 _moveDirection;

        private readonly string _saveKey;

        private Vector2 _jumpDirection;
        private Vector2 _pickupStart;
        private const int PickupTime = 500;

        private float WalkSpeed = 0.75f;
        private const float CarrySpeed = 0.25f;

        private int _lives = EnemyLives.Smasher;
        private int _direction;
        private int _jumpCount;

        public MBossSmasher(Map.Map map, int posX, int posY, string saveKey) : base(map, "smasher")
        {
            Tags = Values.GameObjectTag.Enemy;

            EntityPosition = new CPosition(posX + 8, posY + 16, 0);
            EntitySize = new Rectangle(-12, -24, 24, 24);

            _saveKey = saveKey;

            if (!string.IsNullOrEmpty(_saveKey) &&
                Game1.GameManager.SaveManager.GetString(_saveKey) == "1")
            {
                IsDead = true;
                return;
            }

            EntityPosition.AddPositionListener(typeof(MBossSmasher), OnPositionChange);

            _pickupCurveX = new CubicBezier(100, new Vector2(0.6f, 0.8f), new Vector2(0.7f, 1));
            _pickupCurveY = new CubicBezier(100, new Vector2(0.15f, 0.55f), new Vector2(0.15f, 1));

            _triggerRectangle = Map.GetField(posX, posY, 16);

            _animator = AnimatorSaveLoad.LoadAnimator("MidBoss/smasher");

            var sprite = new CSprite(EntityPosition);
            var animationComponent = new AnimationComponent(_animator, sprite, new Vector2(0, 0));

            _body = new BodyComponent(EntityPosition, -9, -12, 18, 12, 8)
            {
                MoveCollision = OnCollision,
                Drag = 0.65f,
                DragAir = 0.75f,
                Gravity = -0.125f,
                FieldRectangle = map.GetField(posX, posY)
            };

            var stateWaiting = new AiState(UpdateWaiting) { Init = InitWaiting };
            var stateWalk = new AiState(UpdateWalk) { Init = InitWalk };
            var statePickup = new AiState { Init = InitPickup };
            statePickup.Trigger.Add(new AiTriggerCountdown(PickupTime, TickPickup, PickupEnd));
            var stateCarry = new AiState(UpdateCarry) { Init = InitCarrying };
            var statePostThrow = new AiState();
            statePostThrow.Trigger.Add(new AiTriggerCountdown(550, null, () => _aiComponent.ChangeState("walk")));

            _aiComponent = new AiComponent();
            _aiComponent.Trigger.Add(new AiTriggerUpdate(Update));

            _aiComponent.States.Add("waiting", stateWaiting);
            _aiComponent.States.Add("walk", stateWalk);
            _aiComponent.States.Add("pickup", statePickup);
            _aiComponent.States.Add("carry", stateCarry);
            _aiComponent.States.Add("postThrow", statePostThrow);
            _damageState = new AiDamageState(this, _body, _aiComponent, sprite, _lives) { BossHitSound = true, ExplosionOffsetY = 6, OnLiveZeroed = OnLiveZeroed };
            _damageState.AddBossDamageState(RemoveObject);

            _aiComponent.ChangeState("waiting");

            var damageCollider = new CBox(EntityPosition, -7, -11, 0, 14, 11, 14, true);
            var hittableBox = new CBox(EntityPosition, -9, -18, 0, 18, 18, 16, true);

            AddComponent(DamageFieldComponent.Index, _damageField = new DamageFieldComponent(damageCollider, HitType.Enemy, 4));
            AddComponent(HittableComponent.Index, _hitComponent = new HittableComponent(hittableBox, OnHit));
            AddComponent(BodyComponent.Index, _body);
            AddComponent(AiComponent.Index, _aiComponent);
            AddComponent(PushableComponent.Index, _pushComponent = new PushableComponent(_body.BodyBox, OnPush));
            AddComponent(BaseAnimationComponent.Index, animationComponent);
            AddComponent(DrawComponent.Index, new BodyDrawComponent(_body, sprite, Values.LayerPlayer));
            AddComponent(DrawShadowComponent.Index, new BodyDrawShadowComponent(_body, sprite) { ShadowWidth = 18, ShadowHeight = 6 });

            _moveDirection = new Vector2(-1.2f, 0);
            _animator.Play("idle_0");

            _ball = new MBossSmasherBall(map, new Vector2(EntityPosition.X + 56, EntityPosition.Y + 16));
            map.Objects.SpawnObject(_ball);

            new ObjSpriteShadow(map, this, Values.LayerPlayer, "sprshadowl");
        }

        private void Update()
        {
            // player left?
            if (!_triggerRectangle.Contains(MapManager.ObjLink.BodyRectangle) &&
                _aiComponent.CurrentStateId == "walk" && _body.IsGrounded)
            {
                _aiComponent.ChangeState("waiting");

                // stop boss music
                Game1.GameManager.SetMusic(-1, 2);
            }
        }

        private void InitWaiting()
        {
            _body.VelocityTarget = Vector2.Zero;
        }

        private void UpdateWaiting()
        {
            // awake if the player enters the room
            if (_triggerRectangle.Contains(MapManager.ObjLink.BodyRectangle))
                StartMoving();
        }

        private void StartMoving()
        {
            _aiComponent.ChangeState("walk");

            // start boss music
            Game1.GameManager.SetMusic(79, 2);
        }

        private void InitPickup()
        {
            if (!_ball.InitPickup())
            {
                _aiComponent.ChangeState("walk");
                return;
            }

            Game1.GameManager.PlaySoundEffect("D370-28-1C");

            _pickupStart = new Vector2(_ball.EntityPosition.Position.X, EntityPosition.Y - _ball.EntityPosition.Position.Y);
            _direction = _ball.EntityPosition.Position.X < EntityPosition.X ? 0 : 1;
            _animator.Play("up_" + _direction);
        }

        private void TickPickup(double countdownState)
        {
            var ballTargetPosition = new Vector2(EntityPosition.X, 15);

            // the ball gets picked up in a curved way
            var percentage = (float)((PickupTime - countdownState) / PickupTime);
            var percentageX = _pickupCurveX.EvaluateX(percentage);
            var percentageY = _pickupCurveY.EvaluateX(percentage);
            var newBallPosition = new Vector2(
                MathHelper.Lerp(_pickupStart.X, ballTargetPosition.X, percentageX),
                MathHelper.Lerp(_pickupStart.Y, ballTargetPosition.Y, percentageY));

            _ball.EntityPosition.Set(new Vector3(newBallPosition.X, EntityPosition.Y + 1, newBallPosition.Y));

            var Link = MapManager.ObjLink;
            var lState = Link.CurrentState;

            if (lState == ObjLink.State.Pulling || lState == ObjLink.State.PreCarrying || lState == ObjLink.State.Carrying)
            {
                _ball.DisableDamageField();
                _aiComponent.ChangeState("walk");
                _ball.EndPickup();
            }
        }

        private void PickupEnd()
        {
            _ball.EntityPosition.Set(new Vector3(EntityPosition.X, EntityPosition.Y + 1, 15));
            _aiComponent.ChangeState("carry");
        }

        private void InitWalk()
        {
            _jumpCount = 0;
        }

        private void UpdateWalk()
        {
            if (_body.Velocity.Z < 0)
                _animator.Play("idle_" + _direction);

            if (_body.IsGrounded)
            {
                // jump towards the ball if the player is not already carrying it
                if (_ball.IsAvailable())
                    JumpTowardsBall();
                else
                    JumpRandom();
            }
        }

        private void JumpRandom()
        {
            // When running from the player movement speed is reduced.
            WalkSpeed = 0.60f;

            // Get a random direction to jump towards.
            if (_jumpCount <= 0)
            {
                _jumpCount = Game1.RandomNumber.Next(2, 3);
                var dirX = Game1.RandomNumber.Next(0, 2) * 2 - 1;
                var dirY = Game1.RandomNumber.Next(0, 2) * 2 - 1;
                _jumpDirection = new Vector2(dirX, dirY * 0.5f);
            }
            // Jump playing the directional animation.
            Jump(_jumpDirection, "up_");
            _jumpCount--;
        }

        private void JumpTowardsBall()
        {
            // When running towards the ball movement speed is increased.
            WalkSpeed = 0.75f;

            // jump toward the ball or pick him up if we are close enough
            var targetPosition = new Vector2(_ball.EntityPosition.X, _ball.EntityPosition.Y);
            if (EntityPosition.Position.X < _ball.EntityPosition.X)
            {
                // need to make sure that the target position is not inside the wall where the boss can not reach it
                var offset = Math.Clamp(14 + _body.Width / 2 - (_ball.EntityPosition.X - (_body.FieldRectangle.X + 16)), 0, 32);
                targetPosition.X -= 14 - offset;
            }
            else
            {
                // need to make sure that the target position is not inside the wall where the boss can not reach it
                var offset = Math.Clamp(14 + _body.Width / 2 - ((_body.FieldRectangle.Right - 16) - _ball.EntityPosition.X), 0, 32);
                targetPosition.X += 14 - offset;
            }
            var ballDirection = targetPosition - EntityPosition.Position;

            if (ballDirection.Length() > 5)
            {
                ballDirection.Normalize();
                Jump(ballDirection, "idle_");
            }
            else
            {
                _aiComponent.ChangeState("pickup");
                _body.VelocityTarget = Vector2.Zero;
            }
        }

        private void Jump(Vector2 direction, string animationName)
        {
            _direction = direction.X < 0 ? 0 : 1;
            _animator.Play(animationName + _direction);

            _body.VelocityTarget = direction * WalkSpeed;
            _body.Velocity = new Vector3(0, 0, 0.8f);
        }

        private void OnPositionChange(CPosition newPosition)
        {
            if (_aiComponent.CurrentStateId != "carry")
                return;

            // set the position of the ball if it is carried
            _ball.EntityPosition.Set(new Vector3(newPosition.X, newPosition.Y + 1, newPosition.Z + 15));
        }

        private void InitCarrying()
        {
            _jumpCount = 0;
        }

        private void UpdateCarry()
        {
            // start throwing
            if (_jumpCount > 2 && _body.Velocity.Z < 0)
            {
                ThrowBall();
                return;
            }

            if (_body.IsGrounded)
            {
                _jumpCount++;

                // jump toward the player
                var ballDirection = MapManager.ObjLink.Position - EntityPosition.Position;

                if (ballDirection.Length() > 5)
                {
                    ballDirection.Normalize();
                    _direction = ballDirection.X < 0 ? 0 : 1;
                    _animator.Play("up_" + _direction);

                    _body.VelocityTarget = ballDirection * CarrySpeed;
                    _body.Velocity = new Vector3(0, 0, 0.8f);
                }
            }
        }

        private void ThrowBall()
        {
            _aiComponent.ChangeState("postThrow");

            Game1.GameManager.PlaySoundEffect("D360-08-08");

            _animator.Play("idle_" + _direction);
            _body.Velocity = new Vector3(0, 0, 1.75f);

            // throw towards the player; scale the throw speed depending on the distance of the player
            var playerDirection = MapManager.ObjLink.Position - EntityPosition.Position;
            var playerDistance = playerDirection.Length();
            if (playerDistance > 0)
                playerDirection.Normalize();
            playerDirection *= Math.Clamp(playerDistance / 24, 0, 2.5f);

            var throwDirection = new Vector3(playerDirection, 1.5f);
            _ball.Throw(throwDirection);
        }

        private bool OnPush(Vector2 direction, PushableComponent.PushType type)
        {
            if (type == PushableComponent.PushType.Impact)
                _body.Velocity = new Vector3(direction.X, direction.Y, _body.Velocity.Z);

            return true;
        }

        public Values.HitCollision OnHit(GameObject gameObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
        {
            // Because of the way the hit system works, this needs to be in any hit that doesn't default to "None" hit collision.
            if ((hitType & HitType.CrystalSmash) != 0 || (hitType & HitType.ClassicSword) != 0)
                return Values.HitCollision.None;

            if (_damageState.IsInDamageState())
                return Values.HitCollision.None;

            // The boss was hit with the ball.
            if ((hitType & HitType.ThrownObject) != 0)
            {
                _damageState.OnHit(gameObject, direction, hitType, damage, pieceOfPower);
                _body.VelocityTarget = Vector2.Zero;
            }

    /*  ----------------------------------------------------------------------------
        Note: These behaviors are not accurate to the original game. Hitting the boss with the sword should
              not cause knockback, nor should it drop the ball when hit. Not sure why it was designed this way.

            else if (_aiComponent.CurrentStateId != "pickup")
            {
                _damageState.HitKnockBack(gameObject, direction, hitType, pieceOfPower, false);
            }
            if (_aiComponent.CurrentStateId == "carry")
            {
                _ball.EndPickup();
                _animator.Play("idle_" + _direction);
                _aiComponent.ChangeState("walk");
            }
        ---------------------------------------------------------------------------- */

            // Remove damage box on death.
            if (_damageState.CurrentLives <= 0)
            {
                _damageField.IsActive = false;
                _hitComponent.IsActive = false;
                _pushComponent.IsActive = false;
            }
            return Values.HitCollision.RepellingParticle;
        }

        private void OnCollision(Values.BodyCollision direction)
        {
            if (_aiComponent.CurrentStateId == "jumping" && (direction & Values.BodyCollision.Horizontal) != 0 &&
                Math.Sign(_body.Velocity.X) == Math.Sign(_moveDirection.X))
            {
                _aiComponent.ChangeState("pushing");
                _moveDirection.X = -_moveDirection.X;
                _body.Velocity.X = -_body.Velocity.X * 0.125f;
                _animator.Play("idle_" + (_moveDirection.X < 0 ? 0 : 1));
            }

            // landed after a jump?
            if ((direction & Values.BodyCollision.Floor) != 0)
            {
                if (_aiComponent.CurrentStateId == "jumping")
                    _aiComponent.ChangeState("idle");
            }
        }

        private void OnLiveZeroed()
        {
            // destroy the ball
            if (_ball != null)
            {
                _ball.Destroy();
                _ball = null;
            }
        }

        private void RemoveObject()
        {
            if (!string.IsNullOrEmpty(_saveKey))
                Game1.GameManager.SaveManager.SetString(_saveKey, "1");

            // stop boss music
            Game1.GameManager.SetMusic(-1, 2);

            // spawns a fairy
            Game1.GameManager.PlaySoundEffect("D360-27-1B");
            Map.Objects.SpawnObject(new ObjDungeonFairy(Map, (int)EntityPosition.X, (int)EntityPosition.Y, 8));

            Map.Objects.DeleteObjects.Add(this);
        }
    }
}