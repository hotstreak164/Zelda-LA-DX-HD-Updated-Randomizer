using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.Map;

namespace ProjectZ.InGame.GameObjects.Dungeon
{
    internal class ObjDungeonMapless : GameObject
    {
        public ObjDungeonMapless() : base("editor dungeon") { }

        public ObjDungeonMapless(Map.Map map, int posX, int posY, string dungeonName, bool updatePosition) : base(map)
        {
            if (!string.IsNullOrEmpty(dungeonName))
                Game1.GameManager.SetDungeonMapless(dungeonName);
        }
    }
}