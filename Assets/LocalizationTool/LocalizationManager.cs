using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LocalizationTool.Editor;
using UnityEngine;

namespace LocalizationTool
{
    public class LocalizationManager
    {
        #region Public Variables

        public static LocalizationManager Instance => _instance ??= new LocalizationManager();

        public List<Data.LANGUAGES> ActiveLanguages => _activeLanguages;
        public Dictionary<string, KeyData> Dictionary => _dictionary;
        public Data.LANGUAGES CurrentLanguage;

        #endregion

        #region Private Variables

        private static LocalizationManager _instance;
        private ISerializerService _serializer;
        
        #region JSON Data

        private const string JSON_PATH = "Assets/LocalizationTool/Data/LocalizationDataLanguage.json";
        private static JsonData _jsonData;

        #endregion

        private bool _isInitialized;

        //TODO: remove this
        private static readonly List<Data.LANGUAGES> _activeLanguages = new()
        {
            Data.LANGUAGES.Spanish,
            Data.LANGUAGES.English,
            Data.LANGUAGES.Italian
        };
        //
        private static Dictionary<string, KeyData> _dictionary;

        #endregion

        #region Public Methods
        
        #region JSON

        public async void AddNewKey(string key, Data.GROUPS group, LocalizationEditor editor)
        {
            if (key.Equals(""))
            {
                editor.AddValueFeedbackLabelText = "Key cannot be an empty value";
                return;
            }

            if (_dictionary.ContainsKey(key))
            {
                editor.AddValueFeedbackLabelText = $"{key} already exists";
                return;
            }
            
            var interDic = _activeLanguages.ToDictionary(language => language, _ => "");
            var interList = _activeLanguages.Select(l => new LanguageValue{Language = l, Value = ""}).ToList();
            _dictionary.Add(key, new KeyData{Group = group, LanguagesData = interDic});

            //Add key to JSON file
            _jsonData.ListDictionaryKeyValue.Add(new KeyValue(key, group, interList));
            
            await SaveFile(_jsonData,JSON_PATH);

            editor.AddValueFeedbackLabelText = $"{key} added correctly";
        }
        
        public async void ChangeValue(string key, string newValue, Data.LANGUAGES language)
        {
            if(_dictionary[key].LanguagesData != null && _dictionary[key].LanguagesData[language].Equals(newValue)) return; //value is not modified

            UpdateValueInDictionary(key, newValue, language);

            //Change JSON file 
            _jsonData.ListDictionaryKeyValue.First(x => x.Key == key).UpdateValue(language, newValue);

            await SaveFile(_jsonData, JSON_PATH);
        }
        
        public async void ChangeGroup(string key, Data.GROUPS newGroup)
        {
            if(_dictionary[key].Group == newGroup) return;

            var oldData = _dictionary[key].LanguagesData;
            _dictionary.Remove(key);
            _dictionary.Add(key, new KeyData{Group = newGroup, LanguagesData = oldData});
            
            //Change JSON file 
            var data = _jsonData.ListDictionaryKeyValue.First(x => x.Key == key);
            data.Group = newGroup;
            
            await SaveFile(_jsonData,JSON_PATH);
        }

        public async void RemoveKey(string key)
        {
            _dictionary.Remove(key);
            
            //Change JSON file 
            var data = _jsonData.ListDictionaryKeyValue.First(x => x.Key == key);
            _jsonData.ListDictionaryKeyValue.Remove(data);

            await SaveFile(_jsonData,JSON_PATH);
        }

        #endregion
        
        public void RefreshData()
        {
            LoadDataFromJSON();
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
            _serializer = new UnityJsonSerializer();
            LoadDataFromJSON();
        }
        
        private async void LoadDataFromJSON()
        {
            _jsonData ??= new JsonData();
            var aux = await LoadFile<JsonData>(JSON_PATH);
            if (aux != null) _jsonData = aux;

            if(_dictionary != null) _dictionary.Clear();
            else _dictionary = new Dictionary<string, KeyData>();
            
            if(_jsonData != null) _dictionary = _jsonData.ListDictionaryKeyValue.ToDictionary(data => data.Key, data => new KeyData{Group = data.Group, LanguagesData = data.DictionaryLanguageValue});
            
            Debug.Log("JSON loaded");
        }
        
        private static void UpdateValueInDictionary(string key, string newValue, Data.LANGUAGES language)
        {
            var languagesData = _dictionary[key].LanguagesData;
            if (languagesData == null)
                languagesData = new Dictionary<Data.LANGUAGES, string>();
            else
                languagesData.Remove(language);

            languagesData.Add(language, newValue);
            var keyData = new KeyData
            {
                Group = _dictionary[key].Group,
                LanguagesData = languagesData
            };

            _dictionary.Remove(key);
            _dictionary.Add(key, keyData);
        }

        #region Save-Load Methods

        private async Task SaveFile<T>(T fileToSave, string path)
        {
            var dataToSave = _serializer.Serialize(fileToSave);
            try
            {
                await using StreamWriter writer = new StreamWriter(path);
                await writer.WriteAsync(dataToSave);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private async Task<T> LoadFile<T>(string path)
        {
            if (!File.Exists(path)) File.Create(path);

            try
            {
                using StreamReader reader = new StreamReader(path);
                return _serializer.Deserialize<T>(await reader.ReadToEndAsync());
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return default;
        }

        #endregion
        
        private void PrintDictionary()
        {
            foreach (var l in _dictionary)
            {
                Debug.Log($"---------------------------");
                Debug.Log($"{l.Key} {l.Value.Group}");
                // foreach (var keyValuePair in l.Value.LanguagesData)
                // {
                //     Debug.Log($"{keyValuePair.Key} : {keyValuePair.Value}");
                // }
            }
        }

        #endregion
    }
}