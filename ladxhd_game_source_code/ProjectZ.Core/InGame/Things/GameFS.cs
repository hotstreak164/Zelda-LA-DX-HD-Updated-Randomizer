using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System.Linq;
#if ANDROID
using Android.Content.Res;
#endif

namespace ProjectZ.InGame.Things
{
    internal static class GameFS
    {
        //-------------------------------------------------------------------------------------------------------------------------------------------------
        //
        //  FUNCTIONS FOR BITMAP FONTS
        //
        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static int LineSpacing => Game1.LanguageManager.CurrentLanguageCode == "chn"
            ? Resources.ChinaFont.LineHeight
            : Resources.GameFont.LineSpacing;

        public static void DrawString(SpriteBatch spriteBatch, string text, Vector2 position, Color color)
        {
            if (Game1.LanguageManager.CurrentLanguageCode == "chn")
                spriteBatch.DrawString(Resources.ChinaFont, text, position, color);
            else
                spriteBatch.DrawString(Resources.GameFont, text, position, color);
        }

        public static void DrawString(SpriteBatch spriteBatch, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            if (Game1.LanguageManager.CurrentLanguageCode == "chn")
                spriteBatch.DrawString(Resources.ChinaFont, text, position, color, rotation, origin, new Vector2(scale, scale), effects, layerDepth);
            else
                spriteBatch.DrawString(Resources.GameFont, text, position, color, rotation, origin, scale, effects, layerDepth);
        }

        public static Vector2 MeasureString(string text)
        {
            if (Game1.LanguageManager.CurrentLanguageCode == "chn")
                return Resources.ChinaFont.MeasureString(text);

            // The "catch" return doesn't matter. An exception can be thrown from font spillover
            // from Chinese language to anything else (happens in Photo Book Overlay for example).
            try    { return Resources.GameFont.MeasureString(text); }
            catch  { return new Vector2(0,0); }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        //
        //  PATH HELPERS
        //
        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static string BaseDir => AppContext.BaseDirectory;

        /// <summary>
        /// Normalize separators. On non-Windows platforms, convert '\' to '/'.
        /// Does not trim rooted paths into relative paths.
        /// </summary>
        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

        #if WINDOWS
            return path.Replace('/', '\\');
        #else
            return path.Replace('\\', '/');
        #endif
        }

        /// <summary>
        /// True when the path points at packaged game assets, not user files.
        /// Valid examples: "Data/...", "Content/..."
        /// </summary>
        private static bool IsPackagedAssetPath(string path)
        {
            path = NormalizePath(path).TrimStart('/', '\\');

        #if WINDOWS
            return path.StartsWith("Data\\", StringComparison.OrdinalIgnoreCase) ||
                   path.Equals("Data", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("Content\\", StringComparison.OrdinalIgnoreCase) ||
                   path.Equals("Content", StringComparison.OrdinalIgnoreCase);
        #else
            return path.StartsWith("Data/", StringComparison.OrdinalIgnoreCase) ||
                   path.Equals("Data", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("Content/", StringComparison.OrdinalIgnoreCase) ||
                   path.Equals("Content", StringComparison.OrdinalIgnoreCase);
        #endif
        }

        /// <summary>
        /// True when the path is an actual rooted filesystem path.
        /// Android mod folders and save folders should end up here.
        /// </summary>
        private static bool IsRealFileSystemPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            path = NormalizePath(path);

            if (IsPackagedAssetPath(path))
                return false;

            return Path.IsPathRooted(path);
        }

        /// <summary>
        /// Converts an absolute desktop path containing /Data/ or /Content/ into an asset-relative path.
        /// Leaves rooted user-data paths alone.
        /// </summary>
        public static string ToAssetPath(string path)
        {
            path = NormalizePath(path);

            if (string.IsNullOrEmpty(path))
                return path;

            if (IsRealFileSystemPath(path))
            {
            #if WINDOWS
                int dataIdx = path.IndexOf("\\Data\\", StringComparison.OrdinalIgnoreCase);
                if (dataIdx >= 0)
                    return path.Substring(dataIdx + 1);

                int contentIdx = path.IndexOf("\\Content\\", StringComparison.OrdinalIgnoreCase);
                if (contentIdx >= 0)
                    return path.Substring(contentIdx + 1);

                return path;
            #else
                int dataIdx = path.IndexOf("/Data/", StringComparison.OrdinalIgnoreCase);
                if (dataIdx >= 0)
                    return path.Substring(dataIdx + 1);

                int contentIdx = path.IndexOf("/Content/", StringComparison.OrdinalIgnoreCase);
                if (contentIdx >= 0)
                    return path.Substring(contentIdx + 1);

                return path;
            #endif
            }

            return path.TrimStart('/', '\\'); // ← was a dead if/else, both branches returned path
        }

        #if !ANDROID
        /// <summary>
        /// Converts a packaged asset path like "Data/..." or "Content/..." into an on-disk path rooted at BaseDirectory.
        /// Only use this for packaged asset paths on desktop.
        /// </summary>
        private static string ToDiskPath(string path)
        {
            path = ToAssetPath(path);

            if (IsRealFileSystemPath(path))
                return path;

            path = path.TrimStart('/', '\\');

        #if WINDOWS
            var parts = path.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
        #else
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        #endif
            return Path.GetFullPath(Path.Combine(new[] { BaseDir }.Concat(parts).ToArray()));
        }
        #endif

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        //
        //  STRING HELPERS
        //
        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static string ReadAllText(string path)
        {
            using var s = OpenRead(path);
            using var sr = new StreamReader(s, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            return sr.ReadToEnd();
        }

        public static string[] ReadAllLines(string path)
        {
            return ReadAllText(path)
                .Replace("\r\n", "\n")
                .Replace('\r', '\n')
                .Split('\n');
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        //
        //  CORE FILE I/O
        //
        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool Exists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            path = NormalizePath(path);

            if (IsRealFileSystemPath(path))
                return File.Exists(path);

            path = ToAssetPath(path);

        #if ANDROID
            try
            {
                using var _ = TitleContainer.OpenStream(path);
                return true;
            }
            catch
            {
                return false;
            }
        #else
            return File.Exists(ToDiskPath(path));
        #endif
        }

        public static Stream OpenRead(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));

            path = NormalizePath(path);

            if (IsRealFileSystemPath(path))
                return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            path = ToAssetPath(path);

        #if ANDROID
            return TitleContainer.OpenStream(path);
        #else
            return File.Open(ToDiskPath(path), FileMode.Open, FileAccess.Read, FileShare.Read);
        #endif
        }

        /// <summary>
        /// Opens either a packaged asset path or a real filesystem path.
        /// </summary>
        public static Stream OpenReadAny(string path)
        {
            return OpenRead(path);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        //
        //  DIRECTORY LISTING
        //
        //-------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns names only, not full paths.
        /// For packaged assets on Android, uses AssetManager.
        /// For real filesystem folders, uses Directory enumeration.
        /// </summary>
        public static string[] List(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir))
                return Array.Empty<string>();

            dir = NormalizePath(dir);

            if (IsRealFileSystemPath(dir))
            {
                if (!Directory.Exists(dir))
                    return Array.Empty<string>();

                return Directory.EnumerateFileSystemEntries(dir)
                    .Select(Path.GetFileName)
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToArray();
            }

            dir = ToAssetPath(dir);

        #if ANDROID
            try
            {
                AssetManager am = Android.App.Application.Context.Assets;
                return am.List(dir) ?? Array.Empty<string>();
            }
            catch
            {
                return Array.Empty<string>();
            }
        #else
            var diskDir = ToDiskPath(dir);

            if (!Directory.Exists(diskDir))
                return Array.Empty<string>();

            return Directory.EnumerateFileSystemEntries(diskDir)
                .Select(Path.GetFileName)
                .Where(n => !string.IsNullOrEmpty(n))
                .ToArray();
        #endif
        }

        public static bool IsDirectory(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir))
                return false;

            dir = NormalizePath(dir);

            if (IsRealFileSystemPath(dir))
                return Directory.Exists(dir);

            dir = ToAssetPath(dir);

        #if ANDROID
            // AssetManager.List() returns empty for both empty directories AND files,
            // so we can't rely on it alone. Instead, try to open the path as a stream:
            // files succeed, directories throw. Fall back to List() only if open fails,
            // because a failed open could also mean the path simply doesn't exist.
            AssetManager am = Android.App.Application.Context.Assets;

            try
            {
                // Opened as a file successfully → not a directory.
                using var stream = am.Open(dir);
                return false;
            }
            catch
            {
                // Could be a directory, or could be a missing path entirely.
                // List() returning non-null (even empty) confirms it's a known directory.
                var entries = am.List(dir);
                return entries != null;
            }
        #else
            return Directory.Exists(ToDiskPath(dir));
        #endif
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        //
        //  ENUMERATION
        //
        //-------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Enumerates files under a directory.
        /// Returned paths include the directory prefix.
        /// - acceptFile: receives filename only
        /// - skipDirectory: receives directory name only
        /// </summary>
        public static IEnumerable<string> EnumerateFiles(string dir, bool recursive, Func<string, bool> acceptFile, Func<string, bool> skipDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(dir))
                yield break;

            dir = NormalizePath(dir);

            if (IsRealFileSystemPath(dir))
            {
                if (!Directory.Exists(dir))
                    yield break;

                if (!recursive)
                {
                    foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly))
                    {
                        var name = Path.GetFileName(file);

                        if (acceptFile == null || acceptFile(name))
                            yield return NormalizePath(file);
                    }

                    yield break;
                }

                foreach (var file in EnumerateRealFilesRecursive(dir, acceptFile, skipDirectory))
                    yield return file;

                yield break;
            }

            dir = ToAssetPath(dir);

        #if ANDROID
            foreach (var file in EnumerateAssetFiles(dir, recursive, acceptFile, skipDirectory))
                yield return file;
        #else
            var diskDir = ToDiskPath(dir);

            if (!Directory.Exists(diskDir))
                yield break;

            if (!recursive)
            {
                foreach (var file in Directory.EnumerateFiles(diskDir, "*", SearchOption.TopDirectoryOnly))
                {
                    var name = Path.GetFileName(file);

                    if (acceptFile == null || acceptFile(name))
                        yield return NormalizePath(file);
                }

                yield break;
            }

            foreach (var file in EnumerateRealFilesRecursive(diskDir, acceptFile, skipDirectory))
                yield return file;
        #endif
        }

        private static IEnumerable<string> EnumerateRealFilesRecursive(string dir, Func<string, bool> acceptFile, Func<string, bool> skipDirectory, HashSet<string> visited = null)
        {
            visited ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string realDir = Path.GetFullPath(dir);
            if (!visited.Add(realDir))
                yield break;

            foreach (var entry in Directory.EnumerateFileSystemEntries(dir, "*", SearchOption.TopDirectoryOnly))
            {
                var name = Path.GetFileName(entry);
                var isDir = (File.GetAttributes(entry) & FileAttributes.Directory) != 0;

                if (isDir)
                {
                    if (skipDirectory != null && skipDirectory(name))
                        continue;

                    foreach (var sub in EnumerateRealFilesRecursive(entry, acceptFile, skipDirectory, visited))
                        yield return sub;

                    continue;
                }

                if (acceptFile == null || acceptFile(name))
                    yield return NormalizePath(entry);
            }
        }

    #if ANDROID
        private static IEnumerable<string> EnumerateAssetFiles(string dir, bool recursive, Func<string, bool> acceptFile, Func<string, bool> skipDirectory)
        {
            foreach (var entry in List(dir))
            {
                var full = string.IsNullOrEmpty(dir) ? entry : $"{dir}/{entry}";
                full = NormalizePath(full);

                if (IsDirectory(full))
                {
                    if (!recursive)
                        continue;

                    if (skipDirectory != null && skipDirectory(entry))
                        continue;

                    foreach (var sub in EnumerateAssetFiles(full, true, acceptFile, skipDirectory))
                        yield return sub;

                    continue;
                }

                if (acceptFile == null || acceptFile(entry))
                    yield return full;
            }
        }
    #endif

        /// <summary>
        /// Enumerates directories under a directory.
        /// Returned paths include the directory prefix.
        /// - acceptDirectory: receives directory name only
        /// - skipDirectory: receives directory name only
        /// </summary>
        public static IEnumerable<string> EnumerateDirectories(string dir, bool recursive, Func<string, bool> acceptDirectory = null, Func<string, bool> skipDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(dir))
                yield break;

            dir = NormalizePath(dir);

            if (IsRealFileSystemPath(dir))
            {
                if (!Directory.Exists(dir))
                    yield break;

                if (!recursive)
                {
                    foreach (var subDir in Directory.EnumerateDirectories(dir, "*", SearchOption.TopDirectoryOnly))
                    {
                        var name = Path.GetFileName(subDir);

                        if (skipDirectory != null && skipDirectory(name))
                            continue;

                        if (acceptDirectory == null || acceptDirectory(name))
                            yield return NormalizePath(subDir);
                    }

                    yield break;
                }

                foreach (var subDir in EnumerateRealDirectoriesRecursive(dir, acceptDirectory, skipDirectory))
                    yield return subDir;

                yield break;
            }

            dir = ToAssetPath(dir);

        #if ANDROID
            foreach (var subDir in EnumerateAssetDirectories(dir, recursive, acceptDirectory, skipDirectory))
                yield return subDir;
        #else
            var diskDir = ToDiskPath(dir);

            if (!Directory.Exists(diskDir))
                yield break;

            if (!recursive)
            {
                foreach (var subDir in Directory.EnumerateDirectories(diskDir, "*", SearchOption.TopDirectoryOnly))
                {
                    var name = Path.GetFileName(subDir);

                    if (skipDirectory != null && skipDirectory(name))
                        continue;

                    if (acceptDirectory == null || acceptDirectory(name))
                        yield return NormalizePath(subDir);
                }

                yield break;
            }

            foreach (var subDir in EnumerateRealDirectoriesRecursive(diskDir, acceptDirectory, skipDirectory))
                yield return subDir;
        #endif
        }

        private static IEnumerable<string> EnumerateRealDirectoriesRecursive(string dir, Func<string, bool> acceptDirectory, Func<string, bool> skipDirectory, HashSet<string> visited = null)
        {
            visited ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Resolve the real path to catch symlinks pointing to already visited dirs
            string realDir = Path.GetFullPath(dir);
            if (!visited.Add(realDir))
                yield break;

            foreach (var subDir in Directory.EnumerateDirectories(dir, "*", SearchOption.TopDirectoryOnly))
            {
                var name = Path.GetFileName(subDir);

                if (skipDirectory != null && skipDirectory(name))
                    continue;

                if (acceptDirectory == null || acceptDirectory(name))
                    yield return NormalizePath(subDir);

                foreach (var sub in EnumerateRealDirectoriesRecursive(subDir, acceptDirectory, skipDirectory, visited))
                    yield return sub;
            }
        }

        private static IEnumerable<string> EnumerateAssetDirectories(string dir, bool recursive, Func<string, bool> acceptDirectory, Func<string, bool> skipDirectory)
        {
            foreach (var entry in List(dir))
            {
                var full = string.IsNullOrEmpty(dir) ? entry : $"{dir}/{entry}";
                full = NormalizePath(full);

                if (!IsDirectory(full))
                    continue;

                if (skipDirectory != null && skipDirectory(entry))
                    continue;

                if (acceptDirectory == null || acceptDirectory(entry))
                    yield return full;

                if (recursive)
                {
                    foreach (var sub in EnumerateAssetDirectories(full, true, acceptDirectory, skipDirectory))
                        yield return sub;
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        //
        //  BYTE HELPERS
        //
        //-------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Reads all bytes from a stream safely on all platforms.
        /// Does not assume stream.Length is supported.
        /// </summary>
        public static byte[] ReadAllBytes(Stream stream)
        {
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        /// <summary>
        /// Opens a file or asset and reads all bytes.
        /// </summary>
        public static byte[] ReadAllBytes(string path)
        {
            using var stream = OpenRead(path);
            return ReadAllBytes(stream);
        }
    }
}