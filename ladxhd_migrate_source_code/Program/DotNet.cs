using System;
using System.Diagnostics;

namespace LADXHD_Migrater
{
    internal class DotNet
    {
        public static bool BuildGame()
        {
            if (!Config.Game_Source.TestPath()) return false;

            try
            {
                using (Process dotnet = new Process())
                {
                    dotnet.StartInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = Config.Game_Source,
                        FileName = "dotnet",
                        Arguments = "publish -c Release -p:\"PublishProfile=FolderProfile\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    dotnet.Start();

                    string output = dotnet.StandardOutput.ReadToEnd();
                    string error = dotnet.StandardError.ReadToEnd();

                    dotnet.WaitForExit();

                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        Forms.okayDialog.Display("Build Error", 250, 40, 27, 9, 15, error);
                    }
                }
            }
            catch (Exception ex)
            {
                Forms.okayDialog.Display("Exception Caught", 250, 40, 27, 9, 15, "Exception: " + ex.Message);
            }
            string GameExePath = Config.Publish_Path + "\\Link's Awakening DX HD.exe";
            return GameExePath.TestPath();
        }
    }
}
