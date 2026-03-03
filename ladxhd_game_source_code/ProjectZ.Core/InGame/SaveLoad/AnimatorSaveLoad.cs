using System;
using System.IO;
using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.SaveLoad
{
    public class AnimatorSaveLoad
    {
        private static string curVersion = "1";

        public static void SaveAnimator(string path, Animator animator)
        {
        #if ANDROID
            // Writing into APK assets isn't a thing; saving is editor/desktop only.
            throw new NotSupportedException("Saving animations is not supported on Android.");
        #else
            if (animator == null) throw new ArgumentNullException(nameof(animator));

            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Invalid path.", nameof(path));

            var tempPath = path + ".temp";

            // Make sure the temp file is disposed even if something goes wrong.
            using (var writer = new StreamWriter(tempPath))
            {
                writer.WriteLine(curVersion);
                writer.WriteLine(animator.SpritePath ?? "");

                for (int i = 0; i < animator.Animations.Count; i++)
                {
                    var anim = animator.Animations[i];

                    var line =
                        anim.Id + ";" +
                        anim.NextAnimation + ";" +
                        anim.LoopCount + ";" +
                        anim.Offset.X + ";" +
                        anim.Offset.Y + ";" +
                        anim.Frames.Length;

                    for (int j = 0; j < anim.Frames.Length; j++)
                    {
                        var f = anim.Frames[j];
                        line += ";" +
                                f.FrameTime + ";" +
                                f.SourceRectangle.X + ";" +
                                f.SourceRectangle.Y + ";" +
                                f.SourceRectangle.Width + ";" +
                                f.SourceRectangle.Height + ";" +
                                f.Offset.X + ";" +
                                f.Offset.Y + ";" +
                                f.CollisionRectangle.X + ";" +
                                f.CollisionRectangle.Y + ";" +
                                f.CollisionRectangle.Width + ";" +
                                f.CollisionRectangle.Height + ";" +
                                f.MirroredV + ";" +
                                f.MirroredH;
                    }

                    writer.WriteLine(line);
                }
            }
            if (File.Exists(path))
                File.Delete(path);

            File.Move(tempPath, path);
        #endif
        }

        public static Animator LoadAnimator(string animatorId, bool redux = false)
        {
            return LoadAnimatorFile(Path.Combine(Values.PathAnimationFolder, animatorId + ".ani"), redux);
        }

        private static string AddReduxToFilename(string spritePath)
        {
            // Safe for names with multiple dots: "foo.bar.png" -> "foo.bar_redux.png"
            var dot = spritePath.LastIndexOf('.');
            return dot > 0
                ? spritePath.Substring(0, dot) + "_redux" + spritePath.Substring(dot)
                : spritePath + "_redux";
        }

        public static Animator LoadAnimatorFile(string filePath, bool redux = false)
        {
            if (!GameFS.Exists(GameFS.ToAssetPath(filePath)))
                return null;

            using var stream = GameFS.OpenRead(GameFS.ToAssetPath(filePath));
            using var reader = new StreamReader(stream);

            var animator = new Animator();
            var version = reader.ReadLine();       // unused
            var spritePath = reader.ReadLine();    // required

            if (string.IsNullOrWhiteSpace(spritePath))
                return null;

            // If uncensored is enabled, pull from the "_redux" version of the sprite sheet.
            if (redux)
                spritePath = AddReduxToFilename(spritePath);

            animator.SpritePath = spritePath;
            animator.SprTexture = Resources.GetTexture(animator.SpritePath);

            // If the texture couldn't be found/loaded, fail fast.
            if (animator.SprTexture == null)
                return null;

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var s = line.Split(';');
                if (s.Length < 16)
                    continue;

                int pos = 0;

                var animationId = (s[pos] ?? "").ToLowerInvariant();
                if (string.IsNullOrEmpty(animationId))
                    continue;

                var animation = new Animation(animationId)
                {
                    NextAnimation = (s[++pos] ?? "").ToLowerInvariant(),
                    LoopCount     = Convert.ToInt32(s[++pos])
                };

                animation.Offset.X = Convert.ToInt32(s[++pos]);
                animation.Offset.Y = Convert.ToInt32(s[++pos]);

                int frames = Convert.ToInt32(s[++pos]);
                if (frames < 0) frames = 0;

                animation.Frames = new Frame[frames];
                animator.AddAnimation(animation);

                for (int i = 0; i < frames; i++)
                {
                    var frame = new Frame
                    {
                        FrameTime = Convert.ToInt32(s[++pos]),

                        SourceRectangle = new Rectangle(
                            Convert.ToInt32(s[++pos]),Convert.ToInt32(s[++pos]),
                            Convert.ToInt32(s[++pos]),Convert.ToInt32(s[++pos])),

                        Offset = new Point(
                            Convert.ToInt32(s[++pos]), Convert.ToInt32(s[++pos])),

                        CollisionRectangle = new Rectangle(
                            Convert.ToInt32(s[++pos]), Convert.ToInt32(s[++pos]),
                            Convert.ToInt32(s[++pos]), Convert.ToInt32(s[++pos])),

                        MirroredV = Convert.ToBoolean(s[++pos]),
                        MirroredH = Convert.ToBoolean(s[++pos]),
                    };
                    animator.SetFrameAt(animationId, i, frame);
                }
            }
            return animator;
        }
    }
}