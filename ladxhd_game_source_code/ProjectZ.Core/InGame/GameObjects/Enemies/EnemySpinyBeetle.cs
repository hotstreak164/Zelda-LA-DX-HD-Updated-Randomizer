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
    internal class EnemySpinyBeetle : GameObject
    {
        public override bool IsActive
        {
            set
            {
                base.IsActive = value;
                _carriedObject.IsActive = value;
            }
        }
        private readonly GameObject _carriedObject;
        private readonly CarriableComponent _carriableComponent;
        private readonly HittableComponent _hitComponent;
        private readonly PushableComponent _pushComponent;
        private readonly BodyComponent _body;
        private readonly AiComponent _aiComponent;
        private readonly Animator _animator;
        private readonly CSprite _sprite;
        private readonly AiDamageState _aiDamageState;
        private readonly AiTriggerTimer _hiddenTimer;
        private readonly DamageFieldComponent _damageField;

        private Rectangle _fieldRectangle;

        // 0: Grass ; 1: Stone ; 2: skull
        private readonly int _type;
        private bool _objectPickedUp;
        private bool _objectDestroyed;
        private int _lives = EnemyLives.SpinyBeetle;

        private float moveTimer;

        public EnemySpinyBeetle() : base("spiny beetle") { }

        public EnemySpinyBeetle(Map.Map map, int posX, int posY, int type) : base(map)
        {
            Tags = Values.GameObjectTag.Enemy;

            EntityPosition = new CPosition(posX + 8, posY + 7, 0);
            ResetPosition  = new CPosition(posX + 8, posY + 7, 0);
            EntitySize = new Rectangle(-6, -2, 12, 10);
            CanReset = true;
            OnReset = Reset;

            _animator = AnimatorSaveLoad.LoadAnimator("Enemies/spiny beetle");
            _animator.Play("idle");

            _sprite = new CSprite(EntityPosition);
            var animationComponent = new AnimationComponent(_animator, _sprite, new Vector2(-8, -4));

            _fieldRectangle = map.GetField(posX, posY);

            _body = new BodyComponent(EntityPosition, -6, -2, 12, 10, 8)
            {
                MoveCollision = OnCollision,
                HoleAbsorb = OnHoleAbsorb,
                Drag = 0.8f,
                CollisionTypes = Values.CollisionTypes.Normal |
                                 Values.CollisionTypes.Field,
                AvoidTypes =     Values.CollisionTypes.Hole |
                                 Values.CollisionTypes.NPCWall,
                FieldRectangle = _fieldRectangle
            };

            // spawn a bush carried by the beetle
            if (type == 0)
                _carriedObject = new ObjBush(map, posX, posY, null, "bush_0", true, true, false, Values.LayerPlayer, null) { NoRespawn = true };
            else if (type == 1)
                _carriedObject = new ObjStone(map, posX, posY, "stone_0", null, null, null, false, false) { NoRespawn = true };
            else
                _carriedObject = new ObjStone(map, posX, posY, "skull", null, null, null, false, false) { NoRespawn = true };

            _type = type;

            // deactivate physics
            var body = (BodyComponent)_carriedObject.Components[BodyComponent.Index];
            if (body != null)
                body.IsActive = false;

            // For some reason the "PickedUp" value doesn't go true when picking up the object so store a custom value.
            _carriableComponent = (CarriableComponent)_carriedObject.Components[CarriableComponent.Index];
            _carriableComponent.Pull = (Vector2 e) => { return CarriableObjectPickedUp(); };
            _objectPickedUp = false;

            var stateInit = new AiState(UpdateInit);
            stateInit.Trigger.Add(new AiTriggerCountdown(500, null, () => _aiComponent.ChangeState("hiding")));

            var stateHiding = new AiState(UpdateHiding);
            stateHiding.Trigger.Add(_hiddenTimer = new AiTriggerTimer(650));

            var stateMoving = new AiState(UpdateMoving);
            //stateMoving.Trigger.Add(new AiTriggerRandomTime(ToHide, 0, 10));

            var stateRunning = new AiState();
            stateRunning.Trigger.Add(new AiTriggerRandomTime(ChangeDirection, 500, 650));

            _aiComponent = new AiComponent();
            _aiComponent.States.Add("init", stateInit);
            _aiComponent.States.Add("hiding", stateHiding);
            _aiComponent.States.Add("moving", stateMoving);
            _aiComponent.States.Add("running", stateRunning);
            new AiFallState(_aiComponent, _body);

            _aiDamageState = new AiDamageState(this, _body, _aiComponent, _sprite, _lives) { OnDeath = OnDeath, OnBurn = OnBurn };
            _aiComponent.ChangeState("moving");

            var damageCollider = new CBox(EntityPosition, -5, -2, 0, 10, 10, 4);
            var hittableRectangle = new CBox(EntityPosition, -5, -2, 10, 10, 8);

            AddComponent(DamageFieldComponent.Index, _damageField = new DamageFieldComponent(damageCollider, HitType.Enemy, 2));
            AddComponent(HittableComponent.Index, _hitComponent = new HittableComponent(hittableRectangle, OnHit));
            AddComponent(BodyComponent.Index, _body);
            AddComponent(PushableComponent.Index, _pushComponent = new PushableComponent(_body.BodyBox, OnPush));
            AddComponent(AiComponent.Index, _aiComponent);
            AddComponent(BaseAnimationComponent.Index, animationComponent);
            AddComponent(DrawComponent.Index, new BodyDrawComponent(_body, _sprite, Values.LayerPlayer));
            AddComponent(DrawShadowComponent.Index, new DrawShadowCSpriteComponent(_sprite));

            EntityPosition.AddPositionListener(typeof(EnemySpinyBeetle), UpdateObjPosition);
            map.Objects.SpawnObject(_carriedObject);
            UpdateObjPosition(EntityPosition);

            ToHide();

            _aiComponent.ChangeState("init");
        }

        private void Reset()
        {
            // Delete carried object only if still on beetle's back.
            if (!_objectPickedUp && !_objectDestroyed)
               Map.Objects.DeleteObjects.Add(_carriedObject);

            // Always delete the beetle and spawn a new one.
            Map.Objects.DeleteObjects.Add(this);
            Map.Objects.SpawnObject(new EnemySpinyBeetle(Map, (int)ResetPosition.X - 8, (int)ResetPosition.Y - 7, _type));
        }

        private void OnBurn()
        {
            _animator.Pause();
            _damageField.IsActive = false;
            _hitComponent.IsActive = false;
            _pushComponent.IsActive = false;
        }

        private bool CarriableObjectPickedUp()
        {
            _objectPickedUp = true;
            return true;
        }

        private void UpdateObjPosition(CPosition newPosition)
        {
            if (_aiComponent.CurrentStateId != "hiding" && _aiComponent.CurrentStateId != "moving")
                return;

            var offset = _aiComponent.CurrentStateId == "hiding" ? 0 : 4;
            var offsetY = _type == 0 ? 1 : 6;
            _carriedObject.EntityPosition.Set(new CPosition(newPosition.X, newPosition.Y + offsetY, newPosition.Z + offset));
        }

        private int PlayerDirection()
        {
            var distance = MapManager.ObjLink.Position - (EntityPosition.Position + new Vector2(0, 9));

            if (_fieldRectangle.Contains(MapManager.ObjLink.PosX, MapManager.ObjLink.PosY))
            {
                const float axisTolerance  = 8f;
                const float detectionRange = 160f;

                if (Math.Abs(distance.Y) < axisTolerance && distance.Length() < detectionRange)
                    return Math.Sign(distance.X) < 0 ? 0 : 2;

                if (Math.Abs(distance.X) < axisTolerance && distance.Y > 0 && distance.Y < detectionRange)
                    return 3;
            }
            return -1;
        }

        private void ToHide()
        {
            if (_carriedObject.IsDead || _objectDestroyed || (_carriableComponent != null && (_objectPickedUp)))
                return;

            if (_aiComponent.CurrentStateId != "moving" || (PlayerDirection() >= 0 && _body.LastVelocityCollision == 0))
                return;

            _damageField.IsActive = false;
            _body.VelocityTarget = Vector2.Zero;
            _sprite.IsVisible = false;
            _aiComponent.ChangeState("hiding");

            UpdateObjPosition(EntityPosition);
        }

        private void CheckCarrier()
        {
            // Object was destroyed or picked up?
            if (_carriedObject.IsDead || _objectDestroyed || (_carriableComponent != null && (_objectPickedUp)))
            {
                ToRunning();
                _body.VelocityTarget = Vector2.Zero;
            }
        }

        private void UpdateInit()
        {
            CheckCarrier();
        }

        private void UpdateMoving()
        {
            moveTimer += Game1.DeltaTime;

            if (moveTimer > 750)
            {
                moveTimer = 0f;
                _hiddenTimer.Reset();
                _damageField.IsActive = false;
                _body.VelocityTarget = Vector2.Zero;
                _sprite.IsVisible = false;
                _aiComponent.ChangeState("hiding");
                UpdateObjPosition(EntityPosition);
            }
            CheckCarrier();
        }

        private void UpdateHiding()
        {
            var playerDirection = PlayerDirection();
            if (playerDirection >= 0 && _hiddenTimer.State)
            {
                ToWalk();
                _body.VelocityTarget = AnimationHelper.DirectionOffset[playerDirection];
                moveTimer = 0f;
            }
            CheckCarrier();
        }

        private void Show()
        {
            _sprite.IsVisible = true;
            _damageField.IsActive = true;
        }

        private void ToWalk()
        {
            Show();
            UpdateObjPosition(EntityPosition);
            _aiComponent.ChangeState("moving");
        }

        private void ToRunning()
        {
            Show();
            ChangeDirection();
            _aiComponent.ChangeState("running");
        }

        private void ChangeDirection()
        {
            var randomDir = Game1.RandomNumber.Next(0, 100);
            var directionRadius = (float)(Math.PI * 2 * (randomDir / 100.0f));
            _body.VelocityTarget = new Vector2((float)Math.Cos(directionRadius), (float)Math.Sin(directionRadius));
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

            // Conditions to break the "skull" type beetle with level 2 sword.
            var lvl2SwordSkullBreak = _type == 2 && GameSettings.SwBreakPots && 
                ((hitType & HitType.Sword2) != 0 || 
                Game1.GameManager.GetItem("sword2") != null && ((hitType & HitType.SwordShot) != 0 || 
                (hitType & HitType.PegasusBootsSword) != 0));

            // If it's a bush it can always be destroyed. If it's a skull, check the conditions laid out above.
            if (!_objectDestroyed && (_type == 0 || lvl2SwordSkullBreak))
            {
                _objectDestroyed = true;

                if (!_carriedObject.IsDead && _carriedObject.GetType() == typeof(ObjBush))
                    ((ObjBush)_carriedObject).DestroyBush(direction);

                if (!_carriedObject.IsDead && _carriedObject.GetType() == typeof(ObjStone))
                    ((ObjStone)_carriedObject).OnCollision();

                if (hitType == HitType.Bomb || hitType == HitType.Bow || hitType == HitType.Hookshot)
                    return Values.HitCollision.Blocking;
            }
            // Attacks get repelled by stone/skull.
            if (_type > 0 && !_objectPickedUp && !_objectDestroyed)
            {
                _body.Velocity = new Vector3(direction.X * 0.25f, direction.Y * 0.25f, _body.Velocity.Z);
                return Values.HitCollision.RepellingParticle;
            }
            // Object has been removed and beetle is vulnerable.
            _sprite.IsVisible = true;

            return _aiDamageState.OnHit(gameObject, direction, hitType, damage, pieceOfPower);
        }

        private void OnDeath(bool pieceOfPower)
        {
            Map.Objects.SpawnObject(new EnemySpinyBeetleRespawner(Map, (int)ResetPosition.X - 8, (int)ResetPosition.Y - 7, _type, _fieldRectangle));
            _aiDamageState.BaseOnDeath(pieceOfPower);
        }

        private void OnCollision(Values.BodyCollision direction)
        {
            // Collided with a wall?
            if ((direction & (Values.BodyCollision.Horizontal | Values.BodyCollision.Vertical)) != 0)
                ToHide();
        }

        private void OnHoleAbsorb()
        {
            _animator.SpeedMultiplier = 2.0f;
        }
    }
}