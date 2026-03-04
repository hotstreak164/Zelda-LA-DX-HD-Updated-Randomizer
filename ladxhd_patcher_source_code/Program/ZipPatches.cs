using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using static LADXHD_Patcher.Config;

namespace LADXHD_Patcher
{
    public class ZipPatches
    {
        private static Dictionary<string, object> resources = ResourceHelper.GetAllResources();

        public static void ExtractPatches()
        {
            string zipName = "patches_dx.zip";

            if (Config.SelectedGraphics == GraphicsAPI.OpenGL)
                zipName = "patches_gl.zip";

            // Set the patches and zipfile paths.
            string patchesPath = Path.Combine(Config.TempFolder, "patches").CreatePath();
            string patchedPath = Path.Combine(Config.TempFolder, "patchedFiles").CreatePath();
            string zipFilePath = Path.Combine(Config.TempFolder, zipName);

            // Write the zipfile, extract it, then delete it.
            File.WriteAllBytes(zipFilePath, (byte[])resources[zipName]);
            ZipFile.ExtractToDirectory(zipFilePath, patchesPath);
            zipFilePath.RemovePath();
        }
    }
}
