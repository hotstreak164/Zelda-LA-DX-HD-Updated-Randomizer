using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.GameObjects.Base.Components.AI;
using ProjectZ.InGame.GameObjects.Dungeon;
using ProjectZ.InGame.GameObjects.Things;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Enemies
{
    internal class EnemyGhini : GameObject, IHasVisibility
    {
        private readonly BodyComponent _body;
        private readonly AiComponent _aiComponent;
        private readonly Animator _animator;
        private readonly AiDamageState _damageState;
        private readonly DamageFieldComponent _damageField;
        private readonly CSprite _sprite;
        private readonly HittableComponent _hitComponent;
        private readonly PushableComponent _pushComponent;

        private readonly Rectangle _triggerField;
        private readonly Vector2 _centerPosition;

        private Vector2 _velocity;

        private double _direction;

        private int _lives = EnemyLives.Ghini;

        private float _flyHeight = 14;
        private float _rotationDirection;
        private float _dirChangeCount;
        private float _transparency;
        private bool _mainGhini;

        public bool IsVisible { get; private set; }

        public EnemyGhini() : base("ghini") { }

        public EnemyGhini(Map.Map map, int posX, int posY, bool mainGhini, bool spawnAnimation) : base(map)
        {
            Tags = Values.GameObjectTag.Enemy;

            EntityPosition = new CPosition(posX + 8, posY + 16, spawnAnimation ? 0 : _flyHeight);
            ResetPosition  = new CPosition(posX + 8, posY + 16, spawnAnimation ? 0 : _flyHeight);
            EntitySize = new Rectangle(-8, -32, 16, 32);
            CanReset = true;
            OnReset = Reset;

            _mainGhini = mainGhini;
            IsVisible = mainGhini;

            _triggerField = map.GetField(posX, posY);
            _centerPosition = new Vector2(_triggerField.Center.X, _triggerField.Center.Y + 16);

            _animator = AnimatorSaveLoad.LoadAnimator("Enemies/ghini");
            _animator.Play("fly_1");

            _sprite = new CSprite(EntityPosition) { Color = spawnAnimation ? Color.Transparent : Color.White };
            var animationComponent = new AnimationComponent(_animator, _sprite, Vector2.Zero);

            _body = new BodyComponent(EntityPosition, -6, -12, 12, 12, 8)
            {
                CollisionTypes = Values.CollisionTypes.Field,
                AvoidTypes     = Values.CollisionTypes.NPCWall,
                IgnoreHoles = true,
                IgnoresZ = true,
            };

            var stateInit = new AiState();
            stateInit.Trigger.Add(new AiTriggerCountdown(64, null, () => _aiComponent.ChangeState("spawning")));
            var stateSpawning = new AiState(UpdateSpawning);
            var stateFlying = new AiState(UpdateFlying);

            _aiComponent = new AiComponent();
            _aiComponent.States.Add("init", stateInit);
            _aiComponent.States.Add("spawning", stateSpawning);
            _aiComponent.States.Add("flying", stateFlying);
            _damageState = new AiDamageState(this, _body, _aiComponent, _sprite, _lives, true, false) { IsActive = !spawnAnimation };
            _damageState.OnDeath = OnDeath;
            _aiComponent.ChangeState(spawnAnimation ? "init" : "flying");

            var damageCollider = new CBox(EntityPosition, -6, -14, 0, 12, 14, 8, true);

            AddComponent(DamageFieldComponent.Index, _damageField = new DamageFieldComponent(damageCollider, HitType.Enemy, 4) { IsActive = !spawnAnimation });
            AddComponent(HittableComponent.Index, _hitComponent = new HittableComponent(damageCollider, OnHit));
            AddComponent(BodyComponent.Index, _body);
            AddComponent(AiComponent.Index, _aiComponent);
            AddComponent(BaseAnimationComponent.Index, animationComponent);
            AddComponent(DrawComponent.Index, new BodyDrawComponent(_body, _sprite, Values.LayerPlayer));
            AddComponent(DrawShadowComponent.Index, new ShadowBodyDrawComponent(EntityPosition));
            AddComponent(PushableComponent.Index, _pushComponent = new PushableComponent(damageCollider, OnPush));

            new ObjSpriteShadow(map, this, Values.LayerPlayer, "sprshadowm");
        }

        private void Reset()
        {
            if (_mainGhini)
            {
                _aiComponent.ChangeState("flying");
                _aiComponent.ChangeState("flying");
                _damageState.CurrentLives = EnemyLives.Ghini;
            }
            // ToDo: Spawned Ghinis might need "ObjOnPushKeySetter" to be reset somehow...
            else
            {

            }
        }

        private void UpdateSpawning()
        {
            _transparency = AnimationHelper.MoveToTarget(_transparency, 1, Game1.TimeMultiplier * 0.15f);
            _sprite.Color = Color.White * _transparency;

            EntityPosition.Z += Game1.TimeMultiplier * 0.25f;

            if (EntityPosition.Z >= _flyHeight)
            {
                EntityPosition.Z = _flyHeight;
                _aiComponent.ChangeState("flying");
                _damageState.IsActive = true;
                _damageField.IsActive = true;
            }
            if (_transparency > 0.5f)
                IsVisible = true;
        }

        private void UpdateFlying()
        {
            _dirChangeCount -= Game1.DeltaTime;

            // change the direction
            if (_dirChangeCount <= 0)
            {
                // the farther away the enemy is from the origin the more likely it becomes that he will move towards the center position
                var directionToStart = _centerPosition - EntityPosition.Position;
                var radiusToCenter = Math.Atan2(directionToStart.Y, directionToStart.X);

                var maxDistanceX = 85.0f;
                var maxDistanceY = 55.0f;
                var distanceMultiplier = Math.Clamp(
                    Math.Min(
                        (maxDistanceX - Math.Abs(directionToStart.X)) / maxDistanceX,
                        (maxDistanceY - Math.Abs(directionToStart.Y)) / maxDistanceY), 0, 1);

                _direction = radiusToCenter + (Math.PI - Game1.RandomNumber.Next(0, 628) / 100f) * distanceMultiplier;

                // new direction + new rotation speed
                _dirChangeCount = Game1.RandomNumber.Next(750, 1500) * (distanceMultiplier * 0.5f + 0.5f);
                _rotationDirection = Game1.RandomNumber.Next(-100, 100) / 1000f * distanceMultiplier;
            }

            _velocity *= (float)Math.Pow(0.95f, Game1.TimeMultiplier);
            _velocity += new Vector2((float)Math.Cos(_direction), (float)Math.Sin(_direction)) * 0.035f * Game1.TimeMultiplier;
            _direction += _rotationDirection * Game1.TimeMultiplier;

            // clamp the speed
            if (_velocity.Length() > 1.75f)
            {
                _velocity.Normalize();
                _velocity *= 1.75f;
            }
            _body.VelocityTarget = _velocity;
            _animator.Play("fly_" + (_body.VelocityTarget.X < 0 ? -1 : 1));
        }

        private void OnDeath(bool pieceOfPower)
        {
            if (_mainGhini)
                KillOtherGhinies();

            // A 25% chance to spawn a fairy.
            if (Game1.RandomNumber.Next(0, 4) == 1)
                Map.Objects.SpawnObject(new ObjDungeonFairy(Map, (int)EntityPosition.X, (int)EntityPosition.Y, (int)EntityPosition.Z));

            _damageState.BaseOnDeath(pieceOfPower);
        }

        private void KillOtherGhinies()
        {
            var enemyList = new List<GameObject>();

            Map.Objects.GetGameObjectsWithTag(enemyList, Values.GameObjectTag.Enemy,
                _triggerField.X, _triggerField.Y, _triggerField.Width, _triggerField.Height);

            foreach (var enemy in enemyList)
            {
                if (enemy != this && enemy.IsActive && (enemy.GetType() == typeof(EnemyGhini) ||
                                                        enemy.GetType() == typeof(EnemyGhiniGiant)))
                {
                    var aiComponent = (AiComponent)enemy.Components[AiComponent.Index];
                    aiComponent?.ChangeState("damageDeath");
                }
            }
        }

        private bool OnPush(Vector2 direction, PushableComponent.PushType type)
        {
            if (type == PushableComponent.PushType.Impact)
                _body.Velocity = new Vector3(direction.X * 2.5f, direction.Y * 2.5f, _body.Velocity.Z);

            return true;
        }

        private Values.HitCollision OnHit(GameObject originObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
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
            if (hitType == HitType.MagicPowder)
                return Values.HitCollision.None;

            if (hitType == HitType.Bomb || hitType == HitType.Bow || hitType == HitType.MagicRod)
                damage *= 2;

            return _damageState.OnHit(originObject, direction, hitType, damage, pieceOfPower);
        }
    }
}