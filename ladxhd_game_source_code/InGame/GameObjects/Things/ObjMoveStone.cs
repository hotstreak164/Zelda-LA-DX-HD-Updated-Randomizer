using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectZ.Base;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.GameObjects.Base.Components.AI;
using ProjectZ.InGame.GameObjects.Base.Systems;
using ProjectZ.InGame.GameObjects.Dungeon;
using ProjectZ.InGame.GameObjects.Effects;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Things
{
    internal class ObjMoveStone : GameObject
    {
        private readonly List<GameObject> _collidingObjects = new List<GameObject>();
        private readonly List<GameObject> _groupOfMoveStone = new List<GameObject>();
        private readonly List<GameObject> _groupOfBarrier = new List<GameObject>();

        private readonly AiComponent _aiComponent;
        private readonly BodyComponent _body;
        private readonly CBox _box;

        private Vector2 _startPosition;
        private Vector2 _goalPosition;

        private readonly int _baseX;
        private readonly int _baseY;
        private readonly string _spriteId;
        Rectangle _collisionRect;
        int _layer;

        private readonly string _strKey;
        private readonly string _strKeyDir;
        private readonly string _strResetKey;
        private readonly int _allowedDirections;
        private readonly int _moveTime = 450;

        private int _moveDirection;
        private bool _freezePlayer;
        private bool _isResetting;
        private bool _respawn;

        public bool NoRespawn;

        // type 1 sets the key directly on push and resets it on spawn
        // used for the gravestone

        private int _type;

        public ObjMoveStone(Map.Map map, int posX, int posY, int moveDirections, string strKey, string spriteId, Rectangle collisionRectangle, int layer, int type, bool freezePlayer, string resetKey) : base(map, spriteId)
        {
            EntityPosition = new CPosition(posX, posY + 16, 0);
            EntitySize = new Rectangle(0, -16, 16, 16);

            _baseX = posX;
            _baseY = posY;
            _spriteId = spriteId;
            _collisionRect = collisionRectangle;
            _layer = layer;

            _allowedDirections = moveDirections;
            _strKey = strKey;
            _strKeyDir = strKey + "_dir";
            _type = type;
            _freezePlayer = freezePlayer;
            _strResetKey = resetKey;

            _body = new BodyComponent(EntityPosition, 3, -13, 10, 10, 8)
            {
                IgnoreHoles = true,
                IgnoreHeight = true
            };
            var movingTrigger = new AiTriggerCountdown(_moveTime, MoveTick, MoveEnd);
            var movedState = new AiState { Init = InitMoved };

            _aiComponent = new AiComponent();
            _aiComponent.States.Add("idle", new AiState());
            _aiComponent.States.Add("moving", new AiState { Trigger = { movingTrigger } });
            _aiComponent.States.Add("moved", movedState);
            new AiFallState(_aiComponent, _body, OnHoleAbsorb, null, 200);
            _aiComponent.ChangeState("idle");

            _box = new CBox(EntityPosition, collisionRectangle.X, collisionRectangle.Y, collisionRectangle.Width, collisionRectangle.Height, 16);

            var sprite = Resources.GetSprite(spriteId);

            AddComponent(AiComponent.Index, _aiComponent);
            AddComponent(BodyComponent.Index, _body);
            AddComponent(PushableComponent.Index, new PushableComponent(_box, OnPush) { InertiaTime = 500 });
            AddComponent(CollisionComponent.Index, new BoxCollisionComponent(_box, Values.CollisionTypes.Normal | Values.CollisionTypes.Hookshot));
            AddComponent(DrawComponent.Index, new DrawSpriteComponent(spriteId, EntityPosition, new Vector2(0, -sprite.SourceRectangle.Height), layer));

            if (!string.IsNullOrEmpty(_strResetKey))
                AddComponent(KeyChangeListenerComponent.Index, new KeyChangeListenerComponent(OnKeyChange));

            // set the key
            if (_type == 1 && _strKey != null)
                Game1.GameManager.SaveManager.SetString(_strKey, "0");
            if (!string.IsNullOrEmpty(_strKeyDir))
                Game1.GameManager.SaveManager.SetString(_strKeyDir, "-1");
        }

        private void OnHoleAbsorb()
        {
            if (!NoRespawn)
                Map.Objects.SpawnObject(new ObjMoveStoneRespawner(Map, _baseX, _baseY, _allowedDirections, _strKey, _spriteId, _collisionRect, _layer, _type, _freezePlayer, _strResetKey));
        }

        private void OnKeyChange()
        {
            // Check if the block should be moved back to the start position.
            var keyState = Game1.GameManager.SaveManager.GetString(_strResetKey);
            if (keyState == "1" && _aiComponent.CurrentStateId == "moved")
            {
                ResetToStart();
            }
        }

        private void ResetToStart()
        {
            _isResetting = true;
            _goalPosition = _startPosition;
            _startPosition = EntityPosition.Position;
            _moveDirection = (_moveDirection + 2) % 4;

            if (!string.IsNullOrEmpty(_strKeyDir))
                Game1.GameManager.SaveManager.SetString(_strKeyDir, "-1");

            ToMoving();
        }

        private bool IsClosestStone(Vector2 pushDirection)
        {
            const float biasStrength = 48f;

            // Get Link's body component and body box.
            var bodyComLink = MapManager.ObjLink.Components[BodyComponent.Index] as BodyComponent;
            var bodyBoxLink = bodyComLink.BodyBox.Box;

            // Get the center of Link's body and the center of the stone's body.
            Vector2 boxCenterLink = new(bodyBoxLink.Center.X, bodyBoxLink.Center.Y);
            Vector2 boxCenterRock = new(_box.Box.Center.X, _box.Box.Center.Y);
            Vector2 distBoxCenter = Vector2.Normalize(boxCenterRock - boxCenterLink);

            // Compare the distance and give it a score to compare to other stones.
            float dotLinkRock = Vector2.Dot(distBoxCenter, pushDirection);
            float distSquared = Vector2.DistanceSquared(boxCenterLink, boxCenterRock);
            float stoneScoreA = distSquared - dotLinkRock * biasStrength;

            // Find nearby objects to add to a list to find stones.
            _groupOfMoveStone.Clear();
            Map.Objects.GetComponentList(_groupOfMoveStone, (int)boxCenterLink.X - 32, (int)boxCenterLink.Y - 32, 64, 64, BodyComponent.Mask);

            // Loop through the object group.
            foreach (var obj in _groupOfMoveStone)
            {
                // If the object is not a stone then skip it.
                if (obj is not ObjMoveStone otherStone) continue;
                if (otherStone == this) continue;

                // Get the center of this stone's box.
                Vector2 boxCentOther = new(otherStone._box.Box.Center.X, otherStone._box.Box.Center.Y);
                Vector2 distBoxOther = Vector2.Normalize(boxCentOther - boxCenterLink);
                
                // Get the distance score of this stone compared to Link.
                float dotRockOther = Vector2.Dot(distBoxOther, pushDirection);
                float distSquOther = Vector2.DistanceSquared(boxCenterLink, boxCentOther);
                float stoneScoreB  = distSquOther - dotRockOther * biasStrength;

                // If it's score is not higher than the previous stone then don't push it.
                if (stoneScoreB < stoneScoreA)
                    return false;
            }
            return true;
        }

        private bool OnPush(Vector2 direction, PushableComponent.PushType type)
        {
            // Must be closest stone, no other stone is moving, impact push type, in idle state, and not heavy without power bracelet.
            if (!IsClosestStone(direction) || type == PushableComponent.PushType.Impact || _aiComponent.CurrentStateId != "idle" || (_type == 1 && Game1.GameManager.StoneGrabberLevel <= 0))
                return false;

            // Link must be pushing and his facing direction must match with the push direction.
            if (!MapManager.ObjLink.WasPushing || AnimationHelper.GetDirection(direction) != MapManager.ObjLink.Direction)
                return false;

            // Get the direction the stone should move.
            _moveDirection = AnimationHelper.GetDirection(direction);

            // Make sure the stone can move into the direction pushed.
            if (_allowedDirections != -1 && (_allowedDirections & (0x01 << _moveDirection)) == 0)
                return false;

            // Only move if there is nothing blocking the way.
            var pushVector = AnimationHelper.DirectionOffset[_moveDirection];
            var collidingRectangle = Box.Empty;
            var collisionBox = new Box(EntityPosition.X + pushVector.X * 16, EntityPosition.Y + pushVector.Y * 16 - 16, 0, 16, 16, 16);

            if (Map.Objects.Collision(collisionBox, Box.Empty, Values.CollisionTypes.Normal | Values.CollisionTypes.Passageway, 0, 0, ref collidingRectangle))
                return true;

            _startPosition = EntityPosition.Position;

            _goalPosition = new Vector2(
                _startPosition.X + pushVector.X * 16,
                _startPosition.Y + pushVector.Y * 16);

            ToMoving();

            if (!string.IsNullOrEmpty(_strResetKey))
                Game1.GameManager.SaveManager.SetString(_strResetKey, "0");

            // set the key
            if (_type == 1 && !string.IsNullOrEmpty(_strKey))
                Game1.GameManager.SaveManager.SetString(_strKey, "1");

            if (_type == 1 && !string.IsNullOrEmpty(_strKeyDir))
                Game1.GameManager.SaveManager.SetString(_strKeyDir, _moveDirection.ToString());

            return true;
        }

        private void ToMoving()
        {
            Game1.GameManager.PlaySoundEffect("D378-17-11");
            _aiComponent.ChangeState("moving");
        }

        private void MoveTick(double time)
        {
            // the movement is fast in the beginning and slows down at the end
            var amount = (float)Math.Sin((_moveTime - time) / _moveTime * (Math.PI / 2f));

            Move(amount);

            if (!_isResetting && _freezePlayer)
                MapManager.ObjLink.FreezePlayer();
        }

        private void MoveEnd()
        {
            // finished moving
            Move(1);

            // set the key
            if (_type == 0 && !string.IsNullOrEmpty(_strKey))
                Game1.GameManager.SaveManager.SetString(_strKey, "1");

            if (_isResetting)
            {
                _isResetting = false;
                _aiComponent.ChangeState("idle");
                if (_type == 0 && !string.IsNullOrEmpty(_strKeyDir))
                    Game1.GameManager.SaveManager.SetString(_strKeyDir, "-1");
            }
            else
            {
                _aiComponent.ChangeState("moved");
                if (_type == 0 && !string.IsNullOrEmpty(_strKeyDir))
                    Game1.GameManager.SaveManager.SetString(_strKeyDir, _moveDirection.ToString());
            }
            // Get any dungeon barriers nearby.
            _groupOfBarrier.Clear();
            Map.Objects.GetComponentList(_groupOfBarrier,
                (int)_body.BodyBox.Box.X, 
                (int)_body.BodyBox.Box.Y, 
                4, 4, CollisionComponent.Mask);

            // Loop through the barriers found.
            foreach (var obj in _groupOfBarrier)
            {
                if (obj is not ObjDungeonBarrier barrier) continue;

                Vector2 barPosition = barrier.EntityPosition.Position;
                Vector2 newPosition = new Vector2(354.5f, 229f);

                // The barrier we want to remove is the one in Level 7 floor 2 when pushing the block up over it.
                if (_moveDirection == 1 && barPosition == newPosition)
                {
                    Map.Objects.DeleteObjects.Add(barrier);
                    break;
                }
            }
            // Can fall into holes after finishing the movement animation.
            _body.IgnoreHoles = false;
        }

        private void InitMoved()
        {
            // fall into the water
            if (_body.CurrentFieldState.HasFlag(MapStates.FieldStates.DeepWater))
            {
                Game1.GameManager.PlaySoundEffect("D360-14-0E");

                // spawn splash effect
                var fallAnimation = new ObjAnimator(Map,
                    (int)(_body.Position.X + _body.OffsetX + _body.Width / 2.0f),
                    (int)(_body.Position.Y + _body.OffsetY + _body.Height - 2),
                    Values.LayerPlayer, "Particles/fishingSplash", "idle", true);
                Map.Objects.SpawnObject(fallAnimation);
                Map.Objects.SpawnObject(new ObjMoveStoneRespawner(Map, _baseX, _baseY, _allowedDirections, _strKey, _spriteId, _collisionRect, _layer, _type, _freezePlayer, _strResetKey));
                Map.Objects.DeleteObjects.Add(this);
            }
        }

        private void Move(float amount)
        {
            var lastBox = _box.Box;

            EntityPosition.Set(Vector2.Lerp(_startPosition, _goalPosition, amount));

            _collidingObjects.Clear();
            Map.Objects.GetComponentList(_collidingObjects, (int)EntityPosition.Position.X, (int)EntityPosition.Position.Y - 16, 17, 17, BodyComponent.Mask);

            foreach (var collidingObject in _collidingObjects)
            {
                if (collidingObject is ObjMoveStone)
                    continue;

                var body = (BodyComponent)collidingObject.Components[BodyComponent.Index];

                if (body.BodyBox.Box.Intersects(_box.Box) && !body.BodyBox.Box.Intersects(lastBox))
                {
                    var offset = Vector2.Zero;
                    if (_moveDirection == 0)
                        offset.X = _box.Box.Left - body.BodyBox.Box.Right - 0.05f;
                    else if (_moveDirection == 2)
                        offset.X = _box.Box.Right - body.BodyBox.Box.Left + 0.05f;
                    else if (_moveDirection == 1)
                        offset.Y = _box.Box.Back - body.BodyBox.Box.Front - 0.05f;
                    else if (_moveDirection == 3)
                        offset.Y = _box.Box.Front - body.BodyBox.Box.Back + 0.05f;

                    SystemBody.MoveBody(body, offset, body.CollisionTypes, false, false, false);
                    body.Position.NotifyListeners();
                }
            }
        }
    }
}