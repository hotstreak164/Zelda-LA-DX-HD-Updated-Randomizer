using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.GameObjects.Base.Components.AI;
using ProjectZ.InGame.GameObjects.Enemies;
using ProjectZ.InGame.GameObjects.Things;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.NPCs
{
    public class ObjBowWow : GameObject
    {
        private readonly List<GameObject> _enemyList = new List<GameObject>();
        private GameObject _enemyTarget;

        private readonly ObjChain _chain;

        public readonly BodyComponent _body;
        private readonly AiComponent _aiComponent;
        private readonly HittableComponent _hitComponent;
        private readonly PushableComponent _pushComponent;
        private readonly AiTriggerSwitch _changeDirectionSwitch;

        private Animator _animator;

        private Vector2 _chainPull;
        private Vector2 _origin;
        private Vector2 _currentDirectionOffset;
        private Vector2 _treasurePosition;

        private int _chainMax;
        private int _direction;
        private float _outsideCounter;
        private bool _followMode;
        private float _attackPlayerCooldown;
        private float _idleBobTimer;

        public ObjBowWow() : base("bowwow") { }

        private ObjBowWowWater _waterGraphic;

        public ObjBowWow(Map.Map map, int posX, int posY, string mode) : base(map)
        {
            EntityPosition = new CPosition(posX, posY + 16, 0);
            EntitySize = new Rectangle(-8, -16, 16, 16);

            _origin = new Vector2(posX + 8, posY + 8);

            var state = Game1.GameManager.SaveManager.GetString("bowWow");

            // cave bowwow
            if (mode == "cave")
            {
                // only spawn at the right state
                if (state == "1")
                {
                    EntityPosition.Set(new Vector2(EntityPosition.X - 8, EntityPosition.Y));
                    _followMode = false;
                }
                else
                {
                    IsDead = true;
                }
            }
            else
            {
                // bowwow is in the cave
                if (state == "1")
                    IsDead = true;
                // bowwow is following the player
                if (state == "2" || state == "3" || state == "5")
                    _followMode = true;
                // bowwow is home
                if (state == "4")
                    _followMode = false;
            }

            if (string.IsNullOrEmpty(mode) && state != "2" && state != "3" && state != "5")
                IsDead = true;

            if (IsDead)
                return;

            _body = new BodyComponent(EntityPosition, -7, -10, 14, 10, 16)
            {
                IgnoreHoles = true,
                Gravity = -0.175f,
                MoveCollision = OnCollision,
                CollisionTypes = Values.CollisionTypes.NPCWall,
                FieldRectangle = map.GetField(posX, posY)
            };

            _animator = AnimatorSaveLoad.LoadAnimator("NPCs/BowWow");
            var sprite = new CSprite(EntityPosition);
            var animationComponent = new AnimationComponent(_animator, sprite, new Vector2(-8, -16));

            var stateIAttack = new AiState(UpdateIAttack);
            stateIAttack.Trigger.Add(new AiTriggerCountdown(400, null, ToAttack));
            var stateIdle = new AiState(UpdateIdle);
            stateIdle.Trigger.Add(new AiTriggerRandomTime(EndIdle, 500, 1500));
            var stateWalking = new AiState(UpdateWalking) { Init = InitWalking };
            stateWalking.Trigger.Add(new AiTriggerRandomTime(EndWalking, 500, 1000));
            stateWalking.Trigger.Add(_changeDirectionSwitch = new AiTriggerSwitch(250));
            var stateAttack = new AiState(UpdateAttack);
            var stateAttackPlayer = new AiState(UpdateAttackPlayer); 
            var stateTreasure = new AiState(UpdateTreasure);

            _aiComponent = new AiComponent();
            _aiComponent.States.Add("iattack", stateIAttack);
            _aiComponent.States.Add("idle", stateIdle);
            _aiComponent.States.Add("walking", stateWalking);
            _aiComponent.States.Add("attack", stateAttack);
            _aiComponent.States.Add("attackPlayer", stateAttackPlayer);
            _aiComponent.States.Add("treasure", stateTreasure);

            if (Game1.RandomNumber.Next(0, 100) < 50)
                _aiComponent.ChangeState("iattack");
            else
                _aiComponent.ChangeState("walking");

            AddComponent(BodyComponent.Index, _body);
            AddComponent(AiComponent.Index, _aiComponent);
            AddComponent(BaseAnimationComponent.Index, animationComponent);
            AddComponent(DrawComponent.Index, new BodyDrawComponent(_body, sprite, Values.LayerPlayer));
            AddComponent(DrawShadowComponent.Index, new BodyDrawShadowComponent(_body, sprite) { ShadowWidth = 12, ShadowHeight = 5 });
            AddComponent(KeyChangeListenerComponent.Index, new KeyChangeListenerComponent(KeyChanged));
            AddComponent(PushableComponent.Index, _pushComponent = new PushableComponent(_body.BodyBox, OnPush));
            AddComponent(HittableComponent.Index, _hitComponent = new HittableComponent(_body.BodyBox, OnHit));

            Map.Objects.SpawnObject(_chain = new ObjChain(map, _origin));
            _currentDirectionOffset = AnimationHelper.DirectionOffset[_direction];

            SetFollowMode(_followMode);
            _waterGraphic = new ObjBowWowWater(Map, posX, posY, this);

            new ObjSpriteShadow(map, this, Values.LayerPlayer, "sprshadowm");
        }

        private Values.HitCollision OnHit(GameObject originObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
        {
            // Because of the way the hit system works, this needs to be in any hit that doesn't default to "None" hit collision.
            if ((hitType & HitType.CrystalSmash) != 0 || (hitType & HitType.ClassicSword) != 0)
                return Values.HitCollision.None;

            return Values.HitCollision.RepellingParticle;
        }

        public override void Init()
        {
            // set the position to the one of the player
            if (_followMode && MapManager.ObjLink.NextMapPositionEnd.HasValue)
            {
                EntityPosition.Set(MapManager.ObjLink.NextMapPositionEnd.Value);
                _chain.SetChainPosition(MapManager.ObjLink.NextMapPositionEnd.Value);
            }
        }

        private bool OnPush(Vector2 direction, PushableComponent.PushType type)
        {
            if (type == PushableComponent.PushType.Impact)
                _body.Velocity = new Vector3(direction.X, direction.Y, 0.25f);

            return true;
        }

        private void KeyChanged()
        {
            var state = Game1.GameManager.SaveManager.GetString("bowWow");
            if (state != null && (state == "2" || state == "3" || state == "5"))
                SetFollowMode(true);
        }

        private void SetFollowMode(bool follow)
        {
            _followMode = follow;
            _hitComponent.IsActive = !follow;
            _pushComponent.IsActive = !follow;

            if (follow)
            {
                _body.CollisionTypes = Values.CollisionTypes.None;
                _body.FieldRectangle = Rectangle.Empty;
                MapManager.ObjLink.SetBowWowFollower(this);
                Map.Objects.RegisterAlwaysAnimateObject(this);
                _chainMax = 46;
            }
            else
            {
                _body.CollisionTypes = Values.CollisionTypes.NPCWall;
                _chainMax = 40;
            }
        }

        private void ToIdle()
        {
            _aiComponent.ChangeState("idle");
            _body.VelocityTarget.X = 0;
            _body.VelocityTarget.Y = 0;

            // Original private countdown 1: random 32-95 frames at 60fps
            _idleBobTimer = Game1.RandomNumber.Next(530, 1580);

            if (!_followMode)
                _outsideCounter = Game1.RandomNumber.Next(80, 200);
        }

        private void UpdateIAttack()
        {
            UpdatePosition();
        }

        private void UpdateIdle()
        {
            if (_attackPlayerCooldown > 0)
                _attackPlayerCooldown -= Game1.DeltaTime;

            // Short timer fires a tiny bob in a random direction, replicating
            // the original's "state 1 light bounce" that ran constantly during idle.
            _idleBobTimer -= Game1.DeltaTime;
            if (_idleBobTimer <= 0)
            {
                _idleBobTimer = Game1.RandomNumber.Next(530, 1580);

                // Pick one of the 8 directions from the original's speed table.
                // Values are scaled down from GB units to match this engine's scale.
                var dirs = new Vector2[]
                {
                    new( 0.25f, -0.75f),
                    new( 0.50f, -0.50f),
                    new( 0.75f,  0.25f),
                    new( 0.50f,  0.50f),
                    new(-0.25f,  0.75f),
                    new(-0.50f,  0.50f),
                    new(-0.75f, -0.25f),
                    new(-0.50f, -0.50f),
                };

                var dir = dirs[Game1.RandomNumber.Next(0, 8)];
                _body.VelocityTarget = dir;
                _body.Velocity.Z = 1.0f; // small bob, not a full lunge jump
                _body.IsGrounded = false;

                _direction = AnimationHelper.GetDirection(_body.VelocityTarget);
                _animator.Play("walk_" + _direction);
            }

            if (!_followMode)
            {
                _outsideCounter -= Game1.DeltaTime;
                if (_outsideCounter <= 0)
                    EndIdle();
                return;
            }

            UpdatePosition();
        }

        private void EndIdle()
        {
            if (_followMode)
            {
                _treasurePosition = GetTreasurePosition();

                if (_treasurePosition != Vector2.Zero)
                {
                    var direction = _treasurePosition - EntityPosition.Position;
                    if (direction != Vector2.Zero)
                        direction.Normalize();
                    _body.VelocityTarget = direction * 1.5f;

                    _direction = AnimationHelper.GetDirection(_body.VelocityTarget);
                    _animator.Play("walk_" + _direction);

                    _aiComponent.ChangeState("treasure");
                    return;
                }

                if (Game1.RandomNumber.Next(0, 100) < 35)
                    _aiComponent.ChangeState("walking");
                else
                    ToAttack();
            }
            else
            {
                if (Game1.RandomNumber.Next(0, 100) < 65 || _attackPlayerCooldown > 0)
                    _aiComponent.ChangeState("walking");
                else
                    ToAttackPlayer();
            }
        }

        private void InitWalking()
        {
            float rotation = 1;
            if (_followMode)
            {
                // the farther away the enemy is from the origin the more likely it becomes that he will move towards the center position
                var origin = MapManager.ObjLink.Position - new Vector2(0, 4);
                var directionToStart = origin - EntityPosition.Position;
                var radiusToCenter = MathF.Atan2(directionToStart.Y, directionToStart.X);

                var maxDistanceX = 64.0f;
                var maxDistanceY = 64.0f;
                var distanceMultiplier = MathHelper.Clamp(
                    MathF.Min(
                        (maxDistanceX - MathF.Abs(directionToStart.X)) / maxDistanceX,
                        (maxDistanceY - MathF.Abs(directionToStart.Y)) / maxDistanceY), 0, 1);

                rotation = radiusToCenter + (MathF.PI - Game1.RandomNumber.Next(0, 628) / 100f) * distanceMultiplier;
            }
            else
            {
                rotation = Game1.RandomNumber.Next(0, 628) / 100f;
            }


            // change the direction
            SetWalkDirection(new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation)));
        }

        private void SetWalkDirection(Vector2 direction)
        {
            _body.VelocityTarget = direction * Game1.RandomNumber.Next(25, 40) / 25f;

            _direction = AnimationHelper.GetDirection(_body.VelocityTarget);
            _animator.Play("walk_" + _direction);
        }

        private void UpdateWalking()
        {
            if (_attackPlayerCooldown > 0)
                _attackPlayerCooldown -= Game1.DeltaTime;

            if (_body.IsGrounded)
                _body.Velocity.Z = 1.50f;

            UpdatePosition();
        }

        private void EndWalking()
        {
            if (_followMode)
            {
                if (Game1.RandomNumber.Next(0, 100) < 35)
                    ToIdle();
                else
                    ToAttack();
            }
            else
            {
                if (Game1.RandomNumber.Next(0, 100) < 65 || _attackPlayerCooldown > 0)
                    ToIdle();
                else
                    ToAttackPlayer();
            }
        }

        private void ToAttackPlayer()
        {
            _attackPlayerCooldown = 3000;
            var playerPos = new Vector2(MapManager.ObjLink.EntityPosition.X, MapManager.ObjLink.EntityPosition.Y - 8);
            var direction = playerPos - new Vector2(EntityPosition.X, EntityPosition.Y - 8);
            if (direction == Vector2.Zero)
            {
                ToIdle();
                return;
            }

            direction.Normalize();

            _body.VelocityTarget = direction * 4;

            // Launch into the air
            _body.Velocity.Z = 2f;
            _body.IsGrounded = false;

            _direction = AnimationHelper.GetDirection(_body.VelocityTarget);
            _animator.Play("walk_" + _direction);
            _aiComponent.ChangeState("attackPlayer");
        }

        private void UpdateAttackPlayer()
        {
            // Wait until he's back on the ground after the jump
            if (_body.IsGrounded && _body.Velocity.Z == 0 && _body.Position.Z <= 0)
            {
                _body.VelocityTarget = Vector2.Zero;
                ToIdle();
                return;
            }

            UpdatePosition();
        }

        private void ToAttack()
        {
            if (!_followMode)
            {
                _body.FieldRectangle = Map.GetField(MapManager.ObjLink.CenterPosition.Position);
            }
            // Reset the target each attack.
            _enemyTarget = null;

            // Get a list of enemies for Bow Wow to attack.
            Map.Objects.GetGameObjectsWithTag(_enemyList, Values.GameObjectTag.Enemy,
                (int)MapManager.ObjLink.CenterPosition.Position.X - 50,
                (int)MapManager.ObjLink.CenterPosition.Position.Y - 50, 100, 100);

            // The types of enemies that Bow Wow shouldn't eat.
            Type[] _dontEat = new Type[]{ typeof(EnemyGhini), typeof(EnemySeaUrchin), typeof(EnemyZombie) };
            _enemyList.RemoveAll(obj => ObjectManager.IsGameObjectType(obj, _dontEat));

            // Make sure the enemy is currently within the field rectangle.
            if (Camera.ClassicMode)
                _enemyList.RemoveAll(obj => !_body.FieldRectangle.Contains(obj.EntityPosition.Position));
            
            // Loop through the enemies in the list.
            foreach (var obj in _enemyList)
            {
                // Try finding a goponga flower and attack them first.
                if (obj is EnemyGopongaFlower || obj is EnemyGopongaFlowerGiant)
                {
                    _enemyTarget = obj;
                    break;
                }
            }
            // Check to see if there are enemies in the list.
            if (_enemyTarget == null && _enemyList.Count > 0)
            {
                // Choose a random enemy to attack.
                var randomIndex = Game1.RandomNumber.Next(0, _enemyList.Count);
                _enemyTarget = _enemyList[randomIndex];

                // Loop through the list again.
                for (var i = 0; i < _enemyList.Count; i++)
                {
                    // If the current enemy isn't available, try the next one.
                    if (!_enemyTarget.IsActive)
                        _enemyTarget = _enemyList[(randomIndex + i) % _enemyList.Count];
                }
            }
            // There is no enemies to attack.
            else
            {
                // Reset back to just walking around.
                if (_aiComponent.CurrentStateId == "iattack")
                {
                    _animator.Play("walk_" + _direction);
                    _aiComponent.ChangeState("walking");
                    return;
                }
            }
            // Verify once again we have an enemy to attack.
            if (_enemyTarget != null && _enemyTarget.IsActive)
            {
                // If the enemy is a fish, it might be underwater.
                if (_enemyTarget is EnemyFish)
                {
                    EnemyFish fish = _enemyTarget as EnemyFish;
                    fish.MakeVulerable();
                }
                // Set the attack direction.
                var damageState = (HittableComponent)_enemyTarget.Components[HittableComponent.Index];
                var direction = damageState.HittableBox.Box.Center - new Vector2(EntityPosition.X, EntityPosition.Y - 8);
                if (direction != Vector2.Zero)
                    direction.Normalize();
                _body.VelocityTarget = direction * 3;

                // Update the animation.
                _direction = AnimationHelper.GetDirection(_body.VelocityTarget);
                _animator.Play("walk_" + _direction);
                _aiComponent.ChangeState("attack");
                return;
            }
            _enemyTarget = null;
            ToIdle();
        }

        private Vector2 GetTreasurePosition()
        {
            if (Map?.DigMap == null || Map?.HoleMap?.ArrayTileMap == null)
                return Vector2.Zero;

            var digPositionX = (int)EntityPosition.X / 16;
            var digPositionY = (int)EntityPosition.Y / 16;

            for (var y = digPositionY - 3; y < digPositionY + 3; y++)
            {
                if (y < 0 || Map.DigMap.GetLength(1) <= y)
                    continue;

                for (var x = digPositionX - 3; x < digPositionX + 3; x++)
                {
                    if (x < 0 || Map.DigMap.GetLength(0) <= x)
                        continue;

                    if (x == digPositionX && y == digPositionY)
                        continue;

                    var digCell = Map.DigMap[x, y];
                    if (Map.HoleMap.ArrayTileMap[x, y, 0] < 0 &&
                        !string.IsNullOrEmpty(digCell) &&
                        digCell.Contains(':'))
                    {
                        var split = digCell.Split(':');
                        if (split.Length >= 2 && Game1.GameManager.SaveManager.GetString(split[1], "0") != "1")
                            return new Vector2(x * 16 + 8, y * 16 + 8);
                    }
                }
            }
            return Vector2.Zero;
        }

        private void UpdateAttack()
        {
            var damageState = (HittableComponent)_enemyTarget.Components[HittableComponent.Index];
            var direction = damageState.HittableBox.Box.Center - new Vector2(EntityPosition.X, EntityPosition.Y - 8);

            // attack the enemy if we are close enough
            if (direction.Length() < 5)
            {
                // already removed from the map?
                if (_enemyTarget.Map != null)
                    damageState?.Hit(this, _body.VelocityTarget * 0.1f, HitType.BowWow, 8, false);

                ToIdle();
            }

            if (_body.VelocityTarget == Vector2.Zero)
                ToIdle();

            UpdatePosition();
        }

        private void UpdateTreasure()
        {
            var direction = _treasurePosition - EntityPosition.Position;

            // move to the treasure
            if (direction.Length() < 5)
            {
                // already removed from the map?
                Game1.GameManager.StartDialogPath("bowWow_dig");
                ToIdle();
            }

            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (_followMode)
                _origin = MapManager.ObjLink.Position - new Vector2(0, 4);

            var distance = (EntityPosition.Position +
                new Vector2(_body.VelocityTarget.X + _body.Velocity.X, _body.VelocityTarget.Y + _body.Velocity.Y) * Game1.TimeMultiplier - new Vector2(0, 4)) - _origin;
            var dist = distance.Length();

            if (dist > _chainMax)
            {
                var dir = distance;
                dir.Normalize();
                var newPosition = _origin + dir * _chainMax + new Vector2(0, 4);

                var direction = newPosition - EntityPosition.Position;
                if (direction.Length() > 0)
                    direction.Normalize();

                var mult = 0.125f + Math.Clamp((dist - _chainMax) / 4, 0, 4);
                _chainPull = direction * mult;

                if (_followMode)
                {
                    if (dist < (distance + _body.VelocityTarget).Length())
                        _body.VelocityTarget = AnimationHelper.MoveToTarget(_body.VelocityTarget, Vector2.Zero, Game1.TimeMultiplier);

                    _outsideCounter -= Game1.DeltaTime;
                    if (_outsideCounter <= 0)
                    {
                        _outsideCounter += 350;
                        SetWalkDirection(direction);
                        _body.VelocityTarget *= 1.5f;
                        _aiComponent.ChangeState("walking", true);
                    }
                }
                else
                {
                    // Hard clamp: compute actual current overshoot and snap position back to
                    // the chain boundary. Also kill all outward XY velocity so there is no
                    // rubber-band correction visible on the next frame.
                    var actualDistance = (EntityPosition.Position - new Vector2(0, 4)) - _origin;
                    var actualDist = actualDistance.Length();
                    if (actualDist > _chainMax)
                    {
                        var clampDir = actualDistance / actualDist;
                        EntityPosition.Set(new Vector2(
                            _origin.X + clampDir.X * _chainMax,
                            _origin.Y + clampDir.Y * _chainMax + 4));

                        // Kill XY velocity but preserve Z so a jump arc completes normally.
                        _body.VelocityTarget = Vector2.Zero;
                        _body.Velocity = new Vector3(0, 0, _body.Velocity.Z);
                        _chainPull = Vector2.Zero;

                        // If he was mid-lunge, end the attack cleanly.
                        if (_aiComponent.CurrentStateId == "attackPlayer")
                            ToIdle();
                    }
                }
            }
            else
            {
                _outsideCounter = 350;
                _chainPull = Vector2.Zero;
            }

            _body.AdditionalMovementVT = _chainPull;
            _chainPull *= (float)Math.Pow(0.75f, Game1.TimeMultiplier);
            UpdateChain();
        }

        private void UpdateChain()
        {
            // update the chain
            var directionOffset = AnimationHelper.DirectionOffset[_direction] * new Vector2(6, 3);
            _currentDirectionOffset = Vector2.Lerp(_currentDirectionOffset, directionOffset, Game1.TimeMultiplier * 0.25f);

            var startPosition = new Vector3(_origin.X, _origin.Y, _followMode ? MapManager.ObjLink.EntityPosition.Z : 0);
            var goalPosition = new Vector3(
                EntityPosition.Position.X - _currentDirectionOffset.X,
                EntityPosition.Position.Y - 4 - _currentDirectionOffset.Y,
                EntityPosition.Z);

            startPosition.Z = MathHelper.Clamp(startPosition.Z, 0, 12);

            _chain.UpdateChain(startPosition, goalPosition);
        }
        
        private void OnCollision(Values.BodyCollision moveCollision)
        {
            // rotate after wall collision
            // top collision
            if ((moveCollision & Values.BodyCollision.Horizontal) != 0)
            {
                if (!_changeDirectionSwitch.State)
                    return;
                _changeDirectionSwitch.Reset();

                _body.VelocityTarget.X = -_body.VelocityTarget.X * 0.5f;
            }
            // vertical collision
            else if ((moveCollision & Values.BodyCollision.Vertical) != 0)
            {
                _body.VelocityTarget.Y = -_body.VelocityTarget.Y * 0.5f;
            }

            if ((moveCollision & (Values.BodyCollision.Vertical | Values.BodyCollision.Horizontal)) != 0)
            {
                _direction = AnimationHelper.GetDirection(_body.VelocityTarget);
                _animator.Play("walk_" + _direction);
            }
        }
    }
}