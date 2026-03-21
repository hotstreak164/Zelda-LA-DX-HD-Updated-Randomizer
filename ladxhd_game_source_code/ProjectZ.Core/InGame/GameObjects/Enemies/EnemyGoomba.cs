using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.GameObjects.Base.Components.AI;
using ProjectZ.InGame.GameObjects.Effects;
using ProjectZ.InGame.GameObjects.Things;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Enemies
{
    internal class EnemyGoomba : GameObject
    {
        private readonly CSprite _sprite;
        private readonly BodyComponent _body;
        private readonly AiComponent _aiComponent;
        private readonly Animator _animator;
        private readonly BoxCollisionComponent _bodyCollision;
        private readonly DamageFieldComponent _damageField;
        private readonly HittableComponent _hitComponent;
        private readonly PushableComponent _pushComponent;
        private readonly AiDamageState _damageState;

        private int _lives = EnemyLives.Goomba;
        private int FadeTime = 75;
        private float _directionCounter;
        private const float WalkSpeed = 0.5f;

        public EnemyGoomba() : base("goomba") { }

        public EnemyGoomba(Map.Map map, int posX, int posY) : base(map)
        {
            Tags = Values.GameObjectTag.Enemy;

            EntityPosition = new CPosition(posX + 8, posY + 16, 0);
            ResetPosition  = new CPosition(posX + 8, posY + 16, 0);
            EntitySize = new Rectangle(-8, -16, 16, 16);
            CanReset = true;
            OnReset = Reset;

            _animator = AnimatorSaveLoad.LoadAnimator("Enemies/goomba");
            _animator.Play("walk");

            _sprite = new CSprite(EntityPosition);
            var animationComponent = new AnimationComponent(_animator, _sprite, new Vector2(-8, Map.Is2dMap ? -14 : -16));

            _body = new BodyComponent(EntityPosition, -6, -11, 12, 11, 8)
            {
                MoveCollision = OnCollision,
                CollisionTypes = Values.CollisionTypes.Normal |
                                 Values.CollisionTypes.Field |
                                 Values.CollisionTypes.Enemy |
                                 Values.CollisionTypes.NPCWall,
                AvoidTypes =     Values.CollisionTypes.Hole,
                FieldRectangle = map.GetField(posX, posY),
                Drag = 0.85f,
                DragAir = 0.85f,
                Gravity2D = 0.15f,
            };

            // random dir -1 or 1
            var dir = Game1.RandomNumber.Next(0, 2) * 2 - 1;
            _body.VelocityTarget.X = dir * WalkSpeed;

            var stateWalking = new AiState(UpdateWalking) { Init = InitWalking };
            var stateDead = new AiState();
            stateDead.Trigger.Add(new AiTriggerCountdown(1000 - FadeTime, null, () => _aiComponent.ChangeState("fade")));
            var stateFade = new AiState() { Init = InitFade };
            stateFade.Trigger.Add(new AiTriggerCountdown(FadeTime, DespawnTick, RemoveEntity));

            _aiComponent = new AiComponent();
            _aiComponent.States.Add("walking", stateWalking);
            _aiComponent.States.Add("dead", stateDead);
            _aiComponent.States.Add("fade", stateFade);
            new AiFallState(_aiComponent, _body, OnHoleAbsorb);
            _damageState = new AiDamageState(this, _body, _aiComponent, _sprite, _lives) { OnBurn = OnBurn };

            _aiComponent.ChangeState("walking");

            CBox damageCollider;
            if (Map.Is2dMap)
                damageCollider = new CBox(EntityPosition, -3, -8, 0, 6, 6, 4);
            else
                damageCollider = new CBox(EntityPosition, -3, -10, 0, 6, 6, 4);

            AddComponent(DamageFieldComponent.Index, _damageField = new DamageFieldComponent(damageCollider, HitType.Enemy, 2));

            if (Map.Is2dMap)
                _damageState.HitMultiplierY = 1.0f;
            AddComponent(HittableComponent.Index, _hitComponent = new HittableComponent(_body.BodyBox, OnHit));

            AddComponent(BodyComponent.Index, _body);
            AddComponent(AiComponent.Index, _aiComponent);
            AddComponent(BaseAnimationComponent.Index, animationComponent);
            if (Map.Is2dMap)
            {
                var collisionBox = new CBox(EntityPosition, -6, -8, 0, 12, 8, 4);
                AddComponent(CollisionComponent.Index, _bodyCollision = new BoxCollisionComponent(collisionBox, Values.CollisionTypes.Enemy));
            }
            AddComponent(PushableComponent.Index, _pushComponent = new PushableComponent(_body.BodyBox, OnPush));
            AddComponent(DrawComponent.Index, new BodyDrawComponent(_body, _sprite, Values.LayerPlayer));
        }

        public override void Reset()
        {
            _animator.Continue();
            _damageField.IsActive = true;
            _hitComponent.IsActive = true;
            _pushComponent.IsActive = true;
            _animator.Play("walk");
            _aiComponent.ChangeState("walking");
            _aiComponent.ChangeState("walking");

            if (_bodyCollision != null)
                _bodyCollision.IsActive = true;
        }

        private void OnBurn()
        {
            _animator.Pause();
            _damageField.IsActive = false;
            _hitComponent.IsActive = false;
            _pushComponent.IsActive = false;

            if (_bodyCollision != null)
                _bodyCollision.IsActive = false;
        }

        private void InitWalking()
        {
            int randomDirection;
            if (Map.Is2dMap)
                randomDirection = Game1.RandomNumber.Next(0, 2) * 2;
            else
                randomDirection = Game1.RandomNumber.Next(0, 4);

            _body.VelocityTarget = AnimationHelper.DirectionOffset[randomDirection] * WalkSpeed;

            _directionCounter = Game1.RandomNumber.Next(750, 1500);
        }

        private void UpdateWalking()
        {
            // player jumped on top?
            if ((!Map.Is2dMap && MapManager.ObjLink._body.Velocity.Z < 0 ||
                 Map.Is2dMap && MapManager.ObjLink._body.Velocity.Y > 0 && MapManager.ObjLink.CenterPosition.Y < EntityPosition.Y) &&
                 _body.BodyBox.Box.Intersects(MapManager.ObjLink._body.BodyBox.Box))
            {
                JumpDeath();
            }

            if (Map.Is2dMap)
                return;

            _directionCounter -= Game1.DeltaTime;
            // change the direction
            if (_directionCounter < 0)
                _aiComponent.ChangeState("walking");
        }

        private void InitFade()
        {
            var animation = new ObjAnimator(Map,
                (int)EntityPosition.X, (int)EntityPosition.Y - 4, 0, 0, Values.LayerTop, "Particles/despawnParticle", "orange", true);
            Map.Objects.SpawnObject(animation);

            // Spawn a heart if they are not disabled.
            if (!GameSettings.NoHeartDrops)
                Map.Objects.SpawnObject(new ObjItem(Map,
                    (int)EntityPosition.X - 8, (int)EntityPosition.Y - 12, "j", null, "heart", null, true));
        }

        private void DespawnTick(double time)
        {
            if (time <= FadeTime)
                _sprite.Color = Color.White * (float)(time / FadeTime);
        }

        private void RemoveEntity()
        {
            Map.Objects.DeleteObjects.Add(this);
        }

        private bool OnPush(Vector2 direction, PushableComponent.PushType type)
        {
            if (type == PushableComponent.PushType.Impact)
                _body.Velocity = new Vector3(direction.X * 2.5f, direction.Y * 2.5f, _body.Velocity.Z);

            return true;
        }

        private void JumpDeath()
        {
            // player jumped on the goomba
            Game1.GameManager.PlaySoundEffect("D370-14-0E");

            if (Map.Is2dMap)
                MapManager.ObjLink._body.Velocity.Y = -1.0f;
            else
                MapManager.ObjLink._body.Velocity.Z = 1.0f;

            _animator.Play("dead");
            _aiComponent.ChangeState("dead");
            _body.VelocityTarget = Vector2.Zero;
            _damageField.IsActive = false;
            _hitComponent.IsActive = false;
            if (_bodyCollision != null)
                _bodyCollision.IsActive = false;
        }

        private void OnHoleAbsorb()
        {
            _animator.SpeedMultiplier = 3f;
        }

        private void OnCollision(Values.BodyCollision direction)
        {
            if (_aiComponent.CurrentStateId != "walking")
                return;

            if (Map.Is2dMap && (direction & Values.BodyCollision.Horizontal) != 0)
                _body.VelocityTarget.X = -_body.VelocityTarget.X;

            // stop walking into the wall
            if (!Map.Is2dMap && (direction & (Values.BodyCollision.Horizontal | Values.BodyCollision.Vertical)) != 0)
                _aiComponent.ChangeState("walking");
        }

        private Values.HitCollision OnHit(GameObject gameObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
        {
            // Because of the way the hit system works, this needs to be in any hit that doesn't default to "None" hit collision.
            if ((hitType & HitType.CrystalSmash) != 0 || (hitType & HitType.ClassicSword) != 0)
                return Values.HitCollision.None;

            if (_damageState.CurrentLives <= 0)
            {
                _damageField.IsActive = false;
                _hitComponent.IsActive = false;
                _pushComponent.IsActive = false;
            }
            if (hitType == HitType.MagicPowder || hitType == HitType.MagicRod)
                _body.VelocityTarget = Vector2.Zero;

            return _damageState.OnHit(gameObject, direction, hitType, damage, pieceOfPower);
        }
    }
}