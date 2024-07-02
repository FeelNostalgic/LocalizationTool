using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LocalizationTool.Data;
using LocalizationTool.Editor;
using UnityEngine;

namespace LocalizationTool
{
    public class LocalizationManager
    {
        #region PUBLIC VARIABLES

        public static LocalizationManager Instance => _instance ??= new LocalizationManager();

        public static List<string> ActiveLanguages => _languagesData.Languagues;
        public static Dictionary<string, KeyData> Dictionary => _dynamicDictionary;
        public string CurrentLanguageInDictionarySection { get; set; }
        public int CurrentToolbarLanguageIndex => _languagesData.Languagues.IndexOf(CurrentLanguageInDictionarySection);
        public string FavouriteLanguage => _languagesData.FavouriteLanguage;

        #endregion

        #region PRIVATE VARIABLES

        private static LocalizationManager _instance;
        private ISerializerService _serializerJson;
        private ISerializerService _serializerBinary;

        #region JSON Data

        private const string JSON_DICTIONARY_PATH = "Assets/LocalizationTool/Data/LocalizationDataLanguage.json";
        private static DictionaryData _dictionaryData;

        #endregion

        #region BINARY Data

        private const string BINARY_LANGUAGES_PATH = "Assets/LocalizationTool/Data/LocalizationLanguages.bin";
        private static LanguagesData _languagesData;

        #endregion

        private bool _isInitialized;

        private static Dictionary<string, KeyData> _dynamicDictionary;

        #endregion

        #region PUBLIC METHODS

        #region JSON

        #region DICTIONARY

        public async void AddNewKey(string key, Enums.GROUPS group, DictionaryEditor editor)
        {
            if (key.Equals(""))
            {
                editor.AddValueFeedbackLabelText = "Key cannot be an empty value";
                return;
            }

            if (_dynamicDictionary.ContainsKey(key))
            {
                editor.AddValueFeedbackLabelText = $"{key} already exists";
                return;
            }

            var interDic = ActiveLanguages.ToDictionary(language => language, _ => "");
            var interList = ActiveLanguages.Select(l => new LanguageValue { Language = l, Value = "" }).ToList();
            _dynamicDictionary.Add(key, new KeyData { Category = group, LanguagesData = interDic });

            //Add key to JSON file
            _dictionaryData.ListDictionaryKeyValue.Add(new KeyValue(key, group, interList));

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            editor.AddValueFeedbackLabelText = $"{key} added correctly";
        }

        public async void ChangeValue(string key, string newValue, string language)
        {
            if (_dynamicDictionary[key].LanguagesData != null && _dynamicDictionary[key].LanguagesData[language].Equals(newValue)) return; //value is not modified

            UpdateValueInDictionary(key, newValue, language);

            //Change JSON file 
            _dictionaryData.ListDictionaryKeyValue.First(x => x.Key == key).UpdateValue(language, newValue);

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);
        }

        public async void ChangeCategory(string key, Enums.GROUPS newCategory)
        {
            if (_dynamicDictionary[key].Category == newCategory) return;

            var oldData = _dynamicDictionary[key].LanguagesData;
            _dynamicDictionary.Remove(key);
            _dynamicDictionary.Add(key, new KeyData { Category = newCategory, LanguagesData = oldData });

            //Change JSON file 
            var data = _dictionaryData.ListDictionaryKeyValue.First(x => x.Key == key);
            data.Category = newCategory;

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);
        }

        public async void RemoveKey(string key)
        {
            _dynamicDictionary.Remove(key);

            //Change JSON file 
            var data = _dictionaryData.ListDictionaryKeyValue.First(x => x.Key == key);
            _dictionaryData.ListDictionaryKeyValue.Remove(data);

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);
        }

        #endregion

        #endregion

        #region LANGUAGE

        public async void AddNewLanguage(string newLanguage, LanguagesEditor editor)
        {
            if (newLanguage.Equals(""))
            {
                editor.AddLanguageFeedbackLabelText = "Language cannot be an empty value";
                return;
            }

            if (ActiveLanguages.Exists(l => l.Equals(newLanguage)))
            {
                editor.AddLanguageFeedbackLabelText = $"{newLanguage} already exists";
                return;
            }

            //Add language to Binary 
            _languagesData.Languagues.Add(newLanguage);

            if (_languagesData.Languagues.Count == 1)
            {
                _languagesData.FavouriteLanguage = newLanguage;
                CurrentLanguageInDictionarySection = newLanguage;
            }

            await SaveFile(_languagesData, BINARY_LANGUAGES_PATH, _serializerBinary);
            
            editor.AddLanguageFeedbackLabelText = $"{newLanguage} added correctly";
        }

        public async void RemoveLanguage(string language)
        {
            //Change BINARY file 
            _languagesData.Languagues.Remove(language);

            await SaveFile(_languagesData, BINARY_LANGUAGES_PATH, _serializerBinary);

            //TODO: delete data from JSON
            foreach (var keyValue in _dictionaryData.ListDictionaryKeyValue)
            {
                await keyValue.RemoveLanguage(language);
            }

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            //Update dynamic Dictionary
            _dynamicDictionary = _dictionaryData.ListDictionaryKeyValue.ToDictionary(data => data.Key, data => new KeyData { Category = data.Category, LanguagesData = data.DictionaryLanguageValue });
        }

        public async void ChangeLanguageValue(string oldLanguage, string newLanguage)
        {
            if (_languagesData.Languagues.FirstOrDefault(l => l.Equals(newLanguage)) != default) return; //the new value is the same

            //Change BINARY file 
            _languagesData.Languagues.Remove(oldLanguage);
            _languagesData.Languagues.Add(newLanguage);

            await SaveFile(_languagesData, BINARY_LANGUAGES_PATH, _serializerBinary);
        }

        public async void ChangeFavoriteLanguage(string newLanguage)
        {
            //Change BINARY file 
            _languagesData.FavouriteLanguage = newLanguage;
            
            await SaveFile(_languagesData, BINARY_LANGUAGES_PATH, _serializerBinary);
        }

        #endregion

        public void RefreshDictionaryData()
        {
            LoadDictionaryDataFromJSON();
        }

        public void RefreshLanguagesData()
        {
            LoadLanguagesDataFromBINARY();
        }

        #endregion

        #region PRIVATE METHODS

        private LocalizationManager()
        {
            Init();
        }

        public void Init()
        {
            if (_isInitialized) return;
            Debug.Log("Manager initialized");
            _isInitialized = true;
            _serializerJson = new UnityJsonSerializer();
            _serializerBinary = new BinarySerializer();
            LoadDictionaryDataFromJSON();
            LoadLanguagesDataFromBINARY();
        }

        private async void LoadDictionaryDataFromJSON()
        {
            _dictionaryData ??= new DictionaryData();
            var aux = await LoadFile<DictionaryData>(JSON_DICTIONARY_PATH, _serializerJson);
            if (aux != null) _dictionaryData = aux;

            if (_dynamicDictionary != null) _dynamicDictionary.Clear();
            else _dynamicDictionary = new Dictionary<string, KeyData>();

            if (_dictionaryData != null) _dynamicDictionary = _dictionaryData.ListDictionaryKeyValue.ToDictionary(data => data.Key, data => new KeyData { Category = data.Category, LanguagesData = data.DictionaryLanguageValue });

            Debug.Log("JSON Dictionary loaded");
        }

        private async void LoadLanguagesDataFromBINARY()
        {
            _languagesData ??= new LanguagesData();
            _languagesData = await LoadFile<LanguagesData>(BINARY_LANGUAGES_PATH, _serializerBinary);

            CurrentLanguageInDictionarySection = _languagesData.FavouriteLanguage;

            Debug.Log("BINARY Languages loaded");
        }

        private static void UpdateValueInDictionary(string key, string newValue, string language)
        {
            var languagesData = _dynamicDictionary[key].LanguagesData;
            if (languagesData == null)
                languagesData = new Dictionary<string, string>();
            else
                languagesData.Remove(language);

            languagesData.Add(language, newValue);
            var keyData = new KeyData
            {
                Category = _dynamicDictionary[key].Category,
                LanguagesData = languagesData
            };

            _dynamicDictionary.Remove(key);
            _dynamicDictionary.Add(key, keyData);
        }

        #region SAVE-LOAD METHODS

        private async Task SaveFile<T>(T fileToSave, string path, ISerializerService serializer)
        {
            var dataToSave = serializer.Serialize(fileToSave);
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

        private async Task<T> LoadFile<T>(string path, ISerializerService serializer)
        {
            if (!File.Exists(path)) File.Create(path);

            try
            {
                using StreamReader reader = new StreamReader(path);
                return serializer.Deserialize<T>(await reader.ReadToEndAsync());
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
            foreach (var l in _dynamicDictionary)
            {
                Debug.Log($"---------------------------");
                Debug.Log($"{l.Key} {l.Value.Category}");
                // foreach (var keyValuePair in l.Value.LanguagesData)
                // {
                //     Debug.Log($"{keyValuePair.Key} : {keyValuePair.Value}");
                // }
            }
        }

        #endregion
    }
}