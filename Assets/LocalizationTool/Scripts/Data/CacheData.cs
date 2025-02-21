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
            IsDataLoaded = true;
            CustomDebug.Log("Database", Colors.Red, "Loaded");
            Instance.OnLocalizationToolDataInitialized?.Invoke();
        }

        public static void LoadDictionaryCacheFromDatabase()
        {
            DictionaryCache = GetKeysTranslationFromDatabase();
        }
        
        #endregion

        #region DATABASE UPDATES

        public static void OpenDatabaseConnection()
        {
            _connectionString = "URI=file:" + DATABASE_PATH;
            _dbConnection = new SqliteConnection(_connectionString);
            _dbConnection.Open();
        }

        public static void CloseDatabaseConnection()
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
            if (_dbConnection.IsNull()) OpenDatabaseConnection();
            using var command = _dbConnection.CreateCommand();
            command.CommandText = query;
            return command.ExecuteReader();
        }

        private static void DeleteDatabase()
        {
            try
            {
                OpenDatabaseConnection();
                //Delete translation
                var query = $"DELETE FROM {TRANSLATION_TABLE}; DELETE FROM sqlite_sequence WHERE name='{TRANSLATION_TABLE}';";
                ExecuteNonQueryCommand(query);

                //Delete translations_keys
                query = $"DELETE FROM {TRANSLATION_KEY_TABLE}; DELETE FROM sqlite_sequence WHERE name='{TRANSLATION_KEY_TABLE}';";
                ExecuteNonQueryCommand(query);

                // Delete category
                query = $"DELETE FROM {CATEGORY_TABLE}; DELETE FROM sqlite_sequence WHERE name='{CATEGORY_TABLE}';";
                ExecuteNonQueryCommand(query);

                // Delete language
                query = $"DELETE FROM {LANGUAGE_TABLE}; DELETE FROM sqlite_sequence WHERE name='{LANGUAGE_TABLE}';";
                ExecuteNonQueryCommand(query);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseDatabaseConnection();
            }
        }

        public static void LogChange(string tableName, string action, string param, string oldData, string newData)
        {
            //TODO 
            //var query = $"INSERT INTO {CHANGE_LOG_TABLE} (table_name, action, params, old_data, new_data) VALUES ('{tableName}', '{action}', '{param}', '{oldData}', '{newData}')";
            //ExecuteNonQueryCommand(query);
        }

        #region UPDATE CACHES

        public static void ClearData()
        {
            DeleteDatabase();

            DictionaryCache.Clear();
        }

        public static void UpdateDictionaryCache()
        {
            DictionaryCache = GetKeysTranslationFromDatabase();
        }
        
        #endregion

        #region KEYS

        public static void InsertKeyToDatabase(string key, string categoryName)
        {
            try
            {
                OpenDatabaseConnection();
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

                LogChange(TRANSLATION_TABLE, "INSERT", "", "", $"{keyID}");
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseDatabaseConnection();
            }
        }

        public static void UpdateKeyNameInDatabase(string oldKeyName, string newKeyName)
        {
            try
            {
                OpenDatabaseConnection();
                var query = $"update {TRANSLATION_KEY_TABLE} set key_name = '{newKeyName}' where key_name = '{oldKeyName}'";
                ExecuteNonQueryCommand(query);

                LogChange(TRANSLATION_KEY_TABLE, "UPDATE", "key_name", $"{oldKeyName}", $"{newKeyName}");
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseDatabaseConnection();
            }
        }

        public static void UpdateKeyTranslationInDatabase(string key, string newTranslation, string language)
        {
            try
            {
                OpenDatabaseConnection();
                var query = $"select id from {TRANSLATION_KEY_TABLE} where key_name = '{key}'";
                var keyID = Convert.ToInt32(ExecuteReaderCommand(query)["id"]);

                query = $"select id from {LANGUAGE_TABLE} where language = '{language}'";
                var languageID = Convert.ToInt32(ExecuteReaderCommand(query)["id"]);

                query = $"select translationText from {TRANSLATION_TABLE} where keyID = {keyID} and languageID = {languageID}";
                var oldText = ExecuteReaderCommand(query)["translationText"].ToString();

                query = $"update {TRANSLATION_TABLE} set translationText = '{newTranslation}' where keyID = {keyID} and languageID = {languageID}";
                ExecuteNonQueryCommand(query);

                LogChange(TRANSLATION_KEY_TABLE, "UPDATE", "translationText", $"{oldText}", $"{newTranslation}");
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseDatabaseConnection();
            }
        }

        public static void UpdateKeyCategoryInDatabase(string key, string newCategory)
        {
            try
            {
                OpenDatabaseConnection();
                var query = $"select id from {CATEGORY_TABLE} where category = '{newCategory}'";
                var categoryID = Convert.ToInt32(ExecuteReaderCommand(query)["id"]);

                query = "select categoryID from {TRANSLATION_KEY_TABLE} where key_name = {key}";
                var oldCategory = ExecuteReaderCommand(query)["categoryID"].ToString();

                query = $"update {TRANSLATION_KEY_TABLE} set categoryID = '{categoryID}' where key_name = '{key}'";
                ExecuteNonQueryCommand(query);

                LogChange(TRANSLATION_KEY_TABLE, "UPDATE", "categoryID", $"{oldCategory}", $"{categoryID}");
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseDatabaseConnection();
            }
        }

        public static void RemoveKeyFromDatabase(string key)
        {
            try
            {
                OpenDatabaseConnection();
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

                LogChange(TRANSLATION_KEY_TABLE, "DELETE", "displayOrder", $"{keyDisplayOrder}", "");
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                CloseDatabaseConnection();
            }
        }

        public static Dictionary<string, KeyData> GetKeysTranslationFromDatabase()
        {
            var result = new Dictionary<string, KeyData>();
            try
            {
                OpenDatabaseConnection();
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
                CloseDatabaseConnection();
            }

            return result;
        }

        #endregion

        #region LANGUAGE

        public static void EmptyLanguageFromDatabase(string languageToEmpty)
        {
            try
            {
                OpenDatabaseConnection();
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
                CloseDatabaseConnection();
            }
        }
        
        #endregion

        #region CATEGORY
        
        public static List<CategoryTuple> GetCategoriesOrderedFromDatabase()
        {
            var result = new List<CategoryTuple>();
            try
            {
                OpenDatabaseConnection();

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
                CloseDatabaseConnection();
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