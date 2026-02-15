using System;
using Microsoft.Xna.Framework;
using ProjectZ.Base;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.GameObjects.Base.Components.AI;
using ProjectZ.InGame.GameObjects.Effects;
using ProjectZ.InGame.GameObjects.Things;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.MidBoss
{
    internal class MBossDesertLanmola : GameObject
    {
        private readonly AiComponent _aiComponent;
        private readonly AiTriggerCountdown _damageTrigger;
        private readonly Animator _animator;
        private readonly CSprite _sprite;
        private readonly CPosition _position;

        private MBossDesertLanmolaHead _head;
        private MBossDesertLanmolaBody[] _bodyParts = new MBossDesertLanmolaBody[6];

        private readonly AiTriggerCountdown _jumpCountdown;
        private readonly RectangleF _field;
        private readonly Rectangle _fieldSmall;

        private Vector2 _jumpStartPosition;
        private Vector2 _jumpPosition;
        private string _triggerKey;
        private string _saveKey;
        private int _jumpTime;

        private int _lives = EnemyLives.DesertLanmola;

        private bool _jumpLandSound;
        private bool _playerLeft = true;
        private bool _defeated;

        private const int CooldownTime = 350;
        private const int DespawnTime = 5500;

        public MBossDesertLanmola() : base("desert lanmola") { }

        public MBossDesertLanmola(Map.Map map, int posX, int posY, string triggerKey, string saveKey) : base(map)
        {
            _position = new CPosition(posX, posY, 0);
            EntitySize = new Rectangle(-8, -16, 16, 16);
            CanReset = true;
            OnReset = Reset;

            _triggerKey = triggerKey;
            _saveKey = saveKey;
            _field = map.GetField(posX, posY);
            _fieldSmall = map.GetField(posX, posY, 16);

            if (!string.IsNullOrEmpty(_saveKey) &&
                Game1.GameManager.SaveManager.GetString(_saveKey) == "1")
            {
                IsDead = true;
                return;
            }

            var stateIdle = new AiState();
            var stateWaiting = new AiState();
            stateWaiting.Trigger.Add(new AiTriggerCountdown(500, null, ToSpawning));
            var stateSpawning = new AiState();
            stateSpawning.Trigger.Add(new AiTriggerCountdown(500, null, ToJumping));
            var stateJumping = new AiState();
            stateJumping.Trigger.Add(_jumpCountdown = new AiTriggerCountdown(500, JumpTick, JumpEnd));
            var stateDespawning = new AiState();
            stateDespawning.Trigger.Add(new AiTriggerCountdown(DespawnTime, DespawnTick, DespawnEnd));

            _aiComponent = new AiComponent();

            _aiComponent.Trigger.Add(new AiTriggerUpdate(Update));
            _aiComponent.Trigger.Add(_damageTrigger = new AiTriggerCountdown(CooldownTime, DamageTick, FinishDamage));
            _aiComponent.States.Add("idle", stateIdle);
            _aiComponent.States.Add("waiting", stateWaiting);
            _aiComponent.States.Add("spawning", stateSpawning);
            _aiComponent.States.Add("jumping", stateJumping);
            _aiComponent.States.Add("despawning", stateDespawning);

            _aiComponent.ChangeState("idle");

            _animator = AnimatorSaveLoad.LoadAnimator("MidBoss/desertLanmola");
            _animator.Play("ground");

            _sprite = new CSprite(_position);
            _sprite.IsVisible = false;

            AddComponent(BaseAnimationComponent.Index, new AnimationComponent(_animator, _sprite, new Vector2(0, 0)));
            AddComponent(DrawComponent.Index, new DrawCSpriteComponent(_sprite, Values.LayerBottom));
            AddComponent(AiComponent.Index, _aiComponent);

            if (!string.IsNullOrEmpty(_triggerKey))
                AddComponent(KeyChangeListenerComponent.Index, new KeyChangeListenerComponent(OnKeyChange));

            _head = new MBossDesertLanmolaHead(map, this, new Vector2(posX, posY));
            map.Objects.SpawnObject(_head);
            _head.Hide();

            for (var i = 0; i < _bodyParts.Length; i++)
            {
                _bodyParts[i] = new MBossDesertLanmolaBody(map, new Vector2(posX, posY), i == 5);
                map.Objects.SpawnObject(_bodyParts[i]);
                _bodyParts[i].Hide();
            }
        }

        private void Reset()
        {
            _lives = EnemyLives.DesertLanmola;
        }

        private void OnKeyChange()
        {
            // The Lanmola is trigger when stepping over a leaveButton found
            // inside the two entrances to the quicksand area.
            var triggerState = Game1.GameManager.SaveManager.GetString(_triggerKey);
            Game1.GameManager.SaveManager.SetString(_triggerKey, "0");

            // The Lanmola leaveButton was triggered.
            if (_playerLeft && triggerState == "1" && !_defeated)
            {
                _playerLeft = false;

                // Start the boss music and the dialog path.
                Game1.GameManager.SetMusic(79, 2);
                Game1.GameManager.StartDialogPath("desertLanmola");

                if (_aiComponent.CurrentStateId == "idle")
                    _aiComponent.ChangeState("waiting");
            }
        }

        private void Update()
        {
            // Get the current field the boss is in.
            Rectangle currentField = GameMath.RectFToRect(_field);

            // Adjust the rect slightly when classic camera is enabled.
            if (Camera.ClassicMode)
                currentField = new Rectangle(currentField.X + 1, currentField.Y + 1, currentField.Width - 2, currentField.Height - 2);

            // The player left the field.
            if (!_playerLeft && !currentField.Contains(MapManager.ObjLink.CenterPosition.Position))
            {
                _playerLeft = true;
                Game1.GameManager.SetMusic(-1, 2);
            }
        }

        private void ToSpawning()
        {
            if (_playerLeft)
            {
                _aiComponent.ChangeState("idle");
                return;
            }

            _aiComponent.ChangeState("spawning");

            var randomX = Game1.RandomNumber.Next(0, 8);
            var randomY = Game1.RandomNumber.Next(0, 6);

            if (randomX == 0 || randomX == 7)
                randomY = Math.Clamp(randomY, 1, 6);

            _position.Set(new Vector2(_fieldSmall.X + randomX * 16 + 8, _fieldSmall.Y + randomY * 16 + 16));

            _sprite.IsVisible = true;
            _animator.Play("ground");
        }

        private void ToJumping()
        {
            if (_playerLeft)
            {
                _sprite.IsVisible = false;
                _aiComponent.ChangeState("idle");
                return;
            }

            _sprite.IsVisible = false;

            var direction = new Vector2(_fieldSmall.Center.X, _fieldSmall.Center.Y) - _position.Position;
            direction.Normalize();

            var randomDistance = Game1.RandomNumber.Next(48, 80);
            _jumpPosition = _position.Position + direction * randomDistance;

            _jumpStartPosition = _position.Position;

            _jumpTime = Game1.RandomNumber.Next(1500, 2000);
            _jumpCountdown.StartTime = _jumpTime + 1000;

            _jumpLandSound = false;

            _aiComponent.ChangeState("jumping");

            _head.Spawn(direction);
            _head.EntityPosition.Set(_jumpStartPosition);

            // sand particles
            SpawnParticles(new Vector2(_position.X, _position.Y));
        }

        private void JumpTick(double count)
        {
            var jumpState = 1 - (float)((count - 1000) / _jumpTime);
            var headPosition = GetPosition(jumpState);

            if (jumpState > 1 && _head.IsVisible)
            {
                _head.Hide();
                // sand particles
                SpawnParticles(new Vector2(headPosition.X, headPosition.Y));
            }

            if (!_jumpLandSound && jumpState > 0.5f)
            {
                _jumpLandSound = true;
                Game1.GameManager.PlaySoundEffect("D378-35-23");
            }

            _head.EntityPosition.Set(headPosition);

            if (jumpState > 0.8f)
                _head.SetDown();

            for (var i = 0; i < _bodyParts.Length; i++)
            {
                var state = 1 - (float)((count - (5 - i) * 166) / _jumpTime);
                var partPosition = GetPosition(state);

                var spawnParticles = false;

                if (0 <= state && state <= 1 && !_bodyParts[i].IsVisible)
                {
                    spawnParticles = true;
                    _bodyParts[i].Show();
                }

                if (state >= 1 && _bodyParts[i].IsVisible)
                {
                    spawnParticles = true;
                    _bodyParts[i].Hide();
                }

                // sand particles
                if (spawnParticles)
                    SpawnParticles(new Vector2(partPosition.X, partPosition.Y));

                _bodyParts[i].EntityPosition.Set(partPosition);
            }
        }

        private void JumpEnd()
        {
            JumpTick(0);
            _aiComponent.ChangeState("waiting");
        }

        private Vector3 GetPosition(float state)
        {
            var newPosition = Vector2.Lerp(_jumpStartPosition, _jumpPosition, state);
            var x = state * 9.1365f;
            var newHeight = (0.75f * MathF.Sin(x) + 1.45f * MathF.Sin(x * 0.36f)) * 10;

            return new Vector3(newPosition.X, newPosition.Y, newHeight);
        }

        private void SpawnParticles(Vector2 position)
        {
            // sand particles
            var leftSand = new MBossDesertLanmolaSand(Map, new Vector2(position.X - 4, position.Y), false);
            Map.Objects.SpawnObject(leftSand);

            var rightSand = new MBossDesertLanmolaSand(Map, new Vector2(position.X + 4, position.Y), true);
            Map.Objects.SpawnObject(rightSand);
        }

        private void DamageTick(double time)
        {
            var currentEffect = (CooldownTime - time) % 133 < 66 ? Resources.DamageSpriteShader0 : null;
            SetEffect(currentEffect);
        }

        private void FinishDamage()
        {
            SetEffect(null);
        }

        private void DespawnTick(double time)
        {
            var currentEffect = time % 133 < 66 ? Resources.DamageSpriteShader0 : null;
            SetEffect(currentEffect);

            // despawn the parts
            if (time < DespawnTime - 2000)
            {
                for (var i = 0; i < _bodyParts.Length; i++)
                {
                    if (_bodyParts[i] != null && _bodyParts[i].IsVisible && time < DespawnTime - 2000 - (6 - i) * 500)
                    {
                        Game1.GameManager.PlaySoundEffect("D378-19-13");

                        var animation = new ObjAnimator(Map, 0, 0, Values.LayerTop, "Particles/spawn", "run", true);
                        animation.EntityPosition.Set(new Vector2(
                            _bodyParts[i].EntityPosition.X - 8,
                            _bodyParts[i].EntityPosition.Y - 16 - _bodyParts[i].EntityPosition.Z));
                        Map.Objects.SpawnObject(animation);

                        Map.Objects.DeleteObjects.Add(_bodyParts[i]);
                        _bodyParts[i] = null;
                    }
                }
            }
        }

        private void DespawnEnd()
        {
            Game1.GameManager.SetMusic(-1, 2);

            Map.Objects.DeleteObjects.Add(_head);

            var animation = new ObjAnimator(Map, 0, 0, Values.LayerTop, "Particles/spawn", "run", true);
            animation.EntityPosition.Set(new Vector2(
                _head.EntityPosition.X - 8,
                _head.EntityPosition.Y - 16 - _head.EntityPosition.Z));
            Map.Objects.SpawnObject(animation);

            // set the save key
            Game1.GameManager.SaveManager.SetString(_saveKey, "1");

            // spawn the fish dungeon key
            Map.Objects.SpawnObject(new ObjItem(Map, (int)_head.EntityPosition.X - 8, (int)_head.EntityPosition.Y - 16, "j", "dkey3Collected", "dkey3", null));

            // prevent recurrent spawns after defeat
            _defeated = true;
        }

        private void SetEffect(SpriteShader effect)
        {
            _head.Sprite.SpriteShader = effect;
            foreach (var part in _bodyParts)
                if (part != null)
                    part.Sprite.SpriteShader = effect;
        }

        public Values.HitCollision OnHit(GameObject originObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
        {
            // Because of the way the hit system works, this needs to be in any hit that doesn't default to "None" hit collision.
            if ((hitType & HitType.CrystalSmash) != 0 || (hitType & HitType.ClassicSword) != 0)
                return Values.HitCollision.None;

            if (_damageTrigger.CurrentTime > 0 || _aiComponent.CurrentStateId == "despawning")
                return Values.HitCollision.None;

            _lives -= damage;

            if (_lives > 0)
            {
                _damageTrigger.OnInit();
                Game1.GameManager.PlaySoundEffect("D370-07-07");
            }
            else
            {
                // Remove damage component from head and body parts.
                _head.Death();

                foreach (var part in _bodyParts)
                    if (part != null)
                        part.Death();

                _aiComponent.ChangeState("despawning");
                Game1.GameManager.PlaySoundEffect("D370-16-10");
            }

            return Values.HitCollision.Enemy;
        }
    }
}