using System;
using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components.AI;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Enemies
{
    internal class EnemyMaskMimic : GameObject
    {
        private readonly BodyComponent _body;
        private readonly AiComponent _aiComponent;
        private readonly Animator _animator;
        private readonly AnimationComponent _animatorComponent;
        private readonly AiDamageState _aiDamageState;
        private readonly AiStunnedState _aiStunnedState;
        private readonly DamageFieldComponent _damageField;
        private readonly HittableComponent _hitComponent;
        private readonly PushableComponent _pushComponent;

        private readonly Rectangle _fieldRectangle;

        private Vector2 _lastPosition;
        private int _direction;
        private bool _wasColliding;
        private int _lives = EnemyLives.MaskMimic;

        public EnemyMaskMimic() : base("mask mimic") { }

        public EnemyMaskMimic(Map.Map map, int posX, int posY) : base(map)
        {
            Tags = Values.GameObjectTag.Enemy;

            EntityPosition = new CPosition(posX + 8, posY + 16, 0);
            ResetPosition  = new CPosition(posX + 8, posY + 16, 0);
            EntitySize = new Rectangle(-8, -16, 16, 16);
            CanReset = true;
            OnReset = Reset;

            _animator = AnimatorSaveLoad.LoadAnimator("Enemies/mask mimic");
            _animator.Play("walk");

            var sprite = new CSprite(EntityPosition);
            _animatorComponent = new AnimationComponent(_animator, sprite, Vector2.Zero);

            _body = new BodyComponent(EntityPosition, -7, -12, 14, 12, 8)
            {
                Gravity = -0.075f,
                DragAir = 1.0f,
                CollisionTypes = Values.CollisionTypes.Normal |
                                 Values.CollisionTypes.Field,
                AvoidTypes =     Values.CollisionTypes.Hole | 
                                 Values.CollisionTypes.NPCWall,
                FieldRectangle = map.GetField(posX, posY),
                IsSlider = true,
                MaxSlideDistance = 4.0f
            };

            _aiComponent = new AiComponent();

            var stateUpdate = new AiState(Update);

            _aiComponent.States.Add("idle", stateUpdate);
            _aiStunnedState = new AiStunnedState(_aiComponent, _animatorComponent, 3300, 900);
            new AiFallState(_aiComponent, _body, null, null, 300);
            _aiDamageState = new AiDamageState(this, _body, _aiComponent, sprite, _lives) { OnBurn = OnBurn };
            _aiComponent.ChangeState("idle");

            var damageBox = new CBox(EntityPosition, -7, -15, 2, 14, 15, 4);
            var hittableBox = new CBox(EntityPosition, -7, -15, 2, 14, 15, 8);
            var pushableBox = new CBox(EntityPosition, -7, -14, 2, 14, 14, 8);

            AddComponent(PushableComponent.Index, _pushComponent = new PushableComponent(pushableBox, OnPush));
            AddComponent(DamageFieldComponent.Index, _damageField = new DamageFieldComponent(damageBox, HitType.Enemy, 2));
            AddComponent(HittableComponent.Index, _hitComponent = new HittableComponent(hittableBox, OnHit));
            AddComponent(AiComponent.Index, _aiComponent);
            AddComponent(BodyComponent.Index, _body);
            AddComponent(BaseAnimationComponent.Index, _animatorComponent);
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
            _aiDamageState.CurrentLives = EnemyLives.MaskMimic;
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

            // Move when Link is in the same field as the Mask Mimic.
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

                    _body.VelocityTarget = moveVelocity * 0.75f;

                    if (moveVelocity.Length() > 0.01f)
                    {
                        moved = true;

                        if (!MapManager.ObjLink.IsChargingState())
                        {
                            // deadzone to not have a fixed point where the direction gets changed
                            if (Math.Abs(moveVelocity.X) * ((_direction % 2 == 0) ? 1.1f : 1f) >
                                Math.Abs(moveVelocity.Y) * ((_direction % 2 != 0) ? 1.1f : 1f))
                                _direction = moveVelocity.X < 0 ? 0 : 2;
                            else
                                _direction = moveVelocity.Y < 0 ? 1 : 3;
                        }
                        var playAnimation = "walk_" + _direction;

                        if (_animator.CurrentAnimation.Id != playAnimation)
                            _animator.Play(playAnimation);
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

            if (hitType == HitType.MagicPowder)
                return Values.HitCollision.None;

            if (hitType == HitType.Bow)
                damage = 1;

            if (hitType == HitType.Hookshot || hitType == HitType.Boomerang)
            {
                _aiStunnedState.StartStun();
                _damageField.IsActive = false;

                _body.VelocityTarget = Vector2.Zero;

                _body.Velocity.X = direction.X * 5;
                _body.Velocity.Y = direction.Y * 5;

                return Values.HitCollision.Enemy;
            }

            // can be hit if the damage source is coming from the back
            var dir = AnimationHelper.GetDirection(direction);
            if (dir == _direction ||
                hitType == HitType.Bomb ||
                hitType == HitType.Bow ||
                hitType == HitType.MagicRod)
            {
                return _aiDamageState.OnHit(gameObject, direction, hitType, damage, pieceOfPower);
            }

            return Values.HitCollision.RepellingParticle | Values.HitCollision.Repelling1;
        }
    }
}