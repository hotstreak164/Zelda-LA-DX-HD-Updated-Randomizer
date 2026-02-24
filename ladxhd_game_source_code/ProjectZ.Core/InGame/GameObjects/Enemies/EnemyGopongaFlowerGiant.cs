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
    internal class EnemyGopongaFlowerGiant : GameObject
    {
        private readonly Animator _animator;
        private readonly DamageFieldComponent _damageField;
        private readonly AiDamageState _damageState;
        private readonly AiComponent _aiComponent;
        private readonly HittableComponent _hitComponent;
        private readonly PushableComponent _pushComponent;
        private BoxCollisionComponent _collisionComponent;

        private CBox _collisionBox;

        private bool _dealsDamage = true;
        private int _lives = EnemyLives.GopongaGiant;

        private float _soundCooldown;
        private bool _blockSound = false;

        public EnemyGopongaFlowerGiant() : base("giant goponga flower") { }

        public EnemyGopongaFlowerGiant(Map.Map map, int posX, int posY) : base(map)
        {
            Tags = Values.GameObjectTag.Enemy;

            EntityPosition = new CPosition(posX + 16, posY + 16, 0);
            ResetPosition  = new CPosition(posX + 8, posY + 8, 0);
            EntitySize = new Rectangle(-16, -16, 32, 32);
            CanReset = false;
            OnReset = Reset;

            _animator = AnimatorSaveLoad.LoadAnimator("Enemies/goponga flower giant");
            _animator.OnAnimationFinished = AnimationFinished;
            _animator.Play("idle");

            var sprite = new CSprite(EntityPosition);
            var animationComponent = new AnimationComponent(_animator, sprite, new Vector2(-16, -16));

            var body = new BodyComponent(EntityPosition, -14, -12, 28, 28, 8) 
            {
                CollisionTypes = Values.CollisionTypes.Normal |
                                 Values.CollisionTypes.Field,
                IgnoresZ = true 
            };

            _aiComponent = new AiComponent();
            _aiComponent.States.Add("idle", new AiState(() => { }));
            _damageState = new AiDamageState(this, body, _aiComponent, sprite, _lives)
            {
                HitMultiplierX = 0,
                HitMultiplierY = 0,
                FlameOffset = new Point(0, -8),
                OnBurn = OnBurn
            };
            _aiComponent.ChangeState("idle");

            _collisionBox = new CBox(EntityPosition, -14, -14, 28, 28, 8);
            var hittableBox = new CBox(EntityPosition, -15, -15, 30, 30, 8);
            var damageBox = new CBox(EntityPosition, -15, -15, 30, 30, 8);

            AddComponent(AiComponent.Index, _aiComponent);
            AddComponent(CollisionComponent.Index, _collisionComponent = new BoxCollisionComponent(_collisionBox, Values.CollisionTypes.Enemy));
            AddComponent(HittableComponent.Index, _hitComponent = new HittableComponent(hittableBox, OnHit));
            AddComponent(DamageFieldComponent.Index, _damageField = new DamageFieldComponent(damageBox, HitType.Enemy, 4));
            AddComponent(BodyComponent.Index, body);
            AddComponent(BaseAnimationComponent.Index, animationComponent);
            AddComponent(UpdateComponent.Index, new UpdateComponent(Update));
            AddComponent(PushableComponent.Index, _pushComponent = new PushableComponent(body.BodyBox, OnPush));
            AddComponent(DrawComponent.Index, new BodyDrawComponent(body, sprite, Values.LayerPlayer) { WaterOutline = false });
        }

        private void Reset()
        {
            _animator.Continue();
            _damageField.IsActive = true;
            _hitComponent.IsActive = true;
            _pushComponent.IsActive = true;
            _aiComponent.ChangeState("idle");
            _aiComponent.ChangeState("idle");

            if (_collisionComponent == null)
                AddComponent(CollisionComponent.Index, _collisionComponent = new BoxCollisionComponent(_collisionBox, Values.CollisionTypes.Enemy));
        }

        private void OnBurn()
        {
            _animator.Pause();
            _dealsDamage = false;
            _damageField.IsActive = false;
            _hitComponent.IsActive = false;
            _pushComponent.IsActive = false;
            RemoveComponent(CollisionComponent.Index);
            _collisionComponent = null;
        }

        private void Update()
        {
            if (_blockSound)
            {
                _soundCooldown += Game1.DeltaTime;
                if (_soundCooldown > 220) 
                {
                    _soundCooldown = 0;
                    _blockSound = false;
                }
            }
        }

        private bool ValidateHit(HitType hitType, bool pieceOfPower)
        {
            // What can kill these:
            // Bow-wow ; Hookshot ; Magic Rod ; Boomerang ; Sword2 + Spin Slash ; Sword2 + Piece of Power/Red Tunic
            if (hitType == HitType.BowWow || hitType == HitType.Hookshot || hitType == HitType.MagicRod ||  hitType == HitType.Boomerang ||
                ((hitType & HitType.Sword2) != 0 && (hitType & HitType.SwordSpin) != 0) ||  
                ((hitType & HitType.Sword2) != 0 && pieceOfPower))
            {
                return true;
            }
            return false;
        }

        private bool OnPush(Vector2 direction, PushableComponent.PushType type)
        {
            return true;
        }

        private void AnimationFinished()
        {
            // start attacking the player?
            if (_animator.CurrentAnimation.Id == "idle")
            {
                var playerDistance = MapManager.ObjLink.Position - EntityPosition.Position;
                if (playerDistance.Length() < 128)
                {
                    _animator.Play("pre_attack");

                    // shoot fireball
                    Map.Objects.SpawnObject(new EnemyFireball(Map, (int)EntityPosition.X, (int)EntityPosition.Y, 0.8f));

                    return;
                }
                // continue with the idle animation and don't start an attack
                _animator.Play("idle");
            }
        }

        private Values.HitCollision OnHit(GameObject originObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
        {
            // Because of the way the hit system works, this needs to be in any hit that doesn't default to "None" hit collision.
            if ((hitType & HitType.CrystalSmash) != 0 || (hitType & HitType.ClassicSword) != 0)
                return Values.HitCollision.None;

            if (ValidateHit(hitType, pieceOfPower))
            {
                if (hitType != HitType.BowWow && (hitType == HitType.MagicRod || damage >= _damageState.CurrentLives))
                {
                    _damageState.HitMultiplierX = 4;
                    _damageState.HitMultiplierY = 4;
                }
                if (_dealsDamage)
                {
                    if (_damageState.CurrentLives <= 0)
                    {
                        _damageField.IsActive = false;
                        _hitComponent.IsActive = false;
                        _collisionComponent.IsActive = false;
                    }
                    return _damageState.OnHit(originObject, direction, hitType, damage, pieceOfPower);
                }
                return Values.HitCollision.None;
            }
            if (!_blockSound)
            {
                Game1.GameManager.PlaySoundEffect("D360-09-09");
                _blockSound = true;
            }
            return Values.HitCollision.Blocking;
        }
    }
}