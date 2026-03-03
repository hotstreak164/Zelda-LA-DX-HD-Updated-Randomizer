using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;

namespace ProjectZ.InGame.GameObjects.Things
{
    public class ObjCameraField : GameObject
    {
        public ObjCameraField() : base("editor field") 
        { 
            EditorColor = Color.Yellow * 0.65f;
        }

        public ObjCameraField(Map.Map map, int posX, int posY) : base(map)
        {
            EntityPosition = new CPosition(posX, posY, 0);
            EntitySize = new Rectangle(0, 0, 16, 16);
            Game1.ClassicCamera.AddToList(this);
        }
    }
}