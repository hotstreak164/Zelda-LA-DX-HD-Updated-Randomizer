using System;
using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Effects;
using ProjectZ.InGame.GameObjects.Things;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Base.Components.AI
{
    class AiDamageState
    {
        public delegate void OnDeleteTemplate(bool pieceOfPower);
        public OnDeleteTemplate OnDeath;

        public delegate void OnLiveZero();
        public OnLiveZero OnLiveZeroed;

        public delegate void OnBurnDelegate();
        public OnBurnDelegate OnBurn;

        public int ExplosionOffsetY;
        public Point FlameOffset;

        public string SpawnItem;

        public float HitMultiplierX = 5;
        public float HitMultiplierY = 5;

        public bool IgnoreZeroDamage;
        public bool IsActive = true;
        public bool MoveBody = true;
        public bool UpdateLastStateFire;
        public bool DeathAnimation = true;
        public bool SpawnItems = true;
        public bool PlayDeathSound = true;
        public bool PlayDeathExplosions = true;

        private readonly GameObject _gameObject;
        private readonly BodyComponent _body;
        private readonly AiComponent _aiComponent;
        private readonly CSprite _sprite;

        public SpriteShader DamageSpriteShader;
        private SpriteShader _normalShader;

        public AiTriggerCountdown DamageTrigger;
        private AiTriggerCountdown _deathCountdown;

        private bool _pieceOfPower;
        private float _bodyDrag;
        private float _bodyDragAir;

        private double _pieceOfPowerCounter;
        private int _pieceOfPowerDeathCount;

        public int CurrentLives;

        private bool _damageBlink;
        private bool _returnState;
        private readonly bool _hasBurnState;
        private bool _isDead;

        public const int BlinkTime = 66;
        public const int CooldownTime = BlinkTime * 6;
        private readonly int _cooldownTime;

        public int ExplostionWidth = 32;
        public int ExplostionHeight = 32;

        public bool HasDamageState;
        public bool BossHitSound;

        private float _deathCount = -1000;

        public AiDamageState(GameObject gameObject, BodyComponent body, AiComponent aiComponent, CSprite sprite, int lives, bool hasDamageState = true, bool hasBurnState = true, int cooldownTime = CooldownTime)
        {
            _gameObject = gameObject;
            _body = body;
            _aiComponent = aiComponent;
            _sprite = sprite;
            _normalShader = sprite.SpriteShader;

            CurrentLives = lives;

            HasDamageState = hasDamageState;
            _hasBurnState = hasBurnState;

            _cooldownTime = cooldownTime;

            // basic on death reaction
            OnDeath = BaseOnDeath;

            DamageSpriteShader = Resources.DamageSpriteShader0;

            _aiComponent.Trigger.Add(DamageTrigger = new AiTriggerCountdown(cooldownTime, DamageTick, FinishDamage));
            if (hasDamageState)
                _aiComponent.States.Add("damage", new AiState());

            var stateKnockBack = new AiState();
            stateKnockBack.Trigger.Add(new AiTriggerCountdown(cooldownTime, null, FinishKnockback));

            var statePieceOfPower = new AiState(UpdatePieceOfPower) { Init = InitPieceOfPower };
            var stateBurning = new AiState(UpdateBurning) { Init = InitBurning };
            var stateDamageDeath = new AiState { Init = () => OnDeath(false) };

            _aiComponent.Trigger.Add(_deathCountdown = new AiTriggerCountdown(cooldownTime, DeathTick, () => DeathTick(0)));

            _aiComponent.States.Add("knockBack", stateKnockBack);
            _aiComponent.States.Add("pieceOfPower", statePieceOfPower);
            if (hasBurnState)
                _aiComponent.States.Add("burning", stateBurning);
            _aiComponent.States.Add("damageDeath", stateDamageDeath);
        }

        public void AddBossDamageState(AiTriggerCountdown.TriggerEndFunction deathAnimationEnd)
        {
            OnDeath = OnDeathBoss;

            var stateDeath = new AiState(UpdateDeath);
            stateDeath.Trigger.Add(new AiTriggerCountdown(3000 / BlinkTime * BlinkTime, UpdateBlink, deathAnimationEnd));

            _aiComponent.States.Add("deathBoss", stateDeath);
        }

        public bool IsInDamageState()
        {
            return DamageTrigger.CurrentTime > 0 ||
                _aiComponent.CurrentStateId == "knockBack" ||
                _aiComponent.CurrentStateId == "burning" ||
                _aiComponent.CurrentStateId == "pieceOfPower";
        }

        public Values.HitCollision HitKnockBack(GameObject gameObject, Vector2 direction, HitType hitType, bool pieceOfPower, bool blink = true)
        {
            if (!IsActive || IsInDamageState())
                return Values.HitCollision.None;

            _aiComponent.ChangeState(pieceOfPower ? "pieceOfPower" : "knockBack");

            _damageBlink = blink;
            DamageTrigger.OnInit();

            if (pieceOfPower && !GameSettings.NoDamageLaunch)
            {
                _body.Velocity.X = direction.X * 3;
                _body.Velocity.Y = direction.Y * 3;
            }
            else
            {
                _body.Velocity.X = direction.X * HitMultiplierX;
                _body.Velocity.Y = direction.Y * HitMultiplierY;
            }

            _returnState = true;

            return Values.HitCollision.Enemy;
        }

        public Values.HitCollision OnHit(GameObject gameObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
        {
            // Because of the way the hit system works, this needs to be in any hit that doesn't default to "None" hit collision.
            if ((hitType & HitType.CrystalSmash) != 0 || (hitType & HitType.ClassicSword) != 0)
                return Values.HitCollision.None;

            // Don't register a hit if object is not active, is not in damage state, or is not pegasus boots types.
            if (!IsActive || IsInDamageState() || hitType == HitType.PegasusBootsPush || hitType == HitType.CrystalSmash)
                return Values.HitCollision.None;

            // Directly delete the GameObject if the attack comes from Bow Wow.
            if (hitType == HitType.BowWow)
            {
                // Bow Wow has a custom sound for attacking so disable the normal sound and play this one.
                Game1.GameManager.PlaySoundEffect("D360-03-03");
                PlayDeathSound = false;
                PlayDeathExplosions = false;
                DeathAnimation = false;
                OnDeath(false);
                return Values.HitCollision.Enemy;
            }
            // If the enemy can detect hits even when health is 0 (or lower).
            if (damage <= 0 && IgnoreZeroDamage || DamageTrigger.CurrentTime > 0)
                return Values.HitCollision.Enemy;

            // Reduce the enemy's health by the amount of damage recieved.
            CurrentLives -= damage;

            // Burn on powder impact.
            if ((hitType == HitType.MagicPowder || hitType == HitType.MagicRod) && _hasBurnState)
            {
                if (_aiComponent.CurrentStateId != "burning")
                {
                    _aiComponent.ChangeState("burning");
                    var speedMultiply = (hitType == HitType.MagicPowder ? 0.125f : 0.5f);

                    if (MoveBody)
                    {
                        _body.Velocity.X = direction.X * HitMultiplierX * speedMultiply;
                        _body.Velocity.Y = direction.Y * HitMultiplierY * speedMultiply;
                    }
                    // Burning with Magic Powder is a combination of two sound effects.
                    Game1.GameManager.PlaySoundEffect("D360-03-03");
                    Game1.GameManager.PlaySoundEffect("D378-18-12");

                    return Values.HitCollision.Enemy;
                }
            }
            // Don't register a hit if object is burning.
            if (_aiComponent.CurrentStateId == "burning")
                return Values.HitCollision.None;

            // Play the appropriate "hit" sound effect.
            if (!BossHitSound)
            {
                if (pieceOfPower)
                    Game1.GameManager.PlaySoundEffect("D370-17-11");

                Game1.GameManager.PlaySoundEffect("D360-03-03");
            }
            else
            {
                if (CurrentLives <= 0)
                    Game1.GameManager.PlaySoundEffect("D378-19-13");
                else
                    Game1.GameManager.PlaySoundEffect("D370-07-07");
            }
            // If the player reduced the damage launch effect.
            if (pieceOfPower && !GameSettings.NoDamageLaunch)
                _aiComponent.ChangeState("pieceOfPower");
            else
            {
                if (HasDamageState)
                {
                    _returnState = true;
                    _aiComponent.ChangeState("damage");
                }
            }
            DamageTrigger.OnInit();

            _damageBlink = damage > 0;

            if (MoveBody)
            {
                if (pieceOfPower && !GameSettings.NoDamageLaunch)
                {
                    _body.Velocity.X = direction.X * 3;
                    _body.Velocity.Y = direction.Y * 3;
                }
                else
                {
                    _body.Velocity.X = direction.X * HitMultiplierX;
                    _body.Velocity.Y = direction.Y * HitMultiplierY;
                }
            }
            // Trigger death when the enemy health is depleted.
            if (CurrentLives <= 0)
            {
                OnLiveZeroed?.Invoke();
                _deathCountdown.OnInit();
                gameObject.Map.Objects.RegisterAlwaysAnimateObject(gameObject);
            }
            return Values.HitCollision.Enemy;
        }

        public void SetDamageState(bool blink = true)
        {
            _damageBlink = blink;
            DamageTrigger.OnInit();
        }

        private void InitPieceOfPower()
        {
            _pieceOfPower = true;

            _bodyDrag = _body.Drag;
            _bodyDragAir = _body.DragAir;

            _body.Drag = 1.0f;
            _body.DragAir = 1.0f;

            _pieceOfPowerDeathCount = 0;
        }

        private void UpdatePieceOfPower()
        {
            if (!HasDamageState)
                _aiComponent.States[_aiComponent.LastStateId].Update?.Invoke();

            // Draw a trail of smoke as the enemy travels.
            if (_pieceOfPowerCounter <= 0)
            {
                _pieceOfPowerCounter = 80;
                var animation = new ObjAnimator(_gameObject.Map, 0, 0, 0, 0, Values.LayerPlayer, "Particles/pieceOfPowerTrail", "run", true);
                var aniOffset = new Vector3(_body.OffsetX + _body.Width / 2f, _body.OffsetY + _body.Height / 2f, 0);
                animation.EntityPosition.Set(_body.Position.ToVector3() + aniOffset);
                Game1.GameManager.MapManager.CurrentMap.Objects.SpawnObject(animation);
                _pieceOfPowerDeathCount++;
            }
            _pieceOfPowerCounter -= Game1.DeltaTime;

            // Filter out any extremely small velocities.
            float epsilon  = 0.001f;

            // Store the current velocities so they can be referenced.
            var bodyVelX = _body.Velocity.X;
            var bodyVelY = _body.Velocity.Y;
            var lastCollision = _body.LastVelocityCollision;

            // Test for collision along the X axis.
            bool collideL = (bodyVelX < -epsilon) && (lastCollision & Values.BodyCollision.Left) != 0;
            bool collideR = (bodyVelX > epsilon) && (lastCollision & Values.BodyCollision.Right) != 0;
            bool blockedX = collideL || collideR;

            // Test for collision along the Y axis.
            bool collideT = (bodyVelY < -epsilon) && (lastCollision & Values.BodyCollision.Top) != 0;
            bool collideB = (bodyVelY > epsilon) && (lastCollision & Values.BodyCollision.Bottom) != 0;
            bool blockedY = collideT || collideB;

            // If a collision took place then cancel the corresponding velocity.
            if (blockedX)
                _body.Velocity.X = 0;
            if (blockedY)
                _body.Velocity.Y = 0;

            // Glide on the wall depending on the angle the body moved towards the wall.
            bool collisionX = blockedX && MathF.Abs(bodyVelX) > MathF.Abs(bodyVelY);
            bool collisionY = blockedY && MathF.Abs(bodyVelY) > MathF.Abs(bodyVelX);
            bool collision  = collisionX || collisionY;

            // If both collisions happened or the counter has reached 5 iterations.
            if ((collision && _pieceOfPowerDeathCount > 1) || (_pieceOfPowerDeathCount > 5))
            {
                // The hit killed the enemy.
                if (CurrentLives <= 0)
                {
                    _body.Velocity.X = 0;
                    _body.Velocity.Y = 0;
                    _deathCountdown.Stop();
                    OnDeath(true);
                    return;
                }
                // The enemy survived the hit.
                _pieceOfPower = false;
                _body.Drag = _bodyDrag;
                _body.DragAir = _bodyDragAir;
                _aiComponent.ChangeState(_aiComponent.LastStateId, true);
            }
        }

        private void InitBurning()
        {
            OnBurn?.Invoke();
            _body.VelocityTarget = Vector2.Zero;

            // Spawn the burning effect.
            var burnAnimator = new ObjBurningEffect(_gameObject.Map, 0, 0, 0, 0);

            burnAnimator.EntityPosition.Set(_gameObject.EntityPosition.Position);

            // Move the animation with the game object.
            burnAnimator.EntityPosition.SetParent(_gameObject.EntityPosition,
                new Vector2((int)(_body.OffsetX + _body.Width / 2) + FlameOffset.X,
                            (int)(_body.OffsetY + _body.Height) - 8 + FlameOffset.Y));

            // Remove the burning sprite if the ai state changes (e.g. by falling down a hole).
            var prevFrameChange = burnAnimator.Animator.OnFrameChange;
            burnAnimator.Animator.OnFrameChange = () =>
            {
                prevFrameChange?.Invoke();
                burnAnimator.AnimationComponent.UpdateSprite();
                if (_aiComponent.Owner.Map == null || _aiComponent.CurrentStateId != "burning")
                    burnAnimator.Map.Objects.DeleteObjects.Add(burnAnimator);
            };
            // Remove the burning sprite if the animation has finished.
            var previousOnFinished = burnAnimator.Animator.OnAnimationFinished;
            burnAnimator.Animator.OnAnimationFinished = () =>
            {
                previousOnFinished?.Invoke();
                FinishBurning();
            };
            Game1.GameManager.MapManager.CurrentMap.Objects.SpawnObject(burnAnimator);
            Game1.GameManager.MapManager.CurrentMap.Objects.RegisterAlwaysAnimateObject(burnAnimator);
        }

        private void UpdateBurning()
        {
            if (UpdateLastStateFire)
                _aiComponent.States[_aiComponent.LastStateId].Update?.Invoke();
        }

        private void FinishBurning()
        {
            OnDeath(false);
        }

        private void DamageTick(double time)
        {
            if (_damageBlink)
                _sprite.SpriteShader = (_cooldownTime - time) % (BlinkTime * 2) < BlinkTime ? DamageSpriteShader : _normalShader;
        }

        private void FinishDamage()
        {
            _sprite.SpriteShader = _normalShader;

            if (CurrentLives > 0 &&
                _aiComponent.CurrentStateId != "pieceOfPower" &&
                _aiComponent.LastStateId != "pieceOfPower" &&
                _aiComponent.LastStateId != "knockBack")
            {
                // Go back to the previous state without calling the init methods.
                if (HasDamageState && _returnState)
                {
                    _returnState = false;
                    _aiComponent.ChangeState(_aiComponent.LastStateId, true);
                }
            }
        }

        private void FinishKnockback()
        {
            _sprite.SpriteShader = _normalShader;

            // Go back to the previous state without calling the init methods.
            _aiComponent.ChangeState(_aiComponent.LastStateId, true);
        }

        private void DeathTick(double time)
        {
            // Destroy the enemy when the timer runs out or the velocity is reduced to the threshold.
            if (time <= 0 || (time < _cooldownTime - 175 && _body.Velocity.Length() < 0.5f && HitMultiplierX > 0 && HitMultiplierY > 0))
            {
                if (_pieceOfPower)
                {
                    _body.Drag = _bodyDrag;
                    _body.DragAir = _bodyDragAir;
                }
                _deathCountdown.Stop();
                OnDeath(false);
            }
        }

        private void UpdateBlink(double time)
        {
            var blinkTime = BlinkTime;
            _sprite.SpriteShader = time % (blinkTime * 2) >= blinkTime ? DamageSpriteShader : _normalShader;
        }

        private void UpdateDeath()
        {
            _deathCount += Game1.DeltaTime;
            if (_deathCount < 100)
                return;
            _deathCount -= 100;

            if (PlayDeathExplosions)
                Game1.GameManager.PlaySoundEffect("D378-19-13");

            var posX = (int)_gameObject.EntityPosition.X - ExplostionWidth / 2 + Game1.RandomNumber.Next(0, ExplostionWidth) - 8;
            var posY = (int)_gameObject.EntityPosition.Y - (int)_gameObject.EntityPosition.Z + ExplosionOffsetY - ExplostionHeight + Game1.RandomNumber.Next(0, ExplostionHeight) - 8;

            // Spawn the particle effect just before the explosion.
            var explosionAnimation = new ObjAnimator(_gameObject.Map, posX, posY, Values.LayerTop, "Particles/spawn", "run", true);
            _gameObject.Map.Objects.SpawnObject(explosionAnimation);
            _gameObject.Map.Objects.RegisterAlwaysAnimateObject(explosionAnimation);
        }

        public void OnDeathBoss(bool pieceOfPower)
        {
            if (PlayDeathSound)
                Game1.GameManager.PlaySoundEffect("D370-16-10");

            IsActive = false;

            // Start the death animation.
            _aiComponent.ChangeState("deathBoss");
        }

        public void BaseOnDeath(bool pieceOfPower)
        {
            // If the object is already gone then return.
            if (_isDead || _gameObject.Map == null)
                return;

            // Prevent stacking deaths.
            _isDead = true;

            // Delete the object.
            _gameObject.Map.Objects.DeleteObjects.Add(_gameObject);

            // Play piece of power sound effect.
            if (pieceOfPower)
                Game1.GameManager.PlaySoundEffect("D370-18-12");

            // Play explosion death sound.
            if (PlayDeathSound)
                Game1.GameManager.PlaySoundEffect("D378-19-13");

            // Spawn the explosion effect.
            var bodyCenter = _body.BodyBox.Box.Center;
            bodyCenter.Y += ExplosionOffsetY;

            // Piece of power has an alternate effect. The paramater being fed to "BaseOnDeath" does not work so use the global field.
            if (DeathAnimation)
            {
                if (!_pieceOfPower)
                {
                    var posX = (int)bodyCenter.X;
                    var posY = (int)(bodyCenter.Y);
                    var explosionAnimation = new ObjDeathExplodeEffect(_gameObject.Map, posX, posY, -12, -12, false);
                    Game1.GameManager.MapManager.CurrentMap.Objects.SpawnObject(explosionAnimation);
                    Game1.GameManager.MapManager.CurrentMap.Objects.RegisterAlwaysAnimateObject(explosionAnimation);
                }
                else
                {
                    var posX = (int)bodyCenter.X;
                    var posY = (int)bodyCenter.Y;
                    var explosionAnimation = new ObjDeathExplodeEffect(_gameObject.Map, posX, posY, 0, 0, true);
                    Game1.GameManager.MapManager.CurrentMap.Objects.SpawnObject(explosionAnimation);
                    Game1.GameManager.MapManager.CurrentMap.Objects.RegisterAlwaysAnimateObject(explosionAnimation);
                }
            }
            if (!SpawnItems)
                return;

            // Add up the kill counts.
            Game1.GameManager.GuardianAcornCount++;
            Game1.GameManager.PieceOfPowerCount++;
            Game1.GameManager.KillCount++;

            // Check to see if a powerup is currently active.
            bool powerupActive = Game1.GameManager.PieceOfPowerIsActive || Game1.GameManager.GuardianAcornIsActive;

            // Piece of Power Conditions:
            // Set up now to make things easier later. It's a 25% chance to spawn between 40-44 enemies killed with a guaranteed spawn at 45.
            int count = Game1.GameManager.PieceOfPowerCount;
            int pop_threshold = 0;

            // If the count is 45 or higher then trigger the threshold.
            if (count >= 45)
                pop_threshold = count;

            // If a dice roll of 25% lands between 40 and 44, track the threshold.
            else if (count >= 40 && Random.Shared.NextDouble() < 0.25)
                pop_threshold = count;
            
            // Guardian Acorn Conditions:
            // Kill 12 enemies without getting hit. Easy enough.
            int acorn_threshold = Game1.GameManager.GuardianAcornCount;

            // Heart or Rupee Conditions:
            // From what I can tell from the disassembly, it uses a lookup table and the values can fluctuate from
            // monster to monster. So here, let's just do 35% to spawn an item then a 50/50 split for heart or rupee.
            string strObject = SpawnItem;

            if (strObject == null)
            {
                // Drop at 30% chance with a 70/30 rupee vs heart if heart drops are not disabled.
                if (Game1.RandomNumber.Next(0, 100) < 30)
                {
                    if (GameSettings.NoHeartDrops)
                        strObject = "ruby";
                    else
                        strObject = (Game1.RandomNumber.Next(0, 100) < 70) ? "ruby" : "heart";
                }
            }

            // Spawn: Guardian Acorn.
            // A simpler check since it's a static 12 kills. 
            if (acorn_threshold >= 12)
            {
                Game1.GameManager.GuardianAcornCount -= acorn_threshold;
                if (!powerupActive)
                {
                    var objItem = new ObjItem(_gameObject.Map, 0, 0, "j", null, "guardianAcorn", null, true);
                    objItem.EntityPosition.Set(new Vector3(bodyCenter.X, bodyCenter.Y, _body.Position.Z));
                    _gameObject.Map.Objects.SpawnObject(objItem);
                    objItem.SpriteShadow = new ObjSpriteShadow(_gameObject.Map, objItem, Values.LayerPlayer, "sprshadowm");
                }
            }
            
            // Spawn: Piece of Power:
            // When the threshold was met, subtract the triggering value and spawn a piece of power.
            else if (pop_threshold > 0)
            {
                Game1.GameManager.PieceOfPowerCount -= pop_threshold;
                if (!powerupActive)
                {
                    var objItem = new ObjItem(_gameObject.Map, 0, 0, "j", null, "pieceOfPower", null, true);
                    objItem.EntityPosition.Set(new Vector3(bodyCenter.X, bodyCenter.Y, _body.Position.Z));
                    _gameObject.Map.Objects.SpawnObject(objItem);
                    objItem.SpriteShadow = new ObjSpriteShadow(_gameObject.Map, objItem, Values.LayerPlayer, "sprshadowm");
                }
            }
            
            // Spawn: Heart or Rupee
            // The powerups take priority even though the logic for this drop was done first.
            else if (strObject != null)
            {
                var objItem = new ObjItem(_gameObject.Map, 0, 0, "j", null, strObject, null, true);
                objItem.EntityPosition.Set(new Vector3(bodyCenter.X, bodyCenter.Y, _body.Position.Z));
                _gameObject.Map.Objects.SpawnObject(objItem);

                if (strObject == "heart")
                    objItem.SpriteShadow = new ObjSpriteShadow(_gameObject.Map, objItem, Values.LayerPlayer, "sprshadows");
                else
                    objItem.SpriteShadow = new ObjSpriteShadow(_gameObject.Map, objItem, Values.LayerPlayer, "sprshadowm");
            }
        }
    }
}
