using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace LADXHD_Patcher
{
    internal class XDelta3
    {
        private static string Exe;
        private static string Args;

        private static Dictionary<string, object> resources = ResourceHelper.GetAllResources();

        public enum Operation { Create, Apply }

        public static void Initialize()
        {
            XDelta3.Exe = Path.Combine(Config.TempFolder, "xdelta3.exe");
        }

        public static string GetCreateArguments(string OldFile, string NewFile, string PatchFile)
        {
		    string args = string.Empty;
		    args = string.Concat(new string[]
		    {
			    args,
			    " -f -s \"",
			    OldFile,
			    "\" \"",
			    NewFile,
			    "\" \"",
			    PatchFile,
			    "\""
		    });
            return args;
        }

        public static string GetApplyArguments(string OldFile, string PatchFile, string NewFile)
        {
		    string args = string.Empty;
		    args = string.Concat(new string[]
		    {
			    args,
			    " -d -f -s \"",
			    OldFile,
			    "\" \"",
			    PatchFile,
			    "\" \"",
			    NewFile,
			    "\""
		    });
            return args;
        }

        private static void Start()
        {
            Process xDelta = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo 
            {
                WorkingDirectory = Config.BaseFolder,
                FileName = XDelta3.Exe,
                Arguments = XDelta3.Args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Normal
            };
            xDelta.StartInfo = startInfo;
            xDelta.Start();
            xDelta.WaitForExit();
            xDelta.Dispose();
        }

        public static void Execute(Operation action, string input, string diff, string output, string target = "")
        {
            if (action == Operation.Apply)
                XDelta3.Args = XDelta3.GetApplyArguments(input, diff, output);
            else if (action == Operation.Create)
                XDelta3.Args = XDelta3.GetCreateArguments(input, diff, output);
            XDelta3.Start();

            if (!string.IsNullOrEmpty(target))
                output.MovePath(target, true);
        }

        public static void Create()
        {
            File.WriteAllBytes(XDelta3.Exe, (byte[])resources["xdelta3.exe"]);
        }

        public static void Remove()
        {
            XDelta3.Exe.RemovePath();
        }
    }
}
