using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static LADXHD_Patcher.Config;

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
                Console.WriteLine("  --silent, -s              Run in silent mode (no GUI, for automated installs)");
                Console.WriteLine("  --platform <value>        Target platform (default: windows)");
                Console.WriteLine("                            Values: windows, android, linux-x86, linux-arm64,");
                Console.WriteLine("                                    macos-x86, macos-arm64");
                Console.WriteLine("  --graphics <value>        Target graphics API");
                Console.WriteLine("                            Default: directx (windows), opengl (all others)");
                Console.WriteLine("                            Values: directx, opengl");
                Console.WriteLine("  --help, -h                Show this help message");
                Console.WriteLine();
                Console.WriteLine("Exit codes:");
                Console.WriteLine("  0  Success");
                Console.WriteLine("  1  Game executable not found");
                Console.WriteLine("  2  Patching failed");
                Console.WriteLine("  3  Invalid arguments");
                Console.WriteLine();
                FreeConsole();
                return 0;
            }

            // Initialize the classes.
            Config.Initialize();
            XDelta3.Initialize();

            if (silentMode)
            {
                if (!TryParseTargetArgs(args, out Platform platform, out GraphicsAPI graphics))
                {
                    FreeConsole();
                    return 3;
                }
                Config.SelectedPlatform = platform;
                Config.SelectedGraphics = graphics;

                int result = Functions.StartPatchingSilent();
                FreeConsole();
                return result;
            }
            else
            {
                // Initialize forms and show the main dialog
                // --platform and --graphics flags are ignored in GUI mode
                Forms.Initialize();
                Forms.MainDialog.ShowDialog();
                return 0;
            }
        }

        /// <summary>
        /// Parses and validates the --platform and --graphics arguments.
        /// Returns true on success and sets the out parameters to the resolved values.
        /// Returns false if an argument is invalid; an error message is already printed.
        /// </summary>
        private static bool TryParseTargetArgs(string[] args, out Platform platform, out GraphicsAPI graphics)
        {
            platform = Platform.Windows;
            graphics = GraphicsAPI.DirectX;

            // Parse --platform
            bool platformParseError = false;
            Platform? platformArg = ParsePlatformArg(args, out platformParseError);
            if (platformParseError)
            {
                Console.WriteLine("ERROR: Invalid --platform value. Valid values: windows, android, linux-x86, linux-arm64, macos-x86, macos-arm64");
                return false;
            }

            // Parse --graphics
            bool graphicsParseError = false;
            GraphicsAPI? graphicsArg = ParseGraphicsArg(args, out graphicsParseError);
            if (graphicsParseError)
            {
                Console.WriteLine("ERROR: Invalid --graphics value. Valid values: directx, opengl");
                return false;
            }

            // Apply platform (default: Windows)
            platform = platformArg ?? Platform.Windows;

            // Apply graphics: default is DirectX for Windows, OpenGL for all other platforms
            if (graphicsArg.HasValue)
            {
                // Validate: DirectX is only supported on Windows
                if (graphicsArg.Value == GraphicsAPI.DirectX && platform != Platform.Windows)
                {
                    Console.WriteLine("ERROR: --graphics directx is only supported on Windows. Use --graphics opengl for other platforms.");
                    return false;
                }
                graphics = graphicsArg.Value;
            }
            else
            {
                // Default graphics based on platform (matches GUI initial state)
                graphics = (platform == Platform.Windows) ? GraphicsAPI.DirectX : GraphicsAPI.OpenGL;
            }

            return true;
        }

        /// <summary>
        /// Parses the --platform argument from the command-line args array.
        /// Supports both "--platform value" and "--platform=value" formats.
        /// Returns null if the flag is not present. Sets parseError to true if
        /// the flag is present but the value is missing or unrecognised.
        /// </summary>
        private static Platform? ParsePlatformArg(string[] args, out bool parseError)
        {
            parseError = false;

            for (int i = 0; i < args.Length; i++)
            {
                string value = null;

                // --platform=value
                if (args[i].StartsWith("--platform=", StringComparison.OrdinalIgnoreCase))
                {
                    value = args[i].Substring("--platform=".Length);
                }
                // --platform value
                else if (args[i].Equals("--platform", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < args.Length)
                        value = args[i + 1];
                }

                if (value != null)
                {
                    switch (value.ToLowerInvariant())
                    {
                        case "windows":    return Platform.Windows;
                        case "android":    return Platform.Android;
                        case "linux-x86":  return Platform.Linux_x86;
                        case "linux-arm64":return Platform.Linux_Arm64;
                        case "macos-x86":  return Platform.MacOS_x86;
                        case "macos-arm64":return Platform.MacOS_Arm64;
                        default:
                            parseError = true;
                            return null;
                    }
                }
            }

            return null; // flag not present — caller uses default
        }

        /// <summary>
        /// Parses the --graphics argument from the command-line args array.
        /// Supports both "--graphics value" and "--graphics=value" formats.
        /// Returns null if the flag is not present. Sets parseError to true if
        /// the flag is present but the value is missing or unrecognised.
        /// </summary>
        private static GraphicsAPI? ParseGraphicsArg(string[] args, out bool parseError)
        {
            parseError = false;

            for (int i = 0; i < args.Length; i++)
            {
                string value = null;

                // --graphics=value
                if (args[i].StartsWith("--graphics=", StringComparison.OrdinalIgnoreCase))
                {
                    value = args[i].Substring("--graphics=".Length);
                }
                // --graphics value
                else if (args[i].Equals("--graphics", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < args.Length)
                        value = args[i + 1];
                }

                if (value != null)
                {
                    switch (value.ToLowerInvariant())
                    {
                        case "directx": return GraphicsAPI.DirectX;
                        case "opengl":  return GraphicsAPI.OpenGL;
                        default:
                            parseError = true;
                            return null;
                    }
                }
            }

            return null; // flag not present — caller uses default
        }
    }
}
