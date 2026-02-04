using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Things
{
    internal class ObjSpriteShadow : GameObject
    {
        private DrawComponent _drawComponent;

        public GameObject _host;
        public CSprite _sprite;
        public CPosition _position;
        private int _layer;
        private Map.Map _currentMap;

        private Vector2 _offset = new Vector2(-8, -14);

        public ObjSpriteShadow() : this("sprshadows") { }

        public ObjSpriteShadow(string spriteName) : base(spriteName) { }

        public ObjSpriteShadow(string spriteName, int layer, float posX, float posY, Map.Map map) : base(map)
        {
            _currentMap = map;
            map.Objects.SpawnObject(this);

            _layer = layer;
            _position = new CPosition(posX, posY, 0);
            _sprite = new CSprite(spriteName, _position);
            EntityPosition = _position;

            AddComponent(DrawComponent.Index, _drawComponent = new DrawCSpriteComponent(_sprite, _layer));
            UpdateVisibility(!GameSettings.EnableShadows);
        }

        public ObjSpriteShadow(string spriteName, GameObject host, int layer, float offsetX, float offsetY, Map.Map map) : this(spriteName, host, layer, map)
        {
            _currentMap = map;
            _offset = new Vector2(offsetX, offsetY);
            _drawComponent.IsActive = !GameSettings.EnableShadows;
        }

        public ObjSpriteShadow(string spriteName, GameObject host, int layer, Map.Map map) : base(map)
        {
            _currentMap = map;

            // A failsafe to prevent crashes. This shouldn't happen but it could.
            if (_host == null && map == null) return;

            map.Objects.SpawnObject(this);

            _host = host;
            _layer = layer;
            _position = new CPosition(_host.EntityPosition.Position.X + _offset.X, _host.EntityPosition.Position.Y + _offset.Y, 0);
            _sprite = new CSprite(spriteName, _position);

            EntityPosition = _position;

            AddComponent(DrawComponent.Index, _drawComponent = new DrawCSpriteComponent(_sprite, _layer));
            AddComponent(UpdateComponent.Index, new UpdateComponent(Update));
        }

        private void Update()
        {
            _drawComponent.IsActive = !GameSettings.EnableShadows;

            if (!GameSettings.EnableShadows)
                UpdatePosition();

            if (_host is IHasVisibility hostVisbility)
                _sprite.IsVisible = hostVisbility.IsVisible;

            if (_host.Map == null)
                Destroy();

            if (!_host.IsActive)
                Destroy();
        }

        private void UpdatePosition()
        {
            _position = new CPosition(_host.EntityPosition.Position.X + _offset.X, _host.EntityPosition.Position.Y + _offset.Y, 0);
            EntityPosition.Set(_position.Position);
        }

        public void Destroy()
        {
            _drawComponent.IsActive = false;
            _currentMap.Objects.RemoveObject(this);
        }

        public void UpdateVisibility(bool visible)
        {
            _sprite.IsVisible = visible;
            _drawComponent.IsActive = visible;
        }
    }
}