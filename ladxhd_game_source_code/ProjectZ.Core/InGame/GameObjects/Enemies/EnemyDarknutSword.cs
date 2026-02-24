using System;
using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Enemies
{
    internal class EnemyDarknutSword : GameObject
    {
        public readonly Animator Animator;
        public readonly CSprite Sprite;
        public readonly DamageFieldComponent _damageField;
        public readonly HittableComponent _hitComponent;
        public readonly PushableComponent _pushComponent;
        private readonly EnemyDarknut _owner;
        private readonly CBox _damageBox;
        private readonly CBox _collisionBox;

        private Rectangle _fieldRect;
        private Vector2 _difference;
        private double _lastHitTime;
        private int _direction;

        public EnemyDarknutSword(Map.Map map, EnemyDarknut owner) : base(map)
        {
            _owner = owner;
            _owner.EntityPosition.AddPositionListener(typeof(EnemyDarknutSword), PositionChange);

            _fieldRect = _owner.Body.FieldRectangle.ToRectangle();
            _direction = _owner.Direction;

            EntityPosition = new CPosition(owner.EntityPosition.X, owner.EntityPosition.Y - 1, owner.EntityPosition.Z);
            EntitySize = new Rectangle(-22, -8 - 24, 44, 48);
            CanReset = false;

            Animator = AnimatorSaveLoad.LoadAnimator("Enemies/darknut sword");

            Sprite = new CSprite(EntityPosition);
            var animationComponent = new AnimationComponent(Animator, Sprite, new Vector2(-8, -15));

            _damageBox = new CBox(0, 0, 0, 0, 0, 4);
            _collisionBox = new CBox(0, 0, 0, 0, 0, 4);

            UpdateBoxes();

            AddComponent(DamageFieldComponent.Index, _damageField = new DamageFieldComponent(_damageBox, HitType.Enemy, 2));
            AddComponent(HittableComponent.Index, _hitComponent = new HittableComponent(_collisionBox, OnHit));
            AddComponent(PushableComponent.Index, _pushComponent = new PushableComponent(_damageBox, OnPush) { RepelParticle = true });
            AddComponent(BaseAnimationComponent.Index, animationComponent);
            AddComponent(UpdateComponent.Index, new UpdateComponent(Update));
            AddComponent(DrawComponent.Index, new DrawCSpriteComponent(Sprite, Values.LayerPlayer));
        }

        private void PositionChange(CPosition position)
        {
            EntityPosition.Set(new Vector2(position.X, position.Y - position.Z - 1));
        }

        private void Update()
        {
            // Optimization: Skip the update function if Link is not in the same rect as the enemy.
            if (!_fieldRect.Contains(MapManager.ObjLink.Position))
                return;

            // Get the difference between the X and Y positions between Link and the Darknut.
            _difference = new Vector2(MapManager.ObjLink.EntityPosition.X - _owner.EntityPosition.X, MapManager.ObjLink.EntityPosition.Y - _owner.EntityPosition.Y);

            // Get the facing direction of the Darknut.
            _direction = _owner.Direction;

            // Update the damage and collision boxes.
            UpdateBoxes();
        }

        private void UpdateBoxes()
        {
            // Update damage boxes to match sprite position.
            _damageBox.Box.X = EntityPosition.X - 8 + Animator.CollisionRectangle.X;
            _damageBox.Box.Y = EntityPosition.Y - 15 + Animator.CollisionRectangle.Y;
            _damageBox.Box.Width = Animator.CollisionRectangle.Width;
            _damageBox.Box.Height = Animator.CollisionRectangle.Height;

            // Set the initial collision sizes.
            int collisionX = (int)_damageBox.Box.X;
            int collisionY = (int)_damageBox.Box.Y;
            int collisionW = (int)_damageBox.Box.Width;
            int collisionH = (int)_damageBox.Box.Height;

            // Calculate collision based on facing direction.
            switch (_direction)
            {
                case 0: { collisionW = (int)_damageBox.Box.Width * 4 / 6; collisionX += (int)_damageBox.Box.Width - collisionW; break; }
                case 1: { collisionH = (int)_damageBox.Box.Height * 4 / 6; collisionY += (int)_damageBox.Box.Height - collisionH; break; }
                case 2: { collisionW = (int)_damageBox.Box.Width * 4 / 6; break; }
                case 3: { collisionH = (int)_damageBox.Box.Height * 4 / 6; break; }
            }
            // Apply the collision to the collision box.
            _collisionBox.Box.X = collisionX;
            _collisionBox.Box.Y = collisionY;
            _collisionBox.Box.Width = collisionW;
            _collisionBox.Box.Height = collisionH;

            // If enemy is aggroed and moving towards the player, "HittableBox" will contract and be offset away from Link to allow Link's sword to poke without
            // erroneously landing a hit. The behavior of the original game is that the enemy should be impervious from the front, but also the swords should appear
            // to overlap by 3-4 pixels. Hit box is only adjusted if Link and the Darknut are within 5 pixels of being "level" with each other based on direction.
            if (_owner.AiState == "attack")
            {
                if (_direction == 0 && Math.Abs(_difference.Y) < 5)
                    _owner.HittableBox = new CBox(EntityPosition, 1, -14, 3, 12, 8);
                if (_direction == 1 && Math.Abs(_difference.X+8) < 5)
                    _owner.HittableBox = new CBox(EntityPosition, -4, -6, 8, 5, 8);
                if (_direction == 2 && Math.Abs(_difference.Y) < 5)
                    _owner.HittableBox = new CBox(EntityPosition, -4, -14, 3, 12, 8);
                if (_direction == 3 && Math.Abs(_difference.X-8) < 5)
                    _owner.HittableBox = new CBox(EntityPosition, -4, -14, 8, 5, 8);
            }
            // If not aggroed or not level with opponent, hitbox is set to default values so other attacks or interactions behave as expected right away again.
            else if ((_direction == 0 || _direction == 2) && (Math.Abs(_difference.Y) > 5) || (_direction == 1 || _direction == 3) && (Math.Abs(_difference.X) > 5))

            if ((_direction == 0 || _direction == 2) && Math.Abs(_difference.Y) > 5 || (_direction == 1 && Math.Abs(_difference.X+8) > 5) || (_direction == 3) && (Math.Abs(_difference.X-8) > 5))
                _owner.HittableBox = new CBox(EntityPosition, -4, -14, 8, 12, 8);
        }

        private bool OnPush(Vector2 direction, PushableComponent.PushType type)
        {
            if (type == PushableComponent.PushType.Impact)
            {
                _owner.Body.Velocity.X = direction.X * 2.5f;
                _owner.Body.Velocity.Y = direction.Y * 2.5f;
            }
            return true;
        }

        private Values.HitCollision OnHit(GameObject gameObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
        {
            // Because of the way the hit system works, this needs to be in any hit that doesn't default to "None" hit collision.
            if ((hitType & HitType.CrystalSmash) != 0 || (hitType & HitType.ClassicSword) != 0)
                return Values.HitCollision.None;

            if (hitType == HitType.MagicRod || hitType == HitType.MagicPowder || hitType == HitType.Bow || hitType == HitType.Hookshot || hitType == HitType.Boomerang ||
                (_lastHitTime != 0 && Game1.TotalGameTime - _lastHitTime < 250))
                return Values.HitCollision.None;

            _lastHitTime = Game1.TotalGameTime;

            _owner.Body.Velocity.X = direction.X * 1.5f;
            _owner.Body.Velocity.Y = direction.Y * 1.5f;

            return Values.HitCollision.RepellingParticle | Values.HitCollision.Repelling2;
        }
    }
}