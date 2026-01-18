using System.IO;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Effects
{
    internal class ObjBurningEffect : ObjAnimator
    {
        bool  light_source = true;
        int   light_red = 255;
        int   light_grn = 230;
        int   light_blu = 230;
        float light_bright = 0.70f;
        int   light_size = 120;
        float light_fade = 0.35f;

        public ObjBurningEffect(Map.Map map, int posX, int posY, int offsetX, int offsetY)
            : base(map, posX, posY, offsetX, offsetY, Values.LayerTop, "Particles/flame", "idle", deleteOnFinish: true)
        {
            // If a mod file exists load the values from it.
            string modFile = Path.Combine(Values.PathLAHDMods, "ObjBurningEffect.lahdmod");

            if (File.Exists(modFile))
                ModFile.Parse(modFile, this);

            System.Diagnostics.Debug.WriteLine(light_source);

            ConfigureLight(light_source, light_red, light_grn, light_blu, light_bright, light_size, light_fade);
        }
    }
}
