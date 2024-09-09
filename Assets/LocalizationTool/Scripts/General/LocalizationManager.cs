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
using LocalizationTool.Scripts.Data.TemplatesForSerializer;
using static LocalizationTool.Scripts.Commons.EditorStrings;
using static LocalizationTool.Scripts.Commons.EditorPaths;
using static LocalizationTool.Database.DatabaseStrings;

namespace LocalizationTool.Scripts.General
{
    public class LocalizationManager
    {
        #region PUBLIC VARIABLES

        public static LocalizationManager Instance => _instance ??= new LocalizationManager();

        public static ConfigurationData Configuration => _configurationData ?? new ConfigurationData();

        public static string CurrentLanguageInDictionarySection { get; set; }
        public static int CurrentToolbarLanguageIndex => CacheData.Languages.IndexOf(CurrentLanguageInDictionarySection);

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

        #region DATABASE

        public static void CreateDatabase()
        {
            if (File.Exists(DATABASE_PATH)) return;
            try
            {
                CacheData.OpenConnection();
                // Table category
                var query =
                    $"CREATE TABLE IF NOT EXISTS {CATEGORY_TABLE} (id INTEGER PRIMARY KEY AUTOINCREMENT, displayOrder INTERGER NOT NULL , category VARCHAR({MAX_CATEGORY_CHARACTERS}) NOT NULL UNIQUE, isDefault INTEGER NOT NULL)";
                CacheData.ExecuteNonQueryCommand(query);

                // Insert None value
                query = $"INSERT INTO {CATEGORY_TABLE} (displayOrder, category, isDefault) VALUES (1, 'None', 1)";
                CacheData.ExecuteNonQueryCommand(query);

                // Table language
                query =
                    $"CREATE TABLE IF NOT EXISTS {LANGUAGE_TABLE} (id INTEGER PRIMARY KEY AUTOINCREMENT, displayOrder INTERGER NOT NULL, language VARCHAR({MAX_LANGUAGE_CHARACTERS}) NOT NULL UNIQUE, isDefault INTEGER NOT NULL)";
                CacheData.ExecuteNonQueryCommand(query);

                // Insert English value
                query = $"INSERT INTO {LANGUAGE_TABLE} (displayOrder, language, isDefault) VALUES (1, 'English', 1)";
                CacheData.ExecuteNonQueryCommand(query);
                CurrentLanguageInDictionarySection = "English";

                //Table translations_keys
                query =
                    $"CREATE TABLE IF NOT EXISTS {TRANSLATION_KEY_TABLE} (id INTEGER PRIMARY KEY AUTOINCREMENT, key_name VARCHAR({MAX_KEY_CHARACTERS}) NOT NULL UNIQUE, categoryID INTEGER references {CATEGORY_TABLE}(id), displayOrder INTERGER NOT NULL)";
                CacheData.ExecuteNonQueryCommand(query);

                // Table translation
                query = $"CREATE TABLE IF NOT EXISTS {TRANSLATION_TABLE} (id INTEGER PRIMARY KEY AUTOINCREMENT," +
                        $" keyID INTEGER references {TRANSLATION_KEY_TABLE}(id), languageID INTEGER references {LANGUAGE_TABLE}(id), translationText TEXT)";
                CacheData.ExecuteNonQueryCommand(query);
            }
            catch (Exception)
            {
                //IGNORE
            }
            finally
            {
                CacheData.CloseConnection();
            }
        }

        #endregion

        #region DICTIONARY

        public static void AddNewKey(string key, string category, EditorWindowAbstract editor = null)
        {
            if (key.IsEmpty() || key.IsNull())
            {
                ShowFeedback(EMPTY_KEY_FEEDBACK_LABEL, editor);
                return;
            }
            
            if (ContainsKey(key))
            {
                ShowFeedback(string.Format(KEY_EXIST_FEEDBACK_LABEL, key), editor);
                return;
            }

            if (key.Length > MAX_KEY_CHARACTERS)
            {
                ShowFeedback(string.Format(CHARACTERS_NUMBER_KEY_FEEDBACK_LABEL, MAX_KEY_CHARACTERS), editor);
                return;
            }

            editor?.ClearAddTextField();

            // Add key to database
            CacheData.InsertKeyToDatabase(key, category);

            // Update cache 
            CacheData.UpdateDictionaryCache();

            ShowFeedback(string.Format(KEY_ADDED_FEEDBACK_LABEL, key), editor);
        }

        public static string ChangeKey(string oldKeyName, string newKeyName, RichTextEditor editor)
        {
            if (oldKeyName.Equals(newKeyName)) return oldKeyName;

            if (newKeyName.IsEmpty() || newKeyName.IsNull())
            {
                ShowFeedback(EMPTY_KEY_FEEDBACK_LABEL, editor);
                return oldKeyName;
            }
            
            if (ContainsKey(newKeyName))
            {
                ShowFeedback(string.Format(KEY_EXIST_FEEDBACK_LABEL, oldKeyName), editor);
                return oldKeyName;
            }

            if (newKeyName.Length > MAX_KEY_CHARACTERS)
            {
                ShowFeedback(string.Format(CHARACTERS_NUMBER_KEY_FEEDBACK_LABEL, MAX_KEY_CHARACTERS), editor);
                return oldKeyName;
            }

            // Update database
            CacheData.UpdateKeyNameInDatabase(oldKeyName, newKeyName);

            // Update cache 
            CacheData.UpdateDictionaryCache();

            UpdateAddonsOnKeyUpdated(oldKeyName, newKeyName);

            return newKeyName;
        }

        public static void ChangeValue(string key, string newTranslation)
        {
            if (!ContainsKey(key)) return;
            if (CacheData.DictionaryCache[key].TranslationData[CurrentLanguageInDictionarySection].Equals(newTranslation)) return;

            // Update database
            CacheData.UpdateKeyTranslationInDatabase(key, newTranslation, CurrentLanguageInDictionarySection);

            // Update cache 
            CacheData.UpdateDictionaryCache();
        }

        public static void ChangeCategory(string key, string newCategory)
        {
            if (CacheData.DictionaryCache[key].Category == newCategory) return;

            // Update database
            CacheData.UpdateKeyCategoryInDatabase(key, newCategory);

            // Update cache 
            CacheData.UpdateDictionaryCache();

            Log(string.Format(DICTIONARY_KEY_CATEGORY_CHANGED_LOG, key, newCategory));
        }

        public static void RemoveKey(string key)
        {
            // Update database
            CacheData.RemoveKeyFromDatabase(key);

            // Update cache 
            CacheData.UpdateDictionaryCache();

            UpdateAddonsOnKeyRemoved(key);
        }

        private static bool ContainsKey(string key)
        {
            return CacheData.DictionaryCache.FirstOrDefault(x => x.Key.Equals(key)).Key.IsNotNull();
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
            CacheData.LoadDictionaryCacheFromDatabase();
        }

        #endregion

        #region LANGUAGE

        public static void AddNewLanguage(string newLanguage, EditorWindowAbstract editor = null)
        {
            if (newLanguage.IsEmpty() || newLanguage.IsNull())
            {
                ShowFeedback(EMPTY_LANGUAGE_FEEDBACK_LABEL, editor);
                return;
            }
            
            if (ContainsLanguage(newLanguage))
            {
                ShowFeedback(string.Format(LANGUAGE_EXIST_FEEDBACK_LABEL, newLanguage), editor);
                return;
            }

            if (newLanguage.Length > MAX_LANGUAGE_CHARACTERS)
            {
                ShowFeedback(string.Format(CHARACTERS_NUMBER_LANGUAGE_FEEDBACK_LABEL, MAX_LANGUAGE_CHARACTERS), editor);
                return;
            }
            
            editor?.ClearAddTextField();

            // Add category to Database
            CacheData.InsertLanguageToDatabase(newLanguage);
            
            // Update cache 
            CacheData.UpdateLanguageCache();

            ShowFeedback(string.Format(LANGUAGE_ADDED_FEEDBACK_LABEL, newLanguage), editor);
        }

        public static void RemoveLanguage(string language)
        {
            // Remove from database
            CacheData.RemoveLanguageFromDatabase(language);

            // Update cache 
            CacheData.UpdateLanguageCache();
            CacheData.UpdateDictionaryCache();
        }

        public static void EmptyLanguage(string language)
        {
            // Empty language in database
            CacheData.EmptyLanguageFromDatabase(language);
            
            // Update cache 
            CacheData.UpdateDictionaryCache();
        }
        
        public static void ChangeLanguageValue(string oldLanguageName, string newLanguageName)
        {
            if (CurrentLanguageInDictionarySection.Equals(oldLanguageName)) CurrentLanguageInDictionarySection = newLanguageName;

            // Update database
            CacheData.UpdateLanguageNameInDatabase(oldLanguageName, newLanguageName);

            // Update cache 
            CacheData.UpdateLanguageCache();

            //Log($"Language '{oldLanguageName}' update to '{newLanguageName}' correctly");
        }

        public static void ChangeDefaultLanguage(string newDefaultLanguage)
        {
            // Update database
            CacheData.UpdateDefaultLanguageInDatabase(newDefaultLanguage);

            // Update cache 
            CacheData.UpdateLanguageCache();
            CacheData.DefaultLanguage = newDefaultLanguage;
        }

        public static void ChangeLanguageIndex(string languageName, int oldIndex, int newIndex)
        {
            if (newIndex <= 1) return;
            if (oldIndex == newIndex) return; //the new index is the same

            // Update database
            CacheData.UpdateLanguageDisplayOrderInDatabase(languageName, oldIndex, newIndex);

            // Update cache 
            CacheData.UpdateLanguageCache();

            Log(string.Format(LANGUAGE_INDEX_CHANGED_LOG, languageName, newIndex));

            //GUI.FocusControl(null);
        }

        public static bool IsDefaultLanguage(string language)
        {
            return CacheData.DefaultLanguage.Equals(language);
        }

        private static bool ContainsLanguage(string language)
        {
            return CacheData.LanguageCache.FirstOrDefault(x => x.Language.Equals(language)).Language.IsNotNull();
        }

        public static void RefreshLanguagesData()
        {
            CacheData.LoadLanguagesCacheFromDatabase();
        }

        #endregion

        #region CATEGORY

        public static void AddNewCategory(string newCategory, EditorWindowAbstract editor = null, bool showEditorLogs = true)
        {
            // Empty value
            if (newCategory.IsEmpty() || newCategory.IsNull())
            {
                if (showEditorLogs) ShowFeedback(EMPTY_CATEGORY_FEEDBACK_LABEL, editor);
                return;
            }
            
            // Already in data
            if (ContainsCategory(newCategory))
            {
                if (showEditorLogs) ShowFeedback(string.Format(CATEGORY_EXIST_FEEDBACK_LABEL, newCategory), editor);
                return;
            }

            if (newCategory.Length > MAX_CATEGORY_CHARACTERS)
            {
                if (showEditorLogs) ShowFeedback(string.Format(CHARACTERS_NUMBER_CATEGORY_FEEDBACK_LABEL, MAX_CATEGORY_CHARACTERS), editor);
                return;
            }

            editor?.ClearAddTextField();

            // Add category to Database
            CacheData.InsertCategoryToDatabase(newCategory);
            
            // Update cache 
            CacheData.UpdateCategoryCache();

            if (CacheData.CategoryCache.Count() == 1) Instance.ChangeDefaultCategory(newCategory);
            
            if (showEditorLogs) ShowFeedback(string.Format(CATEGORY_ADDED_FEEDBACK_LABEL, newCategory), editor);
        }

        public static void RemoveCategory(string categoryToRemove)
        {
            // Remove from Database
            CacheData.RemoveCategoryFromDatabase(categoryToRemove);

            // Update cache
            CacheData.UpdateCategoryCache();
            CacheData.UpdateDictionaryCache();
        }

        public static void ChangeCategoryName(string oldCategoryName, string newCategoryName)
        {
            // Update database
            CacheData.UpdateCategoryNameInDatabase(oldCategoryName, newCategoryName);

            // Update cache 
            CacheData.UpdateCategoryCache();
        }

        public void ChangeDefaultCategory(string newDefaultCategory)
        {
            // Update database
            CacheData.UpdateDefaultCategoryInDatabase(newDefaultCategory);

            // Update cache 
            CacheData.UpdateCategoryCache();
            CacheData.DefaultCategory = newDefaultCategory;

            OnDefaultCategoryUpdate?.Invoke(newDefaultCategory);
        }

        public static void ChangeCategoryIndex(string categoryName, int oldIndex, int newIndex)
        {
            if (newIndex <= 1) return;
            if (oldIndex == newIndex) return; //the new index is the same

            // Update database
            CacheData.UpdateCategoryDisplayOrderInDatabase(categoryName, oldIndex, newIndex);

            // Update cache 
            CacheData.UpdateCategoryCache();

            Log(string.Format(CATEGORY_INDEX_CHANGED_LOG, categoryName, newIndex));
        }

        public static bool IsDefaultCategory(string category)
        {
            return CacheData.DefaultCategory.Equals(category);
        }

        public static bool ContainsCategory(string category)
        {
            return CacheData.CategoryCache.FirstOrDefault(x => x.Category.Equals(category)).Category.IsNotNull();
        }

        public static void RefreshCategoriesData()
        {
            CacheData.LoadCategoriesCacheFromDatabase();
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
            var serializer = new CSV_Serializer();
            serializer.SetSeparator(separator);

            serializer.AddTitle(CacheData.Languages);

            var languageDictionary = new Dictionary<string, int>();
            for (var i = 0; i < CacheData.LanguageCache.Count; i++)
            {
                languageDictionary.Add(CacheData.Languages[i], i);
            }

            var data = new Dictionary<string, CacheData.KeyData>(CacheData.DictionaryCache);

            foreach (var (key, keyData) in data)
            {
                var items = new List<string>();
                var category = keyData.Category;

                items.Add(key);
                items.Add(category);

                var auxArray = new string[languageDictionary.Count];
                foreach (var (language, value) in keyData.TranslationData)
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

            var data = new Dictionary<string, CacheData.KeyData>(CacheData.DictionaryCache);

            foreach (var (key, keyData) in data)
            {
                var category = keyData.Category;
                var languageValues = keyData.TranslationData.Select(item => new DictionaryTemplate.LanguageValue { Language = item.Key, Value = item.Value }).ToList()
                    .OrderBy(x => CacheData.LanguageCache.First(y => y.Language.Equals(x.Language)).DisplayOrder).ToList();
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
                if (!ContainsCategory(category)) nCategories++;

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

            if (data.DictionaryKeyCategoryLanguages.IsEmpty())
            {
                progressWindow.Complete($"Selected file '{path}' IS NOT CORRECT or EMPTY");
                yield break;
            }

            // Items count
            var totalItems = data.DictionaryKeyCategoryLanguages[0].LanguageValues.Count + data.DictionaryKeyCategoryLanguages.Count;
            var itemCount = 0f;

            //Languages
            if (progressWindow.IsNotNull()) progressWindow.SetStatus(IMPORT_STATUS_LANGUAGES);

            foreach (var languageValue in data.DictionaryKeyCategoryLanguages[0].LanguageValues)
            {
                ImportLanguage(languageValue.Language);
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
                if (!ContainsCategory(keyCategoryLanguage.Category)) nCategories++;

                foreach (var (language, translation) in keyCategoryLanguage.LanguageValues)
                {
                    ImportKey(keyCategoryLanguage.Key, keyCategoryLanguage.Category, language, translation);
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
        }

        public static IEnumerator ImportSerializedDataCoroutineForGameMode(string path, Action onComplete)
        {
            //CacheData.ClearData();
            
            // Load data
            var loadFileTask = SaveLoadFileManager.LoadFile<DictionaryTemplate>(path, new UnityJsonSerializer());
            var awaiter = loadFileTask.GetAwaiter();
            while (!awaiter.IsCompleted) yield return null;
            var data = awaiter.GetResult();

            if (data.DictionaryKeyCategoryLanguages.IsEmpty())
            {
                yield break;
            }
            
            //Languages
            foreach (var languageValue in data.DictionaryKeyCategoryLanguages[0].LanguageValues)
            {
                ImportLanguage(languageValue.Language, false);
                yield return null;
            }
            
            // Keys
            foreach (var keyCategoryLanguage in data.DictionaryKeyCategoryLanguages)
            {
                foreach (var (language, translation) in keyCategoryLanguage.LanguageValues)
                {
                    ImportKey(keyCategoryLanguage.Key, keyCategoryLanguage.Category, language, translation, false);
                }
                
                yield return null;
            }

            onComplete?.Invoke();
        }
        
        public static void ImportLanguage(string languageToImport, bool showLog = true)
        {
            if (ContainsLanguage(languageToImport)) return;

            // Add category to Database
            CacheData.InsertLanguageToDatabase(languageToImport);

            // Update cache 
            CacheData.UpdateLanguageCache();

            if (CacheData.LanguageCache.Count() == 1)
            {
                ChangeDefaultLanguage(languageToImport);
                CurrentLanguageInDictionarySection = languageToImport;
            }

            if(showLog) Log(string.Format(IMPORTED_LANGUAGE_LOG, languageToImport));
        }
        
        private static void ImportKey(string keyToImport, string category, Dictionary<string, string> values)
        {
            if (keyToImport.IsEmpty()) return;
            Log(string.Format(IMPORTED_KEY_LOG, keyToImport, category, string.Join(" | ", values)));
            if (ContainsKey(keyToImport))
            {
                //Change category
                if (CacheData.DictionaryCache[keyToImport].Category != category)
                {
                    if (!ContainsCategory(category)) AddNewCategory(category.IsEmpty() ? CacheData.DefaultCategory : category, null, false);

                    CacheData.UpdateKeyCategoryInDatabase(keyToImport, category);
                }

                //Update values
                foreach (var (language, translation) in values)
                {
                    CacheData.UpdateKeyTranslationInDatabase(keyToImport, translation, language);
                }
            }
            else
            {
                //Add new key
                if (!ContainsCategory(category)) AddNewCategory(category.IsEmpty() ? CacheData.DefaultCategory : category, null, false);

                CacheData.InsertKeyToDatabase(keyToImport, category);
                foreach (var (language, translation) in values)
                {
                    CacheData.UpdateKeyTranslationInDatabase(keyToImport, translation, language);
                }
            }

            //Update cache
            CacheData.UpdateDictionaryCache();
        }

        public static void ImportKey(string keyToImport, string category, string language, string translation, bool showLog = true)
        {
            if (keyToImport.Equals("")) return;
            if(showLog) Log(string.Format(IMPORTED_KEY_SINGLE_LOG, keyToImport, category, language, translation));

            ImportLanguage(language, showLog);

            if (ContainsKey(keyToImport))
            {
                //Change category
                if (!ContainsCategory(category)) AddNewCategory(category.IsEmpty() ? CacheData.DefaultCategory : category, null, false);

                if (CacheData.DictionaryCache[keyToImport].Category != category)
                {
                    //Change category
                    CacheData.UpdateKeyCategoryInDatabase(keyToImport, category);
                }

                //Update value
                CacheData.UpdateKeyTranslationInDatabase(keyToImport, translation, language);
            }
            else
            {
                //Add new key
                if (!ContainsCategory(category)) AddNewCategory(category.IsEmpty() ? CacheData.DefaultCategory : category, null, false);

                CacheData.InsertKeyToDatabase(keyToImport, category);
                CacheData.UpdateKeyTranslationInDatabase(keyToImport, translation, language);
            }

            //Update cache
            CacheData.UpdateDictionaryCache();
        }

        #endregion

        #region LOGS

        public static void Log(string log)
        {
            if(_configurationData.IsNull()) return;
            
            try
            {
                if (_configurationData.showLogsInConsole) Debug.Log(log);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public static void LogWarning(string log)
        {
            if(_configurationData.IsNull()) return;
            
            try
            {
                if (_configurationData.showLogsInConsole) Debug.LogWarning(log);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public static void LogError(string log)
        {
            if(_configurationData.IsNull()) return;
            
            try
            {
                if (_configurationData.showLogsInConsole) Debug.LogError(log);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        #endregion
        
        #endregion

        #region PRIVATE METHODS
        
        private static void ShowFeedback(string text, EditorWindowAbstract editor)
        {
            Log(text);
            editor?.ControlFeedbackLabel(text);
        }

        private static void ShowFeedback(string text, RichTextEditor editor)
        {
            Log(text);
            editor.ControlFeedbackLabel(text);
        }

        #region LOAD FROM MEMORY

        public void InitForEditor()
        {
            if (_isInitialized) return; // Just the first call
            _isInitialized = true;

            _serializerBinary ??= new BinarySerializer();
            LoadConfigurationDataFromMemory();

            Log(TOOL_INITIALIZED_LOG);
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