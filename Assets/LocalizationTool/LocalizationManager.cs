using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LocalizationTool.Controller;
using LocalizationTool.Data;
using LocalizationTool.Data.Binary;
using LocalizationTool.Data.Json;
using LocalizationTool.Editors;
using LocalizationTool.Serializer;
using UnityEditor;
using UnityEngine;

namespace LocalizationTool.Manager
{
    public class LocalizationManager
    {
        #region PUBLIC VARIABLES

        public static LocalizationManager Instance => _instance ??= new LocalizationManager();

        public static List<LanguagesData.LanguageTuple> OrderedLanguages => _languagesData.OrderedLanguages ?? new List<LanguagesData.LanguageTuple>();
        public static List<string> ActiveLanguages => _languagesData.Languages ?? new List<string>();
        public static List<CategoriesData.CategoryTuple> OrderedCategories => _categoriesData?.OrderedCategories ?? new List<CategoriesData.CategoryTuple>();
        public static List<string> Categories => _categoriesData?.Categories ?? new List<string>();
        public static ConfigurationData Configuration => _configurationData ?? new ConfigurationData();

        public static Dictionary<string, KeyData> Dictionary => _dynamicDictionary ?? new Dictionary<string, KeyData>();
        public string CurrentLanguageInDictionarySection { get; set; }
        public int CurrentToolbarLanguageIndex => _languagesData.Languages.IndexOf(CurrentLanguageInDictionarySection);
        public static string DefaultCategory => _categoriesData.DefaultCategory;
        public static bool IsDataLoaded { get; private set; }
        public Texture2D YellowIcon { get; private set; }

        #region Actions

        public Action<string> OnDefaultCategoryUpdate { get; set; }

        #endregion
        
        
        #endregion

        #region PRIVATE VARIABLES

        private static LocalizationManager _instance;
        private ISerializerService _serializerJson;
        private ISerializerService _serializerBinary;

        #region JSON Data

        private const string JSON_DICTIONARY_PATH = "Assets/LocalizationTool/Data/DoNotTouch/LocalizationDataLanguage.json";
        private static DictionaryData _dictionaryData;

        #endregion

        #region BINARY Data

        private const string BINARY_LANGUAGES_PATH = "Assets/LocalizationTool/Data/DoNotTouch/LocalizationLanguages.bin";
        private static LanguagesData _languagesData;

        private const string BINARY_CATEGORIES_PATH = "Assets/LocalizationTool/Data/DoNotTouch/LocalizationCategories.bin";
        private static CategoriesData _categoriesData;

        private const string BINARY_CONFIGURATION_PATH = "Assets/LocalizationTool/Data/DoNotTouch/LocalizationConfiguration.bin";
        private static ConfigurationData _configurationData;

        #endregion

        private static bool _isInitialized;

        private static Dictionary<string, KeyData> _dynamicDictionary;

        #endregion

        #region PUBLIC METHODS

        #region JSON

        #region DICTIONARY

        public async void AddNewKey(string key, string category, DictionaryEditor editor = null)
        {
            if (key.Equals(""))
            {
                if (editor != null && editor.AddValueFeedbackLabelText != "") return;
                Log("Key cannot be an empty value");
                editor.AddValueFeedbackLabelText = "Key cannot be an empty value";
                return;
            }

            if (_dynamicDictionary.ContainsKey(key))
            {
                if (editor != null && editor.AddValueFeedbackLabelText != "") return;
                Log($"Key '{key}' already exists");
                if (editor != null) editor.AddValueFeedbackLabelText = $"Key '{key}' already exists";
                return;
            }

            if (editor != null) editor.AddValueFeedbackLabelText = $"Key '{key}' added correctly";

            var interDic = ActiveLanguages.ToDictionary(language => language, _ => "");
            var interList = ActiveLanguages.Select(l => new LanguageValue { Language = l, Value = "" }).ToList();
            _dynamicDictionary.Add(key, new KeyData { Category = category, LanguagesData = interDic });

            //Add key to JSON file
            _dictionaryData.ListDictionaryKeyCategoryLanguages.Add(new KeyCategoryLanguageValues(key, category, interList));

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            Log($"Key '{key}' added correctly");
        }

        public async Task ChangeValue(string key, string newValue)
        {
            if (_dynamicDictionary[key].LanguagesData != null && _dynamicDictionary[key].LanguagesData[CurrentLanguageInDictionarySection].Equals(newValue)) return;

            UpdateValueInDictionary(key, newValue, CurrentLanguageInDictionarySection);

            //Change JSON file 
            _dictionaryData.ListDictionaryKeyCategoryLanguages.First(x => x.Key == key).UpdateValue(CurrentLanguageInDictionarySection, newValue);

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);
        }

        public async void ChangeCategory(string key, string newCategory)
        {
            if (_dynamicDictionary[key].Category == newCategory) return;

            var oldData = _dynamicDictionary[key].LanguagesData;
            _dynamicDictionary.Remove(key);
            _dynamicDictionary.Add(key, new KeyData { Category = newCategory, LanguagesData = oldData });

            //Change JSON file 
            var data = _dictionaryData.ListDictionaryKeyCategoryLanguages.First(x => x.Key == key);
            data.Category = newCategory;

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            Log($"Key '{key}' changed category to '{newCategory}' correctly");
        }

        public async void RemoveKey(string key)
        {
            _dynamicDictionary.Remove(key);

            //Change JSON file 
            var data = _dictionaryData.ListDictionaryKeyCategoryLanguages.First(x => x.Key == key);
            _dictionaryData.ListDictionaryKeyCategoryLanguages.Remove(data);

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);
        }

        #endregion

        #endregion

        #region BINARY

        #region LANGUAGE

        public async void AddNewLanguage(string newLanguage, LanguagesEditor editor = null, bool showLogs = true)
        {
            if (newLanguage.Equals(""))
            {
                if (!showLogs) return;
                if (editor != null && editor.AddLanguageFeedbackLabelText != "") return;
                Log("Language cannot be an empty value");
                if (editor != null) editor.AddLanguageFeedbackLabelText = "Language cannot be an empty value";
                return;
            }

            if (_languagesData.Contains(newLanguage))
            {
                if (!showLogs) return;
                if (editor != null && editor.AddLanguageFeedbackLabelText != "") return;
                Log($"Language '{newLanguage}' already exists");
                if (editor != null) editor.AddLanguageFeedbackLabelText = $"Language '{newLanguage}' already exists";
                return;
            }

            if (editor != null) editor.AddLanguageFeedbackLabelText = $"Language '{newLanguage}' added correctly";

            //Add language to Binary 
            _languagesData.Add(newLanguage);

            if (_languagesData.Count() == 1)
            {
                _languagesData.FavouriteLanguage = newLanguage;
                CurrentLanguageInDictionarySection = newLanguage;
            }

            await SaveFile(_languagesData, BINARY_LANGUAGES_PATH, _serializerBinary);

            //Update JSON
            foreach (var keyValue in _dictionaryData.ListDictionaryKeyCategoryLanguages)
            {
                keyValue.AddNewLanguage(newLanguage);
            }

            //Update dynamic Dictionary
            _dynamicDictionary = _dictionaryData.ListDictionaryKeyCategoryLanguages.ToDictionary(data => data.Key, data => new KeyData { Category = data.Category, LanguagesData = data.DictionaryLanguageValue });

            if (showLogs) Log($"Language '{newLanguage}' added correctly");
        }

        public async void RemoveLanguage(string language)
        {
            //Change BINARY file 
            _languagesData.Remove(language);
            if (_languagesData.Count() == 1)
            {
                _languagesData.FavouriteLanguage = _languagesData.Languages[0];
            }

            await SaveFile(_languagesData, BINARY_LANGUAGES_PATH, _serializerBinary);

            //Update JSON
            foreach (var keyValue in _dictionaryData.ListDictionaryKeyCategoryLanguages)
            {
                await keyValue.RemoveLanguage(language);
            }

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            //Update dynamic Dictionary
            _dynamicDictionary = _dictionaryData.ListDictionaryKeyCategoryLanguages.ToDictionary(data => data.Key, data => new KeyData { Category = data.Category, LanguagesData = data.DictionaryLanguageValue });
        }

        public async void ChangeLanguageValue(string oldLanguageName, string newLanguageName)
        {
            if (newLanguageName.Length == 0) return; //TODO: mostrar warning de alguna manera
            if (_languagesData.Contains(oldLanguageName)) return; //the new value is the same

            //Change BINARY file 
            _languagesData.ChangeName(oldLanguageName, newLanguageName);

            await SaveFile(_languagesData, BINARY_LANGUAGES_PATH, _serializerBinary);

            //Update JSON
            foreach (var keyValue in _dictionaryData.ListDictionaryKeyCategoryLanguages)
            {
                await keyValue.UpdateLanguageName(oldLanguageName, newLanguageName);
            }

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            //Update dynamic Dictionary
            _dynamicDictionary = _dictionaryData.ListDictionaryKeyCategoryLanguages.ToDictionary(data => data.Key, data => new KeyData { Category = data.Category, LanguagesData = data.DictionaryLanguageValue });

            //Log($"Language '{oldLanguageName}' update to '{newLanguageName}' correctly");
        }

        public async void ChangeFavoriteLanguage(string newLanguage)
        {
            //Change BINARY file 
            _languagesData.FavouriteLanguage = newLanguage;

            await SaveFile(_languagesData, BINARY_LANGUAGES_PATH, _serializerBinary);
        }

        public async void ChangeLanguageIndex(int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex) return; //the new index is the same

            //Change binary file
            var language = _languagesData.ChangeIndex(oldIndex, newIndex);

            await SaveFile(_languagesData, BINARY_LANGUAGES_PATH, _serializerBinary);

            Log($"Language '{language}' update to index '{newIndex}' correctly");

            //GUI.FocusControl(null);
        }

        public static bool IsFavouriteLanguage(string language)
        {
            return _languagesData.FavouriteLanguage.Equals(language);
        }

        #endregion

        #region CATEGORY

        public async void AddNewCategory(string newCategory, CategoriesEditor editor = null, bool showEditorLogs = true)
        {
            // Empty value
            if (newCategory.Equals(""))
            {
                if (!showEditorLogs) return;
                if (editor != null && editor.AddCategoryFeedbackLabelText != "") return;
                Log("Category cannot be an empty value");
                if (editor != null) editor.AddCategoryFeedbackLabelText = "Category cannot be an empty value";
                return;
            }

            // Already in data
            if (_categoriesData.Contains(newCategory))
            {
                if (!showEditorLogs) return;
                if (editor != null && editor.AddCategoryFeedbackLabelText != "") return;
                Log($"Category '{newCategory}' already exists");
                if (editor != null) editor.AddCategoryFeedbackLabelText = $"Category '{newCategory}' already exists";
                return;
            }

            if (editor != null) editor.AddCategoryFeedbackLabelText = $"Category '{newCategory}' added correctly";

            //Add language to Binary 
            _categoriesData.Add(newCategory);

            await SaveFile(_categoriesData, BINARY_CATEGORIES_PATH, _serializerBinary);

            if (showEditorLogs) Log($"Category '{newCategory}' added correctly");
        }

        public async void RemoveCategory(string category)
        {
            //Change BINARY file 
            _categoriesData.Remove(category);

            await SaveFile(_categoriesData, BINARY_CATEGORIES_PATH, _serializerBinary);

            //Update JSON
            foreach (var keyValue in _dictionaryData.ListDictionaryKeyCategoryLanguages)
            {
                await keyValue.RemoveCategory(category);
            }

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            //Update dynamic Dictionary
            _dynamicDictionary = _dictionaryData.ListDictionaryKeyCategoryLanguages
                .ToDictionary(data => data.Key, data => new KeyData { Category = data.Category, LanguagesData = data.DictionaryLanguageValue });
        }

        public async void ChangeCategoryName(string oldCategoryName, string newCategoryName)
        {
            if (_categoriesData.Contains(newCategoryName)) return; //the new value is the same

            //Change BINARY file 
            _categoriesData.ChangeName(oldCategoryName, newCategoryName);

            await SaveFile(_categoriesData, BINARY_CATEGORIES_PATH, _serializerBinary);

            //Update JSON
            foreach (var keyValue in _dictionaryData.ListDictionaryKeyCategoryLanguages)
            {
                await keyValue.UpdateCategoryName(oldCategoryName, newCategoryName);
            }

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            //Update dynamic Dictionary
            _dynamicDictionary = _dictionaryData.ListDictionaryKeyCategoryLanguages
                .ToDictionary(data => data.Key, data => new KeyData { Category = data.Category, LanguagesData = data.DictionaryLanguageValue });

            //Log($"Category '{oldCategoryName}' update to '{newCategoryName}' correctly");
        }

        public async void ChangeCategoryIndex(int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex) return; //the new index is the same

            //Change binary file
            var category = _categoriesData.ChangeIndex(oldIndex, newIndex);

            await SaveFile(_categoriesData, BINARY_CATEGORIES_PATH, _serializerBinary);

            Log($"Category '{category}' update to index '{newIndex}' correctly");

            //GUI.FocusControl(null);
        }

        public async void ChangeDefaultCategory(string newCategory)
        {
            //Change BINARY file 
            _categoriesData.DefaultCategory = newCategory;
            _categoriesData.ChangeIndex(newCategory, 0);

            await SaveFile(_categoriesData, BINARY_CATEGORIES_PATH, _serializerBinary);
            
            OnDefaultCategoryUpdate?.Invoke(newCategory);
        }

        public static bool IsDefaultCategory(string category)
        {
            return _categoriesData.DefaultCategory.Equals(category);
        }

        #endregion

        #region CONFIGURATION

        public async void UpdateDeleteConfirmation(bool newValue, Enums.GUI_WINDOW window)
        {
            switch (window)
            {
                case Enums.GUI_WINDOW.Dictionary:
                    if (_configurationData.DictionaryDeleteConfirmation == newValue) return;
                    _configurationData.DictionaryDeleteConfirmation = newValue;
                    break;
                case Enums.GUI_WINDOW.Languages:
                    if (_configurationData.LanguageDeleteConfirmation == newValue) return;
                    _configurationData.LanguageDeleteConfirmation = newValue;
                    break;
                case Enums.GUI_WINDOW.Categories:
                    if (_configurationData.CategoryDeleteConfirmation == newValue) return;
                    _configurationData.CategoryDeleteConfirmation = newValue;
                    break;
            }

            await SaveFile(_configurationData, BINARY_CONFIGURATION_PATH, _serializerBinary);
        }

        public async void UpdateSearchType(int searchTypeIndex)
        {
            if (_configurationData.SearchTypeIndex == searchTypeIndex) return;

            _configurationData.SearchTypeIndex = searchTypeIndex;
            await SaveFile(_configurationData, BINARY_CONFIGURATION_PATH, _serializerBinary);
        }

        public async void UpdateShowLog(bool showLogs)
        {
            if (_configurationData.ShowLogsInConsole == showLogs) return;

            _configurationData.ShowLogsInConsole = showLogs;
            await SaveFile(_configurationData, BINARY_CONFIGURATION_PATH, _serializerBinary);
        }

        #endregion

        #endregion

        #region IMPORT

        public async Task ImportLanguage(string language)
        {
            if (_languagesData.Contains(language)) return;

            //Add language to Binary 
            _languagesData.Add(language);

            if (_languagesData.Count() == 1)
            {
                _languagesData.FavouriteLanguage = language;
                CurrentLanguageInDictionarySection = language;
            }

            await SaveFile(_languagesData, BINARY_LANGUAGES_PATH, _serializerBinary);

            //Update JSON
            foreach (var keyValue in _dictionaryData.ListDictionaryKeyCategoryLanguages)
            {
                keyValue.AddNewLanguage(language);
            }

            //Update JSON
            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            //Update dynamic Dictionary
            _dynamicDictionary = _dictionaryData.ListDictionaryKeyCategoryLanguages.ToDictionary(data => data.Key, data => new KeyData { Category = data.Category, LanguagesData = data.DictionaryLanguageValue });

            Log($"Language {language} imported");
        }

        public async Task ImportKey(string key, string category, Dictionary<string, string> values)
        {
            if (key.Equals("")) return;
            Log($"Key '{key}' - '{category}' : {string.Join(" | ", values)} imported");
            if (_dynamicDictionary.ContainsKey(key))
            {
                if (_dynamicDictionary[key].Category != category)
                {
                    //Change category
                    if (!_categoriesData.Contains(category)) AddNewCategory(category.Equals("") ? "None" : category, null, false);

                    _dictionaryData.UpdateCategoryName(key, category);
                }

                //Update values
                foreach (var (language, value) in values)
                {
                    _dictionaryData.UpdateLanguageValue(key, language, value);
                }
            }
            else
            {
                //Add new key

                if (!_categoriesData.Contains(category)) AddNewCategory(category.Equals("") ? "None" : category, null, false);

                var interList = values.Select(languageValuePair => new LanguageValue { Language = languageValuePair.Key, Value = languageValuePair.Value }).ToList();

                //Add key to JSON file
                _dictionaryData.AddNewKeyCategoryLanguage(new KeyCategoryLanguageValues(key, category, interList));
            }

            //Update JSON
            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            //Update dynamic Dictionary
            _dynamicDictionary = _dictionaryData.ListDictionaryKeyCategoryLanguages.ToDictionary(data => data.Key, data => new KeyData { Category = data.Category, LanguagesData = data.DictionaryLanguageValue });
        }

        public async Task ImportKey(string key, string category, string language, string value)
        {
            if (key.Equals("")) return;
            Log($"Key '{key}' - '{category}' - '{language}' : '{value}' imported");

            ImportLanguage(language);

            if (_dynamicDictionary.ContainsKey(key))
            {
                AddNewCategory(category.Equals("") ? _categoriesData.DefaultCategory : category, null, false);
                
                if (_dynamicDictionary[key].Category != category)
                {
                    //Change category
                    _dictionaryData.UpdateCategoryName(key, category);
                }

                //Update value
                _dictionaryData.UpdateLanguageValue(key, language, value);
            }
            else
            {
                //Add new key

                AddNewCategory(category.Equals("") ? _categoriesData.DefaultCategory : category, null, false);

                var interDic = ActiveLanguages.ToDictionary(l => l, _ => "");
                interDic[language] = value;
                var interList = interDic.Select(l => new LanguageValue { Language = l.Key, Value = l.Value }).ToList();

                _dynamicDictionary.Add(key, new KeyData { Category = category, LanguagesData = interDic });

                //Add key to JSON file
                _dictionaryData.ListDictionaryKeyCategoryLanguages.Add(new KeyCategoryLanguageValues(key, category, interList));
            }

            //Update JSON
            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            //Update dynamic Dictionary
            _dynamicDictionary = _dictionaryData.ListDictionaryKeyCategoryLanguages.ToDictionary(data => data.Key, data => new KeyData { Category = data.Category, LanguagesData = data.DictionaryLanguageValue });
        }

        public bool ExistCategory(string category)
        {
            return _categoriesData.Contains(category);
        }

        #endregion

        #region RefreshData

        public void RefreshDictionaryData()
        {
            CreateFiles();
            LoadDictionaryDataFromJSON();
        }

        public void RefreshLanguagesData()
        {
            CreateFiles();
            LoadLanguagesDataFromBINARY();
        }

        public void RefreshCategoriesData()
        {
            CreateFiles();
            LoadCategoriesDataFromBINARY();
        }

        #endregion

        public static void Log(string log)
        {
            try
            {
                if (_configurationData.ShowLogsInConsole) Debug.Log(log);
            }
            catch (Exception)
            {
                Debug.Log(log);
            }
        }

        #endregion

        #region PRIVATE METHODS

        private LocalizationManager()
        {
#pragma warning disable CS4014
            //TODO: te if this is necessary => Init();
#pragma warning restore CS4014
        }

        public async Task Init()
        {
            if (_isInitialized) return; // Just the first call
            _isInitialized = true;
            
            CreateFiles();
            _serializerJson = new UnityJsonSerializer();
            _serializerBinary = new BinarySerializer();
#pragma warning disable CS4014
            LoadConfigurationDataFromBINARY();
#pragma warning restore CS4014
            await LoadDictionaryDataFromJSON();
            await LoadLanguagesDataFromBINARY();
            await LoadCategoriesDataFromBINARY();
            YellowIcon = GetColoredIcon("d_Favorite", Color.yellow);

            IsDataLoaded = true;
            Log("Manager initialized");
        }

        private static void CreateFiles()
        {
            if (!File.Exists(JSON_DICTIONARY_PATH)) File.Create(JSON_DICTIONARY_PATH);
            if (!File.Exists(BINARY_LANGUAGES_PATH)) File.Create(BINARY_LANGUAGES_PATH);
            if (!File.Exists(BINARY_CATEGORIES_PATH)) File.Create(BINARY_CATEGORIES_PATH);
            if (!File.Exists(BINARY_CONFIGURATION_PATH)) File.Create(BINARY_CONFIGURATION_PATH);
        }

        private async Task LoadDictionaryDataFromJSON()
        {
            _dictionaryData = new DictionaryData();
            var aux = await LoadFile<DictionaryData>(JSON_DICTIONARY_PATH, _serializerJson);
            if (aux != null) _dictionaryData = aux;

            if (_dynamicDictionary != null) _dynamicDictionary.Clear();
            else _dynamicDictionary = new Dictionary<string, KeyData>();

            if (_dictionaryData != null)
                _dynamicDictionary = _dictionaryData.ListDictionaryKeyCategoryLanguages.ToDictionary(data => data.Key, data => new KeyData { Category = data.Category, LanguagesData = data.DictionaryLanguageValue });

            Log("Dictionary loaded");
        }

        private async Task LoadLanguagesDataFromBINARY()
        {
            // _languagesData ??= new LanguagesData();
            _languagesData = await LoadFile<LanguagesData>(BINARY_LANGUAGES_PATH, _serializerBinary);

            if (_languagesData == null)
            {
                _languagesData = new LanguagesData
                {
                    FavouriteLanguage = "English"
                };
                _languagesData.Add("English");
                await SaveFile(_languagesData, BINARY_LANGUAGES_PATH, _serializerBinary);
            }

            CurrentLanguageInDictionarySection = _languagesData.FavouriteLanguage;
            LocalizationToolController.Instance.ActiveLanguage = _languagesData.FavouriteLanguage;

            Log("Languages loaded");
        }

        private async Task LoadCategoriesDataFromBINARY()
        {
            _categoriesData = await LoadFile<CategoriesData>(BINARY_CATEGORIES_PATH, _serializerBinary);

            if (_categoriesData == null)
            {
                _categoriesData = new CategoriesData
                {
                    DefaultCategory = "None"
                };
                _categoriesData.Add("None");
                await SaveFile(_categoriesData, BINARY_CATEGORIES_PATH, _serializerBinary);
            }

            Log("Categories loaded");
        }

        private async Task LoadConfigurationDataFromBINARY()
        {
            //_configurationData ??= new ConfigurationData();
            _configurationData = await LoadFile<ConfigurationData>(BINARY_CONFIGURATION_PATH, _serializerBinary);

            if (_configurationData == null)
            {
                _configurationData = new ConfigurationData
                {
                    DictionaryDeleteConfirmation = true,
                    CategoryDeleteConfirmation = true,
                    LanguageDeleteConfirmation = true,
                    SearchTypeIndex = 0,
                    ShowLogsInConsole = true
                };
                await SaveFile(_configurationData, BINARY_CONFIGURATION_PATH, _serializerBinary);
            }

            Log("Configuration loaded");
        }

        #region UPDATES

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

        #endregion

        #region SAVE-LOAD METHODS

        private static async Task SaveFile<T>(T fileToSave, string path, ISerializerService serializer)
        {
            var dataToSave = serializer.Serialize(fileToSave);
            try
            {
                await using var writer = new StreamWriter(path);
                await writer.WriteAsync(dataToSave);
            }
            catch (Exception e)
            {
                if (e is not IOException) Debug.LogError($"{path}: {e}");
            }
        }

        public static async Task<T> LoadFile<T>(string path, ISerializerService serializer)
        {
            if (!File.Exists(path)) File.Create(path);

            try
            {
                using var reader = new StreamReader(path);
                return serializer.Deserialize<T>(await reader.ReadToEndAsync());
            }
            catch (Exception e)
            {
                Debug.LogError($"{path}: {e}");
            }

            return default;
        }

        public static async void SaveFile(string path, string fileContent, string title, string message, string okMessage)
        {
            await File.WriteAllTextAsync(path, fileContent);

            EditorUtility.DisplayDialog(title, message, okMessage);
        }

        #endregion

        private static Texture2D GetColoredIcon(string iconName, Color color)
        {
            var originalTexture = EditorGUIUtility.IconContent(iconName).image as Texture2D;
            if (originalTexture == null)
            {
                return null;
            }

            var coloredTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);

            Graphics.CopyTexture(originalTexture, coloredTexture);

            for (var y = 0; y < coloredTexture.height; y++)
            {
                for (var x = 0; x < coloredTexture.width; x++)
                {
                    var originalColor = coloredTexture.GetPixel(x, y);
                    var newColor = originalColor * color;
                    coloredTexture.SetPixel(x, y, newColor);
                }
            }

            coloredTexture.Apply();
            return coloredTexture;
        }

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