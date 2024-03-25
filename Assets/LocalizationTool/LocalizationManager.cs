using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using File = System.IO.File;

namespace LocalizationTool
{
    public class LocalizationManager
    {
        public static LocalizationManager Instance => _instance ??= new LocalizationManager();
        private static LocalizationManager _instance;

        #region Public Variables

        public static List<Data.LANGUAGES> ActiveLanguages => _activeLanguages;

        #endregion

        #region Private Variables

        private bool _isInitialized;
        private static readonly List<Data.LANGUAGES> _activeLanguages = new List<Data.LANGUAGES>();

        private const string CSV_PATH = "Assets/LocalizationTool/Data/LocalizationDataLanguage.csv";

        #endregion


        #region Public Methods

        public async Task<string> AddNewLanguageToCSV(Data.LANGUAGES newLanguage)
        {
            if (_activeLanguages.Contains(newLanguage)) return $"{newLanguage} already exists";

            if (!File.Exists(CSV_PATH)) await InitFile();

            string line;

            using (var reader = new StreamReader(CSV_PATH))
            {
                line = await reader.ReadLineAsync();
            }

            await using (var writer = new StreamWriter(CSV_PATH))
            {
                await writer.WriteAsync($"{line},{newLanguage}");
            }

            _activeLanguages.Add(newLanguage);
            
            return $"{newLanguage} added correctly";
        }

        public void RemoveLanguageFromCVS(Data.LANGUAGES newLanguage)
        {
            
            _activeLanguages.Remove(newLanguage);
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
            Debug.Log("CVS loaded");
        }

        private static void PrintActiveLanguages()
        {
            foreach (var l in _activeLanguages)
            {
                Debug.Log(l);
            }
        }

        #endregion
    }
}