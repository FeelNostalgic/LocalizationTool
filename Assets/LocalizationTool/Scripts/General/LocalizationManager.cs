using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LocalizationTool.Data;
using LocalizationTool.Data.Json;
using LocalizationTool.Data.Templates;
using LocalizationTool.Scripts.Addons;
using LocalizationTool.Scripts.API;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Data.Binary;
using LocalizationTool.Scripts.Editors;
using LocalizationTool.Scripts.ExportSerializer;
using LocalizationTool.Scripts.Serializer;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LocalizationTool.Scripts.Commons.EditorStrings;
using static LocalizationTool.Scripts.Commons.EditorPaths;

namespace LocalizationTool.Scripts.General
{
    public class LocalizationManager
    {
        #region PUBLIC VARIABLES

        public static LocalizationManager Instance => _instance ??= new LocalizationManager();

        public static List<LanguagesData.LanguageTuple> OrderedLanguages => _languagesData.orderedLanguages ?? new List<LanguagesData.LanguageTuple>();
        public static List<string> ActiveLanguages => _languagesData.Languages ?? new List<string>();
        public static string DefaultLanguage => _languagesData.FavouriteLanguage;
        public static List<CategoriesData.CategoryTuple> OrderedCategories => _categoriesData?.orderedCategories ?? new List<CategoriesData.CategoryTuple>();
        public static List<string> Categories => _categoriesData?.Categories ?? new List<string>();
        public static ConfigurationData Configuration => _configurationData ?? new ConfigurationData();

        public static Dictionary<string, KeyData> Dictionary => _dynamicDictionary ?? new Dictionary<string, KeyData>();
        public static List<string> Keys => _dictionaryData.listDictionaryKeyCategoryLanguages.Select(x => x.key).ToList();
        public string CurrentLanguageInDictionarySection { get; set; }
        public int CurrentToolbarLanguageIndex => _languagesData.Languages.IndexOf(CurrentLanguageInDictionarySection);
        public static string DefaultCategory => _categoriesData.defaultCategory;
        public static bool IsDataLoaded { get; private set; }

        #region Actions

        public Action OnLocalizationToolInitialized { get; set; }
        public Action<string> OnDefaultCategoryUpdate { get; set; }

        #endregion

        #endregion

        #region PRIVATE VARIABLES

        private static LocalizationManager _instance;
        private ISerializerService _serializerJson;
        private ISerializerService _serializerBinary;

        #region JSON Data

        private static DictionaryData _dictionaryData;

        #endregion

        #region BINARY Data

        private static LanguagesData _languagesData;
        private static CategoriesData _categoriesData;
        private static ConfigurationData _configurationData;

        #endregion

        private static bool _isInitialized;

        private static Dictionary<string, KeyData> _dynamicDictionary;

        #endregion

        #region PUBLIC METHODS

        #region DICTIONARY

        public async void AddNewKey(string key, string category, LocalizationEditor editor = null)
        {
            if (key.IsEmpty())
            {
                ShowFeedback(EMPTY_KEY_FEEDBACK_LABEL, editor);
                return;
            }

            if (key.Contains(" "))
            {
                ShowFeedback(SPACES_KEY_FEEDBACK_LABEL, editor);
                return;
            }

            if (_dynamicDictionary.ContainsKey(key))
            {
                ShowFeedback(string.Format(KEY_EXIST_FEEDBACK_LABEL, key), editor);
                return;
            }

            if (key.Length > MAX_KEY_CHARACTERS)
            {
                ShowFeedback(string.Format(CHARACTERS_NUMBER_KEY_FEEDBACK_LABEL, MAX_KEY_CHARACTERS), editor);
                return;
            }

            if (editor != null) editor.ClearAddTextField();

            var interDic = ActiveLanguages.ToDictionary(language => language, _ => "");
            var interList = ActiveLanguages.Select(l => new LanguageValue { language = l, value = "" }).ToList();
            _dynamicDictionary.Add(key, new KeyData { Category = category, LanguagesData = interDic });

            //Add key to JSON file
            _dictionaryData.listDictionaryKeyCategoryLanguages.Add(new KeyCategoryLanguageValues(key, category, interList));

            ShowFeedback(string.Format(KEY_ADDED_FEEDBACK_LABEL, key), editor);

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);
        }

        public async Task<string> ChangeKey(string oldKey, string newKey, LocalizationEditor editor)
        {
            if (oldKey.Equals(newKey)) return oldKey;

            if (newKey.IsEmpty())
            {
                ShowFeedback(EMPTY_KEY_FEEDBACK_LABEL, editor);
                return oldKey;
            }

            if (newKey.Contains(" "))
            {
                ShowFeedback(SPACES_KEY_FEEDBACK_LABEL, editor);
                return oldKey;
            }

            if (_dynamicDictionary.ContainsKey(newKey))
            {
                ShowFeedback(string.Format(KEY_EXIST_FEEDBACK_LABEL, oldKey), editor);
                return oldKey;
            }

            if (newKey.Length > MAX_KEY_CHARACTERS)
            {
                ShowFeedback(string.Format(CHARACTERS_NUMBER_KEY_FEEDBACK_LABEL, MAX_KEY_CHARACTERS), editor);
                return oldKey;
            }

            var oldData = _dynamicDictionary[oldKey];
            _dynamicDictionary.Remove(oldKey);
            _dynamicDictionary.Add(newKey, oldData);

            //Change JSON file 
            _dictionaryData.listDictionaryKeyCategoryLanguages.First(x => x.key == oldKey).UpdateKey(newKey);

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            UpdateAddonsOnKeyUpdated(oldKey, newKey);

            return newKey;
        }

        public async Task ChangeValue(string key, string newValue)
        {
            if (!_dynamicDictionary.ContainsKey(key)) return;
            if (_dynamicDictionary[key].LanguagesData != null && _dynamicDictionary[key].LanguagesData[CurrentLanguageInDictionarySection].Equals(newValue)) return;

            UpdateValueInDictionary(key, newValue, CurrentLanguageInDictionarySection);

            //Change JSON file 
            _dictionaryData.listDictionaryKeyCategoryLanguages.First(x => x.key == key).UpdateValue(CurrentLanguageInDictionarySection, newValue);

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);
        }

        public async void ChangeCategory(string key, string newCategory)
        {
            if (_dynamicDictionary[key].Category == newCategory) return;

            var oldData = _dynamicDictionary[key].LanguagesData;
            _dynamicDictionary.Remove(key);
            _dynamicDictionary.Add(key, new KeyData { Category = newCategory, LanguagesData = oldData });

            //Change JSON file 
            _dictionaryData.listDictionaryKeyCategoryLanguages.First(x => x.key == key).UpdateCategory(newCategory);

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            Log(string.Format(DICTIONARY_KEY_CATEGORY_CHANGED_LOG, key, newCategory));
        }

        public async void RemoveKey(string key)
        {
            _dynamicDictionary.Remove(key);

            //Change JSON file 
            _dictionaryData.RemoveKey(key);
            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            UpdateAddonsOnKeyRemoved(key);
        }

        private static void UpdateAddonsOnKeyRemoved(string key)
        {
            var tmproText = Resources.FindObjectsOfTypeAll(typeof(TextMeshProUGUI));
            foreach (var obj in tmproText)
            {
                var item = (TextMeshProUGUI)obj;
                var addon = item.GetComponent<LocalizationToolAddon>();
                if (addon != null)
                {
                    addon.OnKeyRemoved(key);
                }
            }

            //Save scene
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        private static void UpdateAddonsOnKeyUpdated(string oldKey, string newKey)
        {
            var tmproText = Resources.FindObjectsOfTypeAll(typeof(TextMeshProUGUI));
            foreach (var obj in tmproText)
            {
                var item = (TextMeshProUGUI)obj;
                var addon = item.GetComponent<LocalizationToolAddon>();
                if (addon != null)
                {
                    addon.OnKeyUpdate(oldKey, newKey);
                }
            }

            //Save scene
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        #endregion

        #region LANGUAGE

        public async void AddNewLanguage(string newLanguage, LocalizationEditor editor = null)
        {
            if (newLanguage.Equals(""))
            {
                ShowFeedback(EMPTY_LANGUAGE_FEEDBACK_LABEL, editor);
                return;
            }

            if (newLanguage.Contains(" "))
            {
                ShowFeedback(SPACES_LANGUAGE_FEEDBACK_LABEL, editor);
                return;
            }

            if (_languagesData.Contains(newLanguage))
            {
                ShowFeedback(string.Format(LANGUAGE_EXIST_FEEDBACK_LABEL, newLanguage), editor);
                return;
            }

            if (newLanguage.Length > MAX_LANGUAGE_CHARACTERS)
            {
                ShowFeedback(string.Format(CHARACTERS_NUMBER_LANGUAGE_FEEDBACK_LABEL, MAX_LANGUAGE_CHARACTERS), editor);
                return;
            }

            if (editor != null) editor.ClearAddTextField();

            //Add language to Binary 
            _languagesData.Add(newLanguage);

            if (_languagesData.Count() == 1)
            {
                _languagesData.FavouriteLanguage = newLanguage;
                CurrentLanguageInDictionarySection = newLanguage;
            }

            ShowFeedback(string.Format(LANGUAGE_ADDED_FEEDBACK_LABEL, newLanguage), editor);

            await SaveFile(_languagesData, BINARY_LANGUAGES_PATH, _serializerBinary);

            //Update JSON
            foreach (var keyValue in _dictionaryData.listDictionaryKeyCategoryLanguages)
            {
                keyValue.AddNewLanguage(newLanguage);
            }

            //Update dynamic Dictionary
            _dynamicDictionary = _dictionaryData.listDictionaryKeyCategoryLanguages.ToDictionary(data => data.key, 
                data => new KeyData { Category = data.category, LanguagesData = data.DictionaryLanguageValue });
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
            foreach (var keyValue in _dictionaryData.listDictionaryKeyCategoryLanguages)
            {
                await keyValue.RemoveLanguage(language);
            }

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            //Update dynamic Dictionary
            _dynamicDictionary = _dictionaryData.listDictionaryKeyCategoryLanguages.ToDictionary(data => data.key, data => new KeyData { Category = data.category, LanguagesData = data.DictionaryLanguageValue });
        }

        public async void ChangeLanguageValue(string oldLanguageName, string newLanguageName)
        {
            if (CurrentLanguageInDictionarySection.Equals(oldLanguageName)) CurrentLanguageInDictionarySection = newLanguageName;

            //Change BINARY file 
            _languagesData.ChangeName(oldLanguageName, newLanguageName);

            await SaveFile(_languagesData, BINARY_LANGUAGES_PATH, _serializerBinary);

            //Update JSON
            foreach (var keyValue in _dictionaryData.listDictionaryKeyCategoryLanguages)
            {
                await keyValue.UpdateLanguageName(oldLanguageName, newLanguageName);
            }

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            //Update dynamic Dictionary
            _dynamicDictionary = _dictionaryData.listDictionaryKeyCategoryLanguages.ToDictionary(data => data.key, data => new KeyData { Category = data.category, LanguagesData = data.DictionaryLanguageValue });

            //Log($"Language '{oldLanguageName}' update to '{newLanguageName}' correctly");
        }

        public async void ChangeFavoriteLanguage(string newLanguage)
        {
            //Change BINARY file 
            _languagesData.FavouriteLanguage = newLanguage;
            _languagesData.ChangeIndex(newLanguage, 0);

            await SaveFile(_languagesData, BINARY_LANGUAGES_PATH, _serializerBinary);
        }

        public async void ChangeLanguageIndex(int oldIndex, int newIndex)
        {
            if (newIndex <= 1) return;
            if (oldIndex == newIndex) return; //the new index is the same

            //Change binary file
            var language = _languagesData.ChangeIndex(oldIndex, newIndex);

            await SaveFile(_languagesData, BINARY_LANGUAGES_PATH, _serializerBinary);

            Log(string.Format(LANGUAGE_INDEX_CHANGED_LOG, language.Language, language.Index));

            //GUI.FocusControl(null);
        }

        public static bool IsFavouriteLanguage(string language)
        {
            return _languagesData.FavouriteLanguage.Equals(language);
        }

        #endregion

        #region CATEGORY

        public async void AddNewCategory(string newCategory, LocalizationEditor editor = null, bool showEditorLogs = true)
        {
            // Empty value
            if (newCategory.IsEmpty())
            {
                if (showEditorLogs) ShowFeedback(EMPTY_CATEGORY_FEEDBACK_LABEL, editor);
                return;
            }

            if (newCategory.Contains(" "))
            {
                if (showEditorLogs) ShowFeedback(SPACES_CATEGORY_FEEDBACK_LABEL, editor);
                return;
            }

            // Already in data
            if (_categoriesData.Contains(newCategory))
            {
                if (showEditorLogs) ShowFeedback(string.Format(CATEGORY_EXIST_FEEDBACK_LABEL, newCategory), editor);
                return;
            }

            if (newCategory.Length > MAX_CATEGORY_CHARACTERS)
            {
                if (showEditorLogs) ShowFeedback(string.Format(CHARACTERS_NUMBER_CATEGORY_FEEDBACK_LABEL, MAX_CATEGORY_CHARACTERS), editor);
                return;
            }

            if (editor != null) editor.ClearAddTextField();

            //Add language to Binary 
            _categoriesData.Add(newCategory);

            if (showEditorLogs) ShowFeedback(string.Format(CATEGORY_ADDED_FEEDBACK_LABEL, newCategory), editor);

            await SaveFile(_categoriesData, BINARY_CATEGORIES_PATH, _serializerBinary);
        }

        public async void RemoveCategory(string category)
        {
            //Change BINARY file 
            _categoriesData.Remove(category);

            await SaveFile(_categoriesData, BINARY_CATEGORIES_PATH, _serializerBinary);

            //Update JSON
            foreach (var keyValue in _dictionaryData.listDictionaryKeyCategoryLanguages)
            {
                await keyValue.RemoveCategory(category);
            }

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            //Update dynamic Dictionary
            _dynamicDictionary = _dictionaryData.listDictionaryKeyCategoryLanguages
                .ToDictionary(data => data.key, data => new KeyData { Category = data.category, LanguagesData = data.DictionaryLanguageValue });
        }

        public async void ChangeCategoryName(string oldCategoryName, string newCategoryName)
        {
            //Change BINARY file 
            _categoriesData.ChangeName(oldCategoryName, newCategoryName);

            await SaveFile(_categoriesData, BINARY_CATEGORIES_PATH, _serializerBinary);

            //Update JSON
            foreach (var keyValue in _dictionaryData.listDictionaryKeyCategoryLanguages)
            {
                await keyValue.UpdateCategoryName(oldCategoryName, newCategoryName);
            }

            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            //Update dynamic Dictionary
            _dynamicDictionary = _dictionaryData.listDictionaryKeyCategoryLanguages
                .ToDictionary(data => data.key, data => new KeyData { Category = data.category, LanguagesData = data.DictionaryLanguageValue });

            //Log($"Category '{oldCategoryName}' update to '{newCategoryName}' correctly");
        }

        public async void ChangeCategoryIndex(int oldIndex, int newIndex)
        {
            if (newIndex <= 1) return;
            if (oldIndex == newIndex) return; //the new index is the same

            //Change binary file
            var category = _categoriesData.ChangeIndex(oldIndex, newIndex);

            await SaveFile(_categoriesData, BINARY_CATEGORIES_PATH, _serializerBinary);

            Log(string.Format(CATEGORY_INDEX_CHANGED_LOG, category.Category, category.Index));

            //GUI.FocusControl(null);
        }

        public async void ChangeDefaultCategory(string newCategory)
        {
            //Change BINARY file 
            _categoriesData.defaultCategory = newCategory;
            _categoriesData.ChangeIndex(newCategory, 0);

            await SaveFile(_categoriesData, BINARY_CATEGORIES_PATH, _serializerBinary);

            OnDefaultCategoryUpdate?.Invoke(newCategory);
        }

        public static bool IsDefaultCategory(string category)
        {
            return _categoriesData.defaultCategory.Equals(category);
        }

        #endregion

        #region CONFIGURATION

        public async void UpdateDeleteConfirmation(bool newValue, Enums.GUIWindow window)
        {
            switch (window)
            {
                case Enums.GUIWindow.Dictionary:
                    _configurationData.dictionaryDeleteConfirmation = newValue;
                    break;
                case Enums.GUIWindow.Language:
                    _configurationData.languageDeleteConfirmation = newValue;
                    break;
                case Enums.GUIWindow.Category:
                    _configurationData.categoryDeleteConfirmation = newValue;
                    break;
            }

            await SaveFile(_configurationData, BINARY_CONFIGURATION_PATH, _serializerBinary);
        }

        public async void UpdateClearAdd(bool newValue, Enums.GUIWindow window)
        {
            switch (window)
            {
                case Enums.GUIWindow.Dictionary:
                    _configurationData.dictionaryClearAdd = newValue;
                    break;
                case Enums.GUIWindow.Language:
                    _configurationData.languageClearAdd = newValue;
                    break;
                case Enums.GUIWindow.Category:
                    _configurationData.categoryClearAdd = newValue;
                    break;
            }

            await SaveFile(_configurationData, BINARY_CONFIGURATION_PATH, _serializerBinary);
        }

        public async void UpdateSearchType(int searchTypeIndex)
        {
            if (_configurationData.searchTypeIndex == searchTypeIndex) return;

            _configurationData.searchTypeIndex = searchTypeIndex;
            await SaveFile(_configurationData, BINARY_CONFIGURATION_PATH, _serializerBinary);
        }

        public async void UpdateShowLog(bool newValue)
        {
            _configurationData.showLogsInConsole = newValue;
            await SaveFile(_configurationData, BINARY_CONFIGURATION_PATH, _serializerBinary);
        }

        #endregion

        #region EXPORT

        public static string BuildCSV(string separator)
        {
            var serializer = new CSV_Serializer();
            serializer.SetSeparator(separator);

            serializer.AddTitle(ActiveLanguages);

            var languageDictionary = new Dictionary<string, int>();
            for (var i = 0; i < ActiveLanguages.Count; i++)
            {
                languageDictionary.Add(ActiveLanguages[i], i);
            }

            var data = new Dictionary<string, KeyData>(Dictionary);

            foreach (var (key, keyData) in data)
            {
                var items = new List<string>();
                var category = keyData.Category;

                items.Add(key);
                items.Add(category);

                var auxArray = new string[languageDictionary.Count];
                foreach (var (language, value) in keyData.LanguagesData)
                {
                    auxArray[languageDictionary[language]] = value.Replace("\n", " ").Replace("\r", " ");
                }

                items.AddRange(auxArray);

                serializer.AddLine(items);
            }

            return serializer.File();
        }

        public static string BuildSerializedData(ISerializerService serializer)
        {
            var dataToSerialize = new DictionaryTemplate();

            var data = new Dictionary<string, KeyData>(Dictionary);

            foreach (var (key, keyData) in data)
            {
                var category = keyData.Category;
                var languageValues = keyData.LanguagesData.Select(item => new DictionaryTemplate.LanguageValue { Language = item.Key, Value = item.Value }).ToList()
                    .OrderBy(x=>OrderedLanguages.First(y=> y.Language.Equals(x.Language)).Index).ToList();
                dataToSerialize.DictionaryKeyCategoryLanguages.Add(new DictionaryTemplate.KeyCategoryLanguageValues
                {
                    Key = key,
                    Category = category,
                    LanguageValues = languageValues
                });
            }

            return serializer.Serialize(dataToSerialize);
        }

        #endregion

        #region IMPORT

        public static IEnumerator ImportCSVCoroutine(string path, string separator, ImportProgressWindow progressWindow)
        {
            using var reader = new StreamReader(path);
            var fileInfo = new FileInfo(path);
            var totalBytes = fileInfo.Length;
            long bytesRead = 0;

            progressWindow.SetStatus(IMPORT_STATUS_LANGUAGES);

            var header = reader.ReadLine();
            Debug.Assert(header != null, nameof(header) + " != null");
            bytesRead += header.Length + Environment.NewLine.Length;
            progressWindow.SetProgress((float)bytesRead / totalBytes);

            var languages = header?.Split(separator);

            Debug.Assert(languages != null, nameof(languages) + " != null");
            if (languages.Length < 2)
            {
                var sbResultError = new StringBuilder();
                sbResultError.AppendLine("Error while importing CSV");
                sbResultError.AppendLine($"Separator [ {separator} ] not found");
                progressWindow.Complete(sbResultError.ToString());

                yield break;
            }

            var languageOrder = new List<string>();
            for (var i = 2; i < languages.Length; i++)
            {
                languageOrder.Add(languages[i]);
                var loadLanguageTask = Instance.ImportLanguage(languages[i]);
                var awaiter = loadLanguageTask.GetAwaiter();
                while (!awaiter.IsCompleted) yield return null;
            }

            progressWindow.SetStatus(IMPORT_STATUS_KEYS);
            var nKeys = 0;
            var nCategories = 0;
            while (!reader.EndOfStream)
            {
                var nextLine = reader.ReadLine();
                Debug.Assert(nextLine != null, nameof(nextLine) + " != null");
                bytesRead += nextLine.Length + Environment.NewLine.Length;
                var lineItems = nextLine?.Split(separator);
                var key = lineItems?[0];
                var category = lineItems?[1];
                var values = new Dictionary<string, string>();

                nKeys++;
                if (!ExistCategory(category)) nCategories++;

                Debug.Assert(lineItems != null, nameof(lineItems) + " != null");
                for (var i = 0; i < lineItems.Length - 2; i++)
                {
                    values.Add(languageOrder[i], lineItems[i + 2]);
                }

                var loadKeyTask = Instance.ImportKey(key, category, values);
                var awaiter = loadKeyTask.GetAwaiter();
                while (!awaiter.IsCompleted) yield return null;

                progressWindow.SetProgress((float)bytesRead / totalBytes);
                progressWindow.SetProgressInfo(string.Format(IMPORT_PROGRESS_KEY_CATEGORY, key, category));
            }

            var sb = new StringBuilder();
            sb.AppendLine(IMPORT_RESULT_SUCCESS);
            sb.AppendLine(string.Format(IMPORT_LANGUAGES_RESULT, languages.Length - 2));
            sb.AppendLine(string.Format(IMPORT_CATEGORIES_RESULT, nCategories));
            sb.AppendLine(string.Format(IMPORT_KEYS_RESULT, nKeys));

            progressWindow.Complete(sb.ToString());
        }

        public static IEnumerator ImportSerializedDataCoroutine(string path, ISerializerService serializer, ImportProgressWindow progressWindow, Action onComplete = null)
        {
            // Load data
            var loadFileTask = LoadFile<DictionaryTemplate>(path, serializer);
            var awaiter = loadFileTask.GetAwaiter();
            while (!awaiter.IsCompleted) yield return null;
            var data = awaiter.GetResult();

            // Items count
            var totalItems = data.DictionaryKeyCategoryLanguages[0].LanguageValues.Count + data.DictionaryKeyCategoryLanguages.Count;
            var itemCount = 0f;

            //Languages
            if (progressWindow.IsNotNull()) progressWindow.SetStatus(IMPORT_STATUS_LANGUAGES);

            foreach (var languageValue in data.DictionaryKeyCategoryLanguages[0].LanguageValues)
            {
                var task = Instance.ImportLanguage(languageValue.Language);
                var awaiterLanguage = task.GetAwaiter();
                while (!awaiterLanguage.IsCompleted) yield return null;
                if (progressWindow.IsNotNull()) progressWindow.SetProgressInfo(string.Format(IMPORT_PROGRESS_LANGUAGE, languageValue.Language));
                if (progressWindow.IsNotNull()) progressWindow.SetProgress(itemCount++ / totalItems);
                yield return null;
            }

            var nKeys = 0;
            var nCategories = 0;

            // Keys
            if (progressWindow.IsNotNull()) progressWindow.SetStatus(IMPORT_STATUS_KEYS);

            foreach (var keyCategoryLanguage in data.DictionaryKeyCategoryLanguages)
            {
                nKeys++;
                if (!ExistCategory(keyCategoryLanguage.Category)) nCategories++;

                foreach (var awaiterKey in keyCategoryLanguage.LanguageValues
                             .Select(languageValue => Instance.ImportKey(keyCategoryLanguage.Key, keyCategoryLanguage.Category, languageValue.Language, languageValue.Value))
                             .Select(task => task.GetAwaiter()))
                {
                    while (!awaiterKey.IsCompleted) yield return null;

                    yield return null;
                }

                if (progressWindow.IsNotNull()) progressWindow.SetProgress(itemCount++ / totalItems);
                if (progressWindow.IsNotNull()) progressWindow.SetProgressInfo(string.Format(IMPORT_PROGRESS_KEY_CATEGORY, keyCategoryLanguage.Key, keyCategoryLanguage.Category));
                yield return null;
            }

            var sb = new StringBuilder();
            sb.AppendLine(IMPORT_RESULT_SUCCESS);
            sb.AppendLine(string.Format(IMPORT_LANGUAGES_RESULT, data.DictionaryKeyCategoryLanguages[0].LanguageValues.Count));
            sb.AppendLine(string.Format(IMPORT_CATEGORIES_RESULT, nCategories));
            sb.AppendLine(string.Format(IMPORT_KEYS_RESULT, nKeys));

            if (progressWindow.IsNotNull()) progressWindow.Complete(sb.ToString());
            
            onComplete?.Invoke();
        }

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
            foreach (var keyValue in _dictionaryData.listDictionaryKeyCategoryLanguages)
            {
                keyValue.AddNewLanguage(language);
            }

            //Update JSON
            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            //Update dynamic Dictionary
            _dynamicDictionary = _dictionaryData.listDictionaryKeyCategoryLanguages.ToDictionary(data => data.key, data => new KeyData { Category = data.category, LanguagesData = data.DictionaryLanguageValue });

            Log(string.Format(IMPORTED_LANGUAGE_LOG, language));
        }

        private async Task ImportKey(string key, string category, Dictionary<string, string> values)
        {
            if (key.IsEmpty()) return;
            Log(string.Format(IMPORTED_KEY_LOG, key, category, string.Join(" | ", values)));
            if (_dynamicDictionary.ContainsKey(key))
            {
                if (_dynamicDictionary[key].Category != category)
                {
                    //Change category
                    if (!_categoriesData.Contains(category)) AddNewCategory(category.IsEmpty() ? _categoriesData.defaultCategory : category, null, false);

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

                if (!_categoriesData.Contains(category)) AddNewCategory(category.IsEmpty() ? _categoriesData.defaultCategory : category, null, false);

                var interList = values.Select(languageValuePair => new LanguageValue { language = languageValuePair.Key, value = languageValuePair.Value }).ToList();

                //Add key to JSON file
                _dictionaryData.AddNewKeyCategoryLanguage(new KeyCategoryLanguageValues(key, category, interList));
            }

            //Update JSON
            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            //Update dynamic Dictionary
            _dynamicDictionary = _dictionaryData.listDictionaryKeyCategoryLanguages.ToDictionary(data => data.key,
                data => new KeyData { Category = data.category, LanguagesData = data.DictionaryLanguageValue });
        }

        public async Task ImportKey(string key, string category, string language, string value)
        {
            if (key.Equals("")) return;
            Log(string.Format(IMPORTED_KEY_SINGLE_LOG, key, category, language, value));

#pragma warning disable CS4014
            ImportLanguage(language);
#pragma warning restore CS4014

            if (_dynamicDictionary.ContainsKey(key))
            {
                AddNewCategory(category.IsEmpty() ? _categoriesData.defaultCategory : category, null, false);

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
                AddNewCategory(category.IsEmpty() ? _categoriesData.defaultCategory : category, null, false);

                var interDic = ActiveLanguages.ToDictionary(l => l, _ => "");
                interDic[language] = value;
                var interList = interDic.Select(l => new LanguageValue { language = l.Key, value = l.Value }).ToList();

                _dynamicDictionary.Add(key, new KeyData { Category = category, LanguagesData = interDic });

                //Add key to JSON file
                _dictionaryData.listDictionaryKeyCategoryLanguages.Add(new KeyCategoryLanguageValues(key, category, interList));
            }

            //Update JSON
            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            //Update dynamic Dictionary
            // _dynamicDictionary = _dictionaryData.listDictionaryKeyCategoryLanguages.ToDictionary(data => data.key,
            //     data => new KeyData { Category = data.category, LanguagesData = data.DictionaryLanguageValue });
        }

        public static bool ExistCategory(string category)
        {
            return _categoriesData.Contains(category);
        }

        #endregion

        #region RefreshData

        // ReSharper disable Unity.PerformanceAnalysis
        public void RefreshDictionaryData()
        {
#pragma warning disable CS4014
            LoadDictionaryDataFromJSON();
#pragma warning restore CS4014
        }

        public void RefreshLanguagesData()
        {
#pragma warning disable CS4014
            LoadLanguagesDataFromBINARY();
#pragma warning restore CS4014
        }

        public void RefreshCategoriesData()
        {
#pragma warning disable CS4014
            LoadCategoriesDataFromBINARY();
#pragma warning restore CS4014
        }

        #endregion

        #region LOGS

        public static void Log(string log)
        {
            try
            {
                if (_configurationData.showLogsInConsole) Debug.Log(log);
            }
            catch (Exception)
            {
                Debug.Log(log);
            }
        }

        public static void LogWarning(string log)
        {
            try
            {
                if (_configurationData.showLogsInConsole) Debug.LogWarning(log);
            }
            catch (Exception)
            {
                Debug.LogWarning(log);
            }
        }

        public static void LogError(string log)
        {
            try
            {
                if (_configurationData.showLogsInConsole) Debug.LogError(log);
            }
            catch (Exception)
            {
                Debug.LogError(log);
            }
        }

        #endregion

        public async void ClearData()
        {
            //File.Delete(JSON_DICTIONARY_PATH);
            _dictionaryData = new DictionaryData();
            await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);

            //File.Delete(BINARY_LANGUAGES_PATH);
            var defaultLanguage = _languagesData.FavouriteLanguage;
            _languagesData = new LanguagesData
            {
                FavouriteLanguage = defaultLanguage
            };
            await SaveFile(_languagesData, BINARY_LANGUAGES_PATH, _serializerBinary);
            
            //File.Delete(BINARY_CATEGORIES_PATH);
            _categoriesData = new CategoriesData
            {
                defaultCategory = _categoriesData.defaultCategory
            };
            await SaveFile(_categoriesData, BINARY_CATEGORIES_PATH, _serializerBinary);
            
            _dynamicDictionary.Clear();
            if (_dictionaryData.IsNotNull())
                _dynamicDictionary = _dictionaryData.listDictionaryKeyCategoryLanguages.ToDictionary(data => data.key,
                    data => new KeyData { Category = data.category, LanguagesData = data.DictionaryLanguageValue });
        }

        #endregion

        #region PRIVATE METHODS

        private LocalizationManager()
        {
#pragma warning disable CS4014
            //TODO: te if this is necessary => Init();
#pragma warning restore CS4014
        }

        private static void ShowFeedback(string text, LocalizationEditor editor)
        {
            Log(text);
            if (editor != null) editor.ControlFeedbackLabel(text);
        }

        #region LOAD FROM MEMORY

        public async Task Init(Action onComplete = null)
        {
            if (_isInitialized) return; // Just the first call
            _isInitialized = true;

            _serializerJson = new UnityJsonSerializer();
            _serializerBinary = new BinarySerializer();
#pragma warning disable CS4014
            LoadConfigurationDataFromBINARY();
#pragma warning restore CS4014
            await LoadDictionaryDataFromJSON();
            await LoadLanguagesDataFromBINARY();
            await LoadCategoriesDataFromBINARY();

            IsDataLoaded = true;
            
            
            Log(TOOL_INITIALIZED_LOG);
            onComplete?.Invoke();
            OnLocalizationToolInitialized?.Invoke();
        }
        
        private async Task LoadDictionaryDataFromJSON()
        {
            var aux = await LoadFile<DictionaryData>(JSON_DICTIONARY_PATH, _serializerJson);
            if (aux.IsNull())
            {
                _dictionaryData = new DictionaryData();
                await SaveFile(_dictionaryData, JSON_DICTIONARY_PATH, _serializerJson);
            }
            else
                _dictionaryData = aux;

            if (_dynamicDictionary.IsNotNull()) _dynamicDictionary.Clear();
            else _dynamicDictionary = new Dictionary<string, KeyData>();

            if (_dictionaryData.IsNotNull())
                _dynamicDictionary = _dictionaryData.listDictionaryKeyCategoryLanguages.ToDictionary(data => data.key,
                    data => new KeyData { Category = data.category, LanguagesData = data.DictionaryLanguageValue });

            //Log("Dictionary loaded");
        }

        private async Task LoadLanguagesDataFromBINARY()
        {
            _languagesData = await LoadFile<LanguagesData>(BINARY_LANGUAGES_PATH, _serializerBinary);

            if (_languagesData.IsNull())
            {
                _languagesData = new LanguagesData
                {
                    FavouriteLanguage = "English"
                };
                _languagesData.Add("English");
                await SaveFile(_languagesData, BINARY_LANGUAGES_PATH, _serializerBinary);
            }

            CurrentLanguageInDictionarySection = _languagesData.FavouriteLanguage;
            if (LocalizationToolAPI.Instance.IsNotNull()) LocalizationToolAPI.ActiveLanguage = _languagesData.FavouriteLanguage;

            //Log("Languages loaded");
        }

        private async Task LoadCategoriesDataFromBINARY()
        {
            _categoriesData = await LoadFile<CategoriesData>(BINARY_CATEGORIES_PATH, _serializerBinary);

            if (_categoriesData.IsNull())
            {
                _categoriesData = new CategoriesData
                {
                    defaultCategory = "None"
                };
                _categoriesData.Add("None");
                await SaveFile(_categoriesData, BINARY_CATEGORIES_PATH, _serializerBinary);
            }

            //Log("Categories loaded");
        }

        private async Task LoadConfigurationDataFromBINARY()
        {
            _configurationData = await LoadFile<ConfigurationData>(BINARY_CONFIGURATION_PATH, _serializerBinary);

            if (_configurationData.IsNull())
            {
                _configurationData = new ConfigurationData
                {
                    dictionaryDeleteConfirmation = true,
                    categoryDeleteConfirmation = true,
                    languageDeleteConfirmation = true,
                    dictionaryClearAdd = true,
                    languageClearAdd = true,
                    categoryClearAdd = true,
                    searchTypeIndex = 0,
                    showLogsInConsole = true
                };
                await SaveFile(_configurationData, BINARY_CONFIGURATION_PATH, _serializerBinary);
            }

            //Log("Configuration loaded");
        }

        #endregion

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
            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(path);
                await writer.WriteAsync(dataToSave);
            }
            catch (Exception e)
            {
                if (e is not IOException) Debug.LogError($"PATH: {path} => {e}");
            }
            finally
            {
                writer?.Close();
            }
        }

        public static async Task<T> LoadFile<T>(string path, ISerializerService serializer)
        {
            StreamReader reader = null;

            try
            {
                var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read);
                reader = new StreamReader(fs);
                return serializer.Deserialize<T>(await reader.ReadToEndAsync());
            }
            catch (Exception e)
            {
                Debug.LogError($"PATH: {path} => {e}");
            }
            finally
            {
                reader?.Close();
            }

            return default;
        }

        public static async void SaveFile(string path, string fileContent, string title, string message, string okMessage)
        {
            await File.WriteAllTextAsync(path, fileContent);

            if (title.IsNotEmpty()) EditorUtility.DisplayDialog(title, message, okMessage);
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