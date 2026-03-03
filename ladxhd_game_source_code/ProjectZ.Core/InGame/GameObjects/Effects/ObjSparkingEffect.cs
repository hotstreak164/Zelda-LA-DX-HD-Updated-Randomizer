using System.IO;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Effects
{
    internal class ObjSparkingEffect : ObjAnimator
    {
        bool  light_source = true;
        int   light_red = 255;
        int   light_grn = 255;
        int   light_blu = 200;
        float light_bright = 0.8f;
        int   light_size = 26;
        float light_fade = 0.15f;

        public ObjSparkingEffect(Map.Map map, int posX, int posY, int offsetX, int offsetY)
            : base(map, posX, posY, offsetX, offsetY, Values.LayerTop, "Particles/swordPoke", "run", deleteOnFinish: true)
        {
            // If a mod file exists load the values from it.
            string modFile = Path.Combine(Values.PathLAHDMods, "ObjSparkingEffect.lahdmod");

            if (File.Exists(modFile))
                ModFile.Parse(modFile, this);

            ConfigureLight(light_source, light_red, light_grn, light_blu, light_bright, light_size, light_fade);
        }
    }
}
