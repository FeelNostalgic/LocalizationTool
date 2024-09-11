using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CustomDebugPlugin;
using LocalizationTool.Scripts.API;
using LocalizationTool.Scripts.Commons;
using LocalizationTool.Scripts.General;
using Mono.Data.Sqlite;
using UnityEngine;
using static LocalizationTool.Scripts.Commons.EditorPaths;
using static LocalizationTool.Database.DatabaseStrings;
using Colors = CustomDebugPlugin.Colors;

namespace LocalizationTool.Scripts.Data
{
    public class CacheData
    {
        #region PUBLIC VARIABLES

        public static CacheData Instance => _instance ??= new CacheData();

        public static bool IsDataLoaded { get; private set; }

        public static List<LanguageTuple> LanguageCache { get; set; }
        public static List<string> Languages => LanguageCache.Select(x => x.Language).ToList();
        public static string DefaultLanguage { get; set; }

        public static List<CategoryTuple> CategoryCache { get; set; }
        public static List<string> Categories => CategoryCache.Select(x => x.Category).ToList();
        public static string DefaultCategory { get; set; }

        public static Dictionary<string, KeyData> DictionaryCache { get; set; }
        public static List<string> Keys => DictionaryCache.Select(x => x.Key).ToList();

        #region Actions

        public Action OnLocalizationToolDataInitialized { get; set; }

        #endregion

        #endregion

        #region PRIVATE VARIABLES

        private static CacheData _instance;
        private static string _connectionString;
        private static IDbConnection _dbConnection;
        private bool _isInitialized;

        #endregion

        #region LOAD CACHE

        public void InitForEditor(Action onComplete = null)
        {
            if (_isInitialized) return; // Just the first call
            _isInitialized = true;

            LoadCacheData();
            
            onComplete?.Invoke();
        }

        public static void LoadCacheData()
        {
            LoadDictionaryCacheFromDatabase();
            LoadLanguagesCacheFromDatabase();
            LoadCategoriesCacheFromDatabase();
            IsDataLoaded = true;
            CustomDebug.Log("Database", Colors.Red, "Loaded");
            Instance.OnLocalizationToolDataInitialized?.Invoke();
        }
        
        public static void LoadDictionaryCacheFromDatabase()
        {
            DictionaryCache = GetKeysTranslationFromDatabase();
        }

        public static void LoadLanguagesCacheFromDatabase()
        {
            LanguageCache = GetLanguagesOrderedFromDatabase();

            DefaultLanguage = GetDefaultLanguageFromDatabase();
#if UNITY_EDITOR
            LocalizationManager.CurrentLanguageInDictionarySection = DefaultLanguage;
#endif
            if (LocalizationToolAPI.Instance.IsNotNull()) LocalizationToolAPI.ActiveLanguage = DefaultLanguage;
        }

        public static void LoadCategoriesCacheFromDatabase()
        {
            CategoryCache = GetCategoriesOrderedFromDatabase();
            DefaultCategory = GetDefaultCategoryFromDatabase();
        }

        #endregion
        
        #region DATABASE UPDATES

        public static void OpenConnection()
        {
            _connectionString = "URI=file:" + DATABASE_PATH;
            _dbConnection = new SqliteConnection(_connectionString);
            _dbConnection.Open();
        }

        public static void CloseConnection()
        {
            if (_dbConnection == null) return;

            _dbConnection.Close();
            _dbConnection = null;
        }

        public static void ExecuteNonQueryCommand(string query)
        {
            using var command = _dbConnection.CreateCommand();
            command.CommandText = query;
            command.ExecuteNonQuery();
        }

        public static IDataReader ExecuteReaderCommand(string query)
        {
            if (_dbConnection.IsNull()) OpenConnection();
            using var command = _dbConnection.CreateCommand();
            command.CommandText = query;
            return command.ExecuteReader();
        }

        private static void DeleteDatabase()
        {
            try
            {
                OpenConnection();
                //Delete translation
                var query = $"DELETE FROM {TRANSLATION_TABLE}; DELETE FROM sqlite_sequence WHERE name='{TRANSLATION_TABLE}';";
                ExecuteNonQueryCommand(query);

                //Delete translations_keys
                query =  $"DELETE FROM {TRANSLATION_KEY_TABLE}; DELETE FROM sqlite_sequence WHERE name='{TRANSLATION_KEY_TABLE}';";
                ExecuteNonQueryCommand(query);

                // Delete category
                query =  $"DELETE FROM {CATEGORY_TABLE}; DELETE FROM sqlite_sequence WHERE name='{CATEGORY_TABLE}';";
                ExecuteNonQueryCommand(query);

                // Delete language
                query =  $"DELETE FROM {LANGUAGE_TABLE}; DELETE FROM sqlite_sequence WHERE name='{LANGUAGE_TABLE}';";
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

        #region UPDATE CACHES

        public static void ClearData()
        {
            DeleteDatabase();

            DictionaryCache.Clear();

            LanguageCache.Clear();

            CategoryCache.Clear();
        }

        public static void UpdateDictionaryCache()
        {
            DictionaryCache = GetKeysTranslationFromDatabase();
        }

        public static void UpdateLanguageCache()
        {
            LanguageCache = GetLanguagesOrderedFromDatabase();
        }

        public static void UpdateCategoryCache()
        {
            CategoryCache = GetCategoriesOrderedFromDatabase();
        }

        #endregion

        #region KEYS

        public static void InsertKeyToDatabase(string key, string categoryName)
        {
            try
            {
                OpenConnection();
                var query = $"SELECT MAX(displayOrder) AS max_display_order FROM {TRANSLATION_KEY_TABLE}";
                var result = ExecuteReaderCommand(query)["max_display_order"];
                var lastDisplayOrder = result.ToString().IsEmpty() ? 1 : Convert.ToInt32(result);

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

        public static void UpdateKeyNameInDatabase(string oldKeyName, string newKeyName)
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

        public static void UpdateKeyTranslationInDatabase(string key, string newTranslation, string language)
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

        public static void UpdateKeyCategoryInDatabase(string key, string newCategory)
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

        public static void RemoveKeyFromDatabase(string key)
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

        public static Dictionary<string, KeyData> GetKeysTranslationFromDatabase()
        {
            var result = new Dictionary<string, KeyData>();
            try
            {
                OpenConnection();
                const string query =
                    "select tk.key_name, c.category, l.language, t.translationText from translations_keys tk join categories c on tk.categoryID = c.id join translations t on tk.id = t.keyID join languages l on t.languageID = l.id";

                using var command = _dbConnection.CreateCommand();
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
                        var keyData = new KeyData
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

        public static void InsertLanguageToDatabase(string newLanguage)
        {
            try
            {
                OpenConnection();
                var query = $"SELECT MAX(displayOrder) AS max_display_order FROM {LANGUAGE_TABLE}";
                var result = ExecuteReaderCommand(query)["max_display_order"];
                var lastDisplayOrder = result.ToString().IsEmpty() ? 1 : Convert.ToInt32(result);

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

        public static void RemoveLanguageFromDatabase(string languageToRemove)
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

        public static void EmptyLanguageFromDatabase(string languageToEmpty)
        {
            try
            {
                OpenConnection();
                var query = $"select id from {LANGUAGE_TABLE} where language = '{languageToEmpty}'";
                var reader = ExecuteReaderCommand(query);
                var languageToEmptyID = Convert.ToInt32(reader["id"]);

                query = $"update {TRANSLATION_TABLE} set translationText = '' where languageID = '{languageToEmptyID}'";
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
        
        public static void UpdateLanguageNameInDatabase(string oldLanguageName, string newLanguageName)
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

        public static void UpdateDefaultLanguageInDatabase(string newDefaultLanguage)
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

        public static void UpdateLanguageDisplayOrderInDatabase(string languageName, int oldLanguageDisplayOrder, int newLanguageDisplayOrder)
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

        public static string GetDefaultLanguageFromDatabase()
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

        public static List<LanguageTuple> GetLanguagesOrderedFromDatabase()
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

        public static void InsertCategoryToDatabase(string newCategory)
        {
            try
            {
                OpenConnection();
                var query = $"SELECT MAX(displayOrder) AS max_display_order FROM {CATEGORY_TABLE}";
                var result = ExecuteReaderCommand(query)["max_display_order"];
                var lastDisplayOrder = result.ToString().IsEmpty() ? 1 : Convert.ToInt32(result);

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

        public static void RemoveCategoryFromDatabase(string categoryToRemove)
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

        public static void UpdateCategoryNameInDatabase(string oldCategoryName, string newCategoryName)
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

        public static void UpdateDefaultCategoryInDatabase(string newDefaultCategory)
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

        public static void UpdateCategoryDisplayOrderInDatabase(string categoryName, int oldCategoryDisplayOrder, int newCategoryDisplayOrder)
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

        public static string GetDefaultCategoryFromDatabase()
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

        public static List<CategoryTuple> GetCategoriesOrderedFromDatabase()
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
    }
}