using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.GameObjects.Effects;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Enemies
{
    internal class EnemyFireball : GameObject
    {
        private readonly CSprite _sprite;
        private readonly BodyComponent _body;
        private readonly DamageFieldComponent _damageField;
        private readonly HittableComponent _hitComponent;
        private readonly PushableComponent _pushComponent;
        private readonly CBox _damageBox;
        private readonly Rectangle _fieldRectangle;

        private double _liveTime = 2250;
        private bool _reflected;

        public EnemyFireball(Map.Map map, int posX, int posY, float speed, bool hittable = true) : base(map)
        {
            Tags = Values.GameObjectTag.Enemy;

            EntityPosition = new CPosition(posX, posY, 0);
            EntitySize = new Rectangle(-5, -5, 10, 10);
            CanReset = true;
            OnReset = Reset;

            var animator = AnimatorSaveLoad.LoadAnimator("Enemies/fireball");
            animator.Play("idle");

            _sprite = new CSprite(EntityPosition);
            var animationComponent = new AnimationComponent(animator, _sprite, new Vector2(-5, -5));

            _body = new BodyComponent(EntityPosition, -5, -5, 10, 10, 8)
            {
                IgnoresZ = true,
                IgnoreHoles = true,
                CollisionTypes = Values.CollisionTypes.None
            };

            _fieldRectangle = Map.GetField(posX, posY);

            var playerDirection = new Vector2(MapManager.ObjLink.EntityPosition.X, MapManager.ObjLink.EntityPosition.Y - 4) - EntityPosition.Position;
            if (playerDirection != Vector2.Zero)
                playerDirection.Normalize();
            _body.VelocityTarget = playerDirection * speed;

            _damageBox = new CBox(EntityPosition, -3, -3, 0, 6, 6, 4);
            var hittableBox = new CBox(EntityPosition, -4, -4, 0, 8, 8, 8);

            AddComponent(BodyComponent.Index, _body);
            if (hittable)
            {
                AddComponent(DamageFieldComponent.Index, _damageField = new DamageFieldComponent(_damageBox, HitType.Enemy, 2));
                AddComponent(HittableComponent.Index, _hitComponent = new HittableComponent(hittableBox, OnHit));
                AddComponent(PushableComponent.Index, _pushComponent = new PushableComponent(_damageBox, OnPush));
            }
            AddComponent(UpdateComponent.Index, new UpdateComponent(Update));
            AddComponent(BaseAnimationComponent.Index, animationComponent);
            AddComponent(DrawComponent.Index, new DrawCSpriteComponent(_sprite, Values.LayerTop));
            Map.Objects.RegisterAlwaysAnimateObject(this);
        }

        private void Reset()
        {
            _sprite.IsVisible = false;
            _damageField.IsActive = false;
            Map.Objects.DeleteObjects.Add(this);
        }

        public void SetVelocity(Vector2 velocity)
        {
            _body.VelocityTarget = velocity;
        }

        private void Update()
        {
            _liveTime -= Game1.DeltaTime;

            if (_liveTime <= 125)
                _sprite.Color = Color.White * ((float)_liveTime / 125f);
            // start despawning if we get outside of the current room
            else if (!_fieldRectangle.Contains(EntityPosition.Position))
                _liveTime = 125;

            if (_liveTime < 0)
                Delete();

            // If the shot was reflected, try to hit an enemy.
            if (_reflected)
            {
                // Probably the closest parallel to player damage types is the Bow.
                var collision = Map.Objects.Hit(MapManager.ObjLink, EntityPosition.Position, _damageBox.Box, HitType.Bow, 2, false, false);
                if ((collision & Values.HitCollision.Enemy) != 0)
                    Map.Objects.DeleteObjects.Add(this);
            }
        }

        private Values.HitCollision OnHit(GameObject originObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
        {
            // Because of the way the hit system works, this needs to be in any hit that doesn't default to "None" hit collision.
            if ((hitType & HitType.CrystalSmash) != 0 || (hitType & HitType.ClassicSword) != 0)
                return Values.HitCollision.None;

            if ((hitType & HitType.Sword) == 0)
                return Values.HitCollision.None;

            Game1.GameManager.PlaySoundEffect("D360-03-03");

            OnDeath(true);

            return Values.HitCollision.Enemy;
        }

        private bool OnPush(Vector2 direction, PushableComponent.PushType type)
        {
            // Check if the incoming push type is from the shield.
            if (type == PushableComponent.PushType.Impact)
            {
                // We only want a single interaction so check if it's been reflected.
                if (!_reflected)
                {
                    // If the shield is able to reflect the shot.
                    if (GameSettings.MirrorReflects && Game1.GameManager.ShieldLevel == 2)
                    {
                        Reflect(direction);
                        return false;
                    }
                    // Otherwise kill it.
                    else
                        OnDeath(false);
                }
            }
            // The shot was not reflected so perform the knockback.
            return true;
        }

        private void Reflect(Vector2 shieldDirection)
        {
            // Play the deflection sound.
            Game1.GameManager.PlaySoundEffect("D360-22-16");

            // Don't let the spear reflect more than once.
            _reflected = true;

            // It should not damage Link from this point on.
            _hitComponent.IsActive = false;
            _damageField.IsActive = false;
            _pushComponent.IsActive = false;
            _liveTime = 2250;

            // Use the incoming direction and the shield reflect direction to determine new direction.
            shieldDirection.Normalize();
            var incoming = _body.VelocityTarget;
            var reflected = (incoming - 2 * Vector2.Dot(incoming, shieldDirection) * shieldDirection) * 1.75f;

            // Reverse the movement of the spear.
            _body.VelocityTarget = reflected;
        }

        private void OnDeath(bool playSound)
        {
            if (playSound)
                Game1.GameManager.PlaySoundEffect("D360-03-03");

            var splashAnimator = new ObjAnimator(Map, 0, 0, 0, 0, Values.LayerTop, "Particles/spawn", "run", true);
            splashAnimator.EntityPosition.Set(EntityPosition.Position - new Vector2(8, 8));
            Map.Objects.SpawnObject(splashAnimator);
            Delete();
        }

        private void Delete()
        {
            Map.Objects.DeleteObjects.Add(this);
        }
    }
}