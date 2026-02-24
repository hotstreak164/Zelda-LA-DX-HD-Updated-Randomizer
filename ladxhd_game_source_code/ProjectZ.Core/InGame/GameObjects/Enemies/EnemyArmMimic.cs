using System;
using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.GameObjects.Base.Components.AI;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Enemies
{
    internal class EnemyArmMimic : GameObject
    {
        private readonly BodyComponent _body;
        private readonly Animator _animator;
        private readonly AiComponent _aiComponent;
        private readonly AiDamageState _damageState;
        private readonly AiTriggerTimer _repelTimer;
        private readonly AiStunnedState _aiStunnedState;
        private readonly DamageFieldComponent _damageField;
        private readonly HittableComponent _hitComponent;
        private readonly PushableComponent _pushComponent;

        private Vector2 _lastPosition;
        private int _direction;
        private bool _wasColliding;
        private int _lives = EnemyLives.ArmMimic;

        public EnemyArmMimic() : base("armMimic") { }

        public EnemyArmMimic(Map.Map map, int posX, int posY) : base(map)
        {
            Tags = Values.GameObjectTag.Enemy;

            EntityPosition = new CPosition(posX + 8, posY + 16, 0);
            ResetPosition  = new CPosition(posX + 8, posY + 16, 0);
            EntitySize = new Rectangle(-8, -16, 16, 16);
            CanReset = true;
            OnReset = Reset;

            _animator = AnimatorSaveLoad.LoadAnimator("Enemies/arm mimic");

            var sprite = new CSprite(EntityPosition);
            var animatorComponent = new AnimationComponent(_animator, sprite, new Vector2(-8, -16));

            _body = new BodyComponent(EntityPosition, -7, -14, 14, 14, 4)
            {
                FieldRectangle = map.GetField(posX, posY),
                CollisionTypes = Values.CollisionTypes.Normal |
                                 Values.CollisionTypes.Enemy |
                                 Values.CollisionTypes.Field,
                AvoidTypes =     Values.CollisionTypes.Hole | 
                                 Values.CollisionTypes.NPCWall,
                IsSlider = true,
                AbsorbPercentage = 0.75f,
                MaxSlideDistance = 4.0f
            };
            var stateUpdate = new AiState(Update);

            _aiComponent = new AiComponent();
            _aiComponent.Trigger.Add(_repelTimer = new AiTriggerTimer(500));

            _aiComponent.States.Add("idle", stateUpdate);
            new AiFallState(_aiComponent, _body, null, null, 300);
            _damageState = new AiDamageState(this, _body, _aiComponent, sprite, _lives) { OnBurn = OnBurn };
            _aiStunnedState = new AiStunnedState(_aiComponent, animatorComponent, 3300, 900);

            _aiComponent.ChangeState("idle");

            var hittableBox = new CBox(EntityPosition, -6, -12, 0, 12, 12, 4);
            var damageBox   = new CBox(EntityPosition, -4, -8,  0,  8,  8, 4);
            var pushableBox = new CBox(EntityPosition, -5, -10, 0, 10, 10, 4);

            AddComponent(DamageFieldComponent.Index, _damageField = new DamageFieldComponent(hittableBox, HitType.Enemy, 12));
            AddComponent(HittableComponent.Index, _hitComponent = new HittableComponent(hittableBox, OnHit));
            AddComponent(AiComponent.Index, _aiComponent);
            AddComponent(BodyComponent.Index, _body);
            AddComponent(BaseAnimationComponent.Index, animatorComponent);
            AddComponent(PushableComponent.Index, _pushComponent = new PushableComponent(pushableBox, OnPush));
            AddComponent(DrawComponent.Index, new BodyDrawComponent(_body, sprite, Values.LayerPlayer));
            AddComponent(DrawShadowComponent.Index, new BodyDrawShadowComponent(_body, sprite));
        }

        private void Reset()
        {
            _animator.Continue();
            _damageField.IsActive = true;
            _hitComponent.IsActive = true;
            _pushComponent.IsActive = true;
            _aiComponent.ChangeState("idle");
            _aiComponent.ChangeState("idle");
            _damageState.CurrentLives = EnemyLives.ArmMimic;
            _body.VelocityTarget = Vector2.Zero;
        }

        private void OnBurn()
        {
            _animator.Pause();
            _damageField.IsActive = false;
            _hitComponent.IsActive = false;
            _pushComponent.IsActive = false;
        }

        private void Update()
        {
            // Tracks if they moved for playing animation.
            var moved = false;

            // Stunning can disable damage field so reactivate it.
            if (!_aiStunnedState.Active)
                _damageField.IsActive = true;

            // Move when Link is in the same field as the Arm Mimic.
            if (_body.FieldRectangle.Contains(MapManager.ObjLink.CenterPosition.Position))
            {
                if (_wasColliding)
                {
                    var moveVelocity = -MapManager.ObjLink.LastMoveVector;
                    var diff = (MapManager.ObjLink.Position - _lastPosition) / Game1.TimeMultiplier;

                    // Stops the enemy if the player runs into an obstacle.
                    moveVelocity = new Vector2(
                        Math.Min(Math.Abs(moveVelocity.X), Math.Abs(diff.X)) * Math.Sign(moveVelocity.X),
                        Math.Min(Math.Abs(moveVelocity.Y), Math.Abs(diff.Y)) * Math.Sign(moveVelocity.Y));

                    _body.VelocityTarget = moveVelocity;

                    if (moveVelocity.Length() > 0.01f)
                    {
                        moved = true;

                        // Use the direction from ObjLink instead of AnimationHelper since it
                        // has "bias" built into the four directions (fixes diagonal movement).
                        if (!MapManager.ObjLink.IsChargingState())
                            _direction = MapManager.ObjLink.ToDirection(moveVelocity);

                        if (_animator.CurrentAnimation.Id != "walk_" + _direction)
                            _animator.Play("walk_" + _direction);
                        else
                            _animator.Continue();
                    }
                }
                _wasColliding = true;
                _lastPosition = MapManager.ObjLink.Position;
            }
            else
            {
                _wasColliding = false;
                _body.VelocityTarget = Vector2.Zero;
            }
            if (!moved)
                _animator.Pause();
        }

        private bool OnPush(Vector2 direction, PushableComponent.PushType type)
        {
            if (type == PushableComponent.PushType.Impact)
                _body.Velocity = new Vector3(direction.X * 2.5f, direction.Y * 2.5f, _body.Velocity.Z);

            return true;
        }

        private Values.HitCollision OnHit(GameObject gameObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
        {
            // Because of the way the hit system works, this needs to be in any hit that doesn't default to "None" hit collision.
            if ((hitType & HitType.CrystalSmash) != 0 || (hitType & HitType.ClassicSword) != 0)
                return Values.HitCollision.None;

            if (!_repelTimer.State)
                return Values.HitCollision.None;
            _repelTimer.Reset();

            // Magic Powder does nothing.
            if (hitType == HitType.MagicPowder)
            {
                return Values.HitCollision.None;
            }

            // stun state
            if (hitType == HitType.Hookshot || hitType == HitType.Boomerang || hitType == HitType.ThrownObject)
            {
                _body.VelocityTarget = Vector2.Zero;
                _damageField.IsActive = false;
                _body.Velocity.X += direction.X * 4.0f;
                _body.Velocity.Y += direction.Y * 4.0f;

                _animator.Pause();
                _aiStunnedState.StartStun();

                return Values.HitCollision.Enemy;
            }

            // damaged not from the front; piece of power or while using pegasus boots
            if (hitType != HitType.PegasusBootsSword && hitType != HitType.SwordShot && (hitType & HitType.SwordSpin) == 0 && !pieceOfPower)
                damage = 0;

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