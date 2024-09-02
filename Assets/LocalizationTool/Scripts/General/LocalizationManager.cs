using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LocalizationTool.Data;
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
using System.Data;
using Mono.Data.Sqlite;
using static LocalizationTool.Scripts.Commons.EditorStrings;
using static LocalizationTool.Scripts.Commons.EditorPaths;
using static LocalizationTool.Database.DatabaseStrings;

namespace LocalizationTool.Scripts.General
{
    public class LocalizationManager
    {
        #region PUBLIC VARIABLES

        public static LocalizationManager Instance => _instance ??= new LocalizationManager();

        public static List<LanguageTuple> LanguagesCache;
        public static List<string> Languages => LanguagesCache.Select(x => x.Language).ToList();
        public static string DefaultLanguage { get; private set; }

        public static List<CategoryTuple> CategoriesCache;
        public static List<string> Categories => CategoriesCache.Select(x => x.Category).ToList();
        public static string DefaultCategory { get; private set; }

        public static ConfigurationData Configuration => _configurationData ?? new ConfigurationData();

        public static Dictionary<string, KeyData> DictionaryCache;
        public static List<string> Keys => DictionaryCache.Select(x => x.Key).ToList();

        public static string CurrentLanguageInDictionarySection { get; set; }
        public static int CurrentToolbarLanguageIndex => Languages.IndexOf(CurrentLanguageInDictionarySection);
        public static bool IsDataLoaded { get; private set; }

        #region Actions

        public Action OnLocalizationToolInitialized { get; set; }
        public Action<string> OnDefaultCategoryUpdate { get; set; }

        #endregion

        #endregion

        #region PRIVATE VARIABLES

        private static LocalizationManager _instance;
        private ISerializerService _serializerBinary;

        private static string connectionString;
        private static IDbConnection dbConnection;

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
                ShowFeedback(EMPTY_KEY_FEEDBACK_LABEL, editor);
                return;
            }

            if (key.Contains(" "))
            {
                ShowFeedback(SPACES_KEY_FEEDBACK_LABEL, editor);
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
            InsertKeyToDatabase(key, category);

            // Update cache 
            DictionaryCache = GetKeysTranslationFromDatabase();

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

            if (newKeyName.Contains(" "))
            {
                ShowFeedback(SPACES_KEY_FEEDBACK_LABEL, editor);
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
            UpdateKeyNameInDatabase(oldKeyName, newKeyName);

            // Update cache 
            DictionaryCache = GetKeysTranslationFromDatabase();

            UpdateAddonsOnKeyUpdated(oldKeyName, newKeyName);

            return newKeyName;
        }

        public static void ChangeValue(string key, string newTranslation)
        {
            if (!ContainsKey(key)) return;
            if (DictionaryCache[key].TranslationData[CurrentLanguageInDictionarySection].Equals(newTranslation)) return;

            // Update database
            UpdateKeyTranslationInDatabase(key, newTranslation, CurrentLanguageInDictionarySection);

            // Update cache 
            DictionaryCache = GetKeysTranslationFromDatabase();
        }

        public static void ChangeCategory(string key, string newCategory)
        {
            if (DictionaryCache[key].Category == newCategory) return;

            // Update database
            UpdateKeyCategoryInDatabase(key, newCategory);

            // Update cache 
            DictionaryCache = GetKeysTranslationFromDatabase();

            Log(string.Format(DICTIONARY_KEY_CATEGORY_CHANGED_LOG, key, newCategory));
        }

        public static void RemoveKey(string key)
        {
            // Update database
            RemoveKeyFromDatabase(key);

            // Update cache 
            DictionaryCache = GetKeysTranslationFromDatabase();

            UpdateAddonsOnKeyRemoved(key);
        }

        private static bool ContainsKey(string key)
        {
            return DictionaryCache.FirstOrDefault(x => x.Key.Equals(key)).Key.IsNotNull();
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

        public static void AddNewLanguage(string newLanguage, EditorWindowAbstract editor = null)
        {
            if (newLanguage.IsEmpty() || newLanguage.IsNull())
            {
                ShowFeedback(EMPTY_LANGUAGE_FEEDBACK_LABEL, editor);
                return;
            }

            if (newLanguage.Contains(" "))
            {
                ShowFeedback(SPACES_LANGUAGE_FEEDBACK_LABEL, editor);
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
            InsertLanguageToDatabase(newLanguage);

            // Update cache 
            LanguagesCache = GetLanguagesOrderedFromDatabase();

            ShowFeedback(string.Format(LANGUAGE_ADDED_FEEDBACK_LABEL, newLanguage), editor);
        }

        public static void RemoveLanguage(string language)
        {
            // Remove from database
            RemoveLanguageFromDatabase(language);
            
            // Update cache 
            LanguagesCache = GetLanguagesOrderedFromDatabase();
            DictionaryCache = GetKeysTranslationFromDatabase();
        }

        public static void ChangeLanguageValue(string oldLanguageName, string newLanguageName)
        {
            if (CurrentLanguageInDictionarySection.Equals(oldLanguageName)) CurrentLanguageInDictionarySection = newLanguageName;

            // Update database
            UpdateLanguageNameInDatabase(oldLanguageName, newLanguageName);

            // Update cache 
            LanguagesCache = GetLanguagesOrderedFromDatabase();

            //Log($"Language '{oldLanguageName}' update to '{newLanguageName}' correctly");
        }

        public static void ChangeDefaultLanguage(string newDefaultLanguage)
        {
            // Update database
            UpdateDefaultLanguageInDatabase(newDefaultLanguage);

            // Update cache 
            LanguagesCache = GetLanguagesOrderedFromDatabase();
            DefaultLanguage = newDefaultLanguage;
        }

        public static void ChangeLanguageIndex(string languageName, int oldIndex, int newIndex)
        {
            if (newIndex <= 1) return;
            if (oldIndex == newIndex) return; //the new index is the same

            // Update database
            UpdateLanguageDisplayOrderInDatabase(languageName, oldIndex, newIndex);

            // Update cache 
            LanguagesCache = GetLanguagesOrderedFromDatabase();

            Log(string.Format(LANGUAGE_INDEX_CHANGED_LOG, languageName, newIndex));

            //GUI.FocusControl(null);
        }

        public static bool IsDefaultLanguage(string language)
        {
            return DefaultLanguage.Equals(language);
        }

        private static bool ContainsLanguage(string language)
        {
            return LanguagesCache.FirstOrDefault(x => x.Language.Equals(language)).Language.IsNotNull();
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

            if (newCategory.Contains(" "))
            {
                if (showEditorLogs) ShowFeedback(SPACES_CATEGORY_FEEDBACK_LABEL, editor);
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
            InsertCategoryToDatabase(newCategory);

            // Update cache 
            CategoriesCache = GetCategoriesOrderedFromDatabase();

            if (showEditorLogs) ShowFeedback(string.Format(CATEGORY_ADDED_FEEDBACK_LABEL, newCategory), editor);
        }

        public static void RemoveCategory(string categoryToRemove)
        {
            // Remove from Database
            RemoveCategoryFromDatabase(categoryToRemove);
            
            // Update cache
            CategoriesCache = GetCategoriesOrderedFromDatabase();
            DictionaryCache = GetKeysTranslationFromDatabase();
        }

        public static void ChangeCategoryName(string oldCategoryName, string newCategoryName)
        {
            // Update database
            UpdateCategoryNameInDatabase(oldCategoryName, newCategoryName);

            // Update cache 
            CategoriesCache = GetCategoriesOrderedFromDatabase();
        }

        public void ChangeDefaultCategory(string newDefaultCategory)
        {
            // Update database
            UpdateDefaultCategoryInDatabase(newDefaultCategory);

            // Update cache 
            CategoriesCache = GetCategoriesOrderedFromDatabase();
            DefaultCategory = newDefaultCategory;

            OnDefaultCategoryUpdate?.Invoke(newDefaultCategory);
        }

        public static void ChangeCategoryIndex(string categoryName, int oldIndex, int newIndex)
        {
            if (newIndex <= 1) return;
            if (oldIndex == newIndex) return; //the new index is the same

            // Update database
            UpdateCategoryDisplayOrderInDatabase(categoryName, oldIndex, newIndex);

            // Update cache 
            CategoriesCache = GetCategoriesOrderedFromDatabase();

            Log(string.Format(CATEGORY_INDEX_CHANGED_LOG, categoryName, newIndex));
        }

        public static bool IsDefaultCategory(string category)
        {
            return DefaultCategory.Equals(category);
        }

        public static bool ContainsCategory(string category)
        {
            return CategoriesCache.FirstOrDefault(x => x.Category.Equals(category)).Category.IsNotNull();
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

            serializer.AddTitle(Languages);

            var languageDictionary = new Dictionary<string, int>();
            for (var i = 0; i < LanguagesCache.Count; i++)
            {
                languageDictionary.Add(Languages[i], i);
            }

            var data = new Dictionary<string, KeyData>(DictionaryCache);

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

            var data = new Dictionary<string, KeyData>(DictionaryCache);

            foreach (var (key, keyData) in data)
            {
                var category = keyData.Category;
                var languageValues = keyData.TranslationData.Select(item => new DictionaryTemplate.LanguageValue { Language = item.Key, Value = item.Value }).ToList()
                    .OrderBy(x => LanguagesCache.First(y => y.Language.Equals(x.Language)).DisplayOrder).ToList();
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

        public static IEnumerator ImportSerializedDataCoroutine(string path, ISerializerService serializer, ImportProgressWindow progressWindow, Action onComplete = null)
        {
            // Load data
            var loadFileTask = LoadFile<DictionaryTemplate>(path, serializer);
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

            onComplete?.Invoke();
        }

        public static void ImportLanguage(string languageToImport)
        {
            if (ContainsLanguage(languageToImport)) return;
            
            // Add category to Database
            InsertLanguageToDatabase(languageToImport);

            // Update cache 
            LanguagesCache = GetLanguagesOrderedFromDatabase();
            
            if (LanguagesCache.Count() == 1)
            {
                DefaultLanguage = languageToImport;
                CurrentLanguageInDictionarySection = languageToImport;
            }
            
            Log(string.Format(IMPORTED_LANGUAGE_LOG, languageToImport));
        }

        private static void ImportKey(string keyToImport, string category, Dictionary<string, string> values)
        {
            if (keyToImport.IsEmpty()) return;
            Log(string.Format(IMPORTED_KEY_LOG, keyToImport, category, string.Join(" | ", values)));
            if (ContainsKey(keyToImport))
            {
                //Change category
                if (DictionaryCache[keyToImport].Category != category)
                {
                     if (!ContainsCategory(category)) AddNewCategory(category.IsEmpty() ? DefaultCategory : category, null, false);

                     UpdateKeyCategoryInDatabase(keyToImport, category);
                }

                //Update values
                foreach (var (language, translation) in values)
                {
                    UpdateKeyTranslationInDatabase(keyToImport, translation, language);
                }
            }
            else
            {
                //Add new key
                if (!ContainsCategory(category)) AddNewCategory(category.IsEmpty() ? DefaultCategory : category, null, false);

                InsertKeyToDatabase(keyToImport, category);
                foreach (var (language, translation) in values)
                {
                    UpdateKeyTranslationInDatabase(keyToImport, translation, language);
                }
            }
            
            //Update cache
            DictionaryCache = GetKeysTranslationFromDatabase();
        }

        public static void ImportKey(string keyToImport, string category, string language, string translation)
        {
            if (keyToImport.Equals("")) return;
            Log(string.Format(IMPORTED_KEY_SINGLE_LOG, keyToImport, category, language, translation));
            
            ImportLanguage(language);

            if (ContainsKey(keyToImport))
            {
                //Change category
                if (!ContainsCategory(category)) AddNewCategory(category.IsEmpty() ? DefaultCategory : category, null, false);

                if (DictionaryCache[keyToImport].Category != category)
                {
                    //Change category
                    UpdateKeyCategoryInDatabase(keyToImport, category);
                }

                //Update value
                UpdateKeyTranslationInDatabase(keyToImport, translation, language);
            }
            else
            {
                //Add new key
                if (!ContainsCategory(category)) AddNewCategory(category.IsEmpty() ? DefaultCategory : category, null, false);
                
                InsertKeyToDatabase(keyToImport, category);
                UpdateKeyTranslationInDatabase(keyToImport, translation, language);
            }
            
            //Update cache
            DictionaryCache = GetKeysTranslationFromDatabase();
        }

        #endregion

        #region DATABASE

        private static void OpenConnection()
        {
            connectionString = "URI=file:" + DATABASE_PATH;
            dbConnection = new SqliteConnection(connectionString);
            dbConnection.Open();
        }

        private static void CloseConnection()
        {
            if (dbConnection == null) return;

            dbConnection.Close();
            dbConnection = null;
        }

        public static void CreateDatabase()
        {
            if (File.Exists(DATABASE_PATH)) return;
            try
            {
                OpenConnection();
                // Table category
                var query =
                    $"CREATE TABLE IF NOT EXISTS {CATEGORY_TABLE} (id INTEGER PRIMARY KEY AUTOINCREMENT, displayOrder INTERGER NOT NULL , category VARCHAR({MAX_CATEGORY_CHARACTERS}) NOT NULL UNIQUE, isDefault INTEGER NOT NULL)";
                ExecuteNonQueryCommand(query);

                // Insert None value
                query = $"INSERT INTO {CATEGORY_TABLE} (displayOrder, category, isDefault) VALUES (1, 'None', 1)";
                ExecuteNonQueryCommand(query);

                // Table language
                query =
                    $"CREATE TABLE IF NOT EXISTS {LANGUAGE_TABLE} (id INTEGER PRIMARY KEY AUTOINCREMENT, displayOrder INTERGER NOT NULL, language VARCHAR({MAX_LANGUAGE_CHARACTERS}) NOT NULL UNIQUE, isDefault INTEGER NOT NULL)";
                ExecuteNonQueryCommand(query);

                // Insert English value
                query = $"INSERT INTO {LANGUAGE_TABLE} (displayOrder, language, isDefault) VALUES (1, 'English', 1)";
                ExecuteNonQueryCommand(query);
                CurrentLanguageInDictionarySection = "English";

                //Table translations_keys
                query =
                    $"CREATE TABLE IF NOT EXISTS {TRANSLATION_KEY_TABLE} (id INTEGER PRIMARY KEY AUTOINCREMENT, key_name VARCHAR({MAX_KEY_CHARACTERS}) NOT NULL UNIQUE, categoryID INTEGER references {CATEGORY_TABLE}(id), displayOrder INTERGER NOT NULL)";
                ExecuteNonQueryCommand(query);

                // Table translation
                query = $"CREATE TABLE IF NOT EXISTS {TRANSLATION_TABLE} (id INTEGER PRIMARY KEY AUTOINCREMENT," +
                        $" keyID INTEGER references {TRANSLATION_KEY_TABLE}(id), languageID INTEGER references {LANGUAGE_TABLE}(id), translationText TEXT)";
                ExecuteNonQueryCommand(query);
            }
            catch (Exception e)
            {
                //IGNORE
            }
            finally
            {
                CloseConnection();
            }
        }

        private static void DeleteDatabase()
        {
            try
            {
                OpenConnection();
                //Delete translation
                var query =   $"DELETE * FROM {TRANSLATION_TABLE}";
                ExecuteNonQueryCommand(query);

                //Delete translations_keys
                query = $"DELETE * FROM {TRANSLATION_KEY_TABLE}";
                ExecuteNonQueryCommand(query);
                
                // Delete category
                query = $"DELETE * FROM {CATEGORY_TABLE}";
                ExecuteNonQueryCommand(query);

                // Delete language
                query = $"DELETE * FROM {LANGUAGE_TABLE}";
                ExecuteNonQueryCommand(query);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }
        }

        #endregion

        #region STRUCTURES

        public struct KeyData
        {
            public string Category;
            public Dictionary<string, string> TranslationData;
        }

        public struct CategoryTuple
        {
            public int DisplayOrder;
            public string Category;

            public void Deconstruct(out int displayOrder, out string category)
            {
                displayOrder = DisplayOrder;
                category = Category;
            }
        }

        public struct LanguageTuple
        {
            public int DisplayOrder;
            public string Language;

            public void Deconstruct(out int displayOrder, out string language)
            {
                displayOrder = DisplayOrder;
                language = Language;
            }
        }

        #endregion

        #region REFRESH DATA

        public static void RefreshDictionaryData()
        {
            LoadDictionaryCacheFromDatabase();
        }

        public static void RefreshLanguagesData()
        {
            LoadLanguagesCacheFromDatabase();
        }

        public static void RefreshCategoriesData()
        {
            LoadCategoriesCacheFromDatabase();
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

        public static void ClearData()
        {
            DeleteDatabase();

            DictionaryCache.Clear();

            LanguagesCache.Clear();

            CategoriesCache.Clear();
        }

        #endregion

        #region PRIVATE METHODS

        #region DATABASE UPDATES

        private static void ExecuteNonQueryCommand(string query)
        {
            using var command = dbConnection.CreateCommand();
            command.CommandText = query;
            command.ExecuteNonQuery();
        }

        private static IDataReader ExecuteReaderCommand(string query)
        {
            if (dbConnection.IsNull()) OpenConnection();
            using var command = dbConnection.CreateCommand();
            command.CommandText = query;
            return command.ExecuteReader();
        }

        #region KEYS

        private static void InsertKeyToDatabase(string key, string categoryName)
        {
            try
            {
                OpenConnection();
                var query = $"SELECT MAX(displayOrder) AS max_display_order FROM {TRANSLATION_KEY_TABLE}";
                var lastDisplayOrder = Convert.ToInt32(ExecuteReaderCommand(query)["max_display_order"]);

                query = $"Select id from {CATEGORY_TABLE} where category = '{categoryName}'";
                var categoryID = Convert.ToInt32(ExecuteReaderCommand(query)["id"]);

                query = $"INSERT INTO {TRANSLATION_KEY_TABLE} (displayOrder, key_name, categoryID) VALUES ({lastDisplayOrder + 1}, '{key}', {categoryID})";
                ExecuteNonQueryCommand(query);

                query = $"Select id from {TRANSLATION_KEY_TABLE} where key_name = '{key}'";
                var keyID = Convert.ToInt32(ExecuteReaderCommand(query)["id"]);

                query = $"select id from {LANGUAGE_TABLE}";
                var readerLanguageIDs = ExecuteReaderCommand(query);

                while (readerLanguageIDs.Read())
                {
                    query = $"INSERT INTO {TRANSLATION_TABLE} (keyID, languageID, translationText) VALUES ({keyID},{readerLanguageIDs["id"]},'')";
                    ExecuteNonQueryCommand(query);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }
        }

        private static void UpdateKeyNameInDatabase(string oldKeyName, string newKeyName)
        {
            try
            {
                OpenConnection();
                var query = $"update {TRANSLATION_KEY_TABLE} set key_name = '{newKeyName}' where key_name = '{oldKeyName}'";
                ExecuteNonQueryCommand(query);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }
        }

        private static void UpdateKeyTranslationInDatabase(string key, string newTranslation, string language)
        {
            try
            {
                OpenConnection();
                var query = $"select id from {TRANSLATION_KEY_TABLE} where key_name = '{key}'";
                var keyID = Convert.ToInt32(ExecuteReaderCommand(query)["id"]);

                query = $"select id from {LANGUAGE_TABLE} where language = '{language}'";
                var languageID = Convert.ToInt32(ExecuteReaderCommand(query)["id"]);

                query = $"update {TRANSLATION_TABLE} set translationText = '{newTranslation}' where keyID = {keyID} and languageID = {languageID}";
                ExecuteNonQueryCommand(query);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }
        }

        private static void UpdateKeyCategoryInDatabase(string key, string newCategory)
        {
            try
            {
                OpenConnection();
                var query = $"select id from {CATEGORY_TABLE} where category = '{newCategory}'";
                var categoryID = Convert.ToInt32(ExecuteReaderCommand(query)["id"]);

                query = $"update {TRANSLATION_KEY_TABLE} set categoryID = '{categoryID}' where key_name = '{key}'";
                ExecuteNonQueryCommand(query);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }
        }

        private static void RemoveKeyFromDatabase(string key)
        {
            try
            {
                OpenConnection();
                var query = $"select id,displayOrder from {TRANSLATION_KEY_TABLE} where key_name = '{key}'";
                var reader = ExecuteReaderCommand(query);
                var keyID = Convert.ToInt32(reader["id"]);
                var keyDisplayOrder = Convert.ToInt32(reader["displayOrder"]);
                
                query = $"delete from {TRANSLATION_TABLE} where keyID = '{keyID}'";
                ExecuteNonQueryCommand(query);

                query = $"delete from {TRANSLATION_KEY_TABLE} where id = '{keyID}'";
                ExecuteNonQueryCommand(query);
                
                query = $"update {TRANSLATION_KEY_TABLE} set displayOrder = displayOrder - 1 where displayOrder > {keyDisplayOrder}";
                ExecuteNonQueryCommand(query);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }
        }

        private static Dictionary<string, KeyData> GetKeysTranslationFromDatabase()
        {
            var result = new Dictionary<string, KeyData>();
            try
            {
                OpenConnection();
                const string query =
                    "select tk.key_name, c.category, l.language, t.translationText from translations_keys tk join categories c on tk.categoryID = c.id join translations t on tk.id = t.keyID join languages l on t.languageID = l.id";

                using var command = dbConnection.CreateCommand();
                command.CommandText = query;
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var key = reader["key_name"].ToString();
                    var category = reader["category"].ToString();

                    if (!result.ContainsKey(key))
                    {
                        var language = reader["language"].ToString();
                        var translationText = reader["translationText"].ToString();
                        var keyData = new KeyData()
                        {
                            Category = category,
                            TranslationData = new Dictionary<string, string> { { language, translationText } }
                        };
                        result.Add(key, keyData);
                    }
                    else
                    {
                        var language = reader["language"].ToString();
                        var translationText = reader["translationText"].ToString();
                        var keyData = result[key];
                        keyData.TranslationData.Add(language, translationText);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }

            return result;
        }

        #endregion

        #region LANGUAGE

        private static void InsertLanguageToDatabase(string newLanguage)
        {
            try
            {
                OpenConnection();
                var query = $"SELECT MAX(displayOrder) AS max_display_order FROM {LANGUAGE_TABLE}";
                var lastDisplayOrder = Convert.ToInt32(ExecuteReaderCommand(query)["max_display_order"]);

                query = $"INSERT INTO {LANGUAGE_TABLE} (displayOrder, language, isDefault) VALUES ({lastDisplayOrder + 1}, '{newLanguage}', 0)";
                ExecuteNonQueryCommand(query);
                
                foreach (var key in Keys)
                {
                    query = $"select id from {TRANSLATION_KEY_TABLE} where key_name = '{key}'";
                    var keyID = Convert.ToInt32(ExecuteReaderCommand(query)["id"]);

                    query = $"select id from {LANGUAGE_TABLE} where language = '{newLanguage}'";
                    var languageID = Convert.ToInt32(ExecuteReaderCommand(query)["id"]);
                    
                    query = $"INSERT INTO {TRANSLATION_TABLE} (keyID, languageID, translationText) VALUES ({keyID},{languageID},'')";
                    ExecuteNonQueryCommand(query);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }
        }

        private static void RemoveLanguageFromDatabase(string languageToRemove)
        {
            try
            {
                OpenConnection();
                var query = $"select id,displayOrder from {LANGUAGE_TABLE} where language = '{languageToRemove}'";
                var reader = ExecuteReaderCommand(query);
                var languageToRemoveID = Convert.ToInt32(reader["id"]);
                var languageDisplayOrder = Convert.ToInt32(reader["displayOrder"]);

                query = $"delete from {TRANSLATION_TABLE} where languageID = '{languageToRemoveID}'";
                ExecuteNonQueryCommand(query);

                query = $"delete from {LANGUAGE_TABLE} where language = '{languageToRemove}'";
                ExecuteNonQueryCommand(query);
                
                query = $"update {LANGUAGE_TABLE} set displayOrder = displayOrder - 1 where displayOrder > {languageDisplayOrder}";
                ExecuteNonQueryCommand(query);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }
        }

        private static void UpdateLanguageNameInDatabase(string oldLanguageName, string newLanguageName)
        {
            try
            {
                OpenConnection();
                var query = $"update {LANGUAGE_TABLE} set language = '{newLanguageName}' where language = '{oldLanguageName}'";
                ExecuteNonQueryCommand(query);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }
        }

        private static void UpdateDefaultLanguageInDatabase(string newDefaultLanguage)
        {
            try
            {
                OpenConnection();
                var query = $"update {LANGUAGE_TABLE} set isDefault = 0 where isDefault = 1";
                ExecuteNonQueryCommand(query);

                query = $"update {LANGUAGE_TABLE} set isDefault = 1 where language = '{newDefaultLanguage}'";
                ExecuteNonQueryCommand(query);

                query = $"select displayOrder from {LANGUAGE_TABLE} where language = '{newDefaultLanguage}'";
                var reader = ExecuteReaderCommand(query);

                query = $"update {LANGUAGE_TABLE} set displayOrder = displayOrder + 1 where displayOrder >= {1} and displayOrder < {Convert.ToInt32(reader["displayOrder"])}";
                ExecuteNonQueryCommand(query);

                query = $"update {LANGUAGE_TABLE} set displayOrder = 1 where language = '{newDefaultLanguage}'";
                ExecuteNonQueryCommand(query);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }
        }

        private static void UpdateLanguageDisplayOrderInDatabase(string languageName, int oldLanguageDisplayOrder, int newLanguageDisplayOrder)
        {
            try
            {
                OpenConnection();
                string query;
                if (oldLanguageDisplayOrder < newLanguageDisplayOrder)
                {
                    // Scroll down elements between oldIndex and newIndex
                    query = $"update {LANGUAGE_TABLE} set displayOrder = displayOrder - 1 where displayOrder > {oldLanguageDisplayOrder} and displayOrder <= {newLanguageDisplayOrder}";
                    ExecuteNonQueryCommand(query);
                }
                else if (oldLanguageDisplayOrder > newLanguageDisplayOrder)
                {
                    // Scroll up elements between oldIndex and newIndex
                    query = $"update {LANGUAGE_TABLE} set displayOrder = displayOrder + 1 where displayOrder >= {newLanguageDisplayOrder} and displayOrder < {oldLanguageDisplayOrder}";
                    ExecuteNonQueryCommand(query);
                }

                query = $"update {LANGUAGE_TABLE} set displayOrder = '{newLanguageDisplayOrder}' where language = '{languageName}'";
                ExecuteNonQueryCommand(query);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }
        }

        private static string GetDefaultLanguageFromDatabase()
        {
            var defaultCategory = "";
            try
            {
                OpenConnection();


                var query = $"SELECT language FROM {LANGUAGE_TABLE} where isDefault = 1";
                var reader = ExecuteReaderCommand(query);
                defaultCategory = reader["language"].ToString();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }

            return defaultCategory;
        }

        private static List<LanguageTuple> GetLanguagesOrderedFromDatabase()
        {
            var result = new List<LanguageTuple>();
            try
            {
                OpenConnection();
                var query = $"SELECT displayOrder, language FROM {LANGUAGE_TABLE} order by displayOrder ASC";
                var reader = ExecuteReaderCommand(query);

                while (reader.Read())
                {
                    result.Add(new LanguageTuple
                    {
                        Language = reader["language"].ToString(),
                        DisplayOrder = Convert.ToInt32(reader["displayOrder"])
                    });
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }

            return result;
        }

        #endregion

        #region CATEGORY

        private static void InsertCategoryToDatabase(string newCategory)
        {
            try
            {
                OpenConnection();
                var query = $"SELECT MAX(displayOrder) AS max_display_order FROM {CATEGORY_TABLE}";
                var reader = ExecuteReaderCommand(query);
                var lastDisplayOrder = Convert.ToInt32(reader["max_display_order"]);

                query = $"INSERT INTO {CATEGORY_TABLE} (displayOrder, category, isDefault) VALUES ({lastDisplayOrder + 1}, '{newCategory}', 0)";
                ExecuteNonQueryCommand(query);
                
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }
        }

        private static void RemoveCategoryFromDatabase(string categoryToRemove)
        {
            try
            {
                OpenConnection();
                var defaultCategory = GetDefaultCategoryFromDatabase();

                var query = $"select id from {CATEGORY_TABLE} where category = '{defaultCategory}'";
                var defaultCategoryID = Convert.ToInt32(ExecuteReaderCommand(query)["id"]);

                query = $"select id,displayOrder from {CATEGORY_TABLE} where category = '{categoryToRemove}'";
                var reader = ExecuteReaderCommand(query);
                var categoryToRemoveID = Convert.ToInt32(reader["id"]);
                var categoryDisplayOrder = Convert.ToInt32(reader["displayOrder"]);

                query = $"update {TRANSLATION_KEY_TABLE} set categoryID = '{defaultCategoryID}' where categoryID = '{categoryToRemoveID}'";
                ExecuteNonQueryCommand(query);

                query = $"delete from {CATEGORY_TABLE} where category = '{categoryToRemove}'";
                ExecuteNonQueryCommand(query);
                
                query = $"update {CATEGORY_TABLE} set displayOrder = displayOrder - 1 where displayOrder > {categoryDisplayOrder}";
                ExecuteNonQueryCommand(query);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }
        }

        private static void UpdateCategoryNameInDatabase(string oldCategoryName, string newCategoryName)
        {
            try
            {
                OpenConnection();
                var query = $"update {CATEGORY_TABLE} set category = '{newCategoryName}' where category = '{oldCategoryName}'";
                ExecuteNonQueryCommand(query);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }
        }

        private static void UpdateDefaultCategoryInDatabase(string newDefaultCategory)
        {
            try
            {
                OpenConnection();
                var query = $"update {CATEGORY_TABLE} set isDefault = 0 where isDefault = 1";
                ExecuteNonQueryCommand(query);

                query = $"update {CATEGORY_TABLE} set isDefault = 1 where category = '{newDefaultCategory}'";
                ExecuteNonQueryCommand(query);

                query = $"select displayOrder from {CATEGORY_TABLE} where category = '{newDefaultCategory}'";
                var reader = ExecuteReaderCommand(query);

                query = $"update {CATEGORY_TABLE} set displayOrder = displayOrder + 1 where displayOrder >= {1} and displayOrder < {Convert.ToInt32(reader["displayOrder"])}";
                ExecuteNonQueryCommand(query);

                query = $"update {CATEGORY_TABLE} set displayOrder = 1 where category = '{newDefaultCategory}'";
                ExecuteNonQueryCommand(query);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }
        }

        private static void UpdateCategoryDisplayOrderInDatabase(string categoryName, int oldCategoryDisplayOrder, int newCategoryDisplayOrder)
        {
            try
            {
                OpenConnection();
                string query;
                if (oldCategoryDisplayOrder < newCategoryDisplayOrder)
                {
                    // Scroll down elements between oldIndex and newIndex

                    query = $"update {CATEGORY_TABLE} set displayOrder = displayOrder - 1 where displayOrder > {oldCategoryDisplayOrder} and displayOrder <= {newCategoryDisplayOrder}";
                    ExecuteNonQueryCommand(query);
                }
                else if (oldCategoryDisplayOrder > newCategoryDisplayOrder)
                {
                    // Scroll up elements between oldIndex and newIndex

                    query = $"update {CATEGORY_TABLE} set displayOrder = displayOrder + 1 where displayOrder >= {newCategoryDisplayOrder} and displayOrder < {oldCategoryDisplayOrder}";
                    ExecuteNonQueryCommand(query);
                }

                query = $"update {CATEGORY_TABLE} set displayOrder = '{newCategoryDisplayOrder}' where category = '{categoryName}'";
                ExecuteNonQueryCommand(query);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }
        }

        private static string GetDefaultCategoryFromDatabase()
        {
            var defaultCategory = "";
            try
            {
                OpenConnection();
                var query = $"SELECT category FROM {CATEGORY_TABLE} where isDefault = 1";
                var reader = ExecuteReaderCommand(query);
                defaultCategory = reader["category"].ToString();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }

            return defaultCategory;
        }

        private static List<CategoryTuple> GetCategoriesOrderedFromDatabase()
        {
            var result = new List<CategoryTuple>();
            try
            {
                OpenConnection();

                var query = $"SELECT displayOrder, category FROM {CATEGORY_TABLE} order by displayOrder ASC";

                var reader = ExecuteReaderCommand(query);

                while (reader.Read())
                {
                    result.Add(new CategoryTuple
                    {
                        Category = reader["category"].ToString(),
                        DisplayOrder = Convert.ToInt32(reader["displayOrder"])
                    });
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseConnection();
            }

            return result;
        }

        #endregion

        #endregion

        private LocalizationManager()
        {
#pragma warning disable CS4014
            //TODO: te if this is necessary => Init();
#pragma warning restore CS4014
        }

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

        public void Init(Action onComplete = null)
        {
            if (_isInitialized) return; // Just the first call
            _isInitialized = true;

            _serializerBinary = new BinarySerializer();
#pragma warning disable CS4014
            LoadConfigurationDataFromBINARY();
#pragma warning restore CS4014

            Log(TOOL_INITIALIZED_LOG);
            onComplete?.Invoke();
            OnLocalizationToolInitialized?.Invoke();
        }

        public static void LoadCache()
        {
            LoadDictionaryCacheFromDatabase();
            LoadLanguagesCacheFromDatabase();
            LoadCategoriesCacheFromDatabase();
            IsDataLoaded = true;
        }

        private static void LoadDictionaryCacheFromDatabase()
        {
            DictionaryCache = GetKeysTranslationFromDatabase();
            //Debug.Log("Dictionary loaded");
        }

        private static void LoadLanguagesCacheFromDatabase()
        {
            LanguagesCache = GetLanguagesOrderedFromDatabase();

            DefaultLanguage = GetDefaultLanguageFromDatabase();
            CurrentLanguageInDictionarySection = DefaultLanguage;
            if (LocalizationToolAPI.Instance.IsNotNull()) LocalizationToolAPI.ActiveLanguage = DefaultLanguage;

            //Debug.Log("Languages loaded");
        }

        private static void LoadCategoriesCacheFromDatabase()
        {
            CategoriesCache = GetCategoriesOrderedFromDatabase();
            DefaultCategory = GetDefaultCategoryFromDatabase();
            //Debug.Log("Categories loaded");
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

            //Debug.Log("Configuration loaded");
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

        #endregion
    }
}