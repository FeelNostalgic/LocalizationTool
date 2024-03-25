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
        public Data.LANGUAGES CurrentLanguage;

        #endregion

        #region Private Variables
        
        private static LocalizationManager _instance;
        
        private const string CSV_PATH = "Assets/LocalizationTool/Data/LocalizationDataLanguage.csv";
        
        private bool _isInitialized;
        
        private readonly List<Data.LANGUAGES> _activeLanguages = new ();
        private static Dictionary<Data.LANGUAGES, Dictionary<string, (Data.GROUPS, string)>> _dictionary = new ();

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

            //Add data to dictionary
            var interDic = new Dictionary<string, (Data.GROUPS, string)>();
            _dictionary.Add(newLanguage, interDic);
            
            _activeLanguages.Add(newLanguage);
            
            editor.AddLanguageFeedbackLabelText = $"{newLanguage} added correctly";
        }

        public async void RemoveLanguageFromCSV(Data.LANGUAGES newLanguage)
        {
            List<string> items;
            var sb = new StringBuilder();
            
            using (var reader = new StreamReader(CSV_PATH))
            {
                var line = await reader.ReadLineAsync(); //Titles
                items = line.Split(',').ToList();
                var index = items.IndexOf(newLanguage.ToString());
                items.Remove(newLanguage.ToString());
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

            //TODO: remove data from dictionary

            _activeLanguages.Remove(newLanguage);
            Debug.Log($"{newLanguage} removed");
        }

        public async void AddNewKeyValue(Data.LANGUAGES language, string key, Data.GROUPS group, string value)
        {
            _dictionary[language].Add(key, (group, value));
            
            List<string> items;
            var sb = new StringBuilder();
            
            //TODO: add to CSV file
            using (var reader = new StreamReader(CSV_PATH))
            {
                var line = await reader.ReadLineAsync(); //Titles
                items = line.Split(',').ToList();
                var index = items.IndexOf(language.ToString());
                sb.AppendLine(line);

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
            
            
        }
        
        #endregion

        #region Private Methods

        private LocalizationManager()
        {
            Init();
        }
        
        public void Init()
        {
            if(_isInitialized) return;
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

            using var reader = new StreamReader(CSV_PATH);

            var line = await reader.ReadLineAsync();
            var items = line.Split(',');
            if (items.Length < 3) return;

            _activeLanguages.Clear();
            
            for (var i = 2; i < items.Length; i++)
            {
                if (Enum.TryParse(items[i], out Data.LANGUAGES language)) _activeLanguages.Add(language);
            }
            
            PrintActiveLanguages();
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