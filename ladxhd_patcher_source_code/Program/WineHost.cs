using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace LADXHD_Patcher
{
    // Detects the actual host operating system, including when running under Wine,
    // and provides Wine/host utility helpers shared across the patcher.
    // All detection fields are evaluated once at class initialisation time and are safe on all platforms.
    internal static class WineHost
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

        // Direct DllImport for wine_get_host_version from ntdll.dll (Wine-private export, CDECL).
        // On native Windows this export does not exist, so the call will throw
        // EntryPointNotFoundException — caught safely in GetUnameSystemName().
        // Parameters are const char** (pointer-to-pointer): out IntPtr is the correct C# mapping.
        [DllImport("ntdll.dll", EntryPoint = "wine_get_host_version",
                   CallingConvention = CallingConvention.Cdecl,
                   CharSet = CharSet.Ansi, ExactSpelling = true)]
        private static extern void WineGetHostVersion(out IntPtr sysname, out IntPtr release);

        // Retrieves the host OS name ("Darwin", "Linux", etc.) via wine_get_host_version.
        private static string GetUnameSystemName()
        {
            try
            {
                WineGetHostVersion(out IntPtr sysname, out IntPtr release);
                return Marshal.PtrToStringAnsi(sysname);
            }
            catch
            {
                return null;
            }
        }

        // True when the process is running inside Wine.
        public static readonly bool IsWine = DetectWine();

        private static readonly string _uname = GetUnameSystemName();

        // True when the host OS is Linux (works both natively and under Wine/CrossOver).
        public static readonly bool IsLinux = _uname == "Linux";

        // True when the host OS is macOS (works both natively and under Wine/CrossOver).
        public static readonly bool IsMacOS = _uname == "Darwin";

        // Converts a Wine Windows path (e.g. "Z:\Users\foo\bar") to a native Unix path ("/Users/foo/bar").
        // Strips the leading drive letter and colon (always a single letter under Wine/Windows) via regex.
        internal static string ToUnixPath(string windowsPath)
        {
            return Regex.Replace(windowsPath, @"^[A-Za-z]:", "").Replace('\\', '/');
        }
    }
}
