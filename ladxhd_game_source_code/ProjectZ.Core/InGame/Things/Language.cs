using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectZ.InGame.Controls;

namespace ProjectZ.InGame.Things
{
    public class Language
    {
        private Dictionary<string, string>[] _languageStrings;

        public Dictionary<string, string> Strings => _languageStrings[CurrentLanguageIndex];
        public List<string> LanguageCode { get; private set; } = new List<string> { "eng" };

        public int CurrentLanguageIndex = 0;
        public int CurrentSubLanguageIndex = 0;
        public string CurrentLanguageCode = "eng";

        public void Load()
        {
            // Base languages from APK assets (Data/)
            var baseFiles = GameFS.EnumerateFiles(
                    Values.PathLanguageFolder,
                    recursive: true,
                    acceptFile: name => name.EndsWith(".lng", StringComparison.OrdinalIgnoreCase)
                );

            // 2) Mod languages from filesystem
            IEnumerable<string> modFiles = Enumerable.Empty<string>();
            if (Directory.Exists(Values.PathMods))
                modFiles = Directory.EnumerateFiles(Values.PathMods, "*.lng", SearchOption.AllDirectories);

            var files = baseFiles
                .Concat(modFiles)
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var languageStrings = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["eng"] = new Dictionary<string, string>()
            };

            for (var i = 0; i < files.Length; i++)
            {
                var fileNameNoExt = Path.GetFileNameWithoutExtension(files[i]);
                var split = fileNameNoExt.Split('_');

                string lngName = "";

                // "eng.lng"
                if (split.Length == 1)
                    lngName = split[0];

                // "dialog_eng.lng"
                if (split.Length == 2)
                    lngName = split[1];

                if (!languageStrings.TryGetValue(lngName, out var dict) || dict == null)
                {
                    dict = new Dictionary<string, string>();
                    languageStrings[lngName] = dict;
                }

                if (split.Length == 1 || (split.Length == 2 && split[0].Equals("dialog", StringComparison.OrdinalIgnoreCase)))
                    LoadFile(dict, files[i]);
            }

            LanguageCode = new List<string> { "eng" };
            LanguageCode.AddRange(languageStrings.Keys.Where(k => !k.Equals("eng", StringComparison.OrdinalIgnoreCase)));

            _languageStrings = LanguageCode.Select(k => languageStrings[k]).ToArray();
            CurrentLanguageIndex = Math.Clamp(CurrentLanguageIndex, 0, _languageStrings.Length - 1);
            CurrentLanguageCode = LanguageCode[CurrentLanguageIndex];
        }

        public void LoadFile(Dictionary<string, string> dictionary, string fileName)
        {
            // If it looks like an asset path (Data/ or Content/), use GameFS.
            // Otherwise treat it as a real OS file path (mods).
            var ap = GameFS.ToAssetPath(fileName);

            Stream stream =
                (ap.StartsWith("Data/", StringComparison.OrdinalIgnoreCase) ||
                 ap.StartsWith("Content/", StringComparison.OrdinalIgnoreCase))
                    ? GameFS.OpenRead(ap)
                    : File.OpenRead(fileName);

            using (stream)
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var strLine = reader.ReadLine();
                    if (string.IsNullOrEmpty(strLine))
                        continue;

                    var spacePosition = strLine.IndexOf(' ');
                    if (spacePosition < 0 || strLine.StartsWith("//"))
                        continue;

                    var strKey = strLine.Substring(0, spacePosition);

                    // empty string
                    if (spacePosition + 1 >= strLine.Length)
                    {
                        dictionary[strKey] = "";
                        continue;
                    }

                    var strValue = strLine.Substring(spacePosition + 1);
                    dictionary[strKey] = strValue;
                }
            }
        }

        public string GetString(string strKey, string defaultString)
        {
            if (strKey == null)
                return "null";

            if (Strings.TryGetValue(strKey, out var value))
                return value;

            // use the english text if there is no translation
            if (_languageStrings[0].TryGetValue(strKey, out value))
                return value;

            return defaultString;
        }

        public void ToggleLanguage()
        {
            // Update the currently selected language.
            CurrentLanguageIndex = (CurrentLanguageIndex + 1) % _languageStrings.Length;
            CurrentLanguageCode = LanguageCode[CurrentLanguageIndex];
        }

        public string ReplacePlaceholderTag(string inputString)
        {
            string Confirm = ControlHandler.ControllerLabels[ControlHandler.ControllerIndex, 0];
            string Cancel  = ControlHandler.ControllerLabels[ControlHandler.ControllerIndex, 1];

            if (GameSettings.SwapButtons)
            {
                Confirm = ControlHandler.ControllerLabels[ControlHandler.ControllerIndex, 1];
                Cancel  = ControlHandler.ControllerLabels[ControlHandler.ControllerIndex, 0];
            }
            string Select = ControlHandler.ControllerLabels[ControlHandler.ControllerIndex, 8];
            string Start  = ControlHandler.ControllerLabels[ControlHandler.ControllerIndex, 9];
            string X = ControlHandler.ControllerLabels[ControlHandler.ControllerIndex, 2];
            string Y = ControlHandler.ControllerLabels[ControlHandler.ControllerIndex, 3];

            // Inserts the players name.
            inputString = inputString.Replace("[NAME]", Game1.GameManager.SaveName);

            // Inserts the trade icons.
            inputString = inputString.Replace("[TRADE0]", "¯");
            inputString = inputString.Replace("[TRADE1]", "¢");
            inputString = inputString.Replace("[TRADE2]", "£");
            inputString = inputString.Replace("[TRADE3]", "¤");
            inputString = inputString.Replace("[TRADE4]", "¥");
            inputString = inputString.Replace("[TRADE5]", "¦");
            inputString = inputString.Replace("[TRADE6]", "§");
            inputString = inputString.Replace("[TRADE7]", "¨");
            inputString = inputString.Replace("[TRADE8]", "©");
            inputString = inputString.Replace("[TRADE9]", "ª");
            inputString = inputString.Replace("[TRADE10]", "«");
            inputString = inputString.Replace("[TRADE11]", "¬");
            inputString = inputString.Replace("[TRADE12]", "­");
            inputString = inputString.Replace("[TRADE13]", "®");

            // Inserts special icons.
            inputString = inputString.Replace("[SKULL]", "µ");
            inputString = inputString.Replace("[MARIN]", "¶");
            inputString = inputString.Replace("[LINK]", "·");

            // Inserts controller icons.
            inputString = inputString.Replace("[LEFT]", "°");
            inputString = inputString.Replace("[RIGHT]", "±");
            inputString = inputString.Replace("[DOWN]", "²");
            inputString = inputString.Replace("[UP]", "³");
            inputString = inputString.Replace("[DPAD]", "´");
            inputString = inputString.Replace("[CONFIRM]", Confirm);
            inputString = inputString.Replace("[CANCEL]", Cancel);
            inputString = inputString.Replace("[START]", Start);
            inputString = inputString.Replace("[SELECT]", Select);
            inputString = inputString.Replace("[X]", X);
            inputString = inputString.Replace("[Y]", Y);
            return inputString;
        }
    }
}
