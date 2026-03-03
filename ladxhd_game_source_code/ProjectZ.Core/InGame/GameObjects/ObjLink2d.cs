using System;
using Microsoft.Xna.Framework;
using ProjectZ.Base;
using ProjectZ.InGame.Controls;
using ProjectZ.InGame.GameObjects.Base.Systems;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects
{
    public partial class ObjLink
    {
        // Init Variables
        public bool Fall2DEntry;
        public bool Is2DMode;
        private bool _init;

        // Movement Values
        private Vector2 _moveVector2D;

        // Disable directional input hack and drop sound effect.
        public bool DisableDirHack2D;
        public bool NoDropSound;

        // Swimming Values
        private float MaxSwimSpeed2D = 0.50f;
        private float _swimAnimationMult;
        private int _swimDirection;
        private bool _inWater;
        private bool _wasInWater;

        // Climbing Values
        private float ClimbSpeed = 0.7f;
        private float _lastClimbY;
        private bool _isClimbing;
        private bool _wasClimbing;
        private bool _tryClimbing;
        private bool _ladderCollision;

        // Jumping Values
        private double _jumpStartTime;
        private bool _playedJumpAnimation;
        private bool _waterJump;
        private bool _spikeDamage;

        private void MapInit2D()
        {
            // Start climbing it the player is touching a ladder at the init position.
            var box = Box.Empty;
            if (Map.Objects.Collision(_body.BodyBox.Box, Box.Empty, Values.CollisionTypes.Ladder, 3, 0, ref box))
            {
                _isWalking = true;
                _isClimbing = true;
                DirectionEntry = 1;
                UpdateAnimation2D();
            }
            // The player is falling into a 2D map.
            else if (Fall2DEntry)
            {
                _jumpEndTimer = 0;
                _jump2DHold = false;
                _jump2DHeld = false;
                Fall2DEntry = false;
                CurrentState = State.Jumping;
                _body.Velocity.Y = 1.5f;
                _playedJumpAnimation = false;
                Direction = 1;
                DirectionEntry = Direction;
                Animation.Play("fall_" + Direction);
            }
            // Move down a little bit after coming from the top.
            if (DirectionEntry == 3)
                _swimVelocity.Y = 0.4f;

            _init = true;
            _jumpStartTime = 0;
            _swimDirection = DirectionEntry;
            _swimAnimationMult = 0.75f;
            _body.DeepWaterOffset = -9;
            EntityPosition.Z = 0;

            // Look towards the middle of the map.
            if (DirectionEntry % 2 != 0)
                _swimDirection = EntityPosition.X < Map.MapWidth * Values.TileSize / 2f ? 2 : 0;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  UPDATE 2D CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void Update2DFrozen()
        {
            // make sure to not fall down while frozen
            if (_isClimbing)
                _body.Velocity.Y = 0;
        }

        private void Update2D()
        {
            // Perform all the updates.
            UpdateSpriteShadow2D();
            UpdateLadder();
            UpdateWaterLava();
            UpdateWalking2D();
            UpdateSwimming2D();
            UpdateJump2D();
            UpdateAnimation2D();
            UpdateSpikeDamage();
            UpdateDrowning2D();
            UpdateMovement2D();
            UpdateMovementPhysics();
            UpdateClimbing2D();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  SPRITE SHADOW CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void UpdateSpriteShadow2D()
        {
            if (_spriteShadow != null)
            {
                Map.Objects.RemoveObject(_spriteShadow);
                _spriteShadow = null;
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  HIT PLAYER CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateMovement2D()
        {
            // Check if the player took a hit.
            if (_hitCount > 0)
                _hitVelocity *= (float)Math.Pow(0.9f, Game1.TimeMultiplier);
            else
                _hitVelocity = Vector2.Zero;
        }

        public void InflictSpikeDamage2D() => _spikeDamage = true;

        private void UpdateSpikeDamage()
        {
            // First frame after falling in lava or hit by spikes.
            if (_hitCount == CooldownTime)
            {
                if (_hitVelocity != Vector2.Zero)
                    _hitVelocity.Normalize();

                _hitVelocity *= 1.75f;
                _swimVelocity *= 0.25f;

                // repell the player up and in the direction the player came from
                if (_spikeDamage)
                {
                    _hitVelocity *= 0.85f;

                    if (_moveVector2D.X < 0)
                        _hitVelocity += new Vector2(2, 0);
                    else if (_moveVector2D.X > 0)
                        _hitVelocity += new Vector2(-2, 0);

                    _body.Velocity.X = _hitVelocity.X;
                    _body.Velocity.Y = _hitVelocity.Y;
                    _hitVelocity = Vector2.Zero;
                }
            }
            _spikeDamage = false;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  ANIMATION CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateAnimation2D()
        {
            var shieldString = Game1.GameManager.ShieldLevel == 2 ? "ms_" : "s_";
            if (!CarryShield)
                shieldString = "_";

            // Check if it's jumping state to play the jump animation.
            if (CurrentState == State.Jumping && !_playedJumpAnimation)
            {
                // If we're already in a jump animation wait until it changes. This fixes a bug when
                // jumping immediately after hitting the ground and the animation is still playing.
                if (!Animation.AnimationID.StartsWith("jump_"))
                {
                    Animation.Play("jump_" + Direction);
                    _playedJumpAnimation = true;
                }
            }
            if (_bootsHolding || _bootsRunning)
            {
                _swordChargeCounter = sword_charge_time;

                if (!_bootsRunning)
                    Animation.Play("walk" + shieldString + Direction);
                else
                {
                    // run while blocking with the shield
                    Animation.Play((CarryShield ? "walkb" : "walk") + shieldString + Direction);
                }
                Animation.SpeedMultiplier = 2.0f;
                return;
            }
            Animation.SpeedMultiplier = 1.0f;

            if ((CurrentState != State.Jumping || !Animation.IsPlaying || _waterJump) && 
                CurrentState != State.Attacking && 
                CurrentState != State.AttackBlocking && 
                CurrentState != State.AttackJumping)
            {
                if (CurrentState == State.Jumping)
                    Animation.Play("fall_" + Direction);
                
                else if (CurrentState == State.ChargeJumping)
                    Animation.Play("cjump" + shieldString + Direction);
                
                else if (CurrentState == State.Idle)
                {
                    if ((_isWalking || _isClimbing) && _jumpEndTimer <= 0)
                    {
                        var newAnimation = "walk" + shieldString + Direction;
                        if (Animation.CurrentAnimation.Id != newAnimation)
                            Animation.Play(newAnimation);
                        else if (_isClimbing)
                            Animation.IsPlaying = _isWalking;
                    }
                    else
                    {
                        Animation.Play("stand" + shieldString + Direction);
                    }
                }
                else if ((_jumpEndTimer > 0 && 
                    !IsAttackingState() &&
                    CurrentState != State.Dying &&
                    CurrentState != State.ShowToadstool &&
                    CurrentState != State.PickingUp &&
                    CurrentState != State.Powdering &&
                    CurrentState != State.Digging &&
                    CurrentState != State.Bombing &&
                    CurrentState != State.Hookshot &&
                    CurrentState != State.Ocarina &&
                    CurrentState != State.MagicRod) ||
                    (!_isWalking && (CurrentState == State.Charging || CurrentState == State.ChargeJumping)))
                {
                    Animation.Play("stand" + shieldString + Direction);
                }
                else if (CurrentState == State.Carrying)
                    Animation.Play((_isWalking ? "walkc_" : "standc_") + Direction);
                else if (_isWalking && (CurrentState == State.Charging || CurrentState == State.ChargeJumping))
                    Animation.Play("walk" + shieldString + Direction);
                else if (CurrentState == State.Blocking || CurrentState == State.ChargeBlocking)
                    Animation.Play((!_isWalking ? "standb" : "walkb") + shieldString + Direction);
                else if (CurrentState == State.Grabbing)
                    Animation.Play("grab_" + Direction);
                else if (CurrentState == State.Pulling)
                    Animation.Play("pull_" + Direction);

                // Show swimming sprite during swimming or charge swimming.
                else if (CurrentState == State.Swimming || CurrentState == State.ChargeSwimming)
                {
                    Animation.Play("swim_2d_" + _swimDirection);
                    Animation.SpeedMultiplier = _swimAnimationMult;
                }
                else if (CurrentState == State.Drowning)
                    Animation.Play("drown");
            }
            // Force a direction from analog stick movement.
            if (!DisableDirHack2D && !IsChargingState() && CurrentState != State.Grabbing && CurrentState != State.Jumping &&
                CurrentState != State.Pulling && !_isHoldingSword && CurrentState != State.Hookshot && !_hookshotPull)
            {
                Vector2 moveVector = ControlHandler.GetMoveVector2();
                if (moveVector != Vector2.Zero)
                    Direction = AnimationHelper.GetDirection(moveVector);
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  COLLISION CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnMoveCollision2D(Values.BodyCollision collision)
        {
            // prevent the body from trying to move up and directly falling down in the next step
            if ((collision & Values.BodyCollision.Horizontal) != 0 && !_isClimbing)
                _body.SlideOffset = Vector2.Zero;

            // collision with the ground
            if ((collision & Values.BodyCollision.Bottom) != 0)
            {
                if (IsJumpingState() || CurrentState == State.BootKnockback)
                {
                    if (CurrentState == State.ChargeJumping)
                        CurrentState = State.Charging;
                    else if (CurrentState == State.AttackJumping)
                        CurrentState = State.Attacking;
                    else
                        CurrentState = State.Idle;

                    // Play the sound when hitting the ground unless disabled.
                    if (!NoDropSound)
                        Game1.GameManager.PlaySoundEffect("D378-07-07");

                    // When hitting the ground reset the "held" state for the next jump.
                    _jump2DHeld = false;

                    // HACK: Jumping then just before landing plays the same frame of animation as the first
                    // frame in walking. This timer forces "stand" animation for a few frames.
                    _jumpEndTimer = 75;

                    // Reset this value for the next go around.
                    NoDropSound = false;
                }
            }
            // collision with the ceiling
            else if ((collision & Values.BodyCollision.Top) != 0)
            {
                _body.Velocity.Y = 0;
            }
            else if ((collision & Values.BodyCollision.Horizontal) != 0)
            {
                _lastMoveVelocity = Vector2.Zero;
                _swimVelocity.X = 0;
            }
            if ((collision & Values.BodyCollision.Vertical) != 0)
            {
                _hitVelocity.Y = 0;
                _swimVelocity.Y = 0;
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  MOVEMENT CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateWalking2D()
        {
            _isWalking = false;

            if ((CurrentState != State.Idle && 
                CurrentState != State.Jumping &&
                CurrentState != State.AttackJumping &&
                CurrentState != State.ChargeJumping &&
                CurrentState != State.Attacking && 
                CurrentState != State.Blocking &&
                CurrentState != State.AttackBlocking && 
                CurrentState != State.Powdering && 
                CurrentState != State.Bombing && 
                CurrentState != State.MagicRod && 
                CurrentState != State.Throwing && 
                CurrentState != State.Carrying && 
                CurrentState != State.Charging && 
                CurrentState != State.ChargeBlocking && 
                (_body.IsGrounded || _isClimbing)) || _inWater)
            {
                _moveVector2D = Vector2.Zero;
                _lastBaseMoveVelocity = _moveVector2D;
                return;
            }
            var walkVelocity = Vector2.Zero;
            if (!_isLocked && !Hookshot.IsMoving && (!IsAttackingState() || !_body.IsGrounded))
                walkVelocity = ControlHandler.GetMoveVector2();

            var walkVelLength = walkVelocity.Length();
            var vectorDirection = ToDirection(walkVelocity);

            // start climbing?
            if (_ladderCollision && ((walkVelocity.Y != 0 && Math.Abs(walkVelocity.X) <= Math.Abs(walkVelocity.Y)) || _tryClimbing) && _jumpStartTime + 175 < Game1.TotalGameTime)
            {
                _isClimbing = true;
                _tryClimbing = false;
            }
            // try climbing down?
            else if (walkVelocity.Y > 0 && Math.Abs(walkVelocity.X) <= Math.Abs(walkVelocity.Y) && !_bootsRunning)
            {
                if (_tryClimbing && !_isHoldingSword)
                    Direction = 3;
                _tryClimbing = true;
            }
            else
                _tryClimbing = false;

            if (_isClimbing && _ladderCollision)
            {
                _moveVector2D = walkVelocity * ClimbSpeed;
                _lastMoveVelocity = new Vector2(_moveVector2D.X, 0);
                if (_isClimbing)
                    Direction = 1;
            }
            // boot running; stop if the player tries to move in the opposite direction
            else if (_bootsRunning && (walkVelLength < 0 || vectorDirection != ReverseDirection(Direction)))
            {
                if (!_bootsStop)
                    _moveVector2D = AnimationHelper.DirectionOffset[Direction] * 2;
                _lastMoveVelocity = _moveVector2D;
            }
            // normally walking on the floor
            else if (walkVelLength > 0)
            {
                // if the player is walking he is walking left or right
                if (walkVelocity.X != 0)
                    walkVelocity.Y = 0;

                // update the direction if not attacking/charging
                var newDirection = AnimationHelper.GetDirection(walkVelocity);

                // reset boot counter if the player changes the direction
                if (newDirection != Direction)
                {
                    _bootsCounter %= _bootsParticleTime;
                    _bootsRunning = false;
                }
                if (newDirection != 3 &&
                    CurrentState != State.Charging && 
                    CurrentState != State.ChargeBlocking && 
                    CurrentState != State.Attacking && 
                    CurrentState != State.AttackBlocking && 
                    CurrentState != State.Jumping && 
                    CurrentState != State.AttackJumping &&
                    CurrentState != State.ChargeJumping)
                {
                    Direction = newDirection;
                }
                if (_body.IsGrounded && CurrentState != State.Hookshot && !_hookshotPull)
                {
                    // Default the added speed to zero.
                    float addSpeed = 0;

                    // If the modifier to add movement speed is used then apply it to 2D walking speed.
                    if (walkVelocity.X != 0)
                    {
                        addSpeed = walkVelocity.X > 0
                            ? GameSettings.MoveSpeedAdded
                            : -GameSettings.MoveSpeedAdded;
                    }
                    _moveVector2D = new Vector2(walkVelocity.X + addSpeed, 0);
                    _lastMoveVelocity = _moveVector2D;
                }
            }
            else if (_body.IsGrounded)
            {
                _moveVector2D = Vector2.Zero;
                _lastMoveVelocity = Vector2.Zero;
            }

            // the player has momentum when he is in the air and can not be controlled directly like on the ground
            if (!_body.IsGrounded && !_isClimbing)
            {
                walkVelocity.Y = 0;

                var distance = (_lastMoveVelocity - walkVelocity * _currentWalkSpeed).Length();

                if (distance > 0 && walkVelocity != Vector2.Zero)
                {
                    // we make sure that when walkVelocity is pointing in the same direction as _lastMoveVelocity we do not decrease the velocity if walkVelocity is smaller
                    var direction = walkVelocity;
                    direction.Normalize();
                    var speed = Math.Max(walkVelocity.Length(), _lastMoveVelocity.Length());
                    _lastMoveVelocity = AnimationHelper.MoveToTarget(_lastMoveVelocity, direction * speed, 0.05f * Game1.TimeMultiplier);
                }
                _moveVector2D = _lastMoveVelocity;

                // update the direction if the player goes left or right in the air
                // only update the animation after the jump animation was played
                if (CurrentState == State.Jumping && _moveVector2D != Vector2.Zero)
                {
                    var newDirection = AnimationHelper.GetDirection(_moveVector2D);
                    if (newDirection % 2 == 0)
                        Direction = newDirection;
                }
            }

            if (_moveVector2D.X != 0 || (_isClimbing && _moveVector2D.Y != 0))
                _isWalking = true;

            _lastBaseMoveVelocity = _moveVector2D;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  LADDER CLIMBING CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateLadder()
        {
            // Detect ladder collision and climbing state.
            var box = Box.Empty;
            _ladderCollision = Map.Objects.Collision(_body.BodyBox.Box, Box.Empty, Values.CollisionTypes.Ladder, 1, 0, ref box);
            if (!_ladderCollision && _isClimbing)
            {
                _isClimbing = false;

                if (CurrentState != State.Carrying)
                {
                    _body.Velocity.Y = 0;
                    CurrentState = State.Idle;
                }
            }
            // Climbing a ladder.
            if (_isClimbing &&
                CurrentState != State.Attacking && 
                CurrentState != State.Blocking && 
                CurrentState != State.AttackBlocking &&
                CurrentState != State.AttackJumping &&
                CurrentState != State.Dying && 
                CurrentState != State.PickingUp &&
                CurrentState != State.PreCarrying && 
                CurrentState != State.Carrying &&
                CurrentState != State.Hookshot && 
                CurrentState != State.MagicRod &&
                CurrentState != State.Powdering && 
                CurrentState != State.Throwing)
            {
                CurrentState = State.Idle;
            }
            if (_isClimbing)
                _body.Velocity.Y = 0;
        }

        private void UpdateClimbing2D()
        {
            // remove ladder collider while climbing
            if (_isClimbing || _tryClimbing)
                _body.CollisionTypes &= ~(Values.CollisionTypes.LadderTop);
            else if (CurrentState == State.Jumping || CurrentState == State.ChargeJumping)
                _body.CollisionTypes |= Values.CollisionTypes.LadderTop;
            else
                _body.CollisionTypes |= Values.CollisionTypes.LadderTop;

            // save the last position the player is grounded to use for the reset position if the player drowns
            if (_body.IsGrounded)
            {
                var bodyCenter = new Vector2(EntityPosition.X, EntityPosition.Y);
                // center the position
                // can lead to the position being inside something
                bodyCenter.X = (int)(bodyCenter.X / 16) * 16 + 8;

                // found new reset position?
                var bodyBox = new Box(bodyCenter.X + _body.OffsetX, bodyCenter.Y + _body.OffsetY, 0, _body.Width, _body.Height, _body.Depth);
                var bodyBoxFloor = new Box(bodyCenter.X + _body.OffsetX, bodyCenter.Y + _body.OffsetY + 1, 0, _body.Width, _body.Height, _body.Depth);
                var cBox = Box.Empty;

                // check it the player is not standing inside something; why???
                if (//!Game1.GameManager.MapManager.CurrentMap.Objects.Collision(bodyBox, Box.Empty, _body.CollisionTypes, 0, 0, ref cBox) &&
                    Map.Objects.Collision(bodyBoxFloor, Box.Empty, _body.CollisionTypes, Values.CollisionTypes.MovingPlatform, 0, 0, ref cBox))
                    _drownResetPosition = bodyCenter;
            }

            // Player reached the bottom of the ladder and touched ground.
            if (_isClimbing && _moveVector2D.Y > 0)
            {
                if (EntityPosition.Y == _lastClimbY)
                {
                    // Create a box to detect when the player touches the ground.
                    var groundCheckBox = new Box(EntityPosition.X + _body.OffsetX, EntityPosition.Y + _body.OffsetY + 1, 0, _body.Width, _body.Height, _body.Depth);
                    var refBox = Box.Empty;
        
                    // Detect collision between the box and the ground.
                    if (Map.Objects.Collision(groundCheckBox, Box.Empty, Values.CollisionTypes.Normal, 0, 0, ref refBox))
                    {
                        NoDropSound = true;
                        _isClimbing = false;
                        _tryClimbing = false;
                        _body.Velocity = Vector3.Zero;
                        _moveVector2D = Vector2.Zero;
                        CurrentState = State.Idle;
                        Direction = 1;
                    }
                }
            }
            // Track the last Y position to compare to next frame.
            if (_isClimbing)
                _lastClimbY = EntityPosition.Y;

            _wasClimbing = _isClimbing;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  SWIMMING CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateSwimming2D()
        {
            if (!_inWater || CurrentState == State.Drowning || CurrentState == State.Drowned)
                return;

            // direction can only be 0 or 2 while swimming
            if (Direction % 2 != 0)
                Direction = _swimDirection;

            // update stored direction for sword charging
            _lastSwimDirection = _swimDirection;

            var moveVector = Vector2.Zero;
            if (!_isLocked && CurrentState != State.Attacking && CurrentState != State.AttackSwimming)
                moveVector = ControlHandler.GetMoveVector2();

            var moveVectorLength = moveVector.Length();
            moveVectorLength = Math.Clamp(moveVectorLength, 0, MaxSwimSpeed2D);

            if (moveVectorLength > 0)
            {
                moveVector.Normalize();
                moveVector *= moveVectorLength;

                // accelerate to the target velocity
                var distance = (moveVector - _swimVelocity).Length();
                var lerpPercentage = MathF.Min(1, (0.0225f * Game1.TimeMultiplier) / distance);
                _swimVelocity = Vector2.Lerp(_swimVelocity, moveVector, lerpPercentage);

                _swimAnimationMult = moveVector.Length() / MaxSwimSpeed2D;

                Direction = AnimationHelper.GetDirection(moveVector);

                if (moveVector.X != 0)
                    _swimDirection = moveVector.X < 0 ? 0 : 2;
            }
            else
            {
                // slows down and stop
                var distance = _swimVelocity.Length();
                var lerpPercentage = MathF.Min(1, (0.0225f / distance) * Game1.TimeMultiplier);
                _swimVelocity = Vector2.Lerp(_swimVelocity, Vector2.Zero, lerpPercentage);

                _swimAnimationMult = Math.Max(0.35f, _swimVelocity.Length() / MaxSwimSpeed2D);
            }
            _moveVector2D = _swimVelocity;
            _lastMoveVelocity.X = _swimVelocity.X;
        }

        private void UpdateDrowning2D()
        {
            // Update drowning.
            if (CurrentState == State.Drowning)
            {
                if (Animation.CurrentFrameIndex < 2)
                {
                    _body.Velocity = Vector3.Zero;
                    EntityPosition.Set(new Vector2(MathF.Round(EntityPosition.X), MathF.Round(EntityPosition.Y)));
                }
                if (Animation.CurrentFrameIndex == 2)
                {
                    IsVisible = false;
                    CurrentState = State.Drowned;
                    _drownResetCounter = 500;
                }
            }
            // Update drowned.
            else if (CurrentState == State.Drowned)
            {
                _body.Velocity = Vector3.Zero;

                _drownResetCounter -= Game1.DeltaTime;
                if (_drownResetCounter <= 0)
                {
                    CurrentState = State.Idle;
                    CanWalk = true;
                    IsVisible = true;

                    _hitCount = CooldownTime;

                    if (_drownedInLava)
                    {
                        Game1.GameManager.CurrentHealth -= (int)MathF.Ceiling(2 * (GameSettings.DamageFactor * 0.25f));
                        _drownedInLava = false;
                    }
                    _body.CurrentFieldState = MapStates.FieldStates.None;
                    EntityPosition.Set(_drownResetPosition);
                }
            }
        }

        private void UpdateWaterLava()
        {
            // Detect when in water or lava.
            var inLava = (_body.CurrentFieldState & MapStates.FieldStates.Lava) != 0;
            _inWater = (_body.CurrentFieldState & MapStates.FieldStates.DeepWater) != 0 || inLava;

            if (_init)
                _wasInWater = _inWater;

            // Play jump animation whenever Link is in the air.
            if (_body.IsGrounded || _isClimbing)
                _playedJumpAnimation = false;

            // Check if Link is in deep water.
            if (_inWater)
            {
                if (!_wasInWater)
                {
                    _swimDirection = Direction;
                    if (_swimDirection % 2 != 0)
                        _swimDirection = 0;
                }

                // Start swimming if the player has flippers.
                if (HasFlippers && !inLava)
                {
                    if (!_wasInWater)
                    {
                        _swimVelocity.X = _body.VelocityTarget.X * 0.35f;
                        _swimVelocity.Y = _isClimbing ? _body.VelocityTarget.Y * 0.35f : _body.Velocity.Y;
                        _body.Velocity = Vector3.Zero;
                    }
                    if (CurrentState == State.Attacking || CurrentState == State.AttackSwimming)
                    {
                        CurrentState = State.AttackSwimming;
                    }
                    else if (CurrentState == State.Charging || CurrentState == State.ChargeSwimming)
                    {
                        CurrentState = State.ChargeSwimming;
                    }
                    else if (CurrentState == State.Hookshot)
                    {
                        CurrentState = State.Hookshot;
                    }
                    else 
                    {
                        if (CurrentState != State.AttackBlocking && 
                            CurrentState != State.PickingUp && 
                            CurrentState != State.Hookshot && 
                            CurrentState != State.Bombing &&
                            CurrentState != State.Powdering && 
                            CurrentState != State.MagicRod && 
                            CurrentState != State.Dying && 
                            CurrentState != State.PreCarrying)
                        {
                            CurrentState = State.Swimming;
                        }
                    }
                    _isClimbing = false;
                }
                // Drowning without flippers or entering lava.
                else
                {
                    if (CurrentState != State.Drowning && CurrentState != State.Drowned)
                    {
                        _body.Velocity = Vector3.Zero;
                        _body.Velocity.X = _lastMoveVelocity.X * 0.25f;

                        if (CurrentState != State.Dying)
                        {
                            Game1.GameManager.PlaySoundEffect("D370-03-03");

                            CurrentState = State.Drowning;
                            _isClimbing = false;
                            _hitCount = inLava ? CooldownTime : 0;

                            _drownedInLava = inLava;
                        }
                    }
                }
            }
            // jump a little bit out of the water
            else if (CurrentState == State.Swimming || 
                CurrentState == State.AttackSwimming || 
                CurrentState == State.ChargeSwimming)
            {
                Direction = _swimDirection;
                _lastMoveVelocity.X = _body.VelocityTarget.X;

                // jump out of the water?
                if (_swimVelocity.Y < -MaxSwimSpeed2D + GameSettings.MoveSpeedAdded)
                {
                    CurrentState = State.Idle;
                    Jump2D(false);
                }
                // just jump up a little out of the water
                else
                {
                    CurrentState = State.Jumping;
                    _body.Velocity.Y = -0.75f;
                    _playedJumpAnimation = true;
                    _waterJump = true;
                }
            }
            _body.IgnoresZ = _inWater || _hookshotPull;
            _wasInWater = _inWater;
            _init = false;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  JUMPING CODE
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void Jump2D(bool PlaySound = true)
        {
            // Ascend in the water faster.
            if (IsSwimmingState())
            {
                Game1.GameManager.PlaySoundEffect("D360-13-0D");
                _swimVelocity.Y = -1.185f;
            }
            // Must not be carrying or must be in one of the following states.
            if (CurrentState == State.Carrying || 
                (CurrentState != State.Idle && 
                CurrentState != State.Attacking && 
                CurrentState != State.AttackBlocking && 
                CurrentState != State.Charging && 
                CurrentState != State.ChargeBlocking))
                return;

            // All three states need to pass simultaneously to return.
            if (!_body.IsGrounded && !_wasInWater && !_isClimbing)
                return;

            // If climbing, set the direction.
            if (_isClimbing)
            {
                if (Math.Abs(_moveVector2D.X) > Math.Abs(_moveVector2D.Y))
                    Direction = _moveVector2D.X < 0 ? 0 : 2;
                else
                    Direction = 1;
            }
            if (PlaySound)
                Game1.GameManager.PlaySoundEffect("D360-13-0D");

            _jumpStartTime = Game1.TotalGameTime;

            // If climbing, jump velocity is reduced. When standing still, velocity  
            // is a fair bit stronger. When walking, use the maximum jump velocity.
            _body.Velocity.Y = _isClimbing 
                ? -1.5f 
                : _isWalking
                    ? -2.10f 
                    : -1.95f;

            // Set up the supporting values.
            _body.IsGrounded = false;
            _moveVector2D = Vector2.Zero;
            _isClimbing = false;
            _waterJump = false;

            // while attacking the player can still jump but without the animation
            if (CurrentState != State.Attacking && CurrentState != State.AttackBlocking &&
                CurrentState != State.Charging && CurrentState != State.ChargeBlocking)
            {
                _playedJumpAnimation = false;

                if (CurrentState == State.Attacking)
                    CurrentState = State.AttackJumping;
                else
                    CurrentState = State.Jumping;
            }
            else
                _playedJumpAnimation = true;

            // Track when the button is held and released.
            _jump2DHeld = true;

            // Convert charging state to ChargeJumping.
            if (CurrentState == State.Attacking)
                CurrentState = State.AttackJumping;
            if (CurrentState == State.Charging)
                CurrentState = State.ChargeJumping;
        }

        private void UpdateJump2D()
        {
            // Update the jump hack timer.
            if (_jumpEndTimer > 0)
                _jumpEndTimer -= Game1.DeltaTime;

            // When letting go of the jump button, the jump should end. Instead of an immediate
            // drop off, the velocity is instead greatly reduced to reduce the pull of gravity.
            if (!_jump2DHold && _jump2DHeld)
            {
                // "Threshold" is current Z velocity while "setY" is replacement velocity.
                float threshold = -1.00f;
                float setY = -0.50f;

                // Check velocity threshold over three iterations.
                for (int i = 0; i < 3; i++)
                {
                    // If currently velocity is greater than current threshold.
                    if (_body.Velocity.Y > threshold)
                    {
                        // Reduce the velocity.
                        _body.Velocity.Y = setY;
                        _jump2DHeld = false;
                        break;
                    }
                    // Adjust the values for the next iteration.
                    threshold -= 0.15f;
                    setY += 0.15f;
                }
            }
            // When velocity falls below the amount it sets, don't apply it anymore.
            if (_body.Velocity.Y >= -0.50)
                _jump2DHeld = false;

            var initState = CurrentState;
            if (!_body.IsGrounded && !_isClimbing && !_bootsRunning &&
                (CurrentState == State.Idle || CurrentState == State.Blocking) &&
                (!_tryClimbing || !_ladderCollision))
            {
                if (CurrentState == State.Charging)
                    CurrentState = State.ChargeJumping;
                else
                    CurrentState = State.Jumping;

                _waterJump = false;

                // if we get pushed down we change the direction in the push direction
                // this does not work for all cases but we only need if for the evil eagle boss where it should work correctly
                if (_body.LastAdditionalMovementVT.X != 0)
                    Direction = _body.LastAdditionalMovementVT.X < 0 ? 0 : 2;

                if (_wasClimbing)
                {
                    // not ontop of a ladder
                    if (SystemBody.MoveBody(_body, new Vector2(0, 2), _body.CollisionTypes | Values.CollisionTypes.LadderTop, false, false, true) == Values.BodyCollision.None)
                    {
                        SystemBody.MoveBody(_body, new Vector2(0, -2), _body.CollisionTypes | Values.CollisionTypes.LadderTop, false, false, true);

                        if (Math.Abs(_moveVector2D.X) >= Math.Abs(_moveVector2D.Y))
                            Direction = _moveVector2D.X < 0 ? 0 : 2;
                        else
                            Direction = 1;
                    }
                    // aligned with the top of the ladder
                    else
                    {
                        _body.IsGrounded = true;
                        _body.Velocity.Y = -0.50f;
                        CurrentState = initState;
                    }
                }
            }
        }
    }
}
