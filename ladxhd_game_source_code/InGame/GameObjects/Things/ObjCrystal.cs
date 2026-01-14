using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Things
{
    internal class ObjCrystal : GameObject
    {
        private readonly Color _lightColor;
        private readonly string _dialogPath;
        private readonly bool _isHardCrystal;
        private readonly int _colorIndex;
        private readonly string _spriteId;

        private CBox _hardCollideBox;
        private CBox _softCollideBox;
        private CBox _softHittableBox;

        bool light_source = true;
        int light_size = 80;

        int light_red_1 = 240;
        int light_grn_1 = 100;
        int light_blu_1 = 255;
        float light_bright_1 = 1.00f;

        int light_red_2 = 255;
        int light_grn_2 = 255;
        int light_blu_2 = 255;
        float light_bright_2 = 0.25f;

        public ObjCrystal(Map.Map map, int posX, int posY, string spriteId, int color, bool hardCrystal, string dialogPath) : base(map, spriteId)
        {
            string modFile = Path.Combine(Values.PathLAHDMods, "ObjCrystal.lahdmod");

            if (File.Exists(modFile))
                ModFile.Parse(modFile, this);

            var sprite = Resources.GetSprite(spriteId);

            EntityPosition = new CPosition(posX + 8, posY + 16, 0);
            EntitySize = new Rectangle(-40, -48, 80, 80);

            _spriteId = spriteId;
            _colorIndex = color;
            _isHardCrystal = hardCrystal;
            _dialogPath = dialogPath;

            _lightColor = color == 0
                ? new Color(light_red_1, light_grn_1, light_blu_1) * light_bright_1
                : new Color(light_red_2, light_grn_2, light_blu_2) * light_bright_2;

            _hardCollideBox = new CBox(posX, posY + 4, 0, 16, 12, 16);
            _softCollideBox = new CBox(EntityPosition, -7, -14, 0, 14, 14, 8);
            _softHittableBox = new CBox(EntityPosition, -8, -16, 0, 16, 16, 8);

            if (_isHardCrystal)
                AddComponent(PushableComponent.Index, new PushableComponent(_hardCollideBox, OnPush) { InertiaTime = 50 });

            AddComponent(HittableComponent.Index, new HittableComponent(_isHardCrystal ? _hardCollideBox : _softHittableBox, OnHit));
            AddComponent(CollisionComponent.Index, new BoxCollisionComponent(_isHardCrystal ? _hardCollideBox : _softCollideBox, Values.CollisionTypes.Normal));
            AddComponent(DrawComponent.Index, new DrawSpriteComponent(spriteId, EntityPosition, new Vector2(-8, -16), Values.LayerPlayer));
            AddComponent(LightDrawComponent.Index, new LightDrawComponent(DrawLight));
        }

        private bool OnPush(Vector2 direction, PushableComponent.PushType pushType)
        {
            if (pushType == PushableComponent.PushType.Impact)
                return false;

            // Don't show the "Oh? What a weird object!" message if disabled.
            if (!GameSettings.NoHelperText && _dialogPath == "crystal_hard")
                Game1.GameManager.StartDialogPath(_dialogPath);

            return false;
        }

        private void DrawLight(SpriteBatch spriteBatch)
        {
            if (light_source && GameSettings.ObjectLighting)
            {
                var _lightRectangle = new Rectangle((int)EntityPosition.X - light_size / 2, (int)EntityPosition.Y - 8 - light_size / 2, light_size, light_size);
                DrawHelper.DrawLight(spriteBatch, _lightRectangle, _lightColor);
            }
        }

        private Values.HitCollision OnHit(GameObject gameObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
        {
            // If "Classic Sword" is enabled, only the tile that the bush/grass is on should "hit".
            if (!_isHardCrystal && MapManager.ObjLink.ClassicSword && (hitType & HitType.Sword) != 0 && !MapManager.ObjLink.IsPoking)
                return Values.HitCollision.None;

            // For large crystals, only dashing with Pegasus Boots should be able to smash them. "CrystalSmash" is dashing when sword is unequipped.
            if ((_isHardCrystal && hitType != HitType.PegasusBootsSword && hitType != HitType.CrystalSmash) || (hitType & HitType.SwordHold) != 0 || hitType == HitType.Hookshot)
                return Values.HitCollision.None;

            // For small crystals, all kinds of shit can smash them.
            if ((hitType & HitType.Sword) != 0 && (hitType & HitType.Boomerang) != 0 && (hitType & HitType.Hookshot) != 0 && (hitType & HitType.Bomb) != 0)
                return Values.HitCollision.None;

            Game1.GameManager.PlaySoundEffect("D378-09-09");

            Map.Objects.DeleteObjects.Add(this);
            Map.Objects.SpawnObject(new CrystalRespawner(Map, (int)EntityPosition.X - 8, (int)EntityPosition.Y - 16, _spriteId, _dialogPath, _isHardCrystal, _colorIndex));

            var mult = hitType == HitType.PegasusBootsSword || hitType == HitType.CrystalSmash ? 1.0f : 0.25f;

            var velZ = 0.5f;
            var diff = 200f;

            var vector0 = new Vector3(-1, -1,  0) * Game1.RandomNumber.Next(50, 75) / diff + new Vector3(direction * mult, velZ);
            var vector1 = new Vector3(-1,  0,  0) * Game1.RandomNumber.Next(50, 75) / diff + new Vector3(direction * mult, velZ);
            var vector2 = new Vector3( 1, -1,  0) * Game1.RandomNumber.Next(50, 75) / diff + new Vector3(direction * mult, velZ);
            var vector3 = new Vector3( 1,  0,  0) * Game1.RandomNumber.Next(50, 75) / diff + new Vector3(direction * mult, velZ);

            var stone0 = new ObjSmallStone(Map, (int)EntityPosition.X + 2, (int)EntityPosition.Y - 10, Game1.RandomNumber.Next(4, 8), vector0);
            var stone1 = new ObjSmallStone(Map, (int)EntityPosition.X + 2, (int)EntityPosition.Y - 6,  Game1.RandomNumber.Next(4, 8), vector1);
            var stone2 = new ObjSmallStone(Map, (int)EntityPosition.X + 6, (int)EntityPosition.Y - 10, Game1.RandomNumber.Next(4, 8), vector2);
            var stone3 = new ObjSmallStone(Map, (int)EntityPosition.X + 6, (int)EntityPosition.Y - 6,  Game1.RandomNumber.Next(4, 8), vector3);

            Map.Objects.SpawnObject(stone0);
            Map.Objects.SpawnObject(stone1);
            Map.Objects.SpawnObject(stone2);
            Map.Objects.SpawnObject(stone3);

            if ((hitType & HitType.Sword) != 0)
                return Values.HitCollision.NoneBlocking;

            return Values.HitCollision.Blocking;
        }
    }
}