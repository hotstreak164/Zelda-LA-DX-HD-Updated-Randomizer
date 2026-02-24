using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.GameObjects.Effects;
using ProjectZ.InGame.GameObjects.Things;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Enemies
{
    class EnemyNut : GameObject
    {
        private readonly BodyComponent _body;
        private readonly CSprite _sprite;
        private readonly HittableComponent _hitComponent;
        private readonly PushableComponent _pushComponent;
        private readonly DamageFieldComponent _damageField;

        private int _collisionCount;
        private bool _wasHit;
        private bool _swordHit;

        public EnemyNut(Map.Map map, Vector3 position, Vector3 direction) : base(map)
        {
            Tags = Values.GameObjectTag.Enemy;

            EntityPosition = new CPosition(position.X, position.Y, position.Z);
            EntitySize = new Rectangle(-6, -48, 12, 48);
            CanReset = true;
            OnReset = Reset;

            _sprite = new CSprite(Resources.SprEnemies, EntityPosition, new Rectangle(306, 2, 12, 12), new Vector2(-6, -12));

            _body = new BodyComponent(EntityPosition, -6, -12, 12, 12, 8)
            {
                MoveCollision = MoveCollision,
                CollisionTypes = Values.CollisionTypes.Field,
                Gravity = -0.1f,
                DragAir = 1.0f,
                Bounciness = 0.75f
            };
            _body.Velocity = direction;

            var hitBox = new CBox(EntityPosition, -5, -11, 0, 10, 10, 10, true);
            AddComponent(PushableComponent.Index, _pushComponent = new PushableComponent(hitBox, OnPush));
            AddComponent(DamageFieldComponent.Index, _damageField = new DamageFieldComponent(hitBox, HitType.Enemy, 2));
            AddComponent(HittableComponent.Index, _hitComponent = new HittableComponent(hitBox, OnHit));
            AddComponent(BodyComponent.Index, _body);
            AddComponent(DrawComponent.Index, new DrawCSpriteComponent(_sprite, Values.LayerPlayer));

            var shadow = new DrawShadowSpriteComponent(Resources.SprShadow, EntityPosition, new Rectangle(0, 0, 65, 66), new Vector2(-6, -6), 12, 6);
            AddComponent(DrawShadowComponent.Index, shadow);

            new ObjSpriteShadow(map, this, Values.LayerPlayer, "sprshadowm");
        }

        private void Reset()
        {
            Map.Objects.DeleteObjects.Add(this);
        }

        private bool OnPush(Vector2 direction, PushableComponent.PushType type)
        {
            if (type == PushableComponent.PushType.Impact)
                _body.Velocity = new Vector3(direction.X * 2.5f, direction.Y * 2.5f, _body.Velocity.Z);

            return true;
        }

        private void MoveCollision(Values.BodyCollision collisionType)
        {
            _collisionCount++;

            if (_collisionCount > 3 || _wasHit)
            {
                // spawn explosion effect
                if (_wasHit)
                {
                    Game1.GameManager.PlaySoundEffect("D360-03-03");

                    if (_swordHit && (Game1.GameManager.PieceOfPowerIsActive || Game1.GameManager.CloakType == GameManager.CloakRed))
                    {
                        var posX = (int)EntityPosition.X;
                        var posY = (int)EntityPosition.Y;
                        Map.Objects.SpawnObject(new ObjDeathExplodeEffect(Map, posX, posY, 0, 0, true));
                    }
                    else
                    {
                        var posX = (int)EntityPosition.X - 12;
                        var posY = (int)EntityPosition.Y - 16;
                        Map.Objects.SpawnObject(new ObjDeathExplodeEffect(Map, posX, posY, 0, 0));
                    }
                    if (Game1.RandomNumber.Next(0, 2) == 0)
                    {
                        var objItem = new ObjItem(Map, (int)EntityPosition.X - 8, (int)EntityPosition.Y - 8, "j", "", "ruby", "", true);
                        objItem.SetSpawnDelay(250);
                        Map.Objects.SpawnObject(objItem);
                    }
                }

                Map.Objects.DeleteObjects.Add(this);
                return;
            }

            if (!_wasHit)
                Game1.GameManager.PlaySoundEffect("D360-09-09");

            // set a new random direction
            var angle = (Game1.RandomNumber.Next(0, 100) / 100f) * (float)Math.PI * 2f;
            _body.Velocity = new Vector3((float)Math.Sin(angle), (float)Math.Cos(angle), _body.Velocity.Z);

            // flip the sprite
            _sprite.SpriteEffect ^= SpriteEffects.FlipHorizontally;
        }

        private Values.HitCollision OnHit(GameObject gameObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
        {
            // Because of the way the hit system works, this needs to be in any hit that doesn't default to "None" hit collision.
            if ((hitType & HitType.CrystalSmash) != 0 || (hitType & HitType.ClassicSword) != 0)
                return Values.HitCollision.None;

            if ((hitType & HitType.Sword) != 0)
                _swordHit = true;

            if (_wasHit)
                return Values.HitCollision.None;

            _damageField.IsActive = false;
            _hitComponent.IsActive = false;
            _pushComponent.IsActive = false;

            _body.Velocity = new Vector3(direction.X, direction.Y, 0.1f) * 3.5f;
            EntityPosition.Set(new Vector3(EntityPosition.X, EntityPosition.Y - EntityPosition.Z, 0));
            _wasHit = true;

            return Values.HitCollision.Enemy;
        }
    }
}