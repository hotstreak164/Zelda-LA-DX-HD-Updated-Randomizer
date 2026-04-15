using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

// Credits to Danny Beckett at stackoverflow.
// https://stackoverflow.com/questions/217902/reading-writing-an-ini-file

namespace LADXHD_ModMaker
{
    internal class IniFile
    {
        string Path;
        string EXE = Assembly.GetExecutingAssembly().GetName().Name;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileSection(string Section, byte[] RetVal, int Size, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileSectionNames(byte[] RetVal, int Size, string FilePath);

        public IniFile(string IniPath = null)
        {
            this.Path = new FileInfo(IniPath ?? this.EXE + ".ini").FullName;
        }

        public string Read(string Key, string Section = null)
        {
            var RetVal = new StringBuilder(4096);
            GetPrivateProfileString(Section ?? this.EXE, Key, "", RetVal, 4096, this.Path);

            string Value = RetVal.ToString();

            if (Value.Contains(";") | Value.Contains("#"))
            {
                string[] SplitValue = Value.Split(';','#');
                Value = SplitValue[0].Trim();
            }
            return Value;
        }

        public void Write(string Key, string Value, string Section = null)
        {
            WritePrivateProfileString(Section ?? this.EXE, Key, Value, this.Path);
        }

        public void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section ?? this.EXE);
        }

        public void ConditionalWriteDelete(string Key, string Value, string Section = null, bool DoWrite = true)
        {
            switch (DoWrite)
            {
                case true:  { Write(Key, Value, Section); break; }
                case false: { DeleteKey(Key, Section); break; }
            }
        }

        public void DeleteSection(string Section = null)
        {
            Write(null, null, Section ?? this.EXE);
        }

        public bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0;
        }

        public List<string> GetSections()
        {
            byte[] ByteArray = new byte[32767];
            GetPrivateProfileSectionNames(ByteArray, 32767, this.Path);

            string[] Chars = Encoding.ASCII.GetString(ByteArray).Split('\0');
            List<string> Sections = new List<string>();
            string sectionName = "";

            for (int i = 0; i < Chars.Length; i++)
            {
                if (Chars[i] != "" & i != Chars.Length - 1)
                {
                    sectionName += Chars[i];
                }
                else if (sectionName != "")
                {
                    Sections.Add(sectionName);
                    sectionName = "";
                }
            }
            return Sections;
        }

        public List<string> GetSectionKeys(string Section)
        {
            byte[] ByteArray = new byte[32767];
            GetPrivateProfileSection(Section ?? this.EXE, ByteArray, 32767, this.Path);

            string[] Chars = Encoding.ASCII.GetString(ByteArray).Trim('\0').Split('\0');
            string Line = "";
            List<string> Keys = new List<string>();

            for (int i = 0; i < Chars.Length; i++)
            {
                if (Chars[i] != "" & i != Chars.Length - 1)
                {
                    Line += Chars[i];
                }
                else if (Line != "")
                {
                    Keys.Add(Line.Split('=')[0]);
                    Line = "";
                }
            }
            return Keys;
        }
    }
}