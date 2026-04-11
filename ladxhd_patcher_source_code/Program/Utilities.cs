using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using static LADXHD_Patcher.Config;

namespace LADXHD_Patcher
{
    public class Utilities
    {
        private static Dictionary<string, object> resources = ResourceHelper.GetAllResources();

        public static void ExtractResourcesZip(string zipName, string destination)
        {
            // The zip file is always written to the temp folder.
            string zipFilePath = Path.Combine(Config.TempFolder, zipName);

            // Write the zip file from resources, extract to destination, and remove the zip file.
            File.WriteAllBytes(zipFilePath, (byte[])resources[zipName]);
            ZipFile.ExtractToDirectory(zipFilePath, destination);
            zipFilePath.RemovePath();
        }

        public static void RunProcess(string fileName, string workingDir, string args)
        {
            // Create the process start info.
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Start a process, report any exceptions, and close out the process.
            using (var proc = new Process { StartInfo = startInfo })
            {
                proc.Start();

                string output = null, errors = null;
                var outTask = Task.Run(() => output = proc.StandardOutput.ReadToEnd());
                var errTask = Task.Run(() => errors = proc.StandardError.ReadToEnd());

                proc.WaitForExit();
                Task.WaitAll(outTask, errTask);

                if (proc.ExitCode != 0)
                    throw new Exception(startInfo.FileName + ":\nOUTPUT:\n" + output + "\nERRORS:\n" + errors);

                proc.Dispose();
            }
        }
    }
}