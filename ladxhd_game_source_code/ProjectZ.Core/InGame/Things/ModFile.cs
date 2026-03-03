using System;
using System.IO;
using System.Globalization;
using System.Reflection;

namespace ProjectZ.InGame.Things
{
    public static class ModFile
    {
        public static void Parse(string modFile, dynamic inputClass)
        {
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
    }
}
