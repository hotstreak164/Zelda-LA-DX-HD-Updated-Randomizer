using System;
using System.IO;
using System.Globalization;
using System.Reflection;
using ProjectZ.InGame.SaveLoad;

namespace ProjectZ.InGame.Things
{
    public static class ModFile
    {
        public static void Parse(string modFile, dynamic inputClass)
        {
            ParseAdvanced(SaveManager.GetAdvancedFile(), inputClass);

            if (!File.Exists(modFile))
                return;

            foreach (string line in File.ReadAllLines(modFile))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                    continue;

                string[] splitLine = line.Split(new char[]{ '=', '/' });
                if (splitLine.Length < 2)
                    continue;

                string varName = splitLine[0].Trim();
                string varValue = splitLine[1].Trim();

                FieldInfo field = inputClass.GetType().GetField(varName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if (field == null) { continue; }

                object convertedValue = Convert.ChangeType(varValue, field.FieldType, CultureInfo.InvariantCulture);
                field.SetValue(inputClass, convertedValue);
            }
        }

        public static void ParseStatic(string modFile, Type inputClass)
        {
            ParseAdvancedStatic(SaveManager.GetAdvancedFile(), inputClass);

            if (!File.Exists(modFile))
                return;

            foreach (string line in File.ReadAllLines(modFile))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                    continue;

                string[] splitLine = line.Split(new char[]{ '=', '/' });
                if (splitLine.Length < 2)
                    continue;

                string varName = splitLine[0].Trim();
                string varValue = splitLine[1].Trim();

                FieldInfo field = inputClass.GetField(varName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

                if (field == null) { continue; }

                object convertedValue = Convert.ChangeType(varValue, field.FieldType, CultureInfo.InvariantCulture);
                field.SetValue(inputClass, convertedValue);
            }
        }

        public static void ParseAdvanced(string advancedFile, dynamic inputClass)
        {
            if (!File.Exists(advancedFile))
                return;

            string className = inputClass.GetType().Name;
            bool inSection = false;

            foreach (string line in File.ReadAllLines(advancedFile))
            {
                // Check for a class section marker: //: ClassName
                if (line.TrimStart().StartsWith("//: "))
                {
                    inSection = line.TrimStart().Substring(4).Trim() == className;
                    continue;
                }

                // Any other comment or blank line ends tracking but doesn't reset section
                // (multiple variables can follow a single //: marker)
                if (!inSection || string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("//"))
                    continue;

                string[] splitLine = line.Split(new char[] { '=', '/' });
                if (splitLine.Length < 2)
                    continue;

                string varName = splitLine[0].Trim();
                string varValue = splitLine[1].Trim();

                FieldInfo field = inputClass.GetType().GetField(varName, 
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field == null) continue;

                try
                {
                    object convertedValue = Convert.ChangeType(varValue, field.FieldType, CultureInfo.InvariantCulture);
                    field.SetValue(inputClass, convertedValue);
                }
                catch { }
            }
        }

        public static void ParseAdvancedStatic(string advancedFile, Type inputClass)
        {
            if (!File.Exists(advancedFile))
                return;

            string className = inputClass.Name;
            bool inSection = false;

            foreach (string line in File.ReadAllLines(advancedFile))
            {
                if (line.TrimStart().StartsWith("//: "))
                {
                    inSection = line.TrimStart().Substring(4).Trim() == className;
                    continue;
                }

                if (!inSection || string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("//"))
                    continue;

                string[] splitLine = line.Split(new char[] { '=', '/' });
                if (splitLine.Length < 2)
                    continue;

                string varName = splitLine[0].Trim();
                string varValue = splitLine[1].Trim();

                FieldInfo field = inputClass.GetField(varName, 
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (field == null) continue;

                try
                {
                    object convertedValue = Convert.ChangeType(varValue, field.FieldType, CultureInfo.InvariantCulture);
                    field.SetValue(null, convertedValue);
                }
                catch { }
            }
        }
    }
}
