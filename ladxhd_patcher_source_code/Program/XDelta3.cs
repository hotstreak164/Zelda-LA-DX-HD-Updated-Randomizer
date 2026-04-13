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

        private static string EscapeArg(string arg)
        {
            return "\"" + arg.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        private static string BuildArguments(IEnumerable<string> args)
        {
            return string.Join(" ", System.Linq.Enumerable.Select(args, EscapeArg));
        }

        private static void Start(IEnumerable<string> args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WorkingDirectory      = Config.BaseFolder,
                FileName              = XDelta3.Exe,
                Arguments             = BuildArguments(args),
                UseShellExecute       = false,
                CreateNoWindow        = true,
                RedirectStandardError = true,
                WindowStyle           = ProcessWindowStyle.Normal
            };

            Process xDelta = new Process();
            xDelta.StartInfo = startInfo;
            try
            {
                xDelta.Start();
                xDelta.WaitForExit();
            }
            finally
            {
                xDelta.Dispose();
            }
        }

        public static void Execute(Operation action, string input, string diff, string output, string target = "")
        {
            List<string> args = action == Operation.Apply
                ? new List<string> { "-d", "-f", "-s", input, diff, output }
                : new List<string> { "-f", "-s", input, diff, output };

            XDelta3.Start(args);

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
