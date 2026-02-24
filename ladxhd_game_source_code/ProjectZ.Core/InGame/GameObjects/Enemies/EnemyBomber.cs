using System;
using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.GameObjects.Base.Components.AI;
using ProjectZ.InGame.GameObjects.Dungeon;
using ProjectZ.InGame.GameObjects.Effects;
using ProjectZ.InGame.GameObjects.Things;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Enemies
{
    internal class EnemyBomber : GameObject
    {
        private readonly BodyComponent _body;
        private readonly AiComponent _aiComponent;
        private readonly Animator _animator;
        private readonly AiDamageState _damageState;
        private readonly HittableComponent _hitComponent;
        private readonly DamageFieldComponent _damageField;

        private ObjBomb _objBomb;

        private Vector2 _startPosition;

        private float _flyHeight = 14;
        private int _lives = EnemyLives.Bomber;

        private bool fairySpawn;

        public EnemyBomber() : base("bomber") { }

        public EnemyBomber(Map.Map map, int posX, int posY) : base(map)
        {
            Tags = Values.GameObjectTag.Enemy;

            EntityPosition = new CPosition(posX + 8, posY + 16, _flyHeight);
            ResetPosition  = new CPosition(posX + 8, posY + 16, _flyHeight);
            EntitySize = new Rectangle(-12, -32, 24, 32);
            CanReset = true;
            OnReset = Reset;

            _startPosition = EntityPosition.Position;

            _animator = AnimatorSaveLoad.LoadAnimator("Enemies/bomber");
            _animator.Play("idle");

            var sprite = new CSprite(EntityPosition);
            var animationComponent = new AnimationComponent(_animator, sprite, new Vector2(-12, -16));

            _body = new BodyComponent(EntityPosition, -8, -12, 16, 12, 8)
            {
                CollisionTypes = Values.CollisionTypes.NPCWall |
                                 Values.CollisionTypes.Field,
                FieldRectangle = map.GetField(posX, posY),
                DragAir = 0.975f,
                Gravity = -0.175f,
                IgnoreHoles = true,
                IgnoresZ = true,
            };

            var stateWaiting = new AiState() { Init = InitWaiting };
            stateWaiting.Trigger.Add(new AiTriggerRandomTime(() => _aiComponent.ChangeState("moving"), 500, 1000));
            var stateMoving = new AiState() { Init = InitMoving };
            stateMoving.Trigger.Add(new AiTriggerRandomTime(() => _aiComponent.ChangeState("waiting"), 500, 1000));

            _aiComponent = new AiComponent();
            _aiComponent.States.Add("waiting", stateWaiting);
            _aiComponent.States.Add("moving", stateMoving);
            _aiComponent.ChangeState("waiting");
            _damageState = new AiDamageState(this, _body, _aiComponent, sprite, _lives) { OnBurn = OnBurn, OnDeath = OnDeath };

            var hittableBox = new CBox(EntityPosition, -7, -12, 0, 14, 12, 8, true);
            var damageBox = new CBox(EntityPosition, -7, -12, 0, 14, 12, 4, true);

            AddComponent(DamageFieldComponent.Index, _damageField = new DamageFieldComponent(damageBox, HitType.Enemy, 2));
            AddComponent(HittableComponent.Index, _hitComponent = new HittableComponent(hittableBox, OnHit));
            AddComponent(BodyComponent.Index, _body);
            AddComponent(AiComponent.Index, _aiComponent);
            AddComponent(BaseAnimationComponent.Index, animationComponent);
            AddComponent(DrawComponent.Index, new BodyDrawComponent(_body, sprite, Values.LayerPlayer));
            AddComponent(DrawShadowComponent.Index, new BodyDrawShadowComponent(_body, sprite) { ShadowWidth = 12, ShadowHeight = 4 });

            var spriteShadow = new ObjSpriteShadow(map, this, Values.LayerPlayer, "sprshadowm");
            Map.Objects.RegisterAlwaysAnimateObject(this);
            Map.Objects.RegisterAlwaysAnimateObject(spriteShadow);
        }

        private void Reset()
        {
            _animator.Continue();
            _damageField.IsActive = true;
            _hitComponent.IsActive = true;
            _aiComponent.ChangeState("moving");
            _aiComponent.ChangeState("moving");

            if (_objBomb != null)
                Map.Objects.DeleteObjects.Add(_objBomb);
        }

        private void OnBurn()
        {
            _animator.Pause();
            _body.IgnoresZ = false;
            _body.DragAir = 0.9f;
            _body.Bounciness = 0.5f;
            _damageField.IsActive = false;
            _hitComponent.IsActive = false;
        }

        private void InitWaiting()
        {
            _body.VelocityTarget = Vector2.Zero;

            var positionLink = MapManager.ObjLink.Position;
            var playerDistance = positionLink - EntityPosition.Position;
            var distance = playerDistance.Length();

            // bomb
            if (distance < 80 && Game1.RandomNumber.Next(0, 4) != 4 && _body.FieldRectangle.Contains(positionLink))
            {
                Vector2 throwDirection;

                if (distance < 64)
                {
                    // throw towards the player
                    if (playerDistance != Vector2.Zero)
                        playerDistance.Normalize();
                    throwDirection = playerDistance * (distance / 64) * 1.0f;
                }
                else
                {
                    // throw into a random direction
                    var randomRadius = Game1.RandomNumber.Next(0, 620) / 100;
                    throwDirection = new Vector2((float)Math.Sin(randomRadius), (float)Math.Cos(randomRadius)) * 0.75f;
                }

                // spawn a bomb
                _objBomb = new ObjBomb(Map, 0, 0, false, true);
                _objBomb.EntityPosition.Set(new Vector3(EntityPosition.X, EntityPosition.Y, 20));
                _objBomb.Body.Velocity = new Vector3(throwDirection, 0);
                _objBomb.Body.CollisionTypes = Values.CollisionTypes.None;
                _objBomb.Body.Gravity = -0.1f;
                _objBomb.Body.DragAir = 1.0f;
                _objBomb.Body.Bounciness = 0.5f;
                Map.Objects.SpawnObject(_objBomb);
                Map.Objects.RegisterAlwaysAnimateObject(_objBomb);
                new ObjSpriteShadow(Map, _objBomb, Values.LayerPlayer, "sprshadowm");
            }
        }

        private void InitMoving()
        {
            // the farther away the enemy is from the origin the more likely it becomes that he will move towards the start position
            var directionToStart = _startPosition - EntityPosition.Position;
            var radiusToStart = Math.Atan2(directionToStart.Y, directionToStart.X);

            var maxDistance = 80.0f;
            var randomDir = radiusToStart + (Math.PI - Game1.RandomNumber.Next(0, 628) / 100f) *
                Math.Clamp(((maxDistance - directionToStart.Length()) / maxDistance), 0, 1);

            _body.VelocityTarget = new Vector2((float)Math.Cos(randomDir), (float)Math.Sin(randomDir)) * 0.5f;
        }

        private void OnDeath(bool pieceOfPower)
        {
            if (fairySpawn)
                Map.Objects.SpawnObject(new ObjDungeonFairy(Map, (int)EntityPosition.X, (int)EntityPosition.Y - 8, 0));

            _damageState.BaseOnDeath(pieceOfPower);
        }

        private Values.HitCollision OnHit(GameObject gameObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
        {
            // Because of the way the hit system works, this needs to be in any hit that doesn't default to "None" hit collision.
            if ((hitType & HitType.CrystalSmash) != 0 || (hitType & HitType.ClassicSword) != 0)
                return Values.HitCollision.None;

            // can only be attacked with the sword while holding it
            if ((hitType & HitType.Sword) != 0 && (hitType & HitType.SwordHold) == 0 && (hitType & HitType.SwordSpin) == 0)
            {
                _body.Velocity.X = direction.X * 5;
                _body.Velocity.Y = direction.Y * 5;

                return Values.HitCollision.None;
            }
            // Magic Rod has 50% chance to spawn fairy.
            if ((hitType & HitType.MagicRod) != 0 && !fairySpawn)
            {
                if (Game1.RandomNumber.Next(0,2) == 0)
                    fairySpawn = true;
            }
            // Boomerang has 100% chance to spawn fairy.
            if ((hitType & HitType.Boomerang) != 0 && !fairySpawn)
            {
                fairySpawn = true;
            }
            // Magic Powder has unique death and 100% chance to spawn fairy.
            if ((hitType & HitType.MagicPowder) != 0 && !fairySpawn)
            {
                fairySpawn = true;

                // We just delete the enemy instead of returning damage state.
                Map.Objects.SpawnObject(new ObjDungeonFairy(Map, (int)EntityPosition.X, (int)EntityPosition.Y - 8, 0));
                Map.Objects.DeleteObjects.Add(this);

                // Play the crunch sound and show the smoke effect.
                Game1.GameManager.PlaySoundEffect("D360-03-03");
                var explosionAnimation = new ObjAnimator(Map, (int)EntityPosition.X-8, (int)EntityPosition.Y-26, Values.LayerTop, "Particles/spawn", "run", true);
                Map.Objects.SpawnObject(explosionAnimation);
                return Values.HitCollision.None;
            }
            return _damageState.OnHit(gameObject, direction, hitType, damage, pieceOfPower);
        }
    }
}