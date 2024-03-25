using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LocalizationTool.Editor;
using UnityEngine;
using File = System.IO.File;

namespace LocalizationTool
{
    public class LocalizationManager
    {
        #region Public Variables

        public static LocalizationManager Instance => _instance ??= new LocalizationManager();

        public List<Data.LANGUAGES> ActiveLanguages => _activeLanguages;
        public Dictionary<(string key, Data.GROUPS group), Dictionary<Data.LANGUAGES, string>> Dictionary => _dictionary;
        public Data.LANGUAGES CurrentLanguage;

        #endregion

        #region Private Variables

        private static LocalizationManager _instance;

        private const string CSV_PATH = "Assets/LocalizationTool/Data/LocalizationDataLanguage.csv";

        private bool _isInitialized;
        private bool _isDictionaryLocked;

        private readonly List<Data.LANGUAGES> _activeLanguages = new();
        private static Dictionary<(string key, Data.GROUPS group), Dictionary<Data.LANGUAGES, string>> _dictionary = new();
        private static Dictionary<string, Data.GROUPS> _groups = new();

        #endregion

        #region Public Methods

        public async void AddNewLanguageToCSV(Data.LANGUAGES newLanguage, LocalizationEditor editor)
        {
            if (_activeLanguages.Contains(newLanguage))
            {
                editor.AddLanguageFeedbackLabelText = $"{newLanguage} already exists";
                return;
            }

            if (!File.Exists(CSV_PATH)) await InitFile();

            string line;

            //Add space for language value to CSV
            var sb = new StringBuilder();
            using (var reader = new StreamReader(CSV_PATH))
            {
                line = await reader.ReadLineAsync(); //Titles
                sb.AppendLine($"{line},{newLanguage}");
                while (!reader.EndOfStream) // Data
                {
                    line = await reader.ReadLineAsync();
                    sb.AppendLine($"{line},");
                }
            }

            await using (var writer = new StreamWriter(CSV_PATH))
            {
                await writer.WriteAsync(sb.ToString());
            }

            _activeLanguages.Add(newLanguage);
            
            foreach (var ((key, group), _) in _dictionary)
            {
                _dictionary[(key, group)].Add(newLanguage, "");
            }

            editor.AddLanguageFeedbackLabelText = $"{newLanguage} added correctly";
        }

        public async void RemoveLanguageFromCSV(Data.LANGUAGES languageToRemove)
        {
            List<string> items;
            var sb = new StringBuilder();

            using (var reader = new StreamReader(CSV_PATH))
            {
                var line = await reader.ReadLineAsync(); //Titles
                items = line.Split(',').ToList();
                var index = items.IndexOf(languageToRemove.ToString());
                items.Remove(languageToRemove.ToString());
                sb.AppendJoin(',', items).AppendLine();

                while (!reader.EndOfStream) // Data
                {
                    line = await reader.ReadLineAsync();
                    items = line.Split(',').ToList();
                    items.RemoveAt(index);
                    sb.AppendJoin(',', items).AppendLine();
                }
            }

            await using (var writer = new StreamWriter(CSV_PATH))
            {
                await writer.WriteAsync(sb.ToString());
            }

            //Remove data from dictionary
            foreach (var ((key, group), _) in _dictionary)
            {
                _dictionary[(key, group)].Remove(languageToRemove);
            }

            _activeLanguages.Remove(languageToRemove);
            Debug.Log($"{languageToRemove} removed");
        }

        public async void AddNewKeyValue(string key, Data.GROUPS group, string value, LocalizationEditor editor)
        {
            if (key.Equals(""))
            {
                editor.AddValueFeedbackLabelText = "Key cannot be an empty value";
                return;
            }

            if (_dictionary.ContainsKey((key, group)))
            {
                editor.AddValueFeedbackLabelText = $"{key} already exists";
                return;
            }
            
            var interDic = _activeLanguages.ToDictionary(language => language, _ => value);
            _dictionary.Add((key,group), interDic);
            _groups.Add(key, group);

            var sb = new StringBuilder();

            //Add key to CSV file
            await using (var writer = File.AppendText(CSV_PATH)) //Add old key-values
            {
                var list = new List<string>
                {
                    key,
                    group.ToString()
                };
                list.AddRange(_activeLanguages.Select(l => ""));
                sb.AppendJoin(',', list);
                await writer.WriteLineAsync(sb.ToString());
            }

            editor.AddValueFeedbackLabelText = $"{key} added correctly";
        }

        public async void ChangeValue(string key, Data.GROUPS group, string value, Data.LANGUAGES language)
        {
            if(_isDictionaryLocked) return;
            _isDictionaryLocked = true;
            
            if(_dictionary[(key,group)][language].Equals(value)) return; //value is not modified
            
            _dictionary[(key,group)].Remove(language);
            _dictionary[(key,group)].Add(language, value);

            //Change CSV file 
            string output;
            using (var reader = new StreamReader(CSV_PATH))
            {
                var header = await reader.ReadLineAsync();
                var readToEnd = await reader.ReadToEndAsync();
                var lines = readToEnd.Split('\n');
                var list = lines.Select(s => s.Split(",")[0]).ToList();
                var index = list.IndexOf(key);
                //Debug.Log($"{key} is in index: {index}");
                var items = lines[index].Split(',');
                items[_activeLanguages.IndexOf(language) + 2] = value;
                lines[index] = string.Join(',', items);

                output = $"{header}\n{string.Join("\n", lines)}";
            }

            await using (var writer = new StreamWriter(CSV_PATH))
            {
                await writer.WriteAsync(output);
            }

            _isDictionaryLocked = false;
        }

        public async void ChangeGroup(string key, Data.GROUPS newGroup)
        {
            if(_isDictionaryLocked) return;
            _isDictionaryLocked = true;
            if(_dictionary.ContainsKey((key,newGroup))) return;

            var oldGroup = _groups[key];
            var interDic = _dictionary[(key, oldGroup)];
            _dictionary.Remove((key, oldGroup));
            _dictionary.Add((key, newGroup), interDic);
            _groups[key] = newGroup;
            
            string output;
            using (var reader = new StreamReader(CSV_PATH))
            {
                var header = await reader.ReadLineAsync();
                var readToEnd = await reader.ReadToEndAsync();
                var lines = readToEnd.Split('\n');
                var list = lines.Select(s => s.Split(",")[0]).ToList();
                var index = list.IndexOf(key);
                //Debug.Log($"{key} is in index: {index}");
                var items = lines[index].Split(',');
                items[1] = newGroup.ToString();
                lines[index] = string.Join(',', items);

                output = $"{header}\n{string.Join("\n", lines)}";
            }

            await using (var writer = new StreamWriter(CSV_PATH))
            {
                await writer.WriteAsync(output);
            }

            _isDictionaryLocked = false;
        }

        public void RefreshData()
        {
            LoadLanguagesFromCSV();
        }

        #endregion

        #region Private Methods

        private LocalizationManager()
        {
            Init();
        }

        public void Init()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            LoadLanguagesFromCSV();
        }

        private async Task InitFile()
        {
            await File.Create(CSV_PATH).DisposeAsync();
            await using var writer = new StreamWriter(CSV_PATH);
            await writer.WriteAsync("Key,Group");
        }

        private async void LoadLanguagesFromCSV()
        {
            if (!File.Exists(CSV_PATH))
            {
                Debug.Log("Creating file");
                await InitFile();
                return;
            }

            using (var reader = new StreamReader(CSV_PATH))
            {
                //Load Titles (Languages)
                var line = await reader.ReadLineAsync();
                var items = line.Split(',');
                if (items.Length < 3) return;

                _activeLanguages.Clear();

                for (var i = 2; i < items.Length; i++)
                {
                    if (!Enum.TryParse(items[i], out Data.LANGUAGES language)) continue;
                    _activeLanguages.Add(language);
                }

                // Load data
                _dictionary.Clear();
                _groups.Clear();
                while (!reader.EndOfStream)
                {
                    line = await reader.ReadLineAsync();
                    items = line.Split(',');
                    var key = items[0];
                    if (Enum.TryParse(items[1], out Data.GROUPS group)) ;
                    var interDic = new Dictionary<Data.LANGUAGES, string>();
                    for (var i = 2; i < items.Length; i++)
                    {
                        interDic.Add(_activeLanguages[i-2], items[i]);
                    }

                    _dictionary.Add((key,group), interDic);
                    _groups.Add(key, group);
                }
                
            }
            //PrintActiveLanguages();
            Debug.Log("CSV loaded");
        }

        private void PrintActiveLanguages()
        {
            foreach (var l in _activeLanguages)
            {
                Debug.Log(l);
            }
        }

        #endregion
    }
}