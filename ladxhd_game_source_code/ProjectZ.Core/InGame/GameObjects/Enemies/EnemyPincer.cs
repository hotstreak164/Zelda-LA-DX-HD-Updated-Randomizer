using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    internal class EnemyPincer : GameObject
    {
        private readonly AiDamageState _damageState;
        private readonly CSprite _sprite;
        private readonly Animator _animator;
        private readonly AiComponent _aiComponent;
        private readonly DamageFieldComponent _damageField;
        private readonly HittableComponent _hitComponent;
        private readonly PushableComponent _pushComponent;
        private readonly BodyComponent _body;
        private readonly AiStunnedState _stunnedState;
        private readonly Rectangle _tailRectangle = new Rectangle(184, 124, 8, 8);

        private readonly Vector2 _spawnPosition;
        private Vector2 _direction;
        private Vector2 _attackOffset;
        private Vector2 _retractStartPosition;

        private ObjHole _hole;
        private bool _overHole;

        private float _attackCounter;
        private int _dirIndex;
        private int _lives = EnemyLives.Pincer;

        private float _waitTimer;
        private bool _powderWindow;
        private bool _wasStunned;

        public EnemyPincer() : base("pincer") { }

        public EnemyPincer(Map.Map map, int posX, int posY) : base(map)
        {
            Tags = Values.GameObjectTag.Enemy;

            EntityPosition = new CPosition(posX + 8, posY + 8, 0);
            ResetPosition  = new CPosition(posX + 8, posY + 8, 0);
            EntitySize = new Rectangle(-32, -32, 64, 64);
            CanReset = true;
            OnReset = Reset;

            _spawnPosition = new Vector2(EntityPosition.X, EntityPosition.Y);

            _animator = AnimatorSaveLoad.LoadAnimator("Enemies/pincer");
            _animator.Play("eyes");

            _sprite = new CSprite(EntityPosition) { IsVisible = false };
            var animationComponent = new AnimationComponent(_animator, _sprite, new Vector2(-8, -8));

            _body = new BodyComponent(EntityPosition, -6, -6, 12, 12, 8)
            {
                CollisionTypes = Values.CollisionTypes.Field,
                Drag = 0.75f,
                IgnoreHoles = true
            };

            var stateWaiting = new AiState(UpdateWaiting);
            var stateSpawning = new AiState(null);
            stateSpawning.Trigger.Add(new AiTriggerCountdown(1000, null, ToAttack));
            var stateAttacking = new AiState(UpdateAttack);
            var stateAttackWait = new AiState(null);
            stateAttackWait.Trigger.Add(new AiTriggerCountdown(1000, null, ToRetract));
            var stateRetract = new AiState(UpdateRetract);

            _aiComponent = new AiComponent();
            _aiComponent.States.Add("waiting", stateWaiting);
            _aiComponent.States.Add("spawning", stateSpawning);
            _aiComponent.States.Add("attacking", stateAttacking);
            _aiComponent.States.Add("attackWait", stateAttackWait);
            _aiComponent.States.Add("retract", stateRetract);
            _damageState = new AiDamageState(this, _body, _aiComponent, _sprite, _lives, false) { OnBurn = OnBurn, HitMultiplierX = 1.5f, HitMultiplierY = 1.5f };
            _stunnedState = new AiStunnedState(_aiComponent, animationComponent, 3300, 900);

            _aiComponent.ChangeState("waiting");

            var damageBox = new CBox(EntityPosition, -5, -5, 0, 10, 10, 4);
            var hittableBox = new CBox(EntityPosition, -7, -7, 14, 14, 8);

            AddComponent(PushableComponent.Index, _pushComponent = new PushableComponent(hittableBox, OnPush));
            AddComponent(BodyComponent.Index, _body);
            AddComponent(DamageFieldComponent.Index, _damageField = new DamageFieldComponent(damageBox, HitType.Enemy, 2) { IsActive = false });
            AddComponent(HittableComponent.Index, _hitComponent = new HittableComponent(hittableBox, OnHit));
            AddComponent(AiComponent.Index, _aiComponent);
            AddComponent(BaseAnimationComponent.Index, animationComponent);
            AddComponent(DrawComponent.Index, new DrawComponent(Draw, Values.LayerPlayer, EntityPosition));
            AddComponent(UpdateComponent.Index, new UpdateComponent(Update));
        }

        private void Reset()
        {
            _animator.Continue();
            _attackCounter = 0;
            _sprite.IsVisible = false;
            _damageField.IsActive = false;
            _hitComponent.IsActive = true;
            _pushComponent.IsActive = true;
            _aiComponent.ChangeState("waiting");
            _aiComponent.ChangeState("waiting");
            _damageState.CurrentLives = EnemyLives.Pincer;
        }

        private void Update()
        {
            // If we haven't found a hole yet.
            if (_hole == null)
            {
                // Find the hole that the pincer is nearest to.
                var holes = new List<GameObject>();
                Map.Objects.GetGameObjectsWithTag(holes, Values.GameObjectTag.Hole, (int)EntityPosition.X - 8, (int)EntityPosition.Y - 8, 16, 16);

                if (holes.Count > 0)
                    _hole = (ObjHole)holes[0];
            }
            // If we have the hole, then determine if it's currently over top of it.
            else
            {
                // The hole rectangle is a bit too big, so resize it.
                var holeRect = _hole.collisionBox.Box.Rectangle();
                var overRect = new Rectangle((int)holeRect.X + 2, (int)holeRect.Y + 2, (int)holeRect.Width - 4, (int)holeRect.Height - 4);
                _overHole = _hole.IsActive && overRect.Contains(EntityPosition.Position);
            }
            // If it's stunned and over the hole, make it fall.
            if (_overHole && _stunnedState.IsStunned())
                FakeHoleDeath();
        }

        private bool OnPush(Vector2 direction, PushableComponent.PushType type)
        {
            if (type == PushableComponent.PushType.Impact &&
                (_aiComponent.CurrentStateId == "attacking" ||
                 _aiComponent.CurrentStateId == "attackWait" ||
                 _aiComponent.CurrentStateId == "retract" ||
                _stunnedState.IsStunned()))
            {
                if (_aiComponent.CurrentStateId == "attacking")
                    _aiComponent.ChangeState("attackWait");

                _body.Velocity = new Vector3(direction.X * 2.5f, direction.Y * 2.5f, 0);
                return true;
            }

            return false;
        }

        private void OnBurn()
        {
            _animator.Pause();
            _damageField.IsActive = false;
            _hitComponent.IsActive = false;
            _pushComponent.IsActive = false;
        }

        private void TryReleaseStun()
        {
            if (!_stunnedState.Active && _wasStunned)
            {
                _damageField.IsActive = true;
                _wasStunned = false;
            }
        }

        private void ToWaiting()
        {
            _aiComponent.ChangeState("waiting");
            _sprite.IsVisible = false;
            _damageField.IsActive = false;
        }

        private void UpdateWaiting()
        {
            if (_waitTimer < 750f)
            {
                _powderWindow = true;
                _waitTimer += Game1.DeltaTime;
                return;
            }
            _direction = MapManager.ObjLink.Position - new Vector2(EntityPosition.Position.X, EntityPosition.Position.Y - 4);
            if (_direction.Length() < 32)
            {
                _aiComponent.ChangeState("spawning");

                EntityPosition.Set(_spawnPosition);

                _powderWindow = false;
                _sprite.IsVisible = true;
                _animator.Play("eyes");
            }
            TryReleaseStun();
        }

        private void ToAttack()
        {
            _waitTimer = 0;
            _damageField.IsActive = true;
            _aiComponent.ChangeState("attacking");
            GetAttackDirection();
            TryReleaseStun();
        }

        private void UpdateAttack()
        {
            _attackCounter += (Game1.TimeMultiplier * 2) / 35.0f;
            if (_attackCounter > 1)
                _attackCounter = 1;

            _attackOffset = _direction * _attackCounter * 35.0f;
            EntityPosition.Set(_spawnPosition + _attackOffset);

            if (_attackCounter >= 1)
                _aiComponent.ChangeState("attackWait");

            TryReleaseStun();
        }

        private void ToRetract()
        {
            _aiComponent.ChangeState("retract");
            _retractStartPosition = EntityPosition.Position - _spawnPosition;
            _attackCounter = 1;
            TryReleaseStun();
        }

        private void UpdateRetract()
        {
            _attackCounter -= (Game1.TimeMultiplier * 1.25f) / 35.0f;
            if (_attackCounter < 0)
                _attackCounter = 0;

            _attackOffset = Vector2.Lerp(_retractStartPosition, Vector2.Zero, 1 - _attackCounter);
            EntityPosition.Set(_spawnPosition + _attackOffset);

            if (_attackCounter <= 0)
                ToWaiting();

            TryReleaseStun();
        }

        private void Draw(SpriteBatch spriteBatch)
        {
            // draw the body
            if (_sprite.IsVisible && _aiComponent.CurrentStateId != "spawning")
                for (var i = 0; i < 3; i++)
                {
                    var position =
                        _spawnPosition + (EntityPosition.Position - _spawnPosition) * (0.15f + (i / 2f) * 0.5f) - new Vector2(4, 4);

                    spriteBatch.Draw(Resources.SprEnemies, position, _tailRectangle, Color.White);
                }

            // draw the head
            _sprite.Draw(spriteBatch);
        }

        private void GetAttackDirection()
        {
            _direction = MapManager.ObjLink.Position - EntityPosition.Position;

            if (_direction != Vector2.Zero)
                _direction.Normalize();

            var degree = MathHelper.ToDegrees((float)Math.Atan2(-_direction.Y, -_direction.X)) + 360;

            _dirIndex = (int)((degree + 22.5f) / 45) % 8;

            _animator.Play(_dirIndex.ToString());
        }

        private void FakeHoleDeath()
        {
            // Play the "falling down hole" sound effect.
            Game1.GameManager.PlaySoundEffect("D360-03-03");
            Game1.GameManager.PlaySoundEffect("D360-24-18");

            // Spawn the graphics for it.
            var fallAnimation = new ObjAnimator(_aiComponent.Owner.Map, 0, 0, Values.LayerBottom, "Particles/fall", "idle", true);
            fallAnimation.EntityPosition.Set(new Vector2(
                _body.Position.X + _body.OffsetX + _body.Width / 2.0f - 5,
                _body.Position.Y + _body.OffsetY + _body.Height / 2.0f - 5));
            _aiComponent.Owner.Map.Objects.SpawnObject(fallAnimation);

            // Remove the object from the map.
            Map.Objects.DeleteObjects.Add(this);
        }

        private Values.HitCollision OnHit(GameObject gameObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
        {
            // Because of the way the hit system works, this needs to be in any hit that doesn't default to "None" hit collision.
            if ((hitType & HitType.CrystalSmash) != 0 || (hitType & HitType.ClassicSword) != 0)
                return Values.HitCollision.None;

            // Simulate the enemy falling down the hole since it can't actually be absorbed by holes.
            if (hitType == HitType.MagicPowder && _powderWindow || _overHole && _damageState.CurrentLives <= 0)
                FakeHoleDeath();
            
            // can only attack while the enemy is attacking
            if (_aiComponent.CurrentStateId != "attacking" &&
                _aiComponent.CurrentStateId != "attackWait" &&
                _aiComponent.CurrentStateId != "retract" &&
                !_stunnedState.IsStunned())
                return Values.HitCollision.None;

            if (hitType == HitType.MagicPowder)
            {
                _wasStunned = true;
                _damageField.IsActive = false;
                _stunnedState.StartStun();
                return Values.HitCollision.Enemy;
            }

            if (_aiComponent.CurrentStateId == "attacking")
                _aiComponent.ChangeState("attackWait");

            _damageState.OnHit(gameObject, direction, hitType, damage, pieceOfPower);

            // make sure to not fly away like for other enemies
            if (pieceOfPower)
                _body.Drag = 0.75f;

            return Values.HitCollision.Enemy;
        }
    }
}