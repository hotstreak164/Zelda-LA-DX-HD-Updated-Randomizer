using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.SaveLoad
{
    public static class SpriteAtlasSerialization
    {
        public sealed class SpriteAtlas
        {
            public int Scale = 1;
            public List<AtlasEntry> Data { get; } = new();
        }

        public sealed class AtlasEntry
        {
            public string EntryId = "";
            public Rectangle SourceRectangle;
            public Vector2 Origin;

            public override string ToString() => EntryId;
        }

        // ----------------------------------------------------------------------------------------------------
        //  SAVE (desktop/editor only)
        // ----------------------------------------------------------------------------------------------------
        public static void SaveSpriteAtlas(string filePath, SpriteAtlas spriteAtlas)
        {
        #if ANDROID
            // APK assets are read-only; saving is editor/desktop only.
            throw new NotSupportedException("SaveSpriteAtlas is not supported on Android.");
        #else
            if (spriteAtlas == null) throw new ArgumentNullException(nameof(spriteAtlas));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("Invalid path.", nameof(filePath));

            using var writer = new StreamWriter(filePath);

            writer.WriteLine("1");                // version
            writer.WriteLine(spriteAtlas.Scale);  // scale

            int scale = spriteAtlas.Scale <= 0 ? 1 : spriteAtlas.Scale;

            // Store unscaled rect/origin (editor convenience).
            for (int i = 0; i < spriteAtlas.Data.Count; i++)
            {
                var e = spriteAtlas.Data[i];
                var r = e.SourceRectangle;
                var o = e.Origin;

                writer.WriteLine(
                    $"{e.EntryId}:" +
                    $"{r.X / scale}," +
                    $"{r.Y / scale}," +
                    $"{r.Width / scale}," +
                    $"{r.Height / scale}," +
                    $"{o.X / scale}," +
                    $"{o.Y / scale}"
                );
            }
        #endif
        }

        // ----------------------------------------------------------------------------------------------------
        //  LOAD
        // ----------------------------------------------------------------------------------------------------
        public static bool LoadSpriteAtlas(string filePath, SpriteAtlas spriteAtlas, bool clearExisting = true)
        {
            if (spriteAtlas == null) throw new ArgumentNullException(nameof(spriteAtlas));
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            filePath = GameFS.NormalizePath(filePath);

            if (!GameFS.Exists(filePath))
                return false;

            if (clearExisting)
                spriteAtlas.Data.Clear();

            using var stream = GameFS.OpenRead(filePath);
            using var reader = new StreamReader(stream);

            _ = reader.ReadLine();

            var scaleLine = reader.ReadLine();
            if (!int.TryParse(scaleLine, NumberStyles.Integer, CultureInfo.InvariantCulture, out var scale) || scale <= 0)
                scale = 1;

            spriteAtlas.Scale = scale;

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                int colon = line.IndexOf(':');
                if (colon <= 0 || colon >= line.Length - 1)
                    continue;

                var id = line.Substring(0, colon);
                var data = line.Substring(colon + 1);

                var parts = data.Split(',');
                if (parts.Length < 4)
                    continue;

                if (!TryInt(parts, 0, out int x) || !TryInt(parts, 1, out int y) || !TryInt(parts, 2, out int w) || !TryInt(parts, 3, out int h))
                    continue;

                float ox = 0, oy = 0;
                if (parts.Length >= 6)
                {
                    TryFloat(parts, 4, out ox);
                    TryFloat(parts, 5, out oy);
                }
                spriteAtlas.Data.Add(new AtlasEntry
                {
                    EntryId = id,
                    SourceRectangle = new Rectangle(x, y, w, h),
                    Origin = new Vector2(ox, oy)
                });
            }
            return true;
        }

        // ----------------------------------------------------------------------------------------------------
        //  DICTIONARY POPULATION
        // ----------------------------------------------------------------------------------------------------
        public static void LoadSourceDictionary(Texture2D texture, string fileName, Dictionary<string, DictAtlasEntry> dictionary)
        {
            if (texture == null) throw new ArgumentNullException(nameof(texture));
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

            var spriteAtlas = new SpriteAtlas();
            if (!LoadSpriteAtlas(fileName, spriteAtlas))
                return;

            for (int i = 0; i < spriteAtlas.Data.Count; i++)
            {
                var e = spriteAtlas.Data[i];
                var dictEntry = new DictAtlasEntry(texture, e.SourceRectangle, e.Origin, spriteAtlas.Scale);
                dictionary[e.EntryId] = dictEntry;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        //  PARSE HELPERS
        // ----------------------------------------------------------------------------------------------------
        private static bool TryInt(string[] parts, int index, out int value)
        {
            value = 0;
            if (index >= parts.Length) return false;
            return int.TryParse(parts[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryFloat(string[] parts, int index, out float value)
        {
            value = 0;
            if (index >= parts.Length) return false;

            // Accept int-like strings too.
            if (float.TryParse(parts[index], NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return true;

            // Fallback: sometimes files have stray whitespace
            return float.TryParse(parts[index].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }
    }
}