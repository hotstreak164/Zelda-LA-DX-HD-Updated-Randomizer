using ProjectZ.InGame.GameObjects.Base;

namespace ProjectZ.InGame.GameObjects.Dungeon
{
    internal class ObjDungeonEgg : GameObject
    {
        public ObjDungeonEgg() : base("editor dungeon") { }

        public ObjDungeonEgg(Map.Map map, int posX, int posY, string dungeonName, bool updatePosition) : base(map)
        {
            if (!string.IsNullOrEmpty(dungeonName))
                Game1.GameManager.SetDungeonEgg(dungeonName);
        }
    }
}