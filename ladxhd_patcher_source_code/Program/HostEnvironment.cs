using System;
using System.IO;
using System.Runtime.InteropServices;

namespace LADXHD_Patcher
{
    // Detects the actual host operating system, including when running under Wine.
    // All fields are evaluated once at class initialisation time and are safe on all platforms.
    internal static class HostEnvironment
    {
        // wine_get_version is only exported by Wine's ntdll.dll, not by real Windows ntdll.dll.
        // On native Windows this P/Invoke call will throw EntryPointNotFoundException — that is
        // the expected, safe outcome. The catch in DetectWine() handles it explicitly.
        [DllImport("ntdll.dll", EntryPoint = "wine_get_version",
                   CharSet = CharSet.Ansi, ExactSpelling = true)]
        private static extern string WineGetVersion();

        private static bool DetectWine()
        {
            // EntryPointNotFoundException is the expected outcome on native Windows — not a bug.
            try { WineGetVersion(); return true; }
            catch { return false; }
        }

        // True when the process is running inside Wine.
        public static readonly bool IsWine = DetectWine();

        // True when the host OS is Linux (works both natively and under Wine).
        public static readonly bool IsLinux = File.Exists("/proc/version");

        // True when the host OS is macOS (works both natively and under Wine).
        public static readonly bool IsMacOS = File.Exists("/System/Library/CoreServices/SystemVersion.plist");

        // True when the host OS is Windows (native, not Wine).
        public static readonly bool IsWindows = !IsLinux && !IsMacOS;
    }
}
