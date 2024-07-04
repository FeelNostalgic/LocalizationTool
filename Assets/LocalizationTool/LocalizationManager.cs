using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LocalizationTool.Controller;
using LocalizationTool.Data;
using LocalizationTool.Editors;
using UnityEditor;
using UnityEngine;

namespace LocalizationTool.Manager
{
    public class LocalizationManager
    {
        #region PUBLIC VARIABLES

        public static LocalizationManager Instance => _instance ??= new LocalizationManager();

        public static List<string> ActiveLanguages => _languagesData.Languagues ?? new List<string>();
        public static List<string> Categories => _categoriesData?.Categories ?? new List<string>();
        public static Dictionary<string, KeyData> Dictionary => _dynamicDictionary ?? new Dictionary<string, KeyData>();
        public string CurrentLanguageInDictionarySection { get; set; }
        public int CurrentToolbarLanguageIndex => _languagesData.Languagues.IndexOf(CurrentLanguageInDictionarySection);
        public string FavouriteLanguage => _languagesData.FavouriteLanguage;
        public bool IsInitalized => _isInitialized;
        public Texture2D YellowIcon { get; set; }
        
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

        private const string BINARY_CATEGORIES_PATH = "Assets/LocalizationTool/Data/LocalizationCategories.bin";
        private static CategoriesData _categoriesData;

        #endregion

        private bool _isInitialized;

        private static Dictionary<string, KeyData> _dynamicDictionary;

        #endregion

        #region PUBLIC METHODS

        #region JSON

        #region DICTIONARY

        public async void AddNewKey(string key, string group, DictionaryEditor editor)
        {
            if (key.Equals(""))
            {
                if (editor.AddValueFeedbackLabelText != "") return;
                editor.AddValueFeedbackLabelText = "Key cannot be an empty value";
                return;
            }

            if (_dynamicDictionary.ContainsKey(key))
            {
                if (editor.AddValueFeedbackLabelText != "") return;
                editor.AddValueFeedbackLabelText = $"Key '{key}' already exists";
                return;
            }
            
            editor.AddValueFeedbackLabelText = $"Key '{key}'added correctly";
            
            var interDic = ActiveLanguages.ToDictionary(language => language, _ => "");
            var interList = ActiveLanguages.Select(l => new LanguageValue { Language = l, Value = "" }).ToList();
            _dynamicDictionary.Add(key, new KeyData { Category = group, LanguagesData = interDic });

            //Add key to JSON file
            _dictionaryData.ListDictionaryKeyCategoryLanguages.Add(new KeyCategoryLanguage(key, group, interList));

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);
            
            Debug.Log($"Key '{key}' added correctly");
        }

        public async void ChangeValue(string key, string newValue, string language)
        {
            if (_dynamicDictionary[key].LanguagesData != null && _dynamicDictionary[key].LanguagesData[language].Equals(newValue)) return; //value is not modified

            UpdateValueInDictionary(key, newValue, language);

            //Change JSON file 
            _dictionaryData.ListDictionaryKeyCategoryLanguages.First(x => x.Key == key).UpdateValue(language, newValue);

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

        public async void AddNewLanguage(string newLanguage, LanguagesEditor editor)
        {
            if (newLanguage.Equals(""))
            {
                if(editor.AddLanguageFeedbackLabelText != "") return;
                editor.AddLanguageFeedbackLabelText = "Language cannot be an empty value";
                return;
            }

            if (ActiveLanguages.Exists(l => l.Equals(newLanguage)))
            {
                if(editor.AddLanguageFeedbackLabelText != "") return;
                editor.AddLanguageFeedbackLabelText = $"Language '{newLanguage}' already exists";
                return;
            }

            editor.AddLanguageFeedbackLabelText = $"Language '{newLanguage}' added correctly";
            
            //Add language to Binary 
            _languagesData.Languagues.Add(newLanguage);

            if (_languagesData.Languagues.Count == 1)
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
        }

        public async void RemoveLanguage(string language)
        {
            //Change BINARY file 
            _languagesData.Languagues.Remove(language);

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
            if (_languagesData.Languagues.FirstOrDefault(l => l.Equals(newLanguageName)) != default) return; //the new value is the same
            
            //Change BINARY file 
            _languagesData.Languagues.Remove(oldLanguageName);
            _languagesData.Languagues.Add(newLanguageName);

            await SaveFile(_languagesData, BINARY_LANGUAGES_PATH, _serializerBinary);
            
            //Update JSON
            foreach (var keyValue in _dictionaryData.ListDictionaryKeyCategoryLanguages)
            {
                await keyValue.UpdateLanguageName(oldLanguageName, newLanguageName);
            }

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);
            
            //Update dynamic Dictionary
            _dynamicDictionary = _dictionaryData.ListDictionaryKeyCategoryLanguages.ToDictionary(data => data.Key, data => new KeyData { Category = data.Category, LanguagesData = data.DictionaryLanguageValue });
        }

        public async void ChangeFavoriteLanguage(string newLanguage)
        {
            //Change BINARY file 
            _languagesData.FavouriteLanguage = newLanguage;

            await SaveFile(_languagesData, BINARY_LANGUAGES_PATH, _serializerBinary);
        }

        #endregion

        #region CATEGORY

        public async void AddNewCategory(string newCategory, CategoriesEditor editor)
        {
            if (newCategory.Equals(""))
            {
                if (editor.AddCategoryFeedbackLabelText != "") return;
                editor.AddCategoryFeedbackLabelText = "Category cannot be an empty value";
                return;
            }

            if (Categories.Exists(c => c.Equals(newCategory)))
            {
                if (editor.AddCategoryFeedbackLabelText != "") return;
                editor.AddCategoryFeedbackLabelText = $"Category '{newCategory}' already exists";
                return;
            }
            editor.AddCategoryFeedbackLabelText = $"Category '{newCategory}' added correctly";

            //Add language to Binary 
            _categoriesData.Categories.Add(newCategory);

            await SaveFile(_categoriesData, BINARY_CATEGORIES_PATH, _serializerBinary);
        }

        public async void RemoveCategory(string category)
        {
            //Change BINARY file 
            _categoriesData.Categories.Remove(category);

            await SaveFile(_categoriesData, BINARY_CATEGORIES_PATH, _serializerBinary);

            //Update JSON
            foreach (var keyValue in _dictionaryData.ListDictionaryKeyCategoryLanguages)
            {
                await keyValue.RemoveCategory(category);
            }
            
            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            //Update dynamic Dictionary
            _dynamicDictionary = _dictionaryData.ListDictionaryKeyCategoryLanguages.ToDictionary(data => data.Key, data => new KeyData { Category = data.Category, LanguagesData = data.DictionaryLanguageValue });
        }

        public async void ChangeCategoryName(string oldCategoryName, string newCategoryName)
        {
            if (_categoriesData.Categories.FirstOrDefault(l => l.Equals(newCategoryName)) != default) return; //the new value is the same
            
            //Change BINARY file 
            _categoriesData.Categories.Remove(oldCategoryName);
            _categoriesData.Categories.Add(newCategoryName);

            await SaveFile(_languagesData, BINARY_LANGUAGES_PATH, _serializerBinary);
            
            //Update JSON
            foreach (var keyValue in _dictionaryData.ListDictionaryKeyCategoryLanguages)
            {
                await keyValue.UpdateCategoryName(oldCategoryName, newCategoryName);
            }

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);
            
            //Update dynamic Dictionary
            _dynamicDictionary = _dictionaryData.ListDictionaryKeyCategoryLanguages.ToDictionary(data => data.Key, data => new KeyData { Category = data.Category, LanguagesData = data.DictionaryLanguageValue });
        }

        #endregion

        #endregion
        
        public void RefreshDictionaryData()
        {
            LoadDictionaryDataFromJSON();
        }

        public void RefreshLanguagesData()
        {
            LoadLanguagesDataFromBINARY();
        }

        public void RefreshCategoriesData()
        {
            LoadCategoriesDataFromBINARY();
        }

        #region GET METHODS


        #endregion

        #endregion

        #region PRIVATE METHODS

        private LocalizationManager()
        {
            Init();
        }

        public async Task Init()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            Debug.Log("Manager initialized");
            _serializerJson = new UnityJsonSerializer();
            _serializerBinary = new BinarySerializer();
            await LoadDictionaryDataFromJSON();
            await LoadLanguagesDataFromBINARY();
            await LoadCategoriesDataFromBINARY();
            YellowIcon = GetColoredIcon("d_Favorite", Color.yellow);

            LocalizationToolController.Instance.ActiveLanguage = FavouriteLanguage;
        }

        private async Task LoadDictionaryDataFromJSON()
        {
            _dictionaryData ??= new DictionaryData();
            var aux = await LoadFile<DictionaryData>(JSON_DICTIONARY_PATH, _serializerJson);
            if (aux != null) _dictionaryData = aux;

            if (_dynamicDictionary != null) _dynamicDictionary.Clear();
            else _dynamicDictionary = new Dictionary<string, KeyData>();

            if (_dictionaryData != null)
                _dynamicDictionary = _dictionaryData.ListDictionaryKeyCategoryLanguages.ToDictionary(data => data.Key, data => new KeyData { Category = data.Category, LanguagesData = data.DictionaryLanguageValue });

            Debug.Log("JSON Dictionary loaded");
        }

        private async Task LoadLanguagesDataFromBINARY()
        {
            _languagesData ??= new LanguagesData();
            _languagesData = await LoadFile<LanguagesData>(BINARY_LANGUAGES_PATH, _serializerBinary);

            CurrentLanguageInDictionarySection = _languagesData.FavouriteLanguage;

            Debug.Log("BINARY Languages loaded");
        }

        private async Task LoadCategoriesDataFromBINARY()
        {
            _categoriesData = await LoadFile<CategoriesData>(BINARY_CATEGORIES_PATH, _serializerBinary);
            
            if (_categoriesData == null)
            {
                _categoriesData = new CategoriesData();
                _categoriesData.Categories.Add("None");
                await SaveFile(_categoriesData, BINARY_CATEGORIES_PATH, _serializerBinary);
            }
            
            Debug.Log("BINARY Categories loaded");
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