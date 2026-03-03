using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.Map;

namespace ProjectZ.InGame.GameObjects.Dungeon
{
    internal class ObjCastle : GameObject
    {
        public ObjCastle() : base("editor dungeon") { }

        public ObjCastle(Map.Map map, int posX, int posY, string dungeonName, bool updatePosition) : base(map)
        {
            if (!string.IsNullOrEmpty(dungeonName))
                Game1.GameManager.SetCastle(dungeonName);
        }
    }
}