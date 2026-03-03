using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.Map;

namespace ProjectZ.InGame.GameObjects.Things
{
    internal class ObjMoveStoneRespawner : GameObject
    {
        private readonly int _moveDirections;
        private readonly string _strKey;
        private readonly string _spriteId;
        private readonly Rectangle _collisionRectangle;
        private readonly int _layer;
        private readonly int _type;
        private readonly bool _freezePlayer;
        private readonly string _resetKey;

        private int _lastFieldTime;
        private bool _respawnStart;
        private float _respawnTimer;
        private bool _respawnedStone;

        public ObjMoveStoneRespawner(Map.Map map, int posX, int posY, int moveDirections, string strKey, string spriteId, Rectangle collisionRectangle, int layer, int type, bool freezePlayer, string resetKey, bool fromSpawner = false) : base(map)
        {
            EntityPosition = new CPosition(posX, posY, 0);
            EntitySize = new Rectangle(0, 0, 16, 16);

            _moveDirections = moveDirections;
            _strKey = strKey;
            _spriteId = spriteId;
            _collisionRectangle = collisionRectangle;
            _layer = layer;
            _type = type;
            _freezePlayer = freezePlayer;
            _resetKey = resetKey;
            _respawnedStone = fromSpawner;

            _lastFieldTime = Map.GetUpdateState(EntityPosition.Position);

            AddComponent(UpdateComponent.Index, new UpdateComponent(Update));
            Map.Objects.RegisterAlwaysAnimateObject(this);
        }

        private void Update()
        {
            // If the stone has already been spawned from a spawner, delete this wrapper immediately.
            if (_respawnedStone)
            {
                DeleteThis();
                return;
            }

            if (Camera.ClassicMode)
            {
                // Classic (screen-locked) dungeon mode: wait for field transition
                if (MapManager.ObjLink.FieldChange)
                    _respawnStart = true;

                if (_respawnStart)
                {
                    _respawnTimer += Game1.DeltaTime;
                    if (_respawnTimer >= 250)
                    {
                        SpawnStone();
                        _respawnStart = false;
                        _respawnTimer = 0;
                    }
                }
            }
            else
            {
                // Overworld / smooth camera mode: respawn when field update time changes
                int updateState = Map.GetUpdateState(EntityPosition.Position);

                if (_lastFieldTime < updateState)
                    SpawnStone();
            }
        }

        private void DeleteThis()
        {
            Map.Objects.DeleteObjects.Add(this);
        }

        private void SpawnStone()
        {
            Map.Objects.DeleteObjects.Add(this);
            Map.Objects.SpawnObject(new ObjMoveStone(Map, (int)EntityPosition.X, (int)EntityPosition.Y, _moveDirections, _strKey, _spriteId, _collisionRectangle, _layer, _type, _freezePlayer, _resetKey));
        }
    }
}
