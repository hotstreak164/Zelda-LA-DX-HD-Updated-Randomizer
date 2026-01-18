using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Enemies
{
    internal class EnemyGiantBubble : GameObject
    {
        private readonly CSprite _sprite;
        private readonly BodyComponent _body;
        private readonly Animator _animator;
        private readonly Color _lightColor = new Color(255, 255, 255) * 0.5f;
        private bool _flashState;
        private double _lastFlashTime;

        public EnemyGiantBubble() : base("giant bubble") { }

        public EnemyGiantBubble(Map.Map map, int posX, int posY) : base(map)
        {
            // maybe create a new tag for enemies that should be ignored by the enemy trigger
            Tags = Values.GameObjectTag.Damage;

            EntityPosition = new CPosition(posX + 16, posY + 16, 0);
            ResetPosition  = new CPosition(posX + 16, posY + 16, 0);
            EntitySize = new Rectangle(-32, -32, 64, 64);
            CanReset = true;

            _animator = AnimatorSaveLoad.LoadAnimator("Enemies/giant bubble");
            _animator.Play("idle");

            _sprite = new CSprite(EntityPosition);
            var animationComponent = new AnimationComponent(_animator, _sprite, Vector2.Zero);

            _body = new BodyComponent(EntityPosition, -10, -10, 20, 20, 8)
            {
                MoveCollision = OnCollision,
                IgnoresZ = true,
                CollisionTypes = Values.CollisionTypes.Normal |
                                 Values.CollisionTypes.Field |
                                 Values.CollisionTypes.NPCWall
            };

            // start with a random direction
            _body.VelocityTarget = new Vector2(
                Game1.RandomNumber.Next(0, 2) * 2 - 1, Game1.RandomNumber.Next(0, 2) * 2 - 1) * 0.7f;

            var damageCollider = new CBox(EntityPosition, -12, -12, 0, 24, 24, 8);

            AddComponent(DamageFieldComponent.Index, new DamageFieldComponent(damageCollider, HitType.Enemy, 2));
            AddComponent(BodyComponent.Index, _body);
            AddComponent(BaseAnimationComponent.Index, animationComponent);
            AddComponent(UpdateComponent.Index, new UpdateComponent(Update));
            AddComponent(DrawComponent.Index, new BodyDrawComponent(_body, _sprite, Values.LayerPlayer) { WaterOutline = false });
            AddComponent(DrawShadowComponent.Index, new DrawShadowCSpriteComponent(_sprite));
            AddComponent(LightDrawComponent.Index, new LightDrawComponent(DrawLight));
        }

        private void OnCollision(Values.BodyCollision collider)
        {
            if ((collider & Values.BodyCollision.Horizontal) != 0)
                _body.VelocityTarget.X = -_body.VelocityTarget.X;
            if ((collider & Values.BodyCollision.Vertical) != 0)
                _body.VelocityTarget.Y = -_body.VelocityTarget.Y;
        }

        private void Update()
        {
            var animationFramePercentage = _animator.FrameCounter / _animator.CurrentFrame.FrameTime;
            bool state = animationFramePercentage % 0.5 < 0.25;

            if (GameSettings.EpilepsySafe)
            {
                double totalSeconds = Game1.TotalGameTime * 0.001;
                if (totalSeconds - _lastFlashTime >= 0.50)
                {
                    _lastFlashTime = totalSeconds;
                    _flashState = !_flashState;
                }
                state = _flashState;
            }
            _sprite.SpriteShader = state ? Resources.DamageSpriteShader0 : null;
        }

        private void DrawLight(SpriteBatch spriteBatch)
        {
            if (GameSettings.ObjectLights)
                if (_sprite.SpriteShader != null)
                    DrawHelper.DrawLight(spriteBatch, new Rectangle((int)EntityPosition.X - 32, (int)EntityPosition.Y - 32, 64, 64), _lightColor);
        }
    }
}