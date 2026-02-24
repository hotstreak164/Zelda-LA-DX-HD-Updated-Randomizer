using System;
using System.Collections.Generic;
using System.IO;
using ProjectZ.InGame.Things;
using System.Globalization;
#if WINDOWS
using System.Windows.Forms;
#endif

namespace ProjectZ.InGame.SaveLoad
{
    public class SaveManager
    {
        private readonly Dictionary<string, bool> _boolDictionary = new Dictionary<string, bool>();
        private readonly Dictionary<string, int> _intDictionary = new Dictionary<string, int>();
        private readonly Dictionary<string, float> _floatDictionary = new Dictionary<string, float>();
        private readonly Dictionary<string, string> _stringDictionary = new Dictionary<string, string>();

        private int _shellCount;
        private bool _hasSwordLevel2;
        private bool _hasMirrorShield;
        private bool[] _hasInstruments;

        public int ShellCount { get => _shellCount; }
        public bool HasSwordLevel2 { get => _hasSwordLevel2; }
        public bool HasMirrorShield { get => _hasMirrorShield; }

        public bool HasInstrument(int index)
        {
            if (_hasInstruments == null || index < 0 || index >= _hasInstruments.Length)
                return false;
            return _hasInstruments[index];
        }

        public static string GetSaveFilePath()
        {
            string portable = Path.Combine(Values.WorkingDirectory,"portable.txt");
            if (File.Exists(portable))
                return "SaveFiles\\";
            return Path.Combine(Values.AppDataFolder,"Zelda_LA","SaveFiles");
        }

        public static string GetSettingsFile()
        {
            string portable = Path.Combine(Values.WorkingDirectory,"portable.txt");
            if (File.Exists(portable))
                return "settings";
            return Path.Combine(Values.AppDataFolder,"Zelda_LA","settings");
        }

        struct HistoryFrame
        {
            public string Key;

            public bool? BoolValueOld;
            public bool? BoolValue;

            public int? IntValueOld;
            public int? IntValue;

            public float? FloatValueOld;
            public float? FloatValue;

            public string StringValueOld;
            public string StringValue;
        }

        private Stack<HistoryFrame> _history = new Stack<HistoryFrame>();
        private bool _historyEnabled;

        public bool HistoryEnabled
        {
            get { return _historyEnabled; }
        }

        public void Save(string filePath, int retries)
        {
            for (var i = 0; i < retries; i++)
            {
                try
                {
                    Save(filePath);
                    return;
                }
                catch (Exception) { }
            }

#if WINDOWS
            MessageBox.Show("Error while saving", "Saving Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
        }

        private void Save(string filePath)
        {
            Directory.CreateDirectory(Values.PathSaveFolder);

            FileStream fileStream;
            if (!File.Exists(filePath))
                fileStream = File.Create(filePath);
            else
            {
                fileStream = File.OpenWrite(filePath);
                fileStream.SetLength(0);
            }

            using (var writer = new StreamWriter(fileStream))
            {
                foreach (var element in _boolDictionary)
                    writer.WriteLine("b " + element.Key + " " + element.Value);
                foreach (var element in _intDictionary)
                    writer.WriteLine("i " + element.Key + " " + element.Value);
                foreach (var element in _floatDictionary)
                    writer.WriteLine("f " + element.Key + " " + element.Value.ToString(CultureInfo.InvariantCulture));
                foreach (var element in _stringDictionary)
                    writer.WriteLine("s " + element.Key + " " + element.Value);
            }
            fileStream.Close();
            fileStream.Dispose();
        }

        public void Reset()
        {
            _boolDictionary.Clear();
            _intDictionary.Clear();
            _stringDictionary.Clear();
        }

        public static bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public bool LoadFile(string filePath)
        {
            Reset();

            if (!File.Exists(filePath))
                return false;

            _hasInstruments = new bool[8];

            for (var i = 0; i < Values.LoadRetries; i++)
            {
                try
                {
                    using (var reader = new StreamReader(filePath))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            var strSplit = line?.Split(' ');

                            if (strSplit?.Length >= 3)
                            {
                                var valueString = line.Substring(strSplit[0].Length + strSplit[1].Length + 2);

                                if (strSplit[0] == "b")
                                    _boolDictionary.Add(strSplit[1], Convert.ToBoolean(valueString));
                                
                                else if (strSplit[0] == "i")
                                    _intDictionary.Add(strSplit[1], Convert.ToInt32(valueString));
                                
                                else if (strSplit[0] == "f")
                                    _floatDictionary.Add(strSplit[1], float.Parse(valueString, CultureInfo.InvariantCulture));
                                
                                else if (strSplit[0] == "s")
                                {
                                    if (strSplit[2] == "sword2:1")
                                        _hasSwordLevel2 = true;

                                    if (strSplit[2] == "mirrorShield:1")
                                        _hasMirrorShield = true;

                                    if (strSplit[2].StartsWith("shell:"))
                                    {
                                        string count = strSplit[2].Split(':')[1];
                                        _shellCount = Convert.ToInt32(count);
                                    }                                       

                                    for (int j = 0; j < 8; j++) 
                                    {
                                        if (strSplit[2] == "instrument" + j + ":1")
                                            _hasInstruments[j] = true;
                                    }
                                    _stringDictionary.Add(strSplit[1], valueString);
                                }
                            }
                        }
                    }
                    return true;
                }
                catch (Exception) { }
            }
            return false;
        }

        public void SetBool(string key, bool value)
        {
            if (_boolDictionary.ContainsKey(key))
            {
                if (_historyEnabled && _boolDictionary[key] != value)
                    _history.Push(new HistoryFrame() { Key = key, BoolValueOld = _boolDictionary[key], BoolValue = value });
                _boolDictionary[key] = value;
            }
            else
            {
                if (_historyEnabled)
                    _history.Push(new HistoryFrame() { Key = key, BoolValue = value });
                _boolDictionary.Add(key, value);
            }
            Game1.GameManager.MapManager.CurrentMap.Objects.TriggerKeyChange();
        }

        public bool GetBool(string key, bool defaultReturn)
        {
            if (key != null && _boolDictionary.ContainsKey(key))
                return _boolDictionary[key];

            return defaultReturn;
        }

        public void SetInt(string key, int value)
        {
            if (_intDictionary.ContainsKey(key))
            {
                if (_historyEnabled && _intDictionary[key] != value)
                    _history.Push(new HistoryFrame() { Key = key, IntValueOld = _intDictionary[key], IntValue = value });

                _intDictionary[key] = value;
            }
            else
            {
                if (_historyEnabled)
                    _history.Push(new HistoryFrame() { Key = key, IntValue = value });

                _intDictionary.Add(key, value);
            }
            Game1.GameManager.MapManager.CurrentMap.Objects.TriggerKeyChange();
        }

        public int GetInt(string key)
        {
            // Retain compatiblity with older saves that used "Hearth" instead of "Hearts".
            if ((key == "maxHearts") & (_intDictionary.ContainsKey("maxHearth")))
                key = "maxHearth";
            if ((key == "currentHealth") & (_intDictionary.ContainsKey("currentHearth")))
                key = "currentHearth";
            return _intDictionary[key];
        }

        public int GetInt(string key, int defaultReturn)
        {
            if (_intDictionary.ContainsKey(key))
                return _intDictionary[key];

            return defaultReturn;
        }

        public void RemoveInt(string key)
        {
            if (_intDictionary.ContainsKey(key))
            {
                if (_historyEnabled)
                    _history.Push(new HistoryFrame() { Key = key, IntValueOld = _intDictionary[key] });

                _intDictionary.Remove(key);
            }
            Game1.GameManager.MapManager.CurrentMap.Objects.TriggerKeyChange();
        }

        public void SetFloat(string key, float value)
        {
            if (_floatDictionary.ContainsKey(key))
            {
                if (_historyEnabled && _floatDictionary[key] != value)
                    _history.Push(new HistoryFrame() { Key = key, FloatValueOld = _floatDictionary[key], FloatValue = value });

                _floatDictionary[key] = value;
            }
            else
            {
                if (_historyEnabled)
                    _history.Push(new HistoryFrame() { Key = key, FloatValue = value });

                _floatDictionary.Add(key, value);
            }
            Game1.GameManager.MapManager.CurrentMap.Objects.TriggerKeyChange();
        }

        public float GetFloat(string key)
        {
            return _floatDictionary[key];
        }

        public float GetFloat(string key, float defaultReturn)
        {
            if (_floatDictionary.ContainsKey(key))
                return _floatDictionary[key];

            return defaultReturn;
        }

        public void SetString(string key, string value)
        {
            if (_stringDictionary.ContainsKey(key))
            {
                if (_historyEnabled && _stringDictionary[key] != value)
                    _history.Push(new HistoryFrame() { Key = key, StringValueOld = _stringDictionary[key], StringValue = value });

                _stringDictionary[key] = value;
            }
            else
            {
                if (_historyEnabled)
                    _history.Push(new HistoryFrame() { Key = key, StringValue = value });

                _stringDictionary.Add(key, value);
            }
            Game1.GameManager.MapManager.CurrentMap.Objects.TriggerKeyChange();
        }

        public string GetString(string key)
        {
            _stringDictionary.TryGetValue(key, out string outString);
            return outString;
        }

        public string GetString(string key, string defaultValue)
        {
            _stringDictionary.TryGetValue(key, out string outString);
            if (outString == null)
                outString = defaultValue;
            return outString;
        }

        public void RemoveString(string key)
        {
            if (_stringDictionary.ContainsKey(key))
            {
                if (_historyEnabled)
                    _history.Push(new HistoryFrame() { Key = key, StringValueOld = _stringDictionary[key] });

                _stringDictionary.Remove(key);
            }
            Game1.GameManager.MapManager.CurrentMap.Objects.TriggerKeyChange();
        }

        public bool ContainsValue(string key)
        {
            return
                _boolDictionary.ContainsKey(key) ||
                _intDictionary.ContainsKey(key) ||
                _floatDictionary.ContainsKey(key) ||
                _stringDictionary.ContainsKey(key);
        }

        public void EnableHistory()
        {
            _historyEnabled = true;
        }

        public void DisableHistory()
        {
            _historyEnabled = false;
            _history.Clear();
        }

        public void RevertHistory()
        {
            while (0 < _history.Count)
            {
                var frame = _history.Pop();

                if (frame.BoolValue != null)
                {
                    if (frame.BoolValueOld != null)
                        _boolDictionary[frame.Key] = frame.BoolValueOld.Value;
                    else
                        _boolDictionary.Remove(frame.Key);
                }
                else if (frame.IntValue != null)
                {
                    if (frame.IntValueOld != null)
                        _intDictionary[frame.Key] = frame.IntValueOld.Value;
                    else
                        _intDictionary.Remove(frame.Key);
                }
                else if (frame.FloatValue != null)
                {
                    if (frame.FloatValueOld != null)
                        _floatDictionary[frame.Key] = frame.FloatValueOld.Value;
                    else
                        _floatDictionary.Remove(frame.Key);
                }
                else if (frame.StringValue != null)
                {
                    if (frame.StringValueOld != null)
                        _stringDictionary[frame.Key] = frame.StringValueOld;
                    else
                        _stringDictionary.Remove(frame.Key);
                }
            }
        }
    }
}
