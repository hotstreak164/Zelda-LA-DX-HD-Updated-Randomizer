using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Dungeon
{
    internal class ObjAnglerFishBarrier : GameObject
    {
        CSprite _sprite;
        public int PosX;
        public int PosY;

        public ObjAnglerFishBarrier() : base("fish_barrier") { }

        public ObjAnglerFishBarrier(Map.Map map, int posX, int posY) : base(map)
        {
            EntityPosition = new CPosition(posX, posY, 0);
            EntitySize = new Rectangle(0, 0, 16, 16);

            PosX = posX;
            PosY = posY;

            var sprite = Resources.GetSprite("fish_barrier");
            _sprite = new CSprite(sprite, EntityPosition, Vector2.Zero);

            var collisionBox = new CBox(posX, posY, 0, 16, 16, 16);

            AddComponent(CollisionComponent.Index, new BoxCollisionComponent(collisionBox, Values.CollisionTypes.Normal));
            AddComponent(DrawComponent.Index, new DrawCSpriteComponent(_sprite, Values.LayerBottom));
            Map.Objects.RegisterAlwaysAnimateObject(this);
        }
    }
}
