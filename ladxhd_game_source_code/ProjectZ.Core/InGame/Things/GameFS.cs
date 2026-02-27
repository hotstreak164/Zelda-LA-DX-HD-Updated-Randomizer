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
            return Resources.GameFont.MeasureString(text);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        //
        //  STRING HELPERS
        //
        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static string ReadAllText(string path)
        {
            path = ToAssetPath(path);
            using var s = OpenRead(path);
            using var sr = new StreamReader(s, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            return sr.ReadToEnd();
        }

        public static string[] ReadAllLines(string path)
        {
            return ReadAllText(path).Replace("\r\n", "\n").Split('\n');
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        //
        //  PATH HELPERS
        //
        //-------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>Normalize separators and remove leading/trailing slashes.</summary>
        public static string NormalizePath(string path)
        {
        #if ANDROID
            return (path ?? "").Replace("\\", "/").Trim('/');
        #endif
            return path;
        }

        /// <summary>
        /// Converts absolute desktop paths into asset-relative paths like "Data/..." or "Content/...".
        /// Safe on all platforms.
        /// </summary>
        public static string ToAssetPath(string path)
        {
            path = NormalizePath(path);

            if (string.IsNullOrEmpty(path))
                return path;

            // Already relative
            if (path.StartsWith("Data/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("Content/", StringComparison.OrdinalIgnoreCase))
                return path;

            // Trim absolute paths down to asset roots
            int dataIdx = path.IndexOf("/Data/", StringComparison.OrdinalIgnoreCase);
            if (dataIdx >= 0)
                return path.Substring(dataIdx + 1);

            int contentIdx = path.IndexOf("/Content/", StringComparison.OrdinalIgnoreCase);
            if (contentIdx >= 0)
                return path.Substring(contentIdx + 1);

            return path.TrimStart('/');
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        //
        //  CORE FILE I/O
        //
        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool Exists(string path)
        {
            path = ToAssetPath(path);

        #if ANDROID
            try { using var _ = TitleContainer.OpenStream(path); return true; }
            catch { return false; }
        #else
            return File.Exists(path);
        #endif
        }

        public static Stream OpenRead(string path)
        {
            path = ToAssetPath(path);

        #if ANDROID
            return TitleContainer.OpenStream(path);
        #else
            return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        #endif
        }

        public static Stream OpenReadAny(string path)
        {
        #if ANDROID
            // Normalize and try to map to an asset path if possible
            var ap = ToAssetPath(path);

            // If it looks like a packaged asset, load from TitleContainer (APK assets on Android)
            if (ap.StartsWith("Data/", StringComparison.OrdinalIgnoreCase) ||
                ap.StartsWith("Content/", StringComparison.OrdinalIgnoreCase))
            {
                return OpenRead(ap); // your existing OpenRead already uses TitleContainer on Android / File on desktop
            }
        #endif
            // Otherwise treat as a real filesystem path (mods/user data)
            return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        //
        //  DIRECTORY LISTING (returns names only, like AssetManager.List)
        //
        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static string[] List(string dir)
        {
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
            if (!Directory.Exists(dir))
                return Array.Empty<string>();

            var entries = Directory.GetFileSystemEntries(dir);
            for (int i = 0; i < entries.Length; i++)
                entries[i] = Path.GetFileName(entries[i]);

            return entries;
        #endif
        }

        public static bool IsDirectory(string dir)
        {
            dir = ToAssetPath(dir);

        #if ANDROID
            // Asset dirs are “virtual”. Listing is the best probe.
            return List(dir).Length > 0;
        #else
            return Directory.Exists(dir);
        #endif
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        //
        //  ENUMERATION
        //
        //-------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Enumerates files under a directory. Returned paths are normalized and include the directory prefix.
        /// - acceptFile: receives the entry filename (no path)
        /// - skipDirectory: receives the directory name (no path) and returns true to skip it
        /// </summary>
        public static IEnumerable<string> EnumerateFiles(
            string dir,
            bool recursive,
            Func<string, bool> acceptFile,
            Func<string, bool>? skipDirectory = null)
        {
            dir = ToAssetPath(dir);

        #if ANDROID
            foreach (var entry in List(dir))
            {
                var full = $"{dir}/{entry}";

                if (IsDirectory(full))
                {
                    if (!recursive)
                        continue;

                    if (skipDirectory != null && skipDirectory(entry))
                        continue;

                    foreach (var sub in EnumerateFiles(full, true, acceptFile, skipDirectory))
                        yield return sub;

                    continue;
                }

                if (acceptFile(entry))
                    yield return full;
            }
        #else
            if (!Directory.Exists(dir))
                yield break;

            var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            foreach (var full in Directory.EnumerateFileSystemEntries(dir, "*", option))
            {
                var name = Path.GetFileName(full);

                if (Directory.Exists(full))
                {
                    if (skipDirectory != null && skipDirectory(name))
                        continue;

                    continue; // directories themselves aren't returned
                }

                if (acceptFile(name))
                    yield return NormalizePath(full);
            }
        #endif
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
        /// Opens a file (desktop or APK asset) and reads all bytes.
        /// </summary>
        public static byte[] ReadAllBytes(string path)
        {
            path = ToAssetPath(path);
            using var stream = OpenRead(path);
            return ReadAllBytes(stream);
        }
    }
}