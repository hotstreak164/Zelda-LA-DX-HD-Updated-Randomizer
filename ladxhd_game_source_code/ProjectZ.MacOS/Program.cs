﻿using System;
using ProjectZ.InGame.Things;

namespace ProjectZ
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var editorMode = false;
            var loadSave = false;
            var saveSlot = 0;

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                if (arg.Equals("editor", StringComparison.OrdinalIgnoreCase))
                {
                    editorMode = true;
                }
                else if (arg.Equals("loadSave", StringComparison.OrdinalIgnoreCase))
                {
                    loadSave = true;

                    if (i + 1 < args.Length && int.TryParse(args[i + 1], out int parsedSlot))
                    {
                        saveSlot = parsedSlot;
                        i++;
                    }
                }
            }

            try
            {
                using (var game = new Game1(editorMode, loadSave, saveSlot))
                {
                    Game1.EditorManager = ProjectZ.Editor.EditorBootstrap.Create(game);
                    game.Run();
                }
            }

            catch (Exception exception)
            {
                // Cross-platform: write to stderr + optionally a file
                Console.Error.WriteLine(exception.ToString());
                System.IO.File.WriteAllText("crash.txt", exception.ToString());
                throw;
            }
        }
    }
}