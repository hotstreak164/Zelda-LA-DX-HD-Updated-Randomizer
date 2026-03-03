using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.GameObjects.Base.Systems;
using ProjectZ.InGame.GameObjects.Effects;
using ProjectZ.InGame.GameObjects.Things;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.NPCs
{
    internal class ObjBowWowWater : GameObject
    {
        private readonly Animator _animator;
        private readonly DrawComponent _drawComponent;

        public CSprite _sprite;
        public CPosition _position;
        public ObjBowWow _host;

        private Vector2 _offset = new Vector2(-3, 11);

        private bool _effectPlayed;

        public ObjBowWowWater() : this("bowwow_water") { }

        public ObjBowWowWater(string spriteName) : base(spriteName) { }

        public ObjBowWowWater(Map.Map map, float posX, float posY, ObjBowWow host) : base(map)
        {
            EntityPosition = new CPosition(posX + _offset.X, posY + _offset.Y, 0);

            _host = host;

            var _sprite = new CSprite(EntityPosition);

            _animator = AnimatorSaveLoad.LoadAnimator("NPCs/bowwow_water");
            var animationComponent = new AnimationComponent(_animator, _sprite, new Vector2(-8, -15));

            AddComponent(BaseAnimationComponent.Index, animationComponent);
            AddComponent(DrawComponent.Index, _drawComponent = new DrawCSpriteComponent(_sprite, Values.LayerBottom));
            AddComponent(UpdateComponent.Index, new UpdateComponent(Update));

            Map.Objects.SpawnObject(this);
            Map.Objects.RegisterAlwaysAnimateObject(this);

            _animator.Play("stand");
        }

        private void Update()
        {
            // Update the position of the water effect.
            _position = new CPosition(_host.EntityPosition.Position.X + _offset.X, _host.EntityPosition.Position.Y + _offset.Y, 0);
            EntityPosition.Set(_position.Position);

            // Check if Bow Wow is currently in the water.
            bool inWater = SystemBody.GetFieldState(_host._body).HasFlag(MapStates.FieldStates.DeepWater);

            // Get the water state and see if he's jumping or not.
            if (inWater && _host.EntityPosition.Z <= 2.5f)
                EnableAndSplash();
            else
                Disable();

            // If the map is null destroy the object.
            if (Map == null || _host.Map == null)
                Destroy();
        }

        private void Disable()
        {
            _drawComponent.IsActive = false;
            _effectPlayed = false;
        }

        private void EnableAndSplash()
        {
            _drawComponent.IsActive = true;
            if (!_effectPlayed)
            {
                var splashAnimator = new ObjAnimator(_host._body.Owner.Map, 0, 0, 0, 3, Values.LayerPlayer, "Particles/splash", "idle", true);
                splashAnimator.EntityPosition.Set(new Vector2(
                    _host._body.Position.X + _host._body.OffsetX + _host._body.Width / 2f,
                    _host._body.Position.Y + _host._body.OffsetY + _host._body.Height - _host._body.Position.Z - 3));
                Game1.GameManager.MapManager.CurrentMap.Objects.SpawnObject(splashAnimator);
            }
            _effectPlayed = true;
        }

        public void Destroy()
        {
            _drawComponent.IsActive = false;
            Map.Objects.RemoveObject(this);
        }
    }
}