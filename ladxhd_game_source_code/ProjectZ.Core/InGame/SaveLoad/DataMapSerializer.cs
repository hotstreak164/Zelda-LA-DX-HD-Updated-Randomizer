using System.IO;
using ProjectZ.InGame.Things;
using System;

#if DIRECTX
using System.Windows.Forms;
#endif

namespace ProjectZ.InGame.SaveLoad
{
    public class DataMapSerializer
    {
        public static void SaveDialog(string[,] data)
        {
#if DIRECTX
            var openFileDialog = new SaveFileDialog
            {
                RestoreDirectory = true,
                Filter = "Data (*.data)|*.data"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
                SaveData(openFileDialog.FileName, data);
#endif
        }

        public static void LoadDialog(ref string[,] data)
        {
#if DIRECTX
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Data (*.data)|*.data",
                RestoreDirectory = true,
            };

            if (openFileDialog.ShowDialog() != DialogResult.OK) return;

            data = LoadData(openFileDialog.FileName);
#endif
        }
        
        public static void SaveData(string path, string[,] data)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            using var writer = new StreamWriter(path);

            writer.WriteLine(data.GetLength(0));
            writer.WriteLine(data.GetLength(1));

            for (var y = 0; y < data.GetLength(1); y++)
            {
                var line = "";
                for (var x = 0; x < data.GetLength(0); x++)
                    line += (data[x, y] ?? "") + ";";

                writer.WriteLine(line);
            }
        }

        public static string[,] LoadData(string path)
        {
            using var stream = GameFS.OpenReadAny(path);
            using var reader = new StreamReader(stream);

            if (!int.TryParse(reader.ReadLine(), out var lengthX) ||
                !int.TryParse(reader.ReadLine(), out var lengthY) ||
                lengthX < 0 || lengthY < 0)
                throw new InvalidDataException($"Invalid .data header: '{path}'");

            var output = new string[lengthX, lengthY];

            for (int y = 0; y < lengthY; y++)
                for (int x = 0; x < lengthX; x++)
                    output[x, y] = "";

            for (int y = 0; y < lengthY; y++)
            {
                var line = reader.ReadLine();
                if (line == null)
                    break;

                var split = line.Split(';');

                int max = Math.Min(lengthX, split.Length);
                for (int x = 0; x < max; x++)
                    output[x, y] = split[x] ?? "";
            }
            return output;
        }
    }
}
