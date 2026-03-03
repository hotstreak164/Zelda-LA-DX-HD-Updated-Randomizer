using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LADXHD_Patcher
{
    internal static class Initialization
    {
        // Win32 API for console attachment
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);
        
        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        private const int ATTACH_PARENT_PROCESS = -1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Check for silent mode arguments
            bool silentMode = args.Any(arg => 
                arg.Equals("--silent", StringComparison.OrdinalIgnoreCase) ||
                arg.Equals("-s", StringComparison.OrdinalIgnoreCase) ||
                arg.Equals("/silent", StringComparison.OrdinalIgnoreCase) ||
                arg.Equals("/s", StringComparison.OrdinalIgnoreCase));

            // Check for help argument
            bool showHelp = args.Any(arg =>
                arg.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
                arg.Equals("-h", StringComparison.OrdinalIgnoreCase) ||
                arg.Equals("/?", StringComparison.OrdinalIgnoreCase));

            // Attach to parent console for command-line output
            if (silentMode || showHelp)
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
            }

            if (showHelp)
            {
                Console.WriteLine();
                Console.WriteLine("LADXHD Patcher v" + Config.Version);
                Console.WriteLine();
                Console.WriteLine("Usage: LADXHD.Patcher.exe [options]");
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine("  --silent, -s    Run in silent mode (no GUI, for automated installs)");
                Console.WriteLine("  --help, -h      Show this help message");
                Console.WriteLine();
                Console.WriteLine("Exit codes:");
                Console.WriteLine("  0  Success");
                Console.WriteLine("  1  Game executable not found");
                Console.WriteLine("  2  Patching failed");
                Console.WriteLine();
                FreeConsole();
                return 0;
            }

            // Initialize the classes.
            Config.Initialize();
            XDelta3.Initialize();

            if (silentMode)
            {
                // Run in silent mode without GUI
                int result = Functions.StartPatchingSilent();
                FreeConsole();
                return result;
            }
            else
            {
                // Initialize forms and show the main dialog
                Forms.Initialize();
                Forms.MainDialog.ShowDialog();
                return 0;
            }
        }
    }
}
