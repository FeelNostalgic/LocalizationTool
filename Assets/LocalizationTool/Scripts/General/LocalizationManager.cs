#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LocalizationTool.Data;
using LocalizationTool.Scripts.Addons;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.Editors;
using LocalizationTool.Scripts.ExportSerializer;
using LocalizationTool.Scripts.Serializer;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using LocalizationTool.Scripts.Data;
using LocalizationTool.Scripts.Data.ScriptableObjects;
using LocalizationTool.Scripts.Data.TemplatesForSerializer;
using static LocalizationTool.Scripts.Commons.EditorStrings;
using static LocalizationTool.Scripts.Commons.EditorPaths;
using Colors = CustomDebug.Colors;

namespace LocalizationTool.Scripts.General
{
    public class LocalizationManager
    {
        #region PUBLIC VARIABLES

        public static LocalizationManager Instance => _instance ??= new LocalizationManager();

        public static ConfigurationData Configuration => _configurationData ?? new ConfigurationData();

        public static string CurrentLanguageInDictionarySection { get; set; }
        public static int CurrentToolbarLanguageIndex => CacheDataSO.Languages.IndexOf(CurrentLanguageInDictionarySection);

        #region Actions

        public Action<string> OnDefaultCategoryUpdate { get; set; }

        #endregion

        #endregion

        #region PRIVATE VARIABLES

        private static LocalizationManager _instance;
        private ISerializerService _serializerBinary;

        #region BINARY Data

        private static ConfigurationData _configurationData;

        #endregion

        private static bool _isInitialized;

        #endregion

        #region PUBLIC METHODS

        #region DICTIONARY

        public static void AddNewKey(string key, string category, EditorWindowAbstract editor = null)
        {
            if (key.IsEmpty() || key.IsNull())
            {
                ShowFeedback(DICTIONARY_LABEL, Colors.Blue, EMPTY_KEY_FEEDBACK_LABEL, editor);
                return;
            }

            if (ExistsKey(key))
            {
                ShowFeedback(DICTIONARY_LABEL, Colors.Blue, string.Format(KEY_EXIST_FEEDBACK_LABEL, key), editor);
                return;
            }

            if (key.Length > MAX_KEY_CHARACTERS)
            {
                ShowFeedback(DICTIONARY_LABEL, Colors.Blue, string.Format(CHARACTERS_NUMBER_KEY_FEEDBACK_LABEL, MAX_KEY_CHARACTERS), editor);
                return;
            }

            editor?.ClearAddTextField();

            // Add key to database
            CacheDataSO.InsertKey(key, category);

            ShowFeedback(DICTIONARY_LABEL, Colors.Blue, string.Format(KEY_ADDED_FEEDBACK_LABEL, key), editor);
        }

        public static string ChangeKey(string oldKeyName, string newKeyName, RichTextEditor editor)
        {
            if (oldKeyName.Equals(newKeyName)) return oldKeyName;

            if (newKeyName.IsEmpty() || newKeyName.IsNull())
            {
                ShowFeedback(EMPTY_KEY_FEEDBACK_LABEL, editor);
                return oldKeyName;
            }

            if (ExistsKey(newKeyName))
            {
                ShowFeedback(string.Format(KEY_EXIST_FEEDBACK_LABEL, newKeyName), editor);
                return oldKeyName;
            }

            if (newKeyName.Length > MAX_KEY_CHARACTERS)
            {
                ShowFeedback(string.Format(CHARACTERS_NUMBER_KEY_FEEDBACK_LABEL, MAX_KEY_CHARACTERS), editor);
                return oldKeyName;
            }

            // Update database
            CacheDataSO.UpdateKeyName(oldKeyName, newKeyName);

            UpdateAddonsOnKeyUpdated(oldKeyName, newKeyName);

            return newKeyName;
        }

        public static void ChangeValue(string key, string newTranslation)
        {
            if (!ExistsKey(key)) return;
            if (CacheDataSO.localizationData.LanguagesDictionary[CurrentLanguageInDictionarySection].TranslationDictionary[key].translationText.Equals(newTranslation)) return;

            // Update database
            CacheDataSO.SetKeyTranslation(key, newTranslation, CurrentLanguageInDictionarySection);
        }

        public static void ChangeCategory(string key, string newCategory)
        {
            if (CacheDataSO.localizationData.KeysDictionary[key].category.name.Equals(newCategory)) return;

            // Update database
            CacheDataSO.UpdateKeyCategory(key, newCategory);

            Log(DICTIONARY_LABEL, Colors.Blue, string.Format(DICTIONARY_KEY_CATEGORY_CHANGED_LOG, key, newCategory));
        }

        public static void RemoveKey(string key)
        {
            // Update database
            CacheDataSO.RemoveKey(key);

            UpdateAddonsOnKeyRemoved(key);
        }

        private static bool ExistsKey(string key)
        {
            return CacheDataSO.localizationData.KeysDictionary.ContainsKey(key);
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

        public static void RefreshDictionaryData()
        {
            CacheDataSO.LoadKeysCache();
        }

        #endregion

        #region LANGUAGE

        public static void AddNewLanguage(string newLanguage, EditorWindowAbstract editor = null)
        {
            if (newLanguage.IsEmpty() || newLanguage.IsNull())
            {
                ShowFeedback(LANGUAGES_LABEL, Colors.Purple, EMPTY_LANGUAGE_FEEDBACK_LABEL, editor);
                return;
            }

            if (ExistsLanguage(newLanguage))
            {
                ShowFeedback(LANGUAGES_LABEL, Colors.Purple, string.Format(LANGUAGE_EXIST_FEEDBACK_LABEL, newLanguage), editor);
                return;
            }

            if (newLanguage.Length > MAX_LANGUAGE_CHARACTERS)
            {
                ShowFeedback(LANGUAGES_LABEL, Colors.Purple, string.Format(CHARACTERS_NUMBER_LANGUAGE_FEEDBACK_LABEL, MAX_LANGUAGE_CHARACTERS), editor);
                return;
            }

            editor?.ClearAddTextField();

            // Add category to Database
            CacheDataSO.InsertLanguage(newLanguage);

            ShowFeedback(LANGUAGES_LABEL, Colors.Purple, string.Format(LANGUAGE_ADDED_FEEDBACK_LABEL, newLanguage), editor);
        }

        public static void RemoveLanguage(string language)
        {
            // Remove from database
            CacheDataSO.RemoveLanguage(language);
        }

        public static void EmptyLanguage(string language)
        {
            // Empty language in database
            CacheDataSO.EmptyLanguage(language);
        }

        public static void ChangeLanguageValue(string oldLanguageName, string newLanguageName)
        {
            if (CurrentLanguageInDictionarySection.Equals(oldLanguageName)) CurrentLanguageInDictionarySection = newLanguageName;

            // Update database
            CacheDataSO.UpdateLanguageName(oldLanguageName, newLanguageName);

            //Log($"Language '{oldLanguageName}' update to '{newLanguageName}' correctly");
        }

        public static void ChangeDefaultLanguage(string newDefaultLanguage)
        {
            // Update database
            CacheDataSO.SetDefaultLanguage(newDefaultLanguage);
        }

        public static void ChangeLanguageIndex(string languageName, int oldIndex, int newIndex)
        {
            if (newIndex <= 1) return;
            if (oldIndex == newIndex) return; //the new index is the same

            newIndex = Math.Clamp(newIndex, 1, CacheDataSO.LanguageCache.Count);

            // Update database
            CacheDataSO.UpdateLanguageDisplayOrder(languageName, oldIndex, newIndex);

            Log(LANGUAGES_LABEL, Colors.Purple, string.Format(LANGUAGE_INDEX_CHANGED_LOG, languageName, newIndex));
        }

        public static bool IsDefaultLanguage(string language)
        {
            return CacheDataSO.DefaultLanguage.Equals(language);
        }

        private static bool ExistsLanguage(string languageName)
        {
            return CacheDataSO.localizationData.LanguagesDictionary.ContainsKey(languageName);
        }

        public static void RefreshLanguagesData()
        {
            CacheDataSO.LoadLanguagesCache();
        }

        #endregion

        #region CATEGORY

        public static void AddNewCategory(string newCategory, EditorWindowAbstract editor = null, bool showEditorLogs = true)
        {
            // Empty value
            if (newCategory.IsEmpty() || newCategory.IsNull())
            {
                if (showEditorLogs) ShowFeedback(CATEGORIES_LABEL, Colors.Green, EMPTY_CATEGORY_FEEDBACK_LABEL, editor);
                return;
            }

            // Already in data
            if (ExistsCategory(newCategory))
            {
                if (showEditorLogs) ShowFeedback(CATEGORIES_LABEL, Colors.Green, string.Format(CATEGORY_EXIST_FEEDBACK_LABEL, newCategory), editor);
                return;
            }

            if (newCategory.Length > MAX_CATEGORY_CHARACTERS)
            {
                if (showEditorLogs) ShowFeedback(CATEGORIES_LABEL, Colors.Green, string.Format(CHARACTERS_NUMBER_CATEGORY_FEEDBACK_LABEL, MAX_CATEGORY_CHARACTERS), editor);
                return;
            }

            editor?.ClearAddTextField();

            // Add category to Database
            CacheDataSO.InsertCategory(newCategory);

            if (showEditorLogs) ShowFeedback(CATEGORIES_LABEL, Colors.Green, string.Format(CATEGORY_ADDED_FEEDBACK_LABEL, newCategory), editor);
        }

        public static void RemoveCategory(string categoryToRemove)
        {
            // Remove from Database
            CacheDataSO.RemoveCategory(categoryToRemove);
        }

        public static void ChangeCategoryName(string oldCategoryName, string newCategoryName)
        {
            // Update database
            CacheDataSO.UpdateCategoryName(oldCategoryName, newCategoryName);
        }

        public void ChangeDefaultCategory(string newDefaultCategory)
        {
            // Update database
            CacheDataSO.SetDefaultCategory(newDefaultCategory);
            OnDefaultCategoryUpdate?.Invoke(newDefaultCategory);
        }

        public static void ChangeCategoryIndex(string categoryName, int oldIndex, int newIndex)
        {
            if (newIndex <= 1) return;
            if (oldIndex == newIndex) return; //the new index is the same

            newIndex = Math.Clamp(newIndex, 1, CacheDataSO.CategoryCache.Count);

            // Update database
            CacheDataSO.UpdateCategoryDisplayOrder(categoryName, oldIndex, newIndex);

            Log(CATEGORIES_LABEL, Colors.Green, string.Format(CATEGORY_INDEX_CHANGED_LOG, categoryName, newIndex));
        }

        public static bool IsDefaultCategory(string category)
        {
            return CacheDataSO.DefaultCategory.Equals(category);
        }

        public static bool ExistsCategory(string categoryName)
        {
            return CacheDataSO.localizationData.CategoriesDictionary.ContainsKey(categoryName);
        }

        public static void RefreshCategoriesData()
        {
            CacheDataSO.LoadCategoriesCache();
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

            await SaveLoadFileManager.SaveFile(_configurationData, CONFIGURATION_PATH, _serializerBinary);
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

            await SaveLoadFileManager.SaveFile(_configurationData, CONFIGURATION_PATH, _serializerBinary);
        }

        public async void UpdateSearchType(int searchTypeIndex)
        {
            if (_configurationData.searchTypeIndex.Equals(searchTypeIndex)) return;

            _configurationData.searchTypeIndex = searchTypeIndex;
            await SaveLoadFileManager.SaveFile(_configurationData, CONFIGURATION_PATH, _serializerBinary);
        }

        public async void UpdateDeleteOrEmptyLanguage(int index)
        {
            if (_configurationData.deleteOrEmptyLanguageIndex.Equals(index)) return;

            _configurationData.deleteOrEmptyLanguageIndex = index;
            await SaveLoadFileManager.SaveFile(_configurationData, CONFIGURATION_PATH, _serializerBinary);
        }

        public async void UpdateShowLog(bool newValue)
        {
            _configurationData.showLogsInConsole = newValue;
            await SaveLoadFileManager.SaveFile(_configurationData, CONFIGURATION_PATH, _serializerBinary);
        }

        #endregion

        #region EXPORT

        public static string BuildCSV(string separator)
        {
            var csvSerializer = new CSV_Serializer();
            csvSerializer.SetSeparator(separator);

            csvSerializer.AddTitle(CacheDataSO.Languages);
            
            foreach (var keyData in CacheDataSO.localizationData.keys)
            {
                var items = new List<string>();

                var category = keyData.category.categoryName;
                items.Add(keyData.keyName);
                items.Add(category);

                var valueList = CacheDataSO.LanguageCache
                    .Select(language => language.TranslationDictionary[keyData.keyName].translationText)
                    .Select(valueToAdd => valueToAdd.Replace("\n", " ").Replace("\r", " ")).ToList();

                items.AddRange(valueList);
                
                csvSerializer.AddLine(items);
            }
            
            return csvSerializer.File();
        }

        public static string BuildSerializedData(ISerializerService serializer)
        {
            var dataToSerialize = new DictionaryTemplate
            {
                categories = CacheDataSO.Categories,
                languages = CacheDataSO.Languages
            };

            foreach (var keyData in CacheDataSO.localizationData.keys)
            {
                var key = keyData.keyName;
                var category = keyData.category.categoryName;
                
                var languageValues = CacheDataSO.LanguageCache
                    .Select(language => new DictionaryTemplate.LanguageValue { Language = language.languageName, Value = language.TranslationDictionary[key].translationText }).ToList();

                dataToSerialize.dictionaryKeyCategoryLanguages.Add(new DictionaryTemplate.KeyCategoryLanguageValues
                {
                    key = key,
                    category = category,
                    languageValues = languageValues
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
            Debug.Assert(header != null, nameof(header) + " == null");
            bytesRead += header.Length + Environment.NewLine.Length;
            progressWindow.SetProgress((float)bytesRead / totalBytes);

            var languages = header?.Split(separator);

            Debug.Assert(languages != null, nameof(languages) + " == null");
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
                ImportLanguage(languages[i]);
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
                if (!ExistsCategory(category)) nCategories++;

                Debug.Assert(lineItems != null, nameof(lineItems) + " != null");
                for (var i = 0; i < lineItems.Length - 2; i++)
                {
                    values.Add(languageOrder[i], lineItems[i + 2]);
                }

                ImportKey(key, category, values);

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

        public static IEnumerator ImportSerializedDataCoroutine(string path, ISerializerService serializer, ImportProgressWindow progressWindow)
        {
            //CacheData.ClearData();

            // Load data
            var loadFileTask = SaveLoadFileManager.LoadFile<DictionaryTemplate>(path, serializer);
            var awaiter = loadFileTask.GetAwaiter();
            while (!awaiter.IsCompleted) yield return null;
            var data = awaiter.GetResult();

            if (data.dictionaryKeyCategoryLanguages.IsEmpty())
            {
                progressWindow.Complete($"Selected file '{path}' IS NOT CORRECT or EMPTY");
                yield break;
            }

            // Items count = languages + categories + keys 
            var totalItems = data.languages.Count + data.categories.Count + data.dictionaryKeyCategoryLanguages.Count;
            var itemCount = 0f;

            //Languages
            if (progressWindow.IsNotNull()) progressWindow.SetStatus(IMPORT_STATUS_LANGUAGES);

            foreach (var language in data.languages)
            {
                ImportLanguage(language);
                if (progressWindow.IsNotNull()) progressWindow.SetProgressInfo(string.Format(IMPORT_PROGRESS_LANGUAGE, language));
                if (progressWindow.IsNotNull()) progressWindow.SetProgress(itemCount++ / totalItems);
                yield return null;
            }

            
            //Categories
            if (progressWindow.IsNotNull()) progressWindow.SetStatus(IMPORT_STATUS_CATEGORIES);
            
            foreach (var category in data.categories)
            {
                ImportCategory(category);
                if (progressWindow.IsNotNull()) progressWindow.SetProgressInfo(string.Format(IMPORT_PROGRESS_CATEGORY, category));
                if (progressWindow.IsNotNull()) progressWindow.SetProgress(itemCount++ / totalItems);
                yield return null;
            }

            // Keys
            if (progressWindow.IsNotNull()) progressWindow.SetStatus(IMPORT_STATUS_KEYS);

            foreach (var keyCategoryLanguage in data.dictionaryKeyCategoryLanguages)
            {
                foreach (var (language, translation) in keyCategoryLanguage.languageValues)
                {
                    ImportKey(keyCategoryLanguage.key, keyCategoryLanguage.category, language, translation);
                }

                if (progressWindow.IsNotNull()) progressWindow.SetProgress(itemCount++ / totalItems);
                if (progressWindow.IsNotNull()) progressWindow.SetProgressInfo(string.Format(IMPORT_PROGRESS_KEY_CATEGORY, keyCategoryLanguage.key, keyCategoryLanguage.category));
                yield return null;
            }

            var sb = new StringBuilder();
            sb.AppendLine(IMPORT_RESULT_SUCCESS);
            sb.AppendLine(string.Format(IMPORT_LANGUAGES_RESULT, data.languages.Count));
            sb.AppendLine(string.Format(IMPORT_CATEGORIES_RESULT, data.categories.Count));
            sb.AppendLine(string.Format(IMPORT_KEYS_RESULT,  data.dictionaryKeyCategoryLanguages.Count));

            if (progressWindow.IsNotNull()) progressWindow.Complete(sb.ToString());
        }

        public static IEnumerator ImportSerializedDataCoroutineForGameMode(string path, Action onComplete)
        {
            //CacheData.ClearData();

            // Load data
            var loadFileTask = SaveLoadFileManager.LoadFile<DictionaryTemplate>(path, new UnityJsonSerializer());
            var awaiter = loadFileTask.GetAwaiter();
            while (!awaiter.IsCompleted) yield return null;
            var data = awaiter.GetResult();

            if (data.dictionaryKeyCategoryLanguages.IsEmpty())
            {
                yield break;
            }

            //Languages
            foreach (var language in data.languages)
            {
                ImportLanguage(language, true);
                yield return null;
            }

            //Categories
            foreach (var category in data.categories)
            {
                ImportCategory(category, true);
                yield return null;
            }
            
            // Keys
            foreach (var keyCategoryLanguage in data.dictionaryKeyCategoryLanguages)
            {
                foreach (var (language, translation) in keyCategoryLanguage.languageValues)
                {
                    ImportKey(keyCategoryLanguage.key, keyCategoryLanguage.category, language, translation, true);
                    yield return null;
                }
                yield return null;
            }

            onComplete?.Invoke();
        }

        public static void ImportLanguage(string languageToImport, bool showLog = true)
        {
            if (languageToImport.IsEmpty()) return;
            if (ExistsLanguage(languageToImport)) return;

            // Add language
            CacheDataSO.InsertLanguage(languageToImport);

            if (CacheDataSO.LanguageCache.Count == 1)
            {
                CurrentLanguageInDictionarySection = languageToImport;
            }

            if (showLog) Log("Import", Colors.Magenta, string.Format(IMPORTED_LANGUAGE_LOG, languageToImport));
        }
        
        public static void ImportCategory(string categoryToImport, bool showLog = true)
        {
            if (categoryToImport.IsEmpty()) return;
            if(ExistsCategory(categoryToImport)) return;
            
            CacheDataSO.InsertCategory(categoryToImport);
            
            if(showLog) Log("Import", Colors.Magenta, string.Format(IMPORTED_CATEGORY_LOG, categoryToImport));
        }

        /// <summary>
        /// Used while importing by CSV
        /// </summary>
        /// <param name="keyToImport"></param>
        /// <param name="category"></param>
        /// <param name="values"></param>
        private static void ImportKey(string keyToImport, string category, Dictionary<string, string> values)
        {
            if (keyToImport.IsEmpty()) return;
            Log("Import", Colors.Magenta, string.Format(IMPORTED_KEY_LOG, keyToImport, category, string.Join(" | ", values)));

            if (ExistsKey(keyToImport))
            {
                //Change category
                if (CacheDataSO.localizationData.KeysDictionary[keyToImport].category.categoryName.NotEquals(category))
                {
                    if (!ExistsCategory(category)) AddNewCategory(category.IsEmpty() ? CacheDataSO.DefaultCategory : category, null, false);

                    CacheDataSO.UpdateKeyCategory(keyToImport, category);
                }

                //Update values
                foreach (var (language, translation) in values)
                {
                    CacheDataSO.SetKeyTranslation(keyToImport, translation, language);
                }
            }
            else
            {
                //Add new key
                if (!ExistsCategory(category)) AddNewCategory(category.IsEmpty() ? CacheDataSO.DefaultCategory : category, null, false);

                CacheDataSO.InsertKey(keyToImport, category);
                foreach (var (language, translation) in values)
                {
                    CacheDataSO.SetKeyTranslation(keyToImport, translation, language);
                }
            }
        }
        
        public static void ImportKey(string keyToImport, string category, string language, string translation, bool showLog = true)
        {
            if (keyToImport.IsEmpty()) return;
            Log("Import", Colors.Magenta, string.Format(IMPORTED_KEY_SINGLE_LOG, keyToImport, category, language, translation));

            if (ExistsKey(keyToImport))
            {
                //Change category
                if (!ExistsCategory(category)) AddNewCategory(category.IsEmpty() ? CacheDataSO.DefaultCategory : category, null, false);

                if (CacheDataSO.localizationData.KeysDictionary[keyToImport].category.categoryName.NotEquals(category))
                {
                    //Change category
                    CacheDataSO.UpdateKeyCategory(keyToImport, category);
                }

                //Update value
                CacheDataSO.SetKeyTranslation(keyToImport, translation, language);
            }
            else
            {
                //Add new key
                if (!ExistsCategory(category)) AddNewCategory(category.IsEmpty() ? CacheDataSO.DefaultCategory : category, null, false);

                CacheDataSO.InsertKey(keyToImport, category);
                CacheDataSO.SetKeyTranslation(keyToImport, translation, language);
            }
        }

        #endregion

        #region LOGS

        public static void Log(string log)
        {
            if (_configurationData.IsNull()) return;

            try
            {
                if (_configurationData.showLogsInConsole) Debug.Log(log);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public static void Log(string title, Colors color, string log)
        {
            if (_configurationData.IsNull()) return;

            try
            {
                if (_configurationData.showLogsInConsole) CustomDebug.CustomDebug.Log(title, color, log);
            }
            catch (Exception e)
            {
                CustomDebug.CustomDebug.LogError(title, color, e);
            }
        }

        public static void LogWarning(string log)
        {
            if (_configurationData.IsNull()) return;

            try
            {
                if (_configurationData.showLogsInConsole) Debug.LogWarning(log);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public static void LogWarning(string title, Colors color, string log)
        {
            if (_configurationData.IsNull()) return;

            try
            {
                if (_configurationData.showLogsInConsole) CustomDebug.CustomDebug.LogWarning(title, color, log);
            }
            catch (Exception e)
            {
                CustomDebug.CustomDebug.LogWarning(title, color, e);
            }
        }

        public static void LogError(string log)
        {
            if (_configurationData.IsNull()) return;

            try
            {
                if (_configurationData.showLogsInConsole) Debug.LogError(log);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public static void LogError(string title, Colors color, string log)
        {
            if (_configurationData.IsNull()) return;

            try
            {
                if (_configurationData.showLogsInConsole) CustomDebug.CustomDebug.LogError(title, color, log);
            }
            catch (Exception e)
            {
                CustomDebug.CustomDebug.LogError(title, color, e);
            }
        }

        #endregion

        #endregion

        #region PRIVATE METHODS

        private static void ShowFeedback(string title, Colors color, string text, EditorWindowAbstract editor)
        {
            Log(title, color, text);
            editor?.ControlFeedbackLabel(text);
        }

        private static void ShowFeedback(string text, RichTextEditor editor)
        {
            Log(DICTIONARY_LABEL, Colors.Blue, text);
            editor.ControlFeedbackLabel(text);
        }

        #region LOAD FROM MEMORY

        public void InitForEditor()
        {
            if (_isInitialized) return; // Just the first call
            _isInitialized = true;

            _serializerBinary ??= new BinarySerializer();
            LoadConfigurationDataFromMemory();

            Log("Localization Tool", Colors.Yellow, TOOL_INITIALIZED_LOG);
        }

        private async void LoadConfigurationDataFromMemory()
        {
            _configurationData = null;
            _configurationData = await SaveLoadFileManager.LoadFile<ConfigurationData>(CONFIGURATION_PATH, _serializerBinary);

            if (_configurationData.IsNotNull()) return;

            _configurationData = new ConfigurationData
            {
                dictionaryDeleteConfirmation = true,
                categoryDeleteConfirmation = true,
                languageDeleteConfirmation = true,
                dictionaryClearAdd = true,
                languageClearAdd = true,
                categoryClearAdd = true,
                searchTypeIndex = 0,
                deleteOrEmptyLanguageIndex = 0,
                showLogsInConsole = true
            };
            await SaveLoadFileManager.SaveFile(_configurationData, CONFIGURATION_PATH, _serializerBinary);
        }

        #endregion

        #endregion
    }
}
#endif