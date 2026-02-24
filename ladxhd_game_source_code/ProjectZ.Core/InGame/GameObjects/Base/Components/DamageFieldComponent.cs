using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Base.Components
{
    class DamageFieldComponent : Component
    {
        public new static int Index = 10;
        public static int Mask = 0x01 << Index;

        public delegate bool OnDamageTemplate();
        public OnDamageTemplate OnDamage;

        public delegate void OnDamagedPlayerTemplate();
        public OnDamagedPlayerTemplate OnDamagedPlayer;

        public CBox CollisionBox;
        public HitType HitType;

        public float PushMultiplier = 1.85f;
        public int Strength;
        public int Direction = -1;
        public bool IsActive = true;

        public DamageFieldComponent(CBox collisionBox, HitType hitType, int damageStrength)
        {
            CollisionBox = collisionBox;
            HitType = hitType;
            Strength = damageStrength;
            OnDamage = DamagePlayer;
        }

        public bool DamagePlayer()
        {
            if (MapManager.ObjLink.HoleFalling == true)
                return false;

            var damagedPlayer = MapManager.ObjLink.HitPlayer(CollisionBox.Box, HitType, Strength, PushMultiplier, Direction);
            if (damagedPlayer && OnDamagedPlayer != null)
                OnDamagedPlayer();

            return damagedPlayer;
        }
    }
}
